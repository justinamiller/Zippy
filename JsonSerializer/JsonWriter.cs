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

namespace JsonSerializer
{
    public sealed class JsonWriter : IDisposable
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
        private bool _hasObject = false;
        private bool _hasArray = false;

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
                return  _arrayIndex ==0 && _objectIndex ==0 &&  ValidJsonFormat(this._sb.ToString());
            }
        }

        /// <summary>
        /// output json to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!Valid)
            {
                //bad json log it
               throw new Exception("Bad Json format", new InvalidDataException(_sb.ToString()));
            }

            return _sb.ToString();
        }

        public void Dispose()
        {
            StringBuilderPool.Release(_sb);
            _textWriter.Dispose();
        }

        public JsonWriter(TextWriter writer)
        {
            _sb = StringBuilderPool.Get();

            _textWriter = writer;
        }

        public JsonWriter()
        {
            _sb = StringBuilderPool.Get();

            _textWriter = new StringWriter(_sb, s_cultureInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartArray()
        {
            _hasArray = true;
            this._textWriter.Write('[');
            _arrayIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStartObject()
        {
            _hasObject = true;
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
                    this.WriteValueInternal(obj);
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
                WriteRawValue(stringData);
            }//end json valid check
            else
            {
                //string
                WriteValue(stringData);
            }
        }

        private void WriteValueInternalPrimitive(object obj)
        {
            if (obj is bool)
            {
                WriteValue((bool)obj);
            }
            else if (obj is double)
            {
                // Have to special case floats to get full precision
                WriteValue((double)obj);
            }
            else if (obj is float)
            {
                WriteValue((float)obj);
            }
            else if (obj is decimal)
            {
                WriteValue((decimal)obj);
            }
            else if (obj is char)
            {
                WriteValue((char)obj);
            }
            else if (obj is IConvertible)
            {
                this._textWriter.Write(((IConvertible)obj).ToString(CultureInfo.InvariantCulture));
            }
        }

        private void WriteValueInternal(object obj)
        {
            // 'null'
            if (obj == null || DBNull.Value.Equals(obj))
            {
                WriteNull();
            }
            else if (obj is IJsonSerializeImplementation)
            {
                WriteRawValue(((IJsonSerializeImplementation)obj).SerializeAsJson());
            }
            else if (obj is string)
            {
                WriteValueInternalString((string)obj);
            }
            else if (obj.GetType().IsPrimitive || obj is decimal)
            {
                WriteValueInternalPrimitive(obj);
            }
            else if (obj.GetType().IsEnum)
            {
                WriteValue((Enum)obj);
            }
            else
            {
                //it's a complex object
                WriteValueInternalComplexObject(obj);
            }
        }

        private void WriteValueInternalComplexObject(object obj)
        {
            if (obj is DateTime)
            {
                WriteValue((DateTime)obj);
            }
            else if (obj is DateTimeOffset)
            {
                // DateTimeOffset is converted to a UTC DateTime and serialized as usual.
                WriteValue(((DateTimeOffset)obj).UtcDateTime);
            }
            else if (obj is Guid)
            {
                WriteValue((Guid)obj);
            }
            else if (obj is System.Xml.XmlNode)
            {
                WriteValue(((System.Xml.XmlNode)obj).OuterXml);
            }
            else if (obj is Uri)
            {
                this.WriteValue(((Uri)obj).GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
            }
            else
            {
                // Serialize all public fields and properties. very slow!
                this.WriteRawValue(Serializer.SerializeObject(obj));                   
            }
        }

        /// <summary>
        /// call for json string
        /// </summary>
        /// <param name="value"></param>
        public void WriteRawValue(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                WriteNull();
                return;
            }

            string trimValue = value.Trim();

            if (!(ValidJsonFormat(trimValue)))
            {
                WriteNull();
            }
            else
            {
                this._textWriter.Write(trimValue);
            }
        }

        public static bool ValidJsonFormat(string value)
        {
            if (value != null)
            {
                string trimValue = value.Trim();
                int length = trimValue.Length;

                if (length >= 2)
                {
                    bool firstPass =
                        (trimValue[0] == '{' && trimValue[length - 1] == '}') //For object
                        ||
                        (trimValue[0] == '[' && trimValue[length - 1] == ']');//For array

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
            _textWriter.Write('\"');
            if (value == Guid.Empty)
            {
                _textWriter.Write("00000000-0000-0000-0000-000000000000");
            }
            else
            {
                _textWriter.Write(value.ToString("D"));
            }

            _textWriter.Write('\"');
        }

        public void WriteProperty(string name, bool value)
        {
            this.WritePropertyName(name);
            WriteValue(value);
        }

        internal void WriteValue(bool value)
        {
            this._textWriter.Write(value ? "true" : "false");
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
            this._textWriter.Write(value.ToString(s_cultureInfo));
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
            this._textWriter.Write(value.ToString(s_cultureInfo));
        }

        internal void WriteValue(ulong value)
        {
            this._textWriter.Write(value.ToString(s_cultureInfo));
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
            this._textWriter.Write(value.ToString("D"));
        }

        public void WriteProperty(string name, DateTime value)
        {
            this.WritePropertyName(name);
            WriteValue(value);
        }

        internal void WriteValue(DateTime value)
        {
            WriteValue(GetDateTimeUtcString(value));
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

        public void WriteProperty(string name, object value)
        {
            this.WritePropertyName(name);
            this.WriteValueInternal(value);
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
        /// <param name="name"></param>
        /// <param name="isSafeName">when true will not do addtional checks</param>
        public void WritePropertyName(string name)
        {
            if (this._propertyInUse)
            {
                this._textWriter.Write(',');
            }
            else
            {
                this._propertyInUse = true;
            }

                string propertyName = name ?? string.Empty;
                if (IsElasticSearchReady)
                {
                    propertyName = FormatElasticName(propertyName);
                }

                this.WriteValue(propertyName);
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
                this._textWriter.Write(new char[2] { '\"', '\"' }, 0, 2);
            }
            else
            {
                WriteStringEncode(str);
            }
        }

        private const int BufferLengthThreshold = 10;
        [SuppressMessage("brain-overload", "S1541")]
        private void WriteStringEncode(string value)
        {
            int valueLength = value.Length;
            int initSize;
            if (valueLength > BufferLengthThreshold)
            {
                initSize = valueLength * 2;
            }
            else
            {
                //size must be at least what buffer threshold is set.
                initSize = BufferLengthThreshold * 2;
            }

            //double size + 2 spots for quotes
            char[] buffer = new char[2 + initSize];
            int bufferLength = buffer.Length;
            int bufferIndex = 0;

            //start quote
            buffer[bufferIndex] = '\"';
            bufferIndex++;

            char[] hexSeqBuffer = new char[4];

            for (int i = 0; i < valueLength; i++)
            {
                char chr = value[i];
                switch (chr)
                {
                    case '\r':
                        buffer[bufferIndex] = '\\';
                        bufferIndex++;
                        buffer[bufferIndex] = 'r';
                        bufferIndex++;
                        break;
                    case '\t':
                        buffer[bufferIndex] = '\\';
                        bufferIndex++;
                        buffer[bufferIndex] = 't';
                        bufferIndex++;
                        break;
                    case '\"':
                        buffer[bufferIndex] = '\\';
                        bufferIndex++;
                        buffer[bufferIndex] = '\"';
                        bufferIndex++;

                        break;
                    case '\\':
                        buffer[bufferIndex] = '\\';
                        bufferIndex++;
                        buffer[bufferIndex] = '\\';
                        bufferIndex++;
                        break;
                    case '\n':
                        buffer[bufferIndex] = '\\';
                        bufferIndex++;
                        buffer[bufferIndex] = 'n';
                        bufferIndex++;
                        break;
                    case '\b':
                        buffer[bufferIndex] = '\\';
                        bufferIndex++;
                        buffer[bufferIndex] = 'b';
                        bufferIndex++;
                        break;
                    case '\f':
                        buffer[bufferIndex] = '\\';
                        bufferIndex++;
                        buffer[bufferIndex] = 'f';
                        bufferIndex++;
                        break;
                    case '<':
                    case '>':
                    case '&':
                    case '\'':
                        //escape html characters
                        buffer[bufferIndex] = '\\';
                        bufferIndex++;
                        buffer[bufferIndex] = 'u';
                        bufferIndex++;

                        ConvertIntToHex(chr, hexSeqBuffer);
                        foreach (char xChr in hexSeqBuffer)
                        {
                            buffer[bufferIndex] = xChr;
                            bufferIndex++;
                        }
                        break;
                    default:
                        // Append the unhandled characters (that do not require special treament)
                        // to the string builder when special characters are detected.
                        var isPrintable = char.IsLetterOrDigit(chr) || char.IsPunctuation(chr) || char.IsSymbol(chr) ||  char.IsWhiteSpace(chr);
                        if (isPrintable | !char.IsControl(chr))
                        {
                            //no encoding required.
                            buffer[bufferIndex] = chr;
                            bufferIndex++;
                        }
                        else
                        {
                            // Default, turn into a \uXXXX sequence
                            buffer[bufferIndex] = '\\';
                            bufferIndex++;
                            buffer[bufferIndex] = 'u';
                            bufferIndex++;

                            ConvertIntToHex(chr, hexSeqBuffer);
                            foreach (char xChr in hexSeqBuffer)
                            {
                                buffer[bufferIndex] = xChr;
                                bufferIndex++;
                            }
                        }
                        break;
                }

                //check if buffer needs to be resized
                if ((bufferIndex + BufferLengthThreshold) > bufferLength)
                {
                    //only resize 2x what is left to scan + BufferLengthThreshold
                    Array.Resize(ref buffer, ((valueLength - i) * 2) + BufferLengthThreshold + bufferLength);
                    bufferLength = buffer.Length;
                }
            }

            //end quote
            buffer[bufferIndex] = '\"';
            bufferIndex++;

            //flush
            _textWriter.Write(buffer, 0, bufferIndex);
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
