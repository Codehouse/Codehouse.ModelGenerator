using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.Activities
{
    public class DatabaseActivity : ActivityBase<ICollection<ItemSet>, IDatabase>
    {
        public override string Description => "Construct database";
        private readonly IDatabaseFactory _databaseFactory;

        public DatabaseActivity(IDatabaseFactory databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }

        protected override Task<IDatabase> ExecuteAsync(Job job, ICollection<ItemSet> input, CancellationToken cancellationToken)
        {
            return Task.Run(() => _databaseFactory.CreateDatabase(input), cancellationToken);
        }
    }
}