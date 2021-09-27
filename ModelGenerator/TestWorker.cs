using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModelGenerator
{
    public class TestWorker : IHostedService
    {
        private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        private ILogger<TestWorker> Logger { get; }
        private TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>();

        public TestWorker(ILogger<TestWorker> logger)
        {
            Logger = logger;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Logger.LogInformation("Hello World");
                await Task.Delay(1000);
            }

            Logger.LogInformation("Stopping");
            TaskCompletionSource.SetResult(true);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Start our application code.
            Task.Run(() => DoWork(CancellationTokenSource.Token));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();
            // Defer completion promise, until our application has reported it is done.
            return TaskCompletionSource.Task;
        }
    }
}