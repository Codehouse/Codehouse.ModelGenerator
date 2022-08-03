using ModelGenerator.Framework.Configuration;

namespace ModelGenerator.Framework.Progress
{
    /// <summary>
    /// A report that has no information associated with it
    /// and will never output anything, but which contains a
    /// result nevertheless.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public class NullReport<T> : IReport<T>
    {
        public T Result { get; }

        public NullReport(T result)
        {
            Result = result;
        }

        public void Print(Verbosities verbosity)
        {
            // This report outputs nothing.
        }
    }
}