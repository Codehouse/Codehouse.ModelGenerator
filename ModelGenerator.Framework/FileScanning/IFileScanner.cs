using System.Collections.Generic;
using System.IO;

namespace ModelGenerator.Framework.FileScanning
{
    public interface IFileScanner
    {
        IEnumerable<FileSet> FindFilesInPath(string root, string path);
    }
}