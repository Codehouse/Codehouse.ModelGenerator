using System;

namespace ModelGenerator.Framework.FileParsing
{
    public interface ITemplateIds
    {
        public Guid Template { get; }
        public Guid TemplateField { get; }
        public Guid TemplateSection { get; }
    }
}