using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JsonSerializer.Utility
{
    static class DateTimeUtils
    {
        private static readonly IFormatProvider s_cultureInfo = CultureInfo.InvariantCulture;

        public static string GetDateTimeUtcString(DateTime datetime)
        {
            DateTime convertDateTime;
            switch (datetime.Kind)
            {
                case DateTimeKind.Local:
                case DateTimeKind.Unspecified:
                    convertDateTime = datetime.ToUniversalTime();
                    break;
                default:
                    convertDateTime = datetime;
                    break;
            }

            //ISO8601
            return convertDateTime.ToString("o", s_cultureInfo);
        }
    }
}
