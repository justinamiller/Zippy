using JsonSerializer.Internal;
using JsonSerializer.Utility;
using System;
using System.Collections.Generic;
using System.IO;
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


        private static readonly JsonTypeSerializer Serializer = new JsonTypeSerializer();

        public static WriteObjectDelegate GetValueTypeToStringMethod(ConvertUtils.TypeCode typeCode)
        {
            if (typeCode >= ConvertUtils.TypeCode.NotSetObject)
            {
                return null;
            }

                while (true)
            {
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
                return  Serializer.WriteDateTime;
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
                        return null;
                    case ConvertUtils.TypeCode.String:
                        return Serializer.WriteString;
                    case ConvertUtils.TypeCode.Bytes:
                        return Serializer.WriteBytes;
                    case ConvertUtils.TypeCode.DBNull:
                        return Serializer.WriteNull;

                    default:
                        return null;
                        //if (value is IConvertible convertible)
                        //{
                        //    ResolveConvertibleValue(convertible, out typeCode, out value);
                        //    continue;
                        //}
                }
            }
        }



    }
}
