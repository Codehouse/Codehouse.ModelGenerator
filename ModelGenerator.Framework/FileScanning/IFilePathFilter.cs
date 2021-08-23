namespace ModelGenerator.Framework.FileScanning
{
    public interface IFilePathFilter
    {
        public bool Accept(string filePath);
    }
}