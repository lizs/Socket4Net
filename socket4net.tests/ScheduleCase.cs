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

using System.Collections;
using System.Diagnostics;
using NUnit.Framework;

namespace socket4net.tests
{
    internal class ScheduleObj : Obj{}

    internal class ScheduleCase : Case
    {
        private Obj _scheduObj;
        [Test]
        public void TestCoroutine()
        {
            _scheduObj = Obj.New<ScheduleObj>(ObjArg.Empty);
            _scheduObj.StartCoroutine(MyCoroutine);
        }

        private IEnumerator MyCoroutine()
        {
            var watch = new Stopwatch();
            watch.Start();

            Logger.Ins.Info("Hello coroutine");
            yield return _scheduObj.WaitFor(2 * 1000);
            Logger.Ins.Info("{0}ms passed", watch.ElapsedMilliseconds);

            yield return _scheduObj.WaitFor(2 * 1000);
            Logger.Ins.Info("{0}ms passed", watch.ElapsedMilliseconds);

            yield return SubCoroutine();
            Logger.Ins.Info("{0}ms passed", watch.ElapsedMilliseconds);

            watch.Stop();
        }

        private IEnumerator SubCoroutine()
        {
            yield return _scheduObj.WaitFor(1 * 1000);
            yield return _scheduObj.WaitFor(1 * 1000);
            yield return _scheduObj.WaitFor(1 * 1000);
        }
    }
}