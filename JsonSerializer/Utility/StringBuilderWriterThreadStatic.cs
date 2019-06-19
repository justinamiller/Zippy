using System;
using Zippy.Serialize.Writers;

namespace Zippy.Utility
{
    //Use separate cache internally to avoid reallocations and cache misses
    static class StringWriterThreadStatic
    {
        [ThreadStatic]
        static StringBuilderWriter s_Cache;

        public static StringBuilderWriter Allocate()
        {
            var ret = s_Cache;
            if (ret == null)
            {
                return new StringBuilderWriter(256);
            }
            else
            {
                ret.Clear();
                s_Cache = null;  //don't re-issue cached instance until it's freed
            }
            return ret;
        }

        public static void Free(StringBuilderWriter writer)
        {
            s_Cache = writer;
        }

        public static string ReturnAndFree(StringBuilderWriter writer)
        {
            var ret = writer.ToString();
            s_Cache = writer;
            return ret;
        }
    }
}
