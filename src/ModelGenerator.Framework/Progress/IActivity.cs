using System.Threading;
using System.Threading.Tasks;

namespace ModelGenerator.Framework.Progress
{
    /// <summary>
    /// Represents an activity within the overall application pipeline
    /// that must be performed.
    /// </summary>
    public interface IActivity
    {
        string Description { get; }

        Task ExecuteAsync(Job job, CancellationToken cancellationToken);
    }

    /// <inheritdoc cref="IActivity"/>
    /// <typeparam name="TInput">The input type</typeparam>
    /// <typeparam name="TOutput">The output type</typeparam>
    public interface IActivity<TInput, TOutput> : IActivity
    {
        IReport<TOutput> GetOutput();

        void SetInput(TInput input);
    }
}