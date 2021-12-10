using ModelGenerator.Framework.FileParsing;
using Superpower.Model;

namespace ModelGenerator.Tds.Parsing
{
    public interface ITdsItemParser
    {
        TdsItem[] ParseTokens(TokenList<TdsItemTokens> tokenList);
    }
}