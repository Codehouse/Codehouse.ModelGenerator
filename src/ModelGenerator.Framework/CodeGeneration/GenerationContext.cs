using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public record GenerationContext(IDatabase Database, TemplateCollection Templates, TypeSet TypeSet);
}