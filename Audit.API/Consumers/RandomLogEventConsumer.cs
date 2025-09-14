using MassTransit;
using Audit.API.Data;
using Audit.API.Models;
using Shared.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Audit.API.Consumers;

public class RandomLogEventConsumer : IConsumer<RandomLogEvent>
{
    private readonly AuditDbContext _context;
    private readonly ILogger<RandomLogEventConsumer> _logger;
    private readonly HttpClient _httpClient;

    public RandomLogEventConsumer(AuditDbContext context, ILogger<RandomLogEventConsumer> logger, HttpClient httpClient)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task Consume(ConsumeContext<RandomLogEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received RandomLogEvent for UserId: {UserId}, Action: {Action}, EventType: {EventType}", 
            message.UserId, message.Action, message.EventType);

        try
        {
            var auditUser = await _context.Users.FirstOrDefaultAsync(u => u.ExternalId == message.UserId.ToString());

            if (auditUser == null)
            { 
                auditUser = new AuditUser
                {
                    ExternalId = message.UserId.ToString(),
                    Name = "",
                    CreatedAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc),
                    UpdatedAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc)
                };
                _context.Users.Add(auditUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created new AuditUser for ExternalId: {ExternalId} with Name: {Name}", auditUser.ExternalId, auditUser.Name);
            }

            var log = new Log
            {
                Action = message.Action,
                UserId = auditUser.Id,
                Message = $"{message.EventType}: {message.Message} (Severity: {message.Severity})",
                CreatedAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc)
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created log entry for RandomLogEvent. LogId: {LogId}, Action: {Action}", 
                log.Id, log.Action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RandomLogEvent for UserId: {UserId}", message.UserId);
            throw;
        }
    }
}
