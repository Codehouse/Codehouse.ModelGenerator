using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class GenerationContext
    {
        public IDatabase Database { get; init; }
        public TemplateCollection Templates { get; init; }
        public TypeSet TypeSet { get; init; }
    }
}