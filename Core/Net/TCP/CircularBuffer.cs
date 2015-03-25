using socket4net.Log;

namespace socket4net.Net.TCP
{
    /// <summary>
    /// 循环buffer
    /// </summary>
    public class CircularBuffer
    {
        public CircularBuffer(short capacity)
        {
            Capacity = capacity;
            Buffer = new byte[capacity];
        }

        /// <summary>
        /// buffer
        /// </summary>
        public byte[] Buffer { get; private set; }
        /// <summary>
        /// 容量
        /// </summary>
        public short Capacity { get; private set; }
        /// <summary>
        /// 头
        /// </summary>
        public short Head { get; private set; }
        /// <summary>
        /// 尾
        /// </summary>
        public short Tail { get; private set; }
        /// <summary>
        /// 可读大小
        /// </summary>
        public short ReadableSize { get { return (short)(Tail - Head); } }
        /// <summary>
        /// 可写大小
        /// </summary>
        public short WritableSize { get { return (short)(Capacity - Tail); } }

        /// <summary>
        /// 头部读取
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        public bool Read(ref byte[] buf, short offset, short len)
        {
            if (len > ReadableSize)
            {
                Logger.Instance.Error("No enough data to read!");
                return false;
            }

            System.Buffer.BlockCopy(Buffer, Head, buf, offset, len);
            Head += len;

            return true;
        }

        public bool MoveByRead(short len)
        {
            if (ReadableSize < len) return false;
            Head += len;
            return true;
        }

        /// <summary>
        /// 写入数据至缓存之后须移动Tail（尾部写入）
        /// </summary>
        /// <param name="len"></param>
        public bool MoveByWrite(short len)
        {
            if (len > WritableSize)
            {
                Logger.Instance.Error("No enough space to write!");
                return false;
            }

            Tail += len;
            return true;
        }

        /// <summary>
        /// buffer快满了？
        /// </summary>
        public bool Overload { get { return WritableSize < .2f * Capacity; } }

        public void Reset()
        {
            var readableSize = ReadableSize;
            if (readableSize != 0)
            {
                System.Buffer.BlockCopy(Buffer, Head, Buffer, 0, readableSize);
            }

            Head = 0;
            Tail = readableSize;
        }
    }
}
