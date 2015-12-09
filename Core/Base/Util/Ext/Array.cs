namespace socket4net
{
    public static class ArrayExt
    {
        public static bool IsNullOrEmpty<T>(this T[] input)
        {
            return input == null || input.Length == 0;
        }
    }
}