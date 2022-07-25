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
        /// <summary>
        /// Adds a configuration type, <typeparamref name="T"/>, which can then be resolved as
        /// either an <see cref="IOptions{TOptions}"/> or directly as <typeparamref name="T"/>.
        /// </summary>
        /// <param name="collection">The service collection</param>
        /// <param name="configuration">The application configuration object</param>
        /// <param name="name">The name of the configuration section (use colons for nested sections)</param>
        /// <typeparam name="T">The type to which the configuration settings should be bound</typeparam>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddConfiguration<T>(this IServiceCollection collection, IConfiguration configuration, string name)
            where T : class
        {
            return collection
                  .Configure<T>(opts => configuration.GetSection(name).Bind(opts))
                  .AddSingleton(sp => sp.GetRequiredService<IOptions<T>>().Value);
        }

        /// <summary>
        /// Formats a <see cref="Guid"/> to the sort that
        /// is expected by Sitecore (upper-case with braces).
        /// </summary>
        /// <param name="id">A guid</param>
        /// <returns>A Sitecore-friendly guid string</returns>
        public static string ToSitecoreId(this Guid id)
        {
            return id.ToString("B").ToUpperInvariant();
        }

        /// <summary>
        /// Filters the <c>null</c> values out from an <c>IEnumerable</c>,
        /// while also explicitly making the output not-null from the compiler
        /// perspective.
        /// </summary>
        /// <param name="o">The input collection</param>
        /// <typeparam name="T">The type contained in the collection</typeparam>
        /// <returns>A filtered collection</returns>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> o)
            where T : class
        {
            return o.Where(x => x != null)!;
        }
    }
}