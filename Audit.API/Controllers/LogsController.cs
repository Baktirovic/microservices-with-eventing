using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Audit.API.Data;
using Audit.API.Models;

namespace Audit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly AuditDbContext _context;

    public LogsController(AuditDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Log>>> GetLogs()
    {
        var logs = await _context.Logs
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        
        foreach (var log in logs)
        {
            if (log.User != null)
            {
                log.User.Logs = new List<Log>();
            }
        }
        
        return logs;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Log>> GetLog(int id)
    {
        var log = await _context.Logs
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (log == null)
        {
            return NotFound();
        }

        if (log.User != null)
        {
            log.User.Logs = new List<Log>();
        }

        return log;
    }

    [HttpPost]
    public async Task<ActionResult<Log>> CreateLog(CreateLogRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalId == request.UserExternalId);

        if (user == null)
        {
            user = new AuditUser
            {
                ExternalId = request.UserExternalId,
                Name = request.UserName ?? "Unknown User",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var log = new Log
        {
            Action = request.Action,
            UserId = user.Id,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();

        await _context.Entry(log)
            .Reference(l => l.User)
            .LoadAsync();

        if (log.User != null)
        {
            log.User.Logs = new List<Log>();
        }

        return CreatedAtAction(nameof(GetLog), new { id = log.Id }, log);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Log>>> GetLogsByUser(int userId)
    {
        var logs = await _context.Logs
            .Include(l => l.User)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        
        foreach (var log in logs)
        {
            if (log.User != null)
            {
                log.User.Logs = new List<Log>();
            }
        }
        
        return logs;
    }

    [HttpGet("action/{action}")]
    public async Task<ActionResult<IEnumerable<Log>>> GetLogsByAction(string action)
    {
        var logs = await _context.Logs
            .Include(l => l.User)
            .Where(l => l.Action == action)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        
        foreach (var log in logs)
        {
            if (log.User != null)
            {
                log.User.Logs = new List<Log>();
            }
        }
        
        return logs;
    }
}

public class CreateLogRequest
{
    public string Action { get; set; } = string.Empty;
    public string UserExternalId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Message { get; set; } = string.Empty;
}
