namespace Shared.Models.Models;

public class AuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? AdditionalData { get; set; }
}

public class CreateAuditEventRequest
{
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? AdditionalData { get; set; }
}

