using Base.Repository.Common;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Base.Service.Common
{
    public class HangfireService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        //private readonly WebSocketConnectionManager1 _websocketConnectionManager;
        public HangfireService(IBackgroundJobClient backgroundJobClient,
                               IRecurringJobManager recurringJobManager)
                               //WebSocketConnectionManager1 websocketConnectionManager)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
        }

        public async Task ConfigureRecurringJobsAsync(string jobName, TimeOnly prepareTime)
        {

        }



            private string ConvertToCronExpression(TimeOnly prepareTime)
        {
            return $"{prepareTime.Minute} {prepareTime.Hour} * * *";
        }

    }
}
