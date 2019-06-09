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
        // The following logic performs circular reference detection
       private readonly Hashtable _serializeStack = new Hashtable(new ReferenceComparer());//new HashSet<object>();
        private readonly Dictionary<object, int> _cirobj = new Dictionary<object, int>();
        private int _currentDepth = 0;

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

        public Serializer()
        {
        }

        private bool SerializeNameValueCollection(System.Collections.Specialized.NameValueCollection value)
        {
            _builder.WriteStartObject();
            try
            {
                string[] keys = value.AllKeys;

                for (int i = 0; i < keys.Length; i++)
                {
                    _builder.WriteProperty(keys[i], value.Get(i));
                }
            }
            finally
            {
                _builder.WriteEndObject();
            }

            return true;
        }


        private bool SerializeEnumerable(IEnumerable anEnumerable)
        {
            var anArray = anEnumerable as Array;
            if (anArray != null && anArray.Rank > 1)
            {
                return SerializeMultidimensionalArray(anArray);
            }

            ConvertUtils.TypeCode valueType = ConvertUtils.TypeCode.Empty;
            bool flag1 = true;
            _builder.WriteStartArray();
            try
            {
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
                        flag1 = false;
                    }

                    if (value == null)
                    {
                        _builder.WriteNull();
                    }
                    else if (valueType >= ConvertUtils.TypeCode.NotSetObject)
                    {
                        //will require more reflection
                       if (!this.SerializeValue(value, valueType))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //shortcut 
                        _builder.WriteObjectValue(value, valueType);
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
                            _builder.WriteComma();
                        }

                        SerializeMultidimensionalArray(values, newIndices);
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
       //private JsonWriter _builder = null;
       private JsWriter _builder = null;
        [MethodImpl(MethodImplOptions.NoInlining)]
        private string SerializeObjectInternal(object json)
        {
            if (json == null)
            {
                return null;
            }


            _builder = new JsWriter() //new JsonTextWriter()
            //_builder = new JsonTextWriter()
            {
                IsElasticSearchReady = JsonSerializerSetting.Current.IsElasticSearchReady
            };
            var typeCode = ConvertUtils.GetTypeCode(json.GetType());
            if (typeCode >= ConvertUtils.TypeCode.NotSetObject)
            {
                //handle for object
                SerializeNonPrimitiveValue(json, typeCode);
            }
            else
            {
                _builder.WriteObjectValue(json, typeCode);
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
        //[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging should not affect program behavior.")]
        //[MethodImpl(MethodImplOptions.NoInlining)]
        //public static string SerializeObject(object Object, IJsonSerializerSetting settings)
        //{
        //    return new Serializer(settings).SerializeObjectInternal(Object);
        //}



        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDictionary(IDictionary values)
        {
            if (values.Count == 0)
            {
                _builder.WriteStartObject();
                _builder.WriteEndObject();
                return true;
            }
            //check if key is string type
            Type type = values.GetType();
            Type[] args = type.GetGenericArguments();

            ConvertUtils.TypeCode keyCodeType = ConvertUtils.TypeCode.String;
            ConvertUtils.TypeCode valueCodeType = ConvertUtils.TypeCode.String;
            if (args.Length > 0)
            {
    keyCodeType = ConvertUtils.GetTypeCode(args[0]);
                if (keyCodeType != ConvertUtils.TypeCode.String)
                {
                    return false;
                }
        valueCodeType = ConvertUtils.GetTypeCode(args[1]);
            }


            return SerializeDictionaryInternal(values, valueCodeType);
        }


        private bool SerializeValueMemberInfo(object instance, IList<ValueMemberInfo> items)
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
                     //   if (!this.SerializeValue(value, item.Code))
                     if(!SerializeNonPrimitiveValue(value, item.Code))
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
        private bool SerializeDictionaryInternal(IDictionary values, ConvertUtils.TypeCode valueCodeType)
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
                    else if (valueCodeType >= ConvertUtils.TypeCode.NotSetObject)
                    {
                        if (!this.SerializeValue(value))
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
        private bool SerializeDataSet(System.Data.DataSet ds)
        {
            _builder.WriteStartObject();
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
                _builder.WriteEndObject();
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
            _builder.WritePropertyName(table.TableName);
            _builder.WriteStartArray();

            try
            {
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
                        SerializeValue(row[column]);
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
        private bool SerializeDataTable(System.Data.DataTable dt)
        {
            _builder.WriteStartObject();
            try
            {
                SerializeDataTableData(dt);
            }
            finally
            {
                _builder.WriteEndObject();
            }
            // end datatable

            return true;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeValue(object value, ConvertUtils.TypeCode typeCode = ConvertUtils.TypeCode.Empty )
        {
            //prevent null
            if (value == null)
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
                return SerializeNonPrimitiveValue(value, typeCode);
            }
            else
            {
                _builder.WriteObjectValue(value, typeCode);
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeNonPrimitiveValue(object value, ConvertUtils.TypeCode objectTypeCode)
        {
            int i = 0;
            //this prevents recursion
            if (!_cirobj.TryGetValue(value, out i))
                _cirobj.Add(value, _cirobj.Count + 1);
            else
            {
                if (_currentDepth > 0)
                {
                    //_circular = true;
                    _builder.WriteValue("{\"$i\":");
                    _builder.WriteValue(i.ToString());
                    _builder.WriteValue("}");
                    return false;
                }
            }
            _currentDepth++;
            try
            {

            //recursion limit or max char length
            if (_currentDepth >= JsonSerializerSetting.Current.RecursionLimit) //|| _builder.Length > _currentJsonSetting.MaxJsonLength)
            {
                _builder.WriteNull();
                return true;
            }

            //if (!AddObjectAsReferenceCheck(value))
            //    return true;

            //try
            //{
            switch (objectTypeCode)
                {
                    case ConvertUtils.TypeCode.Array:
                    case ConvertUtils.TypeCode.Enumerable:
                    case ConvertUtils.TypeCode.IList:
                        return this.SerializeEnumerable((IEnumerable)value);
                    case ConvertUtils.TypeCode.Dictionary:
                    case ConvertUtils.TypeCode.GenericDictionary:
                        return SerializeDictionary((IDictionary)value);
                    case ConvertUtils.TypeCode.NameValueCollection:
                        return this.SerializeNameValueCollection((System.Collections.Specialized.NameValueCollection)value);
                    case ConvertUtils.TypeCode.DataSet:
                        return SerializeDataSet((System.Data.DataSet)value);
                    case ConvertUtils.TypeCode.DataTable:
                        return SerializeDataTable((System.Data.DataTable)value);
                    default:
                        {
                            IValue[] obj = null;
                            // if (CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out obj))
                            if (CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObjectImproved(value, value.GetType(),  out obj))
                            {
                                var list = new System.Collections.Generic.List<ValueMemberInfo>(obj.Select(p => (ValueMemberInfo)p));
          
                                return this.SerializeValueMemberInfo(value, list);
                            }
                            else
                            {
                                _builder.WriteNull();
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
