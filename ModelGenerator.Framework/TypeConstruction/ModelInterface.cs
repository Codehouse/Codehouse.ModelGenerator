using System.Collections.Immutable;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record ModelInterface : ModelType
    {
        public IImmutableList<TemplateField> Fields { get; init; }
        public Template Template { get; init; }
    }
}