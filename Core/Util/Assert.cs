using System;
using System.IO;

namespace socket4net
{
    /// <summary>
    ///     断言
    /// </summary>
    public static class Assert
    {
        /// <summary>
        ///     保证目标非空，否则抛出异常
        /// </summary>
        /// <param name="input"></param>
        /// <param name="msg"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public static void NotNull<T>(this T input, string msg) where T : class
        {
            if (input == null)
                throw new ArgumentNullException(msg);
        }

        /// <summary>
        ///     保证目标为true，否则抛出异常
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="msg"></param>
        /// <exception cref="InvalidDataException"></exception>
        public static void IsTrue(this bool boolean, string msg)
        {
            if (!boolean)
                throw new InvalidDataException(msg);
        }

        /// <summary>
        ///     若目标表达式为true，则抛出异常
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="msg"></param>
        /// <exception cref="InvalidDataException"></exception>
        public static void ThrowIf(this bool boolean, string msg)
        {
            if (boolean)
                throw new InvalidDataException(msg);
        }

        /// <summary>
        ///     若目标表达式非true，则抛出异常
        /// </summary>
        /// <param name="boolean"></param>
        /// <param name="msg"></param>
        /// <exception cref="InvalidDataException"></exception>
        public static void ThrowIfNot(this bool boolean, string msg)
        {
            if (!boolean)
                throw new InvalidDataException(msg);
        }
    }
}
