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
        internal readonly static IJsonSerializerStrategy CurrentJsonSerializerStrategy = new LambdaJsonSerializerStrategy();

        // The following logic performs circular reference detection
        private readonly ReferenceCheck _cirobj = new ReferenceCheck();
        private int _currentDepth = 0;
        private JsonWriter _jsonWriter;

        private readonly int _recursionLimit = JSON.Options.RecursionLimit;
        private readonly bool _excludeNulls = JSON.Options.ShouldExcludeNulls;


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
                    var name = keys[i];
                    if (!name.IsNullOrEmpty())
                    {
                        _jsonWriter.WritePropertyName(name);
                        _jsonWriter.WriteStringNullable(value.Get(i));
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
        private bool SerializeEnumerable(IEnumerable values, IValueMemberInfo valueMemberInfo)
        {
            Type lastType = null;
            bool flag1 = true;
            bool isTyped = false;
            ValueMemberInfo valueMember = null;
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
                        var valueType = GetEnumerableValueType(values, valueMemberInfo.ObjectType);
                        if (valueType != typeof(object))
                        {
                            valueMember = new ValueMemberInfo(valueType);
                            isTyped = true;
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
                            var valueType = value.GetType();
                            if (valueType != lastType)
                            {
                                lastType = valueMemberInfo.ObjectType;
                                valueMember = new ValueMemberInfo(valueType);
                            }
                        }

                        if (!WriteObjectValue(value, valueMember))
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

        private bool SerializeIListTyped(IList list, int len, IValueMemberInfo valueMemberInfo)
        {
            _jsonWriter.WriteStartArray();
            try
            {
                //one type
                for (var i = 0; i < len; i++)
                {
                    if (i > 0)
                    {
                        _jsonWriter.WriteComma();
                    }

                    var value = list[i];
                    if (!WriteObjectValue(value, valueMemberInfo))
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

        private bool SerializeIListUnTyped(IList list, int len, IValueMemberInfo valueMemberInfo)
        {
            Type lastType = null;

            _jsonWriter.WriteStartArray();
            try
            {
                //dynamic types
                for (var i = 0; i < len; i++)
                {
                    if (i > 0)
                    {
                        _jsonWriter.WriteComma();
                    }

                    var value = list[i];
                    if (value != null)
                    {
                        var valueType = value.GetType();
                        if (lastType != valueType)
                        {
                            lastType = valueType;
                            valueMemberInfo = new ValueMemberInfo(valueType);
                        }
                    }

                    if (!WriteObjectValue(value, valueMemberInfo))
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
        private bool SerializeArray(Array array, IValueMemberInfo valueMemberInfo)
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
                return true;
            }

            var valueMember = valueMemberInfo.ExtendedValueInfo;

            if (valueMember.IsType)
            {
                return SerializeIListTyped(array, len, valueMember);
            }

            //no typed.
            return SerializeIListUnTyped(array, len, valueMember);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeList(System.Collections.IList list, IValueMemberInfo valueMemberInfo)
        {
            int len = list.Count;
            if (len == 0)
            {
                _jsonWriter.WriteStartArray();
                _jsonWriter.WriteEndArray();
                return true;
            }

            IValueMemberInfo valueMember = valueMemberInfo.ExtendedValueInfo;

            if (valueMember.IsType)
            {
                return SerializeIListTyped(list, len, valueMember);
            }

            //no typed.
            return SerializeIListUnTyped(list, len, valueMember);
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

            Type lastTypeCode = null;
            IValueMemberInfo valueMember = null;
            _jsonWriter.WriteStartArray();
            try
            {
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
                            valueMember = new ValueMemberInfo(typeCode);
                        }
                        if (!WriteObjectValue(value, valueMember))
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
        public void SerializeObjectInternal(object obj, TextWriter writer)
        {
            _jsonWriter = new JsonWriter(writer);

            var valueMember = new ValueMemberInfo(obj.GetType());

            if (!WriteObjectValue(obj, valueMember))
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
            ValueMemberInfo valueMember = null;
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
                            valueMember = new ValueMemberInfo(valueType);
                        }

                        if (!WriteObjectValue(dictionaryValue, valueMember))
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
        private bool SerializeGenericDictionary(IDictionary values, IValueMemberInfo valueMemberInfo)
        {
            if (values.Count == 0)
            {
                _jsonWriter.WriteStartObject();
                _jsonWriter.WriteEndObject();
                return true;
            }

            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
            IDictionaryEnumerator e = values.GetEnumerator();
            var valueMember = valueMemberInfo.ExtendedValueInfo;

            _jsonWriter.WriteStartObject();
            try
            {
                while (e.MoveNext())
                {
                    DictionaryEntry entry = e.Entry;

                    string name = entry.Key.ToString();
                    if (!name.IsNullOrEmpty())
                    {
                        var value = entry.Value;

                        _jsonWriter.WritePropertyName(name);
                        if (!WriteObjectValue(value, valueMember))
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
        private bool SerializeValueMemberInfo(object instance, IValueMemberInfo[] items)
        {
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.

            int len = items.Length;
            IValueMemberInfo item;
            bool isError = false;
            _jsonWriter.WriteStartObject();
            try
            {
                for (var i = 0; i < len; i++)
                {
                    item = items[i];

                    var value = item.GetValue(instance, ref isError);

                    if (!isError || JSON.Options.SerializationErrorHandling == SerializationErrorHandling.ReportValueAsNull)
                    {
                        if (value != null || !_excludeNulls)
                        {
                            _jsonWriter.WritePropertyNameFast(item.Name);

                            if (!WriteObjectValue(value, item))
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
        private bool WriteObjectValue(object value, IValueMemberInfo valueMemberInfo)
        {
            if (value == null)
            {
                _jsonWriter.WriteNull();
                return true;
            }

            var typeCode = valueMemberInfo.Code;
            if (typeCode >= TypeSerializerUtils.TypeCode.CustomObject)
            {
                //object type
                return SerializeNonPrimitiveValue(value, valueMemberInfo);
            }

            //primiative type
            return _jsonWriter.WriteValueTypeToStringMethod(typeCode, value);
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
            var columnType = new List<Tuple<string, ValueMemberInfo, int>>();
            int columnCount = 0;
            foreach (System.Data.DataColumn column in cols)
            {
                var valueMember = new ValueMemberInfo(column.DataType);

                var columnName = BuildPropertyName(column.ColumnName);

                columnType.Add(new Tuple<string, ValueMemberInfo, int>(columnName, valueMember, column.Ordinal));
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
                            var value = row[column.Item3];
                            if (!WriteObjectValue(value, column.Item2))
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

        private bool SerializeNonPrimitiveValue(object value, IValueMemberInfo valueMemberInfo)
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
            if (_currentDepth >= _recursionLimit)
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
                switch (valueMemberInfo.Code)
                {
                    case TypeSerializerUtils.TypeCode.CustomObject:
                        {
                            return SerializeCustomObject(value, valueMemberInfo);
                        }
                    case TypeSerializerUtils.TypeCode.Array:
                        {
                            return SerializeArray((Array)value, valueMemberInfo);
                        }
                    case TypeSerializerUtils.TypeCode.IList:
                        {
                            return SerializeList((IList)value, valueMemberInfo);
                        }
                    case TypeSerializerUtils.TypeCode.Enumerable:
                        {
                            return this.SerializeEnumerable((IEnumerable)value, valueMemberInfo);
                        }
                    case TypeSerializerUtils.TypeCode.Dictionary:
                        {
                            return SerializeNonGenericDictionary((IDictionary)value);
                        }
                    case TypeSerializerUtils.TypeCode.GenericDictionary:
                        {
                            return SerializeGenericDictionary((IDictionary)value, valueMemberInfo);
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
                    default:
                        {
                            throw new NotImplementedException(valueMemberInfo.Code.ToString());
                        }
                }
            }
            finally
            {
                _currentDepth--;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeCustomObject(object value, IValueMemberInfo valueMemberInfo)
        {
            IValueMemberInfo[] obj = valueMemberInfo.GetCustomObjectMemberInfo();

            if (obj != null)
            {
                return this.SerializeValueMemberInfo(value, obj);
            }
            else
            {
                _jsonWriter.WriteNull();
                return true;//was false but when false prevent continuing.
            }
        }
    }
}
