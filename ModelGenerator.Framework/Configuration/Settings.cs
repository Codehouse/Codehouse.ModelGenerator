namespace ModelGenerator.Framework.Configuration
{
    public record Settings
    {
        public string ModelFolder { get; init; }
        public string ModelNamespace { get; init; }
        public string[] Patterns { get; init; }
        public string Root { get; init; }
        public Verbosities Verbosity { get; init; }
    }
}