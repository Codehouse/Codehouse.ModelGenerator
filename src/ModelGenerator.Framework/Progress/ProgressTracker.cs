using System;
using System.Threading.Tasks;
using Spectre.Console;

namespace ModelGenerator.Framework.Progress
{
    public class ProgressTracker : IProgressTracker
    {
        private class ContextContainer
        {
            public Task CompletionTask { get; }

            public ProgressContext Context
            {
                get
                {
                    if (_isComplete)
                    {
                        throw new InvalidOperationException("Context stopped.");
                    }

                    return _context;
                }
            }

            private readonly ProgressContext _context;

            private bool _isComplete;
            private readonly TaskCompletionSource _tcs;

            public ContextContainer(ProgressContext context)
            {
                _context = context;
                _tcs = new TaskCompletionSource();

                CompletionTask = _tcs.Task;
            }

            public void Finish()
            {
                if (!_isComplete)
                {
                    _isComplete = true;
                    _tcs.SetResult();
                }
            }
        }

        private ContextContainer _container = null!;

        public ProgressTracker()
        {
            var progress = AnsiConsole
                          .Progress()
                          .HideCompleted(false)
                          .Columns(
                               new TaskDescriptionColumn(),
                               new ProgressBarColumn(),
                               new PercentageColumn(),
                               new ElapsedTimeColumn());

            var containerCreationFlag = new TaskCompletionSource();
            progress.StartAsync(ctx =>
            {
                _container = new ContextContainer(ctx);
                containerCreationFlag.SetResult();

                return _container.CompletionTask;
            });

            containerCreationFlag.Task.Wait();
        }

        public Job CreateJob(string description)
        {
            return new Job(_container.Context, description);
        }

        public void Dispose()
        {
            Finish();
        }

        public void Finish()
        {
            _container.Finish();
        }
    }
}