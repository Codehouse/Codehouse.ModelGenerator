using System;
using System.IO;

namespace ModelGenerator.Tds
{
    public record TdsSettings
    {
        public string Root
        {
            get => _root ?? Directory.GetCurrentDirectory();
            init => _root = string.IsNullOrEmpty(value) ? null : value;
        }
        
        public string[] Sources { get; init; } = Array.Empty<string>();

        private readonly string? _root;
    }
}