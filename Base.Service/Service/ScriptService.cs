using Base.Service.Common;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class ScriptService
{
    public void setTime(DateTime time)
    {
        // Setup server datetime
        ServerDateTime.SetServerTime(time);

        // setup job at the past
        var api = JobStorage.Current.GetMonitoringApi();
        //var pastJob = api.
    }
}
