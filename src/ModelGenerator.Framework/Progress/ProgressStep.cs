using System.Threading;
using System.Threading.Tasks;

namespace ModelGenerator.Framework.Progress
{
    /// <summary>
    /// Class to wrap an activity with some progress tracking,
    /// allowing it to show as a progress bar in the progress
    /// tracker.
    /// </summary>
    /// <typeparam name="T">The type of the activity</typeparam>
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