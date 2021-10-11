using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record ModelType
    {
        // TODO: Remove different subclasses for different Fortis type requirements 
        public string Name { get; init; }
        public Template Template { get; init; }
    }
}