using System;
using System.Collections.Generic;

namespace AutoMapper.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first,
            IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            var set = new HashSet<TKey>(second, comparer);

            foreach (var element in first)
                if (set.Add(keySelector(element)))
                    yield return element;
        }
    }
}