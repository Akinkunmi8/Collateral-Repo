using CollateralPortal.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CollateralPortal.Infrastructure.BackgroundServices;

public sealed class CollateralImportWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CollateralImportWorker> _logger;

    public CollateralImportWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<CollateralImportWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Collateral import worker started.");

        // Run once immediately when the API starts
        await RunImportAsync(stoppingToken);

        // Then run every 5 minutes
        using var timer =
            new PeriodicTimer(TimeSpan.FromMinutes(5));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunImportAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Collateral import worker stopped.");
        }
    }

    private async Task RunImportAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var ingestionService =
                scope.ServiceProvider
                    .GetRequiredService<ICollateralIngestionService>();

            var importedCount =
                await ingestionService.ImportAsync(
                    cancellationToken);

            _logger.LogInformation(
                "Collateral synchronization completed. " +
                "{Count} new collateral(s) imported.",
                importedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while importing collaterals.");
        }
    }
}