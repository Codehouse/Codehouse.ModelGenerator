using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.Activities
{
    public abstract class CollectionActivityBase<TInput, TOutput> : ActivityBase<IEnumerable<TInput>, ICollection<TOutput>>
    {
        private readonly SemaphoreSlim _semaphore = new(0);

        public override void SetInput(IEnumerable<TInput> input)
        {
            base.SetInput(input.ToArray());
        }

        protected override async Task<ICollection<TOutput>> ExecuteAsync(Job job, IEnumerable<TInput> inputs, CancellationToken cancellationToken)
        {
            job.MaxValue = inputs.Count() - 1;
            var taskList = inputs.Select(i => CreateItemTaskAsync(job, i, cancellationToken))
                                 .ToList();

            // TODO: Make DOP configurable.               
            _semaphore.Release(1);
            return (await Task.WhenAll(taskList))
                   .Where(r => r is not null)
                   .ToArray()!;
        }

        protected abstract Task<TOutput?> ExecuteItemAsync(Job job, TInput input);

        private async Task<TOutput?> CreateItemTaskAsync(Job job, TInput input, CancellationToken cancellationToken)
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