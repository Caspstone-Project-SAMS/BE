using Base.API.Controllers;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Base.API.Service
{
    public class HangfireService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly WebSocketConnectionManager1 _websocketConnectionManager;
        public HangfireService(IBackgroundJobClient backgroundJobClient,
                               IRecurringJobManager recurringJobManager,
                               WebSocketConnectionManager1 websocketConnectionManager)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
            _websocketConnectionManager = websocketConnectionManager;
        }

        public void ConfigureRecurringJobsAsync(string jobName, TimeOnly? prepareTime, int moduleId)
        {
            DateTime vnDateTime = ServerDateTime.GetVnDateTime();
            DateOnly? date = null;
            if (prepareTime.HasValue && prepareTime.Value >= new TimeOnly(19, 0) && prepareTime.Value < new TimeOnly(0, 0))
            {
                date = DateOnly.FromDateTime(vnDateTime.AddDays(1));
            }
            else
            {
                date = DateOnly.FromDateTime(vnDateTime);
            }

            var cronExpression = ConvertToCronExpression(prepareTime);

            _recurringJobManager.AddOrUpdate(
                jobName,
                () => SenDataToModule(date, moduleId),
                cronExpression,
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"),
                }
            );
                
        }
        public void RemoveRecurringJobsAsync(string jobId)
        {
            _recurringJobManager.RemoveIfExists(jobId);
        }

        public async Task<string> SenDataToModule(DateOnly? date, int moduleId)
        {
            var messageSendMode = new WebsocketMessage
            {
                Event = "PrepareSchedules",
                Data = new
                {
                    PrepareDate = date?.ToString("yyyy-MM-dd")
                },
            };
            var jsonPayloadMode = JsonSerializer.Serialize(messageSendMode);
            var resultMode = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode,moduleId);

            try
            {
                if (resultMode)
                {
                    return $"Prepare schedules for module unsuccessfully";
                }
                return $"Prepare schedules for module unsuccessfully, data {jsonPayloadMode}";
            }
            catch (Exception ex)
            {
                return $"Error sending data to module: {ex.Message}";
            }
        }


        private static string ConvertToCronExpression(TimeOnly? prepareTime)
        {
            if (prepareTime.HasValue)
            {
                TimeOnly time = prepareTime.Value;
                return Cron.Daily(time.Hour, time.Minute);
            }
            else
            {
                return Cron.Daily();
            }
        }
    }
}
