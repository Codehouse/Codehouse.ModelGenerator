using Superpower.Model;

namespace ModelGenerator.Tds.Parsing
{
    internal interface ITdsTokenizer
    {
        TokenList<TdsItemTokens> Tokenize(string input);
    }
}