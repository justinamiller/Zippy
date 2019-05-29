using JsonSerializer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using static JsonSerializer.Utility.ConvertUtils;

namespace JsonSerializer
{
    public abstract class JsonWriter : IDisposable
    {
        //used to format property names for elastic.
        public bool IsElasticSearchReady { get; set; } = true;
        private bool _propertyInUse = false;
        private int _objectIndex = 0;
        private int _arrayIndex = 0;

        internal virtual int Length
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// indicate if json is valid format.
        /// </summary>
        public bool Valid
        {
            get
            {
                return IsValid();
            }
        }

        private bool IsValid()
        {
            if (_arrayIndex == 0 && _objectIndex == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// output json to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Empty;
        }

        public virtual void Dispose()
        {
            return;
        }

        //public JsonWriter(TextWriter writer)
        //{
        //    _textWriter = writer;
        //}

        protected JsonWriter()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteStartArray()
        {
            this.WriteJsonSymbol('[');
            _arrayIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteStartObject()
        {
            this.WriteJsonSymbol('{');
            this._propertyInUse = false;
            _objectIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteEndArray()
        {
            this.WriteJsonSymbol(']');
            _arrayIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteEndObject()
        {
            this.WriteJsonSymbol('}');
            this._propertyInUse = true;
            _objectIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteComma()
        {
            this.WriteJsonSymbol(',');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteNameSeparator()
        {
            this.WriteJsonSymbol(':');
        }

        internal void WriteValue(IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                this.WriteNull();
            }
            else
            {
                WriteStartArray();
                bool isFirstElement = true;
                foreach (object obj in enumerable)
                {
                    if (!isFirstElement)
                    {
                        WriteComma();
                    }
                    else
                    {
                        isFirstElement = false;
                    }

                    this.WriteValue(obj);
                }
                WriteEndArray();
            }
        }

        internal void WriteValue(object value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            var json = value as IJsonSerializeImplementation;
            if (json != null)
            {
                WriteRawJson(json.SerializeAsJson(), true);
                return;
            }

            var valueType = GetTypeCode(value.GetType());
            WriteObjectValue(value, valueType);
        }

        internal void WriteObjectValue(object value, ConvertUtils.TypeCode typeCode)
        {
            while (true)
            {
                switch (typeCode)
                {
                    case ConvertUtils.TypeCode.Char:
                        WriteValue((char)value);
                        return;
                    case ConvertUtils.TypeCode.Boolean:
                        WriteValue((bool)value);
                        return;
                    case ConvertUtils.TypeCode.SByte:
                        WriteValue((sbyte)value);
                        return;
                    case ConvertUtils.TypeCode.Int16:
                        WriteValue((short)value);
                        return;
                    case ConvertUtils.TypeCode.UInt16:
                        WriteValue((ushort)value);
                        return;
                    case ConvertUtils.TypeCode.Int32:
                        WriteValue((int)value);
                        return;
                    case ConvertUtils.TypeCode.Byte:
                        WriteValue((byte)value);
                        return;
                    case ConvertUtils.TypeCode.UInt32:
                        WriteValue((uint)value);
                        return;
                    case ConvertUtils.TypeCode.Int64:
                        WriteValue((long)value);
                        return;
                    case ConvertUtils.TypeCode.UInt64:
                        WriteValue((ulong)value);
                        return;
                    case ConvertUtils.TypeCode.Single:
                        WriteValue((float)value);
                        return;
                    case ConvertUtils.TypeCode.Double:
                        WriteValue((double)value);
                        return;
                    case ConvertUtils.TypeCode.DateTime:
                        WriteValue((DateTime)value);
                        return;
                    case ConvertUtils.TypeCode.DateTimeOffset:
                        WriteValue(((DateTimeOffset)value).UtcDateTime);
                        return;
                    case ConvertUtils.TypeCode.Decimal:
                        WriteValue((decimal)value);
                        return;
                    case ConvertUtils.TypeCode.Guid:
                        WriteValue((Guid)value);
                        return;
                    case ConvertUtils.TypeCode.TimeSpan:
                        WriteValue((TimeSpan)value);
                        return;
                    //case PrimitiveTypeCode.BigInteger:
                    //    // this will call to WriteValue(object)
                    //    WriteValue((BigInteger)value);
                    //    return;
                    case ConvertUtils.TypeCode.Uri:
                        WriteValue((Uri)value);
                        return;
                    case ConvertUtils.TypeCode.String:
                        WriteValueInternalString((string)value);
                        return;
                    case ConvertUtils.TypeCode.Bytes:
                        WriteValue((byte[])value);
                        return;
                    case ConvertUtils.TypeCode.DBNull:
                        WriteNull();
                        return;
                    default:
                        if (value is IConvertible convertible)
                        {
                            ResolveConvertibleValue(convertible, out typeCode, out value);
                            continue;
                        }

                        // write an unknown null value
                        if (value == null)
                        {
                            WriteNull();
                            return;
                        }

                        this.WriteRawJson(Serializer.SerializeObject(value), false);
                        return;
                }
            }
        }

        private void WriteValueInternalString(string stringData)
        {
            //check if json format
            if (StringExtension.ValidJsonFormat(stringData))
            {
                //string is json
                WriteRawJson(stringData, false);
            }//end json valid check
            else
            {
                //string
                WriteValue(stringData);
            }
        }

        /// <summary>
        /// call for json string
        /// </summary>
        /// <param name="value"></param>
        internal void WriteRawJson(string value, bool doValidate)
        {
            if (value.IsNullOrWhiteSpace())
            {
                WriteNull();
                return;
            }

            string trimValue = value.Trim();

            if (doValidate && !StringExtension.ValidJsonFormat(trimValue))
            {
                WriteNull();
            }
            else
            {
                WriteRawString(trimValue);
            }
        }

        public void WriteProperty(string name, string value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }


        public void WriteProperty(string name, Guid value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(Guid value)
        {
        }

        public void WriteProperty(string name, bool value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(bool value)
        {
        }

        public void WriteProperty(string name, int value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        public void WriteProperty(string name, char value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(int value)
        {
        }

        public void WriteProperty(string name, uint value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(uint value)
        {
        }

        public void WriteProperty(string name, sbyte value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(sbyte value)
        {
        }

        public void WriteProperty(string name, byte value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(byte value)
        {
        }

        public void WriteProperty(string name, short value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(short value)
        {
        }

        public void WriteProperty(string name, ushort value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(ushort value)
        {
        }

        public void WriteProperty(string name, double value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(double value)
        {
        }

        internal virtual void WriteValue(char value)
        {
        }

        public void WriteProperty(string name, long value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(long value)
        {
        }

        internal virtual void WriteValue(ulong value)
        {
        }

        internal virtual void WriteValue(float value)
        {
        }

        internal virtual void WriteValue(decimal value)
        {

        }

        internal void WriteValue(Enum value)
        {
         WriteRawString(value.ToString("D"));
        }

        public void WriteProperty(string name, Uri value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal void WriteValue(Uri value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                WriteValue(value.OriginalString);
            }
        }

        public void WriteProperty(string name, DateTime value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(DateTime value)
        {
        }

        public void WriteProperty(string name, TimeSpan value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal virtual void WriteValue(TimeSpan value)
        {
        }

        public void WriteProperty(string name, object value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        public void WriteProperty(string name, IDictionary<string, string> values)
        {
            this.WritePropertyName(name);
            this.WriteValue(values);
        }

        internal void WriteValue(IDictionary<string, string> values)
        {
            this.WriteStartObject();
            foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>)values)
            {
                this.WriteProperty(keyValuePair.Key, keyValuePair.Value);
            }
            this.WriteEndObject();
        }

        public void WriteProperty(string name, object[] values)
        {
            this.WritePropertyName(name);
            this.WriteValue(values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteQuotation()
        {
            this.WriteJsonSymbol('\"');
        }

        internal virtual void WriteJsonSymbol(char value)
        {
            WriteRawString(value.ToString());
        }

        internal virtual void WriteRawString(string value)
        {
        }

        /// <summary>
        /// requires to be json compliant
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
        public void WritePropertyName(string name, bool escape = true)
        {
            if (this._propertyInUse)
            {
                WriteComma();
            }
            else
            {
                this._propertyInUse = true;
            }

            // string propertyName = name ?? string.Empty;
            //if (IsElasticSearchReady)
            //{
            //    propertyName = FormatElasticName(propertyName);
            //}

            if (escape)
            {
                //slower
                this.WriteValue(name);
            }
            else
            {
                //fast
                WriteQuotation();
                WriteRawString(name);
                WriteQuotation();
            }

            WriteNameSeparator();
        }


        public virtual void WriteNull()
        {
            WriteQuotation();
            WriteRawString("null");
            WriteQuotation();
        }

        internal virtual void WriteValue(string str)
        {
        }
    }
}
