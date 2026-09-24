namespace NetMonitor.Api.Models;

public class MonitorTarget
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public string Type { get; set; } = "Ping";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}