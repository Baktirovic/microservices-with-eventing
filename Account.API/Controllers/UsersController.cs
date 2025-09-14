using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Account.API.Data;
using Account.API.Models;
using MassTransit;
using Shared.Models.Models;

namespace Account.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AccountDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public UsersController(AccountDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Person)
            .Where(u => u.IsActive)
            .ToListAsync();
        
        foreach (var user in users)
        {
            if (user.Person != null)
            {
                user.Person.User = null;
            }
        }
        
        return users;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

        if (user == null)
        {
            return NotFound();
        }

        if (user.Person != null)
        {
            user.Person.User = null;
        }

        return user;
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest("Username already exists");
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("Email already exists");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = request.PasswordHash,
            Salt = request.Salt,
            IsActive = true,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new PersonNameCreated
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = "",
            LastName = "",
            MiddleName = null,
            CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc)
        });

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
            
        if (user == null)
        {
            return NotFound();
        }

        // Update user properties
        user.Email = request.Email;
        user.IsEmailVerified = request.IsEmailVerified;
        user.UpdatedAt = DateTime.UtcNow;

        // Handle person name changes
        if (user.Person != null && (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName)))
        {
            var oldFirstName = user.Person.FirstName;
            var oldLastName = user.Person.LastName;
            var oldMiddleName = user.Person.MiddleName;

            // Update person names if provided
            if (!string.IsNullOrEmpty(request.FirstName))
            {
                user.Person.FirstName = request.FirstName;
            }
            if (!string.IsNullOrEmpty(request.LastName))
            {
                user.Person.LastName = request.LastName;
            }
            
            user.Person.UpdatedAt = DateTime.UtcNow;

            // Check if names actually changed
            bool namesChanged = (oldFirstName != user.Person.FirstName) || (oldLastName != user.Person.LastName);

            await _context.SaveChangesAsync();

            // Publish PersonNameChanged event if names changed
            if (namesChanged)
            {
                await _publishEndpoint.Publish(new PersonNameChanged
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    OldFirstName = oldFirstName,
                    NewFirstName = user.Person.FirstName,
                    OldLastName = oldLastName,
                    NewLastName = user.Person.LastName,
                    OldMiddleName = oldMiddleName,
                    NewMiddleName = user.Person.MiddleName,
                    ChangedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                });
            }
        }
        else
        {
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || !user.IsActive)
        {
            return NotFound();
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Salt { get; set; }
}

public class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
