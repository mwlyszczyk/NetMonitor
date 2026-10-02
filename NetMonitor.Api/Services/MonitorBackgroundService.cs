using Microsoft.EntityFrameworkCore;
using NetMonitor.Api.Data;

namespace NetMonitor.Api.Services;

public class MonitorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonitorBackgroundService> _logger;

    public MonitorBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<MonitorBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitor background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAllMonitorsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while checking monitors.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }

        _logger.LogInformation("Monitor background service stopped.");
    }

    private async Task CheckAllMonitorsAsync(
        CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<NetMonitorDbContext>();

        var pingMonitorService = scope.ServiceProvider
            .GetRequiredService<PingMonitorService>();

        var monitors = await context.MonitorTargets
            .Where(m => m.IsActive)
            .ToListAsync(stoppingToken);

        _logger.LogInformation(
            "Checking {Count} active monitors.",
            monitors.Count);

        foreach (var monitor in monitors)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await pingMonitorService
                    .CheckMonitorAsync(monitor.Id);

                _logger.LogInformation(
                    "Checked monitor {MonitorName} ({Host}).",
                    monitor.Name,
                    monitor.Host);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to check monitor {MonitorName} ({Host}).",
                    monitor.Name,
                    monitor.Host);
            }
        }
    }
}