using JsonSerializer.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace JsonSerializer
{
    //public struct JsonByteWriter
    //{
    //    private static readonly byte[] emptyBytes = new byte[0];

    //    internal byte[] buffer;

    //    internal int offset;

    //    public int CurrentOffset => offset;

    //    public void AdvanceOffset(int offset)
    //    {
    //        checked
    //        {
    //            this.offset += offset;
    //        }
    //    }

    //    public static byte[] GetEncodedPropertyName(string propertyName)
    //    {
    //        JsonByteWriter JsonByteWriter = default(JsonByteWriter);
    //        JsonByteWriter.WritePropertyName(propertyName);
    //        return JsonByteWriter.ToUtf8ByteArray();
    //    }

    //    public static byte[] GetEncodedPropertyNameWithPrefixValueSeparator(string propertyName)
    //    {
    //        JsonByteWriter JsonByteWriter = default(JsonByteWriter);
    //        JsonByteWriter.WriteValueSeparator();
    //        JsonByteWriter.WritePropertyName(propertyName);
    //        return JsonByteWriter.ToUtf8ByteArray();
    //    }

    //    public static byte[] GetEncodedPropertyNameWithBeginObject(string propertyName)
    //    {
    //        JsonByteWriter JsonByteWriter = default(JsonByteWriter);
    //        JsonByteWriter.WriteBeginObject();
    //        JsonByteWriter.WritePropertyName(propertyName);
    //        return JsonByteWriter.ToUtf8ByteArray();
    //    }

    //    public static byte[] GetEncodedPropertyNameWithoutQuotation(string propertyName)
    //    {
    //        JsonByteWriter JsonByteWriter = default(JsonByteWriter);
    //        JsonByteWriter.WriteString(propertyName);
    //        ArraySegment<byte> arraySegment = JsonByteWriter.GetBuffer();
    //        checked
    //        {
    //            byte[] array = new byte[arraySegment.Count - 2];
    //            Buffer.BlockCopy(arraySegment.Array, arraySegment.Offset + 1, array, 0, array.Length);
    //            return array;
    //        }
    //    }

    //    public JsonByteWriter(byte[] initialBuffer)
    //    {
    //        buffer = initialBuffer;
    //        offset = 0;
    //    }

    //    public ArraySegment<byte> GetBuffer()
    //    {
    //        if (buffer == null)
    //        {
    //            return new ArraySegment<byte>(emptyBytes, 0, 0);
    //        }
    //        return new ArraySegment<byte>(buffer, 0, offset);
    //    }

    //    public byte[] ToUtf8ByteArray()
    //    {
    //        if (buffer == null)
    //        {
    //            return emptyBytes;
    //        }
    //        return BinaryUtil.FastCloneWithResize(buffer, offset);
    //    }

    //    public override string ToString()
    //    {
    //        if (buffer == null)
    //        {
    //            return null;
    //        }
    //        return Encoding.UTF8.GetString(buffer, 0, offset);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void EnsureCapacity(int appendLength)
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, appendLength);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteRaw(byte rawValue)
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
    //        buffer[checked(offset++)] = rawValue;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteRaw(byte[] rawValue)
    //    {
    //        //UnsafeMemory.WriteRaw(ref this, rawValue);
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, rawValue.Length);
    //        foreach(var b in rawValue)
    //        {
    //            buffer[checked(offset++)] = b;
    //        }

    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteRawUnsafe(byte rawValue)
    //    {
    //        buffer[checked(offset++)] = rawValue;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteBeginArray()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
    //        buffer[checked(offset++)] = 91;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteEndArray()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
    //        buffer[checked(offset++)] = 93;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteBeginObject()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
    //        buffer[checked(offset++)] = 123;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteEndObject()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
    //        buffer[checked(offset++)] = 125;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteValueSeparator()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
    //        buffer[checked(offset++)] = 44;
    //    }

    //    //
    //    // Summary:
    //    //     :
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteNameSeparator()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
    //        buffer[checked(offset++)] = 58;
    //    }

    //    //
    //    // Summary:
    //    //     WriteString + WriteNameSeparator
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WritePropertyName(string propertyName)
    //    {
    //        WriteString(propertyName);
    //        WriteNameSeparator();
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteQuotation()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
    //        buffer[checked(offset++)] = 34;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteNull()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 4);
    //        buffer[offset] = 110;
    //        checked
    //        {
    //            buffer[offset + 1] = 117;
    //            buffer[offset + 2] = 108;
    //            buffer[offset + 3] = 108;
    //            offset += 4;
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteBoolean(bool value)
    //    {
    //        checked
    //        {
    //            if (value)
    //            {
    //                BinaryUtil.EnsureCapacity(ref buffer, offset, 4);
    //                buffer[offset] = 116;
    //                buffer[offset + 1] = 114;
    //                buffer[offset + 2] = 117;
    //                buffer[offset + 3] = 101;
    //                offset += 4;
    //            }
    //            else
    //            {
    //                BinaryUtil.EnsureCapacity(ref buffer, offset, 5);
    //                buffer[offset] = 102;
    //                buffer[offset + 1] = 97;
    //                buffer[offset + 2] = 108;
    //                buffer[offset + 3] = 115;
    //                buffer[offset + 4] = 101;
    //                offset += 5;
    //            }
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteTrue()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 4);
    //        buffer[offset] = 116;
    //        checked
    //        {
    //            buffer[offset + 1] = 114;
    //            buffer[offset + 2] = 117;
    //            buffer[offset + 3] = 101;
    //            offset += 4;
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteFalse()
    //    {
    //        BinaryUtil.EnsureCapacity(ref buffer, offset, 5);
    //        buffer[offset] = 102;
    //        checked
    //        {
    //            buffer[offset + 1] = 97;
    //            buffer[offset + 2] = 108;
    //            buffer[offset + 3] = 115;
    //            buffer[offset + 4] = 101;
    //            offset += 5;
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteSingle(float value)
    //    {
    //        checked
    //        {
    //            offset += DoubleToStringConverter.GetBytes(ref buffer, offset, value);
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteDouble(double value)
    //    {
    //        checked
    //        {
    //            offset += DoubleToStringConverter.GetBytes(ref buffer, offset, value);
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteByte(byte value)
    //    {
    //        WriteUInt64(value);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteUInt16(ushort value)
    //    {
    //        WriteUInt64(value);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteUInt32(uint value)
    //    {
    //        WriteUInt64(value);
    //    }

    //    public void WriteUInt64(ulong value)
    //    {
    //        checked
    //        {
    //            offset += NumberConverter.WriteUInt64(ref buffer, offset, value);
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteSByte(sbyte value)
    //    {
    //        WriteInt64(value);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteInt16(short value)
    //    {
    //        WriteInt64(value);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void WriteInt32(int value)
    //    {
    //        WriteInt64(value);
    //    }

    //    public void WriteInt64(long value)
    //    {
    //        checked
    //        {
    //            offset += NumberConverter.WriteInt64(ref buffer, offset, value);
    //        }
    //    }

    //    public void WriteString(string value)
    //    {
    //        checked
    //        {
    //            if (value == null)
    //            {
    //                WriteNull();
    //            }
    //            else
    //            {
    //                int num = offset;
    //                int num2 = StringEncoding.UTF8.GetMaxByteCount(value.Length) + 2;
    //                BinaryUtil.EnsureCapacity(ref buffer, num, num2);
    //                int num3 = 0;
    //                int length = value.Length;
    //                buffer[offset++] = 34;
    //                for (int i = 0; i < value.Length; i++)
    //                {
    //                    byte b = 0;
    //                    switch (value[i])
    //                    {
    //                        case '"':
    //                            b = 34;
    //                            goto IL_0211;
    //                        case '\\':
    //                            b = 92;
    //                            goto IL_0211;
    //                        case '\b':
    //                            b = 98;
    //                            goto IL_0211;
    //                        case '\f':
    //                            b = 102;
    //                            goto IL_0211;
    //                        case '\n':
    //                            b = 110;
    //                            goto IL_0211;
    //                        case '\r':
    //                            b = 114;
    //                            goto IL_0211;
    //                        case '\t':
    //                            {
    //                                b = 116;
    //                                goto IL_0211;
    //                            }
    //                        IL_0211:
    //                            num2 += 2;
    //                            BinaryUtil.EnsureCapacity(ref buffer, num, num2);
    //                            offset += StringEncoding.UTF8.GetBytes(value, num3, i - num3, buffer, offset);
    //                            num3 = i + 1;
    //                            buffer[offset++] = 92;
    //                            buffer[offset++] = b;
    //                            break;
    //                    }
    //                }
    //                if (num3 != value.Length)
    //                {
    //                    offset += StringEncoding.UTF8.GetBytes(value, num3, value.Length - num3, buffer, offset);
    //                }
    //                buffer[offset++] = 34;
    //            }
    //        }
    //    }
    //}
}
