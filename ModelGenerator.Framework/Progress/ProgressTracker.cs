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
                _isComplete = true;
                _tcs.SetResult();
            }
        }

        private ContextContainer _container;

        public ProgressTracker()
        {
            var progress = AnsiConsole.Progress();
            progress.HideCompleted = false;
            progress.StartAsync(ctx =>
            {
                _container = new ContextContainer(ctx);

                return _container.CompletionTask;
            });
        }

        public Job CreateJob(string description)
        {
            return new Job(_container.Context, description);
        }
    }
}