using System.Collections.Generic;

namespace ModelGenerator.Framework.FileParsing
{
    public interface IFileParser
    {
        IAsyncEnumerable<Item> ParseFile(string filePath);
    }
}