using System;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class GenerationException : FrameworkException
    {
        public GenerationException(string message) : base(message)
        {
        }

        public GenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}