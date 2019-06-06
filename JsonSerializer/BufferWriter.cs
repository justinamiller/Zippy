using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JsonSerializer.Utility;

namespace JsonSerializer
{
    class BufferWriter : TextWriter
    {
        private const int _defaultLength = 1024;

        private int _length = _defaultLength;
        private char[] _buffer = new char[_defaultLength];
        private int _offset = 0;

        public override Encoding Encoding
        {
            get
            {
                return Encoding.Default;
            }
        }

        public override string ToString()
        {
            //if (_buffer.Length > _offset)
            //{
            //    Array.Resize(ref _buffer, _offset);
            //}
            return new string(_buffer, 0, _offset);
        }


        public override void Write(char buffer)
        {
            //EnsureCapacity(1);
            _buffer[_offset++] = buffer;
        }

        public override void Write(char[] buffer)
        {
            if (buffer == null)
            {
                return;
            }
            Write(buffer, 0, buffer.Length);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            //  EnsureCapacity(count);

            //Array.Copy(buffer, 0, _buffer, _offset, count);
            //  _offset += count;

            unsafe
            {
                fixed (char* chr = buffer)
                {

                    /* let us have array address in pointer */
                    for (int i = 0; i < count; i++)
                    {
                        _buffer[_offset++] = *(chr + i);
                    }

                }
            }
        }

        public override void Write(string value)
        {
            if (value == null)
            {
                return;
            }
            //int len = value.Length;
            //var chr= value.ToCharArray();
            //Write(chr, 0, chr.Length);

            //EnsureCapacity(len);


            unsafe
            {
                fixed (char* chr = value)
                {
                    int len = value.Length;
                    for (int i = 0; i < len; i++)
                    {
                        _buffer[_offset++] = *(chr + i);
                    }
                }
            }


            //unsafe
            //{
            //    char c;
            //    fixed (char* chr = value)
            //    {
            //        char* ptr = chr;
            //        while ((c = *(ptr++)) != '\0')
            //        {
            //            _buffer[_offset] = c;
            //        }
            //    }
            //}
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int appendLength)
        {
            var newLength = _offset + appendLength;
            // like MemoryStream.EnsureCapacity

            if (newLength > _length)
            {
                int num = newLength;

                var newSize = unchecked((_length * 2));
                if (newSize < 0) // overflow
                {
                    num = 0x7FFFFFC7;
                }
                else
                {
                    if (num < newSize)
                    {
                        num = newSize;
                    }
                }

                FastResize(ref _buffer, num);
                _length = num;
            }
        }

        // Buffer.BlockCopy version of Array.Resize
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastResize<T>(ref T[] array, int newSize)
        {
            if (newSize < 0) throw new ArgumentOutOfRangeException("newSize");

            T[] array2 = array;
            if (array2 == null)
            {
                array = new T[newSize];
                return;
            }

            if (array2.Length != newSize)
            {
                T[] array3 = new T[newSize];
                Buffer.BlockCopy(array2, 0, array3, 0, (array2.Length > newSize) ? newSize : array2.Length);
                array = array3;
            }
        }

    }
}
