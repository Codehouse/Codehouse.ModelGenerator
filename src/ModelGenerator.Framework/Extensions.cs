using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ModelGenerator.Framework
{
    public static class Extensions
    {
        public static IServiceCollection AddConfiguration<T>(this IServiceCollection collection, IConfiguration configuration, string name)
            where T : class
        {
            return collection
                  .Configure<T>(opts => configuration.GetSection(name).Bind(opts))
                  .AddSingleton(sp => sp.GetRequiredService<IOptions<T>>().Value);
        }

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