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
using System.IO;
using System.Reflection;
using ProtoBuf;

namespace socket4net
{
    /// <summary>
    ///     bytes反序列化成Value
    /// </summary>
    public static class Value
    {
        public static T As<T>(this byte[] bytes)
        {
            return PiSerializer.DeserializeValue<T>(bytes);
        }
    }

    /// <summary>
    ///     包装一个值以用在序列化
    ///     注意：Value并不是IProtobufInstance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ProtoContract]
    public class ValueProto<T>
    {
        [ProtoMember(1)]
        public T Result { get; set; }

        public ValueProto(T value)
        {
            Result = value;
        }

        /// <summary>
        ///     for pb
        /// </summary>
        public ValueProto()
        {
        }
    }

    /// <summary>
    ///     基于protobuf的序列化器
    ///     注：尽可能使用泛型接口
    /// </summary>
    public static class PiSerializer
    {
        /// <summary>
        ///     序列化一个protobuf实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T proto)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, proto);
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Exception(e);
                return null;
            }
        }

        /// <summary>
        ///     反序列化为一个pb实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    return Serializer.Deserialize<T>(ms);
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Exception(e);
                return default(T);
            }
        }

        /// <summary>
        ///     序列化一个值
        ///     比如：int, float, long ...
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static byte[] SerializeValue<T>(T value)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, new ValueProto<T>(value));
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Exception(e);
                return null;
            }
        }

        public static byte[] SerializeValue(object value)
        {
            if (Equals(value, null)) return null;

            try
            {
                using (var ms = new MemoryStream())
                {
                    var genericType = typeof(ValueProto<>).MakeGenericType(value.GetType());
                    var constructor = genericType.GetConstructor(new[] { value.GetType() });
                    Serializer.NonGeneric.Serialize(ms, constructor.Invoke(new[] { value }));
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Exception(e);
                return null;
            }
        }

        /// <summary>
        ///     反序列化为值
        /// </summary>
        /// <typeparam name="T">List<string> ...</typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T DeserializeValue<T>(byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    return Serializer.Deserialize<ValueProto<T>>(ms).Result;
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Exception(e);
                return default(T);
            }
        }

        public static object DeserializeValue(Type type, byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    var genericType = typeof(ValueProto<>).MakeGenericType(type);
                    var value = Serializer.NonGeneric.Deserialize(genericType, ms);

                    return genericType.GetProperty("Result").GetValue(value, BindingFlags.GetProperty, null, null, null);
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Exception(e);
                return null;
            }
        }

        /// <summary>
        ///     非泛型方式的反序列化
        ///     仅用于pb实例
        /// </summary>
        public static object Deserialize(Type type, byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    return Serializer.NonGeneric.Deserialize(type, ms);
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Exception(e);
                return null;
            }
        }

        //public static byte[] Serialize(object obj)
        //{
        //    try
        //    {
        //        using (var ms = new MemoryStream(data))
        //        {
        //            return Serializer.NonGeneric.Deserialize(type, ms);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Ins.Exception(e);
        //    }

        //    return Serialize(ins);
        //}
    }
}
