using System.Threading.Tasks;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.FileParsing
{
    public interface IFileParser
    {
        Task<Item[]> ParseFile(ScopedRagBuilder<string> scopedRagBuilder, FileSet fileSet, ItemFile file);
    }
}