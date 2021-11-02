namespace ModelGenerator.Framework.Progress
{
    public interface IProgressTracker
    {
        Job CreateJob(string description);
    }
}