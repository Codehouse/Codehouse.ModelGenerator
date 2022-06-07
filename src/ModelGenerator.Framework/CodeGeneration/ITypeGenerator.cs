using ModelGenerator.Framework.CodeGeneration.FileTypes;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface ITypeGenerator<TFile>
        where TFile : IFileType
    {
        string Tag { get; }
        NamespacedType? GenerateType(TFile file);
    }
}