using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModelGenerator.Framework.FileParsing
{
    public interface IFileParser
    {
        Task<IEnumerable<Item>> ParseFile(string filePath);
    }
}