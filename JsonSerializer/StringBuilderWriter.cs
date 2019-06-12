using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace SwiftJson
{
    public sealed class StringBuilderWriter:TextWriter
    {
        private readonly static Encoding s_encoding = Encoding.Default;
        private readonly StringBuilder _sb = null;

        public StringBuilderWriter():this (512)
        {
        }

        public StringBuilderWriter(int capacity)
        {
            _sb = Utility.StringBuilderPool.Get(capacity>=0 ? capacity : 512);
        }


        public override Encoding Encoding
        {
            get
            {
                return s_encoding;
            }
        }

        public override string ToString()
        {
            return Utility.StringBuilderPool.GetStringAndRelease(_sb);
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
}
