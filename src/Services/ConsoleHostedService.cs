using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAppNet5.Services
{
    internal sealed class ConsoleHostedService : IHostedService
    {
        private int? _exitCode;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IDemoService _demoService;

        public ConsoleHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<ConsoleHostedService> logger,
            IHostApplicationLifetime appLifetime,
            IDemoService demoService
            )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _appLifetime = appLifetime;
            _demoService = demoService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting with arguments: {string.Join(" ", Environment.GetCommandLineArgs())}");

            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        _logger.LogInformation("Starting the process...");

                        // You can still use services with a different life scope (transient, scoped)
                        // using (var scope = _scopeFactory.CreateScope())
                        // {
                        //    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                        // }

                        bool result = await _demoService.DoWork();

                        _logger.LogInformation($"Result is: {result}");

                        _exitCode = 0;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception");
                        _exitCode = 1;
                    }
                    finally
                    {
                        _logger.LogInformation("Sync completed.");
                        _appLifetime.StopApplication();
                    }
                });
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Exiting with return code: {_exitCode}");

            // Exit code may be null if the user cancelled via Ctrl+C/SIGTERM
            Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
            return Task.CompletedTask;
        }
    }
}
