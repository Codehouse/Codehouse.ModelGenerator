namespace ModelGenerator
{
    public record Settings
    {
        public string ModelFolder { get; init; }
        public string[] Patterns { get; init; }
        public string Root { get; init; }
    }
}