using System;

namespace ModelGenerator.Scs.FileParsing
{
    public class ScsLanguage
    {
        public ScsField[] Fields { get; set; } = Array.Empty<ScsField>();
        public string Language { get; set; } = string.Empty;
        public ScsVersion[] Versions { get; set; } = Array.Empty<ScsVersion>();
    }
}