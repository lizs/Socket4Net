namespace socket4net
{
    /// <summary>
    ///     字符串扩展
    /// </summary>
    public static class StringExt
    {
        /// <summary>
        ///     字符串是否为空或仅包含空格
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string input)
        {
#if NET35
            return input.IsNullOrEmpty() || input.Trim(' ').IsNullOrEmpty();
#else
            return string.IsNullOrWhiteSpace(input);
#endif
        }
    }
}
