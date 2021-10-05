using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record ModelIdType : ModelType
    {
        public Template Template { get; init; }
    }
}