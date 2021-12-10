using System;

namespace ModelGenerator.Framework.FileParsing
{
    public class TokenisationException : Exception
    {
        public TokenisationException(string message) : base(message)
        {
        }
    }
}