namespace ModelGenerator.Framework.Configuration
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // Record is initialised from configuration.
    public record Settings
    {
        public string? MinVersion { get; init; }
        public string ModelFolder { get; init; } = string.Empty;
        public string ModelNamespace { get; init; } = string.Empty;
        public Verbosities Verbosity { get; init; }
    }
}