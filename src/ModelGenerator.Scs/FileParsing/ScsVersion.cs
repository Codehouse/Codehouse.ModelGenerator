using System;

namespace ModelGenerator.Scs.FileParsing
{
    public class ScsVersion
    {
        public ScsField[] Fields { get; set; } = Array.Empty<ScsField>();
        public int Version { get; set; }
    }
}