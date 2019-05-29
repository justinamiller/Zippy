using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JsonSerializer.Utility;

namespace JsonSerializer
{
    public class JsonBufferWriter : JsonWriter
    {
        static readonly byte[] _emptyBytes = new byte[0];
        private byte[] _buffer;
        private int _offset = 0;

        public override bool Valid => base.Valid;

        internal override int Length
        {
            get
            {
                return _offset;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            var json=Encoding.UTF8.GetString(_buffer, 0, _offset);
            if (StringExtension.ValidJsonFormat(json))
                return json;

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteComma()
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 1);
            _buffer[_offset++] = (byte)',';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteQuotation()
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 1);
            _buffer[_offset++] = (byte)'\"';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteEndArray()
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 1);
            _buffer[_offset++] = (byte)']';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteEndObject()
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 1);
            _buffer[_offset++] = (byte)'}';
        }

        public override void WriteNull()
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 4);
            _buffer[_offset + 0] = (byte)'n';
            _buffer[_offset + 1] = (byte)'u';
            _buffer[_offset + 2] = (byte)'l';
            _buffer[_offset + 3] = (byte)'l';
            _offset += 4;
        }

        public override void WriteProperty(string name, string value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, Guid value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, bool value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, int value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, char value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, uint value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, sbyte value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, byte value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, short value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, ushort value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, double value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, long value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, Uri value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, DateTime value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, TimeSpan value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, object value)
        {
            base.WriteProperty(name, value);
        }

        public override void WriteProperty(string name, IDictionary<string, string> values)
        {
            base.WriteProperty(name, values);
        }

        public override void WriteProperty(string name, object[] values)
        {
            base.WriteProperty(name, values);
        }

        public override void WritePropertyName(string name, bool escape = true)
        {
            base.WritePropertyName(name, escape);
        }

        public override void WriteRawValue(string value, bool doValidate)
        {
            base.WriteRawValue(value, doValidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteStartArray()
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 1);
            _buffer[_offset++] = (byte)'[';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteStartObject()
        {
            BinaryUtil.EnsureCapacity(ref _buffer, _offset, 1);
            _buffer[_offset++] = (byte)'{';
        }

        public override void WriteValue(object value)
        {
            base.WriteValue(value);
        }

        public override void WriteValue(string str)
        {
            base.WriteValue(str);
        }

        internal override void WriteObjectValue(object value, ConvertUtils.TypeCode typeCode)
        {
            base.WriteObjectValue(value, typeCode);
        }

        internal override void WriteValue(IEnumerable enumerable)
        {
            base.WriteValue(enumerable);
        }

        internal override void WriteValue(Guid value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(bool value)
        {
            if (value)
            {
                BinaryUtil.EnsureCapacity(ref _buffer, _offset, 4);
                _buffer[_offset + 0] = (byte)'t';
                _buffer[_offset + 1] = (byte)'r';
                _buffer[_offset + 2] = (byte)'u';
                _buffer[_offset + 3] = (byte)'e';
                _offset += 4;
            }
            else
            {
                BinaryUtil.EnsureCapacity(ref _buffer, _offset, 5);
                _buffer[_offset + 0] = (byte)'f';
                _buffer[_offset + 1] = (byte)'a';
                _buffer[_offset + 2] = (byte)'l';
                _buffer[_offset + 3] = (byte)'s';
                _buffer[_offset + 4] = (byte)'e';
                _offset += 5;
            }
        }

        internal override void WriteValue(int value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(uint value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(sbyte value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(byte value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(short value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(ushort value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(double value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(char value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(long value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(ulong value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(float value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(decimal value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(Enum value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(Uri value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(DateTime value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(TimeSpan value)
        {
            base.WriteValue(value);
        }

        internal override void WriteValue(IDictionary<string, string> values)
        {
            base.WriteValue(values);
        }
    }
}
