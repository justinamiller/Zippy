using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Buffers.Text;
using System.Runtime.InteropServices;

namespace Zippy.Utility
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

        /// <summary>
        /// memory buffer; faster than using stringbuilder.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="quote">apply quotes</param>
        [SuppressMessage("brain-overload", "S1541")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static unsafe char[] GetEncodeString(string str, bool quote = true)
        {
            int len = (str.Length * 2) + 2;
            char[] bufferWriter = new char[len];
            int bufferIndex = 0;

            if (quote)
            {
                //open quote
                bufferWriter[bufferIndex] = '\"';
                bufferIndex++;
            }

            if (len > 2)
            {
                char c;
                fixed (char* chr = str)
                {
                    char* ptr = chr;
                    while ((c = *(ptr++)) != '\0')
                    {
                        switch (c)
                        {
                            case '"':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = '\"';
                                bufferIndex++;
                                break;
                            case '\\':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                break;
                            case '\u0007':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'a';
                                bufferIndex++;
                                break;
                            case '\u0008':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'b';
                                bufferIndex++;
                                break;
                            case '\u0009':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 't';
                                bufferIndex++;
                                break;
                            case '\u000A':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'n';
                                bufferIndex++;
                                break;
                            case '\u000B':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'v';
                                bufferIndex++;
                                break;
                            case '\u000C':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'f';
                                bufferIndex++;
                                break;
                            case '\u000D':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'r';
                                bufferIndex++;
                                break;
                            default:
                                if (31 >= c)
                                {
                                    bufferWriter[bufferIndex] = '\\';
                                    bufferIndex++;
                                    bufferWriter[bufferIndex] = c;
                                    bufferIndex++;
                                }
                                else
                                {
                                    bufferWriter[bufferIndex] = c;
                                    bufferIndex++;
                                }
                                break;
                        }
                    }
                }
            }

            if (quote)
            {
                //close quote
                bufferWriter[bufferIndex] = '\"';
                bufferIndex++;
            }

            //flush
            var buffer = new char[bufferIndex];
            Array.Copy(bufferWriter, 0, buffer, 0, bufferIndex);
            return buffer;
        }

        /// <summary>
        /// clean up invalid characters that is not support within elastic search.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FormatElasticName(string input)
        {
            char[] charArray = input.ToCharArray();
            int length = charArray.Length;
            char[] data = new char[length];
            for (var i = 0; i < length; i++)
            {
                char ch = charArray[i];
                if (char.IsLetterOrDigit(ch))
                {
                    data[i] = ch;
                }
                else
                {
                    data[i] = '_';
                }

            }
            return new string(data);
        }


        /// <summary>
        /// Searches the string for one or more non-printable characters.
        /// </summary>
        /// <param name="value">The string to search.</param>
        /// <param name="escapeHtmlChars"></param>
        /// <returns>True if there are any characters that require escaping. False if the value can be written verbatim.</returns>
        /// <remarks>
        /// Micro optimizations: since quote and backslash are the only printable characters requiring escaping, removed previous optimization
        /// (using flags instead of value.IndexOfAny(EscapeChars)) in favor of two equality operations saving both memory and CPU time.
        /// Also slightly reduced code size by re-arranging conditions.
        /// TODO: Possible Linq-only solution requires profiling: return value.Any(c => !c.IsPrintable() || c == QuoteChar || c == EscapeChar);
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyEscapeChars(this string value, bool escapeHtmlChars)
        {
            char c;
            unsafe
            {
                fixed (char* chr = value)
                {
                    char* ptr = chr;
                    while ((c = *(ptr++)) != '\0')
                    {
                        if (c.HasAnyEscapeChar(escapeHtmlChars))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyEscapeChar(this char c, bool escapeHtmlChars)
        {
            if (!(c >= 32 && c <= 126) || c == '"' || c == '\\')
                return true;

            if (escapeHtmlChars && (c == '<' || c == '>' || c == '&' || c == '=' || c == '\\'))
                return true;

            return false;
        }

        public static string PrettyPrint(string input)
        {
            string spaces = new string(' ', 3);

            var stringBuilder = StringBuilderPool.Get();
            int num = 0;
            int length = input.Length;
            char[] array = input.ToCharArray();
            for (int i = 0; i < length; i++)
            {
                char c = array[i];
                if (c == '"')
                {
                    bool flag = true;
                    while (flag)
                    {
                        stringBuilder.Append(c);
                        c = array[++i];
                        switch (c)
                        {
                            case '\\':
                                stringBuilder.Append(c);
                                c = array[++i];
                                break;
                            case '"':
                                flag = false;
                                break;
                        }
                    }
                }
                switch (c)
                {
                    case '[':
                    case '{':
                        stringBuilder.Append(c);
                        stringBuilder.AppendLine();
                        AppendIndent(stringBuilder, ++num, spaces);
                        break;
                    case ']':
                    case '}':
                        stringBuilder.AppendLine();
                        AppendIndent(stringBuilder, --num, spaces);
                        stringBuilder.Append(c);
                        break;
                    case ',':
                        stringBuilder.Append(c);
                        stringBuilder.AppendLine();
                        AppendIndent(stringBuilder, num, spaces);
                        break;
                    case ':':
                        stringBuilder.Append(" : ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(c))
                        {
                            stringBuilder.Append(c);
                        }
                        break;
                }
            }
            return StringBuilderPool.GetStringAndRelease(stringBuilder);
        }

        private static void AppendIndent(StringBuilder sb, int count, string indent)
        {
            while (count > 0)
            {
                sb.Append(indent);
                count--;
            }
        }

        private const int LowerCaseOffset = 'a' - 'A';
        public static string ToCamelCase(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            var len = value.Length;
            var newValue = new char[len];
            var firstPart = true;

            for (var i = 0; i < len; ++i)
            {
                var c0 = value[i];
                var c1 = i < len - 1 ? value[i + 1] : 'A';
                var c0isUpper = c0 >= 'A' && c0 <= 'Z';
                var c1isUpper = c1 >= 'A' && c1 <= 'Z';

                if (firstPart && c0isUpper && (c1isUpper || i == 0))
                    c0 = (char)(c0 + LowerCaseOffset);
                else
                    firstPart = false;

                newValue[i] = c0;
            }

            return new string(newValue);
        }

        public static string ToLowercaseUnderscore(this string value)
        {
            if (value.IsNullOrEmpty()) return value;
            value = value.ToCamelCase();

            var sb = StringBuilderPool.Get();
            foreach (char t in value)
            {
                if (char.IsDigit(t) || (char.IsLetter(t) && char.IsLower(t)) || t == '_')
                {
                    sb.Append(t);
                }
                else
                {
                    sb.Append("_");
                    sb.Append(char.ToLower(t));
                }
            }
            return StringBuilderPool.GetStringAndRelease(sb);
        }
    }
}
