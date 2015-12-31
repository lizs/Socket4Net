using System;

namespace socket4net
{
    public static class Convertor
    {
        /// <summary>
        ///     转成数字或枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T To<T>(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return default(T);

            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), input);

            return (T)Convert.ChangeType(input, typeof(T));
        }
    }
}
