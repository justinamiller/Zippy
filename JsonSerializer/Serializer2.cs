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

        private static IJsonSerializerSetting s_defaultJsonSetting;

        private IJsonSerializerSetting _currentJsonSetting;
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


        public IJsonSerializerSetting CurrentJsonSetting
        {
            get
            {
                IJsonSerializerSetting setting = _currentJsonSetting;
                if (setting == null)
                {
                    setting = DefaultJsonSetting;
                    _currentJsonSetting = setting;
                }
                return setting;
            }
            set
            {
                if (value == null)
                {
                    _currentJsonSetting = DefaultJsonSetting;
                }
                else
                {
                    _currentJsonSetting = value;
                }
            }
        }


        public static IJsonSerializerSetting DefaultJsonSetting
        {
            get
            {
                IJsonSerializerSetting defaultJsonSetting = s_defaultJsonSetting;
                if (defaultJsonSetting == null)
                {
                    IJsonSerializerSetting defaultJsonSetting1 = new JsonSerializerSetting();
                    s_defaultJsonSetting = defaultJsonSetting1;
                    defaultJsonSetting = defaultJsonSetting1;
                }
                return defaultJsonSetting;
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
            this.CurrentJsonSetting = DefaultJsonSetting;
            this._writer = new StringWriter();
        }

        public Serializer2(IJsonSerializerSetting setting)
        {
            this.CurrentJsonSetting = setting ?? DefaultJsonSetting;
            this._writer = new StringWriter();
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

        [MethodImpl(MethodImplOptions.NoInlining)]
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


        [MethodImpl(MethodImplOptions.NoInlining)]
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

                        if (writeObject != null)
                        {
                            writeObject(_writer, value);
                        }
                        //will require more reflection
                        else if (!this.SerializeNonPrimitiveValue(value, valueType))
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

        [MethodImpl(MethodImplOptions.NoInlining)]
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

                    if (value == null)
                    {
                        WriteNull();
                    }
                    else if (writeObject != null)
                    {
                        writeObject(_writer, value);
                    }
                    //will require more reflection
                    else if (!this.SerializeNonPrimitiveValue(value, valueType))
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


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeList(System.Collections.IList list)
        {
            ConvertUtils.TypeCode valueType = ConvertUtils.TypeCode.Empty;
            WriteObjectDelegate writeObject = null;
            bool flag1 = true;
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
                        valueType = GetTypeCode(value.GetType());
                        writeObject = FastJsonWriter.GetValueTypeToStringMethod(valueType);

                        if (writeObject != null)
                        {
                            writeObject(_writer, value);
                        }
                        //will require more reflection
                        else if (!this.SerializeNonPrimitiveValue(value, valueType))
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


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeMultidimensionalArray(Array values)
        {
            return SerializeMultidimensionalArray(values, Array.Empty<int>());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
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
                        if (!this.SerializeValue(value, ConvertUtils.GetTypeCode(value.GetType())))
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        private string SerializeObjectInternal(object json)
        {
            if (json == null)
            {
                return null;
            }

            var typeCode = ConvertUtils.GetTypeCode(json.GetType());
            if (typeCode >= ConvertUtils.TypeCode.NotSetObject)
            {
                //handle for object
                SerializeNonPrimitiveValue(json, typeCode);
            }
            else
            {
                FastJsonWriter.GetValueTypeToStringMethod(typeCode)?.Invoke(_writer, json);
            }

            return _writer.ToString();
        }

        /// <summary>
        /// use default settings
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging should not affect program behavior.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string SerializeObject(object Object)
        {
            return new Serializer2().SerializeObjectInternal(Object);
        }

        /// <summary>
        /// use your settings
        /// </summary>
        /// <param name="json"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging should not affect program behavior.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string SerializeObject(object Object, IJsonSerializerSetting settings)
        {
            return new Serializer2(settings).SerializeObjectInternal(Object);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDictionary(IDictionary values)
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
            var keyCodeType = ConvertUtils.GetTypeCode(args[0]);
            if (keyCodeType != ConvertUtils.TypeCode.String)
            {
                return false;
            }
            var valueCodeType = ConvertUtils.GetTypeCode(args[1]);
            return SerializeDictionaryInternal(values, valueCodeType);
        }


        private bool SerializeValueMemberInfo(object instance, IList<ValueMemberInfo> items)
        {
            WriteStartObject();
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
            try
            {
                foreach (var item in items)
                {
                    WritePropertyName(item.NameChar);
                    var value = item.GetValue(instance);
                    if (value == null)
                    {
                        WriteNull();
                    }
                    else if (item.WriteObject != null)
                    {
                        item.WriteObject(_writer, value);
                    }
                    else if (!SerializeNonPrimitiveValue(value, item.Code))
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



        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDictionaryInternal(IDictionary values, ConvertUtils.TypeCode valueCodeType)
        {
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
                    if (value == null)
                    {
                        WriteNull();
                    }
                    else if (valueCodeType >= ConvertUtils.TypeCode.NotSetObject)
                    {
                        if (!this.SerializeNonPrimitiveValue(value, valueCodeType))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //short cut.
                        FastJsonWriter.GetValueTypeToStringMethod(valueCodeType)?.Invoke(_writer, value);
                    }
                }
            }
            finally
            {
                WriteEndObject();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
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

        [MethodImpl(MethodImplOptions.NoInlining)]
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

                    foreach (System.Data.DataColumn column in cols)
                    {
                        //build column name
                        WritePropertyName(column.ColumnName);
                        //build column data
                        SerializeValue(row[column]);
                    }
                    WriteEndObject();
                }
            }
            finally
            {
                WriteEndArray();
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
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


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeValue(object value, ConvertUtils.TypeCode typeCode = ConvertUtils.TypeCode.Empty)
        {
            //prevent null
            if (value == null)
            {
                WriteNull();
                return true;
            }

            if (typeCode == ConvertUtils.TypeCode.Empty)
            {
                typeCode = ConvertUtils.GetTypeCode(value);
            }

            if (typeCode >= ConvertUtils.TypeCode.NotSetObject)
            {
                //handle for object
                return SerializeNonPrimitiveValue(value, typeCode);
            }
            else
            {
                FastJsonWriter.GetValueTypeToStringMethod(typeCode)?.Invoke(_writer, value);
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
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
                if (_currentDepth >= _currentJsonSetting.RecursionLimit) //|| _builder.Length > _currentJsonSetting.MaxJsonLength)
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
                        return SerializeDictionary((IDictionary)value);
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
                            IList<ValueMemberInfo> obj = null;
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
