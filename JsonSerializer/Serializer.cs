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
                IJsonSerializerSetting JsonSetting = _currentJsonSetting;
                if (JsonSetting == null)
                {
                    JsonSetting = DefaultJsonSetting;
                    _currentJsonSetting = JsonSetting;
                }
                return JsonSetting;
            }
            set
            {
                _currentJsonSetting = value;
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
                    IJsonSerializerStrategy defaultJsonSerializerStrategy1 = new LambdaJsonSerializerStrategy();//new  DelegateJsonSerializerStrategy();
                    IJsonSerializerStrategy defaultJsonSerializerStrategy2 = defaultJsonSerializerStrategy1;
                    s_defaultJsonSerializerStrategy = defaultJsonSerializerStrategy1;
                    defaultJsonSerializerStrategy = defaultJsonSerializerStrategy2;
                }
                return defaultJsonSerializerStrategy;
            }
        }

        public Serializer()
        {
            this.CurrentJsonSetting = DefaultJsonSetting;
        }

        public Serializer(IJsonSerializerSetting setting) : this()
        {
            this.CurrentJsonSetting = setting ?? DefaultJsonSetting;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeList(IList anList, int recursiveCount)
        {
            if (!AddObjectAsReferenceCheck(anList))
                return true;

            bool flag;
            bool flag1 = true;

            _builder.WriteStartArray();
            try
            {
                // note that an error in the IEnumerable won't be caught
                for (int i = 0; i < anList.Count; i++)
                {
                    if (!flag1)
                    {
                        _builder.WriteComma();
                    }
                    if (this.SerializeValue(anList[i], recursiveCount))
                    {
                        flag1 = false;
                    }
                    else
                    {
                        flag = false;
                        return flag;
                    }
                }
            }
            finally
            {
                _builder.WriteEndArray();
                RemoveObjectAsReferenceCheck(anList);
            }

            return true;
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
                    if (!flag)
                    {
                        _builder.WriteComma();
                    }

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
            if (!AddObjectAsReferenceCheck(anEnumerable))
                return true;

            bool flag;
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
            }
            finally
            {
                _builder.WriteEndArray();
                RemoveObjectAsReferenceCheck(anEnumerable);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeArray(Array anArray, int recursiveCount)
        {
            if (anArray.Rank > 1)
                return SerializeMultidimensionalArray(anArray, recursiveCount);

            if (!AddObjectAsReferenceCheck(anArray))
                return true;


            bool flag1 = true;

            _builder.WriteStartArray();
            try
            {
                for (int i = 0; i < anArray.Length; i++)
                {
                    if (!flag1)
                    {
                        _builder.WriteComma();
                    }
                    if (this.SerializeValue(anArray.GetValue(i), recursiveCount))
                    {
                        flag1 = false;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            finally
            {
                _builder.WriteEndArray();
                RemoveObjectAsReferenceCheck(anArray);
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeDateTime(DateTime dateTime)
        {
            DateTime universalTime = dateTime.ToUniversalTime();
            string output = universalTime.ToString(s_Iso8601Format[0], s_defaultJsonCultureInfo);

            return SerializeString(output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            string output = dateTimeOffset.ToUniversalTime().ToString(s_Iso8601Format[0], s_defaultJsonCultureInfo);

            return SerializeString(output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeGuid(Guid guid)
        {
            _builder.WriteValue(guid);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeChar(char value)
        {
            _builder.WriteValue(value);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeEnum(Enum value)
        {
            _builder.WriteValue(value);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeBool(bool value)
        {
            _builder.WriteValue(value);
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeNumber(object number)
        {
            if (number is int)
            {
                _builder.WriteValue((int)number);
                return true;
            }
            if (number is long)
            {
                _builder.WriteValue((long)number);
                return true;
            }
            if ((number is double))
            {
                _builder.WriteValue((double)number);
                return true;
            }
            if (number is decimal)
            {
                _builder.WriteValue((decimal)number);
                return true;
            }
            if ((number is float))
            {
                _builder.WriteValue((float)number);
                return true;
            }
            if ((number is short))
            {
                _builder.WriteValue((short)number);
                return true;
            }
            if ((number is byte))
            {
                _builder.WriteValue((byte)number);
                return true;
            }
            if (number is ulong)
            {
                _builder.WriteValue((ulong)number);
                return true;
            }
            if (number is uint)
            {
                _builder.WriteValue((uint)number);
                return true;
            }
            if ((number is ushort))
            {
                _builder.WriteValue((ushort)number);
                return true;
            }
            if ((number is sbyte))
            {
                _builder.WriteValue((sbyte)number);
                return true;
            }

            return false;
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
            Serializer json = new Serializer();
            try
            {
                return json.SerializeObjectInternal(Object);
            }
            finally
            {
                json = null;
            }
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
        private bool SerializeDictionary(IDictionary<string, string> data, int recursiveCount)
        {
            if (!AddObjectAsReferenceCheck(data))
                return true;

            _builder.WriteStartObject();

            try
            {
                if (data != null && data.Count > 0)
                {
                    bool flag = true;
                    var datas = data.ToArray();

                    for (int i = 0; i < datas.Length; i++)
                    {
                        if (!flag)
                            _builder.WriteComma();

                        var item = datas[i];

                        _builder.WriteProperty(item.Key, item.Value);

                        flag = false;
                    }
                }
            }
            finally
            {
                _builder.WriteEndObject();
                RemoveObjectAsReferenceCheck(data);
            }


            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeDictionary(IDictionary values, int recursiveCount)
        {
            if (!AddObjectAsReferenceCheck(values))
                return true;

            _builder.WriteStartObject();
            bool flag = true;
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

                    if (!flag)
                        _builder.WriteComma();


                    _builder.WritePropertyName(name);

                    if (!this.SerializeValue(entry.Value, recursiveCount))
                    {
                        return false;
                    }
                    flag = false;
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
        private bool SerializeDictionary(IDictionary<string, object> data, int recursiveCount)
        {
            if (!AddObjectAsReferenceCheck(data))
                return true;

            _builder.WriteStartObject();

            bool flag = true;
            try
            {
                if (data != null && data.Count > 0)
                {
                    var datas = data.ToArray();

                    for (int i = 0; i < datas.Length; i++)
                    {
                        if (!flag)
                            _builder.WriteComma();

                        var item = datas[i];


                        _builder.WritePropertyName(item.Key);

                        if (!this.SerializeValue(item.Value, recursiveCount))
                        {
                            return false;
                        }
                        flag = false;
                    }
                }

                return true;
            }
            finally
            {
                _builder.WriteEndObject();
                RemoveObjectAsReferenceCheck(data);
            }
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
                    if (!flag)
                    {
                        _builder.WriteComma();
                    }

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

            bool tablesep = false;
            _builder.WriteStartObject();
            try
            {

                foreach (System.Data.DataTable table in ds.Tables)
                {
                    if (tablesep)
                    {
                        _builder.WriteComma();
                    }
                    
                    tablesep = true;
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

                    bool pendingSeperator = false;
                    foreach (System.Data.DataColumn column in cols)
                    {
                        if (pendingSeperator)
                        {
                            _builder.WriteComma();
                        }

                        //build column name
                        _builder.WritePropertyName(column.ColumnName);
                        //build column data
                        SerializeValue(row[column], recursiveCount);
                        pendingSeperator = true;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SerializeNull()
        {
            _builder.WriteNull();
            return true;
        }

        private bool SerializeString(string value)
        {
            _builder.WriteValue(value);
            return true;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeValue(object value, int recursiveCount)
        {
            //prevent null
            if (value == null)
            {
                return SerializeNull();
            }
                

            recursiveCount += 1;
            //recursion limit or max char length
            if (recursiveCount >= this.CurrentJsonSetting.RecursionLimit || _builder.Length > this.CurrentJsonSetting.MaxJsonLength)
            {
                return SerializeNull();
            }

            //check ValueType
            if (value is ValueType)
            {
                if (value is bool)
                    return SerializeBool((bool)value);
                if (value is char)
                    return this.SerializeChar((char)value);
                if (value is Enum)
                    return this.SerializeEnum((Enum)value);
                if (value is DateTime)
                    return this.SerializeDateTime((DateTime)value);
                if (value is DateTimeOffset)
                    return this.SerializeDateTimeOffset((DateTimeOffset)value);
                if (value is Guid)
                    return this.SerializeGuid((Guid)value);
                if (value is TimeSpan)
                    return this.SerializeString(((TimeSpan)value).ToString());
                if (this.SerializeNumber(value))//try to make it value
                    return true;
                // the value is a non-standard IConvertible
                // convert to the underlying value and retry

                if (value is IConvertible)
                {
                    IConvertible convertable = value as IConvertible;
                    object convertedValue = null;

                    if (value is object)
                        convertedValue = convertable.ToType(typeof(string), s_defaultJsonCultureInfo);
                    else
                        convertedValue = convertable.ToType(typeof(object), s_defaultJsonCultureInfo);

                    return this.SerializeValue(convertedValue, recursiveCount - 1);
                }
            }//end value type

            //check object type
            if (value is object)
            {
                if (value is string)
                {
                    return this.SerializeString((string)value);
                }

                if (value is ICollection)
                {
                    if (value is IDictionary<string, object>)
                        return this.SerializeDictionary((IDictionary<string, object>)value, recursiveCount);
                    if (value is IDictionary<string, string>)
                        return this.SerializeDictionary((IDictionary<string, string>)value, recursiveCount);
                    if (value is IDictionary)
                        return SerializeDictionary((IDictionary)value, recursiveCount);
                    if (value is Array)
                        return this.SerializeArray((Array)value, recursiveCount);
                    if (value is IList)
                        return this.SerializeList((IList)value, recursiveCount);
                    if (value is System.Collections.Specialized.NameValueCollection)
                        return this.SerializeNameValueCollection((System.Collections.Specialized.NameValueCollection)value, recursiveCount);
                }//value type

                if (value is IEnumerable)
                    return this.SerializeEnumerable((IEnumerable)value, recursiveCount);
                if (value is Uri)
                    return this.SerializeString(((Uri)value).OriginalString);
                if (value is System.Data.DataSet)
                    return SerializeDataset((System.Data.DataSet)value, recursiveCount);
                if (value is System.Data.DataTable)
                    return SerializeDataTable((System.Data.DataTable)value, recursiveCount);
                if (value is System.Xml.XmlNode)
                    return this.SerializeString(((System.Xml.XmlNode)value).OuterXml);       
                if (value is System.DBNull)
                    return SerializeNull();

                return SerializeNonPrimitiveValue(value, recursiveCount);
            }

            if (value is IEnumerable)
                return this.SerializeEnumerable((IEnumerable)value, recursiveCount);

            //handle for object
            return SerializeNonPrimitiveValue(value, recursiveCount);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SerializeNonPrimitiveValue(object value, int recursiveCount)
        {
            //this prevents recursion
            if (!AddObjectAsReferenceCheck(value))
                return true;
            try
            {
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
                        return SerializeNull();
                    }
                }

                IDictionary<string, object> obj;
                try
                {
                    if (CurrentJsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out obj))
                    {
                        //add flag?
                        this.SerializeDictionary(obj, recursiveCount);
                        return true;
                    }
                    else
                    {
                        SerializeNull();
                        return true;//was false but when false prevent continuing.
                    }
                }
                catch (Exception)
                {
                    SerializeNull();
                    return true;//was false but when false prevent continuing.
                }
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
                return false;

            // if (!this._serializeStack.Add(value))
            if (this._serializeStack.ContainsKey(value))
            {
                SerializeNull();//"Self referencing loop detected";
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
