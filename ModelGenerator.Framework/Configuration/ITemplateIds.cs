using System;

namespace ModelGenerator.Framework.FileParsing
{
    public interface ITemplateIds
    {
        Guid RenderingParameters { get; }
        public Guid Template { get; }
        public Guid TemplateField { get; }
        public Guid TemplateSection { get; }
    }
}