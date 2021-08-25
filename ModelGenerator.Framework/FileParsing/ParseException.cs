using System;

namespace ModelGenerator.Framework.FileParsing
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}