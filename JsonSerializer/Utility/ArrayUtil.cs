using System;
using System.Collections.Generic;
using System.Text;

namespace Zippy.Utility
{
    static class ArrayUtil
    {
        public static T[] Splice<T>(this T[] source, int index, int length)
        {
            return new ArraySegment<T>(source, index, length).Array;
        }

        public static string ToString(this char[] value)
        {
            return new string(value, 0, value.Length);
        }
    }
}
