using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Common
{
    public static class ServerDateTime
    {
        // If the timeDifference is larger than 0 => updated time is on future
        // If the timeDifference is less than 0 => updated time is on past
        private static double timeDifferenceMinutes = 0;

        public static void SetServerTime(DateTime newTime)
        {
            var currentVnTime = GetExactlyVnDateTime();

            // Count the difference
            var difference = currentVnTime - newTime;
            timeDifferenceMinutes = difference.TotalMinutes;
        }

        public static DateTime GetVnDateTime()
        {
            DateTime utcDateTime = DateTime.UtcNow;
            string vnTimeZoneKey = "SE Asia Standard Time";
            TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(vnTimeZoneKey);
            DateTime vnDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, vnTimeZone);
            return vnDateTime.AddMinutes(timeDifferenceMinutes);
        }

        public static DateTime GetExactlyVnDateTime()
        {
            DateTime utcDateTime = DateTime.UtcNow;
            string vnTimeZoneKey = "SE Asia Standard Time";
            TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(vnTimeZoneKey);
            DateTime vnDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, vnTimeZone);
            return vnDateTime;
        }
    }
}
