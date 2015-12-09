using System;
using System.IO;
using System.Reflection;
using ProtoBuf;

namespace socket4net
{
    public interface IProtobufInstance
    {
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
        public static byte[] Serialize<T>(T proto) where T : IProtobufInstance
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
                Logger.Instance.ErrorFormat("{0}:{1}", e.Message, e.StackTrace);
                return null;
            }
        }

        /// <summary>
        ///     反序列化为一个pb实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data) where T : IProtobufInstance
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
                Logger.Instance.ErrorFormat("{0}:{1}", e.Message, e.StackTrace);
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
                Logger.Instance.ErrorFormat("{0}:{1}", e.Message, e.StackTrace);
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
                Logger.Instance.ErrorFormat("{0}:{1}", e.Message, e.StackTrace);
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
                Logger.Instance.ErrorFormat("{0}:{1}", e.Message, e.StackTrace);
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
                Logger.Instance.ErrorFormat("{0}:{1}", e.Message, e.StackTrace);
                return null;
            }
        }

        /// <summary>
        ///     非泛型方式的反序列化
        ///     仅用于pb实例
        /// </summary>
        public static object Deserialize(Type type, byte[] data)
        {
            if (type.IsSubclassOf(typeof(IProtobufInstance)))
            {
                using (var ms = new MemoryStream(data))
                {
                    return Serializer.NonGeneric.Deserialize(type, ms);
                }
            }

            Logger.Instance.Error("Unkonwn type");
            return null;
        }

        public static byte[] Serialize(object obj)
        {
            var ins = obj as IProtobufInstance;
            if (ins != null)
                return Serialize(ins);

            Logger.Instance.Error("Unkonwn type");
            return null;
        }
    }
}
