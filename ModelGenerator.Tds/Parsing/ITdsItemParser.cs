using System.Collections.Generic;
using ModelGenerator.Framework.FileParsing;
using Superpower.Model;

namespace ModelGenerator.Tds.Parsing
{
    public interface ITdsItemParser
    {
        Item[] ParseTokens(TokenList<TdsItemTokens> tokenList);
    }
}