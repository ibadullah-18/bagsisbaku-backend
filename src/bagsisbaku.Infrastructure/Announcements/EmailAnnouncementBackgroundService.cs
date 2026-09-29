using bagsisbaku.Application.Announcements;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace bagsisbaku.Infrastructure.Announcements;

internal sealed class EmailAnnouncementBackgroundService(
    IServiceScopeFactory scopeFactory,
    EmailAnnouncementSettings settings,
    ILogger<EmailAnnouncementBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!settings.ProcessingEnabled)
        {
            logger.LogInformation(
                "Email announcement background processing is disabled.");

            return;
        }

        var interval =
            TimeSpan.FromSeconds(
                settings.ProcessingIntervalSeconds);

        logger.LogInformation(
            "Email announcement background processing started. IntervalSeconds: {IntervalSeconds}; BatchSize: {BatchSize}; DemoMode: {DemoMode}",
            settings.ProcessingIntervalSeconds,
            settings.BatchSize,
            settings.DemoMode);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingAsync(
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken
                    .IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Email announcement background processing failed.");
            }

            try
            {
                await Task.Delay(
                    interval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken
                    .IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessPendingAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            scopeFactory.CreateAsyncScope();

        var processor =
            scope.ServiceProvider
                .GetRequiredService<
                    IEmailAnnouncementProcessor>();

        var processedCount =
            await processor.ProcessPendingAsync(
                settings.BatchSize,
                cancellationToken);

        if (processedCount > 0)
        {
            logger.LogInformation(
                "Email announcements processed. Count: {ProcessedCount}",
                processedCount);
        }
    }
}