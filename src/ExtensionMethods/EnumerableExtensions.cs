using System;
using System.Collections.Generic;

namespace WeatherLink.ExtensionMethods {

    static class EnumerableExtensions {

        //from: https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/MinBy.cs
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
                                                                        Func<TSource, TKey> selector) {
            return source.MinBy(selector, null);
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
                                                                        Func<TSource, TKey> selector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            comparer = comparer ?? Comparer<TKey>.Default;
            using (var sourceIterator = source.GetEnumerator()) {
                if (!sourceIterator.MoveNext()) {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
                var min = sourceIterator.Current;
                var minKey = selector(min);
                while (sourceIterator.MoveNext()) {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0) {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }
    }
}