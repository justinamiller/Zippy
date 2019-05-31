using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace JsonSerializer.Utility
{
   internal static class DateTimeExtension
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

        public static string ToTimeOffsetString(this TimeSpan offset, string seperator = "")
        {
            var hours = Math.Abs(offset.Hours).ToString(CultureInfo.InvariantCulture);
            var minutes = Math.Abs(offset.Minutes).ToString(CultureInfo.InvariantCulture);
            return (offset < TimeSpan.Zero ? "-" : "+")
                + (hours.Length == 1 ? "0" + hours : hours)
                + seperator
                + (minutes.Length == 1 ? "0" + minutes : minutes);
        }
    }
}
