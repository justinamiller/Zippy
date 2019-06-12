using System;
using System.Collections.Generic;
using System.Text;

namespace SwiftJson.Utility
{
   static class CharPool
    {
        [ThreadStatic]
        static char[] _buffer = null;

        public static char[] GetBuffer(int capacity = 100)
        {
            if (capacity <= 512)
            {
              var buffer = _buffer;
                if (buffer != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= buffer.Length)
                    {
                        _buffer = null;
                        return buffer;
                    }
                }
            }
            return new char[capacity];
        }

        public static void Return(char[] buffer)
        {
            var len = _buffer.Length;
            if (512 >= len)
            {
                Array.Clear(_buffer, 0, len);
                _buffer = buffer;
            }
        }
    }
}
