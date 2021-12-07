using System;

namespace ModelGenerator.Framework.FileScanning
{
    public class FileScanException : FrameworkException
    {
        public FileScanException(string message) : base(message)
        {
        }

        public FileScanException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}