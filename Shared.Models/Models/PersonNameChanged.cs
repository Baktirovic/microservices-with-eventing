namespace Shared.Models.Models;

public class PersonNameChanged
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string OldFirstName { get; set; } = string.Empty;
    public string NewFirstName { get; set; } = string.Empty;
    public string OldLastName { get; set; } = string.Empty;
    public string NewLastName { get; set; } = string.Empty;
    public string? OldMiddleName { get; set; }
    public string? NewMiddleName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string EventId { get; set; } = Guid.NewGuid().ToString();
}
