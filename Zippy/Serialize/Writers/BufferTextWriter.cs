using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zippy.Serialize.Writers
{
    sealed class BufferTextWriter : TextWriter
    {
        private char[] _buffer = new char[1000];
        int _bufferIndex = 0;

     public override Encoding Encoding { get; } = Utility.StringExtension.DefaultEncoding;

        public override void Write(string value)
        {
           unsafe
            {
                int strLength = value.Length;
                fixed (char* pString = value)
                {
                    char* pChar = pString;
                    for (int i = 0; i < strLength; i++)
                    {
                        _buffer[++_bufferIndex] = *pChar++;
                    }
                }
            }
        }

        public override void Write(char value)
        {
            _buffer[++_bufferIndex] = value;
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Array.Copy(buffer, index, _buffer, _bufferIndex, count);
            _bufferIndex += count;
        }

        public override void Write(char[] buffer)
        {
            int count = buffer.Length;
            Array.Copy(buffer, 0, _buffer, _bufferIndex, count);
            _bufferIndex += count;
        }

        public override string ToString()
        {
            return new string(_buffer, 0, _bufferIndex);
        }
    }
}
