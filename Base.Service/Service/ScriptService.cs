using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class ScriptService : IScriptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHangfireService _hangfireService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IExpoPushNotification _expoPushNotification;
    private readonly IWebSocketConnectionManager1 _webSocketConnectionManager;

    public ScriptService(IUnitOfWork unitOfWork, 
        IHangfireService hangfireService, 
        IServiceScopeFactory serviceScopeFactory, 
        IExpoPushNotification expoPushNotification, 
        IWebSocketConnectionManager1 webSocketConnectionManager)
    {
        _unitOfWork = unitOfWork;
        _hangfireService = hangfireService;
        _serviceScopeFactory = serviceScopeFactory;
        _expoPushNotification = expoPushNotification;
        _webSocketConnectionManager = webSocketConnectionManager;
    }

    public void ResetServerTime()
    {
        ServerDateTime.ResetServerTime();

        SetupServerTime();
    }

    public void SetServerTime(DateTime time)
    {
        // Setup server datetime
        ServerDateTime.SetServerTime(time);

        SetupServerTime();
    }

    public async Task AutoRegisterFingerprint()
    {
        var fingerprints = _unitOfWork.StoredFingerprintDemoRepository
            .Get(s => s.FingerprintTemplate != string.Empty)
            .AsNoTracking()
            .ToArray();

        var unauthenticatedStudents = _unitOfWork.StudentRepository
            .Get(s => !s.IsDeleted && s.FingerprintTemplates.Count() == 0, 
            new System.Linq.Expressions.Expression<Func<Repository.Entity.Student, object?>>[]
            {
                s => s.FingerprintTemplates
            })
            .AsNoTracking()
            .ToList();

        var addedFingerprints = new List<FingerprintTemplate>();
        int fingerprintIndex = 0;
        var fingersTotal = fingerprints.Count() - 1;

        foreach (var student in unauthenticatedStudents)
        {
            addedFingerprints.Add(new FingerprintTemplate
            {
                FingerprintTemplateData = fingerprints[fingerprintIndex++].FingerprintTemplate,
                Status = 1,
                StudentID = student.StudentID,
            });
            if (fingerprintIndex > fingersTotal)
            {
                fingerprintIndex = 0;
            }

            addedFingerprints.Add(new FingerprintTemplate
            {
                FingerprintTemplateData = fingerprints[fingerprintIndex++].FingerprintTemplate,
                Status = 1,
                StudentID = student.StudentID,
            });
            if(fingerprintIndex > fingersTotal)
            {
                fingerprintIndex = 0;
            }
        }

        await _unitOfWork.FingerprintRepository.AddRangeAsync(addedFingerprints);
        await _unitOfWork.SaveChangesAsync();
    }



    private void SetupServerTime()
    {
        if (ServerDateTime.ServerTimeIsAhead())
        {
            // In the future, execute task in the future
            // Identify how much it did go through
            var oldDateTime = ServerDateTime.GetOldVnDateTime();
            var newDateTime = ServerDateTime.GetVnDateTime();
            var difference = newDateTime - oldDateTime;

            // Identify how much hours and minutes has passed
            var passedDays = difference.Days;
            var passedHours = difference.Hours + passedDays * 24;
            var passedMinutes = difference.Minutes;

            // Identify which dates are passed
            var oldDateOnly = DateOnly.FromDateTime(oldDateTime);
            var newDateOnly = DateOnly.FromDateTime(newDateTime);
            var pastDates = new List<PastDate>();
            while (true)
            {
                pastDates.Add(new PastDate
                {
                    Date = oldDateOnly
                });
                oldDateOnly = oldDateOnly.AddDays(1);
                if (oldDateOnly > newDateOnly)
                {
                    break;
                }
            }

            // Identify which slots are past on each date
            oldDateOnly = DateOnly.FromDateTime(oldDateTime);
            newDateOnly = DateOnly.FromDateTime(newDateTime);
            var oldTimeOnly = TimeOnly.FromDateTime(oldDateTime);
            var newTimeOnly = TimeOnly.FromDateTime(newDateTime);
            foreach (var pastDate in pastDates)
            {
                if (oldDateOnly == newDateOnly)
                {
                    var pastSlots = new List<PastSlot>();
                    var slots = _unitOfWork.SlotRepository
                        .Get(s => !s.IsDeleted &&
                            s.Endtime >= oldTimeOnly &&
                            s.StartTime <= newTimeOnly)
                        .AsNoTracking()
                        .ToList();

                    foreach (var slot in slots)
                    {
                        var pastSlot = new PastSlot
                        {
                            SlotId = slot.SlotID,
                            PastStart = slot.StartTime <= newTimeOnly && slot.StartTime > oldTimeOnly,
                            PastFirst15Min = slot.StartTime.AddMinutes(15) <= newTimeOnly && slot.StartTime.AddMinutes(15) > oldTimeOnly,
                            PastEnd = slot.Endtime <= newTimeOnly,
                            PastLast15Min = oldTimeOnly < slot.Endtime.AddMinutes(-15) && slot.Endtime.AddMinutes(-15) <= newTimeOnly
                        };
                        pastSlots.Add(pastSlot);
                    }
                    pastDate.PastSlots = pastSlots;
                }
                else if (pastDate.Date == oldDateOnly)
                {
                    var pastSlots = new List<PastSlot>();

                    var slots = _unitOfWork.SlotRepository
                        .Get(s => !s.IsDeleted && s.Endtime > oldTimeOnly)
                        .AsNoTracking()
                        .ToList();
                    foreach (var slot in slots)
                    {
                        var pastSlot = new PastSlot
                        {
                            SlotId = slot.SlotID,
                            PastStart = oldTimeOnly <= slot.StartTime,
                            PastFirst15Min = oldTimeOnly <= slot.StartTime.AddMinutes(15),
                            PastEnd = true,
                            PastLast15Min = oldTimeOnly <= slot.Endtime.AddMinutes(-15)
                        };
                        pastSlots.Add(pastSlot);
                    }

                    pastDate.PastSlots = pastSlots;
                }
                else if (pastDate.Date == newDateOnly)
                {
                    var pastSlots = new List<PastSlot>();

                    var slots = _unitOfWork.SlotRepository
                        .Get(s => !s.IsDeleted && s.StartTime <= newTimeOnly)
                        .AsNoTracking()
                        .ToList();

                    foreach (var slot in slots)
                    {
                        var pastSlot = new PastSlot
                        {
                            SlotId = slot.SlotID,
                            PastStart = true,
                            PastFirst15Min = newTimeOnly >= slot.StartTime.AddMinutes(15),
                            PastEnd = newTimeOnly >= slot.Endtime,
                            PastLast15Min = newTimeOnly >= slot.Endtime.AddMinutes(-15)
                        };
                        pastSlots.Add(pastSlot);
                    }

                    pastDate.PastSlots = pastSlots;
                }
                else
                {
                    var slots = _unitOfWork.SlotRepository
                        .Get(s => !s.IsDeleted)
                        .AsNoTracking()
                        .Select(s => new PastSlot
                        {
                            SlotId = s.SlotID,
                            PastStart = true,
                            PastFirst15Min = true,
                            PastEnd = true,
                            PastLast15Min = true
                        })
                        .ToList();

                    pastDate.PastSlots = slots;
                }
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.4 * 2))
            };
            Parallel.ForEach(pastDates, parallelOptions, (pastDate, state) =>
            {
                if (pastDate.Date != oldDateOnly && pastDate.Date != newDateOnly)
                {
                    _ = SetSchedulesEnd(pastDate.Date);
                }
                else
                {
                    foreach (var pastSlot in pastDate.PastSlots)
                    {
                        if (pastSlot.PastStart && !pastSlot.PastEnd)
                        {
                            _ = _hangfireService.SetSlotStart(pastSlot.SlotId, pastDate.Date);
                        }
                        if (pastSlot.PastEnd)
                        {
                            _ = _hangfireService.SetSlotEnd(pastSlot.SlotId, pastDate.Date);
                        }
                        if (pastSlot.PastFirst15Min)
                        {
                            // Check and remind lecturer to take attendance
                            _ = RemindLecturerToCheckAttendanceAfterFirst15MinInMobile(pastDate.Date, pastSlot.SlotId);
                        }
                        if (pastSlot.PastLast15Min)
                        {
                            // Email students for double-check
                        }
                    }
                }
            });

            // Need to set hangfire for each job of each slot, each job of remind task and double-check task
            // to follow the update date time here
        }
        else
        {
            // In the past, just need to change schedule status
            var oldDateTime = ServerDateTime.GetOldVnDateTime();
            var newDateTime = ServerDateTime.GetVnDateTime();

            var oldDateOnly = DateOnly.FromDateTime(oldDateTime);
            var newDateOnly = DateOnly.FromDateTime(newDateTime);

            var oldTimeOnly = TimeOnly.FromDateTime(oldDateTime);
            var newTimeOnly = TimeOnly.FromDateTime(newDateTime);

            var effectedDates = new List<EffectedDate>();

            if (oldDateOnly == newDateOnly)
            {
                EffectedDate effectedDate = new EffectedDate
                {
                    Date = newDateOnly
                };
                var effectedSlots = new List<EffectedSlot>();

                var slots = _unitOfWork.SlotRepository
                    .Get(s => !s.IsDeleted &&
                        s.StartTime <= oldTimeOnly &&
                        s.Endtime > newTimeOnly)
                    .AsNoTracking()
                    .ToList();
                foreach (var slot in slots)
                {
                    effectedSlots.Add(new EffectedSlot
                    {
                        SlotId = slot.SlotID,
                        EffectedEnd = slot.Endtime <= oldTimeOnly,
                        EffectedStart = slot.StartTime > newTimeOnly
                    });
                }
                effectedDate.EffectedSlots = effectedSlots;
                effectedDates.Add(effectedDate);
            }
            else
            {
                var usedDateOnly = oldDateOnly;
                while (true)
                {
                    var effectedDate = new EffectedDate
                    {
                        Date = usedDateOnly
                    };

                    if (usedDateOnly == oldDateOnly)
                    {
                        var effectedSlots = _unitOfWork.SlotRepository
                            .Get(s => !s.IsDeleted && s.StartTime <= oldTimeOnly)
                            .AsNoTracking()
                            .Select(s => new EffectedSlot
                            {
                                SlotId = s.SlotID,
                                EffectedStart = true,
                                EffectedEnd = s.Endtime <= oldTimeOnly
                            })
                            .ToList();
                        effectedDate.EffectedSlots = effectedSlots;
                    }
                    else if (usedDateOnly == newDateOnly)
                    {
                        var effectedSlots = _unitOfWork.SlotRepository
                            .Get(s => !s.IsDeleted && s.Endtime > newTimeOnly)
                            .AsNoTracking()
                            .Select(s => new EffectedSlot
                            {
                                SlotId = s.SlotID,
                                EffectedStart = s.StartTime > newTimeOnly,
                                EffectedEnd = true
                            })
                            .ToList();
                        effectedDate.EffectedSlots = effectedSlots;
                    }
                    else
                    {
                        var effectedSlots = _unitOfWork.SlotRepository
                            .Get(s => !s.IsDeleted)
                            .AsNoTracking()
                            .Select(s => new EffectedSlot
                            {
                                SlotId = s.SlotID,
                                EffectedStart = true,
                                EffectedEnd = true
                            })
                            .ToList();
                        effectedDate.EffectedSlots = effectedSlots;
                    }
                    effectedDates.Add(effectedDate);

                    usedDateOnly = usedDateOnly.AddDays(-1);
                    if (usedDateOnly < newDateOnly)
                    {
                        break;
                    }
                }
            }

            // Handle effected slots in each effected date
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.4 * 2))
            };
            Parallel.ForEach(effectedDates, parallelOptions, (effectedDate, state) =>
            {
                if (effectedDate.Date != oldDateOnly && effectedDate.Date != newDateOnly)
                {
                    _ = SetSchedulesFuture(effectedDate.Date);
                }
                else
                {
                    foreach (var efefctedSlot in effectedDate.EffectedSlots)
                    {
                        if (efefctedSlot.EffectedStart)
                        {
                            _ = SetSchedulesOfSlotFuture(effectedDate.Date, efefctedSlot.SlotId);
                        }
                        else
                        {
                            _ = SetSchedulesOfSlotOnGoing(effectedDate.Date, efefctedSlot.SlotId);
                        }
                    }
                }
            });
        }
    }

    private async Task SetSchedulesEnd(DateOnly date)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var schedules = unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Date == date)
            .ToList();

        foreach (var schedule in schedules)
        {
            schedule.ScheduleStatus = 3; // Finished
        }

        await unitOfWork.SaveChangesAsync();
    }

    private async Task SetSchedulesFuture(DateOnly date)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var schedules = unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Date == date)
            .ToList();

        foreach (var schedule in schedules)
        {
            schedule.ScheduleStatus = 1; // Not yet
        }

        await unitOfWork.SaveChangesAsync();
    }

    private async Task SetSchedulesOfSlotFuture(DateOnly date, int slotId)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var schedules = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Date == date && s.SlotID == slotId)
            .ToList();

        foreach (var schedule in schedules)
        {
            schedule.ScheduleStatus = 1; // Not yet
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task SetSchedulesOfSlotOnGoing(DateOnly date, int slotId)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var schedules = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Date == date && s.SlotID == slotId)
            .ToList();

        foreach (var schedule in schedules)
        {
            schedule.ScheduleStatus = 2; // On-going
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task RemindLecturerToCheckAttendanceAfterFirst15MinInMobile(DateOnly date, int slotId)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificationTypeService = serviceScope.ServiceProvider.GetRequiredService<INotificationTypeService>();
        var notificationService = serviceScope.ServiceProvider.GetRequiredService<INotificationService>();

        var schedules = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Date == date && s.SlotID == slotId,
            new System.Linq.Expressions.Expression<Func<Schedule, object?>>[]
            {
                s => s.Class,
                s => s.Slot
            })
            .AsNoTracking()
            .ToList();
        foreach (var schedule in schedules)
        {
            // If lecturer does not check attendance, lets notify to mobile
            if(schedule.Attended == 1)
            {
                var lecturerId = schedule.Class?.LecturerID;
                if(lecturerId is not null)
                {
                    // Get device token of lecturer
                    var getDeviceTokenTask = _unitOfWork.UserRepository
                        .Get(u => !u.Deleted && u.Id == lecturerId).AsNoTracking()
                        .Select(l => l.DeviceToken).FirstOrDefaultAsync();

                    // Get notification type of warning
                    var getNotificationTypeTask = _unitOfWork.NotificationTypeRepository
                        .Get(n => !n.IsDeleted && n.TypeName.ToUpper() == "WARNING").AsNoTracking()
                        .Select(n => n.NotificationTypeID).FirstOrDefaultAsync();

                    await Task.WhenAll(getDeviceTokenTask, getNotificationTypeTask);

                    // Create new notification
                    var notificationTypeId = getNotificationTypeTask.Result;
                    if (notificationTypeId <= 0)
                    {
                        // Create a warning notification type
                        var warningType = (await notificationTypeService.Create(new NotificationTypeVM
                        {
                            TypeName = "Warning",
                            TypeDescription = "Used to warning something that could cause error"
                        })).Result;
                        notificationTypeId = warningType?.NotificationTypeID ?? 0;
                    }
                    var newNotification = new NotificationVM
                    {
                        Title = "Don't forget to check attendance",
                        Description = $"Class {schedule.Class?.ClassCode ?? "***"} on {schedule.Date.ToString("dd-MM-yyyy") ?? "***"} at {schedule.Slot?.StartTime.ToString("HH:mm:ss") ?? "***"} o'clock has past the first 15 minutes",
                        Read = false,
                        UserID = lecturerId.Value,
                        ScheduleId = schedule.ScheduleID,
                        NotificationTypeID = notificationTypeId
                    };
                    var createdNotification = await notificationService.Create(newNotification);

                    // Notify to mobile
                    var deviceToken = getDeviceTokenTask.Result;
                    if(deviceToken is not null)
                    {
                        _ = _expoPushNotification.SendMessageToMobile(deviceToken, 
                            "Don't forget to check attendance", "Remind attendance check", 
                            $"The class {schedule.Class?.ClassCode ?? "***"} is on going",
                            new
                            {
                                Event = "NewNotification",
                                Data = new
                                {
                                    NotificationId = createdNotification.Result!.NotificationID
                                }
                            });
                    }

                    // Notify to web client
                    var messageSend = new
                    {
                        Event = "NewNotification",
                        Data = new
                        {
                            NotificationId = createdNotification.Result!.NotificationID
                        }
                    };
                    var jsonPayload = JsonSerializer.Serialize(messageSend);
                    _ = _webSocketConnectionManager.SendMessageToClient(jsonPayload, lecturerId.Value);
                }
            }
        }
    }
}

internal class PastDate
{
    public DateOnly Date { get; set; }
    public IEnumerable<PastSlot> PastSlots { get; set; } = new List<PastSlot>();
}

internal class PastSlot
{
    public int SlotId { get; set; }
    public bool PastFirst15Min { get; set; } = false;
    public bool PastLast15Min { get; set; } = false;
    public bool PastStart { get; set; } = false;
    public bool PastEnd { get; set; } = false;
}

internal class EffectedDate
{
    public DateOnly Date { get; set; }
    public IEnumerable<EffectedSlot> EffectedSlots { get; set; } = new List<EffectedSlot>();
}

internal class EffectedSlot
{
    public int SlotId { get; set; }
    public bool EffectedStart { get; set; } = false;
    public bool EffectedEnd { get; set; } = false;
}
