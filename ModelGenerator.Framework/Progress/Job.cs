using System;
using Spectre.Console;

namespace ModelGenerator.Framework.Progress
{
    public class Job : IDisposable
    {
        public string Description
        {
            get => _task.Description;
            set => _task.Description = value;
        }

        public double MaxValue
        {
            get => _task.MaxValue;
            set => _task.MaxValue = value;
        }
        
        private readonly ProgressContext _containerContext;
        private readonly ProgressTask _task;

        public Job(ProgressContext containerContext, string description)
        {
            _containerContext = containerContext;
            _task = _containerContext.AddTask(description, false);
        }

        public void Increment()
        {
            Increment(1);
        }

        public void Increment(double value)
        {
            _task.Increment(value);
        }

        public void Start()
        {
            _task.StartTask();
        }

        public void Stop()
        {
            _task.Value = _task.MaxValue;
            _task.StopTask();
        }

        public void Dispose()
        {
            _task.StopTask();
        }
    }
}