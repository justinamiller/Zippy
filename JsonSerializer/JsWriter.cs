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
    public sealed class JsWriter : IDisposable
    {
        private readonly TextWriter _textWriter;
        //marker for writePropertyname is in use
        private bool _propertyInUse;
        //used to format property names for elastic.
        public bool IsElasticSearchReady { get; set; } = true;
        private readonly StringBuilder _sb;

        private static readonly IFormatProvider s_cultureInfo = CultureInfo.InvariantCulture;
        private int _arrayIndex = 0;
        private int _objectIndex = 0;
        private const char _quoteChar = '\"';


        internal int Length
        {
            get
            {
                return _sb.Length;
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

        private bool IsValid(string json = null)
        {
            if (_arrayIndex == 0 && _objectIndex == 0)
            {
                //if (json == null)
                //{
                //    json = _sb.ToString();
                //}
                //return ValidJsonFormat(json);
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
            var json = _sb.ToString();
            if (!IsValid(json))
            {
                //bad json log it
                throw new InvalidCastException("Bad Json format", new InvalidDataException(json));
            }

            return json;
        }

        public void Dispose()
        {
            StringBuilderPool.Release(_sb);
            _textWriter.Dispose();
        }

        //public JsonWriter(TextWriter writer)
        //{
        //    _textWriter = writer;
        //}

        public JsWriter()
        {
            _sb = StringBuilderPool.Get();

            _textWriter = new StringWriter(_sb, s_cultureInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartArray()
        {
            this._textWriter.Write('[');
            _arrayIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartObject()
        {
            this._textWriter.Write('{');
            this._propertyInUse = false;
            _objectIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndArray()
        {
            this._textWriter.Write(']');
            _arrayIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndObject()
        {
            this._propertyInUse = true;
            this._textWriter.Write('}');
            _objectIndex--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteComma()
        {
            this._textWriter.Write(',');
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

                    this.WriteValue(obj);
                    isFirstElement = false;
                }
                WriteEndArray();
            }
        }
        public void WriteValue(object value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            var json = value as IJsonSerializeImplementation;
            if (json != null)
            {
                _textWriter.Write(json.SerializeAsJson());
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
                        WriteValue((string)value);
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

                        this._textWriter.Write(Serializer.SerializeObject(value));
                        return;
                }
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

        public void WriteProperty(string name, string value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }


        public void WriteProperty(string name, Guid value)
        {
            this.WritePropertyName(name);
            WriteValue(value);
        }

        internal void WriteValue(Guid value)
        {
            WriteStringFast(value.ToString("D", s_cultureInfo));
        }

        private void WriteBytes(byte[] bytes)
        {
            WriteStringFast(Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None));
        }

        public void WriteProperty(string name, bool value)
        {
            this.WritePropertyName(name);
            WriteValue(value);
        }

        private readonly static char[] s_true = new char[4] { 't', 'r', 'u', 'e' };
        private readonly static char[] s_false = new char[5] { 'f', 'a', 'l', 's', 'e' };

        internal void WriteValue(bool value)
        {
            if (value)
            {
                this._textWriter.Write(s_true, 0, 4);
            }
            else
            {
                this._textWriter.Write(s_false, 0, 5);
            }
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

        internal void WriteValue(int value)
        {
            WriteIntegerValue(value);
        }

        public void WriteProperty(string name, uint value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal void WriteValue(uint value)
        {
            WriteIntegerValue(value);
        }

        public void WriteProperty(string name, sbyte value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal void WriteValue(sbyte value)
        {
            WriteIntegerValue(value);
        }

        public void WriteProperty(string name, byte value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal void WriteValue(byte value)
        {
            WriteIntegerValue(value);
        }

        public void WriteProperty(string name, short value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal void WriteValue(short value)
        {
            WriteIntegerValue(value);
        }

        public void WriteProperty(string name, ushort value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal void WriteValue(ushort value)
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


        public void WriteProperty(string name, double value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal void WriteValue(double value)
        {
            //enforce decimal
            this._textWriter.Write(value.ToString("0.0##############", s_cultureInfo));
        }

        internal void WriteValue(char value)
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

        public void WriteProperty(string name, long value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal void WriteValue(long value)
        {
            WriteIntegerValue(value);
        }

        internal void WriteValue(ulong value)
        {
            WriteIntegerValue(value, false);
        }

        internal void WriteValue(float value)
        {
            this._textWriter.Write(value.ToString("0.0######", s_cultureInfo));
        }

        internal void WriteValue(decimal value)
        {
            this._textWriter.Write(value.ToString("0.0###########################", s_cultureInfo));
        }

        internal void WriteValue(Enum value)
        {
            WriteValue(Convert.ToInt32(value));
        }

        public void WriteProperty(string name, Uri value)
        {
            this.WritePropertyName(name);
            WriteValue(value);
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
            WriteValue(value);
        }

        internal void WriteValue(DateTime value)
        {
            WriteStringFast(DateTimeUtils.GetDateTimeUtcString(value));
        }

        public void WriteProperty(string name, TimeSpan value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        internal void WriteValue(TimeSpan value)
        {
            WriteStringFast(value.ToString(null, s_cultureInfo));
        }


        public void WriteProperty(string name, object value)
        {
            this.WritePropertyName(name);
            this.WriteValue(value);
        }

        public void WriteProperty(string name, IDictionary<string, string> values)
        {
            this.WritePropertyName(name);
            if ((values?.Count ?? 0) == 0)
            {
                this.WriteNull();
                return;
            }

            WriteValue(values);
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


        /// <summary>
        /// clean up invalid characters that is not support within elastic search.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string FormatElasticName(string input)
        {
            char[] charArray = input.ToCharArray();
            int length = charArray.Length;
            char[] data = new char[length];
            for (var i = 0; i < length; i++)
            {
                char ch = charArray[i];
                if (char.IsLetterOrDigit(ch))
                {
                    data[i] = ch;
                }
                else
                {
                    data[i] = '_';
                }

            }
            return new string(data);
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
                WriteStringFast(name);
            }

            this._textWriter.Write(':');
        }

        private readonly static char[] s_Null = new char[4] { 'n', 'u', 'l', 'l' };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            this._textWriter.Write(s_Null, 0, 4);
        }

        /// <summary>
        /// use pointers to improve performance
        /// </summary>
        /// <param name="str"></param>
        public void WriteValue(string str)
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
                this._textWriter.Write(GetEncodeString(str));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteStringFast(string s)
        {
            this._textWriter.Write(_quoteChar);
            this._textWriter.Write(s);
            this._textWriter.Write(_quoteChar);
        }

        /// <summary>
        /// memory buffer; faster than using stringbuilder.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="quote">apply quotes</param>
        [SuppressMessage("brain-overload", "S1541")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static unsafe char[] GetEncodeString(string str, bool quote = true)
        {
            char[] bufferWriter = new char[(str.Length * 2) + 2];
            int bufferIndex = 0;

            if (quote)
            {
                //open quote
                bufferWriter[bufferIndex] = _quoteChar;
                bufferIndex++;
            }

            if (bufferWriter.Length > 2)
            {
                char c;
                fixed (char* chr = str)
                {
                    char* ptr = chr;
                    while ((c = *(ptr++)) != '\0')
                    {
                        switch (c)
                        {
                            case '"':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = '\"';
                                bufferIndex++;
                                break;
                            case '\\':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                break;
                            case '\u0007':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'a';
                                bufferIndex++;
                                break;
                            case '\u0008':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'b';
                                bufferIndex++;
                                break;
                            case '\u0009':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 't';
                                bufferIndex++;
                                break;
                            case '\u000A':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'n';
                                bufferIndex++;
                                break;
                            case '\u000B':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'v';
                                bufferIndex++;
                                break;
                            case '\u000C':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'f';
                                bufferIndex++;
                                break;
                            case '\u000D':
                                bufferWriter[bufferIndex] = '\\';
                                bufferIndex++;
                                bufferWriter[bufferIndex] = 'r';
                                bufferIndex++;
                                break;
                            default:
                                if (31 >= c)
                                {
                                    bufferWriter[bufferIndex] = '\\';
                                    bufferIndex++;
                                    bufferWriter[bufferIndex] = c;
                                    bufferIndex++;
                                }
                                else
                                {
                                    bufferWriter[bufferIndex] = c;
                                    bufferIndex++;
                                }
                                break;
                        }
                    }
                }
            }

            if (quote)
            {
                //close quote
                bufferWriter[bufferIndex] = _quoteChar;
                bufferIndex++;
            }

            //flush
            var buffer = new char[bufferIndex];
            for (var i = 0; i < bufferIndex; i++)
            {
                buffer[i] = bufferWriter[i];
            }
            return buffer;
            // Array.Resize(ref bufferWriter, bufferIndex);
            //  return bufferWriter;
            //_textWriter.Write(bufferWriter, 0, bufferIndex);
        }

        internal static unsafe char[] GetStringBuffer(string str, bool quote = true)
        {
            char[] bufferWriter = new char[str.Length + (quote ? 2 : 0)];
            int bufferIndex = 0;

            if (quote)
            {
                //open quote
                bufferWriter[bufferIndex] = _quoteChar;
                bufferIndex++;
            }

            if (bufferWriter.Length > 2)
            {
                char c;
                fixed (char* chr = str)
                {
                    char* ptr = chr;
                    while ((c = *(ptr++)) != '\0')
                    {
                        bufferWriter[bufferIndex] = c;
                        bufferIndex++;
                    }
                }
            }

            if (quote)
            {
                //close quote
                bufferWriter[bufferIndex] = _quoteChar;
            }

            //flush
            return bufferWriter;
            // Array.Resize(ref bufferWriter, bufferIndex);
            //  return bufferWriter;
            //_textWriter.Write(bufferWriter, 0, bufferIndex);
        }

        // Micro optimized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertIntToHex(int intValue, char[] hex)
        {
            for (int i = 3; i >= 0; i--)
            {
                int num = intValue & 0xF; // intValue % 16

                // 0x30 + num == '0' + num
                // 0x37 + num == 'A' + (num - 10)
                hex[i] = (char)((num < 10 ? 0x30 : 0x37) + num);

                intValue >>= 4;
            }
        }
    }
}