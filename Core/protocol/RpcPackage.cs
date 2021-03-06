﻿#region MIT
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
using System.Text;
using ProtoBuf;

namespace socket4net
{
    /// <summary>
    ///     rpc package
    /// </summary>
    [ProtoContract]
    public class RpcPackage
    {
        /// <summary>
        ///     rpc type
        ///     push/request/response
        /// </summary>
        [ProtoMember(1)]
        public ERpc Type { get; set; }

        /// <summary>
        ///     rpc serial number
        /// </summary>
        [ProtoMember(2)]
        public ushort Serial { get; set; }

        /// <summary>
        ///     rpc data
        /// </summary>
        [ProtoMember(3)]
        public byte[] Data { get; set; }

        /// <summary>
        ///     
        /// </summary>
        [ProtoMember(4)]
        public bool Success { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Type : " + Type);
            sb.AppendLine("Serial : " + Serial);
            sb.AppendFormat("Data length : {0}\r\n", Data.IsNullOrEmpty() ? 0 : Data.Length);

            return sb.ToString();
        }
    }
}
