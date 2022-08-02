using System;

namespace ModelGenerator
{
    public class ProviderNameException : Exception
    {
        public string? OriginalName { get; }

        protected ProviderNameException(string? originalName, string message) : base(message)
        {
            OriginalName = originalName;
        }

        public static ProviderNameException InvalidProvider(string? originalName, Type enumType)
        {
            var message = originalName is null
                ? $"A null value could not be parsed as one of the {enumType.Name}"
                : $"Provider name '{originalName}' could not be parsed as one of the {enumType.Name}.";

            return new ProviderNameException(originalName, message);
        }

        public static ProviderNameException UnsupportedProvider(string? originalName, Type enumType)
        {
            var message = originalName is null
                ? $"Null is not supported as one of the {enumType.Name}."
                : $"Provider name '{originalName}' is not supported as one of the {enumType.Name}.";

            return new ProviderNameException(originalName, message);
        }
    }
}