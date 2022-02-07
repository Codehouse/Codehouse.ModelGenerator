using ModelGenerator.Framework.Configuration;

namespace ModelGenerator.Framework.Progress
{
    public interface IReport
    {
        void Print(Verbosities verbosity);
    }

    public interface IReport<T> : IReport
    {
        T Result { get; }
    }
}