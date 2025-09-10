namespace Shared.Models.Models;

public class RandomLogEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
