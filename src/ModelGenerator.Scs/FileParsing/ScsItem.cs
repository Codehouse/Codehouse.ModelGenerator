using System;
using YamlDotNet.Serialization;

namespace ModelGenerator.Scs.FileParsing
{
    public class ScsItem
    {
        [YamlMember(Alias = "BranchID", ApplyNamingConventions = false)]
        public Guid BranchId { get; set; }
        [YamlMember(Alias = "ID", ApplyNamingConventions = false)]
        public Guid Id { get; set; }
        public ScsLanguage[] Languages { get; set; } = Array.Empty<ScsLanguage>();
        public string Name => _name ??= GetNameFromPath(Path);
        public Guid Parent { get; set; }
        public string Path { get; set; } = string.Empty;
        public ScsField[] SharedFields { get; set; } = Array.Empty<ScsField>();
        public Guid Template { get; set; }

        private string? _name;

        private string GetNameFromPath(string path)
        {
            var index = path.LastIndexOf('/');
            if (index == -1 || index == path.Length - 1)
            {
                throw new ArgumentException("Argument is invalid.", nameof(path));
            }

            return path.Substring(index + 1);
        }
    }
}