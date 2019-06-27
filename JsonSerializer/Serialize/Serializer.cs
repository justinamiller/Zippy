using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Zippy.Internal;
using Zippy.Utility;
using static Zippy.Utility.TypeSerializerUtils;
using System.Linq;
using System.Collections.Specialized;

namespace Zippy.Serialize
{
    /// <summary>
    /// Json Serializer.
    /// </summary>
    /// <remarks>guide from http://www.json.org/ </remarks>
    sealed class Serializer
    {
        private readonly static IJsonSerializerStrategy s_currentJsonSerializerStrategy = new LambdaJsonSerializerStrategy();

        // The following logic performs circular reference detection
        private readonly ReferenceCheck _cirobj = new ReferenceCheck();

        private int _currentDepth = 0;
        private JsonWriter _jsonWriter;


        public Serializer()
        {
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeNameValueCollection(System.Collections.Specialized.NameValueCollection value)
        {
            if (value.Count == 0)
            {
                _jsonWriter.WriteStartObject();
                _jsonWriter.WriteEndObject();
                return true;
            }

            _jsonWriter.WriteStartObject();
            try
            {
                string[] keys = value.AllKeys;
                int len = keys.Length;
                for (int i = 0; i < len; i++)
                {
                    _jsonWriter.WritePropertyName(keys[i]);
                    _jsonWriter.WriteString(value.Get(i));
                }
            }
            finally
            {
                _jsonWriter.WriteEndObject();
            }

            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeEnumerable(IEnumerable values, Type type)
        {
            TypeSerializerUtils.TypeCode valueType = TypeSerializerUtils.TypeCode.Empty;
            Type lastType = null;
            bool flag1 = true;
            bool isTyped = false;
            _jsonWriter.WriteStartArray();
            try
            {
                var e = values.GetEnumerator();

                // note that an error in the IEnumerable won't be caught
                while (e.MoveNext())
                {
                    var value = e.Current;
                    if (!flag1)
                    {
                        _jsonWriter.WriteComma();
                    }
                    else
                    {
                        //first record.
                        valueType = GetEnumerableValueTypeCode(values, type);
                        isTyped = valueType != TypeSerializerUtils.TypeCode.NotSetObject;

                        if (!isTyped)
                        {
                            //check if generic type and is typed.
                            if (type.IsGenericType)
                            {
                                type = type.GetGenericArguments()[0];
                                isTyped = type != typeof(object);
                            }
                        }

                        flag1 = false;
                    }

                    if (value == null)
                    {
                        _jsonWriter.WriteNull();
                    }
                    else
                    {
                        if (!isTyped)
                        {
                            //is not generic
                            type = value.GetType();
                            if (type != lastType)
                            {
                                lastType = type;
                                valueType = GetTypeCode(type);
                            }
                        }

                        if (!WriteObjectValue(value, type, valueType))
                        {
                            return false;
                        }
                    }
                }
            }
            finally
            {
                _jsonWriter.WriteEndArray();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeArray(Array array, Type type)
        {
            if (array.Rank > 1)
            {
                return SerializeMultidimensionalArray(array);
            }

            int len = array.Length;
            if (len == 0)
            {
                _jsonWriter.WriteStartArray();
                _jsonWriter.WriteEndArray();
            }

            TypeSerializerUtils.TypeCode valueTypeCode = TypeSerializerUtils.TypeCode.Empty;
            Type lastType = null;
            bool flag1 = false;

            var currentType = GetArrayValueTypeCode(type);
            bool isTyped = currentType != TypeSerializerUtils.TypeCode.NotSetObject;
            var valueType = type.GetElementType();
            if (!isTyped)
            {
                //check one more time.
                isTyped = valueType != typeof(object);
            }

            if (isTyped)
            {
                valueTypeCode = currentType;
            }



            _jsonWriter.WriteStartArray();
            try
            {
                // note that an error in the IEnumerable won't be caught
                for (var i = 0; i < len; i++)
                {
                    if (flag1)
                    {
                        _jsonWriter.WriteComma();
                    }
                    else
                    {
                        flag1 = true;
                    }

                    var value = array.GetValue(i);
                    if (!isTyped)
                    {
                        valueType = value.GetType();
                        if (lastType != valueType)
                        {
                            lastType = valueType;
                            valueTypeCode = GetTypeCode(valueType);
                        }
                    }

                    if (!WriteObjectValue(value, valueType, valueTypeCode))
                    {
                        return false;
                    }
                }
            }
            finally
            {
                _jsonWriter.WriteEndArray();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeList(System.Collections.IList list, Type objectType)
        {
            int len = list.Count;
            if (len == 0)
            {
                _jsonWriter.WriteStartArray();
                _jsonWriter.WriteEndArray();
            }

            TypeSerializerUtils.TypeCode valueTypeCode = TypeSerializerUtils.TypeCode.Empty;
            bool flag1 = true;
            Type lastType = null;

            var currentType = GetEnumerableValueTypeCode(list, objectType);
            bool isTyped = currentType != TypeSerializerUtils.TypeCode.NotSetObject;

            if (isTyped)
            {
                valueTypeCode = currentType;
            }

            _jsonWriter.WriteStartArray();
            try
            {
                // note that an error in the IEnumerable won't be caught
                for (var i = 0; i < len; i++)
                {
                    if (!flag1)
                    {
                        _jsonWriter.WriteComma();
                    }
                    else
                    {
                        //first record.
                        flag1 = false;
                    }

                    var value = list[i];
                    if (value == null)
                    {
                        _jsonWriter.WriteNull();
                    }
                    else
                    {
                        if (!isTyped)
                        {
                            Type valueType = value.GetType();
                            if (lastType != valueType)
                            {
                                lastType = valueType;
                                valueTypeCode = GetTypeCode(valueType);
                            }
                        }

                        if (!WriteObjectValue(value, lastType, valueTypeCode))
                        {
                            return false;
                        }
                    }
                }
            }
            finally
            {
                _jsonWriter.WriteEndArray();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeMultidimensionalArray(Array values)
        {
            return SerializeMultidimensionalArray(values, Array.Empty<int>());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeMultidimensionalArray(Array values, int[] indices)
        {
            bool flag = true;
            int dimension = indices.Length;
            int[] newIndices = new int[dimension + 1];
            for (int i = 0; i < dimension; i++)
            {
                newIndices[i] = indices[i];
            }

            _jsonWriter.WriteStartArray();
            try
            {
                Type lastTypeCode = null;
                TypeSerializerUtils.TypeCode valueTypeCode = TypeSerializerUtils.TypeCode.Empty;

                for (int i = values.GetLowerBound(dimension); i <= values.GetUpperBound(dimension); i++)
                {
                    newIndices[dimension] = i;
                    bool isTopLevel = (newIndices.Length == values.Rank);
                    if (isTopLevel)
                    {
                        object value = values.GetValue(newIndices);

                        if (!flag)
                        {
                            _jsonWriter.WriteComma();
                        }
                        else
                        {
                            flag = false;
                        }

                        var typeCode = value.GetType();
                        if (lastTypeCode != typeCode)
                        {
                            lastTypeCode = typeCode;
                            valueTypeCode = GetTypeCode(typeCode);
                        }
                        if (!WriteObjectValue(value, typeCode, valueTypeCode))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (i != 0)
                        {
                            _jsonWriter.WriteComma();
                        }

                        SerializeMultidimensionalArray(values, newIndices);
                    }
                }
            }
            finally
            {
                _jsonWriter.WriteEndArray();
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeObjectInternal(object json, TextWriter writer)
        {
            _jsonWriter = new JsonWriter(writer);
            Type type = json.GetType();
            var typeCode = GetTypeCode(type);

            if (!WriteObjectValue(json, type, typeCode))
            {
                throw new Exception("Unable to Serialize");
            }

            //check to ensure everything was done correctly.
            if (_currentDepth > 0 || !_jsonWriter.IsValid())
            {
                throw new Exception("Unable to Serialize");
            }
        }


        private bool SerializeNonGenericDictionary(IDictionary values)
        {
            if (values.Count == 0)
            {
                _jsonWriter.WriteStartObject();
                _jsonWriter.WriteEndObject();
                return true;
            }

            TypeSerializerUtils.TypeCode valueTypeCode = TypeSerializerUtils.TypeCode.Empty;
            Type lastValueType = null;
            var ranOnce = false;
            var enumerator = values.GetEnumerator();
            _jsonWriter.WriteStartObject();
            try
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Key;
                    if (!ranOnce)
                    {
                        //check if key is string type if not then will skip serialization.
                        var keyType = key.GetType();
                        if (GetTypeCode(keyType) != TypeSerializerUtils.TypeCode.String)
                        {
                            return false;
                        }
                        ranOnce = true;
                    }
                    else
                    {
                        //index 1+
                        _jsonWriter.WriteComma();
                    }

                    string name = key.ToString();
                    _jsonWriter.WritePropertyName(name);

                    var dictionaryValue = enumerator.Value;
                    if (dictionaryValue == null)
                    {
                        _jsonWriter.WriteNull();
                    }
                    else
                    {
                        var valueType = dictionaryValue.GetType();
                        if (lastValueType != valueType)
                        {
                            lastValueType = valueType;
                            valueTypeCode = GetTypeCode(valueType);
                        }

                        if (!WriteObjectValue(dictionaryValue, valueType, valueTypeCode))
                        {
                            return false;
                        }
                    }
                }
            }
            finally
            {
                _jsonWriter.WriteEndObject();
            }

            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeGenericDictionary(IDictionary values, Type type)
        {
            if (values.Count == 0)
            {
                _jsonWriter.WriteStartObject();
                _jsonWriter.WriteEndObject();
                return true;
            }

            //check if key is string type
            Type[] args = type.GetGenericArguments();

            if (args.Length == 0)
            {
                //System.Collections.IDictionary
                return SerializeNonGenericDictionary(values);
            }

            //System.Collections.Generic.IDictionary
            var keyCodeType = GetTypeCode(args[0]);
            if (keyCodeType != TypeSerializerUtils.TypeCode.String)
            {
                return false;
            }

            //get value type
            type = args[1];
            var valueCodeType = GetTypeCode(type);
            return SerializeGenericDictionaryInternal(values, valueCodeType, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeValueMemberInfo(object instance, IValueMemberInfo[] items)
        {
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.

            int len = items.Length;
            IValueMemberInfo item;
            _jsonWriter.WriteStartObject();
            try
            {
                bool isError = false;
                for (var i = 0; i < len; i++)
                {
                    item = items[i];

                    var value = item.GetValue(instance, ref isError);

                    if (!isError || JSON.Options.SerializationErrorHandling == SerializationErrorHandling.ReportValueAsNull)
                    {
                        if (value != null || !JSON.Options.ShouldExcludeNulls)
                        {
                            _jsonWriter.WritePropertyNameFast(item.Name);

                            if (!WriteObjectValue(value, item.ValueType, item.Code))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            finally
            {
                _jsonWriter.WriteEndObject();
            }

            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WriteObjectValue(object value, Type type, TypeSerializerUtils.TypeCode valueTypeCode)
        {
            if (value == null)
            {
                _jsonWriter.WriteNull();
                return true;
            }

            if (valueTypeCode >= TypeSerializerUtils.TypeCode.NotSetObject)
            {
                return SerializeNonPrimitiveValue(value, type, valueTypeCode);
            }

            return _jsonWriter.WriteValueTypeToStringMethod(valueTypeCode, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeGenericDictionaryInternal(IDictionary values, TypeSerializerUtils.TypeCode valueCodeType, Type valueType)
        {
            _jsonWriter.WriteStartObject();
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
            IDictionaryEnumerator e = values.GetEnumerator();
            DictionaryEntry entry;
            try
            {
                while (e.MoveNext())
                {
                    entry = e.Entry;

                    string name = entry.Key.ToString();
                    _jsonWriter.WritePropertyName(name);
                    var value = entry.Value;

                    if (!WriteObjectValue(value, valueType, valueCodeType))
                    {
                        return false;
                    }
                }
            }
            finally
            {
                _jsonWriter.WriteEndObject();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeDataSet(System.Data.DataSet ds)
        {
            _jsonWriter.WriteStartObject();
            try
            {
                foreach (System.Data.DataTable table in ds.Tables)
                {
                    if (!SerializeDataTableData(table))
                    {
                        return false;
                    }
                }        
            }
            finally
            {
                _jsonWriter.WriteEndObject();
            }

            return true;
        }

        private bool SerializeDataTableData(System.Data.DataTable table)
        {
            var rows = table.Rows;
            int count = rows.Count;
            if (count == 0)
            {
                return true;
            }

            System.Data.DataColumnCollection cols = table.Columns;
            var columnType = new List<Tuple<string, TypeSerializerUtils.TypeCode, Type, int>>();
            int columnCount = 0;
            foreach (System.Data.DataColumn column in cols)
            {
                var typeCode = GetTypeCode(column.DataType);

                var columnName = BuildPropertyName(column.ColumnName);

                columnType.Add(new Tuple<string, TypeSerializerUtils.TypeCode, Type, int>(columnName, typeCode, column.DataType, column.Ordinal));
                columnCount++;
            }

            var tableName = table.TableName;
            if (tableName.IsNullOrWhiteSpace())
            {
                tableName = "_empty_";
            }

            bool rowseparator = false;
            _jsonWriter.WritePropertyName(tableName);
            _jsonWriter.WriteStartArray();
            try
            {
                for (var i = 0; i < count; i++)
                {
                    System.Data.DataRow row = rows[i];
                    if (rowseparator)
                    {
                        _jsonWriter.WriteComma();
                    }
                    else
                    {
                        rowseparator = true;
                    }

                    _jsonWriter.WriteStartObject();
                    try
                    {
                        for (var c = 0; c < columnCount; c++)
                        {
                            var column = columnType[c];
                            //build column name
                            _jsonWriter.WritePropertyNameFast(column.Item1);
                            //build column data
                            var value = row[column.Item4];
                            if (!WriteObjectValue(value, column.Item3, column.Item2))
                            {
                                return false;
                            }
                        }
                    }
                    finally
                    {
                        _jsonWriter.WriteEndObject();
                    }
                }
            }
            finally
            {
                _jsonWriter.WriteEndArray();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeDataTable(System.Data.DataTable dt)
        {
            _jsonWriter.WriteStartObject();
            try
            {
                if (!SerializeDataTableData(dt))
                {
                    return false;
                }
            }
            finally
            {
                _jsonWriter.WriteEndObject();
            }
            // end datatable

            return true;
        }




        private bool SerializeNonPrimitiveValue(object value, Type type, TypeSerializerUtils.TypeCode objectTypeCode)
        {
            //this prevents recursion
            if (_cirobj.IsReferenced(value) && _currentDepth > 0)
            {
                //_circular = true;
                _jsonWriter.WriteNull();
                return true;
            }
            _currentDepth++;
            //recursion limit or max char length
            if (_currentDepth >= JSON.Options.RecursionLimit)
            {
                _currentDepth--;
                _jsonWriter.WriteNull();
                return true;
            }
            //if( _jsonWriter.Length > JSON.Options.MaxJsonLength)
            //{
            //    _currentDepth--;
            //    _jsonWriter.WriteNull();
            //    return false;
            //}

            try
            {
                switch (objectTypeCode)
                {
                    case TypeSerializerUtils.TypeCode.Array:
                        {
                            return SerializeArray((Array)value, type);
                        }
                    case TypeSerializerUtils.TypeCode.IList:
                        {
                            return SerializeList((IList)value, type);
                        }
                    case TypeSerializerUtils.TypeCode.Enumerable:
                        {
                            return this.SerializeEnumerable((IEnumerable)value, type);
                        }
                    case TypeSerializerUtils.TypeCode.Dictionary:
                        {
                            return SerializeNonGenericDictionary((IDictionary)value);
                        }
                    case TypeSerializerUtils.TypeCode.GenericDictionary:
                        {
                            return SerializeGenericDictionary((IDictionary)value, type);
                        }
                    case TypeSerializerUtils.TypeCode.NameValueCollection:
                        {
                            return this.SerializeNameValueCollection((NameValueCollection)value);
                        }
                    case TypeSerializerUtils.TypeCode.DataSet:
                        {
                            return SerializeDataSet((System.Data.DataSet)value);
                        }
                    case TypeSerializerUtils.TypeCode.DataTable:
                        {
                            return SerializeDataTable((System.Data.DataTable)value);
                        }
                    case TypeSerializerUtils.TypeCode.NotSetObject:
                        {
                            IValueMemberInfo[] obj = null;
                            if (s_currentJsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, type, out obj))
                            {
                                return this.SerializeValueMemberInfo(value, obj);
                            }
                            else
                            {
                                _jsonWriter.WriteNull();
                                return true;//was false but when false prevent continuing.
                            }
                        }
                    default:
                        {
                            throw new NotImplementedException(objectTypeCode.ToString());
                        }
                }
            }
            finally
            {
                _currentDepth--;
            }
        }
    }
}
