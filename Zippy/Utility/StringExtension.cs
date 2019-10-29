using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Runtime.InteropServices;

namespace Zippy.Utility
{
    static class StringExtension
    {
        public readonly static Encoding DefaultEncoding = Encoding.UTF8;
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



        /// <summary>
        /// memory buffer; faster than using stringbuilder.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="quote">apply quotes</param>
        [SuppressMessage("brain-overload", "S1541")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
        public static char[] GetEncodeString(string str, bool escapeHtmlChars, bool quote = true)
        {
            int strLen = str.Length;
            char[] bufferWriter = new char[strLen + (quote ? 2 : 0)];
            int bufferIndex = 0;
            bool hasEncoded = false;

            if (quote)
            {
                //open quote
                bufferWriter[bufferIndex++] = '\"';
            }

            for (var i = 0; i < strLen; i++)
            {
                char c = str[i];
                switch (c)
                {
                    case '"':
                        hasEncoded = true;
                        BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 3);
                        bufferWriter[bufferIndex++] = '\\';
                        bufferWriter[bufferIndex] = '\"';
                        bufferIndex++;
                        break;
                    case '\\':
                        hasEncoded = true;
                        BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 3);
                        bufferWriter[bufferIndex++] = '\\';
                        bufferWriter[bufferIndex] = '\\';
                        bufferIndex++;
                        break;
                    case '\u0007':
                        hasEncoded = true;
                        BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 3);
                        bufferWriter[bufferIndex++] = '\\';
                        bufferWriter[bufferIndex] = 'a';
                        bufferIndex++;
                        break;
                    case '\u0008':
                        hasEncoded = true;
                        BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 3);
                        bufferWriter[bufferIndex++] = '\\';
                        bufferWriter[bufferIndex] = 'b';
                        bufferIndex++;
                        break;
                    case '\u0009':
                        hasEncoded = true;
                        BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 3);
                        bufferWriter[bufferIndex++] = '\\';
                        bufferWriter[bufferIndex] = 't';
                        bufferIndex++;
                        break;
                    case '\u000A':
                        hasEncoded = true;
                        BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 3);
                        bufferWriter[bufferIndex++] = '\\';
                        bufferWriter[bufferIndex] = 'n';
                        bufferIndex++;
                        break;
                    case '\u000B':
                        hasEncoded = true;
                        BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 3);
                        bufferWriter[bufferIndex++] = '\\';
                        bufferWriter[bufferIndex] = 'v';
                        bufferIndex++;
                        break;
                    case '\u000C':
                        hasEncoded = true;
                        BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 3);
                        bufferWriter[bufferIndex++] = '\\';
                        bufferWriter[bufferIndex] = 'f';
                        bufferIndex++;
                        break;
                    case '\u000D':
                        hasEncoded = true;
                        BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 3);
                        bufferWriter[bufferIndex++] = '\\';
                        bufferWriter[bufferIndex++] = 'r';
                        break;
                    default:
                        if (escapeHtmlChars)
                        {
                            switch (c)
                            {
                                case '<':
                                    hasEncoded = true;
                                    BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 7);
                                    bufferWriter[bufferIndex++] = '\\';
                                    bufferWriter[bufferIndex++] = 'u';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '3';
                                    bufferWriter[bufferIndex++] = 'c';
                                    continue;
                                case '>':
                                    hasEncoded = true;
                                    BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 7);
                                    bufferWriter[bufferIndex++] = '\\';
                                    bufferWriter[bufferIndex++] = 'u';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '3';
                                    bufferWriter[bufferIndex++] = 'e';
                                    continue;
                                case '&':
                                    hasEncoded = true;
                                    BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 7);
                                    bufferWriter[bufferIndex++] = '\\';
                                    bufferWriter[bufferIndex++] = 'u';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '2';
                                    bufferWriter[bufferIndex++] = '6';
                                    continue;
                                case '=':
                                    hasEncoded = true;
                                    BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 7);
                                    bufferWriter[bufferIndex++] = '\\';
                                    bufferWriter[bufferIndex++] = 'u';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '3';
                                    bufferWriter[bufferIndex++] = 'd';
                                    continue;
                                case '\'':
                                    hasEncoded = true;
                                    BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 7);
                                    bufferWriter[bufferIndex++] = '\\';
                                    bufferWriter[bufferIndex++] = 'u';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '0';
                                    bufferWriter[bufferIndex++] = '2';
                                    bufferWriter[bufferIndex++] = '7';
                                    continue;
                            }
                        }

                        //printable
                        if (c >= 32 && c <= 126)// ' ' && '~'
                        {
                            if (hasEncoded)
                            {
                                BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 2);
                            }

                            bufferWriter[bufferIndex++] = c;
                            break;
                        }
                        else if (char.IsControl(c))
                        {
                            hasEncoded = true;
                            BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 7);

                            var hexSeqBuffer = new char[4];
                            // Default, turn into a \uXXXX sequence
                            IntToHex(c, hexSeqBuffer);
                            bufferWriter[bufferIndex++] = '\\';
                            bufferWriter[bufferIndex++] = 'u';
                            bufferWriter[bufferIndex++] = hexSeqBuffer[0];
                            bufferWriter[bufferIndex++] = hexSeqBuffer[1];
                            bufferWriter[bufferIndex++] = hexSeqBuffer[2];
                            bufferWriter[bufferIndex++] = hexSeqBuffer[3];
                            break;
                        }
                        else
                        {
                            if (hasEncoded)
                            {
                                BinaryUtil.EnsureCapacityFixed(ref bufferWriter, bufferIndex, 2);
                            }

                            bufferWriter[bufferIndex++] = c;
                            break;
                        }
                }
            }

            if (quote)
            {
                //close quote
                bufferWriter[bufferIndex++] = '\"';
            }

            //flush
            if (hasEncoded && bufferWriter.Length != bufferIndex)
            {
                BinaryUtil.FastResize(ref bufferWriter, bufferIndex);
            }

            return bufferWriter;
        }



        public static string PrettyPrint(string input)
        {
            if (input.IsNullOrEmpty()) 
            {
                return null;
            }
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

        // Micro optimized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntToHex(int intValue, char[] hex)
        {
            for (var i = 3; i >= 0; i--)
            {
                var num = intValue & 0xF; // intValue % 16

                // 0x30 + num == '0' + num
                // 0x37 + num == 'A' + (num - 10)
                hex[i] = (char)((num < 10 ? 0x30 : 0x37) + num);

                intValue >>= 4;
            }
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
