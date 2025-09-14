using Microsoft.EntityFrameworkCore;
using Audit.API.Data;
using Audit.API.Models;
using Shared.Models.SeedData;

namespace Audit.API.Services;

public class DatabaseSeeder
{
    private readonly AuditDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AuditDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding for Audit API...");

            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Seed AuditUsers
            await SeedAuditUsersAsync();

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedAuditUsersAsync()
    {
        var existingUsers = await _context.Users.ToListAsync();
        
        if (existingUsers.Any())
        {
            _logger.LogInformation("AuditUsers already exist, skipping seeding.");
            return;
        }

        var seedUsers = SeedDataProvider.GetSeedAuditUsers();
        var auditUsers = seedUsers.Select(seedUser => new AuditUser
        {
            Id = seedUser.Id,
            ExternalId = seedUser.ExternalId,
            Name = seedUser.Name,
            CreatedAt = DateTime.SpecifyKind(seedUser.CreatedAt, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(seedUser.CreatedAt, DateTimeKind.Utc)
        }).ToList();

        _context.Users.AddRange(auditUsers);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} AuditUsers.", auditUsers.Count);
    }
}
