
namespace socket4net
{
    /// <summary>
    /// 网络包协议
    /// 二进制流格式：Length(2 Byte) + Body(Length Byte) + ... + Length(2 Byte) + Body(Length Byte)
    /// 
    /// 注意：本层仅负责打包一个个完整的Body
    ///           上层为Body提供反序列化方式（protobuf）
    /// </summary>
    public class NetProto
    {
        public ushort Length { get; set; }
    }
}
