using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace JsonSerializer.Utility
{
    class ConvertUtils
    {
        internal enum PrimitiveTypeCode
        {
            Empty = 0,
            Object = 1,
            Char = 2,
            CharNullable = 3,
            Boolean = 4,
            BooleanNullable = 5,
            SByte = 6,
            SByteNullable = 7,
            Int16 = 8,
            Int16Nullable = 9,
            UInt16 = 10,
            UInt16Nullable = 11,
            Int32 = 12,
            Int32Nullable = 13,
            Byte = 14,
            ByteNullable = 15,
            UInt32 = 16,
            UInt32Nullable = 17,
            Int64 = 18,
            Int64Nullable = 19,
            UInt64 = 20,
            UInt64Nullable = 21,
            Single = 22,
            SingleNullable = 23,
            Double = 24,
            DoubleNullable = 25,
            DateTime = 26,
            DateTimeNullable = 27,
            DateTimeOffset = 28,
            DateTimeOffsetNullable = 29,
            Decimal = 30,
            DecimalNullable = 31,
            Guid = 32,
            GuidNullable = 33,
            TimeSpan = 34,
            TimeSpanNullable = 35,
            BigInteger = 36,
            BigIntegerNullable = 37,
            Uri = 38,
            String = 39,
            Bytes = 40,
            DBNull = 41
        }

        internal enum ObjectTypeCode
        {
            Empty = 0,
            Custom = 1,
            DataTable=2,
            DataSet = 3,
            Dictionary = 4,
            NameValueCollection = 5,
             Enumerable=6
        }

        private static readonly Dictionary<Type, PrimitiveTypeCode> TypeCodeMap =
    new Dictionary<Type, PrimitiveTypeCode>
    {
                { typeof(char), PrimitiveTypeCode.Char },
                { typeof(char?), PrimitiveTypeCode.CharNullable },
                { typeof(bool), PrimitiveTypeCode.Boolean },
                { typeof(bool?), PrimitiveTypeCode.BooleanNullable },
                { typeof(sbyte), PrimitiveTypeCode.SByte },
                { typeof(sbyte?), PrimitiveTypeCode.SByteNullable },
                { typeof(short), PrimitiveTypeCode.Int16 },
                { typeof(short?), PrimitiveTypeCode.Int16Nullable },
                { typeof(ushort), PrimitiveTypeCode.UInt16 },
                { typeof(ushort?), PrimitiveTypeCode.UInt16Nullable },
                { typeof(int), PrimitiveTypeCode.Int32 },
                { typeof(int?), PrimitiveTypeCode.Int32Nullable },
                { typeof(byte), PrimitiveTypeCode.Byte },
                { typeof(byte?), PrimitiveTypeCode.ByteNullable },
                { typeof(uint), PrimitiveTypeCode.UInt32 },
                { typeof(uint?), PrimitiveTypeCode.UInt32Nullable },
                { typeof(long), PrimitiveTypeCode.Int64 },
                { typeof(long?), PrimitiveTypeCode.Int64Nullable },
                { typeof(ulong), PrimitiveTypeCode.UInt64 },
                { typeof(ulong?), PrimitiveTypeCode.UInt64Nullable },
                { typeof(float), PrimitiveTypeCode.Single },
                { typeof(float?), PrimitiveTypeCode.SingleNullable },
                { typeof(double), PrimitiveTypeCode.Double },
                { typeof(double?), PrimitiveTypeCode.DoubleNullable },
                { typeof(DateTime), PrimitiveTypeCode.DateTime },
                { typeof(DateTime?), PrimitiveTypeCode.DateTimeNullable },
                { typeof(DateTimeOffset), PrimitiveTypeCode.DateTimeOffset },
                { typeof(DateTimeOffset?), PrimitiveTypeCode.DateTimeOffsetNullable },
                { typeof(decimal), PrimitiveTypeCode.Decimal },
                { typeof(decimal?), PrimitiveTypeCode.DecimalNullable },
                { typeof(Guid), PrimitiveTypeCode.Guid },
                { typeof(Guid?), PrimitiveTypeCode.GuidNullable },
                { typeof(TimeSpan), PrimitiveTypeCode.TimeSpan },
                { typeof(TimeSpan?), PrimitiveTypeCode.TimeSpanNullable },
                { typeof(BigInteger), PrimitiveTypeCode.BigInteger },
                { typeof(BigInteger?), PrimitiveTypeCode.BigIntegerNullable },
                { typeof(Uri), PrimitiveTypeCode.Uri },
                { typeof(string), PrimitiveTypeCode.String },
                { typeof(byte[]), PrimitiveTypeCode.Bytes },
                { typeof(DBNull), PrimitiveTypeCode.DBNull }
    };

        private static readonly Dictionary<Type, ObjectTypeCode> ObjectTypeCodeMap =
new Dictionary<Type, ObjectTypeCode>
{
                { typeof(System.Data.DataTable), ObjectTypeCode.DataTable },
                { typeof(System.Data.DataSet), ObjectTypeCode.DataSet },
                { typeof(System.Collections.IDictionary), ObjectTypeCode.Dictionary },
                { typeof(System.Collections.Specialized.NameValueCollection), ObjectTypeCode.NameValueCollection },
                { typeof(Array), ObjectTypeCode.Enumerable }
};

        internal class TypeInformation
        {
            public Type Type { get; set; }
            public PrimitiveTypeCode TypeCode { get; set; }
        }

        private static readonly TypeInformation[] PrimitiveTypeCodes =
{
            // need all of these. lookup against the index with TypeCode value
            new TypeInformation { Type = typeof(object), TypeCode = PrimitiveTypeCode.Empty },
            new TypeInformation { Type = typeof(object), TypeCode = PrimitiveTypeCode.Object },
            new TypeInformation { Type = typeof(object), TypeCode = PrimitiveTypeCode.DBNull },
            new TypeInformation { Type = typeof(bool), TypeCode = PrimitiveTypeCode.Boolean },
            new TypeInformation { Type = typeof(char), TypeCode = PrimitiveTypeCode.Char },
            new TypeInformation { Type = typeof(sbyte), TypeCode = PrimitiveTypeCode.SByte },
            new TypeInformation { Type = typeof(byte), TypeCode = PrimitiveTypeCode.Byte },
            new TypeInformation { Type = typeof(short), TypeCode = PrimitiveTypeCode.Int16 },
            new TypeInformation { Type = typeof(ushort), TypeCode = PrimitiveTypeCode.UInt16 },
            new TypeInformation { Type = typeof(int), TypeCode = PrimitiveTypeCode.Int32 },
            new TypeInformation { Type = typeof(uint), TypeCode = PrimitiveTypeCode.UInt32 },
            new TypeInformation { Type = typeof(long), TypeCode = PrimitiveTypeCode.Int64 },
            new TypeInformation { Type = typeof(ulong), TypeCode = PrimitiveTypeCode.UInt64 },
            new TypeInformation { Type = typeof(float), TypeCode = PrimitiveTypeCode.Single },
            new TypeInformation { Type = typeof(double), TypeCode = PrimitiveTypeCode.Double },
            new TypeInformation { Type = typeof(decimal), TypeCode = PrimitiveTypeCode.Decimal },
            new TypeInformation { Type = typeof(DateTime), TypeCode = PrimitiveTypeCode.DateTime },
            new TypeInformation { Type = typeof(object), TypeCode = PrimitiveTypeCode.Empty }, // no 17 in TypeCode for some reason
            new TypeInformation { Type = typeof(string), TypeCode = PrimitiveTypeCode.String }
        };

        public static ObjectTypeCode GetObjectTypeCode(Type t)
        {
            if (ObjectTypeCodeMap.TryGetValue(t, out ObjectTypeCode typeCode))
            {
                return typeCode;
            }
            var baseType = t.BaseType;
            if (baseType!=null && baseType != typeof(object))
            {
                return GetObjectTypeCode(t.BaseType);
            }


            return ObjectTypeCode.Empty;
        }


        public static ObjectTypeCode GetInstanceObjectTypeCode(object value)
        {
            if (value is System.Collections.IEnumerable)
            {
                if (value is System.Collections.IDictionary)
                {
                    return ObjectTypeCode.Dictionary;
                }

                if (value is System.Collections.Specialized.NameValueCollection)
                {
                    return ObjectTypeCode.NameValueCollection;
                }

                return ObjectTypeCode.Enumerable;

            }//IEnumerable

            if (value is System.ComponentModel.IListSource)
            {
                if (value is System.Data.DataSet)
                    return ObjectTypeCode.DataSet;
                if (value is System.Data.DataTable)
                    return ObjectTypeCode.DataTable;
            }

            return ObjectTypeCode.Custom;
        }


        public static PrimitiveTypeCode GetTypeCode(Type t)
        {
            if (TypeCodeMap.TryGetValue(t, out PrimitiveTypeCode typeCode))
            {
                return typeCode;
            }

            if (t.IsEnum)
            {
                return GetTypeCode(Enum.GetUnderlyingType(t));
            }

            return PrimitiveTypeCode.Object;
        }

        public static bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static void ResolveConvertibleValue(IConvertible convertible, out PrimitiveTypeCode typeCode, out object value)
        {
            // the value is a non-standard IConvertible
            // convert to the underlying value and retry
            TypeInformation typeInformation = PrimitiveTypeCodes[(int)convertible.GetTypeCode()];

            // if convertible has an underlying typecode of Object then attempt to convert it to a string
            typeCode = typeInformation.TypeCode == PrimitiveTypeCode.Object ? PrimitiveTypeCode.String : typeInformation.TypeCode;
            Type resolvedType = typeInformation.TypeCode == PrimitiveTypeCode.Object ? typeof(string) : typeInformation.Type;
            value = convertible.ToType(resolvedType, CultureInfo.InvariantCulture);
        }

        public static PrimitiveTypeCode GetEnumerableValueTypeCode(System.Collections.IEnumerable anEnumerable)
        {
            if(anEnumerable is Array)
            {
                return GetTypeCode(anEnumerable.GetType().GetElementType());
            }
            else if(anEnumerable is System.Collections.ArrayList)
            {
                return PrimitiveTypeCode.Object;
            }
            else if(anEnumerable is System.Collections.IList)
            {
                Type type =anEnumerable.GetType();
                if (type.IsGenericType)
                {
                    return (GetTypeCode(type.GetGenericArguments()[0]));
                }
            }

             return PrimitiveTypeCode.Object;
        }
    }
}
