//========================================================================
/// 公用内存管理器
/// @neoyang
/// @2015.01.09
//========================================================================

using UnityEngine;
using System.Collections;
using System.Text;
using System;

public class CMemoryManager
{
    //private const int c_arraySize = 100;

    //public static int[] s_intArray = new int[c_arraySize];
    //public static MonoBehaviour[] s_scriptArray = new MonoBehaviour[c_arraySize];
    //public static GameObject[] s_gameObjectArray = new GameObject[c_arraySize];
    //public static string[] s_stringArray = new string[c_arraySize];

    //内存Copy委托(需要调用C++实现)
    public delegate void DelegateCopyMemory(IntPtr dest, IntPtr src, int length);
    public static DelegateCopyMemory s_copyMemory = null;

    //----------------------------------------------
    /// 注册回调
    /// @copyMemory
    //----------------------------------------------
    public static void RegisterDelegate(DelegateCopyMemory copyMemory)
    {
        s_copyMemory = copyMemory;
    }

    //----------------------------------------------
    /// 写入byte数据
    /// @value
    /// @data
    /// @offset
    //----------------------------------------------
    public static void WriteByte(byte value, byte[] data, ref int offset)
    {
        data[offset] = (byte)value;

        offset++;
    }

    //----------------------------------------------
    /// 写入short数据[高高低低]
    /// @value
    /// @data
    /// @offset
    //----------------------------------------------
    public static void WriteShort(short value, byte[] data, ref int offset)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);

        offset += 2;
    }

    //----------------------------------------------
    /// 写入int数据[高高低低]
    /// @value
    /// @data
    /// @offset
    //----------------------------------------------
    public static void WriteInt(int value, byte[] data, ref int offset)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);

        offset += 4;
    }

    //----------------------------------------------
    /// 写入long数据[高高低低]
    /// @value
    /// @data
    /// @offset
    //----------------------------------------------
    public static void WriteLong(long value, byte[] data, ref int offset)
    {
        uint value1 = (uint)value;
        uint value2 = (uint)((ulong)value >> 32);

        WriteInt((int)value1, data, ref offset);
        WriteInt((int)value2, data, ref offset);
    }

    //----------------------------------------------
    /// 读出byte数据
    /// @data
    /// @offset
    /// @return byte value
    //----------------------------------------------
    public static int ReadByte(byte[] data, ref int offset)
    {
        int value = data[offset];

        offset++;

        return value;
    }

    //----------------------------------------------
    /// 读出short数据[高高低低]
    /// @data
    /// @offset
    /// @return short value
    //----------------------------------------------
    public static int ReadShort(byte[] data, ref int offset)
    {
        int value = ((data[offset + 1] << 8) | data[offset]);

        offset += 2;

        return value;
    }

    //----------------------------------------------
    /// 读出int数据[高高低低]
    /// @data
    /// @offset
    /// @return short value
    //----------------------------------------------
    public static int ReadInt(byte[] data, ref int offset)
    {
        int value = ((data[offset + 3] << 24) | (data[offset + 2] << 16) | (data[offset + 1] << 8) | data[offset]);

        offset += 4;

        return value;
    }

    //----------------------------------------------
    /// 读出long数据[高高低低]
    /// @data
    /// @offset
    /// @return short value
    //----------------------------------------------
    public static long ReadLong(byte[] data, ref int offset)
    {
        uint value1 = (uint)ReadInt(data, ref offset);
        uint value2 = (uint)ReadInt(data, ref offset);

        return (long)((((ulong)value2) << 32) | ((ulong)value1));
    }

    //----------------------------------------------
    /// 写入字符串数据
    /// @按UTF-8编码，长度占2字节
    /// @str
    /// @data
    /// @offset
    /// @return offset after write
    //----------------------------------------------
    public static void WriteString(string str, byte[] data, ref int offset)
    {
        //写入字符串数据
        int length = System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, data, offset + 2);
        
        //写入字符串数据长度
        WriteShort((short)length, data, ref offset);

        offset += length;
    }

    //----------------------------------------------
    /// 读入字符串数据
    /// @按UTF-8编码，长度占2字节
    /// @data
    /// @offset
    /// @return str
    //----------------------------------------------
    public static string ReadString(byte[] data, ref int offset)
    {
        //读出字符串数据长度
        int length = ReadShort(data, ref offset);

        //读出字符串
        string str = System.Text.Encoding.UTF8.GetString(data, offset, length);

        offset += length;

        return str;
    }

    //----------------------------------------------
    /// 跳过读入字符串数据
    /// @按UTF-8编码，长度占2字节
    /// @data
    /// @offset
    //----------------------------------------------
    public static void SkipString(byte[] data, ref int offset)
    {
        //读出字符串数据长度
        int length = ReadShort(data, ref offset);
        offset += length;
    }

    //----------------------------------------------
    /// 写入DateTime
    /// @datetIME
    /// @data
    /// @offset
    //----------------------------------------------
    public static void WriteDateTime(ref DateTime dateTime, byte[] data, ref int offset)
    {
        byte[] buffer = BitConverter.GetBytes(dateTime.Ticks);

        for (int i = 0; i < buffer.Length; i++)
        {
            data[offset] = buffer[i];
            offset++;
        }
    }

    //----------------------------------------------
    /// 读入DateTime
    /// @data
    /// @offset
    /// @return DateTime
    //----------------------------------------------
    public static DateTime ReadDateTime(byte[] data, ref int offset)
    {
        long ticks = BitConverter.ToInt64(data, offset);
        offset += 8;

        if (ticks < DateTime.MaxValue.Ticks && ticks > DateTime.MinValue.Ticks)   
        {   
            DateTime dateTime = new DateTime(ticks);
            return dateTime;
        }

        return new DateTime();
    }

    public static unsafe bool Copy(byte[] dst, byte[] src, uint length)
    {
        if (dst.Length < length || src.Length < length)
        {
            return false;
        }

        if (s_copyMemory != null)
        {
            fixed (byte* pDst = dst)
            {
                fixed (byte* pSrc = src)
                {
                    s_copyMemory(new IntPtr(pDst), new IntPtr(pSrc), (int)length);
                }
            }
        }
        else
        {
            Array.Copy(src, dst, length);
        }

        return true;
    }
};

