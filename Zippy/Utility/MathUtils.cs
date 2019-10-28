using System.Runtime.CompilerServices;

namespace Zippy.Utility
{
    static class MathUtils
    {
        internal readonly static char[] charNumbers = new char[10]
   {
            '0','1','2','3','4','5','6','7','8','9'
   };

        public static int GetIntLength(this ulong n)
        {
            int num = 0;
            do
            {
                num++;
                n /= 10uL;
            }
            while (n != 0uL);
            return num;
        }

        public static char[] WriteNumberToBuffer(uint value, bool negative)
        {
            int  totalLength = GetIntLength(value);

            if (negative)
            {
                totalLength++;
            }
            char[] buffer = new char[totalLength];

            if (negative)
            {
                buffer[0] = '-';
            }

            do
            {
                uint quotient = value / 10;
                uint digit = value - (quotient * 10);
                buffer[--totalLength] = charNumbers[digit];
                value = quotient;
            } while (value != 0);

            return buffer;
        }

        public static char[] WriteNumberToBuffer(ulong value, bool negative)
        {
            if (value <= uint.MaxValue)
            {
                // avoid the 64 bit division if possible
                return WriteNumberToBuffer((uint)value, negative);
            }

           int  totalLength = GetIntLength(value);
            if (negative)
            {
                totalLength++;
            }
            char[] buffer = new char[totalLength];

            if (negative)
            {
                buffer[0] = '-';
            }

            int index = totalLength;

            do
            {
                ulong quotient = value / 10;
                ulong digit = value - (quotient * 10);
                buffer[--index] = charNumbers[digit];
                value = quotient;
            } while (value != 0);

            return buffer;
        }
    }
}
