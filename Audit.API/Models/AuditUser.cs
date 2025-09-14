using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Audit.API.Models;

[Table("Users", Schema = "audit")]
public class AuditUser
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string ExternalId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();
}
