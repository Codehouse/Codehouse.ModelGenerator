using System.Collections.Generic;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record ModelIdType : ModelType
    {
        public List<Template> Templates { get; } = new List<Template>();
    }
}