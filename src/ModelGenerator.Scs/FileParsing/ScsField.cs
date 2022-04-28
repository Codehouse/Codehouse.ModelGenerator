using System;
using YamlDotNet.Serialization;

namespace ModelGenerator.Scs.FileParsing
{
    public class ScsField
    {
        [YamlMember(Alias = "ID", ApplyNamingConventions = false)]
        public Guid Id { get; set; }
        [YamlMember(Alias = "Hint", ApplyNamingConventions = false)]
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}