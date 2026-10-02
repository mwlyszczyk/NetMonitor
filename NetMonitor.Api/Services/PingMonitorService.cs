using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using NetMonitor.Api.Data;
using NetMonitor.Api.Models;

namespace NetMonitor.Api.Services;

public class PingMonitorService
{
    private readonly NetMonitorDbContext _context;

    public PingMonitorService(NetMonitorDbContext context)
    {
        _context = context;
    }

    public async Task CheckMonitorAsync(int monitorId)
    {
        var monitor = await _context.MonitorTargets
            .FirstOrDefaultAsync(m => m.Id == monitorId);

        if (monitor == null)
        {
            return;
        }

        if (!monitor.IsActive)
        {
            return;
        }

        using var ping = new Ping();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var reply = await ping.SendPingAsync(
                monitor.Host,
                5000);

            stopwatch.Stop();

            var result = new MonitorResult
            {
                MonitorTargetId = monitor.Id,
                IsSuccess = reply.Status == IPStatus.Success,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorMessage = reply.Status == IPStatus.Success
                    ? null
                    : reply.Status.ToString(),
                CheckedAt = DateTime.UtcNow
            };

            _context.MonitorResults.Add(result);

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var result = new MonitorResult
            {
                MonitorTargetId = monitor.Id,
                IsSuccess = false,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorMessage = ex.Message,
                CheckedAt = DateTime.UtcNow
            };

            _context.MonitorResults.Add(result);

            await _context.SaveChangesAsync();
        }
    }
}