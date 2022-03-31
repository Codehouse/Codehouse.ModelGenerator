using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModelGenerator
{
    public class Worker : IHostedService
    {
        private readonly CancellationTokenSource _cts = new ();
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<Worker> _logger;
        private readonly Runner _runner;
        private readonly TaskCompletionSource _tcs = new ();

        public Worker(IHostApplicationLifetime lifetime, ILogger<Worker> logger, Runner runner)
        {
            _lifetime = lifetime;
            _logger = logger;
            _runner = runner;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting...");
            Task.Run(() => ExecuteAsync(_cts.Token).ContinueWith(t => _lifetime.StopApplication()));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (!_tcs.Task.IsCanceled)
            {
                _logger.LogInformation("Sending cancellation request...");
                _cts.Cancel();
            }

            return _tcs.Task;
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Executing");
                await _runner.RunAsync(stoppingToken);
                _tcs.SetResult();
            }
            catch (OperationCanceledException)
            {
                _tcs.SetCanceled(stoppingToken);
            }
            catch (Exception ex)
            {
                _tcs.SetException(ex);
            }
        }
    }
}