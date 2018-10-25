using System.Collections.Generic;

namespace Ordos.Core.Utilities
{
    public static class IEnumerableExtensions
    {
        public static void AddRange<T>(this ICollection<T> inputList, IEnumerable<T> range)
        {
            foreach (var item in range) inputList.Add(item);
        }
    }
}