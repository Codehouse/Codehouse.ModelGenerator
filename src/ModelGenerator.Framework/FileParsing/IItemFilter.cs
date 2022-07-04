using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.FileParsing
{
    public interface IItemFilter
    {
        public bool Accept(ScopedRagBuilder<string> tracker, Item item);
    }
}