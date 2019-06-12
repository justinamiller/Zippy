using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Zippy.Utility
{
    //Use separate cache internally to avoid reallocations and cache misses
     static class StringWriterThreadStatic
    {
        [ThreadStatic]
        static StringWriter s_Cache;

        public static StringWriter Allocate()
        {
            var ret = s_Cache;
            if (ret == null)
            {
                return new StringWriter(new StringBuilder(512),CultureInfo.InvariantCulture);
            }
            else
            {
                var sb = ret.GetStringBuilder();
                sb.Length = 0;
            }
                        
            s_Cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringWriter writer)
        {
            s_Cache = writer;
        }

        public static string ReturnAndFree(StringWriter writer)
        {
            var ret = writer.ToString();
            s_Cache = writer;
            return ret;
        }
    }
}
