using System.IO;
using ModelGenerator.Framework.CodeGeneration.FileTypes;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface IFileGenerator
    {
        public bool CanGenerate(IFileType file);
        public FileInfo? GenerateFile(IFileType file);
    }
    public interface IFileGenerator<TFile> : IFileGenerator
      where TFile : IFileType
    {
        public FileInfo? GenerateFile(TFile file);
    }
}