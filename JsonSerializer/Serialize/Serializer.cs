using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Zippy.Internal;
using Zippy.Utility;
using static Zippy.Utility.TypeSerializerUtils;

namespace Zippy.Serialize
{
    /// <summary>
    /// Json Serializer.
    /// </summary>
    /// <remarks>guide from http://www.json.org/ </remarks>
    sealed class Serializer
    {
        private static IJsonSerializerStrategy s_currentJsonSerializerStrategy = new LambdaJsonSerializerStrategy();

        // The following logic performs circular reference detection
        private readonly Dictionary<object, int> _cirobj = new Dictionary<object, int>();
        private int _currentDepth = 0;
        private TextWriter _writer;
        private bool _propertyInUse;

        public Serializer()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteEndObject()
        {
            this._propertyInUse = true;
            this._writer.Write('}');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteEndArray()
        {
            this._writer.Write(']');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteStartArray()
        {
            this._writer.Write('[');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteStartObject()
        {
            this._writer.Write('{');
            this._propertyInUse = false;
        }

        private void WritePropertyName(string value)
        {
            WritePropertyName(StringExtension.GetEncodeString(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteNull()
        {
            _writer.Write(JsonTypeSerializer.Null, 0, 4);
        }

        private void WritePropertyName(char[] value)
        {
            if (this._propertyInUse)
            {
                this._writer.Write(',');
            }
            else
            {
                this._propertyInUse = true;
            }

            _writer.Write(value);
            _writer.Write(':');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeNameValueCollection(System.Collections.Specialized.NameValueCollection value)
        {
            WriteStartObject();
            try
            {
                string[] keys = value.AllKeys;
                int len = keys.Length;
                for (int i = 0; i < len; i++)
                {
                    WritePropertyName(keys[i]);
                    JsonTypeSerializer.Serializer.WriteString(_writer, value.Get(i));
                }
            }
            finally
            {
                WriteEndObject();
            }

            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeEnumerable(IEnumerable anEnumerable)
        {
            TypeSerializerUtils.TypeCode valueType = TypeSerializerUtils.TypeCode.Empty;
            WriteObjectDelegate writeObject = null;
            Type type = null;
            bool flag1 = true;
            bool isTyped = false;
            WriteStartArray();
            try
            {
                // note that an error in the IEnumerable won't be caught
                foreach (object value in anEnumerable)
                {
                    if (!flag1)
                    {
                        _writer.Write(',');
                    }
                    else
                    {
                        //first record.
                        valueType = GetEnumerableValueTypeCode(anEnumerable);
                        writeObject = JsonTypeSerializer.GetValueTypeToStringMethod(valueType);

                        isTyped = valueType != TypeSerializerUtils.TypeCode.NotSetObject;
                        flag1 = false;
                    }

                    if (value == null)
                    {
                        WriteNull();
                    }
                    else
                    {
                        if (!isTyped)
                        {
                            //is not generic
                            type = value.GetType();
                            valueType = GetTypeCode(type);
                            writeObject = JsonTypeSerializer.GetValueTypeToStringMethod(valueType);
                        }

                        if (!WriteObjectValue(value, writeObject, type, valueType))
                        {
                            return false;
                        }
                    }
                }
            }
            finally
            {
                WriteEndArray();
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

            TypeSerializerUtils.TypeCode valueTypeCode = TypeSerializerUtils.TypeCode.Empty;
            WriteObjectDelegate writeObject = null;
            Type lastType = null;
            bool flag1 = true;
            WriteStartArray();

            Type valueType = null;
            var currentType = GetArrayValueTypeCode(type);
            bool isTyped = currentType != TypeSerializerUtils.TypeCode.NotSetObject;
            if (isTyped)
            {
                valueTypeCode = currentType;
                writeObject = JsonTypeSerializer.GetValueTypeToStringMethod(valueTypeCode);
                valueType = type.GetElementType();
            }

            int len = array.Length;
            try
            {
                // note that an error in the IEnumerable won't be caught
                for (var i = 0; i < len; i++)
                {
                    if (!flag1)
                    {
                        _writer.Write(',');
                    }

                    var value = array.GetValue(i);
                    if (!isTyped)
                    {
                        valueType = value.GetType();
                        if (lastType != valueType)
                        {
                            lastType = valueType;
                            valueTypeCode = GetTypeCode(valueType);
                            writeObject = JsonTypeSerializer.GetValueTypeToStringMethod(valueTypeCode);
                        }
                    }

                    if (!WriteObjectValue(value, writeObject, valueType, valueTypeCode))
                    {
                        return false;
                    }
                }
            }
            finally
            {
                WriteEndArray();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeList(System.Collections.IList list)
        {
            TypeSerializerUtils.TypeCode valueTypeCode = TypeSerializerUtils.TypeCode.Empty;
            WriteObjectDelegate writeObject = null;
            bool flag1 = true;
            Type lastType = null;

            var currentType = TypeSerializerUtils.GetEnumerableValueTypeCode(list);
            bool isTyped = currentType != TypeSerializerUtils.TypeCode.NotSetObject;

            if (isTyped)
            {
                valueTypeCode = currentType;
                writeObject = JsonTypeSerializer.GetValueTypeToStringMethod(valueTypeCode);
            }

            WriteStartArray();
            try
            {
                int len = list.Count;
                // note that an error in the IEnumerable won't be caught
                for (var i = 0; i < len; i++)
                {
                    var value = list[i];
                    if (!flag1)
                    {
                        _writer.Write(',');
                    }
                    else
                    {
                        //first record.
                        flag1 = false;
                    }

                    if (value == null)
                    {
                        WriteNull();
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
                                writeObject = JsonTypeSerializer.GetValueTypeToStringMethod(valueTypeCode);
                            }
                        }

                        if (!WriteObjectValue(value, writeObject, lastType, valueTypeCode))
                        {
                            return false;
                        }
                    }
                }
            }
            finally
            {
                WriteEndArray();
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

            WriteStartArray();
            try
            {
                Type lastTypeCode = null;
                WriteObjectDelegate writeValueFn = null;
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
                            _writer.Write(',');
                        }

                        var typeCode = value.GetType();
                        if (lastTypeCode != typeCode)
                        {
                            lastTypeCode = typeCode;
                            valueTypeCode = Utility.TypeSerializerUtils.GetTypeCode(typeCode);
                            writeValueFn = JsonTypeSerializer.GetValueTypeToStringMethod(valueTypeCode);
                        }
                        if (!WriteObjectValue(value, writeValueFn, typeCode, valueTypeCode))
                        {
                            return false;
                        }
                        flag = false;
                    }
                    else
                    {
                        if (i != 0)
                        {
                            _writer.Write(',');
                        }

                        SerializeMultidimensionalArray(values, newIndices);
                    }
                }
            }
            finally
            {
                WriteEndArray();
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeObjectInternal(object json, TextWriter writer)
        {
            Type type = json.GetType();
            var typeCode = TypeSerializerUtils.GetTypeCode(type);
            if (typeCode >= TypeSerializerUtils.TypeCode.NotSetObject)
            {
                _writer = writer;
                //handle for object
                SerializeNonPrimitiveValue(json, type, typeCode);
            }
            else
            {
                JsonTypeSerializer.GetValueTypeToStringMethod(typeCode)?.Invoke(writer, json);
            }
        }

        private bool SerializeNonGenericDictionary(IDictionary values)
        {
            WriteObjectDelegate writeKeyFn = null;
            WriteObjectDelegate writeValueFn = null;

            TypeSerializerUtils.TypeCode keyTypeCode = TypeSerializerUtils.TypeCode.Empty;
            TypeSerializerUtils.TypeCode valueTypeCode = TypeSerializerUtils.TypeCode.Empty;

            Type lastKeyType = null;
            Type lastValueType = null;

            var ranOnce = false;
            WriteStartObject();
            try
            {
                foreach (var key in values.Keys)
                {
                    var dictionaryValue = values[key];

                    var keyType = key.GetType();
                    if (lastKeyType != keyType)
                    {
                        lastKeyType = keyType;
                        keyTypeCode = TypeSerializerUtils.GetTypeCode(keyType);
                        writeKeyFn = JsonTypeSerializer.GetValueTypeToStringMethod(keyTypeCode);
                    }

                    if (ranOnce)
                    {
                        _writer.Write(',');
                    }

                    WriteObjectValue(key, writeKeyFn, keyType, keyTypeCode);
                    _writer.Write(':');

                    if (dictionaryValue == null)
                    {
                        WriteNull();
                    }
                    else
                    {
                        var valueType = dictionaryValue.GetType();
                        if (lastValueType != valueType)
                        {
                            lastValueType = valueType;
                            valueTypeCode = Utility.TypeSerializerUtils.GetTypeCode(keyType);
                            writeValueFn = JsonTypeSerializer.GetValueTypeToStringMethod(valueTypeCode);
                        }

                        if (!WriteObjectValue(dictionaryValue, writeValueFn, valueType, valueTypeCode))
                        {
                            return false;
                        }
                    }
                    ranOnce = true;
                }
            }
            finally
            {
                WriteEndObject();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeGenericDictionary(IDictionary values, Type type)
        {
            if (values.Count == 0)
            {
                WriteStartObject();
                WriteEndObject();
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
            var keyCodeType = TypeSerializerUtils.GetTypeCode(args[0]);
            if (keyCodeType != TypeSerializerUtils.TypeCode.String)
            {
                return false;
            }

            //get value type
            type = args[1];
            var valueCodeType = TypeSerializerUtils.GetTypeCode(type);
            return SerializeGenericDictionaryInternal(values, valueCodeType, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeValueMemberInfo(object instance, IValue[] items)
        {
            WriteStartObject();
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.

            int len = items.Length;
            try
            {
                for (var i = 0; i < len; i++)
                {
                    IValue item = items[i];
                    var value = item.GetValue(instance);
                    if (value != null || !JSON.Options.ShouldExcludeNulls)
                    {
                        WritePropertyName(item.NameChar);

                        if (!WriteObjectValue(value, item.WriteObject, item.ValueType, item.Code))
                        {
                            return false;
                        }
                    }
                }
            }
            finally
            {
                WriteEndObject();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WriteObjectValue(object value, WriteObjectDelegate writeValueFn, Type type, TypeSerializerUtils.TypeCode valueTypeCode)
        {
            if (value == null)
            {
                WriteNull();
                return true;
            }
            else if (writeValueFn != null)
            {
                writeValueFn(_writer, value);
                return true;
            }
            else
            {
                return SerializeNonPrimitiveValue(value, type, valueTypeCode);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeGenericDictionaryInternal(IDictionary values, TypeSerializerUtils.TypeCode valueCodeType, Type valueType)
        {

            WriteObjectDelegate writeValue = JsonTypeSerializer.GetValueTypeToStringMethod(valueCodeType);

            WriteStartObject();
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
            IDictionaryEnumerator e = values.GetEnumerator();
            try
            {
                while (e.MoveNext())
                {
                    DictionaryEntry entry = e.Entry;

                    string name = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                    WritePropertyName(name);
                    var value = entry.Value;

                    if (!WriteObjectValue(value, writeValue, valueType, valueCodeType))
                    {
                        return false;
                    }
                }
            }
            finally
            {
                WriteEndObject();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeDataSet(System.Data.DataSet ds)
        {
            WriteStartObject();
            try
            {
                foreach (System.Data.DataTable table in ds.Tables)
                {
                    SerializeDataTableData(table);
                }
                // end dataset
            }
            finally
            {
                WriteEndObject();
            }

            return true;
        }

        private void SerializeDataTableData(System.Data.DataTable table)
        {
            var rows = table.Rows;
            if (rows.Count == 0)
            {
                return;
            }
            WritePropertyName(table.TableName);
            WriteStartArray();

            try
            {
                System.Data.DataColumnCollection cols = table.Columns;
                bool rowseparator = false;
                foreach (System.Data.DataRow row in rows)
                {
                    if (rowseparator)
                    {
                        _writer.Write(',');
                    }
                    rowseparator = true;
                    WriteStartObject();
                    try
                    {
                        var columnType = new Dictionary<System.Data.DataColumn, Tuple<char[], TypeSerializerUtils.TypeCode, WriteObjectDelegate, Type>>();
                        foreach (System.Data.DataColumn column in cols)
                        {
                            var typeCode = TypeSerializerUtils.GetTypeCode(column.DataType);
                            columnType.Add(column, new Tuple<char[], TypeSerializerUtils.TypeCode, WriteObjectDelegate, Type>(StringExtension.GetEncodeString(column.ColumnName), typeCode, JsonTypeSerializer.GetValueTypeToStringMethod(typeCode), column.DataType));
                        }

                        foreach (var column in columnType)
                        {
                            //build column name
                            WritePropertyName(column.Value.Item1);
                            //build column data
                            var value = row[column.Key];

                            if (!WriteObjectValue(value, column.Value.Item3, column.Value.Item4, column.Value.Item2))
                            {
                                return;
                            }

                        }
                    }
                    finally
                    {
                        WriteEndObject();
                    }
                }
            }
            finally
            {
                WriteEndArray();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeDataTable(System.Data.DataTable dt)
        {
            WriteStartObject();
            try
            {
                SerializeDataTableData(dt);
            }
            finally
            {
                WriteEndObject();
            }
            // end datatable

            return true;
        }

        private bool SerializeNonPrimitiveValue(object value, Type type, TypeSerializerUtils.TypeCode objectTypeCode)
        {
            //this prevents recursion
            int i = 0;
            if (!_cirobj.TryGetValue(value, out i))
            {
                _cirobj.Add(value, _cirobj.Count + 1);
            }
            else if (_currentDepth > 0)
            {
                //_circular = true;
                WriteNull();
                return true;
            }
            _currentDepth++;
            //recursion limit or max char length
            if (_currentDepth >= JSON.Options.RecursionLimit) //|| _builder.Length > _currentJsonSetting.MaxJsonLength)
            {
                _currentDepth--;
                WriteNull();
                return true;
            }

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
                            return SerializeList((IList)value);
                        }
                    case TypeSerializerUtils.TypeCode.Enumerable:
                        {
                            return this.SerializeEnumerable((IEnumerable)value);
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
                            return this.SerializeNameValueCollection((System.Collections.Specialized.NameValueCollection)value);
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
                            IValue[] obj = null;
                            if (s_currentJsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, type, out obj))
                            {
                                return this.SerializeValueMemberInfo(value, obj);
                            }
                            else
                            {
                                WriteNull();
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
