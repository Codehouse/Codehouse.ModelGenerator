namespace ModelGenerator.Framework.Configuration
{
    public record Settings
    {
        public string ModelFolder { get; init; }
        public string ModelNamespace { get; init; }
        public Verbosities Verbosity { get; init; }
    }
}