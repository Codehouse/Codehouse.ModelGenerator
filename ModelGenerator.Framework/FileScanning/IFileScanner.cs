using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ModelGenerator.Framework.FileScanning
{
    public interface IFileScanner
    {
        IAsyncEnumerable<FileSet> FindFilesInPath(string root, string path);
    }
}