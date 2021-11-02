using System.Threading;
using System.Threading.Tasks;

namespace ModelGenerator.Framework.Progress
{
    public interface IActivity
    {
        string Description { get; }
        Task ExecuteAsync(Job job, CancellationToken cancellationToken);
    }

    public interface IActivity<TInput, TOutput> : IActivity
    {
        TOutput GetOutput();
        void SetInput(TInput input);
    }
}