﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Zippy.Utility
{
    sealed class TypeSerializerUtils
    {
        public enum TypeCode
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
            Uri = 38,
            String = 39,
            Bytes = 40,
            DBNull = 41,
            Exception = 42,
            NotSetObject = 100,
            DataTable = 101,
            DataSet = 102,
            Dictionary = 103,
            NameValueCollection = 104,
            Enumerable = 105,
            Array = 106,
            IList = 107,
            GenericDictionary = 108
            //    ,Object=200
        }

        private static readonly Hashtable TypeCodeMap = new Hashtable()
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
                { typeof(Uri), TypeCode.Uri },
                { typeof(string), TypeCode.String },
                { typeof(byte[]), TypeCode.Bytes },
                { typeof(DBNull), TypeCode.DBNull },
            {typeof(Exception), TypeCode.Exception },
               {typeof(List<>), TypeCode.Enumerable},
            {typeof(LinkedList<>),TypeCode.Enumerable},
            {typeof(Queue<>), TypeCode.Enumerable},
            {typeof(Stack<>), TypeCode.Enumerable},
            {typeof(HashSet<>), TypeCode.Enumerable},
            {typeof(System.Collections.ObjectModel.ReadOnlyCollection<>), TypeCode.Enumerable},
        {typeof(System.Collections.IList), TypeCode.IList },
            {typeof(IList<>), TypeCode.Enumerable},
            {typeof(ICollection<>), TypeCode.Enumerable},
            {typeof(IEnumerable<>), TypeCode.Enumerable},
            {typeof(Dictionary<,>), TypeCode.GenericDictionary},
            {typeof(IDictionary<,>), TypeCode.GenericDictionary},
            {typeof(SortedDictionary<,>), TypeCode.Dictionary},
            {typeof(SortedList<,>), TypeCode.Dictionary},
            {typeof(System.Linq.ILookup<,>), TypeCode.Enumerable},
            {typeof(System.Linq.IGrouping<,>), TypeCode.Enumerable},
            #if NETSTANDARD
            {typeof(System.Collections.ObjectModel.ObservableCollection<>), TypeCode.Enumerable},
            {typeof(System.Collections.ObjectModel.ReadOnlyObservableCollection<>),TypeCode.Enumerable},
            {typeof(IReadOnlyList<>), TypeCode.Enumerable},
            {typeof(IReadOnlyCollection<>),TypeCode.Enumerable},
            {typeof(ISet<>), TypeCode.Enumerable},
            {typeof(System.Collections.Concurrent.ConcurrentBag<>), TypeCode.Enumerable},
            {typeof(System.Collections.Concurrent.ConcurrentQueue<>),TypeCode.Enumerable},
            {typeof(System.Collections.Concurrent.ConcurrentStack<>), TypeCode.Enumerable},
            {typeof(System.Collections.ObjectModel.ReadOnlyDictionary<,>), TypeCode.Dictionary},
            {typeof(IReadOnlyDictionary<,>),TypeCode.Dictionary},
            {typeof(System.Collections.Concurrent.ConcurrentDictionary<,>), TypeCode.Dictionary},
            {typeof(System.Data.DataTable), TypeCode.DataTable},
            {typeof(System.Data.DataSet), TypeCode.DataSet},
             {typeof(System.Collections.Specialized.NameValueCollection), TypeCode.NameValueCollection},
            {typeof(System.Collections.IDictionary), TypeCode.Dictionary}
              //          ,{typeof(object), TypeCode.Object}
//  {typeof(Lazy<>), typeof(LazyFormatter<>)},
//{typeof(Task<>), typeof(TaskValueFormatter<>)},
#endif
};


        private static TypeData _td = new TypeData()
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
                { typeof(Uri), TypeCode.Uri },
                { typeof(string), TypeCode.String },
                { typeof(byte[]), TypeCode.Bytes },
                { typeof(DBNull), TypeCode.DBNull },
            {typeof(Exception), TypeCode.Exception },
               {typeof(List<>), TypeCode.Enumerable},
            {typeof(LinkedList<>),TypeCode.Enumerable},
            {typeof(Queue<>), TypeCode.Enumerable},
            {typeof(Stack<>), TypeCode.Enumerable},
            {typeof(HashSet<>), TypeCode.Enumerable},
            {typeof(System.Collections.ObjectModel.ReadOnlyCollection<>), TypeCode.Enumerable},
        {typeof(System.Collections.IList), TypeCode.IList },
            {typeof(IList<>), TypeCode.Enumerable},
            {typeof(ICollection<>), TypeCode.Enumerable},
            {typeof(IEnumerable<>), TypeCode.Enumerable},
            {typeof(Dictionary<,>), TypeCode.GenericDictionary},
            {typeof(IDictionary<,>), TypeCode.GenericDictionary},
            {typeof(SortedDictionary<,>), TypeCode.Dictionary},
            {typeof(SortedList<,>), TypeCode.Dictionary},
            {typeof(System.Linq.ILookup<,>), TypeCode.Enumerable},
            {typeof(System.Linq.IGrouping<,>), TypeCode.Enumerable},
            #if NETSTANDARD
            {typeof(System.Collections.ObjectModel.ObservableCollection<>), TypeCode.Enumerable},
            {typeof(System.Collections.ObjectModel.ReadOnlyObservableCollection<>),TypeCode.Enumerable},
            {typeof(IReadOnlyList<>), TypeCode.Enumerable},
            {typeof(IReadOnlyCollection<>),TypeCode.Enumerable},
            {typeof(ISet<>), TypeCode.Enumerable},
            {typeof(System.Collections.Concurrent.ConcurrentBag<>), TypeCode.Enumerable},
            {typeof(System.Collections.Concurrent.ConcurrentQueue<>),TypeCode.Enumerable},
            {typeof(System.Collections.Concurrent.ConcurrentStack<>), TypeCode.Enumerable},
            {typeof(System.Collections.ObjectModel.ReadOnlyDictionary<,>), TypeCode.Dictionary},
            {typeof(IReadOnlyDictionary<,>),TypeCode.Dictionary},
            {typeof(System.Collections.Concurrent.ConcurrentDictionary<,>), TypeCode.Dictionary},
            {typeof(System.Data.DataTable), TypeCode.DataTable},
            {typeof(System.Data.DataSet), TypeCode.DataSet},
             {typeof(System.Collections.Specialized.NameValueCollection), TypeCode.NameValueCollection},
            {typeof(System.Collections.IDictionary), TypeCode.Dictionary}
              //          ,{typeof(object), TypeCode.Object}
//  {typeof(Lazy<>), typeof(LazyFormatter<>)},
//{typeof(Task<>), typeof(TaskValueFormatter<>)},
#endif
};
        static TypeSerializerUtils()
        {
       foreach(System.Collections.DictionaryEntry item in TypeCodeMap)
            {
                _td.Add((Type)item.Key, (TypeCode)item.Value);
            }
        }


        public static TypeCode GetTypeCode(Type type)
        {
            var tc = _td.Get(type);

           var typeCode = TypeCodeMap[type];
            if (typeCode != null)
            {
                return (TypeCode)typeCode;
            }
            if (type.IsGenericType)
            {
                typeCode = TypeCodeMap[type.GetGenericTypeDefinition()];
                if (typeCode != null)
                {
                    return (TypeCode)typeCode;
                }
            }

            var baseType = type.BaseType;
            if (baseType == typeof(Enum))
            {
                return GetTypeCode(Enum.GetUnderlyingType(type));
            }
            if (baseType == typeof(Array))
            {
                return TypeCode.Array;
            }

            return TypeCode.NotSetObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string  FormatPropertyName(string value)
        {
            switch (JSON.Options.TextCase)
            {
                case TextCase.CamelCase:
                    return value.ToCamelCase();
                case TextCase.SnakeCase:
                    return value.ToLowercaseUnderscore();
                default:
                    return value;
            }
        }

        public static string BuildPropertyName(string name)
        {
            name = FormatPropertyName(name);
            name = ArrayUtil.ToString(StringExtension.GetEncodeString(name, true));
            name = string.Concat(name, ":");
            return name;
        }

        public static TypeCode GetEnumerableValueTypeCode(System.Collections.IEnumerable anEnumerable, Type type)
        {
            if (anEnumerable is System.Collections.ArrayList)
            {
                return TypeCode.NotSetObject;
            }
            else if (anEnumerable is System.Collections.IList)
            {
                return GetIListValueTypeCode(type);
            }
            else if (anEnumerable is Array)
            {
                return GetArrayValueTypeCode(type);
            }
            return TypeCode.NotSetObject;
        }

        public static TypeCode GetArrayValueTypeCode(Type type)
        {
            return GetTypeCode(type.GetElementType());
        }

        public static TypeCode GetIListValueTypeCode(Type type)
        {
            if (type.IsGenericType)
            {
                return (GetTypeCode(type.GetGenericArguments()[0]));
            }

            return TypeCode.NotSetObject;
        }

        public static TypeCode GetEnumerableValueTypeCode(Type type)
        {
            if (type.IsArray)
            {
                return GetTypeCode(type.GetElementType());
            }
            //else if (anEnumerable is System.Collections.ArrayList)
            //{
            //    return TypeCode.NotSetObject;
            //}
            //else if (type is System.Collections.IList)
            //{
            //    return GetIListValueTypeCode((System.Collections.IList)anEnumerable);
            //}

            return TypeCode.NotSetObject;
        }
    }
}
