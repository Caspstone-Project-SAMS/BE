using Base.API.Controllers;
using Base.Repository.Common;
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

        public void ConfigureRecurringJobsAsync(string jobName, TimeOnly prepareTime,int scheduleID)
        {
            var cronExpression = ConvertToCronExpression(prepareTime);
            _recurringJobManager.AddOrUpdate(jobName,() => SenDataToModule(scheduleID), cronExpression);
            
        }


        public async Task<string> SenDataToModule(int scheduleID)
        {
            var messageSendMode = new MessageSend
            {
                Event = "SendData",
                Data = scheduleID.ToString(),
            };
            var jsonPayloadMode = JsonSerializer.Serialize(messageSendMode);
            var resultMode = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode, scheduleID);

            try
            {
                if (resultMode)
                {
                    return "Send data to moudle successfully";
                }

                return "Send data to moudle unsuccessfully";
            }
            catch (Exception ex)
            {
                return $"Error sending data to module: {ex.Message}";
            }
        }


        private string ConvertToCronExpression(TimeOnly prepareTime)
        {
            return Cron.Daily(prepareTime.Hour, prepareTime.Minute);
        }

    }
}
