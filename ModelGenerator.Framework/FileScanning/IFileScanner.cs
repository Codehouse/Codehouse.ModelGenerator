using System.Collections.Generic;

namespace ModelGenerator.Framework.FileScanning
{
    public interface IFileScanner
    {
        IAsyncEnumerable<FileSet> FindFilesInPath(string root, string path);
    }
}