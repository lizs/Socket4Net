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

namespace Stateless
{
    partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// Associates configured parameters with an underlying trigger value.
        /// </summary>
        public abstract class TriggerWithParameters
        {
            readonly TTrigger _underlyingTrigger;
            readonly Type[] _argumentTypes;

            /// <summary>
            /// Create a configured trigger.
            /// </summary>
            /// <param name="underlyingTrigger">Trigger represented by this trigger configuration.</param>
            /// <param name="argumentTypes">The argument types expected by the trigger.</param>
            public TriggerWithParameters(TTrigger underlyingTrigger, params Type[] argumentTypes)
            {
                Enforce.ArgumentNotNull(argumentTypes, "argumentTypes");

                _underlyingTrigger = underlyingTrigger;
                _argumentTypes = argumentTypes;
            }

            /// <summary>
            /// Gets the underlying trigger value that has been configured.
            /// </summary>
            public TTrigger Trigger { get { return _underlyingTrigger; } }

            /// <summary>
            /// Ensure that the supplied arguments are compatible with those configured for this
            /// trigger.
            /// </summary>
            /// <param name="args"></param>
            public void ValidateParameters(object[] args)
            {
                Enforce.ArgumentNotNull(args, "args");

                ParameterConversion.Validate(args, _argumentTypes);
            }
        }

        /// <summary>
        /// A configured trigger with one required argument.
        /// </summary>
        /// <typeparam name="TArg0">The type of the first argument.</typeparam>
        public class TriggerWithParameters<TArg0> : TriggerWithParameters
        {
            /// <summary>
            /// Create a configured trigger.
            /// </summary>
            /// <param name="underlyingTrigger">Trigger represented by this trigger configuration.</param>
            public TriggerWithParameters(TTrigger underlyingTrigger)
                : base(underlyingTrigger, typeof(TArg0))
            {
            }
        }

        /// <summary>
        /// A configured trigger with two required arguments.
        /// </summary>
        /// <typeparam name="TArg0">The type of the first argument.</typeparam>
        /// <typeparam name="TArg1">The type of the second argument.</typeparam>
        public class TriggerWithParameters<TArg0, TArg1> : TriggerWithParameters
        {
            /// <summary>
            /// Create a configured trigger.
            /// </summary>
            /// <param name="underlyingTrigger">Trigger represented by this trigger configuration.</param>
            public TriggerWithParameters(TTrigger underlyingTrigger)
                : base(underlyingTrigger, typeof(TArg0), typeof(TArg1))
            {
            }
        }

        /// <summary>
        /// A configured trigger with three required arguments.
        /// </summary>
        /// <typeparam name="TArg0">The type of the first argument.</typeparam>
        /// <typeparam name="TArg1">The type of the second argument.</typeparam>
        /// <typeparam name="TArg2">The type of the third argument.</typeparam>
        public class TriggerWithParameters<TArg0, TArg1, TArg2> : TriggerWithParameters
        {
            /// <summary>
            /// Create a configured trigger.
            /// </summary>
            /// <param name="underlyingTrigger">Trigger represented by this trigger configuration.</param>
            public TriggerWithParameters(TTrigger underlyingTrigger)
                : base(underlyingTrigger, typeof(TArg0), typeof(TArg1), typeof(TArg2))
            {
            }
        }
    }
}
