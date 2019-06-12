using System;
using System.Collections.Generic;
using System.Text;

namespace SwiftJson.Utility
{
    /// <summary>
    /// Provides a cached reusable instance of a StringBuilder per thread.
    /// </summary>
    static class StringBuilderPool
    {
        //Avoid stringbuilder block fragmentation
        private const int MAX_BUILDER_SIZE = 360;
        //internal stringbuilder default capacity
        private const int DEFAULT_CAPACITY = 256;

        [ThreadStatic]
        private static StringBuilder _cachedInstance;

        /// <summary>
        /// Gets a string builder to use of a particular size.
        /// </summary>
        public static StringBuilder Get(int capacity = DEFAULT_CAPACITY)
        {
            if (capacity <= MAX_BUILDER_SIZE)
            {
                StringBuilder sb = _cachedInstance;
                if (sb != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= sb.Capacity)
                    {
                        _cachedInstance = null;
                        sb.Clear();
                        return sb;
                    }
                }
            }
            return new StringBuilder(capacity);
        }

        /// <summary>
        /// Place the specified builder in the cache if it is not too big.
        /// </summary>
        public static void Release(this StringBuilder sb)
        {
            if (sb?.Capacity <= MAX_BUILDER_SIZE)
            {
                _cachedInstance = sb;
            }
        }

        /// <summary>
        /// Gets the resulting string and releases a StringBuilder instance.
        /// </summary>
        public static string GetStringAndRelease(this StringBuilder sb)
        {
            string result = sb?.ToString();
            Release(sb);
            return result;
        }
    }
}
