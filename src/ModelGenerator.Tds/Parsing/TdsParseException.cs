using ModelGenerator.Framework.FileParsing;
using Superpower.Model;

namespace ModelGenerator.Tds.Parsing
{
    public class TdsParseException : ParseException
    {
        public TdsParseException(string message) : base(message)
        {
        }

        public TdsParseException(Position position, string message) : this(message + $" (at line {position.Line:N0}, col {position.Column:N0})")
        {
        }
    }
}