﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Zippy.Internal;

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
            CustomObject = 100,
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

        public static ValueMemberInfo[] GetterValueFactory(Type type)
        {
            var allMembers = ReflectionExtension.GetFieldsAndProperties(type);

            int len = allMembers.Count;
            var data = new ValueMemberInfo[len];
            int dataIndex = 0;
            for (int i = 0; i < len; i++)
            {
                //get item
                var valueInfo = new ValueMemberInfo(allMembers[i]);
                if (!valueInfo.Name.IsNullOrEmpty())
                {
                    //must have property name.
                    data[dataIndex++] = valueInfo;
                }
            }

            if (dataIndex != len)
            {
                var temp = new ValueMemberInfo[dataIndex];
                Array.Copy(temp, 0, data, 0, dataIndex);
                return temp;
            }

            return data;
        }



        private static readonly FastLookup<Type, TypeCode> TypeCodeMap = new FastLookup<Type, TypeCode>()
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
            {typeof(Exception), TypeCode.Exception },
               {typeof(List<>), TypeCode.IList},
            {typeof(LinkedList<>),TypeCode.Enumerable},
            {typeof(Queue<>), TypeCode.Enumerable},
            {typeof(Stack<>), TypeCode.Enumerable},
            {typeof(HashSet<>), TypeCode.Enumerable},
            {typeof(System.Collections.ObjectModel.ReadOnlyCollection<>), TypeCode.Enumerable},
        {typeof(System.Collections.IList), TypeCode.IList },
            {typeof(IList<>), TypeCode.IList},
            {typeof(ICollection<>), TypeCode.Enumerable},
            {typeof(IEnumerable<>), TypeCode.Enumerable},
            {typeof(Dictionary<,>), TypeCode.GenericDictionary},
            {typeof(IDictionary<,>), TypeCode.GenericDictionary},
            {typeof(SortedDictionary<,>), TypeCode.Dictionary},
            {typeof(SortedList<,>), TypeCode.Dictionary},
            {typeof(System.Linq.ILookup<,>), TypeCode.Enumerable},
            {typeof(System.Linq.IGrouping<,>), TypeCode.Enumerable},
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
            {typeof(System.Collections.Specialized.NameValueCollection), TypeCode.NameValueCollection},
            {typeof(System.Collections.IDictionary), TypeCode.Dictionary},
            {typeof(object), TypeCode.CustomObject},
              {typeof(Array), TypeCode.Array},
            { typeof(DBNull), TypeCode.DBNull },
            {typeof(System.Data.DataTable), TypeCode.DataTable},
            {typeof(System.Data.DataSet), TypeCode.DataSet},
};


        public static TypeCode GetTypeCode(Type type)
        {
            TypeCode typeCode;
            if (TypeCodeMap.TryGetValue(type, out typeCode))
            {
                return typeCode;
            }

            if (type.IsGenericType && TypeCodeMap.TryGetValue(type.GetGenericTypeDefinition(), out typeCode))
            {
                return typeCode;
            }

            var baseType = type.BaseType;
            if (baseType == typeof(object))
            {
                return TypeCode.CustomObject;
            }
            if (baseType == typeof(Enum))
            {
                return GetTypeCode(Enum.GetUnderlyingType(type));
            }
            if (baseType == typeof(Array))
            {
                return TypeCode.Array;
            }

            return TypeCode.CustomObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatPropertyName(string value, TextCase textCase)
        {
            switch (textCase)
            {
                case TextCase.CamelCase:
                    return value.ToCamelCase();
                case TextCase.SnakeCase:
                    return value.ToLowercaseUnderscore();
                default:
                    return value;
            }
        }

        public static string BuildPropertyName(string name, TextCase textCase, bool encode)
        {
            if (name.IsNullOrEmpty())
            {
                return string.Empty;
            }

            name = FormatPropertyName(name, textCase);
            //encode propertyName
            if (encode)
            {
                //force encode.
                var buffer = StringExtension.GetEncodeString(name, false, true);
                int length = buffer.Length;

                var newArray = new char[length + 1];
                Array.Copy(buffer, 0, newArray, 0, length);
                newArray[length] = ':';

                return new string(newArray, 0, length + 1);
            }
            else
            {
                return string.Concat("\"", name, "\":");
            }
        }

        public static Type GetEnumerableValueType(System.Collections.IEnumerable anEnumerable, Type type)
        {
            if (anEnumerable is System.Collections.ArrayList)
            {
                return typeof(object);
            }
            else if (anEnumerable is System.Collections.IList)
            {
                if (type.IsGenericType)
                {
                    return type.GetGenericArguments()[0];
                }

                return typeof(object);
            }
            else if (anEnumerable is Array)
            {
                return type.GetElementType();
            }

            return typeof(object);
        }

        public static bool HasExtendedValueInformation(TypeCode typeCode)
        {
            return typeCode == TypeCode.GenericDictionary
                || typeCode == TypeCode.IList
                || typeCode == TypeCode.Array
                || typeCode == TypeCode.Dictionary
                || typeCode == TypeCode.Enumerable;
        }
    }
}
