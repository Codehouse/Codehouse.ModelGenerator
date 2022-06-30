using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration.FileTypes
{
    public interface IFileType
    {
        GenerationContext Context { get; }
        ModelFile Model { get; }
        ScopedRagBuilder<string> ScopedRagBuilder { get; }
    }
}