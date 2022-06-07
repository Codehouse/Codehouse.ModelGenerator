using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class DefaultFileFactory : IFileFactory<DefaultFile>
    {
        public DefaultFile CreateFile(GenerationContext generationContext, ModelFile modelFile, ScopedRagBuilder<string> tracker)
        {
            return new DefaultFile(generationContext, modelFile, tracker);
        }

        IFileType IFileFactory.CreateFile(GenerationContext generationContext, ModelFile modelFile, ScopedRagBuilder<string> tracker)
        {
            return CreateFile(generationContext, modelFile, tracker);
        }
    }
}