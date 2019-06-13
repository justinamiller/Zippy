using Zippy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using static Zippy.Utility.DateTimeExtension;

namespace Zippy.Serialize
{

   delegate void WriteObjectDelegate(TextWriter writer, object obj);
    sealed class JsonTypeSerializer
    {
        public const char QuoteChar = '"';
        internal static readonly JsonTypeSerializer Serializer = new JsonTypeSerializer();
        private static readonly CultureInfo _cultureInfo = CultureInfo.InvariantCulture;

        public static WriteObjectDelegate GetValueTypeToStringMethod(TypeSerializerUtils.TypeCode typeCode)
        {
            if (typeCode >= TypeSerializerUtils.TypeCode.NotSetObject)
            {
                return null;
            }

            switch (typeCode)
            {
                case TypeSerializerUtils.TypeCode.CharNullable:
                case TypeSerializerUtils.TypeCode.Char:
                    return Serializer.WriteChar;
                case TypeSerializerUtils.TypeCode.BooleanNullable:
                case TypeSerializerUtils.TypeCode.Boolean:
                    return Serializer.WriteBool;
                case TypeSerializerUtils.TypeCode.SByteNullable:
                case TypeSerializerUtils.TypeCode.SByte:
                    return Serializer.WriteSByte;
                case TypeSerializerUtils.TypeCode.Int16Nullable:
                case TypeSerializerUtils.TypeCode.Int16:
                    return Serializer.WriteInt16;
                case TypeSerializerUtils.TypeCode.UInt16Nullable:
                case TypeSerializerUtils.TypeCode.UInt16:
                    return Serializer.WriteUInt16;
                case TypeSerializerUtils.TypeCode.Int32Nullable:
                case TypeSerializerUtils.TypeCode.Int32:
                    return Serializer.WriteInt32;
                case TypeSerializerUtils.TypeCode.ByteNullable:
                case TypeSerializerUtils.TypeCode.Byte:
                    return Serializer.WriteByte;
                case TypeSerializerUtils.TypeCode.UInt32Nullable:
                case TypeSerializerUtils.TypeCode.UInt32:
                    return Serializer.WriteUInt32;
                case TypeSerializerUtils.TypeCode.Int64Nullable:
                case TypeSerializerUtils.TypeCode.Int64:
                    return Serializer.WriteInt64;
                case TypeSerializerUtils.TypeCode.UInt64Nullable:
                case TypeSerializerUtils.TypeCode.UInt64:
                    return Serializer.WriteUInt64;
                case TypeSerializerUtils.TypeCode.SingleNullable:
                case TypeSerializerUtils.TypeCode.Single:
                    return Serializer.WriteFloat;
                case TypeSerializerUtils.TypeCode.DoubleNullable:
                case TypeSerializerUtils.TypeCode.Double:
                    return Serializer.WriteDouble;
                case TypeSerializerUtils.TypeCode.DateTimeNullable:
                    return Serializer.WriteNullableDateTime;
                case TypeSerializerUtils.TypeCode.DateTime:
                    return Serializer.WriteDateTime;
                case TypeSerializerUtils.TypeCode.DateTimeOffsetNullable:
                    return Serializer.WriteNullableDateTimeOffset;
                case TypeSerializerUtils.TypeCode.DateTimeOffset:
                    return Serializer.WriteDateTimeOffset;
                case TypeSerializerUtils.TypeCode.DecimalNullable:
                case TypeSerializerUtils.TypeCode.Decimal:
                    return Serializer.WriteDecimal;
                case TypeSerializerUtils.TypeCode.GuidNullable:
                    return Serializer.WriteNullableGuid;
                case TypeSerializerUtils.TypeCode.Guid:
                    return Serializer.WriteGuid;
                case TypeSerializerUtils.TypeCode.TimeSpanNullable:
                    return Serializer.WriteNullableTimeSpan;
                case TypeSerializerUtils.TypeCode.TimeSpan:
                    return Serializer.WriteTimeSpan;
                case TypeSerializerUtils.TypeCode.Uri:
                    return Serializer.WriteUri;
                case TypeSerializerUtils.TypeCode.String:
                    return Serializer.WriteString;
                case TypeSerializerUtils.TypeCode.Bytes:
                    return Serializer.WriteBytes;
                case TypeSerializerUtils.TypeCode.DBNull:
                    return Serializer.WriteNull;
                case TypeSerializerUtils.TypeCode.Exception:
                    return Serializer.WriteException;

                default:
                    throw new NotImplementedException();
                    //if (value is IConvertible convertible)
                    //{
                    //    ResolveConvertibleValue(convertible, out typeCode, out value);
                    //    continue;
                    //}
            }
        }

        /// <summary>
        /// Shortcut escape when we're sure value doesn't contain any escaped chars
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public void WriteRawString(TextWriter writer, string value)
        {
            writer.Write(QuoteChar);
            writer.Write(value);
            writer.Write(QuoteChar);
        }

        /// <summary>
        /// Shortcut escape when we're sure value doesn't contain any escaped chars
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public void WriteRawString(TextWriter writer, char[] value)
        {
            writer.Write(QuoteChar);
            writer.Write(value);
            writer.Write(QuoteChar);
        }


        public void WriteString(TextWriter writer, object value)
        {
          //  WriteString1(writer, value);
            WriteString(writer, (string)value);
        }

        public void WriteString(TextWriter writer, string value)
        {
            if (value == null)
            {
                WriteNull(writer, null);
            }
            else if (!value.HasAnyEscapeChars(JSON.Options.EscapeHtmlChars))
            {
                writer.Write(QuoteChar);
                writer.Write(value);
                writer.Write(QuoteChar);
            }
            else
            {
                //force encode.
                writer.Write(StringExtension.GetEncodeString(value));
            }
        }

        public void WriteException(TextWriter writer, object value)
        {
            WriteString(writer, ((Exception)value).Message);
        }

        public void WriteDateTime(TextWriter writer, object oDateTime)
        {
            writer.Write(QuoteChar);
            WriteJsonDate(writer, (DateTime)oDateTime);
            writer.Write(QuoteChar);
        }

        private static readonly long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
        private  static readonly TimeZoneInfo LocalTimeZone = TimeZoneInfo.Local;

        private static void WriteJsonDate(TextWriter writer, DateTime dateTime)
        {
            switch (JSON.Options.DateHandler)
            {
                case DateHandler.ISO8601:
                    writer.Write(dateTime.ToString("o", CultureInfo.InvariantCulture));
                    return;
                case DateHandler.ISO8601DateOnly:
                    writer.Write(dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    return;
                case DateHandler.ISO8601DateTime:
                    writer.Write(dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                    return;
                case DateHandler.RFC1123:
                    writer.Write(dateTime.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture));
                    return;
            }

            char[] offset = null;
            DateTime utcDate = dateTime;
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    offset = new char[5] { '-', '0', '0', '0', '0' };
                }
                else
                {
                    offset = LocalTimeZone.GetUtcOffset(dateTime).ToTimeOffsetString();
                }
                    
                //need to convert to utc time
                utcDate = dateTime.ToUniversalTime();
            }

            writer.Write(@"\/Date(");
            var value = (utcDate.Ticks - DatetimeMinTimeTicks) / 10000;
            writer.Write(value.ToString(CultureInfo.InvariantCulture));
            if (offset != null)
            {
                writer.Write(offset);
            }
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
            writer.Write(QuoteChar);
            writer.Write(((DateTimeOffset)oDateTimeOffset).ToString("o", CultureInfo.InvariantCulture));
            writer.Write(QuoteChar);
        }

        public void WriteNullableDateTimeOffset(TextWriter writer, object dateTimeOffset)
        {
            if (dateTimeOffset == null)
                WriteNull(writer, null);
            else
                WriteDateTimeOffset(writer, dateTimeOffset);
        }

        public void WriteUri(TextWriter writer, object uri)
        {
            if (uri == null)
                WriteNull(writer, null);
            else
                WriteString(writer, ((Uri)uri).OriginalString);
        }


        public void WriteTimeSpan(TextWriter writer, object oTimeSpan)
        {
            writer.Write(QuoteChar);
            writer.Write(((TimeSpan)oTimeSpan).ToString());
            writer.Write(QuoteChar);
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
            writer.Write(QuoteChar);
            writer.Write(((Guid)oValue).ToString("D"));
            writer.Write(QuoteChar);
        }

        public void WriteNullableGuid(TextWriter writer, object oValue)
        {
            if (oValue == null) return;
            WriteGuid(writer, oValue);
        }

        public void WriteBytes(TextWriter writer, object oByteValue)
        {
            if (oByteValue == null) return;
            WriteRawString(writer, Convert.ToBase64String((byte[])oByteValue));
        }

        internal readonly static char[] Null = new char[4] { 'n', 'u', 'l', 'l' };
        public void WriteNull(TextWriter writer, object oNull)
        {
            writer.Write(Null, 0, 4);
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
            {
                WriteNull(writer, null);
            }
            else
            {
                WriteIntegerValue(writer, (int)intValue);
            }
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
                WriteIntegerValue(writer, (ulong)ulongValue);
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
               writer.Write(((float)floatValue).ToString("r", CultureInfo.InvariantCulture));
            }
        }

        public void WriteDouble(TextWriter writer, object doubleValue)
        {
            if (doubleValue == null)
                WriteNull(writer, null);
            else
            {
                    writer.Write(((double)doubleValue).ToString(CultureInfo.InvariantCulture));
            }
        }

        public void WriteDecimal(TextWriter writer, object decimalValue)
        {
            if (decimalValue == null)
                WriteNull(writer, null);
            else
                writer.Write(((decimal)decimalValue).ToString(CultureInfo.InvariantCulture));
        }

        private static void WriteIntegerValue(TextWriter writer, int value)
        {
            if (value >= 0 && value <= 9)
            {
                writer.Write(MathUtils.charNumbers[value]);
            }
            else
            {
                writer.Write(value.ToString());
               // bool negative = value < 0;
                //WriteIntegerValue(writer, negative ? (uint)-value : (uint)value, negative);
            }
        }

        private static void WriteIntegerValue(TextWriter writer,uint value, bool negative)
        {
            if (!negative && value <= 9)
            {
                writer.Write(MathUtils.charNumbers[value]);
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
                writer.Write(MathUtils.charNumbers[value]);
            }
            else
            {
                writer.Write(value.ToString());
                // bool negative = value < 0;
                //WriteIntegerValue(writer, negative ? (ulong)-value : (ulong)value, negative);
            }
        }

        private static void WriteIntegerValue(TextWriter writer, ulong value)
        {
            if (value >= 0 && value <= 9)
            {
                writer.Write(MathUtils.charNumbers[value]);
            }
            else
            {
                writer.Write(value.ToString());
                // bool negative = value < 0;
                //WriteIntegerValue(writer, negative ? (ulong)-value : (ulong)value, negative);
            }
        }

        private static void WriteIntegerValue(TextWriter writer, ulong value, bool negative)
        {
            if (!negative && value <= 9)
            {
                writer.Write(MathUtils.charNumbers[value]);
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
                buffer[--index] = MathUtils.charNumbers[digit];
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

            totalLength = MathUtils.IntLength(value);

            char[] buffer = new char[totalLength+1];

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
                buffer[--index] = MathUtils.charNumbers[digit];
                value = quotient;
            } while (value != 0);

            return buffer;
        }
    }
}
