using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface IFileFactory
    {
        IFileType CreateFile(GenerationContext generationContext, ModelFile modelFile, ScopedRagBuilder<string> tracker);
    }

    public interface IFileFactory<TFile> : IFileFactory
        where TFile : IFileType
    {
        new TFile CreateFile(GenerationContext generationContext, ModelFile modelFile, ScopedRagBuilder<string> tracker);
    }
}