namespace ModelGenerator.Framework.ItemModelling
{
    public interface ITemplateCollectionFactory
    {
        TemplateCollection ConstructTemplates(IDatabase database);
    }
}