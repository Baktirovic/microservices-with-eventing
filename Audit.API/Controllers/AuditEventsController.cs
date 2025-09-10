using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Models;
using Audit.API.Data;
using Audit.API.Models;

namespace Audit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditEventsController : ControllerBase
{
    private readonly AuditDbContext _context;

    public AuditEventsController(AuditDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditEvent>>> GetAuditEvents()
    {
        var logs = await _context.Logs
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var auditEvents = logs.Select(log => new AuditEvent
        {
            Id = log.Id,
            EventType = log.Action,
            EntityType = "Log",
            EntityId = log.Id,
            Description = log.Message,
            UserId = log.User?.ExternalId ?? "Unknown",
            Timestamp = log.CreatedAt,
            AdditionalData = $"User: {log.User?.Name ?? "Unknown"}"
        });

        return Ok(auditEvents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuditEvent>> GetAuditEvent(int id)
    {
        var log = await _context.Logs
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (log == null)
        {
            return NotFound();
        }

        var auditEvent = new AuditEvent
        {
            Id = log.Id,
            EventType = log.Action,
            EntityType = "Log",
            EntityId = log.Id,
            Description = log.Message,
            UserId = log.User?.ExternalId ?? "Unknown",
            Timestamp = log.CreatedAt,
            AdditionalData = $"User: {log.User?.Name ?? "Unknown"}"
        };

        return Ok(auditEvent);
    }

    [HttpPost]
    public async Task<ActionResult<AuditEvent>> CreateAuditEvent(CreateAuditEventRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalId == request.UserId);

        if (user == null)
        {
            user = new AuditUser
            {
                ExternalId = request.UserId,
                Name = "System User",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var log = new Log
        {
            Action = request.EventType,
            UserId = user.Id,
            Message = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();

        var auditEvent = new AuditEvent
        {
            Id = log.Id,
            EventType = log.Action,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Description = log.Message,
            UserId = user.ExternalId,
            Timestamp = log.CreatedAt,
            AdditionalData = request.AdditionalData
        };

        return CreatedAtAction(nameof(GetAuditEvent), new { id = auditEvent.Id }, auditEvent);
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<AuditEvent>>> GetAuditEventsByEntity(string entityType, int entityId)
    {
        var logs = await _context.Logs
            .Include(l => l.User)
            .Where(l => l.Action == entityType)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var auditEvents = logs.Select(log => new AuditEvent
        {
            Id = log.Id,
            EventType = log.Action,
            EntityType = entityType,
            EntityId = entityId,
            Description = log.Message,
            UserId = log.User?.ExternalId ?? "Unknown",
            Timestamp = log.CreatedAt,
            AdditionalData = $"User: {log.User?.Name ?? "Unknown"}"
        });

        return Ok(auditEvents);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<AuditEvent>>> GetAuditEventsByUser(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalId == userId);

        if (user == null)
        {
            return Ok(new List<AuditEvent>());
        }

        var logs = await _context.Logs
            .Include(l => l.User)
            .Where(l => l.UserId == user.Id)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var auditEvents = logs.Select(log => new AuditEvent
        {
            Id = log.Id,
            EventType = log.Action,
            EntityType = "Log",
            EntityId = log.Id,
            Description = log.Message,
            UserId = user.ExternalId,
            Timestamp = log.CreatedAt,
            AdditionalData = $"User: {user.Name}"
        });

        return Ok(auditEvents);
    }
}
