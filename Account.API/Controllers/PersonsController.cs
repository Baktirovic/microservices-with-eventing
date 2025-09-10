using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Account.API.Data;
using Account.API.Models;
using MassTransit;
using Shared.Models.Models;

namespace Account.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly AccountDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public PersonsController(AccountDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Person>>> GetPersons()
    {
        var persons = await _context.Persons
            .Include(p => p.User)
            .ToListAsync();
        
        foreach (var person in persons)
        {
            if (person.User != null)
            {
                person.User.Person = null;
            }
        }
        
        return persons;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Person>> GetPerson(int id)
    {
        var person = await _context.Persons
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
        {
            return NotFound();
        }

        if (person.User != null)
        {
            person.User.Person = null;
        }

        return person;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<Person>> GetPersonByUserId(int userId)
    {
        var person = await _context.Persons
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (person == null)
        {
            return NotFound();
        }

        if (person.User != null)
        {
            person.User.Person = null;
        }

        return person;
    }

    [HttpPost]
    public async Task<ActionResult<Person>> CreatePerson(CreatePersonRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return BadRequest("User not found");
        }

        if (await _context.Persons.AnyAsync(p => p.UserId == request.UserId))
        {
            return BadRequest("Person already exists for this user");
        }

        var person = new Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Notes = request.Notes,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new PersonNameCreated
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = person.FirstName,
            LastName = person.LastName,
            MiddleName = person.MiddleName,
            CreatedAt = person.CreatedAt
        });

        return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePerson(int id, UpdatePersonRequest request)
    {
        var person = await _context.Persons
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (person == null)
        {
            return NotFound();
        }

        var oldFirstName = person.FirstName;
        var oldLastName = person.LastName;
        var oldMiddleName = person.MiddleName;

        person.FirstName = request.FirstName;
        person.LastName = request.LastName;
        person.MiddleName = request.MiddleName;
        person.PhoneNumber = request.PhoneNumber;
        person.Address = request.Address;
        person.City = request.City;
        person.State = request.State;
        person.PostalCode = request.PostalCode;
        person.Country = request.Country;
        person.DateOfBirth = request.DateOfBirth;
        person.Gender = request.Gender;
        person.Notes = request.Notes;
        person.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (oldFirstName != person.FirstName || oldLastName != person.LastName || oldMiddleName != person.MiddleName)
        {
            await _publishEndpoint.Publish(new PersonNameChanged
            {
                UserId = person.UserId,
                Username = person.User.Username,
                Email = person.User.Email,
                OldFirstName = oldFirstName,
                NewFirstName = person.FirstName,
                OldLastName = oldLastName,
                NewLastName = person.LastName,
                OldMiddleName = oldMiddleName,
                NewMiddleName = person.MiddleName,
                ChangedAt = person.UpdatedAt.Value
            });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePerson(int id)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null)
        {
            return NotFound();
        }

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class CreatePersonRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Notes { get; set; }
    public int UserId { get; set; }
}

public class UpdatePersonRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Notes { get; set; }
}
