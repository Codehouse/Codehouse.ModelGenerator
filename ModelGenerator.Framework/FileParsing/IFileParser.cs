using System.Collections.Generic;
using ModelGenerator.Framework.FileScanning;

namespace ModelGenerator.Framework.FileParsing
{
    public interface IFileParser
    {
        IAsyncEnumerable<Item> ParseFile(FileSet fileSet, ItemFile file);
    }
}