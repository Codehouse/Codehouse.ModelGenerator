using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModelGenerator.Framework.FileParsing
{
    public interface IFileParser
    {
        IAsyncEnumerable<Item> ParseFile(string filePath);
    }
}