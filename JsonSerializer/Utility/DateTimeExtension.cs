using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace SwiftJson.Utility
{
    static class DateTimeExtension
    {
        private const long UnixEpoch = 621355968000000000L;
        private static readonly DateTime UnixEpochDateTimeUtc = new DateTime(UnixEpoch, DateTimeKind.Utc);
        private static readonly DateTime MinDateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimeMs(this DateTime dateTime)
        {
            var universal = ToDateTimeSinceUnixEpoch(dateTime);
            return (long)universal.TotalMilliseconds;
        }

        private static TimeSpan ToDateTimeSinceUnixEpoch(this DateTime dateTime)
        {
            var dtUtc = dateTime;
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                dtUtc = dateTime.Kind == DateTimeKind.Unspecified && dateTime > DateTime.MinValue && dateTime < DateTime.MaxValue
                    ? DateTime.SpecifyKind(dateTime.Subtract(TimeZoneInfo.Local.GetUtcOffset(dateTime)), DateTimeKind.Utc)
                    : dateTime.ToStableUniversalTime();
            }

            var universal = dtUtc.Subtract(UnixEpochDateTimeUtc);
            return universal;
        }

        public static DateTime ToStableUniversalTime(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;
            if (dateTime == DateTime.MinValue)
                return MinDateTimeUtc;

            return dateTime.ToUniversalTime();
        }

        public static long ToUnixTimeMs(this long ticks)
        {
            return (ticks - UnixEpoch) / TimeSpan.TicksPerMillisecond;
        }

        readonly static string[] _time = new string[24]
        {
            "0","1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19","20","21","22","23"
        };

        readonly static char[] _number = new char[10]
        {
            '0','1','2','3','4','5','6','7','8','9'
        };

        public static string ToTimeOffsetString(this TimeSpan offset)
        {
            string hours;
            string minutes;
            var h = Math.Abs(offset.Hours);
            var m = Math.Abs(offset.Minutes);

            if (9 >= h)
            {
                hours = new string(new char[2] { '0', _number[h] });
            }
            else
            {
                hours = _time[h];
            }

            if (m == 0)
            {
                minutes = "00";
            }
            else if (9 >= m)
            {
                minutes = new string(new char[2] { '0', _number[m] });
            }
            else
            {
                minutes = _time[m];
            }


            return string.Concat((offset < TimeSpan.Zero ? "-" : "+"), hours, minutes);
        }
    }
}
