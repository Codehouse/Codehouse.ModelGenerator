using ModelGenerator.Framework.Configuration;

namespace ModelGenerator.Framework.Progress
{
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