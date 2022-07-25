using ModelGenerator.Framework.Configuration;

namespace ModelGenerator.Framework.Progress
{
    /// <summary>
    /// Represents some kind of report produced by
    /// a <see cref="ProgressStep{T}"/> and written to
    /// by an <see cref="IActivity{TInput,TOutput}"/>.
    /// </summary>
    public interface IReport
    {
        /// <summary>
        /// Prints the report to the console
        /// </summary>
        /// <param name="verbosity">The minimum level of verbosity that should be output</param>
        void Print(Verbosities verbosity);
    }

    /// <summary>
    /// Represents some kind of report produced by
    /// a <see cref="ProgressStep{T}"/> and written to
    /// by an <see cref="IActivity{TInput,TOutput}"/>.
    /// <para>Also wraps a <typeparamref name="T"/>
    /// result object.</para>
    /// </summary>
    /// <typeparam name="T">The type of the result</typeparam>
    public interface IReport<T> : IReport
    {
        T Result { get; }
    }
}