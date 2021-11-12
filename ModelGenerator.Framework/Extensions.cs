using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModelGenerator.Framework
{
    public static class Extensions
    {
        public static string ToSitecoreId(this Guid id)
        {
            return id.ToString("B").ToUpperInvariant();
        }

        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> collection)
        {
            return Task.WhenAll(collection);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> o)
            where T : class
        {
            return o.Where(x => x != null)!;
        }
    }
}