using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace JsonSerializer.Utility
{
    static class StringExtension
    {
        /// <summary>
        /// check if string is null or empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string value)
        {
            return value == null || value.Length == 0;
        }

        /// <summary>
        /// check if string is null and not blank
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Pure]
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static readonly CompareInfo s_CompareInfo = CultureInfo.CurrentCulture.CompareInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FastStartsWith(this string source, string value, bool caseSensitive = true)
        {
            if (source == null || value == null)
            {
                return source == null && value == null;
            }
            else if (caseSensitive)
            {
                //internal calls of startwitch calls isprefix
                return s_CompareInfo.IsPrefix(source, value, CompareOptions.None);
            }
            else
            {
                //handles through internal calls
                return source.StartsWith(value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
