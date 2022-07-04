using System;
using System.IO;

namespace ModelGenerator.Scs
{
    public class ScsSettings
    {
        public string BaseNamespace { get; set; } = string.Empty;
        public string ItemFolder { get; set; } = string.Empty;
        public string ModelFolder { get; set; } = string.Empty;

        // TODO: Reduce duplication with TDS settings
        public string Root
        {
            get => _root ?? Directory.GetCurrentDirectory();
            set => _root = string.IsNullOrEmpty(value) ? null : value;
        }

        public string[] Sources { get; set; } = Array.Empty<string>();

        private string? _root;
    }
}