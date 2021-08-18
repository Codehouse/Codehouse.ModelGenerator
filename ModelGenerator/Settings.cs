namespace ModelGenerator
{
    public record Settings
    {
        public string[] Patterns { get; init; }
        public string Root { get; init; }
    }
}