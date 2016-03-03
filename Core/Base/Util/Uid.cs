using System;

namespace socket4net
{
    public static class Uid
    {
        public static long New()
        {
            var buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}
