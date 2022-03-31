using System.Threading;
using System.Threading.Tasks;

namespace ModelGenerator.Framework.Progress
{
    public class ProgressStep<T>
        where T : IActivity
    {
        public T Activity { get; }

        public ProgressStep(T activity)
        {
            Activity = activity;
        }

        public async Task ExecuteAsync(IProgressTracker tracker, CancellationToken cancellationToken)
        {
            using var job = tracker.CreateJob(Activity.Description);
            job.Start();
            await Activity.ExecuteAsync(job, cancellationToken);
            job.Stop();
        }
    }
}