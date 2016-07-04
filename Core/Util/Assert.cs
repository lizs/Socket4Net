using System;
using System.IO;

namespace socket4net
{
    /// <summary>
    /// 
    /// </summary>
    public static class Assert
    {
        public static void NotNull<T>(this T input, string msg) where T : class
        {
            if (input == null)
                throw new ArgumentNullException(msg);
        }

        public static void IsTrue(this bool boolean, string msg)
        {
            if (!boolean)
                throw new InvalidDataException(msg);
        }

        public static void ThrowIf(this bool boolean, string msg)
        {
            if (boolean)
                throw new InvalidDataException(msg);
        }

        public static void ThrowIfNot(this bool boolean, string msg)
        {
            if (!boolean)
                throw new InvalidDataException(msg);
        }
    }
}
