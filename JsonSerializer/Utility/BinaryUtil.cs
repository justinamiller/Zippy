using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zippy.Utility
{
    internal sealed class BinaryUtil
    {
        static readonly Encoding StringEncoding = new UTF8Encoding(false);

        public const int ArrayMaxSize = 0x7FFFFFC7; // https://msdn.microsoft.com/en-us/library/system.array

            //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
         public static void EnsureCapacity<T>(ref T[] array, int offset, int appendLength)
        {
            var newLength = offset + appendLength;

            // If null(most case fisrt time) fill byte.
            if (array == null)
            {
                array = new T[newLength];
                return;
            }

        
            // like MemoryStream.EnsureCapacity
            var current = array.Length;
            if (newLength > current)
            {
                int num = newLength;
                if (num < 256)
                {
                    num = 256;
                    FastResize(ref array, num);
                    return;
                }

                if (current == ArrayMaxSize)
                {
                    throw new InvalidOperationException("T[] size reached maximum size of array(0x7FFFFFC7), can not write to single T[]. Details: https://msdn.microsoft.com/en-us/library/system.array");
                }

                var newSize = unchecked(current * 2);
                if (newSize < 0) // overflow
                {
                    num = ArrayMaxSize;
                }
                else if (num < newSize)
                {
                    num = newSize;
                }

                FastResize(ref array, num);
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
            int len = array2.Length;
            if (len != newSize)
            {
                T[] array3 = new T[newSize];
                Array.Copy(array2, 0, array3, 0, (len > newSize) ? newSize : len);

                array = array3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte[] FastCloneWithResize(byte[] src, int newSize)
        {
            if (src == null)
                return new byte[newSize];

            if (newSize < 0) throw new ArgumentOutOfRangeException("newSize");
            if (src.Length < newSize) throw new ArgumentException("length < newSize");

            byte[] dst = new byte[newSize];

#if NETSTANDARD && !NET45
            fixed (byte* pSrc = &src[0])
            fixed (byte* pDst = &dst[0])
            {
                Buffer.MemoryCopy(pSrc, pDst, dst.Length, newSize);
            }
#else
            Buffer.BlockCopy(src, 0, dst, 0, newSize);
#endif

            return dst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteBoolean(ref byte[] bytes, int offset, bool value)
        {
            EnsureCapacity(ref bytes, offset, 1);

            bytes[offset] = (byte)(value ? 1 : 0);
            return 1;
        }

        /// <summary>
        /// Unsafe! don't ensure capacity and don't return size.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBooleanUnsafe(ref byte[] bytes, int offset, bool value)
        {
            bytes[offset] = (byte)(value ? 1 : 0);
        }

        /// <summary>
        /// Unsafe! don't ensure capacity and don't return size.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBooleanTrueUnsafe(ref byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(1);
        }

        /// <summary>
        /// Unsafe! don't ensure capacity and don't return size.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBooleanFalseUnsafe(ref byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBoolean(ref byte[] bytes, int offset)
        {
            return (bytes[offset] == 0) ? false : true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteByte(ref byte[] bytes, int offset, byte value)
        {
            EnsureCapacity(ref bytes, offset, 1);

            bytes[offset] = value;
            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(ref byte[] bytes, int offset)
        {
            return bytes[offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteBytes(ref byte[] bytes, int offset, byte[] value)
        {
            EnsureCapacity(ref bytes, offset, value.Length);
            Buffer.BlockCopy(value, 0, bytes, offset, value.Length);
            return value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadBytes(ref byte[] bytes, int offset, int count)
        {
            var dest = new byte[count];
            Buffer.BlockCopy(bytes, offset, dest, 0, count);
            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteSByte(ref byte[] bytes, int offset, sbyte value)
        {
            EnsureCapacity(ref bytes, offset, 1);

            bytes[offset] = (byte)value;
            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSByte(ref byte[] bytes, int offset)
        {
            return (sbyte)bytes[offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteSingle(ref byte[] bytes, int offset, float value)
        {
            EnsureCapacity(ref bytes, offset, 4);

            if (offset % 4 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    *(float*)(ptr + offset) = value;
                }
            }
            else
            {
                uint num = *(uint*)(&value);
                bytes[offset] = (byte)num;
                bytes[offset + 1] = (byte)(num >> 8);
                bytes[offset + 2] = (byte)(num >> 16);
                bytes[offset + 3] = (byte)(num >> 24);
            }

            return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float ReadSingle(ref byte[] bytes, int offset)
        {
            if (offset % 4 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    return *(float*)(ptr + offset);
                }
            }
            else
            {
                uint num = (uint)((int)bytes[offset] | (int)bytes[offset + 1] << 8 | (int)bytes[offset + 2] << 16 | (int)bytes[offset + 3] << 24);
                return *(float*)(&num);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteDouble(ref byte[] bytes, int offset, double value)
        {
            EnsureCapacity(ref bytes, offset, 8);

            if (offset % 8 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    *(double*)(ptr + offset) = value;
                }
            }
            else
            {
                ulong num = (ulong)(*(long*)(&value));
                bytes[offset] = (byte)num;
                bytes[offset + 1] = (byte)(num >> 8);
                bytes[offset + 2] = (byte)(num >> 16);
                bytes[offset + 3] = (byte)(num >> 24);
                bytes[offset + 4] = (byte)(num >> 32);
                bytes[offset + 5] = (byte)(num >> 40);
                bytes[offset + 6] = (byte)(num >> 48);
                bytes[offset + 7] = (byte)(num >> 56);
            }

            return 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe double ReadDouble(ref byte[] bytes, int offset)
        {
            if (offset % 8 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    return *(double*)(ptr + offset);
                }
            }
            else
            {
                uint num = (uint)((int)bytes[offset] | (int)bytes[offset + 1] << 8 | (int)bytes[offset + 2] << 16 | (int)bytes[offset + 3] << 24);
                ulong num2 = (ulong)((int)bytes[offset + 4] | (int)bytes[offset + 5] << 8 | (int)bytes[offset + 6] << 16 | (int)bytes[offset + 7] << 24) << 32 | (ulong)num;
                return *(double*)(&num2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteInt16(ref byte[] bytes, int offset, short value)
        {
            EnsureCapacity(ref bytes, offset, 2);

            fixed (byte* ptr = bytes)
            {
                *(short*)(ptr + offset) = value;
            }

            return 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe short ReadInt16(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(short*)(ptr + offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteInt32(ref byte[] bytes, int offset, int value)
        {
            EnsureCapacity(ref bytes, offset, 4);

            fixed (byte* ptr = bytes)
            {
                *(int*)(ptr + offset) = value;
            }

            return 4;
        }

        /// <summary>
        /// Unsafe! don't ensure capacity and don't return size.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteInt32Unsafe(ref byte[] bytes, int offset, int value)
        {
            fixed (byte* ptr = bytes)
            {
                *(int*)(ptr + offset) = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadInt32(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(int*)(ptr + offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteInt64(ref byte[] bytes, int offset, long value)
        {
            EnsureCapacity(ref bytes, offset, 8);

            fixed (byte* ptr = bytes)
            {
                *(long*)(ptr + offset) = value;
            }

            return 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe long ReadInt64(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(long*)(ptr + offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteUInt16(ref byte[] bytes, int offset, ushort value)
        {
            EnsureCapacity(ref bytes, offset, 2);

            fixed (byte* ptr = bytes)
            {
                *(ushort*)(ptr + offset) = value;
            }

            return 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ushort ReadUInt16(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(ushort*)(ptr + offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteUInt32(ref byte[] bytes, int offset, uint value)
        {
            EnsureCapacity(ref bytes, offset, 4);

            fixed (byte* ptr = bytes)
            {
                *(uint*)(ptr + offset) = value;
            }

            return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint ReadUInt32(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(uint*)(ptr + offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteUInt64(ref byte[] bytes, int offset, ulong value)
        {
            EnsureCapacity(ref bytes, offset, 8);

            fixed (byte* ptr = bytes)
            {
                *(ulong*)(ptr + offset) = value;
            }

            return 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ulong ReadUInt64(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(ulong*)(ptr + offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteChar(ref byte[] bytes, int offset, char value)
        {
            return WriteUInt16(ref bytes, offset, (ushort)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ReadChar(ref byte[] bytes, int offset)
        {
            return (char)ReadUInt16(ref bytes, offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteString(ref byte[] bytes, int offset, string value)
        {
            var ensureSize = StringEncoding.GetMaxByteCount(value.Length);
            EnsureCapacity(ref bytes, offset, ensureSize);

            return StringEncoding.GetBytes(value, 0, value.Length, bytes, offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(ref byte[] bytes, int offset, int count)
        {
            return StringEncoding.GetString(bytes, offset, count);
        }

        // decimal underlying "flags, hi, lo, mid" fields are sequential and same layuout with .NET Framework and Mono(Unity)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteDecimal(ref byte[] bytes, int offset, decimal value)
        {
            EnsureCapacity(ref bytes, offset, 16);

            fixed (byte* ptr = bytes)
            {
                *(Decimal*)(ptr + offset) = value;
            }

            return 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe decimal ReadDecimal(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(Decimal*)(ptr + offset);
            }
        }

        // Guid's underlying _a,...,_k field is sequential and same layuout as .NET Framework and Mono(Unity)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteGuid(ref byte[] bytes, int offset, Guid value)
        {
            EnsureCapacity(ref bytes, offset, 16);

            fixed (byte* ptr = bytes)
            {
                *(Guid*)(ptr + offset) = value;
            }

            return 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Guid ReadGuid(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(Guid*)(ptr + offset);
            }
        }

        #region Timestamp/Duration

        public static unsafe int WriteTimeSpan(ref byte[] bytes, int offset, TimeSpan timeSpan)
        {
            checked
            {
                long ticks = timeSpan.Ticks;
                long seconds = ticks / TimeSpan.TicksPerSecond;
                int nanos = (int)(ticks % TimeSpan.TicksPerSecond) * Duration.NanosecondsPerTick;

                EnsureCapacity(ref bytes, offset, 12);
                fixed (byte* ptr = bytes)
                {
                    *(long*)(ptr + offset) = seconds;
                    *(int*)(ptr + offset + 8) = nanos;
                }

                return 12;
            }
        }

        public static unsafe TimeSpan ReadTimeSpan(ref byte[] bytes, int offset)
        {
            checked
            {
                fixed (byte* ptr = bytes)
                {
                    var seconds = *(long*)(ptr + offset);
                    var nanos = *(int*)(ptr + offset + 8);

                    if (!Duration.IsNormalized(seconds, nanos))
                    {
                        throw new InvalidOperationException("Duration was not a valid normalized duration");
                    }
                    long ticks = seconds * TimeSpan.TicksPerSecond + nanos / Duration.NanosecondsPerTick;
                    return TimeSpan.FromTicks(ticks);
                }
            }
        }

        public static unsafe int WriteDateTime(ref byte[] bytes, int offset, DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();

            // Do the arithmetic using DateTime.Ticks, which is always non-negative, making things simpler.
            long secondsSinceBclEpoch = dateTime.Ticks / TimeSpan.TicksPerSecond;
            int nanoseconds = (int)(dateTime.Ticks % TimeSpan.TicksPerSecond) * Duration.NanosecondsPerTick;

            EnsureCapacity(ref bytes, offset, 12);
            fixed (byte* ptr = bytes)
            {
                *(long*)(ptr + offset) = (secondsSinceBclEpoch - Timestamp.BclSecondsAtUnixEpoch);
                *(int*)(ptr + offset + 8) = nanoseconds;
            }

            return 12;
        }

        public static unsafe DateTime ReadDateTime(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                var seconds = *(long*)(ptr + offset);
                var nanos = *(int*)(ptr + offset + 8);

                if (!Timestamp.IsNormalized(seconds, nanos))
                {
                    throw new InvalidOperationException(string.Format(@"Timestamp contains invalid values: Seconds={0}; Nanos={1}", seconds, nanos));
                }
                return Timestamp.UnixEpoch.AddSeconds(seconds).AddTicks(nanos / Duration.NanosecondsPerTick);
            }
        }

        internal static class Timestamp
        {
            internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            internal const long BclSecondsAtUnixEpoch = 62135596800;
            internal const long UnixSecondsAtBclMaxValue = 253402300799;
            internal const long UnixSecondsAtBclMinValue = -BclSecondsAtUnixEpoch;
            internal const int MaxNanos = Duration.NanosecondsPerSecond - 1;

            internal static bool IsNormalized(long seconds, int nanoseconds)
            {
                return nanoseconds >= 0 &&
                    nanoseconds <= MaxNanos &&
                    seconds >= UnixSecondsAtBclMinValue &&
                    seconds <= UnixSecondsAtBclMaxValue;
            }
        }

        internal static class Duration
        {
            public const int NanosecondsPerSecond = 1000000000;
            public const int NanosecondsPerTick = 100;
            public const long MaxSeconds = 315576000000L;
            public const long MinSeconds = -315576000000L;
            internal const int MaxNanoseconds = NanosecondsPerSecond - 1;
            internal const int MinNanoseconds = -NanosecondsPerSecond + 1;

            internal static bool IsNormalized(long seconds, int nanoseconds)
            {
                // Simple boundaries
                if (seconds < MinSeconds || seconds > MaxSeconds ||
                    nanoseconds < MinNanoseconds || nanoseconds > MaxNanoseconds)
                {
                    return false;
                }
                // We only have a problem is one is strictly negative and the other is
                // strictly positive.
                return Math.Sign(seconds) * Math.Sign(nanoseconds) != -1;
            }
        }

        #endregion
    }
}
