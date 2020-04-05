using System;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.Scrapers.Helpers
{
    public static class IEnumerableExtensions
    {
        public static int? NullableSum(this IEnumerable<int?> source)
            => source.Any(v => v.HasValue) ? source.Sum() : null;

        public static int? NullableSum<T>(this IEnumerable<T> source, Func<T, int?> selector)
            => source.Select(selector).NullableSum();

        public static double? NullableSum(this IEnumerable<double?> source)
            => source.Any(v => v.HasValue) ? source.Sum() : null;

        public static double? NullableSum<T>(this IEnumerable<T> source, Func<T, double?> selector)
            => source.Select(selector).NullableSum();
    }
}
