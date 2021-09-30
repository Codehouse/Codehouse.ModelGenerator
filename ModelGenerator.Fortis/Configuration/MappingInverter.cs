using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ModelGenerator.Fortis.Configuration
{
    public static class MappingInverter
    {
        public static IImmutableDictionary<U, T> InvertMapping<T, U>(Dictionary<T, U[]> inputMapping, IEqualityComparer<U> comparer)
        {
            return inputMapping.SelectMany(
                                   kvp => kvp.Value,
                                   (kvp, x) => KeyValuePair.Create(x, kvp.Key))
                               .ToImmutableDictionary(comparer);
        }
    }
}   