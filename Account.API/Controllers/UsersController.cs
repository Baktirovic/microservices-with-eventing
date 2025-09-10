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
    public async Task<ActionResult<User>> GetUser(int id)
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
            CreatedAt = user.CreatedAt
        });

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || !user.IsActive)
        {
            return NotFound();
        }

        user.Email = request.Email;
        user.IsEmailVerified = request.IsEmailVerified;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
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
}
