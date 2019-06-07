using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace JsonSerializer
{
    public sealed class StringBuilderWriter:TextWriter
    {
        private readonly static Encoding s_encoding = Encoding.Default;
        private readonly StringBuilder _sb = new StringBuilder(512);

        public override Encoding Encoding
        {
            get
            {
                return s_encoding;
            }
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _sb.Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char buffer)
        {
            _sb.Append(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char[] buffer)
        {
            _sb.Append(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char[] buffer, int index, int count)
        {
            _sb.Append(buffer, index, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(string value)
        {
           _sb.Append(value);
        }
    }

    public sealed class StringBuilderWriter2 : System.IO.TextWriter
    {
        private readonly static Encoding s_encoding = Encoding.Default;

        private readonly StringBuilder _sb = new StringBuilder(256);
        public override Encoding Encoding
        {
            get
            {
                return s_encoding;
            }
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        public void Clear()
        {
            _sb.Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char buffer)
        {
            _sb.Append(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char[] buffer)
        {
            Write(buffer, buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(char[] buffer, int index, int count)
        {
            if (index == 0)
            {
                Write(buffer, count);
            }
            else
            {
                var temp = new char[count];
                Array.Copy(buffer, index, temp, 0, count);
                Write(temp, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(char[] buffer, int count)
        {
            fixed (char* src = buffer)
            {
                _sb.Append(src, count);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe override void Write(string value)
        {
                fixed (char* src = value)
                {
                    _sb.Append(src, value.Length);
                }
        }
    }
}
