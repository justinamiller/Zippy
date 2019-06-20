using System;
using System.Collections.Generic;
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

        private readonly static char[][] _time = new char[60][];

        static DateTimeExtension()
        {
            for (var i = 0; i < 60; i++)
            {
                _time[i] = i.ToString().ToCharArray();
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

        public static long ToUniversalTicks(this DateTime dateTime, TimeSpan offset)
        {
            // special case min and max value
            // they never have a timezone appended to avoid issues
            if (dateTime.Kind == DateTimeKind.Utc || dateTime == DateTime.MaxValue || dateTime == DateTime.MinValue)
            {
                return dateTime.Ticks;
            }

            long ticks = dateTime.Ticks - offset.Ticks;
            if (ticks > 3155378975999999999L)
            {
                return 3155378975999999999L;
            }

            if (ticks < 0L)
            {
                return 0L;
            }

            return ticks;
        }

        public static char[] ToTimeSpanChars(this TimeSpan value)
        {
            // 00:00:00.0848510
            //10675199.02:48:05.4775807
            char[] buffer = new char[26];
            int index = 0;
                long ticks = value.Ticks;
                int day = (int)(ticks / TimeSpan.TicksPerDay);
            long time = ticks % TimeSpan.TicksPerDay;


            int hours = (int)(time / TimeSpan.TicksPerHour % 24);
            int minutes = (int)(time / TimeSpan.TicksPerMinute % 60);
            int seconds = (int)(time / TimeSpan.TicksPerSecond % 60);
            int fraction = (int)(time % TimeSpan.TicksPerSecond);

            if (ticks < 0)
            {
                buffer[index++] = '-';
            }

            if (day > 0)
            {
                if (9 >= day)
                {
                    buffer[index++] = '0';
                    buffer[index++] = MathUtils.charNumbers[day];
                }
                else if(59>=day)
                {
                    var temp = _time[day];
                    buffer[index++] = temp[0];
                    buffer[index++] = temp[1];
                }
                buffer[index++] = '.';
            }

            if (9 >= hours)
            {
                buffer[index++] = '0';
                buffer[index++] = MathUtils.charNumbers[hours];
            }
            else
            {
                var temp = _time[hours];
                buffer[index++] = temp[0];
                buffer[index++] = temp[1];
            }
            buffer[index++] = ':';

            if (9 >= minutes)
            {
                buffer[index++] = '0';
                buffer[index++] = MathUtils.charNumbers[minutes];
            }
            else
            {
                var temp = _time[minutes];
                buffer[index++] = temp[0];
                buffer[index++] = temp[1];
            }
            buffer[index++] = ':';

            if (9 >= seconds)
            {
                buffer[index++] = '0';
                buffer[index++] = MathUtils.charNumbers[seconds];
            }
            else
            {
                var temp = _time[seconds];
                buffer[index++] = temp[0];
                buffer[index++] = temp[1];
            }

            if (fraction > 0)
            {
                buffer[index++] = '.';


                int effectiveDigits = 7 - MathUtils.IntLength((ulong)fraction);

                for (var i = 0; i < effectiveDigits; i++)
                {
                    buffer[index++] = '0';
                }
                var temp = MathUtils.WriteNumberToBuffer((uint)fraction, false);
                int tempLen = temp.Length;
                Array.Copy(temp, 0, buffer, index, tempLen);
                index += tempLen;
            }

            if (index != 26)
            {
                var temp = new char[index];
                Array.Copy(buffer, 0, temp, 0, index);
                return temp;
            }

            return buffer;
        }
    }
}
