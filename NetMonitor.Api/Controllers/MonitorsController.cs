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

    // GET: api/monitors/5/results returns list of monitor results for the specified monitor target ID, with optional limit query parameter
    [HttpGet("{id}/results")]
    public async Task<ActionResult<IEnumerable<MonitorResult>>> GetResults(
    int id,
    [FromQuery] int limit = 50)
    {
        if (limit <= 0 || limit > 500)
        {
            return BadRequest("Limit must be between 1 and 500.");
        }

        var monitorExists = await _context.MonitorTargets
            .AnyAsync(m => m.Id == id);

        if (!monitorExists)
        {
            return NotFound();
        }

        var results = await _context.MonitorResults
            .AsNoTracking()
            .Where(r => r.MonitorTargetId == id)
            .OrderByDescending(r => r.CheckedAt)
            .Take(limit)
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/monitors/5/statistics returns statistics for the specified monitor target ID
    [HttpGet("{id}/statistics")]
    public async Task<IActionResult> GetStatistics(int id)
    {
        var monitorExists = await _context.MonitorTargets
            .AnyAsync(m => m.Id == id);

        if (!monitorExists)
        {
            return NotFound();
        }

        var results = await _context.MonitorResults
            .AsNoTracking()
            .Where(r => r.MonitorTargetId == id)
            .ToListAsync();

        if (results.Count == 0)
        {
            return Ok(new
            {
                uptimePercentage = 0,
                averageResponseTimeMs = 0,
                successfulChecks = 0,
                failedChecks = 0,
                totalChecks = 0,
                lastStatus = "Unknown"
            });
        }

        var successfulChecks = results.Count(r => r.IsSuccess);
        var failedChecks = results.Count(r => !r.IsSuccess);

        var uptimePercentage =
            (double)successfulChecks / results.Count * 100;

        var averageResponseTimeMs =
            results.Average(r => r.ResponseTimeMs);

        var latestResult = results
            .OrderByDescending(r => r.CheckedAt)
            .First();

        return Ok(new
        {
            uptimePercentage = Math.Round(uptimePercentage, 2),
            averageResponseTimeMs = Math.Round(averageResponseTimeMs, 2),
            successfulChecks,
            failedChecks,
            totalChecks = results.Count,
            lastStatus = latestResult.IsSuccess
                ? "Online"
                : "Offline"
        });
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