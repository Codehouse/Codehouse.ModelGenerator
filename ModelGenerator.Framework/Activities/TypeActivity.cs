using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.Activities
{
    public class TypeActivity : ActivityBase<TemplateCollection, IImmutableList<TypeSet>>
    {
        public override string Description => "Create types";
        private readonly ITypeFactory _typeFactory;

        public TypeActivity(ITypeFactory typeFactory)
        {
            _typeFactory = typeFactory;
        }

        protected override Task<IReport<IImmutableList<TypeSet>>> ExecuteAsync(Job job, TemplateCollection input, CancellationToken cancellationToken)
        {
            return Task.Run(() => Execute(input), cancellationToken);
        }

        private IReport<IImmutableList<TypeSet>> Execute(TemplateCollection input)
        {
            return new NullReport<IImmutableList<TypeSet>>(_typeFactory.CreateTypeSets(input));
        }
    }
}