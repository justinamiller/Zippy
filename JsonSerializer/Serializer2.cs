using JsonSerializer.Internal;
using JsonSerializer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using static JsonSerializer.Utility.ConvertUtils;
using System.IO;

namespace JsonSerializer
{
    /// <summary>
    /// Json Serializer.
    /// </summary>
    /// <remarks>guide from http://www.json.org/ </remarks>
    public sealed class Serializer2
    {
        private static IJsonSerializerStrategy s_currentJsonSerializerStrategy;

        private static IJsonSerializerStrategy s_defaultJsonSerializerStrategy;

        // The following logic performs circular reference detection
        private readonly Hashtable _serializeStack = new Hashtable(new ReferenceComparer());//new HashSet<object>();
        private readonly Dictionary<object, int> _cirobj = new Dictionary<object, int>();
        private int _currentDepth = 0;
        private TextWriter _writer;
        private bool _propertyInUse;
        private int _arrayIndex = 0;
        private int _objectIndex = 0;

        internal static IJsonSerializerStrategy CurrentJsonSerializerStrategy
        {
            get
            {
                IJsonSerializerStrategy jsonSerializerStrategy = s_currentJsonSerializerStrategy;
                if (jsonSerializerStrategy == null)
                {
                    jsonSerializerStrategy = DefaultJsonSerializerStrategy;
                    s_currentJsonSerializerStrategy = jsonSerializerStrategy;
                }
                return jsonSerializerStrategy;
            }
            set
            {
                s_currentJsonSerializerStrategy = value;
            }
        }

        internal static IJsonSerializerStrategy DefaultJsonSerializerStrategy
        {
            get
            {
                if (s_defaultJsonSerializerStrategy == null)
                {
                    s_defaultJsonSerializerStrategy = new CachedLambdaJsonSerializerStrategy();
                    //   s_defaultJsonSerializerStrategy = new DelegateJsonSerializerStrategy();
                }
                return s_defaultJsonSerializerStrategy;
            }
        }

        public Serializer2()
        {
   //         this._writer = new StringWriter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndObject()
        {
            this._propertyInUse = true;
            this._writer.Write('}');
            _objectIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndArray()
        {
            this._writer.Write(']');
            _arrayIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartArray()
        {
            this._writer.Write('[');
            _arrayIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartObject()
        {
            this._writer.Write('{');
            this._propertyInUse = false;
            _objectIndex++;
        }

        private void WritePropertyName(string value)
        {
            WritePropertyName(StringExtension.GetEncodeString(value));
        }

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

            _writer.Write(value, 0, value.Length);
            _writer.Write(':');
        }

        private readonly static JsonTypeSerializer _jsonWriter = new JsonTypeSerializer();

        private bool SerializeNameValueCollection(System.Collections.Specialized.NameValueCollection value)
        {
            WriteStartObject();
            try
            {
                string[] keys = value.AllKeys;

                for (int i = 0; i < keys.Length; i++)
                {
                    WritePropertyName(keys[i]);
                    _jsonWriter.WriteRawString(_writer, StringExtension.GetEncodeString(value.Get(i)));
                }
            }
            finally
            {
                WriteEndObject();
            }

            return true;
        }


       
        private bool SerializeEnumerable(IEnumerable anEnumerable)
        {
            ConvertUtils.TypeCode valueType = ConvertUtils.TypeCode.Empty;
            WriteObjectDelegate writeObject = null;
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
                        writeObject = FastJsonWriter.GetValueTypeToStringMethod(valueType);
                        isTyped = valueType != ConvertUtils.TypeCode.Custom;

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
                            valueType = GetTypeCode(value);
                            writeObject = FastJsonWriter.GetValueTypeToStringMethod(valueType);
                        }

                        if (!WriteObjectValue(value, writeObject, valueType))
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

        private bool SerializeArray(Array array)
        {

            if (array.Rank > 1)
            {
                return SerializeMultidimensionalArray(array);
            }

            ConvertUtils.TypeCode valueType = ConvertUtils.TypeCode.Empty;
            WriteObjectDelegate writeObject = null;
            bool flag1 = true;
            WriteStartArray();
            try
            {

                // note that an error in the IEnumerable won't be caught
                for (var i = 0; i < array.Length; i++)
                {
                    var value = array.GetValue(i);
                    if (!flag1)
                    {
                        _writer.Write(',');
                    }
                    else
                    {
                        //first record.
                        valueType = GetEnumerableValueTypeCode((IEnumerable)array);
                        writeObject = FastJsonWriter.GetValueTypeToStringMethod(valueType);
                        flag1 = false;
                    }

                    if(!WriteObjectValue(value, writeObject, valueType))
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


      
        private bool SerializeList(System.Collections.IList list)
        {
            ConvertUtils.TypeCode valueTypeCode = ConvertUtils.TypeCode.Empty;
            WriteObjectDelegate writeObject = null;
            bool flag1 = true;
            Type lastType=null;

            WriteStartArray();
            try
            {

                // note that an error in the IEnumerable won't be caught
                for (var i = 0; i < list.Count; i++)
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
                        Type valueType = value.GetType();
                        if(lastType!= valueType)
                        {
                            lastType = valueType;
                            valueTypeCode = GetTypeCode(valueType);
                            writeObject = FastJsonWriter.GetValueTypeToStringMethod(valueTypeCode);
                        }

                        if(!WriteObjectValue(value, writeObject, valueTypeCode))
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

        private bool SerializeMultidimensionalArray(Array values)
        {
            return SerializeMultidimensionalArray(values, Array.Empty<int>());
        }

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
                ConvertUtils.TypeCode valueTypeCode = ConvertUtils.TypeCode.Empty;

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
                            valueTypeCode = Utility.ConvertUtils.GetTypeCode(typeCode);
                            writeValueFn = FastJsonWriter.GetValueTypeToStringMethod(valueTypeCode);
                        }
                        if (!WriteObjectValue(value, writeValueFn, valueTypeCode))
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

        //string builder
        //private JsonWriter _builder = null;
        private string SerializeObjectInternal(object json)
        {
            var typeCode = ConvertUtils.GetTypeCode(json.GetType());
            if (typeCode >= ConvertUtils.TypeCode.NotSetObject)
            {
                //handle for object
                _writer = StringWriterThreadStatic.Allocate();
                SerializeNonPrimitiveValue(json, typeCode);
                return StringWriterThreadStatic.ReturnAndFree((StringWriter)_writer);
            }
            else
            {   
                var writer = StringWriterThreadStatic.Allocate();
                FastJsonWriter.GetValueTypeToStringMethod(typeCode)?.Invoke(writer, json);
                return StringWriterThreadStatic.ReturnAndFree(writer);
            }
        }

        /// <summary>
        /// use default settings
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging should not affect program behavior.")]
        public static string SerializeObject(object Object)
        {
            if (Object == null)
            {
                return null;
            }

            return new Serializer2().SerializeObjectInternal(Object);
        }

        private bool SerializeNonGenericDictionary(IDictionary values)
        {
            WriteObjectDelegate writeKeyFn = null;
            WriteObjectDelegate writeValueFn = null;

            ConvertUtils.TypeCode keyTypeCode = ConvertUtils.TypeCode.Empty;
            ConvertUtils.TypeCode valueTypeCode = ConvertUtils.TypeCode.Empty;

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
                        keyTypeCode = Utility.ConvertUtils.GetTypeCode(keyType);
                        writeKeyFn = FastJsonWriter.GetValueTypeToStringMethod(keyTypeCode);
                    }

                    if (ranOnce)
                    {
                        _writer.Write(',');
                    }

                    WriteObjectValue(key, writeKeyFn, keyTypeCode);
                    _writer.Write(':');

                    if (dictionaryValue == null)
                    {
                        WriteNull();
                    }
                    else
                    {
                        var valueType = dictionaryValue.GetType();
                        if (writeValueFn == null || lastValueType != valueType)
                        {
                            lastValueType = valueType;
                            valueTypeCode = Utility.ConvertUtils.GetTypeCode(keyType);
                            writeValueFn = FastJsonWriter.GetValueTypeToStringMethod(valueTypeCode);
                        }

                        if (!WriteObjectValue(dictionaryValue, writeValueFn, valueTypeCode))
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


        private bool SerializeGenericDictionary(IDictionary values)
        {
            if (values.Count == 0)
            {
                WriteStartObject();
                WriteEndObject();
                return true;
            }
            //check if key is string type
            Type type = values.GetType();
            Type[] args = type.GetGenericArguments();

            if (args.Length == 0)
            {
                //System.Collections.IDictionary
                return SerializeNonGenericDictionary(values);
            }

            //System.Collections.Generic.IDictionary
            var keyCodeType = ConvertUtils.GetTypeCode(args[0]);
            if (keyCodeType != ConvertUtils.TypeCode.String)
            {
                return false;
            }
            var valueCodeType = ConvertUtils.GetTypeCode(args[1]);
            return SerializeGenericDictionaryInternal(values, valueCodeType);
        }


        private bool SerializeValueMemberInfo(object instance, ValueMemberInfo[] items)
        {
            WriteStartObject();
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
            try
            {
             for(var i=0; i<items.Length; i++)
                {
                    var item = items[i];
                    WritePropertyName(item.NameChar);
                    var value = item.GetValue(instance);

                   if(!WriteObjectValue(value, item.WriteObject, item.Code))
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
        private bool WriteObjectValue(object value, WriteObjectDelegate writeValueFn, ConvertUtils.TypeCode valueTypeCode)
        {
            if (value == null)
            {
                WriteNull();
                return true;
            }
            else if (writeValueFn  != null)
            {
                writeValueFn(_writer, value);
                            return true;
            }
            else 
            {
                return SerializeNonPrimitiveValue(value, valueTypeCode);
            }
        }

        private bool SerializeGenericDictionaryInternal(IDictionary values, ConvertUtils.TypeCode valueCodeType)
        {

            WriteObjectDelegate writeValue = FastJsonWriter.GetValueTypeToStringMethod(valueCodeType);

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

                    if (!WriteObjectValue(value, writeValue, valueCodeType))
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
                    var columnType = new Dictionary<System.Data.DataColumn, Tuple<char[], ConvertUtils.TypeCode, WriteObjectDelegate>>();
                    foreach(System.Data.DataColumn column in cols)
                    {
                        var typeCode = ConvertUtils.GetTypeCode(column.DataType);
                        columnType.Add(column, new Tuple<char[], ConvertUtils.TypeCode, WriteObjectDelegate>(StringExtension.GetEncodeString(column.ColumnName), typeCode, FastJsonWriter.GetValueTypeToStringMethod(typeCode)));
                    }

                    foreach (var column in columnType)
                    {
                        //build column name
                        WritePropertyName(column.Value.Item1);
                        //build column data
                        var value = row[column.Key];

                        if(!WriteObjectValue(value, column.Value.Item3, column.Value.Item2))
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


        private bool SerializeNonPrimitiveValue(object value, ConvertUtils.TypeCode objectTypeCode)
        {
            //this prevents recursion
            int i = 0;
            if (!_cirobj.TryGetValue(value, out i))
                _cirobj.Add(value, _cirobj.Count + 1);
            else
            {
                if (_currentDepth > 0)
                {
                    //_circular = true;
                    return false;
                }
            }
            _currentDepth++;
            try
            {

                //recursion limit or max char length
                if (_currentDepth >= JsonSerializerSetting.Current.RecursionLimit) //|| _builder.Length > _currentJsonSetting.MaxJsonLength)
                {
                    WriteNull();
                    return false;
                }

                //try
                //{
                switch (objectTypeCode)
                {
                    case ConvertUtils.TypeCode.Array:
                        {
                            return SerializeArray((Array)value);
                        }
                    case ConvertUtils.TypeCode.IList:
                        {
                            return SerializeList((IList)value);
                        }
                    case ConvertUtils.TypeCode.Enumerable:
                        {
                            return this.SerializeEnumerable((IEnumerable)value);
                        }
                    case ConvertUtils.TypeCode.Dictionary:
                        return SerializeNonGenericDictionary((IDictionary)value);
                    case ConvertUtils.TypeCode.GenericDictionary:
                        return SerializeGenericDictionary((IDictionary)value);
                    case ConvertUtils.TypeCode.NameValueCollection:
                        return this.SerializeNameValueCollection((System.Collections.Specialized.NameValueCollection)value);
                    case ConvertUtils.TypeCode.DataSet:
                        return SerializeDataSet((System.Data.DataSet)value);
                    case ConvertUtils.TypeCode.DataTable:
                        return SerializeDataTable((System.Data.DataTable)value);
                    case ConvertUtils.TypeCode.IJsonSerializeImplementation:
                        {
                            // handles it's own serialization.
                            _jsonWriter.WriteRawString(_writer, ((IJsonSerializeImplementation)value).SerializeAsJson());
                            return true;
                        }
                    default:
                        {
                            ValueMemberInfo[] obj = null;
                            // if (CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out obj))
                            if (CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObjectImproved(value, out obj))
                            {
                                return this.SerializeValueMemberInfo(value, obj);
                            }
                            else
                            {
                                WriteNull();
                                return true;//was false but when false prevent continuing.
                            }
                        }
                }
                //}
                //catch (Exception)
                //{
                //    _builder.WriteNull();
                //    return true;//was false but when false prevent continuing.
                //}

                //finally
                //{
                //    RemoveObjectAsReferenceCheck(value);
                //}
            }
            finally
            {
                _currentDepth--;
            }
        }

        /// <summary>
        /// check if this object was already serialized within it's object path.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AddObjectAsReferenceCheck(object value)
        {
            if (this._serializeStack.ContainsKey(value))
            {
                //Self referencing loop detected;
                WriteNull();
                return false;
            }

            // Add the object to the _serializeStack
            _serializeStack.Add(value, null);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveObjectAsReferenceCheck(object value)
        {
            this._serializeStack.Remove(value);
        }

        /// <summary>
        ///We use this for our cycle detection for the case where objects override equals/gethashcode
        /// </summary>
        private class ReferenceComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
            {
                return x == y;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
