using System.Threading;
using System.Threading.Tasks;

namespace ModelGenerator.Framework.Progress
{
    public class ProgressStep<T>
        where T : IActivity
    {
        public T Activity { get; }
        private readonly IProgressTracker _tracker;

        public ProgressStep(T activity, IProgressTracker tracker)
        {
            Activity = activity;
            _tracker = tracker;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var job = _tracker.CreateJob(Activity.Description);
            job.Start();
            await Activity.ExecuteAsync(job, cancellationToken);
            job.Stop();
        }
    }
}