using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zippy.Serialize.Writers
{
    public sealed class BufferTextWriter : TextWriter
    {
        private char[] _buffer = new char[1000];
        int _bufferIndex = 0;
        private readonly static Encoding s_encoding = Encoding.Default;

        public override Encoding Encoding { get; } = s_encoding;

        public override void Write(string value)
        {


            //int length = value.Length;
            //char[] chars = new char[length];
            //if (length > 0)
            //{
            //    unsafe
            //    {
            //        fixed (char* src = value)
            //        fixed (char* dest = chars)
            //        {
            //            Array.Copy()
            //            Buffer.MemoryCopy()
            //            Buffer.Memcpy((byte*)dmem, (byte*)smem, charCount * 2);
            //            wstrcpy(dest, src, length);
            //        }
            //    }
            //}
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

                //fixed (char* pString = value)
                //{
                //    char* pChar = pString;
                //    while (*pChar != '\0')
                //    {
                //        _buffer[++_bufferIndex] = *pChar++;
                //    }
                //}
            }




            //var buffer = value.ToCharArray();
            //int count = buffer.Length;
            //Array.Copy(buffer, 0, _buffer, _bufferIndex, count);
            //_bufferIndex += count;
            //foreach (var v in value)
            //{
            //    _buffer[++_bufferIndex] = v;
            //}
        }

        public override void Write(char value)
        {
            _buffer[++_bufferIndex] = value;
        }

        public override void Write(char[] buffer, int index, int count)
        {
        //    Array.Copy(buffer, index, _buffer, _bufferIndex, count);
        //    _bufferIndex += count;
        }

        public override void Write(char[] buffer)
        {
        //    int count = buffer.Length;
      //      Array.Copy(buffer, 0, _buffer, _bufferIndex, count);
        //    _bufferIndex += count;
        }

        public override string ToString()
        {
            return new string(_buffer, 0, _bufferIndex);
        }
    }
}
