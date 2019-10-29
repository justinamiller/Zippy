using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zippy.Utility
{
    internal sealed class BinaryUtil
    {

        public const int ArrayMaxSize = 0x7FFFFFC7; // https://msdn.microsoft.com/en-us/library/system.array


        public static void EnsureCapacityFixed<T>(ref T[] array, int offset, int appendLength)
        {
            var newLength = offset + appendLength;

            var current = array.Length;
            if (newLength > current)
            {
                var newSize = current + (newLength * 2);
     
                FastResize(ref array, newSize);
            }
        }


        // Buffer.BlockCopy version of Array.Resize
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastResize<T>(ref T[] array, int newSize)
        {
            if (newSize < 0) throw new ArgumentOutOfRangeException(nameof(newSize));

            T[] array2 = array;
            if (array2 == null)
            {
                array = new T[newSize];
                return;
            }
            int len = array2.Length;
            if (len != newSize)
            {
                T[] array3 = new T[newSize];
                Array.Copy(array2, 0, array3, 0, (len > newSize) ? newSize : len);

                array = array3;
            }
        }
    }
}
