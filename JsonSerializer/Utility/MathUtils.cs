using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SwiftJson.Utility
{
    static class MathUtils
    {
        public static int IntLength(ulong i)
        {
            if (i < 10000000000)
            {
                if (i < 10)
                {
                    return 1;
                }
                if (i < 100)
                {
                    return 2;
                }
                if (i < 1000)
                {
                    return 3;
                }
                if (i < 10000)
                {
                    return 4;
                }
                if (i < 100000)
                {
                    return 5;
                }
                if (i < 1000000)
                {
                    return 6;
                }
                if (i < 10000000)
                {
                    return 7;
                }
                if (i < 100000000)
                {
                    return 8;
                }
                if (i < 1000000000)
                {
                    return 9;
                }

                return 10;
            }
            else
            {
                if (i < 100000000000)
                {
                    return 11;
                }
                if (i < 1000000000000)
                {
                    return 12;
                }
                if (i < 10000000000000)
                {
                    return 13;
                }
                if (i < 100000000000000)
                {
                    return 14;
                }
                if (i < 1000000000000000)
                {
                    return 15;
                }
                if (i < 10000000000000000)
                {
                    return 16;
                }
                if (i < 100000000000000000)
                {
                    return 17;
                }
                if (i < 1000000000000000000)
                {
                    return 18;
                }
                if (i < 10000000000000000000)
                {
                    return 19;
                }

                return 20;
            }
        }

        // Micro optimized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertIntToHex(int intValue, char[] hex)
        {
            for (int i = 3; i >= 0; i--)
            {
                int num = intValue & 0xF; // intValue % 16

                // 0x30 + num == '0' + num
                // 0x37 + num == 'A' + (num - 10)
                hex[i] = (char)((num < 10 ? 0x30 : 0x37) + num);

                intValue >>= 4;
            }
        }

        public static string EnsureDecimalPlace(double value, string text)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || text.IndexOf('.') != -1 || text.IndexOf('E') != -1 || text.IndexOf('e') != -1)
            {
                return text;
            }

            return text + ".0";
        }
    }
}
