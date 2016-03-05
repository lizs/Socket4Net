#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
namespace socket4net
{
    /// <summary>
    /// 循环buffer
    /// </summary>
    public class CircularBuffer
    {
        public CircularBuffer(ushort capacity)
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
        public ushort Capacity { get; private set; }
        /// <summary>
        /// 头
        /// </summary>
        public ushort Head { get; private set; }
        /// <summary>
        /// 尾
        /// </summary>
        public ushort Tail { get; private set; }
        /// <summary>
        /// 可读大小
        /// </summary>
        public ushort ReadableSize { get { return (ushort)(Tail - Head); } }
        /// <summary>
        /// 可写大小
        /// </summary>
        public ushort WritableSize { get { return (ushort)(Capacity - Tail); } }

        /// <summary>
        /// 头部读取
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        public bool Read(ref byte[] buf, ushort offset, ushort len)
        {
            if (len > ReadableSize)
            {
                Logger.Ins.Error("No enough data to read!");
                return false;
            }

            System.Buffer.BlockCopy(Buffer, Head, buf, offset, len);
            Head += len;

            return true;
        }

        public bool MoveByRead(ushort len)
        {
            if (ReadableSize < len) return false;
            Head += len;
            return true;
        }

        /// <summary>
        /// 写入数据至缓存之后须移动Tail（尾部写入）
        /// </summary>
        /// <param name="len"></param>
        public bool MoveByWrite(ushort len)
        {
            if (len > WritableSize)
            {
                Logger.Ins.Error("No enough space to write!");
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
