using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace JsonSerializer.Utility
{
    class ConvertUtils
    {
        internal enum TypeCode
        {
            Empty = 0,
       //Object = 1,
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
            DBNull = 41,
            NotSetObject=100,
            DataTable = 101,
            DataSet = 102,
            Dictionary = 103,
            NameValueCollection = 104,
            Enumerable = 105,
            IJsonSerializeImplementation = 106,
           Custom = 200
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

        private static readonly Dictionary<Type, TypeCode> TypeCodeMap =
    new Dictionary<Type, TypeCode>
    {
                { typeof(char), TypeCode.Char },
                { typeof(char?), TypeCode.CharNullable },
                { typeof(bool), TypeCode.Boolean },
                { typeof(bool?), TypeCode.BooleanNullable },
                { typeof(sbyte), TypeCode.SByte },
                { typeof(sbyte?), TypeCode.SByteNullable },
                { typeof(short), TypeCode.Int16 },
                { typeof(short?), TypeCode.Int16Nullable },
                { typeof(ushort), TypeCode.UInt16 },
                { typeof(ushort?), TypeCode.UInt16Nullable },
                { typeof(int), TypeCode.Int32 },
                { typeof(int?), TypeCode.Int32Nullable },
                { typeof(byte), TypeCode.Byte },
                { typeof(byte?), TypeCode.ByteNullable },
                { typeof(uint), TypeCode.UInt32 },
                { typeof(uint?), TypeCode.UInt32Nullable },
                { typeof(long), TypeCode.Int64 },
                { typeof(long?), TypeCode.Int64Nullable },
                { typeof(ulong), TypeCode.UInt64 },
                { typeof(ulong?), TypeCode.UInt64Nullable },
                { typeof(float), TypeCode.Single },
                { typeof(float?), TypeCode.SingleNullable },
                { typeof(double), TypeCode.Double },
                { typeof(double?), TypeCode.DoubleNullable },
                { typeof(DateTime), TypeCode.DateTime },
                { typeof(DateTime?), TypeCode.DateTimeNullable },
                { typeof(DateTimeOffset), TypeCode.DateTimeOffset },
                { typeof(DateTimeOffset?), TypeCode.DateTimeOffsetNullable },
                { typeof(decimal), TypeCode.Decimal },
                { typeof(decimal?), TypeCode.DecimalNullable },
                { typeof(Guid), TypeCode.Guid },
                { typeof(Guid?), TypeCode.GuidNullable },
                { typeof(TimeSpan), TypeCode.TimeSpan },
                { typeof(TimeSpan?), TypeCode.TimeSpanNullable },
                { typeof(BigInteger), TypeCode.BigInteger },
                { typeof(BigInteger?), TypeCode.BigIntegerNullable },
                { typeof(Uri), TypeCode.Uri },
                { typeof(string), TypeCode.String },
                { typeof(byte[]), TypeCode.Bytes },
                { typeof(DBNull), TypeCode.DBNull }
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
            public TypeCode TypeCode { get; set; }
        }

        private static readonly TypeInformation[] PrimitiveTypeCodes =
{
            // need all of these. lookup against the index with TypeCode value
            new TypeInformation { Type = typeof(object), TypeCode = TypeCode.Empty },
            new TypeInformation { Type = typeof(object), TypeCode = TypeCode.NotSetObject },
            new TypeInformation { Type = typeof(object), TypeCode = TypeCode.DBNull },
            new TypeInformation { Type = typeof(bool), TypeCode = TypeCode.Boolean },
            new TypeInformation { Type = typeof(char), TypeCode = TypeCode.Char },
            new TypeInformation { Type = typeof(sbyte), TypeCode = TypeCode.SByte },
            new TypeInformation { Type = typeof(byte), TypeCode = TypeCode.Byte },
            new TypeInformation { Type = typeof(short), TypeCode = TypeCode.Int16 },
            new TypeInformation { Type = typeof(ushort), TypeCode = TypeCode.UInt16 },
            new TypeInformation { Type = typeof(int), TypeCode = TypeCode.Int32 },
            new TypeInformation { Type = typeof(uint), TypeCode = TypeCode.UInt32 },
            new TypeInformation { Type = typeof(long), TypeCode = TypeCode.Int64 },
            new TypeInformation { Type = typeof(ulong), TypeCode = TypeCode.UInt64 },
            new TypeInformation { Type = typeof(float), TypeCode = TypeCode.Single },
            new TypeInformation { Type = typeof(double), TypeCode = TypeCode.Double },
            new TypeInformation { Type = typeof(decimal), TypeCode = TypeCode.Decimal },
            new TypeInformation { Type = typeof(DateTime), TypeCode = TypeCode.DateTime },
            new TypeInformation { Type = typeof(object), TypeCode = TypeCode.Empty }, // no 17 in TypeCode for some reason
            new TypeInformation { Type = typeof(string), TypeCode = TypeCode.String }
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


        public static TypeCode GetInstanceObjectTypeCode(object value)
        {
            if (value is System.Collections.IEnumerable)
            {
                if (value is System.Collections.IDictionary)
                {
                    return TypeCode.Dictionary;
                }

                if (value is System.Collections.Specialized.NameValueCollection)
                {
                    return TypeCode.NameValueCollection;
                }

                return TypeCode.Enumerable;

            }//IEnumerable

            if (value is System.ComponentModel.IListSource)
            {
                if (value is System.Data.DataSet)
                    return TypeCode.DataSet;
                if (value is System.Data.DataTable)
                    return TypeCode.DataTable;
            }

            if(value is IJsonSerializeImplementation)
            {
                return TypeCode.IJsonSerializeImplementation;
            }

            return TypeCode.Custom;
        }

        public static TypeCode GetTypeCode(Type type)
        {
            if (TypeCodeMap.TryGetValue(type, out TypeCode typeCode))
            {
                return typeCode;
            }

            if (type.IsEnum)
            {
                return GetTypeCode(Enum.GetUnderlyingType(type));
            }

            return TypeCode.NotSetObject;
        }

        public static TypeCode GetTypeCode(object obj)
        {
            Type type = obj.GetType();
            if (TypeCodeMap.TryGetValue(type, out TypeCode typeCode))
            {
                return typeCode;
            }

            if (type.IsEnum)
            {
                return GetTypeCode(Enum.GetUnderlyingType(type));
            }

            return GetInstanceObjectTypeCode(obj);
        }

        public static bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static void ResolveConvertibleValue(IConvertible convertible, out TypeCode typeCode, out object value)
        {
            // the value is a non-standard IConvertible
            // convert to the underlying value and retry
            TypeInformation typeInformation = PrimitiveTypeCodes[(int)convertible.GetTypeCode()];

            // if convertible has an underlying typecode of Object then attempt to convert it to a string
            typeCode = typeInformation.TypeCode == TypeCode.NotSetObject ? TypeCode.String : typeInformation.TypeCode;
            Type resolvedType = typeInformation.TypeCode == TypeCode.NotSetObject ? typeof(string) : typeInformation.Type;
            value = convertible.ToType(resolvedType, CultureInfo.InvariantCulture);
        }

        public static TypeCode GetEnumerableValueTypeCode(System.Collections.IEnumerable anEnumerable)
        {
            if(anEnumerable is Array)
            {
                return GetTypeCode(anEnumerable.GetType().GetElementType());
            }
            else if(anEnumerable is System.Collections.ArrayList)
            {
                return TypeCode.Custom;
            }
            else if(anEnumerable is System.Collections.IList)
            {
                Type type =anEnumerable.GetType();
                if (type.IsGenericType)
                {
                    return (GetTypeCode(type.GetGenericArguments()[0]));
                }
            }

             return TypeCode.Custom;
        }
    }
}
