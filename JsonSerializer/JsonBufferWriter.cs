using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JsonSerializer.Utility;

namespace JsonSerializer
{
    public class JsonBufferWriter : JsonWriter
    {
        private static readonly Encoding s_UTF8 = new UTF8Encoding(false);
        private static readonly IFormatProvider s_cultureInfo = CultureInfo.InvariantCulture;

        static readonly byte[] _emptyBytes = Array.Empty<byte>();
        private byte[] _buffer;
        private int _offset = 0;
        private bool _propertyInUse = false;

        internal override int Length
        {
            get
            {
                return _offset;
            }
        }

        public override void Dispose()
        {
            _buffer = null;
            _offset = 0;
        }

        public byte[] GetBytes()
        {
            if (!Valid)
            {
                //bad json log it
                throw new InvalidCastException("Bad Json format", new InvalidDataException(this.ToString()));
            }

            if (_buffer == null) return _emptyBytes;
            return BinaryUtil.FastCloneWithResize(_buffer, _offset);
        }

        public override string ToString()
        {
            var json=Encoding.UTF8.GetString(_buffer, 0, _offset);
            if (StringExtension.ValidJsonFormat(json))
                return json;

            return string.Empty;
        }

        public override void WriteNull()
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 4);
            _buffer[_offset++] = (byte)'n';
            _buffer[_offset++] = (byte)'u';
            _buffer[_offset++] = (byte)'l';
            _buffer[_offset++] = (byte)'l';
        }

        internal override void WriteJsonSymbol(char value)
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 1);
            _buffer[_offset++] = (byte)value;
        }


        internal override void WriteValue(string str)
        {
            if (str == null)
            {
                WriteNull();
                return;
            }
            // single-path escape
            var length = str.Length;

            if (length == 0)
            {
                //empty
                WriteQuotation();
                WriteQuotation();
                return;
            }


            // nonescaped-ensure
            var startoffset = _offset;
            var max = s_UTF8.GetMaxByteCount(length) + 2;
            BinaryUtil.EnsureCapacity(ref _buffer, startoffset, max);

            var from = 0;

            _buffer[_offset++] = (byte)'\"';

            // for JIT Optimization, for-loop i < str.Length
            for (int i = 0; i < length; i++)
            {
                byte escapeChar = default(byte);
                switch (str[i])
                {
                    case '"':
                        escapeChar = (byte)'"';
                        break;
                    case '\\':
                        escapeChar = (byte)'\\';
                        break;
                    case '\b':
                        escapeChar = (byte)'b';
                        break;
                    case '\f':
                        escapeChar = (byte)'f';
                        break;
                    case '\n':
                        escapeChar = (byte)'n';
                        break;
                    case '\r':
                        escapeChar = (byte)'r';
                        break;
                    case '\t':
                        escapeChar = (byte)'t';
                        break;
                    // use switch jumptable
                    case (char)0:
                    case (char)1:
                    case (char)2:
                    case (char)3:
                    case (char)4:
                    case (char)5:
                    case (char)6:
                    case (char)7:
                    case (char)11:
                    case (char)14:
                    case (char)15:
                    case (char)16:
                    case (char)17:
                    case (char)18:
                    case (char)19:
                    case (char)20:
                    case (char)21:
                    case (char)22:
                    case (char)23:
                    case (char)24:
                    case (char)25:
                    case (char)26:
                    case (char)27:
                    case (char)28:
                    case (char)29:
                    case (char)30:
                    case (char)31:
                    case (char)32:
                    case (char)33:
                    case (char)35:
                    case (char)36:
                    case (char)37:
                    case (char)38:
                    case (char)39:
                    case (char)40:
                    case (char)41:
                    case (char)42:
                    case (char)43:
                    case (char)44:
                    case (char)45:
                    case (char)46:
                    case (char)47:
                    case (char)48:
                    case (char)49:
                    case (char)50:
                    case (char)51:
                    case (char)52:
                    case (char)53:
                    case (char)54:
                    case (char)55:
                    case (char)56:
                    case (char)57:
                    case (char)58:
                    case (char)59:
                    case (char)60:
                    case (char)61:
                    case (char)62:
                    case (char)63:
                    case (char)64:
                    case (char)65:
                    case (char)66:
                    case (char)67:
                    case (char)68:
                    case (char)69:
                    case (char)70:
                    case (char)71:
                    case (char)72:
                    case (char)73:
                    case (char)74:
                    case (char)75:
                    case (char)76:
                    case (char)77:
                    case (char)78:
                    case (char)79:
                    case (char)80:
                    case (char)81:
                    case (char)82:
                    case (char)83:
                    case (char)84:
                    case (char)85:
                    case (char)86:
                    case (char)87:
                    case (char)88:
                    case (char)89:
                    case (char)90:
                    case (char)91:
                    default:
                        continue;
                }

                max += 2;
                BinaryUtil.EnsureCapacity(ref _buffer, startoffset, max); // check +escape capacity

                _offset += s_UTF8.GetBytes(str, from, i - from, _buffer, _offset);
                from = i + 1;
                _buffer[_offset++] = (byte)'\\';
                _buffer[_offset++] = escapeChar;
            }

            if (from != length)
            {
                _offset += s_UTF8.GetBytes(str, from, length - from, _buffer, _offset);
            }

            _buffer[_offset++] = (byte)'\"';
        }

        public void WriteStringRaw(string value)
        {
            WriteRawString(value);
        }

        internal override void WriteRawString(string value)
        {
            var length = value.Length;
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, length);

            for (var i = 0; i < length; i++)
            {
                _buffer[_offset++] = (byte)value[i];
            }
        }

        internal override void WriteValue(Guid value)
        {
            WriteQuotation();
            if (value == Guid.Empty)
            {
                WriteRawString("00000000-0000-0000-0000-000000000000");
            }
            else
            {
                WriteRawString(value.ToString("D", s_cultureInfo));
            }
            WriteQuotation();
        }

        internal override void WriteValue(bool value)
        {
            if (value)
            {
                BinaryUtil.EnsureCapacity(ref _buffer, _offset, 4);
                _buffer[_offset++] = (byte)'t';
                _buffer[_offset++] = (byte)'r';
                _buffer[_offset++] = (byte)'u';
                _buffer[_offset++] = (byte)'e';
            }
            else
            {
                BinaryUtil.EnsureCapacity(ref _buffer, _offset, 5);
                _buffer[_offset++] = (byte)'f';
                _buffer[_offset++] = (byte)'a';
                _buffer[_offset++] = (byte)'l';
                _buffer[_offset++] = (byte)'s';
                _buffer[_offset++] = (byte)'e';
            }
        }

        internal override void WriteValue(int value)
        {
            WriteValue((long)value);
        }

        internal override void WriteValue(uint value)
        {
            WriteValue((ulong)value);
        }

        internal override void WriteValue(sbyte value)
        {
            WriteValue((long)value);
        }

        internal override void WriteValue(byte value)
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 1);
            _buffer[_offset++] =  value;
        }

        internal override void WriteValue(short value)
        {
            WriteValue((long)value);
        }

        internal override void WriteValue(ushort value)
        {
            WriteValue((ulong)value);
        }

        internal override void WriteValue(double value)
        {
            var str = value.ToString();
            WriteRawString(str);
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

        internal override void WriteValue(long value)
        {
            WriteRawString(value.ToString());
        }

        internal override void WriteValue(ulong value)
        {
            WriteRawString(value.ToString());
        }

        internal override void WriteValue(float value)
        {
            WriteValue((double)value);
        }

        internal override void WriteValue(decimal value)
        {
            WriteValue((double)value);
        }

        internal override void WriteValue(DateTime value)
        {
            WriteQuotation();
            WriteRawString(DateTimeUtils.GetDateTimeUtcString(value));
            WriteQuotation();
        }

        internal override void WriteValue(TimeSpan value)
        {
            WriteQuotation();
            WriteRawString(value.ToString(null, s_cultureInfo));
            WriteQuotation();
        }
    }
}
