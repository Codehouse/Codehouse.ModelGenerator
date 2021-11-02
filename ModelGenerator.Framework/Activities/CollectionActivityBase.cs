using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.Activities
{
    public abstract class CollectionActivityBase<TInput, TOutput> : IActivity<IEnumerable<TInput>, ICollection<TOutput>>
    {
        public abstract string Description { get; }
        
        private TInput[] _inputs;
        private bool _isRunning;
        private ICollection<TOutput> _outputs;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        public async Task ExecuteAsync(Job job, CancellationToken cancellationToken)
        {
            _isRunning = true;
            job.MaxValue = _inputs.Length;
            var taskList = _inputs.Select(i => CreateItemTaskAsync(job, i, cancellationToken))
                                  .ToList();

            // TODO: Make DOP configurable.
            _semaphore.Release(1);
            _outputs = (await Task.WhenAll(taskList))
                .Where(r => r is not null)
                .ToArray()!;
            _isRunning = false;
        }

        public ICollection<TOutput> GetOutput()
        {
            if (_outputs == null)
            {
                throw new InvalidOperationException("Output is not set.");
            }

            if (_isRunning)
            {
                throw new InvalidOperationException("Activity is still running.");
            }

            return _outputs;
        }

        public virtual void SetInput(IEnumerable<TInput> input)
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("Cannot modify input while running.");
            }
            
            _inputs = input.ToArray();
        }

        protected virtual async Task<TOutput?> CreateItemTaskAsync(Job job, TInput input, CancellationToken cancellationToken)
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

        protected abstract Task<TOutput?> ExecuteItemAsync(Job job, TInput input);
    }
}