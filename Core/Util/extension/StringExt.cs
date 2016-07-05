namespace socket4net
{
    public static class StringExt
    {
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
