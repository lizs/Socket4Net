using System.Collections.Generic;
using Core.RPC;

namespace ChatS
{
    public static class ChaterMgr
    {
        private static readonly Dictionary<long, Chater> Chaters = new Dictionary<long, Chater>();

        public static Chater Create(RpcSession session)
        {
            var chater = new Chater(session);
            chater.Boot();
            Chaters.Add(session.Id, chater);

            return chater;
        }

        public static void Destroy(long id)
        {
            if (Chaters.ContainsKey(id))
            {
                var chater = Chaters[id];
                chater.Dispose();

                Chaters.Remove(id);
            }
        }
    }
}