using System.Runtime.CompilerServices;

namespace StarResonanceDpsAnalysis.Core.Extends.System
{
    public static class ByteExtends
    {
        #region ReadInt32BigEndian()

        /// <summary>
        /// 从头 4 字节数组读取大端序 32 位整数; 转换失败时抛出异常
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32BigEndianEx(this byte[] buf)
        {
            if (buf.Length < 4)
                throw new ArgumentException("至少4字节才能转出");

            return (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
        }

        /// <summary>
        /// 从 4 字节数组读取大端序 32 位整数; 转换失败时返回默认值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32BigEndian(this byte[] buf, int def = 0)
        {
            try
            {
                return ReadInt32BigEndianEx(buf);
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// 尝试从 4 字节数组读取大端序 32 位整数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt32BigEndian(this byte[] buf, out int result)
        {
            result = 0;
            try
            {
                result = ReadInt32BigEndianEx(buf);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ReadUInt32BigEndian()

        /// <summary>
        /// 从头 4 字节数组读取大端序 32 位无符号整数; 转换失败时抛出异常
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32BigEndianEx(this byte[] buf)
        {
            if (buf.Length < 4)
                throw new ArgumentException("至少4字节才能转出");

            return (uint)((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3]);
        }

        /// <summary>
        /// 从 4 字节数组读取大端序 32 位无符号整数; 转换失败时返回默认值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32BigEndian(this byte[] buf, uint def = 0)
        {
            try
            {
                return ReadUInt32BigEndianEx(buf);
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// 尝试从 4 字节数组读取大端序 32 位无符号整数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt32BigEndian(this byte[] buf, out uint result)
        {
            result = 0;
            try
            {
                result = ReadUInt32BigEndianEx(buf);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
