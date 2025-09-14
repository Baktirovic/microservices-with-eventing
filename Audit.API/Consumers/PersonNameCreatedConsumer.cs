using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Models;
using Audit.API.Data;
using Audit.API.Models;

namespace Audit.API.Consumers;

public class PersonNameCreatedConsumer : IConsumer<PersonNameCreated>
{
    private readonly AuditDbContext _context;
    private readonly ILogger<PersonNameCreatedConsumer> _logger;

    public PersonNameCreatedConsumer(AuditDbContext context, ILogger<PersonNameCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PersonNameCreated> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received PersonNameCreated event for UserId: {UserId}, Username: {Username}", 
            message.UserId, message.Username);

        try
        {
            var auditUser = await _context.Users
                .FirstOrDefaultAsync(u => u.ExternalId == message.UserId.ToString());

            if (auditUser == null)
            {
                auditUser = new AuditUser
                {
                    ExternalId = message.UserId.ToString(),
                    Name = $"{message.FirstName} {message.LastName}".Trim(),
                    CreatedAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc)
                };
                _context.Users.Add(auditUser);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new AuditUser with ExternalId: {ExternalId}, Name: {Name}", 
                    auditUser.ExternalId, auditUser.Name);
            }
            else
            {
                var newName = $"{message.FirstName} {message.LastName}".Trim();
                if (auditUser.Name != newName)
                {
                    auditUser.Name = newName;
                    auditUser.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Updated AuditUser name for ExternalId: {ExternalId}, New Name: {Name}", 
                        auditUser.ExternalId, auditUser.Name);
                }
            }

            var log = new Log
            {
                Action = "PersonNameCreated",
                UserId = auditUser.Id,
                Message = $"Person name created for user {message.Username} ({message.Email}). " +
                         $"Name: {message.FirstName} {message.MiddleName} {message.LastName}".Trim(),
                CreatedAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc)
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created log entry for PersonNameCreated event. LogId: {LogId}", log.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PersonNameCreated event for UserId: {UserId}", message.UserId);
            throw;
        }
    }
}
