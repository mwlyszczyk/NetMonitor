namespace NetMonitor.Api.Models;

public class MonitorResult
{
    public int Id { get; set; }

    public int MonitorTargetId { get; set; }

    public bool IsSuccess { get; set; }

    public long ResponseTimeMs { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    public MonitorTarget? MonitorTarget { get; set; }
}