using System.Collections.Generic;

namespace ModelGenerator.Framework.FileScanning
{
    public interface ISourceProvider
    {
        IEnumerable<string> GetSources();
    }
}