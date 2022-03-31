using System.Threading;
using System.Threading.Tasks;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.Activities
{
    public class TemplateActivity : ActivityBase<IDatabase, TemplateCollection>
    {
        public override string Description => "Create template structure";
        private readonly ITemplateCollectionFactory _templateFactory;

        public TemplateActivity(ITemplateCollectionFactory templateFactory)
        {
            _templateFactory = templateFactory;
        }

        protected override Task<IReport<TemplateCollection>> ExecuteAsync(Job job, IDatabase input, CancellationToken cancellationToken)
        {
            return Task.Run(() => Execute(input), cancellationToken);
        }

        private IReport<TemplateCollection> Execute(IDatabase input)
        {
            // TODO: Support RagReport for this activity.
            return new NullReport<TemplateCollection>(_templateFactory.ConstructTemplates(input));
        }
    }
}