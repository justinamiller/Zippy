using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
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

        private static readonly long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
        private static readonly TimeZoneInfo LocalTimeZone = TimeZoneInfo.Local;
        internal static readonly CultureInfo CurrentCulture = CultureInfo.InvariantCulture;
        private bool _propertyInUse;
        private readonly TextWriter _writer;

        public JsonWriter(TextWriter writer)
        {
            _writer = writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartObject()
        {
            _writer.Write(OpenObjectChar);
            this._propertyInUse = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndObject()
        {
            this._propertyInUse = true;
            _writer.Write(CloseObjectChar);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndArray()
        {
            _writer.Write(CloseArrayChar);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartArray()
        {
            _writer.Write(OpenArrayChar);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WritePropertySeperator()
        {
            _writer.Write(':');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteComma()
        {
            _writer.Write(',');
        }

        public void WritePropertyNameFast(string value)
        {
            if (this._propertyInUse)
            {
                _writer.Write(',');
            }
            else
            {
                this._propertyInUse = true;
            }
           _writer.Write(value);
        }

        public void WritePropertyName(string value)
        {
            if (this._propertyInUse)
            {
                _writer.Write(',');
            }
            else
            {
                this._propertyInUse = true;
            }

            value = TypeSerializerUtils.FormatPropertyName(value);
            _writer.Write(StringExtension.GetEncodeString(value));
            _writer.Write(':');
        }

        public bool WriteValueTypeToStringMethod(TypeSerializerUtils.TypeCode typeCode, object value)
        {
            switch (typeCode)
            {
                case TypeSerializerUtils.TypeCode.String:
                    WriteString((string)value);
                    break;
                case TypeSerializerUtils.TypeCode.CharNullable:
                case TypeSerializerUtils.TypeCode.Char:
                    WriteChar(value);
                    break;
                case TypeSerializerUtils.TypeCode.BooleanNullable:
                case TypeSerializerUtils.TypeCode.Boolean:
                    WriteBool(value);
                    break;
                case TypeSerializerUtils.TypeCode.Int32Nullable:
                case TypeSerializerUtils.TypeCode.Int32:
                    WriteInt32(value);
                    break;
                case TypeSerializerUtils.TypeCode.DateTime:
                    WriteDateTime(value);
                    break;
                case TypeSerializerUtils.TypeCode.Guid:
                    WriteGuid(value);
                    break;
                case TypeSerializerUtils.TypeCode.DoubleNullable:
                case TypeSerializerUtils.TypeCode.Double:
                    WriteDouble(value);
                    break;
                case TypeSerializerUtils.TypeCode.SByteNullable:
                case TypeSerializerUtils.TypeCode.SByte:
                    WriteSByte(value);
                    break;
                case TypeSerializerUtils.TypeCode.Int16Nullable:
                case TypeSerializerUtils.TypeCode.Int16:
                    WriteInt16(value);
                    break;
                case TypeSerializerUtils.TypeCode.UInt16Nullable:
                case TypeSerializerUtils.TypeCode.UInt16:
                    WriteUInt16(value);
                    break;
                case TypeSerializerUtils.TypeCode.ByteNullable:
                case TypeSerializerUtils.TypeCode.Byte:
                    WriteByte(value);
                    break;
                case TypeSerializerUtils.TypeCode.UInt32Nullable:
                case TypeSerializerUtils.TypeCode.UInt32:
                    WriteUInt32(value);
                    break;
                case TypeSerializerUtils.TypeCode.Int64Nullable:
                case TypeSerializerUtils.TypeCode.Int64:
                    WriteInt64(value);
                    break;
                case TypeSerializerUtils.TypeCode.UInt64Nullable:
                case TypeSerializerUtils.TypeCode.UInt64:
                    WriteUInt64(value);
                    break;
                case TypeSerializerUtils.TypeCode.SingleNullable:
                case TypeSerializerUtils.TypeCode.Single:
                    WriteFloat(value);
                    break;
                case TypeSerializerUtils.TypeCode.DateTimeNullable:
                    WriteNullableDateTime(value);
                    break;
                case TypeSerializerUtils.TypeCode.DateTimeOffsetNullable:
                    WriteNullableDateTimeOffset(value);
                    break;
                case TypeSerializerUtils.TypeCode.DateTimeOffset:
                    WriteDateTimeOffset(value);
                    break;
                case TypeSerializerUtils.TypeCode.DecimalNullable:
                case TypeSerializerUtils.TypeCode.Decimal:
                    WriteDecimal(value);
                    break;
                case TypeSerializerUtils.TypeCode.GuidNullable:
                    WriteNullableGuid(value);
                    break;
                case TypeSerializerUtils.TypeCode.TimeSpanNullable:
                    WriteNullableTimeSpan(value);
                    break;
                case TypeSerializerUtils.TypeCode.TimeSpan:
                    WriteTimeSpan(value);
                    break;
                case TypeSerializerUtils.TypeCode.Uri:
                    WriteUri(value);
                    break;
                case TypeSerializerUtils.TypeCode.Bytes:
                    WriteBytes(value);
                    break;
                case TypeSerializerUtils.TypeCode.DBNull:
                    WriteNull();
                    break;
                case TypeSerializerUtils.TypeCode.Exception:
                    WriteException(value);
                    break;
                default:
                    throw new NotImplementedException();
                    //if (value is IConvertible convertible)
                    //{
                    //    ResolveConvertibleValue(convertible, out typeCode, out value);
                    //    continue;
                    //}
            }

            return true;
        }

        /// <summary>
        /// Shortcut escape when we're sure value doesn't contain any escaped chars
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public void WriteRawString(string value)
        {
            _writer.Write(QuoteChar);
            _writer.Write(value);
            _writer.Write(QuoteChar);
        }

        /// <summary>
        /// Shortcut escape when we're sure value doesn't contain any escaped chars
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public void WriteRawString( char[] value)
        {
            _writer.Write(QuoteChar);
            _writer.Write(value);
            _writer.Write(QuoteChar);
        }

        public void WriteString( string value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else if (!value.HasAnyEscapeChars(JSON.Options.EscapeHtmlChars))
            {
                _writer.Write(QuoteChar);
                _writer.Write(value);
                _writer.Write(QuoteChar);
            }
            else
            {
                //force encode.
                _writer.Write(StringExtension.GetEncodeString(value));
            }
        }

        public void WriteException( object value)
        {
            WriteString( ((Exception)value).Message);
        }

        public void WriteDateTime( object oDateTime)
        {
            _writer.Write(QuoteChar);
            WriteJsonDate(_writer,(DateTime)oDateTime);
            _writer.Write(QuoteChar);
        }

        private  static void WriteJsonDate(TextWriter writer, DateTime dateTime)
        {
            switch (JSON.Options.DateHandler)
            {
                case DateHandler.ISO8601:
                    writer.Write(dateTime.ToString("o", CurrentCulture));
                    return;
                case DateHandler.ISO8601DateOnly:
                    writer.Write(dateTime.ToString("yyyy-MM-dd", CurrentCulture));
                    return;
                case DateHandler.ISO8601DateTime:
                    writer.Write(dateTime.ToString("yyyy-MM-dd HH:mm:ss", CurrentCulture));
                    return;
                case DateHandler.RFC1123:
                    writer.Write(dateTime.ToUniversalTime().ToString("R", CurrentCulture));
                    return;
            }

            char[] offset = null;
            DateTime utcDate = dateTime;
            var kind = dateTime.Kind;
            if (kind != DateTimeKind.Utc)
            {
                if (kind == DateTimeKind.Unspecified)
                {
                    offset = new char[5] { '-', '0', '0', '0', '0' };
                }
                else
                {
                    offset = LocalTimeZone.GetUtcOffset(dateTime).ToTimeOffsetString();
                }

               // need to convert to utc time
                utcDate = dateTime.ToUniversalTime();
            }
            writer.Write(@"\/Date(");
       //     _writer.Write(_datePrefix, 0, 7);
            var value = (utcDate.Ticks - DatetimeMinTimeTicks) / 10000;
            writer.Write(value.ToString(CurrentCulture));
            if (offset != null)
            {
                writer.Write(offset);
            }
            writer.Write(@")\/");
          //  _writer.Write(_dateSuffix,0,3);
        }

        //private static readonly char[] _datePrefix= new char[7] { '\\', '/', 'D', 'a', 't', 'e', '(' };
        //private static readonly char[] _dateSuffix = new char[3] { ')', '\\','/' };

        public void WriteNullableDateTime( object dateTime)
        {
            if (dateTime == null)
                WriteNull();
            else
                WriteDateTime( dateTime);
        }

        public void WriteDateTimeOffset( object oDateTimeOffset)
        {
            _writer.Write(QuoteChar);
            _writer.Write(((DateTimeOffset)oDateTimeOffset).ToString("o", CurrentCulture));
            _writer.Write(QuoteChar);
        }

        public void WriteNullableDateTimeOffset( object dateTimeOffset)
        {
            if (dateTimeOffset == null)
                WriteNull();
            else
                WriteDateTimeOffset( dateTimeOffset);
        }

        public void WriteUri( object uri)
        {
            if (uri == null)
                WriteNull();
            else
                WriteString( ((Uri)uri).OriginalString);
        }


        public void WriteTimeSpan( object oTimeSpan)
        {
            _writer.Write(QuoteChar);
            // _writer.Write(((TimeSpan)oTimeSpan).ToString());
            _writer.Write(((TimeSpan)oTimeSpan).ToTimeSpanChars());

            _writer.Write(QuoteChar);
        }

        public void WriteNullableTimeSpan( object oTimeSpan)
        {
            if (oTimeSpan == null)
                WriteNull();
            else
                WriteTimeSpan(oTimeSpan);
        }

        public void WriteGuid( object oValue)
        {
            _writer.Write(QuoteChar);
            _writer.Write(((Guid)oValue).ToString("D", CurrentCulture));
            _writer.Write(QuoteChar);
        }

        public void WriteNullableGuid( object oValue)
        {
            if (oValue == null) return;
            WriteGuid(oValue);
        }

        public void WriteBytes( object oByteValue)
        {
            if (oByteValue == null) return;
            WriteRawString(Convert.ToBase64String((byte[])oByteValue));
        }

        internal readonly static char[] Null = new char[4] { 'n', 'u', 'l', 'l' };
        public void WriteNull( )
        {
            _writer.Write(Null, 0, 4);
        }

        public void WriteChar( object charValue)
        {
            if (charValue == null)
                WriteNull();
            else
                WriteString(((char)charValue).ToString());
        }

        public void WriteByte( object byteValue)
        {
            if (byteValue == null)
                WriteNull();
            else
                WriteIntegerValue (_writer, (byte)byteValue);
        }

        public void WriteSByte( object sbyteValue)
        {
            if (sbyteValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (sbyte)sbyteValue);
        }

        public void WriteInt16( object intValue)
        {
            if (intValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (short)intValue);
        }

        public void WriteUInt16( object intValue)
        {
            if (intValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (ushort)intValue);
        }

        public void WriteInt32( object intValue)
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

        public void WriteUInt32( object uintValue)
        {
            if (uintValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (uint)uintValue);
        }

        public void WriteInt64( object integerValue)
        {
            if (integerValue == null)
                WriteNull();
            else
                WriteIntegerValue(_writer, (long)integerValue);
        }

        public void WriteUInt64( object ulongValue)
        {
            if (ulongValue == null)
            {
                WriteNull();
            }
            else
                WriteIntegerValue(_writer, (ulong)ulongValue);
        }

        public void WriteBool( object boolValue)
        {
            if (boolValue == null)
                WriteNull();
            else
                _writer.Write(((bool)boolValue) ? "true" : "false");
        }

        public void WriteFloat( object floatValue)
        {
            if (floatValue == null)
                WriteNull();
            else
            {
                _writer.Write(((float)floatValue).ToString("r", CurrentCulture));
            }
        }

        public void WriteDouble( object doubleValue)
        {
            if (doubleValue == null)
                WriteNull();
            else
            {
                _writer.Write(((double)doubleValue).ToString(CurrentCulture));
            }
        }

        public void WriteDecimal( object decimalValue)
        {
            if (decimalValue == null)
                WriteNull();
            else
                _writer.Write(((decimal)decimalValue).ToString(CurrentCulture));
        }

        private void WriteIntegerValue(TextWriter writer, int value)
        {
            if (value >= 0 && value <= 9)
            {
                writer.Write(MathUtils.charNumbers[value]);
            }
            else
            {
             //  writer.Write(value.ToString());
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
                //writer.Write(value.ToString());
                bool negative = value < 0;
                WriteIntegerValue(writer,negative ? (ulong)-value : (ulong)value, negative);
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
                //    writer.Write(value.ToString());

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
    }
}
