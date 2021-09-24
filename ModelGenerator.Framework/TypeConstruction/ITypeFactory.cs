using System.Collections.Immutable;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.TypeConstruction
{
    public interface ITypeFactory
    {
        IImmutableList<TypeSet> CreateTypeSets(TemplateCollection collection);
    }
}