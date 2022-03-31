using System;

namespace ModelGenerator.Framework.Progress
{
    public interface IProgressTracker : IDisposable
    {
        Job CreateJob(string description);
        void Finish();
    }
}