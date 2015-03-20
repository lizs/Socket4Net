using System;
using System.IO;

namespace Core.Serialize
{
    public class Serializer
    {
        public static T Deserialize<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return ProtoBuf.Serializer.Deserialize<T>(ms);
            }
        }

        public static byte[] Serialize<T>(T proto)
        {
            if (Equals(proto, null)) return null;

            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, proto);
                return ms.ToArray();
            }
        }

//         public static byte[] Serialize(object obj)
//         {
//             if (obj == null) return null;
// 
//             using (var ms = new MemoryStream())
//             {
//                 ProtoBuf.Serializer.NonGeneric.Serialize(ms, obj);
//                 return ms.ToArray();
//             }
//         }
// 
//         public static object Deserialize(Type type, byte[] bytes)
//         {
//             using (var ms = new MemoryStream(bytes))
//             {
//                 return ProtoBuf.Serializer.NonGeneric.Deserialize(type, ms);
//             }
//         }
    }
}
