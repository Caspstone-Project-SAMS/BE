using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidateGet _validateGet;
        public ScheduleService(IUnitOfWork unitOfWork, IValidateGet validateGet)
        {
            _unitOfWork = unitOfWork;
            _validateGet = validateGet;
        }

        public async Task<ServiceResponseVM<List<ScheduleVM>>> Create(List<ScheduleVM> newEntities)
        {
            List<ScheduleVM> createdSchedule = new List<ScheduleVM>();
            List<string> errors = new List<string>();
            foreach (var newEntity in newEntities)
            {
                try
                {
                    var existedClass = await _unitOfWork.ClassRepository.Get(c => c.ClassCode.Equals(newEntity.ClassCode)).SingleOrDefaultAsync();
                    if (existedClass is null)
                    {
                        errors.Add($"Class with Class Code: {newEntity.ClassCode} not existed");
                        continue;
                    }

                    var existedSlot = await _unitOfWork.SlotRepository.Get(s => s.SlotNumber == newEntity.SlotNumber).SingleOrDefaultAsync();
                    if( existedSlot is null)
                    {
                        errors.Add($"Slot {newEntity.SlotNumber} not existed");
                        continue;
                    }

                    var existedSchedule = await _unitOfWork.ScheduleRepository.Get(s => s.Date == newEntity.Date && s.SlotID == existedSlot.SlotID).SingleOrDefaultAsync();
                    if(existedSchedule is not null)
                    {
                        errors.Add($"Class with Class Code: {newEntity.ClassCode} can not create schedule because on {newEntity.Date}, in slot {newEntity.SlotNumber} is already occupied by another class");
                        continue;
                    }
                    DayOfWeek dayOfWeek = newEntity.Date.DayOfWeek;
                    Schedule newSchedule = new Schedule()
                    {
                        Date = newEntity.Date,
                        DateOfWeek =(int)dayOfWeek,
                        ScheduleStatus = 1,
                        SlotID = existedSlot.SlotID,
                        ClassID = existedClass.ClassID,
                        CreatedBy = newEntity.CreatedBy,
                        CreatedAt = ServerDateTime.GetVnDateTime(),
                        IsDeleted = false
                    };
                        await _unitOfWork.ScheduleRepository.AddAsync(newSchedule);
                }
                catch (DbUpdateException ex)
                {
                    errors.Add($"DbUpdateException for ClassCode {newEntity.ClassCode}: {ex.Message}");
                    continue;
                }
                catch (OperationCanceledException ex)
                {
                    errors.Add($"OperationCanceledException for ClassCode {newEntity.ClassCode}: {ex.Message}");
                    continue;
                }
            }
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponseVM<List<ScheduleVM>>
                    {
                        IsSuccess = true,
                        Title = "Create schedule successfully",
                        Result = createdSchedule
                    };
                }
                else
                {
                    return new ServiceResponseVM<List<ScheduleVM>>
                    {
                        IsSuccess = false,
                        Title = "Create schedule failed",
                        Errors = errors.ToArray()
                    };
                }
        }

        public async Task<IEnumerable<Schedule>> GetSchedules(int startPage, int endPage, Guid lecturerId, int quantity, int semesterId, DateTime? startDate, DateTime? endDate)
        {
            int quantityResult = 0;
            _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
            if (quantityResult == 0)
            {
                throw new ArgumentException("Error when get quantity per page");
            }

            var query = await _unitOfWork.ScheduleRepository.Get(s => s.Class!.LecturerID == lecturerId && s.Class.SemesterID == semesterId)
                .Include(s => s.Class)
                .Include(s => s.Class!.Semester)
                .Include(s => s.Class!.Room)
                .Include(s => s.Slot)
                .Include(s => s.Class!.Subject).ToArrayAsync();

            if (startDate.HasValue)
            {
                var startDateOnly = DateOnly.FromDateTime(startDate.Value);
                query = query.Where(s => s.Date >= startDateOnly).ToArray();

            }
            if (endDate.HasValue)
            {
                var endDateOnly = DateOnly.FromDateTime(endDate.Value);
                query = query.Where(s => s.Date <= endDateOnly).ToArray();
            }

            var schedules = query
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult);

            return schedules;
        }
    }
}
