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

namespace socket4net
{
    public abstract class Tsk : Obj
    {
        public event Action<int, int> EventProgressChanged;
        public event Action<string> EventFailed;
        public event Action EventCompleted;

        private bool _isDone;
        private bool _failed;
        int _progress;

        public int Progress
        {
            get { return _progress; }
            protected set
            {
                if (value == _progress) return;

                _progress = value;
                if (EventProgressChanged != null)
                    EventProgressChanged(Steps, Progress);
            }
        }

        public int Steps { get; protected set; }

        public bool IsDone
        {
            get { return _isDone; }
            protected set
            {
                if (value == _isDone) return;
                _isDone = value;
                if (_isDone && EventCompleted != null)
                    EventCompleted();
            }
        }

        public bool Failed
        {
            get { return _failed; }
            protected set
            {
                if (_failed == value) return;
                _failed = value;

                if (_failed && EventFailed != null)
                    EventFailed("");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 清理事件
            EventCompleted = null;
            EventFailed = null;
            EventProgressChanged = null;
        }
    }
}
