namespace ModelGenerator.Framework.FileParsing
{
    public interface IItemFilter
    {
        public bool Accept(Item item);
    }
}