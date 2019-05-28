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

namespace JsonSerializer
{
    /// <summary>
    /// Json Serializer.
    /// </summary>
    /// <remarks>guide from http://www.json.org/ </remarks>
    public sealed class Serializer
    {
        private static IJsonSerializerStrategy s_currentJsonSerializerStrategy;

        private static IJsonSerializerStrategy s_defaultJsonSerializerStrategy;

        private static IJsonSerializerSetting s_defaultJsonSetting;

        private IJsonSerializerSetting _currentJsonSetting;
        // The following logic performs circular reference detection
        private readonly Hashtable _serializeStack = new Hashtable(new ReferenceComparer());//new HashSet<object>();

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

        public Serializer()
        {
            this.CurrentJsonSetting = DefaultJsonSetting;
        }

        public Serializer(IJsonSerializerSetting setting)
        {
            this.CurrentJsonSetting = setting ?? DefaultJsonSetting;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeNameValueCollection(System.Collections.Specialized.NameValueCollection value, int recursiveCount)
        {
            _builder.WriteStartObject();
                string[] keys = value.AllKeys;

                for (int i = 0; i < keys.Length; i++)
                {
                    _builder.WriteProperty(keys[i], value.Get(i));
                }
            return true;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeEnumerable(IEnumerable anEnumerable, int recursiveCount)
        {
            var anArray = anEnumerable as Array;
            if (anArray != null && anArray.Rank > 1)
            {
                return SerializeMultidimensionalArray(anArray, recursiveCount);
            }

            bool flag1 = true;
            _builder.WriteStartArray();
            try
            {
                ConvertUtils.TypeCode valueType = ConvertUtils.TypeCode.Empty;
                // note that an error in the IEnumerable won't be caught
                foreach (object value in anEnumerable)
                {
                   if (!flag1)
                    {
                        _builder.WriteComma();
                    }
                    else
                    {
                        //first record.
                        valueType = GetEnumerableValueTypeCode(anEnumerable);
                    }

                    if (value == null)
                    {
                        _builder.WriteNull();
                        flag1 = false;
                    }
                    else if (valueType >= ConvertUtils.TypeCode.NotSetObject)
                    {
                        //will require more reflection
                        if (this.SerializeValue(value, recursiveCount))
                        {
                            flag1 = false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //shortcut 
                        _builder.WriteObjectValue(value, valueType);
                        flag1 = false;
                    }
                }
            }
            finally
            {
                _builder.WriteEndArray();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeMultidimensionalArray(Array values, int recursiveCount)
        {
              return SerializeMultidimensionalArray(values, recursiveCount, Array.Empty<int>());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeMultidimensionalArray(Array values, int recursiveCount, int[] indices)
        {
            bool flag = true;
            int dimension = indices.Length;
            int[] newIndices = new int[dimension + 1];
            for (int i = 0; i < dimension; i++)
            {
                newIndices[i] = indices[i];
            }

            _builder.WriteStartArray();
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
                            _builder.WriteComma();
                        }
                        if (!this.SerializeValue(value, recursiveCount))
                        {
                            return false;
                        }

                        flag = false;
                    }
                    else
                    {
                        if (i != 0)
                        {
                            _builder.WriteComma();
                        }

                        SerializeMultidimensionalArray(values, recursiveCount, newIndices);
                    }
                }
            }
            finally
            {
                _builder.WriteEndArray();
            }
            return true;
        }

        //string builder
        private JsonWriter _builder = null;
        [MethodImpl(MethodImplOptions.NoInlining)]
        private string SerializeObjectInternal(object json)
        {
            _builder = new JsonWriter()
            {
                IsElasticSearchReady = this.CurrentJsonSetting.IsElasticSearchReady
            };

            if (!this.SerializeValue(json, 0))
            {
                return null;
            }

            return _builder.ToString();
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
            return new Serializer().SerializeObjectInternal(Object);
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
            return new Serializer(settings).SerializeObjectInternal(Object);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDictionary(IDictionary values, int recursiveCount)
        {
            //check if key is string type
            Type type = values.GetType();
            var keyCodeType = ConvertUtils.GetTypeCode(type.GetGenericArguments()[0]);
            if (keyCodeType != ConvertUtils.TypeCode.String)
            {
                return false;
            }
            var valueCodeType = ConvertUtils.GetTypeCode(type.GetGenericArguments()[1]);
            return SerializeDictionaryInternal(values, recursiveCount, valueCodeType);
        }


        private bool SerializeValueMemberInfo(object instance, IList<ValueMemberInfo> items, int recursiveCount)
        {
            _builder.WriteStartObject();
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
            try
            {
                foreach(var item in items)
                {
                    _builder.WritePropertyName(item.Name,false);

                    var value = item.GetValue(instance);
          
                    if(value == null)
                    {
                        _builder.WriteNull();
                    }
                    else if (item.Code >= ConvertUtils.TypeCode.NotSetObject)
                    {
                        if (!this.SerializeValue(value, recursiveCount, item.Code))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //short cut.
                        _builder.WriteObjectValue(value, item.Code);
                    }
                }
            }
            finally
            {
                _builder.WriteEndObject();
            }

            return true;
        }



        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDictionaryInternal(IDictionary values, int recursiveCount, ConvertUtils.TypeCode valueCodeType)
        {
            _builder.WriteStartObject();
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
            IDictionaryEnumerator e = values.GetEnumerator();
            try
            {
                while (e.MoveNext())
                {
                    DictionaryEntry entry = e.Entry;

                    string name = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                    _builder.WritePropertyName(name);
                    var value = entry.Value;
                    if (value == null)
                    {
                        _builder.WriteNull();
                    }
                    else if (valueCodeType > ConvertUtils.TypeCode.Enumerable)
                    {
                        if (!this.SerializeValue(value, recursiveCount))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //short cut.
                        _builder.WriteObjectValue(value, valueCodeType);
                    }
                }
            }
            finally
            {
                _builder.WriteEndObject();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDataset(System.Data.DataSet ds, int recursiveCount)
        {
            _builder.WriteStartObject();
            try
            {

                foreach (System.Data.DataTable table in ds.Tables)
                {
                    SerializeDataTableData(table, recursiveCount);
                }
                // end dataset
            }
            finally
            {
                _builder.WriteEndObject();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SerializeDataTableData(System.Data.DataTable table, int recursiveCount)
        {
            _builder.WritePropertyName(table.TableName);
            _builder.WriteStartArray();

            try
            {
                var rows = table.Rows;
                if (rows.Count == 0)
                {
                    return;
                }

                System.Data.DataColumnCollection cols = table.Columns;
                bool rowseparator = false;
                foreach (System.Data.DataRow row in rows)
                {
                    if (rowseparator)
                    {
                        _builder.WriteComma();
                    }
                    rowseparator = true;
                    _builder.WriteStartObject();

                    foreach (System.Data.DataColumn column in cols)
                    {
                        //build column name
                        _builder.WritePropertyName(column.ColumnName);
                        //build column data
                        SerializeValue(row[column], recursiveCount);
                    }
                    _builder.WriteEndObject();
                }
            }
            finally
            {
                _builder.WriteEndArray();
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDataTable(System.Data.DataTable dt, int recursiveCount)
        {
            _builder.WriteStartObject();
            try
            {
                SerializeDataTableData(dt, recursiveCount);
            }
            finally
            {
                _builder.WriteEndObject();
            }
            // end datatable


            return true;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeValue(object value, int recursiveCount, ConvertUtils.TypeCode typeCode= ConvertUtils.TypeCode.Empty )
        {
            //prevent null
            if (value == null)
            {
                _builder.WriteNull();
                return true;
            }

            recursiveCount += 1;
            //recursion limit or max char length
            if (recursiveCount >= _currentJsonSetting.RecursionLimit || _builder.Length > _currentJsonSetting.MaxJsonLength)
            {
                _builder.WriteNull();
                return true;
            }

            if(typeCode == ConvertUtils.TypeCode.Empty)
            {
                typeCode = ConvertUtils.GetTypeCode(value);
            }
           
            if (typeCode >= ConvertUtils.TypeCode.NotSetObject)
            {
                //handle for object
                return SerializeNonPrimitiveValue(value, recursiveCount, typeCode);
            }
            else
            {
                _builder.WriteObjectValue(value, typeCode);
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeNonPrimitiveValue(object value, int recursiveCount, ConvertUtils.TypeCode objectTypeCode)
        {
            //this prevents recursion
            if (!AddObjectAsReferenceCheck(value))
                return true;

            try
            {
                switch (objectTypeCode)
                {
                    case ConvertUtils.TypeCode.Enumerable:
                        return this.SerializeEnumerable((IEnumerable)value, recursiveCount);
                    case ConvertUtils.TypeCode.Dictionary:
                        return SerializeDictionary((IDictionary)value, recursiveCount);
                    case ConvertUtils.TypeCode.NameValueCollection:
                        return this.SerializeNameValueCollection((System.Collections.Specialized.NameValueCollection)value, recursiveCount);
                    case ConvertUtils.TypeCode.DataSet:
                        return SerializeDataset((System.Data.DataSet)value, recursiveCount);
                    case ConvertUtils.TypeCode.DataTable:
                        return SerializeDataTable((System.Data.DataTable)value, recursiveCount);
                    case ConvertUtils.TypeCode.IJsonSerializeImplementation:
                        {
                            // handles it's own serialization.
                            _builder.WriteRawValue(((IJsonSerializeImplementation)value).SerializeAsJson(), true);
                            return true;
                        }
                    default:
                        {
                            IList<ValueMemberInfo> obj = null;
                            // if (CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out obj))
                            if (CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObjectImproved(value, out obj))
                            {
                                //add flag?
                                //return   this.SerializeDictionaryInternal((IDictionary)obj, recursiveCount, ConvertUtils.PrimitiveTypeCode.Object);
                                return this.SerializeValueMemberInfo(value, obj, recursiveCount);
                            }
                            else
                            {
                                _builder.WriteNull();
                                return true;//was false but when false prevent continuing.
                            }
                        }
                }
            }
            catch (Exception)
            {
                _builder.WriteNull();
                return true;//was false but when false prevent continuing.
            }

            finally
            {
                RemoveObjectAsReferenceCheck(value);
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
            if (value == null)
            {
                return false;
            }


            if (this._serializeStack.ContainsKey(value))
            {
                //Self referencing loop detected;
                _builder.WriteNull();
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
