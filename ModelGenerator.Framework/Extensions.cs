using System;

namespace ModelGenerator.Framework
{
    public static class Extensions
    {
        public static string ToSitecoreId(this Guid id)
        {
            return id.ToString("B").ToUpperInvariant();
        }
    }
}