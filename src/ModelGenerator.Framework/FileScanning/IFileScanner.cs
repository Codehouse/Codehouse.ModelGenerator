using System.Threading.Tasks;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.FileScanning
{
    public interface IFileScanner
    {
        Task<FileSet?> ScanSourceAsync(RagBuilder<string> ragBuilder, string path);
    }
}