using System.Collections.Generic;

namespace socket4net
{
    public static class ListExt
    {
        public static bool IsNullOrEmpty<T>(this List<T> input)
        {
            return input == null || input.Count == 0;
        }
    }
}
