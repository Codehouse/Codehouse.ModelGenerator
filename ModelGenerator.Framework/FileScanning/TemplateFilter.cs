using System;

namespace ModelGenerator.Framework.FileScanning
{
    public class TemplateFilter : IFilePathFilter
    {
        public bool Accept(string filePath)
        {
            return filePath != null && filePath.Contains("\\sitecore\\templates", StringComparison.OrdinalIgnoreCase);
        }
    }
}