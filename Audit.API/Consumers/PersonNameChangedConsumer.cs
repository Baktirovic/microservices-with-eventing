using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Models;
using Audit.API.Data;
using Audit.API.Models;

namespace Audit.API.Consumers;

public class PersonNameChangedConsumer : IConsumer<PersonNameChanged>
{
    private readonly AuditDbContext _context;
    private readonly ILogger<PersonNameChangedConsumer> _logger;

    public PersonNameChangedConsumer(AuditDbContext context, ILogger<PersonNameChangedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PersonNameChanged> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received PersonNameChanged event for UserId: {UserId}, Username: {Username}", 
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
                    Name = $"{message.NewFirstName} {message.NewLastName}".Trim(),
                    CreatedAt = message.ChangedAt
                };
                _context.Users.Add(auditUser);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new AuditUser for PersonNameChanged event with ExternalId: {ExternalId}, Name: {Name}", 
                    auditUser.ExternalId, auditUser.Name);
            }
            else
            {
                var newName = $"{message.NewFirstName} {message.NewLastName}".Trim();
                auditUser.Name = newName;
                auditUser.UpdatedAt = message.ChangedAt;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated AuditUser name for ExternalId: {ExternalId}, New Name: {Name}", 
                    auditUser.ExternalId, auditUser.Name);
            }

            var log = new Log
            {
                Action = "PersonNameChanged",
                UserId = auditUser.Id,
                Message = $"Person name changed for user {message.Username} ({message.Email}). " +
                         $"Changed from '{message.OldFirstName} {message.OldMiddleName} {message.OldLastName}' " +
                         $"to '{message.NewFirstName} {message.NewMiddleName} {message.NewLastName}'".Trim(),
                CreatedAt = message.ChangedAt
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created log entry for PersonNameChanged event. LogId: {LogId}", log.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PersonNameChanged event for UserId: {UserId}", message.UserId);
            throw;
        }
    }
}
