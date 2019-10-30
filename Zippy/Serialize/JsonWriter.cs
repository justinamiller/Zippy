using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Zippy.Utility;
using static Zippy.Utility.DateTimeExtension;

namespace Zippy.Serialize
{
    sealed class JsonWriter
    {
        public const char QuoteChar = '"';
        public const char OpenArrayChar = '[';
        public const char CloseArrayChar = ']';
        public const char OpenObjectChar = '{';
        public const char CloseObjectChar = '}';
        public const char CommaChar = ',';

        private static readonly long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
        private static readonly TimeZoneInfo LocalTimeZone = TimeZoneInfo.Local;
        private static readonly CultureInfo CurrentCulture = CultureInfo.InvariantCulture;
        private bool _propertyInUse;
        private readonly TextWriter _writer;
        private int _arrayIndex = 0;
        private int _objectIndex = 0;
        private readonly Options _options;

        public JsonWriter(TextWriter writer, Options options)
        {
            _writer = writer;
            _options = options;
        }

        public bool IsValid()
        {
            return _arrayIndex == 0 && _objectIndex == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartObject()
        {
            _writer.Write(OpenObjectChar);
            this._propertyInUse = false;
            _objectIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndObject()
        {
            this._propertyInUse = true;
            _writer.Write(CloseObjectChar);
            _objectIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndArray()
        {
            _writer.Write(CloseArrayChar);
            _arrayIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartArray()
        {
            _writer.Write(OpenArrayChar);
            _arrayIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteComma()
        {
            _writer.Write(CommaChar);
        }

        /// <summary>
        /// prestructure property name with \" & \":
        /// </summary>
        /// <param name="value"></param>
        public void WritePropertyNameFast(string value)
        {
            if (this._propertyInUse)
            {
                _writer.Write(CommaChar);
            }
            else
            {
                this._propertyInUse = true;
            }
            _writer.Write(value);
        }

        public void WritePropertyName(string value, bool encode)
        {
            WritePropertyNameFast(TypeSerializerUtils.BuildPropertyName(value, _options.TextCase, encode));
        }


        public void WriteString(string value)
        {
            //force encode.
            _writer.Write(StringExtension.GetEncodeString(value, _options.EscapeHtmlChars));
        }


        public void WriteStringNullable(string value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            //force encode.
            _writer.Write(StringExtension.GetEncodeString(value, _options.EscapeHtmlChars));
        }


        public void WriteException(object value)
        {
            WriteStringNullable(((Exception)value).Message);
        }

        private void WriteTimeStampOffset(DateTime dateTime)
        {
            _writer.Write(QuoteChar);
            var offset = LocalTimeZone.GetUtcOffset(dateTime).Ticks;
            var ticks = dateTime.ToUniversalTicks(offset);
            long value = unchecked((ticks - DatetimeMinTimeTicks) / 10000);

            _writer.Write(@"\/Date(");
            _writer.Write(value.ToString(CurrentCulture));
            _writer.Write(offset.ToTimeOffsetChars());
            _writer.Write(@")\/");
            _writer.Write(QuoteChar);
        }

        public void WriteDateTime(object oDateTime)
        {
            var dateTime = (DateTime)oDateTime;
            switch (_options.DateHandler)
            {
                case DateHandler.TimestampOffset:
                    WriteTimeStampOffset(dateTime);
                    return;
                case DateHandler.ISO8601:
                    _writer.Write(dateTime.ToString("o", CurrentCulture));
                    return;
                case DateHandler.ISO8601DateOnly:
                    _writer.Write(dateTime.ToString("yyyy-MM-dd", CurrentCulture));
                    return;
                case DateHandler.ISO8601DateTime:
                    _writer.Write(dateTime.ToString("yyyy-MM-dd HH:mm:ss", CurrentCulture));
                    return;
                case DateHandler.RFC1123:
                    _writer.Write(dateTime.ToUniversalTime().ToString("R", CurrentCulture));
                    return;
            }
        }

        public void WriteNullableDateTime(object dateTime)
        {
            if (dateTime == null)
                WriteNull();
            else
                WriteDateTime(dateTime);
        }

        public void WriteDateTimeOffset(object oDateTimeOffset)
        {
            _writer.Write(QuoteChar);
            _writer.Write(((DateTimeOffset)oDateTimeOffset).ToString("o", CurrentCulture));
            _writer.Write(QuoteChar);
        }

        public void WriteNullableDateTimeOffset(object dateTimeOffset)
        {
            if (dateTimeOffset == null)
                WriteNull();
            else
                WriteDateTimeOffset(dateTimeOffset);
        }

        public void WriteUri(object uri)
        {
            if (uri == null)
                WriteNull();
            else
                WriteStringNullable(((Uri)uri).OriginalString);
        }


        public void WriteTimeSpan(object oTimeSpan)
        {
            _writer.Write(QuoteChar);
            _writer.Write(((TimeSpan)oTimeSpan).ToTimeSpanChars());
            _writer.Write(QuoteChar);
        }

        public void WriteNullableTimeSpan(object oTimeSpan)
        {
            if (oTimeSpan == null)
                WriteNull();
            else
                WriteTimeSpan(oTimeSpan);
        }

        public void WriteGuid(object oValue)
        {
            var guid = new FastGuidStruct((Guid)oValue);
            _writer.Write(QuoteChar);
            _writer.Write(guid.GetBuffer(), 0, 36);
            //_writer.Write(((Guid)oValue).ToString("D", CurrentCulture));
            _writer.Write(QuoteChar);
        }


        public void WriteNullableGuid(object oValue)
        {
            if (oValue == null) return;
            WriteGuid(oValue);
        }

        public void WriteBytes(object oByteValue)
        {
            if (oByteValue == null) return;

            _writer.Write(QuoteChar);
            _writer.Write(Convert.ToBase64String((byte[])oByteValue));
            _writer.Write(QuoteChar);
        }

        internal readonly static char[] Null = new char[4] { 'n', 'u', 'l', 'l' };
        public void WriteNull()
        {
            _writer.Write(Null, 0, 4);
        }

        public void WriteChar(object charValue)
        {
            if (charValue == null)
                WriteNull();
            else
            {
                _writer.Write(StringExtension.GetEncodeString(charValue.ToString(), _options.EscapeHtmlChars));
            }
        }

        public void WriteByte(object byteValue)
        {
            if (byteValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (byte)byteValue);
        }

        public void WriteSByte(object sbyteValue)
        {
            if (sbyteValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (sbyte)sbyteValue);
        }

        public void WriteInt16(object intValue)
        {
            if (intValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (short)intValue);
        }

        public void WriteUInt16(object intValue)
        {
            if (intValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (ushort)intValue);
        }

        public void WriteInt32Nullable(object intValue)
        {
            if (intValue == null)
            {
                WriteNull();
            }
            else
            {
                WriteIntegerValue(_writer, (int)intValue);
            }
        }

        public void WriteInt32(object intValue)
        {
             WriteIntegerValue(_writer, (int)intValue);
        }

        public void WriteUInt32(object uintValue)
        {
            if (uintValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (uint)uintValue);
        }

        public void WriteInt64(object integerValue)
        {
              WriteIntegerValue(_writer, (long)integerValue);
        }

        public void WriteInt64Nullable(object integerValue)
        {
            if (integerValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (long)integerValue);
        }

        public void WriteUInt64(object ulongValue)
        {
            if (ulongValue == null)
            {
                WriteNull();
            }
            else
                WriteIntegerValue(_writer, (ulong)ulongValue);
        }

        public void WriteBool(object boolValue)
        {
            if (boolValue == null)
                WriteNull();
            else
                _writer.Write(((bool)boolValue) ? "true" : "false");
        }

        public void WriteFloat(object floatValue)
        {
            if (floatValue == null)
                WriteNull();
            else
            {
                _writer.Write(((float)floatValue).ToString("0.0####################", CurrentCulture));
            }
        }

        public void WriteDouble(object doubleValue)
        {
            _writer.Write(((double)doubleValue).ToString("0.0####################", CurrentCulture));
        }

        public void WriteDoubleNullable(object doubleValue)
        {
            if (doubleValue == null)
                WriteNull();
            else
            {
                _writer.Write(((double)doubleValue).ToString("0.0####################", CurrentCulture));
            }
        }

        public void WriteDecimal(object decimalValue)
        {
            if (decimalValue == null)
                WriteNull();
            else
                _writer.Write(((decimal)decimalValue).ToString("0.0####################", CurrentCulture));
        }

        private void WriteIntegerValue(TextWriter writer, int value)
        {
            if (value >= 0 && value <= 9)
            {
                writer.Write(MathUtils.charNumbers[value]);
            }
            else
            {
                bool negative = value < 0;
                WriteIntegerValue(writer, negative ? (uint)-value : (uint)value, negative);
            }
        }

        private static void WriteIntegerValue(TextWriter writer, uint value, bool negative)
        {
            if (!negative && value <= 9)
            {
                writer.Write(MathUtils.charNumbers[value]);
            }
            else
            {
                var buffer = MathUtils.WriteNumberToBuffer(value, negative);
                writer.Write(buffer);
            }
        }

        private static void WriteIntegerValue(TextWriter writer, long value)
        {
            if (value >= 0 && value <= 9)
            {
                writer.Write(MathUtils.charNumbers[value]);
            }
            else
            {
                bool negative = value < 0;
                WriteIntegerValue(writer, negative ? (ulong)-value : (ulong)value, negative);
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
                WriteIntegerValue(writer, (ulong)value, false);
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
                var buffer = MathUtils.WriteNumberToBuffer(value, negative);
                writer.Write(buffer);
            }
        }

        public delegate void WriteObjectDelegate(JsonWriter writer, object obj);

        public static WriteObjectDelegate GetWriteObjectDelegate(TypeSerializerUtils.TypeCode typeCode)
        {
            if (typeCode >= TypeSerializerUtils.TypeCode.CustomObject)
            {
                return null;
            }

            switch (typeCode)
            {
                case TypeSerializerUtils.TypeCode.CharNullable:
                case TypeSerializerUtils.TypeCode.Char:
                    return WriteChar;
                case TypeSerializerUtils.TypeCode.String:
                    return WriteString;
                case TypeSerializerUtils.TypeCode.BooleanNullable:
                case TypeSerializerUtils.TypeCode.Boolean:
                    return WriteBool;
                case TypeSerializerUtils.TypeCode.SByteNullable:
                case TypeSerializerUtils.TypeCode.SByte:
                    return WriteSByte;
                case TypeSerializerUtils.TypeCode.Int16Nullable:
                case TypeSerializerUtils.TypeCode.Int16:
                    return WriteInt16;
                case TypeSerializerUtils.TypeCode.UInt16Nullable:
                case TypeSerializerUtils.TypeCode.UInt16:
                    return WriteUInt16;
                case TypeSerializerUtils.TypeCode.Int32Nullable:
                case TypeSerializerUtils.TypeCode.Int32:
                    return WriteInt32;
                case TypeSerializerUtils.TypeCode.ByteNullable:
                case TypeSerializerUtils.TypeCode.Byte:
                    return WriteByte;
                case TypeSerializerUtils.TypeCode.UInt32Nullable:
                case TypeSerializerUtils.TypeCode.UInt32:
                    return WriteUInt32;
                case TypeSerializerUtils.TypeCode.Int64Nullable:
                case TypeSerializerUtils.TypeCode.Int64:
                    return WriteInt64;
                case TypeSerializerUtils.TypeCode.UInt64Nullable:
                case TypeSerializerUtils.TypeCode.UInt64:
                    return WriteUInt64;
                case TypeSerializerUtils.TypeCode.SingleNullable:
                case TypeSerializerUtils.TypeCode.Single:
                    return WriteFloat;
                case TypeSerializerUtils.TypeCode.DoubleNullable:
                case TypeSerializerUtils.TypeCode.Double:
                    return WriteDouble;
                case TypeSerializerUtils.TypeCode.DateTimeNullable:
                    return WriteNullableDateTime;
                case TypeSerializerUtils.TypeCode.DateTime:
                    return WriteDateTime;
                case TypeSerializerUtils.TypeCode.DateTimeOffsetNullable:
                    return WriteNullableDateTimeOffset;
                case TypeSerializerUtils.TypeCode.DateTimeOffset:
                    return WriteDateTimeOffset;
                case TypeSerializerUtils.TypeCode.DecimalNullable:
                case TypeSerializerUtils.TypeCode.Decimal:
                    return WriteDecimal;
                case TypeSerializerUtils.TypeCode.GuidNullable:
                    return WriteNullableGuid;
                case TypeSerializerUtils.TypeCode.Guid:
                    return WriteGuid;
                case TypeSerializerUtils.TypeCode.TimeSpanNullable:
                    return WriteNullableTimeSpan;
                case TypeSerializerUtils.TypeCode.TimeSpan:
                    return WriteTimeSpan;

                ////case PrimitiveTypeCode.BigInteger:
                ////    // this will call to WriteValue(object)
                ////    WriteValue((BigInteger)value);
                ////    return;
                case TypeSerializerUtils.TypeCode.Uri:
                    return WriteUri;

                case TypeSerializerUtils.TypeCode.Bytes:
                    return WriteBytes;
                case TypeSerializerUtils.TypeCode.DBNull:
                    return WriteNull;
                case TypeSerializerUtils.TypeCode.Exception:
                    return WriteException;

                default:
                    return null;
                    //if (value is IConvertible convertible)
                    //{
                    //    ResolveConvertibleValue(convertible, out typeCode, out value);
                    //    continue;
                    //}
            }

        }



        public static void WriteString(JsonWriter writer, object value)
        {
            writer.WriteString((string)value);
        }

        public static void WriteChar(JsonWriter writer, object value)
        {
            writer.WriteChar(value);
        }

        public static void WriteInt32(JsonWriter writer, object value)
        {
            writer.WriteInt32(value);
        }

        public static void WriteInt16(JsonWriter writer, object value)
        {
            writer.WriteInt16(value);
        }

        public static void WriteBool(JsonWriter writer, object value)
        {
            writer.WriteBool(value);
        }

        public static void WriteSByte(JsonWriter writer, object value)
        {
            writer.WriteSByte(value);
        }

        public static void WriteUInt16(JsonWriter writer, object value)
        {
            writer.WriteUInt16(value);
        }

        public static void WriteByte(JsonWriter writer, object value)
        {
            writer.WriteByte(value);
        }

        public static void WriteDouble(JsonWriter writer, object value)
        {
            writer.WriteDouble(value);
        }


        public static void WriteDecimal(JsonWriter writer, object value)
        {
            writer.WriteDecimal(value);
        }


        public static void WriteException(JsonWriter writer, object value)
        {
            writer.WriteException(value);
        }

        public static void WriteNullableGuid(JsonWriter writer, object value)
        {
            writer.WriteNullableGuid(value);
        }

        public static void WriteNull(JsonWriter writer, object value)
        {
            writer.WriteNull();
        }
        public static void WriteBytes(JsonWriter writer, object value)
        {
            writer.WriteBytes(value);
        }
        public static void WriteGuid(JsonWriter writer, object value)
        {
            writer.WriteGuid(value);
        }

        public static void WriteInt64(JsonWriter writer, object value)
        {
            writer.WriteInt64(value);
        }
        public static void WriteTimeSpan(JsonWriter writer, object value)
        {
            writer.WriteTimeSpan(value);
        }
        public static void WriteUri(JsonWriter writer, object value)
        {
            writer.WriteUri(value);
        }
        public static void WriteNullableTimeSpan(JsonWriter writer, object value)
        {
            writer.WriteNullableTimeSpan(value);
        }
        public static void WriteUInt32(JsonWriter writer, object value)
        {
            writer.WriteUInt32(value);
        }
        public static void WriteDateTimeOffset(JsonWriter writer, object value)
        {
            writer.WriteDateTimeOffset(value);
        }
        public static void WriteDateTime(JsonWriter writer, object value)
        {
            writer.WriteDateTime(value);
        }
        public static void WriteUInt64(JsonWriter writer, object value)
        {
            writer.WriteUInt64(value);
        }

        public static void WriteFloat(JsonWriter writer, object value)
        {
            writer.WriteFloat(value);
        }

        public static void WriteNullableDateTime(JsonWriter writer, object value)
        {
            writer.WriteNullableDateTime(value);
        }
        public static void WriteNullableDateTimeOffset(JsonWriter writer, object value)
        {
            writer.WriteNullableDateTimeOffset(value);
        }

        public override string ToString()
        {
            return this._writer.ToString();
        }
    }
}
