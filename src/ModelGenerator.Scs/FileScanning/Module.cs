using System;

namespace ModelGenerator.Scs.FileParsing
{
    public class Module
    {
        public ModuleItems Items { get; set; } = new ();
        public string Namespace { get; set; } = string.Empty;

        public class ModuleItems
        {
            public ItemSource[] Includes { get; set; } = Array.Empty<ItemSource>();
        }
    }
}