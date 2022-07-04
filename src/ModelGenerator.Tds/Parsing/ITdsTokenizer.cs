using Superpower;
using Superpower.Model;

namespace ModelGenerator.Tds.Parsing
{
    internal interface ITdsTokenizer
    {
        TextParser<TextSpan> PropertyName { get; }

        TokenList<TdsItemTokens> Tokenize(string input);
    }
}