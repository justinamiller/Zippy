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

namespace JsonSerializer
{
    /// <summary>
    /// Json Serializer.
    /// </summary>
    /// <remarks>guide from http://www.json.org/ </remarks>
    public sealed class Serializer
    {
        private static readonly string[] s_Iso8601Format = new string[] { "yyyy-MM-dd\\THH:mm:ss.FFFFFFF\\Z", "yyyy-MM-dd\\THH:mm:ss\\Z", "yyyy-MM-dd\\THH:mm:ssK" };

        private static readonly CultureInfo s_defaultJsonCultureInfo = CultureInfo.InvariantCulture;

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
                IJsonSerializerStrategy defaultJsonSerializerStrategy = s_defaultJsonSerializerStrategy;
                if (defaultJsonSerializerStrategy == null)
                {
                    s_defaultJsonSerializerStrategy = new LambdaJsonSerializerStrategy();
                    //   s_defaultJsonSerializerStrategy = new DelegateJsonSerializerStrategy();
                    defaultJsonSerializerStrategy = s_defaultJsonSerializerStrategy;
                }
                return defaultJsonSerializerStrategy;
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
            if (!AddObjectAsReferenceCheck(value))
                return true;

            bool flag = true;

            _builder.WriteStartObject();
            try
            {
                string[] keys = value.AllKeys;

                for (int i = 0; i < keys.Length; i++)
                {
                    //if (!flag)
                    //{
                    //    _builder.WriteComma();
                    //}

                    _builder.WriteProperty(keys[i], value.Get(i));
                    flag = false;
                }
            }
            finally
            {
                RemoveObjectAsReferenceCheck(value);
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

            if (!AddObjectAsReferenceCheck(anEnumerable))
                return true;

            bool flag;
            bool flag1 = true;
            _builder.WriteStartArray();
            try
            {
                var valueType = ConvertUtils.GetEnumerableValueTypeCode(anEnumerable);

                // note that an error in the IEnumerable won't be caught
                foreach (object value in anEnumerable)
                {
                    if (!flag1)
                    {
                        _builder.WriteComma();
                    }

                    if (valueType == ConvertUtils.PrimitiveTypeCode.Object)
                    {
                        //will require more reflection
                        if (this.SerializeValue(value, recursiveCount))
                        {
                            flag1 = false;
                        }
                        else
                        {
                            flag = false;
                            return flag;
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
                RemoveObjectAsReferenceCheck(anEnumerable);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeMultidimensionalArray(Array values, int recursiveCount)
        {
            if (!AddObjectAsReferenceCheck(values))
                return true;

            try
            {
                return SerializeMultidimensionalArray(values, recursiveCount, new int[0]);
            }
            finally
            {
                RemoveObjectAsReferenceCheck(values);
            }
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

            try
            {
                if (!this.SerializeValue(json, 0))
                {
                    return null;
                }

                return _builder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _builder = null;
            }
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
            Serializer json = new Serializer(settings);
            try
            {
                return json.SerializeObjectInternal(Object);
            }
            finally
            {
                json = null;
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDictionary(IDictionary values, int recursiveCount)
        {
            if (!AddObjectAsReferenceCheck(values))
                return true;

            //check if key is string type

            Type type = values.GetType();
            var keyCodeType = ConvertUtils.GetTypeCode(type.GetGenericArguments()[0]);
            if (keyCodeType != ConvertUtils.PrimitiveTypeCode.String)
            {
                return false;
            }
            var valueCodeType = ConvertUtils.GetTypeCode(type.GetGenericArguments()[1]);

            _builder.WriteStartObject();
            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
            IDictionaryEnumerator e = values.GetEnumerator();
            try
            {
                while (e.MoveNext())
                {
                    DictionaryEntry entry = e.Entry;

                    if (entry.Key == null)
                        continue;

                    string name = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                    if (name.IsNullOrEmpty())
                        continue;

                    _builder.WritePropertyName(name);

                    if (valueCodeType == ConvertUtils.PrimitiveTypeCode.Object)
                    {
                    if (!this.SerializeValue(entry.Value, recursiveCount))
                   //  if(!this.SerializeNonPrimitiveValue(entry.Value, recursiveCount))//bug for circule reference
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //short cut.
                        _builder.WriteObjectValue(entry.Value, valueCodeType);
                    }
                }
            }
            finally
            {
                (e as IDisposable)?.Dispose();
                _builder.WriteEndObject();
                RemoveObjectAsReferenceCheck(values);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeObject(IEnumerable keys, IEnumerable values, int recursiveCount)
        {
            _builder.WriteStartObject();
            try
            {
                IEnumerator enumerator = keys.GetEnumerator();
                IEnumerator enumerator1 = values.GetEnumerator();
                bool flag = true;
                while (enumerator.MoveNext() && enumerator1.MoveNext())
                {
                    string current = (enumerator.Current as string);
                    if (current == null)
                        continue;

                    object obj = enumerator1.Current;
                    _builder.WritePropertyName(current);

                    if (obj is string)
                    {
                        _builder.WriteValue((string)obj);
                    }
                    else if (!this.SerializeValue(obj, recursiveCount))
                    {
                        return false;
                    }
                    flag = false;
                }
                return true;
            }
            finally
            {
                _builder.WriteEndObject();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDataset(System.Data.DataSet ds, int recursiveCount)
        {
            if (!AddObjectAsReferenceCheck(ds))
                return true;

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
                RemoveObjectAsReferenceCheck(ds);
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
                System.Data.DataColumnCollection cols = table.Columns;
                bool rowseparator = false;
                foreach (System.Data.DataRow row in table.Rows)
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
            if (!AddObjectAsReferenceCheck(dt))
                return true;

            _builder.WriteStartObject();
            try
            {
                SerializeDataTableData(dt, recursiveCount);
            }
            finally
            {
                _builder.WriteEndObject();
                RemoveObjectAsReferenceCheck(dt);
            }
            // end datatable


            return true;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeValue(object value, int recursiveCount)
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

         var   typeCode = ConvertUtils.GetTypeCode(value.GetType());

            if (typeCode == ConvertUtils.PrimitiveTypeCode.Object)
            {
                //handle for object
                return SerializeNonPrimitiveValue(value, recursiveCount);
            }
            else
            {
                _builder.WriteObjectValue(value, typeCode);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeNonPrimitiveValue(object value, int recursiveCount)
        {
            //check if it's collection
            if (value is IEnumerable)
            {
                if (value is IDictionary)
                {
                    return SerializeDictionary((IDictionary)value, recursiveCount);
                }

                if (value is System.Collections.Specialized.NameValueCollection)
                {
                    return this.SerializeNameValueCollection((System.Collections.Specialized.NameValueCollection)value, recursiveCount);
                }

                return this.SerializeEnumerable((IEnumerable)value, recursiveCount);

            }//IEnumerable

            if (value is System.ComponentModel.IListSource)
            {
                if (value is System.Data.DataSet)
                    return SerializeDataset((System.Data.DataSet)value, recursiveCount);
                if (value is System.Data.DataTable)
                    return SerializeDataTable((System.Data.DataTable)value, recursiveCount);
            }

            //NonPrimitive Object
            IJsonSerializeImplementation IJson = value as IJsonSerializeImplementation;
            if (IJson != null)
            {
                //object handles it's own Serializer
                try
                {
                    _builder.WriteRawValue(IJson.SerializeAsJson());
                    return true;
                }
                catch (Exception)
                {
                    _builder.WriteNull();
                    return true;
                }
            }


            //this prevents recursion
            if (!AddObjectAsReferenceCheck(value))
                return true;

            IDictionary<string, object> obj;
            try
            {
                if (CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out obj))
                {
                    //add flag?
                    this.SerializeDictionary((IDictionary)obj, recursiveCount);
                    return true;
                }
                else
                {
                    _builder.WriteNull();
                    return true;//was false but when false prevent continuing.
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
