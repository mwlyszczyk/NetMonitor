using Microsoft.EntityFrameworkCore;
using NetMonitor.Api.Models;

namespace NetMonitor.Api.Data;

public class NetMonitorDbContext : DbContext
{
    public NetMonitorDbContext(
        DbContextOptions<NetMonitorDbContext> options)
        : base(options)
    {
    }

    public DbSet<MonitorTarget> MonitorTargets => Set<MonitorTarget>();

    public DbSet<MonitorResult> MonitorResults => Set<MonitorResult>();
}