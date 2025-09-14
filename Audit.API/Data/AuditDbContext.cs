using Microsoft.EntityFrameworkCore;
using Audit.API.Models;

namespace Audit.API.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<Log> Logs { get; set; }
    public DbSet<AuditUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("audit");
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Action)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Message)
                  .IsRequired()
                  .HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("timezone('utc', now())")
                  .HasConversion(
                      v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Logs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditUser>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.ExternalId)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("timezone('utc', now())")
                  .HasConversion(
                      v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entity.Property(e => e.UpdatedAt)
                  .HasConversion(
                      v => v.HasValue && v.Value.Kind == DateTimeKind.Utc ? v
                           : v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                      v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            entity.HasIndex(e => e.ExternalId).IsUnique();
        });
    }
}
