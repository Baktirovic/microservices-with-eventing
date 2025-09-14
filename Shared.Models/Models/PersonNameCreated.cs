namespace Shared.Models.Models;

public class PersonNameCreated
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string EventId { get; set; } = Guid.NewGuid().ToString();
}

