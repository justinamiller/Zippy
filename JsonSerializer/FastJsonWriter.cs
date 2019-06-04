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


        private static Dictionary<ConvertUtils.TypeCode, Action<TextWriter, object>> _actions = new Dictionary<ConvertUtils.TypeCode, Action<TextWriter, object>>();


        public static Action<TextWriter, object> GetValueTypeActionMethod(ConvertUtils.TypeCode typeCode, WriteObjectDelegate wod)
        {
            if (typeCode >= ConvertUtils.TypeCode.NotSetObject)
            {
                return null;
            }

            Action<TextWriter, object> action = null;

            if (_actions.TryGetValue(typeCode, out action))
            {
                return action;
            }


            var mi = typeof(JsonTypeSerializer).GetMethod(wod.Method.Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, new[] { typeof(TextWriter), typeof(object) }, null);
            //var actArg = mi.GetParameters()[0];
            //var args = actArg.ParameterType.GenericTypeArguments;

            //var para = mi.GetParameters().Select(a => Expression.Parameter(a.ParameterType))
            //    .ToArray();

            var p1 = Expression.Parameter(typeof(TextWriter), "writer");
            var p2 = Expression.Parameter(typeof(object), "value");
            // var asDateTime = Expression.Call(typeof(JsonTypeSerializer), "WriteDateTime", null, p1,p2); // calls static method "DateTime.Parse"

            var instance = Expression.Constant(Serializer);
            var asDateTime = Expression.Call(instance, mi, p1, p2); // calls static method "DateTime.Parse"

            var lamba = Expression.Lambda<Action<TextWriter, object>>(asDateTime, wod.Method.Name, new ParameterExpression[] { p1, p2 });
            action = lamba.Compile();
            _actions.Add(typeCode, action);

            return action;
        }

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
