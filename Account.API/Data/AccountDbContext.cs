using Microsoft.EntityFrameworkCore;
using Account.API.Models;

namespace Account.API.Data;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("account");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "account");
            entity.HasKey(e => e.Id); 
            entity.Property(e => e.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Salt).HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("timezone('utc', now())")
                  .HasConversion(
                      v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entity.Property(e => e.LastLoginAt)
                  .HasConversion(
                      v => v.HasValue && v.Value.Kind == DateTimeKind.Utc ? v
                           : v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                      v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            entity.Property(e => e.UpdatedAt)
                  .HasConversion(
                      v => v.HasValue && v.Value.Kind == DateTimeKind.Utc ? v
                           : v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                      v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("persons", "account");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Notes).HasMaxLength(500);

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
             
            entity.Property(e => e.DateOfBirth)
                  .HasConversion(
                      v => v.HasValue && v.Value.Kind == DateTimeKind.Utc ? v
                           : v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                      v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Person)
                  .HasForeignKey<Person>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
             
            entity.HasIndex(e => e.UserId).IsUnique();
        });
    }

}
