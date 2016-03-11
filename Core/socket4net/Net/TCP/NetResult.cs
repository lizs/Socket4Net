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
    /// Rpc类型
    /// C->S or S->C
    /// </summary>
    public enum ERpc
    {
        /// <summary>
        /// 通知
        /// 无需等待对端响应
        /// </summary>
        Push,
        /// <summary>
        /// 请求
        /// 需要等待对端响应才能完成本次Rpc
        /// </summary>
        Request,
        /// <summary>
        /// 响应Request
        /// </summary>
        Response,
    }


    public class NetResult : Pair<bool, byte[]>
    {
        public NetResult(bool item1, byte[] item2)
            : base(item1, item2)
        {
        }

        public override string ToString()
        {
            return Key.ToString();
        }

        public static implicit operator bool(NetResult rt)
        {
            return rt.Key;
        }

        public static implicit operator NetResult(bool ret)
        {
            return ret ? Success : Failure;
        }

        public static NetResult Success
        {
            get { return new NetResult(true, null); }
        }

        public static NetResult Failure
        {
            get { return new NetResult(false, null); }
        }

        public static NetResult MakeSuccess(byte[] data)
        {
            return new NetResult(true, data);
        }

        public static NetResult MakeSuccess<T>(T proto)
        {
            return new NetResult(true, PiSerializer.Serialize(proto));
        }

        public static NetResult MakeFailure<T>(T proto)
        {
            return new NetResult(false, PiSerializer.Serialize(proto));
        }
    }

    public class NetResult<T> : NetResult
    {
        public NetResult(bool item1, byte[] item2) : base(item1, item2) { }
        public NetResult(bool item1, T proto) : base(item1, PiSerializer.Serialize(proto)) { }
    }
}
