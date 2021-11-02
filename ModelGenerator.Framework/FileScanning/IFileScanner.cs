using System.Threading.Tasks;

namespace ModelGenerator.Framework.FileScanning
{
    public interface IFileScanner
    {
        Task<FileSet?> ScanSourceAsync(string path);
    }
}