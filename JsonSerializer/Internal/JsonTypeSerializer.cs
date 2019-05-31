﻿using JsonSerializer.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using static JsonSerializer.Utility.DateTimeExtension;

namespace JsonSerializer.Internal
{
    class JsonTypeSerializer
    {

        /// <summary>
        /// Shortcut escape when we're sure value doesn't contain any escaped chars
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public void WriteRawString(TextWriter writer, string value)
        {
            writer.Write(FastJsonWriter.QuoteChar);
            writer.Write(value);
            writer.Write(FastJsonWriter.QuoteChar);
        }

        /// <summary>
        /// Shortcut escape when we're sure value doesn't contain any escaped chars
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public void WriteRawString(TextWriter writer, char[] value)
        {
            writer.Write(FastJsonWriter.QuoteChar);
            writer.Write(value);
            writer.Write(FastJsonWriter.QuoteChar);
        }

        public void WriteString(TextWriter writer, object value)
        {
          //  WriteString1(writer, value);
            WriteString(writer, (string)value);
        }

        public void WriteString1(TextWriter writer, object value)
        {
            string str = (string)value;
            if (str == null)
            {
                WriteNull(writer, null);
            }
            else if (str.Length == 0)
            {
                writer.Write(new char[2] { FastJsonWriter.QuoteChar, FastJsonWriter.QuoteChar }, 0, 2);
            }
            else
            {
                writer.Write(StringExtension.GetEncodeString(str));
            }
        }

        public const char SpaceChar = ' ';
        public const char TabChar = '\t';
        public const char CarriageReturnChar = '\r';
        public const char LineFeedChar = '\n';
        public const char FormFeedChar = '\f';
        public const char BackspaceChar = '\b';
        public const char EscapeChar = '\\';
        public const char QuoteChar = '"';

        private static readonly char[] EscapedBackslash = { EscapeChar, EscapeChar };
        private static readonly char[] EscapedTab = { EscapeChar, 't' };
        private static readonly char[] EscapedCarriageReturn = { EscapeChar, 'r' };
        private static readonly char[] EscapedLineFeed = { EscapeChar, 'n' };
        private static readonly char[] EscapedFormFeed = { EscapeChar, 'f' };
        private static readonly char[] EscapedBackspace = { EscapeChar, 'b' };
        private static readonly char[] EscapedQuote = { EscapeChar, QuoteChar };

        public void WriteString(TextWriter writer, string value)
        {
            if (value == null)
            {
                WriteNull(writer, null);
                return;
            }

            var escapeHtmlChars = false;
            var escapeUnicode = false;

            if (!value.HasAnyEscapeChars(escapeHtmlChars))
            {
                writer.Write(FastJsonWriter.QuoteChar);
                writer.Write(value);
                writer.Write(FastJsonWriter.QuoteChar);
                return;
            }


            var hexSeqBuffer = new char[4];
            writer.Write(FastJsonWriter.QuoteChar);
            var len = value.Length;
            for (var i = 0; i < len; i++)
            {
                char c = value[i];

                switch (c)
                {
                    case LineFeedChar:
                        writer.Write(EscapedLineFeed);
                        continue;

                    case CarriageReturnChar:
                        writer.Write(EscapedCarriageReturn);
                        continue;

                    case TabChar:
                        writer.Write(EscapedTab);
                        continue;

                    case QuoteChar:
                        writer.Write(EscapedQuote);
                        continue;

                    case EscapeChar:
                        writer.Write(EscapedBackslash);
                        continue;

                    case FormFeedChar:
                        writer.Write(EscapedFormFeed);
                        continue;

                    case BackspaceChar:
                        writer.Write(EscapedBackspace);
                        continue;
                }

                if (escapeHtmlChars)
                {
                    switch (c)
                    {
                        case '<':
                            writer.Write("\\u003c");
                            continue;
                        case '>':
                            writer.Write("\\u003e");
                            continue;
                        case '&':
                            writer.Write("\\u0026");
                            continue;
                        case '=':
                            writer.Write("\\u003d");
                            continue;
                        case '\'':
                            writer.Write("\\u0027");
                            continue;
                    }
                }

                if (c.IsPrintable())
                {
                    writer.Write(c);
                    continue;
                }

                // http://json.org/ spec requires any control char to be escaped
                if (escapeUnicode || char.IsControl(c))
                {
                    // Default, turn into a \uXXXX sequence
                    IntToHex(c, hexSeqBuffer);
                    writer.Write("\\u");
                    writer.Write(hexSeqBuffer);
                }
                else
                {
                    writer.Write(c);
                }
            }

            writer.Write(FastJsonWriter.QuoteChar);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntToHex(int intValue, char[] hex)
        {
            // TODO: test if unrolling loop is faster
            for (var i = 3; i >= 0; i--)
            {
                var num = intValue & 0xF; // intValue % 16

                // 0x30 + num == '0' + num
                // 0x37 + num == 'A' + (num - 10)
                hex[i] = (char)((num < 10 ? 0x30 : 0x37) + num);

                intValue >>= 4;
            }
        }

        public void WriteBuiltIn(TextWriter writer, object value)
        {
       
        }

        public void WriteObjectString(TextWriter writer, object value)
        {

        }

        public void WriteFormattableObjectString(TextWriter writer, object value)
        {
        }

        public void WriteException(TextWriter writer, object value)
        {
        }

        private static readonly long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
        public void WriteDateTime(TextWriter writer, object oDateTime)
        {
            writer.Write(FastJsonWriter.QuoteChar);
         //  writer.Write(DateTimeUtils.GetDateTimeUtcString((DateTime)oDateTime));

            WriteJsonDate(writer, (DateTime)oDateTime);

            writer.Write(FastJsonWriter.QuoteChar);
        }

        private static void WriteJsonDate(TextWriter writer, DateTime dateTime)
        {
            writer.Write(@"\/Date(");
            writer.Write((dateTime.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
            writer.Write(@")\/");
        }

        public void WriteNullableDateTime(TextWriter writer, object dateTime)
        {
            if (dateTime == null)
                WriteNull(writer, null);
            else
                WriteDateTime(writer, dateTime);
        }

        public void WriteDateTimeOffset(TextWriter writer, object oDateTimeOffset)
        {
        }

        public void WriteNullableDateTimeOffset(TextWriter writer, object dateTimeOffset)
        {
        }

        public void WriteTimeSpan(TextWriter writer, object oTimeSpan)
        {
            writer.Write(FastJsonWriter.QuoteChar);
            writer.Write(((TimeSpan)oTimeSpan).ToString());
            writer.Write(FastJsonWriter.QuoteChar);
        }

        public void WriteNullableTimeSpan(TextWriter writer, object oTimeSpan)
        {
            if (oTimeSpan == null)
                WriteNull(writer, null);
            else
                WriteTimeSpan(writer, oTimeSpan);
        }

        public void WriteGuid(TextWriter writer, object oValue)
        {
            WriteRawString(writer, ((Guid)oValue).ToString("N"));
        }

        public void WriteNullableGuid(TextWriter writer, object oValue)
        {
            if (oValue == null) return;
            WriteRawString(writer, ((Guid)oValue).ToString("N"));
        }

        public void WriteBytes(TextWriter writer, object oByteValue)
        {
            if (oByteValue == null) return;
            WriteRawString(writer, Convert.ToBase64String((byte[])oByteValue));
        }

        public void WriteUri(TextWriter writer, object uri)
        {
            if (uri == null)
            {
                WriteNull(writer, null);
            }
            else
            {
                WriteString(writer,((Uri)uri).OriginalString);
            }
        }

        private readonly static char[] s_Null = new char[4] { 'n', 'u', 'l', 'l' };
        public void WriteNull(TextWriter writer, object oNull)
        {
            writer.Write(s_Null, 0, 4);
        }

        public void WriteChar(TextWriter writer, object charValue)
        {
            if (charValue == null)
                WriteNull(writer, null);
            else
                WriteString(writer, ((char)charValue).ToString());
        }

        public void WriteByte(TextWriter writer, object byteValue)
        {
            if (byteValue == null)
                WriteNull(writer, null);
            else
                WriteIntegerValue(writer, (byte)byteValue);
        }

        public void WriteSByte(TextWriter writer, object sbyteValue)
        {
            if (sbyteValue == null)
                WriteNull(writer, null);
            else
                WriteIntegerValue(writer, (sbyte)sbyteValue);
        }

        public void WriteInt16(TextWriter writer, object intValue)
        {
            if (intValue == null)
                WriteNull(writer, null);
            else
                WriteIntegerValue(writer, (short)intValue);
        }

        public void WriteUInt16(TextWriter writer, object intValue)
        {
            if (intValue == null)
                WriteNull(writer, null);
            else
                WriteIntegerValue(writer, (ushort)intValue);
        }

        public void WriteInt32(TextWriter writer, object intValue)
        {
            if (intValue == null)
                WriteNull(writer, null);
            else
                WriteIntegerValue(writer, (int)intValue);
        }

        public void WriteUInt32(TextWriter writer, object uintValue)
        {
            if (uintValue == null)
                WriteNull(writer, null);
            else
                WriteIntegerValue(writer, (uint)uintValue);
        }

        public void WriteInt64(TextWriter writer, object integerValue)
        {
            if (integerValue == null)
                WriteNull(writer, null);
            else
                WriteIntegerValue(writer, (long)integerValue);
        }

        public void WriteUInt64(TextWriter writer, object ulongValue)
        {
            if (ulongValue == null)
            {
                WriteNull(writer, null);
            }
            else
                writer.Write((ulong)ulongValue);
        }

        public void WriteBool(TextWriter writer, object boolValue)
        {
            if (boolValue == null)
                WriteNull(writer, null);
            else
                writer.Write(((bool)boolValue) ?"true" :"false");
        }

        public void WriteFloat(TextWriter writer, object floatValue)
        {
            if (floatValue == null)
                WriteNull(writer, null);
            else
            {
                var floatVal = (float)floatValue;
               writer.Write(floatVal.ToString("r", CultureInfo.InvariantCulture));
            }
        }

        public void WriteDouble(TextWriter writer, object doubleValue)
        {
            if (doubleValue == null)
                WriteNull(writer, null);
            else
            {
                var doubleVal = (double)doubleValue;
                    writer.Write(doubleVal.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void WriteDecimal(TextWriter writer, object decimalValue)
        {
            if (decimalValue == null)
                WriteNull(writer, null);
            else
                writer.Write(((decimal)decimalValue).ToString(CultureInfo.InvariantCulture));
        }

        public void WriteEnum(TextWriter writer, object enumValue)
        {

        }

        private static void WriteIntegerValue(TextWriter writer, int value)
        {
            if (value >= 0 && value <= 9)
            {
                writer.Write((char)('0' + value));
            }
            else
            {
                bool negative = value < 0;
                WriteIntegerValue(writer, negative ? (uint)-value : (uint)value, negative);
            }
        }

        private static void WriteIntegerValue(TextWriter writer,uint value, bool negative)
        {
            if (!negative && value <= 9)
            {
                writer.Write((char)('0' + value));
            }
            else
            {
                int length = 0;
                var buffer = WriteNumberToBuffer(value, negative, ref length);
                writer.Write(buffer, 0, length);
            }
        }

        private static void WriteIntegerValue(TextWriter writer,long value)
        {
            if (value >= 0 && value <= 9)
            {
              writer.Write((char)('0' + value));
            }
            else
            {
                bool negative = value < 0;
                WriteIntegerValue(writer, negative ? (ulong)-value : (ulong)value, negative);
            }
        }

        private static void WriteIntegerValue(TextWriter writer, ulong value, bool negative)
        {
            if (!negative && value <= 9)
            {
                writer.Write((char)('0' + value));
            }
            else
            {
                int length = 0;
                var buffer = WriteNumberToBuffer(value, negative, ref length);
                writer.Write(buffer, 0, length);
            }
        }

        private static char[] WriteNumberToBuffer(uint value, bool negative, ref int totalLength)
        {
            char[] buffer = new char[35];
            totalLength = MathUtils.IntLength(value);

            if (negative)
            {
                totalLength++;
                buffer[0] = '-';
            }

            int index = totalLength;

            do
            {
                uint quotient = value / 10;
                uint digit = value - (quotient * 10);
                buffer[--index] = (char)('0' + digit);
                value = quotient;
            } while (value != 0);

            return buffer;
        }

        private static char[] WriteNumberToBuffer(ulong value, bool negative, ref int totalLength)
        {
            if (value <= uint.MaxValue)
            {
                // avoid the 64 bit division if possible
                return WriteNumberToBuffer((uint)value, negative, ref totalLength);
            }


            char[] buffer = new char[35];
            totalLength = MathUtils.IntLength(value);

            if (negative)
            {
                totalLength++;
                buffer[0] = '-';
            }

            int index = totalLength;

            do
            {
                ulong quotient = value / 10;
                ulong digit = value - (quotient * 10);
                buffer[--index] = (char)('0' + digit);
                value = quotient;
            } while (value != 0);

            return buffer;
        }
    }
}