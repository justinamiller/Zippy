using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Zippy.Serialize
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct FastGuidStruct
    {
        static readonly char[] s_writeGuidLookup = new char[] { '0', '0', '0', '1', '0', '2', '0', '3', '0', '4', '0', '5', '0', '6', '0', '7', '0', '8', '0', '9', '0', 'a', '0', 'b', '0', 'c', '0', 'd', '0', 'e', '0', 'f', '1', '0', '1', '1', '1', '2', '1', '3', '1', '4', '1', '5', '1', '6', '1', '7', '1', '8', '1', '9', '1', 'a', '1', 'b', '1', 'c', '1', 'd', '1', 'e', '1', 'f', '2', '0', '2', '1', '2', '2', '2', '3', '2', '4', '2', '5', '2', '6', '2', '7', '2', '8', '2', '9', '2', 'a', '2', 'b', '2', 'c', '2', 'd', '2', 'e', '2', 'f', '3', '0', '3', '1', '3', '2', '3', '3', '3', '4', '3', '5', '3', '6', '3', '7', '3', '8', '3', '9', '3', 'a', '3', 'b', '3', 'c', '3', 'd', '3', 'e', '3', 'f', '4', '0', '4', '1', '4', '2', '4', '3', '4', '4', '4', '5', '4', '6', '4', '7', '4', '8', '4', '9', '4', 'a', '4', 'b', '4', 'c', '4', 'd', '4', 'e', '4', 'f', '5', '0', '5', '1', '5', '2', '5', '3', '5', '4', '5', '5', '5', '6', '5', '7', '5', '8', '5', '9', '5', 'a', '5', 'b', '5', 'c', '5', 'd', '5', 'e', '5', 'f', '6', '0', '6', '1', '6', '2', '6', '3', '6', '4', '6', '5', '6', '6', '6', '7', '6', '8', '6', '9', '6', 'a', '6', 'b', '6', 'c', '6', 'd', '6', 'e', '6', 'f', '7', '0', '7', '1', '7', '2', '7', '3', '7', '4', '7', '5', '7', '6', '7', '7', '7', '8', '7', '9', '7', 'a', '7', 'b', '7', 'c', '7', 'd', '7', 'e', '7', 'f', '8', '0', '8', '1', '8', '2', '8', '3', '8', '4', '8', '5', '8', '6', '8', '7', '8', '8', '8', '9', '8', 'a', '8', 'b', '8', 'c', '8', 'd', '8', 'e', '8', 'f', '9', '0', '9', '1', '9', '2', '9', '3', '9', '4', '9', '5', '9', '6', '9', '7', '9', '8', '9', '9', '9', 'a', '9', 'b', '9', 'c', '9', 'd', '9', 'e', '9', 'f', 'a', '0', 'a', '1', 'a', '2', 'a', '3', 'a', '4', 'a', '5', 'a', '6', 'a', '7', 'a', '8', 'a', '9', 'a', 'a', 'a', 'b', 'a', 'c', 'a', 'd', 'a', 'e', 'a', 'f', 'b', '0', 'b', '1', 'b', '2', 'b', '3', 'b', '4', 'b', '5', 'b', '6', 'b', '7', 'b', '8', 'b', '9', 'b', 'a', 'b', 'b', 'b', 'c', 'b', 'd', 'b', 'e', 'b', 'f', 'c', '0', 'c', '1', 'c', '2', 'c', '3', 'c', '4', 'c', '5', 'c', '6', 'c', '7', 'c', '8', 'c', '9', 'c', 'a', 'c', 'b', 'c', 'c', 'c', 'd', 'c', 'e', 'c', 'f', 'd', '0', 'd', '1', 'd', '2', 'd', '3', 'd', '4', 'd', '5', 'd', '6', 'd', '7', 'd', '8', 'd', '9', 'd', 'a', 'd', 'b', 'd', 'c', 'd', 'd', 'd', 'e', 'd', 'f', 'e', '0', 'e', '1', 'e', '2', 'e', '3', 'e', '4', 'e', '5', 'e', '6', 'e', '7', 'e', '8', 'e', '9', 'e', 'a', 'e', 'b', 'e', 'c', 'e', 'd', 'e', 'e', 'e', 'f', 'f', '0', 'f', '1', 'f', '2', 'f', '3', 'f', '4', 'f', '5', 'f', '6', 'f', '7', 'f', '8', 'f', '9', 'f', 'a', 'f', 'b', 'f', 'c', 'f', 'd', 'f', 'e', 'f', 'f' };

        public Guid Raw
        {
            get
            {
                return _value;
            }
        }

        [FieldOffset(0)]
        private readonly Guid _value;

        [FieldOffset(0)]
        public readonly byte B00;
        [FieldOffset(1)]
        public readonly byte B01;
        [FieldOffset(2)]
        public readonly byte B02;
        [FieldOffset(3)]
        public readonly byte B03;
        [FieldOffset(4)]
        public readonly byte B04;
        [FieldOffset(5)]
        public readonly byte B05;

        [FieldOffset(6)]
        public readonly byte B06;
        [FieldOffset(7)]
        public readonly byte B07;
        [FieldOffset(8)]
        public readonly byte B08;
        [FieldOffset(9)]
        public readonly byte B09;

        [FieldOffset(10)]
        public readonly byte B10;
        [FieldOffset(11)]
        public readonly byte B11;

        [FieldOffset(12)]
        public readonly byte B12;
        [FieldOffset(13)]
        public readonly byte B13;
        [FieldOffset(14)]
        public readonly byte B14;
        [FieldOffset(15)]
        public readonly byte B15;


        public FastGuidStruct(Guid guid)
            : this()
        {
            _value = guid;
        }

        public char[] GetBuffer()
        {
            char[] buffer = new char[36] ;
            buffer[8] = '-';
            buffer[13] = '-';
            buffer[18] = '-';
            buffer[23] = '-';

            // bytes[0]
            var b = this.B00 * 2;
            buffer[6] = s_writeGuidLookup[b];
            buffer[7] = s_writeGuidLookup[b + 1];

            // bytes[1]
            b = this.B01 * 2;
            buffer[4] = s_writeGuidLookup[b];
            buffer[5] = s_writeGuidLookup[b + 1];

            // bytes[2]
            b = this.B02 * 2;
            buffer[2] = s_writeGuidLookup[b];
            buffer[3] = s_writeGuidLookup[b + 1];

            // bytes[3]
            b = this.B03 * 2;
            buffer[0] = s_writeGuidLookup[b];
            buffer[1] = s_writeGuidLookup[b + 1];

            // bytes[4]
            b = this.B04 * 2;
            buffer[11] = s_writeGuidLookup[b];
            buffer[12] = s_writeGuidLookup[b + 1];

            // bytes[5]
            b = this.B05 * 2;
            buffer[9] = s_writeGuidLookup[b];
            buffer[10] = s_writeGuidLookup[b + 1];

            // bytes[6]
            b = this.B06 * 2;
            buffer[16] = s_writeGuidLookup[b];
            buffer[17] = s_writeGuidLookup[b + 1];

            // bytes[7]
            b = this.B07 * 2;
            buffer[14] = s_writeGuidLookup[b];
            buffer[15] = s_writeGuidLookup[b + 1];

            // bytes[8]
            b = this.B08 * 2;
            buffer[19] = s_writeGuidLookup[b];
            buffer[20] = s_writeGuidLookup[b + 1];

            // bytes[9]
            b = this.B09 * 2;
            buffer[21] = s_writeGuidLookup[b];
            buffer[22] = s_writeGuidLookup[b + 1];

            // bytes[10]
            b = this.B10 * 2;
            buffer[24] = s_writeGuidLookup[b];
            buffer[25] = s_writeGuidLookup[b + 1];

            // bytes[11]
            b = this.B11 * 2;
            buffer[26] = s_writeGuidLookup[b];
            buffer[27] = s_writeGuidLookup[b + 1];

            // bytes[12]
            b = this.B12 * 2;
            buffer[28] = s_writeGuidLookup[b];
            buffer[29] = s_writeGuidLookup[b + 1];

            // bytes[13]
            b = this.B13 * 2;
            buffer[30] = s_writeGuidLookup[b];
            buffer[31] = s_writeGuidLookup[b + 1];

            // bytes[14]
            b = this.B14 * 2;
            buffer[32] = s_writeGuidLookup[b];
            buffer[33] = s_writeGuidLookup[b + 1];

            // bytes[15]
            b = this.B15 * 2;
            buffer[34] = s_writeGuidLookup[b];
            buffer[35] = s_writeGuidLookup[b + 1];

            return buffer;
        }

        public override string ToString()
        {
            return new string(GetBuffer());
        }
    }
}
