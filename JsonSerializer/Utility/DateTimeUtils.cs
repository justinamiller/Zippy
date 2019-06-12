using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SwiftJson.Utility
{
    static class DateTimeUtils
    {
        private static readonly IFormatProvider s_cultureInfo = CultureInfo.InvariantCulture;

        public static string GetDateTimeUtcString(DateTime datetime)
        {
            DateTime convertDateTime = datetime;
            if(convertDateTime.Kind!= DateTimeKind.Utc)
            {
                convertDateTime = convertDateTime.ToUniversalTime();
            }

            //ISO8601
            return convertDateTime.ToString("o", s_cultureInfo);
        }
    }
}
