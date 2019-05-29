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
    public sealed class JsonTextWriter : JsonWriter
    {
        private readonly TextWriter _textWriter;
        //marker for writePropertyname is in use
        private bool _propertyInUse;
        //used to format property names for elastic.
        private readonly StringBuilder _sb;

        private static readonly IFormatProvider s_cultureInfo = CultureInfo.InvariantCulture;
        private int _arrayIndex = 0;
        private int _objectIndex = 0;
        private const char _quoteChar = '\"';


        internal override int Length
        {
            get
            {
                return _sb.Length;
            }
        }

        /// <summary>
        /// indicate if json is valid format.
        /// </summary>
        public override bool Valid
        {
            get
            {
                return IsValid();
            }
        }

        private bool IsValid(string json = null)
        {
            if (_arrayIndex == 0 && _objectIndex == 0)
            {
                if (json == null)
                {
                    json = _sb.ToString();
                }
                return ValidJsonFormat(json);
            }
            return false;
        }

        /// <summary>
        /// output json to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var json = _sb.ToString();
            if (!IsValid(json))
            {
                //bad json log it
                throw new InvalidCastException("Bad Json format", new InvalidDataException(json));
            }

            return json;
        }

        public override void Dispose()
        {
            StringBuilderPool.Release(_sb);
            _textWriter.Dispose();
        }

        //public JsonWriter(TextWriter writer)
        //{
        //    _textWriter = writer;
        //}

        public JsonTextWriter()
        {
            _sb = StringBuilderPool.Get();

            _textWriter = new StringWriter(_sb, s_cultureInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteStartArray()
        {
            this._textWriter.Write('[');
            _arrayIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteStartObject()
        {
            this._textWriter.Write('{');
            this._propertyInUse = false;
            _objectIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteEndArray()
        {
            this._textWriter.Write(']');
            _arrayIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteEndObject()
        {
            this._propertyInUse = true;
            this._textWriter.Write('}');
            _objectIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteComma()
        {
            this._textWriter.Write(',');
        }

        internal override void WriteValue(IEnumerable enumerable)
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

                    this.WriteValue(obj);
                    isFirstElement = false;
                }
                WriteEndArray();
            }
        }

        private void WriteValueInternalString(string stringData)
        {
            //check if json format
            if (ValidJsonFormat(stringData))
            {
                //string is json
                WriteRawValue(stringData, false);
            }//end json valid check
            else
            {
                //string
                WriteValue(stringData);
            }
        }

        public override void WriteValue(object value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            var json = value as IJsonSerializeImplementation;
            if (json != null)
            {
                WriteRawValue(json.SerializeAsJson(), true);
                return;
            }

            var valueType = GetTypeCode(value.GetType());
            WriteObjectValue(value, valueType);
        }

        internal override void WriteObjectValue(object value, ConvertUtils.TypeCode typeCode)
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

                        this.WriteRawValue(Serializer.SerializeObject(value), false);
                        return;
                }
            }
        }

        /// <summary>
        /// call for json string
        /// </summary>
        /// <param name="value"></param>
        public override void WriteRawValue(string value, bool doValidate)
        {
            if (value.IsNullOrWhiteSpace())
            {
                WriteNull();
                return;
            }

            string trimValue = value.Trim();

            if (doValidate && !ValidJsonFormat(trimValue))
            {
                WriteNull();
            }
            else
            {
                this._textWriter.Write(trimValue);
            }
        }

        private static bool ValidJsonFormat(string value)
        {
            if (value != null)
            {
                string trimValue = value.Trim();
                int length = trimValue.Length;

                if (length >= 2)
                {
                    char firstchr = trimValue[0];
                    bool firstPass =
                        (firstchr == '{' && trimValue[length - 1] == '}') //For object
                        ||
                        (firstchr == '[' && trimValue[length - 1] == ']');//For array

                    return firstPass;
                }
            }
            return false;
        }

        public override void WriteProperty(string name, string value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }


        public override void WriteProperty(string name, Guid value)
        {
            this.WritePropertyName(name);
            WriteValue(value);
        }

        internal override void WriteValue(Guid value)
        {
            _textWriter.Write(_quoteChar);
            if (value == Guid.Empty)
            {
                _textWriter.Write("00000000-0000-0000-0000-000000000000");
            }
            else
            {
                _textWriter.Write(value.ToString("D", s_cultureInfo));
            }

            _textWriter.Write(_quoteChar);
        }

        public override void WriteProperty(string name, bool value)
        {
            this.WritePropertyName(name);
            WriteValue(value);
        }

        internal override void WriteValue(bool value)
        {
            this._textWriter.Write(value ? "true" : "false");
        }

        public override void WriteProperty(string name, int value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        public override void WriteProperty(string name, char value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal override void WriteValue(int value)
        {
            WriteIntegerValue(value);
        }

        public override void WriteProperty(string name, uint value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal override void WriteValue(uint value)
        {
            WriteIntegerValue(value);
        }

        public override void WriteProperty(string name, sbyte value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal override void WriteValue(sbyte value)
        {
            WriteIntegerValue(value);
        }

        public override void WriteProperty(string name, byte value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal override void WriteValue(byte value)
        {
            WriteIntegerValue(value);
        }

        public override void WriteProperty(string name, short value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal override void WriteValue(short value)
        {
            WriteIntegerValue(value);
        }

        public override void WriteProperty(string name, ushort value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal override void WriteValue(ushort value)
        {
            WriteIntegerValue(value);
        }

        private void WriteIntegerValue(int value)
        {
            if (value >= 0 && value <= 9)
            {
                this._textWriter.Write((char)('0' + value));
            }
            else
            {
                bool negative = value < 0;
                WriteIntegerValue(negative ? (uint)-value : (uint)value, negative);
            }
        }

        private void WriteIntegerValue(uint value, bool negative)
        {
            if (!negative && value <= 9)
            {
                this._textWriter.Write((char)('0' + value));
            }
            else
            {
                int length = 0;
                var buffer = WriteNumberToBuffer(value, negative, ref length);
                this._textWriter.Write(buffer, 0, length);
            }
        }

        private void WriteIntegerValue(long value)
        {
            if (value >= 0 && value <= 9)
            {
                this._textWriter.Write((char)('0' + value));
            }
            else
            {
                bool negative = value < 0;
                WriteIntegerValue(negative ? (ulong)-value : (ulong)value, negative);
            }
        }

        private void WriteIntegerValue(ulong value, bool negative)
        {
            if (!negative && value <= 9)
            {
                this._textWriter.Write((char)('0' + value));
            }
            else
            {
                int length = 0;
                var buffer = WriteNumberToBuffer(value, negative, ref length);
                this._textWriter.Write(buffer, 0, length);
            }
        }

        private static char[] WriteNumberToBuffer(uint value, bool negative, ref int totalLength)
        {
            char[] buffer = new char[35];
            totalLength = MathUtils.IntLength(value);

            if (negative)
            {
                totalLength++;
                buffer[0] = '-';
            }

            int index = totalLength;

            do
            {
                uint quotient = value / 10;
                uint digit = value - (quotient * 10);
                buffer[--index] = (char)('0' + digit);
                value = quotient;
            } while (value != 0);

            return buffer;
        }

        private static char[] WriteNumberToBuffer(ulong value, bool negative, ref int totalLength)
        {
            if (value <= uint.MaxValue)
            {
                // avoid the 64 bit division if possible
                return WriteNumberToBuffer((uint)value, negative, ref totalLength);
            }


            char[] buffer = new char[35];
            totalLength = MathUtils.IntLength(value);

            if (negative)
            {
                totalLength++;
                buffer[0] = '-';
            }

            int index = totalLength;

            do
            {
                ulong quotient = value / 10;
                ulong digit = value - (quotient * 10);
                buffer[--index] = (char)('0' + digit);
                value = quotient;
            } while (value != 0);

            return buffer;
        }


        public override void WriteProperty(string name, double value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal override void WriteValue(double value)
        {
            //enforce decimal
            this._textWriter.Write(value.ToString("0.0##############", s_cultureInfo));
        }

        internal override void WriteValue(char value)
        {
            // Special case the null char as we don't want it to turn into a null string
            if (value == '\0')
            {
                WriteNull();
            }
            else
            {
                this.WriteValue(value.ToString());
            }
        }

        public override void WriteProperty(string name, long value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal override void WriteValue(long value)
        {
            WriteIntegerValue(value);
        }

        internal override void WriteValue(ulong value)
        {
            WriteIntegerValue(value, false);
        }

        internal override void WriteValue(float value)
        {
            this._textWriter.Write(value.ToString("0.0######", s_cultureInfo));
        }

        internal override void WriteValue(decimal value)
        {
            this._textWriter.Write(value.ToString("0.0###########################", s_cultureInfo));
        }

        internal override void WriteValue(Enum value)
        {
            this._textWriter.Write(value.ToString("D"));
        }

        public override void WriteProperty(string name, Uri value)
        {
            this.WritePropertyName(name);
            WriteValue(value);
        }

        internal override void WriteValue(Uri value)
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

        public override void WriteProperty(string name, DateTime value)
        {
            this.WritePropertyName(name);
            WriteValue(value);
        }

        internal override void WriteValue(DateTime value)
        {
            WriteValue(GetDateTimeUtcString(value));
        }

        public override void WriteProperty(string name, TimeSpan value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal override void WriteValue(TimeSpan value)
        {
            _textWriter.Write(_quoteChar);
            _textWriter.Write(value.ToString(null, s_cultureInfo));
            _textWriter.Write(_quoteChar);
        }


        public static string GetDateTimeUtcString(DateTime datetime)
        {
            DateTime convertDateTime;
            switch (datetime.Kind)
            {
                case DateTimeKind.Local:
                case DateTimeKind.Unspecified:
                    convertDateTime = datetime.ToUniversalTime();
                    break;
                default:
                    convertDateTime = datetime;
                    break;
            }

            return convertDateTime.ToString("yyyy-MM-dd\\THH:mm:ss.FFFFFFF\\Z", s_cultureInfo);
        }

        public override void WriteProperty(string name, object value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        public override void WriteProperty(string name, IDictionary<string, string> values)
        {
            this.WritePropertyName(name);
            if ((values?.Count ?? 0) == 0)
            {
                this.WriteNull();
                return;
            }

            WriteValue(values);
        }

        internal override void WriteValue(IDictionary<string, string> values)
        {
            this.WriteStartObject();
            foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>)values)
            {
                this.WriteProperty(keyValuePair.Key, keyValuePair.Value);
            }
            this.WriteEndObject();
        }

        public override void WriteProperty(string name, object[] values)
        {
            this.WritePropertyName(name);
            this.WriteValue(values);
        }




        /// <summary>
        /// requires to be json compliant
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
        public override void WritePropertyName(string name, bool escape = true)
        {
            if (this._propertyInUse)
            {
                this._textWriter.Write(',');
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
                this._textWriter.Write(_quoteChar);
                this._textWriter.Write(name);
                this._textWriter.Write(_quoteChar);
            }

            this._textWriter.Write(':');
        }

        private readonly static char[] s_Null = new char[4] { 'n', 'u', 'l', 'l' };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteNull()
        {
            this._textWriter.Write(s_Null, 0, 4);
        }

        /// <summary>
        /// use pointers to improve performance
        /// </summary>
        /// <param name="str"></param>
        public override void WriteValue(string str)
        {
            if (str == null)
            {
                WriteNull();
            }
            else if (str.Length == 0)
            {
                this._textWriter.Write(new char[2] { _quoteChar, _quoteChar }, 0, 2);
            }
            else
            {
                this._textWriter.Write(StringExtension.GetEncodeString(str));
            }
        }
    }
}
