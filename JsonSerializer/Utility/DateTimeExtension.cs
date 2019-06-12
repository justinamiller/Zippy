﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Zippy.Utility
{
    static class DateTimeExtension
    {
        private const long UnixEpoch = 621355968000000000L;
        private static readonly DateTime UnixEpochDateTimeUtc = new DateTime(UnixEpoch, DateTimeKind.Utc);
        private static readonly DateTime MinDateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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

        readonly static IList<char[]> _time = new List<char[]>();

        static DateTimeExtension()
        {
            for(var i=0; i < 60; i++)
            {
                _time.Add(i.ToString().ToCharArray());
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char[] ToTimeOffsetString(this TimeSpan offset)
        {
            var h = offset.Hours;
            if (h < 0)
            {
                h = -h;
            }
            var m = offset.Minutes;
            if (m < 0)
            {
                m = -m;
            }

            char[] items = new char[5];
            int index = 0;

            items[index++] = offset < TimeSpan.Zero ? '-' : '+';

            if (9 >= h)
            {
                items[index++] = '0';
                items[index++] = MathUtils.charNumbers[h];
            }
            else
            {
                var time = _time[h];
                items[index++] = time[0];
                items[index++] = time[1];
            }

            if (m == 0)
            {
                items[index++] = '0';
                items[index++] = '0';
            }
            else if (9 >= m)
            {
                items[index++] = '0';
                items[index++] = MathUtils.charNumbers[m];
            }
            else
            {
                var time = _time[m];
                items[index++] = time[0];
                items[index++] = time[1];
            }

            return items;
        }
    }
}
