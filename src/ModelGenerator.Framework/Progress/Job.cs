using System;
using Spectre.Console;

namespace ModelGenerator.Framework.Progress
{
    /// <summary>
    /// A wrapper of the <see cref="ProgressTask"/> from
    /// Spectre Console.
    /// <para>Represents a job on the progress tracker UI.</para>
    /// </summary>
    public class Job : IDisposable
    {
        /// <summary>
        /// Gets or sets the job description
        /// </summary>
        public string Description
        {
            get => _task.Description;
            set => _task.Description = value;
        }

        /// <summary>
        /// Gets or sets the job's maximum value (used
        /// for the progress bar).
        /// </summary>
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

        public void Dispose()
        {
            _task.StopTask();
        }

        /// <summary>
        /// Increments the job value by one (affecting
        /// the progress bar).
        /// </summary>
        public void Increment()
        {
            Increment(1);
        }

        /// <summary>
        /// Increments the job value by <paramref name="value"/>
        /// (affecting the progress bar).
        /// </summary>
        /// <param name="value">The number by which the job value should be incremented.</param>
        public void Increment(double value)
        {
            _task.Increment(value);
        }

        /// <summary>
        /// Updates the job status on the UI to started.
        /// </summary>
        public void Start()
        {
            _task.StartTask();
        }

        /// <summary>
        /// Updates the job status on the UI to completed.
        /// </summary>
        public void Stop()
        {
            _task.Value = _task.MaxValue;
            _task.StopTask();
        }
    }
}