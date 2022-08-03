using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public record GenerationContext(TemplateCollection Templates, TypeSet TypeSet);
}