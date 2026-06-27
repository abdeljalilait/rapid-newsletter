using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Interfaces;

namespace NewsletterPlatform.Infrastructure.Workers;

public sealed class CampaignDispatchWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CampaignDispatchWorker> _logger;
    private readonly CampaignDispatchWorkerOptions _options;

    public CampaignDispatchWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<CampaignDispatchWorker> logger,
        IOptions<CampaignDispatchWorkerOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Campaign dispatch worker is disabled");
            return;
        }

        _logger.LogInformation(
            "Campaign dispatch worker started with polling interval {PollingInterval} and batch size {BatchSize}",
            _options.PollingInterval,
            _options.BatchSize);

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<ICampaignDispatcher>();
                await dispatcher.DispatchPendingCampaignsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Campaign dispatch worker iteration failed");
            }

            try
            {
                await Task.Delay(_options.PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Campaign dispatch worker stopped");
    }
}
