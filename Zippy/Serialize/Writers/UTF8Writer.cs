using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zippy.Serialize.Writers
{
    internal class UTF8Writer: TextWriter
    {
        private  byte[] _buffer;
        private int _index = 0;

        private readonly static Encoding s_encoding = Encoding.UTF8;

        public override Encoding Encoding { get; } = s_encoding;

        public UTF8Writer()
        {
            _buffer = new byte[500];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(string value)
        {
            var source = s_encoding.GetBytes(value);
            var len = source.Length;
            Buffer.BlockCopy(source, 0, _buffer, _index, len);
            _index += len;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char value)
        {
            _buffer[_index++] = (byte)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char[] buffer, int index, int count)
        {
            var source = s_encoding.GetBytes(buffer,index,count);
            var len = source.Length;
            Buffer.BlockCopy(source, 0, _buffer, 0, len);
            _index += len;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char[] buffer)
        {
            var source = s_encoding.GetBytes(buffer);
            var len = source.Length;
            Buffer.BlockCopy(source, 0, _buffer, 0, len);
            _index += len;
        }

        public override string ToString()
        {
           return s_encoding.GetString(_buffer, 0, _index);
        }
    }
}
