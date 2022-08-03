using System;

namespace ModelGenerator.Framework.Progress
{
    /// <summary>
    /// Represents the overall progress tracker for
    /// the application, allowing for the creation of
    /// jobs, which will display as graphical progress
    /// indicators.
    /// </summary>
    public interface IProgressTracker : IDisposable
    {
        /// <summary>
        /// Creates a new job entry on the progress tracker
        /// with a given <paramref name="description"/>.
        /// </summary>
        /// <param name="description">The job description</param>
        /// <returns>An un-started job</returns>
        Job CreateJob(string description);

        /// <summary>
        /// Used to indicate that the overall application is
        /// finished and that the progress tracker should terminate
        /// (needed for UI reasons with Spectre Console).
        /// </summary>
        void Finish();
    }
}