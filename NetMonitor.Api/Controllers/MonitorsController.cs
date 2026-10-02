using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetMonitor.Api.Data;
using NetMonitor.Api.Models;
using NetMonitor.Api.Services;

namespace NetMonitor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonitorsController : ControllerBase
{
    private readonly NetMonitorDbContext _context;

    public MonitorsController(NetMonitorDbContext context)
    {
        _context = context;
    }

    // GET: api/monitors returns list of all monitor targets in the database
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MonitorTarget>>> GetMonitors()
    {
        var monitors = await _context.MonitorTargets
            .ToListAsync();

        return Ok(monitors);
    }

    // GET: api/monitors/5 returns monitor target with the specified ID or 404 if not found
    [HttpGet("{id}")]
    public async Task<ActionResult<MonitorTarget>> GetMonitor(int id)
    {
        var monitor = await _context.MonitorTargets
            .FindAsync(id);

        if (monitor == null)
        {
            return NotFound();
        }

        return Ok(monitor);
    }

    // POST: api/monitors create new monitor target and return the created monitor target with its ID
    [HttpPost]
    public async Task<ActionResult<MonitorTarget>> CreateMonitor(
        MonitorTarget monitor)
    {
        monitor.Id = 0;
        monitor.CreatedDate = DateTime.UtcNow;

        _context.MonitorTargets.Add(monitor);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMonitor),
            new { id = monitor.Id },
            monitor);
    }

    // POST: api/monitors/5/check tests PingMonitorService
    [HttpPost("{id}/check")]
    public async Task<IActionResult> CheckMonitor(
    int id,
    [FromServices] PingMonitorService pingMonitorService)
    {
        var monitor = await _context.MonitorTargets.FindAsync(id);

        if (monitor == null)
        {
            return NotFound();
        }

        await pingMonitorService.CheckMonitorAsync(id);

        return Ok(new
        {
            message = "Monitor checked successfully."
        });
    }

    // PUT: api/monitors/5 updates monitor target with the specified ID or 404 if not found
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMonitor(
        int id,
        MonitorTarget monitor)
    {

        var existingMonitor = await _context.MonitorTargets
            .FindAsync(id);

        if (existingMonitor == null)
        {
            return NotFound();
        }

        existingMonitor.Name = monitor.Name;
        existingMonitor.Host = monitor.Host;
        existingMonitor.Type = monitor.Type;
        existingMonitor.IsActive = monitor.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/monitors/5 deletes monitor target with the specified ID or 404 if not found
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMonitor(int id)
    {
        var monitor = await _context.MonitorTargets
            .FindAsync(id);

        if (monitor == null)
        {
            return NotFound();
        }

        _context.MonitorTargets.Remove(monitor);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}