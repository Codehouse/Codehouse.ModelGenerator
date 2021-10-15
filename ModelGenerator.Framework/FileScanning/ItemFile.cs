using System.Collections.Generic;

namespace ModelGenerator.Framework.FileScanning
{
    public record ItemFile(
        string Path,
        IDictionary<string, string> Properties);
}