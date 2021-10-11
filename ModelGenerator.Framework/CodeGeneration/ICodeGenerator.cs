using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface ICodeGenerator
    {
        void GenerateFile(GenerationContext context, string modelFolder, ModelFile modelFile);
    }
}