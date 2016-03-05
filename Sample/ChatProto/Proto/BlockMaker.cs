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
using System;
using socket4net;

namespace Shared
{
    public static class BlockMaker
    {
        public static IBlock Create(short pid)
        {
            return Create((EPid) pid);
        }

        public static IBlock Create(EPid epid)
        {
            if (!Enum.IsDefined(typeof(EPid), epid))
                throw new ArgumentException("epid");

            var pid = (short) epid;
            switch (epid)
            {
                case EPid.One:
                    return new SettableBlock<int>(pid, 1, EBlockMode.Synchronizable);

                case EPid.Two:
                    return new IncreasableBlock<int>(pid, 2, EBlockMode.Synchronizable, 0, 10);

                case EPid.Three:
                    return new ListBlock<int>(pid, new []{1, 2, 3}, EBlockMode.Synchronizable);

                default:
                    throw new NotImplementedException(pid.ToString());
            }
        }
    }
}
