using System.Collections.Generic;
using System.Threading.Tasks;
using ModelGenerator.Framework.FileScanning;

namespace ModelGenerator.Framework.FileParsing
{
    public interface IFileParser
    {
        Task<Item[]> ParseFile(FileSet fileSet, ItemFile file);
    }
}