using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class UsingSorter : IComparer<string>, IComparer<UsingDirectiveSyntax>, IEqualityComparer<UsingDirectiveSyntax>
    {
        private const string SystemNamespace = "System";

        /// <inheritdoc />
        public int Compare(string? x, string? y)
        {
            if (x is null || y is null)
            {
                return string.CompareOrdinal(x, y);
            }

            var xIsSystemNamespace = x == SystemNamespace || x.StartsWith(SystemNamespace + ".");
            var yIsSystemNamespace = y == SystemNamespace || y.StartsWith(SystemNamespace + ".");
            if (xIsSystemNamespace && !yIsSystemNamespace)
            {
                return -1;
            }

            if (yIsSystemNamespace && !xIsSystemNamespace)
            {
                return 1;
            }

            return string.CompareOrdinal(x, y);
        }

        /// <inheritdoc />
        public int Compare(UsingDirectiveSyntax? x, UsingDirectiveSyntax? y)
        {
            if (x is null && y is null)
            {
                return 0;
            }

            var xNamespace = x?.Name.ToString();
            var yNamespace = y?.Name.ToString();
            return Compare(xNamespace, yNamespace);
        }

        /// <inheritdoc />
        public bool Equals(UsingDirectiveSyntax? x, UsingDirectiveSyntax? y)
        {
            return Compare(x, y) == 0;
        }

        /// <inheritdoc />
        public int GetHashCode(UsingDirectiveSyntax obj)
        {
            return obj.Name.ToString().GetHashCode();
        }
    }
}