using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.Activities
{
    public abstract class CollectionActivityBase<TInput, TOutput> : CollectionActivityBase<TInput, TOutput, TOutput>
        where TOutput : class
    {
        protected override ICollection<TOutput> ConvertResults(TOutput?[] results)
        {
            return results.WhereNotNull().ToArray();
        }
    }
    
    public abstract class CollectionActivityBase<TInput, TOutput, TItemOutput> : ActivityBase<IEnumerable<TInput>, ICollection<TOutput>>
        where TOutput : class
    {
        private readonly SemaphoreSlim _semaphore = new(0);

        public override void SetInput(IEnumerable<TInput> input)
        {
            base.SetInput(input.ToArray());
        }

        protected override async Task<IReport<ICollection<TOutput>>> ExecuteAsync(Job job, IEnumerable<TInput> inputs, CancellationToken cancellationToken)
        {
            job.MaxValue = inputs.Count() - 1;
            var taskList = inputs.Select(i => CreateItemTaskAsync(job, i, cancellationToken))
                                 .ToList();

            // TODO: Make DOP configurable.               
            _semaphore.Release(1);
            var results = await Task.WhenAll(taskList);
            return CreateReport(ConvertResults(results));
        }

        protected abstract IReport<ICollection<TOutput>> CreateReport(ICollection<TOutput> results);
        protected abstract ICollection<TOutput> ConvertResults(TItemOutput?[] results);
        protected abstract Task<TItemOutput?> ExecuteItemAsync(Job job, TInput input);

        private async Task<TItemOutput?> CreateItemTaskAsync(Job job, TInput input, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                return await ExecuteItemAsync(job, input);
            }
            finally
            {
                job.Increment();
                _semaphore.Release();
            }
        }
    }
}