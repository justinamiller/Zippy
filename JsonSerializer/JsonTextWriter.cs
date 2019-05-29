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
        //used to format property names for elastic.
        private readonly StringBuilder _sb;

        private static readonly IFormatProvider s_cultureInfo = CultureInfo.InvariantCulture;
        private const char _quoteChar = '\"';


        internal override int Length
        {
            get
            {
                return _sb.Length;
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
                throw new InvalidCastException("Bad Json format", new InvalidDataException(_sb.ToString()));
            }

            return _sb.ToString();
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


        internal override void WriteValue(bool value)
        {
            this._textWriter.Write(value ? "true" : "false");
        }

        internal override void WriteValue(int value)
        {
            WriteIntegerValue(value);
        }

        internal override void WriteValue(uint value)
        {
            WriteIntegerValue(value);
        }

        internal override void WriteValue(sbyte value)
        {
            WriteIntegerValue(value);
        }

        internal override void WriteValue(byte value)
        {
            WriteIntegerValue(value);
        }


        internal override void WriteValue(short value)
        {
            WriteIntegerValue(value);
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

        internal override void WriteJsonSymbol(char value)
        {
            _textWriter.Write(value);
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

        internal override void WriteValue(DateTime value)
        {
            WriteValue(DateTimeUtils.GetDateTimeUtcString(value));
        }


        internal override void WriteValue(TimeSpan value)
        {
            _textWriter.Write(_quoteChar);
            _textWriter.Write(value.ToString(null, s_cultureInfo));
            _textWriter.Write(_quoteChar);
        }

        private readonly static char[] s_Null = new char[4] { 'n', 'u', 'l', 'l' };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteNull()
        {
            this._textWriter.Write(s_Null, 0, 4);
        }

        public override void WriteRawString(string value)
        {
            this._textWriter.Write(value);
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
