using System;

namespace socket4net
{
    //<summary>
    //    组合了属性发布器的对象接口扩展
    //</summary>
    public static class PublisherExt
    {
        public static void Publish<TPKey>(this IPropertyPublishable<TPKey> source, IPropertiedObj<TPKey> host,
            IBlock<TPKey> block)
        {
            source.Publisher.Publish(host, block);
        }

        public static void GlobalListen<TPKey>(this IPropertyPublishable<TPKey> source,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)

        {
            source.Publisher.GlobalListen(handler);
        }

        public static void GlobalUnlisten<TPKey>(this IPropertyPublishable<TPKey> source,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)
        {
            source.Publisher.GlobalUnlisten(handler);
        }

        public static void GlobalListen<TPKey, T>(this IPropertyPublishable<TPKey> source,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler) where T : IPropertiedObj<TPKey>
        {
            source.Publisher.GlobalListen<T>(handler);
        }

        public static void GlobalUnlisten<TPKey, T>(this IPropertyPublishable<TPKey> source,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler) where T : IPropertiedObj<TPKey>
        {
            source.Publisher.GlobalUnlisten<T>(handler);
        }

        public static void GlobalListen<TPKey>(this IPropertyPublishable<TPKey> source, Type type,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)
        {
            source.Publisher.GlobalListen(type, handler);
        }

        public static void GlobalUnlisten<TPKey>(this IPropertyPublishable<TPKey> source, Type type,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)
        {
            source.Publisher.GlobalUnlisten(type, handler);
        }

        public static void Listen<TPKey, T>(this IPropertyPublishable<TPKey> source,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids) where T : IPropertiedObj<TPKey>
        {
            source.Publisher.Listen<T>(handler, pids);
        }

        public static void Unlisten<TPKey, T>(this IPropertyPublishable<TPKey> source,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids) where T : IPropertiedObj<TPKey>
        {
            source.Publisher.Unlisten<T>(handler, pids);
        }

        public static void Listen<TPKey>(this IPropertyPublishable<TPKey> source, Type type,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids)
        {
            source.Publisher.Listen(type, handler, pids);
        }

        public static void Unlisten<TPKey>(this IPropertyPublishable<TPKey> source, Type type,
            Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids)
        {
            source.Publisher.Unlisten(type, handler, pids);
        }
    }
}