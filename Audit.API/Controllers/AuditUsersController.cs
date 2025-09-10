using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Audit.API.Data;
using Audit.API.Models;

namespace Audit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditUsersController : ControllerBase
{
    private readonly AuditDbContext _context;

    public AuditUsersController(AuditDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditUser>>> GetAuditUsers()
    {
        var users = await _context.Users
            .Include(u => u.Logs)
            .OrderBy(u => u.Name)
            .ToListAsync();
        
        foreach (var user in users)
        {
            user.Logs = new List<Log>();
        }
        
        return users;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuditUser>> GetAuditUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Logs)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        user.Logs = new List<Log>();

        return user;
    }

    [HttpGet("external/{externalId}")]
    public async Task<ActionResult<AuditUser>> GetAuditUserByExternalId(string externalId)
    {
        var user = await _context.Users
            .Include(u => u.Logs)
            .FirstOrDefaultAsync(u => u.ExternalId == externalId);

        if (user == null)
        {
            return NotFound();
        }

        user.Logs = new List<Log>();

        return user;
    }

    [HttpPost]
    public async Task<ActionResult<AuditUser>> CreateAuditUser(CreateAuditUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.ExternalId == request.ExternalId))
        {
            return BadRequest("User with this External ID already exists");
        }

        var user = new AuditUser
        {
            ExternalId = request.ExternalId,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAuditUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuditUser(int id, UpdateAuditUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.Name = request.Name;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuditUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateAuditUserRequest
{
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UpdateAuditUserRequest
{
    public string Name { get; set; } = string.Empty;
}
