using System;
using System.Threading;
using System.Threading.Tasks;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.Activities
{
    public abstract class ActivityBase<TInput, TOutput> : IActivity<TInput, TOutput>
    {
        public abstract string Description { get; }

        private TInput? _input;
        private bool _isRunning;
        private TOutput? _output;

        public async Task ExecuteAsync(Job job, CancellationToken cancellationToken)
        {
            if (_input == null)
            {
                throw new InvalidOperationException("Input not set.");
            }

            try
            {
                _isRunning = true;
                _output = await ExecuteAsync(job, _input, cancellationToken);
            }
            finally
            {
                _isRunning = false;
            }
        }

        public virtual TOutput GetOutput()
        {
            if (_output == null)
            {
                throw new InvalidOperationException("Output is not set.");
            }

            if (_isRunning)
            {
                throw new InvalidOperationException("Activity is still running.");
            }

            return _output;
        }

        public virtual void SetInput(TInput input)
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("Cannot modify input while running.");
            }

            _input = input;
        }

        protected abstract Task<TOutput> ExecuteAsync(Job job, TInput input, CancellationToken cancellationToken);
    }
}