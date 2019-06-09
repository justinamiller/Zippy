using JsonSerializer.Internal;
using JsonSerializer.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace JsonSerializer
{
    public delegate void WriteObjectDelegate(TextWriter writer, object obj);

    class FastJsonWriter
    {
        public const char MapStartChar = '{';
        public const char MapKeySeperator = ':';
        public const char ItemSeperator = ',';
        public const char MapEndChar = '}';
        public const string MapNullValue = "\"\"";
        public const string EmptyMap = "{}";

        public const char ListStartChar = '[';
        public const char ListEndChar = ']';
        public const char ReturnChar = '\r';
        public const char LineFeedChar = '\n';

        public const char QuoteChar = '"';
        public const string QuoteString = "\"";
        public const string EscapedQuoteString = "\\\"";
        public const string ItemSeperatorString = ",";
        public const string MapKeySeperatorString = ":";

        internal static readonly JsonTypeSerializer Serializer = new JsonTypeSerializer();

        public static WriteObjectDelegate GetValueTypeToStringMethod(ConvertUtils.TypeCode typeCode)
        {
            if (typeCode >= ConvertUtils.TypeCode.NotSetObject)
            {
                return null;
            }

            switch (typeCode)
            {
                case ConvertUtils.TypeCode.CharNullable:
                case ConvertUtils.TypeCode.Char:
                    return Serializer.WriteChar;
                case ConvertUtils.TypeCode.BooleanNullable:
                case ConvertUtils.TypeCode.Boolean:
                    return Serializer.WriteBool;
                case ConvertUtils.TypeCode.SByteNullable:
                case ConvertUtils.TypeCode.SByte:
                    return Serializer.WriteSByte;
                case ConvertUtils.TypeCode.Int16Nullable:
                case ConvertUtils.TypeCode.Int16:
                    return Serializer.WriteInt16;
                case ConvertUtils.TypeCode.UInt16Nullable:
                case ConvertUtils.TypeCode.UInt16:
                    return Serializer.WriteUInt16;
                case ConvertUtils.TypeCode.Int32Nullable:
                case ConvertUtils.TypeCode.Int32:
                    return Serializer.WriteInt32;
                case ConvertUtils.TypeCode.ByteNullable:
                case ConvertUtils.TypeCode.Byte:
                    return Serializer.WriteByte;
                case ConvertUtils.TypeCode.UInt32Nullable:
                case ConvertUtils.TypeCode.UInt32:
                    return Serializer.WriteUInt32;
                case ConvertUtils.TypeCode.Int64Nullable:
                case ConvertUtils.TypeCode.Int64:
                    return Serializer.WriteInt64;
                case ConvertUtils.TypeCode.UInt64Nullable:
                case ConvertUtils.TypeCode.UInt64:
                    return Serializer.WriteUInt64;
                case ConvertUtils.TypeCode.SingleNullable:
                case ConvertUtils.TypeCode.Single:
                    return Serializer.WriteFloat;
                case ConvertUtils.TypeCode.DoubleNullable:
                case ConvertUtils.TypeCode.Double:
                    return Serializer.WriteDouble;
                case ConvertUtils.TypeCode.DateTimeNullable:
                    return Serializer.WriteNullableDateTime;
                case ConvertUtils.TypeCode.DateTime:
                    return Serializer.WriteDateTime;
                case ConvertUtils.TypeCode.DateTimeOffsetNullable:
                    return Serializer.WriteNullableDateTimeOffset;
                case ConvertUtils.TypeCode.DateTimeOffset:
                    return Serializer.WriteDateTimeOffset;
                case ConvertUtils.TypeCode.DecimalNullable:
                case ConvertUtils.TypeCode.Decimal:
                    return Serializer.WriteDecimal;
                case ConvertUtils.TypeCode.GuidNullable:
                case ConvertUtils.TypeCode.Guid:
                    return Serializer.WriteGuid;
                case ConvertUtils.TypeCode.TimeSpanNullable:
                case ConvertUtils.TypeCode.TimeSpan:
                    return Serializer.WriteTimeSpan;

                //case PrimitiveTypeCode.BigInteger:
                //    // this will call to WriteValue(object)
                //    WriteValue((BigInteger)value);
                //    return;
                case ConvertUtils.TypeCode.Uri:
                    return Serializer.WriteUri;
                case ConvertUtils.TypeCode.String:
                    return Serializer.WriteString;
                case ConvertUtils.TypeCode.Bytes:
                    return Serializer.WriteBytes;
                case ConvertUtils.TypeCode.DBNull:
                    return Serializer.WriteNull;

                default:
                    throw new NotImplementedException();
                    //if (value is IConvertible convertible)
                    //{
                    //    ResolveConvertibleValue(convertible, out typeCode, out value);
                    //    continue;
                    //}
            }
        }

        public static bool WriteToStringMethod(ConvertUtils.TypeCode typeCode, object value, TextWriter writer)
        {
            if (typeCode >= ConvertUtils.TypeCode.NotSetObject)
            {
                return false;
            }

            switch (typeCode)
            {

                case ConvertUtils.TypeCode.CharNullable:
                case ConvertUtils.TypeCode.Char:
                    {
                        Serializer.WriteChar(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.BooleanNullable:
                case ConvertUtils.TypeCode.Boolean:
                    {
                        Serializer.WriteBool(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.SByteNullable:
                case ConvertUtils.TypeCode.SByte:
                    {
                        Serializer.WriteSByte(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.Int16Nullable:
                case ConvertUtils.TypeCode.Int16:
                    {
                        Serializer.WriteInt16(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.UInt16Nullable:
                case ConvertUtils.TypeCode.UInt16:
                    {
                        Serializer.WriteUInt16(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.Int32Nullable:
                case ConvertUtils.TypeCode.Int32:
                    {
                        Serializer.WriteInt32(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.ByteNullable:
                case ConvertUtils.TypeCode.Byte:
                    {
                        Serializer.WriteByte(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.UInt32Nullable:
                case ConvertUtils.TypeCode.UInt32:
                    {
                        Serializer.WriteUInt32(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.Int64Nullable:
                case ConvertUtils.TypeCode.Int64:
                    {
                        Serializer.WriteInt64(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.UInt64Nullable:
                case ConvertUtils.TypeCode.UInt64:
                    {
                        Serializer.WriteUInt64(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.SingleNullable:
                case ConvertUtils.TypeCode.Single:
                    {
                        Serializer.WriteFloat(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.DoubleNullable:
                case ConvertUtils.TypeCode.Double:
                    {
                        Serializer.WriteDouble(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.DateTimeNullable:
                    {
                        Serializer.WriteNullableDateTime(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.DateTime:
                    {
                        Serializer.WriteDateTime(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.DateTimeOffsetNullable:
                    {
                        Serializer.WriteNullableDateTimeOffset(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.DateTimeOffset:
                    {
                        Serializer.WriteDateTimeOffset(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.DecimalNullable:
                case ConvertUtils.TypeCode.Decimal:
                    {
                        Serializer.WriteDecimal(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.GuidNullable:
                case ConvertUtils.TypeCode.Guid:
                    {
                        Serializer.WriteGuid(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.TimeSpanNullable:
                case ConvertUtils.TypeCode.TimeSpan:
                    {
                        Serializer.WriteTimeSpan(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.Uri:
                    {
                        Serializer.WriteUri(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.String:
                    {
                        Serializer.WriteString(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.Bytes:
                    {
                        Serializer.WriteBytes(writer, value);
                        return true;
                    }
                case ConvertUtils.TypeCode.DBNull:
                    {
                        Serializer.WriteNull(writer, value);
                        return true;
                    }

                default:
                    throw new NotImplementedException();
                    //if (value is IConvertible convertible)
                    //{
                    //    ResolveConvertibleValue(convertible, out typeCode, out value);
                    //    continue;
                    //}
            }
        }

    }
}
