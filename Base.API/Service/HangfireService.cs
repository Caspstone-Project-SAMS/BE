using Base.API.Controllers;
using Base.Repository.Common;
using Base.Repository.Entity;
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

        public void ConfigureRecurringJobsAsync(string jobName, TimeOnly? prepareTime, DateOnly? date, int moduleId)
        {
            var cronExpression = ConvertToCronExpression(prepareTime);
            _recurringJobManager.AddOrUpdate(jobName,() => SenDataToModule(date, moduleId), cronExpression);
            
        }


        public async Task<string> SenDataToModule(DateOnly? date, int moduleId)
        {
            var messageSendMode = new WebsocketMessage
            {
                Event = "PrepareSchedules",
                Data = date?.ToString(),
            };
            var jsonPayloadMode = JsonSerializer.Serialize(messageSendMode);
            var resultMode = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode,moduleId);

            try
            {
                if (resultMode)
                {
                    return $"Prepare schedules for module unsuccessfully";
                }
                return "Prepare schedules for module unsuccessfully";
            }
            catch (Exception ex)
            {
                return $"Error sending data to module: {ex.Message}";
            }
        }


        private string ConvertToCronExpression(TimeOnly? prepareTime)
        {
            if (prepareTime.HasValue)
            {
                TimeOnly time = prepareTime.Value;
                TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                // Tạo DateTime với Kind là Unspecified
                DateTime vnDateTimeUnspecified = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, time.Hour, time.Minute, 0, DateTimeKind.Unspecified);

                // Chuyển đổi từ thời gian Unspecified sang thời gian VN (Local)
                DateTime vnDateTime = TimeZoneInfo.ConvertTime(vnDateTimeUnspecified, vnTimeZone);

                // Lấy giờ và phút theo thời gian VN để tạo cron expression
                return Cron.Daily(vnDateTime.Hour, vnDateTime.Minute);
            }
            else
            {
                return Cron.Daily();
            }
        }

    }
}
