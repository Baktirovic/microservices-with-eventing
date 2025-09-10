using MassTransit;
using Shared.Models.Models;
using System.Text.Json;

namespace BusinessLogic.API.Services;

public class RandomLogEventGenerator : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RandomLogEventGenerator> _logger;
    private readonly Random _random = new();
    private readonly string[] _actions = {
        "User Login", "User Logout", "Password Change", "Profile Update", "Transaction Created",
        "Transaction Processed", "Transaction Failed", "Account Locked", "Account Unlocked",
        "Email Sent", "SMS Sent", "Data Export", "Data Import", "Report Generated",
        "System Maintenance", "Security Alert", "Performance Warning", "Error Occurred",
        "Configuration Changed", "Backup Completed", "Restore Initiated", "Audit Trail Created"
    };

    private readonly string[] _eventTypes = {
        "Authentication", "Authorization", "Transaction", "System", "Security", "Performance",
        "Audit", "Notification", "Maintenance", "Error", "Warning", "Info"
    };

    private readonly string[] _severities = {
        "Low", "Medium", "High", "Critical", "Info", "Warning", "Error", "Debug"
    };

    private readonly string[] _messages = {
        "User successfully logged into the system",
        "Failed login attempt detected",
        "Password has been changed successfully",
        "Profile information updated",
        "New transaction created and queued for processing",
        "Transaction processed successfully",
        "Transaction failed due to insufficient funds",
        "Account has been locked due to multiple failed attempts",
        "Account unlocked by administrator",
        "Email notification sent to user",
        "SMS notification sent to user",
        "Data export completed successfully",
        "Data import process started",
        "Monthly report generated and sent",
        "System maintenance window scheduled",
        "Security alert: suspicious activity detected",
        "Performance warning: high CPU usage",
        "System error occurred and logged",
        "System configuration updated",
        "Database backup completed successfully",
        "System restore process initiated",
        "Audit trail entry created for compliance"
    };

    public RandomLogEventGenerator(IServiceProvider serviceProvider, ILogger<RandomLogEventGenerator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RandomLogEventGenerator started. Will generate events every 20 seconds.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateRandomLogEvent();
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RandomLogEventGenerator stopped due to cancellation request.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating random log event");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task GenerateRandomLogEvent()
    {
        using var scope = _serviceProvider.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

        try
        {
            var userId = await GetRandomUserIdAsync(httpClient);
            if (userId == 0)
            {
                _logger.LogWarning("No users found in Account.API, skipping event generation");
                return;
            }

            var logEvent = new RandomLogEvent
            {
                UserId = userId,
                Action = _actions[_random.Next(_actions.Length)],
                Message = _messages[_random.Next(_messages.Length)],
                EventType = _eventTypes[_random.Next(_eventTypes.Length)],
                Severity = _severities[_random.Next(_severities.Length)],
                CreatedAt = DateTime.UtcNow,
                Metadata = GenerateRandomMetadata()
            };

            await publishEndpoint.Publish(logEvent);
            
            _logger.LogInformation("Published random log event for UserId: {UserId}, Action: {Action}, EventType: {EventType}", 
                logEvent.UserId, logEvent.Action, logEvent.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate or publish random log event");
            throw;
        }
    }

    private async Task<int> GetRandomUserIdAsync(HttpClient httpClient)
    {
        try
        {
            var response = await httpClient.GetAsync("http://localhost:5001/api/users");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (users != null && users.Count > 0)
                {
                    var randomUser = users[_random.Next(users.Count)];
                    return randomUser.Id;
                }
            }
            else
            {
                _logger.LogWarning("Failed to fetch users from Account.API. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users from Account.API");
        }

        return 0;
    }

    private Dictionary<string, object> GenerateRandomMetadata()
    {
        var metadata = new Dictionary<string, object>
        {
            ["SessionId"] = Guid.NewGuid().ToString(),
            ["IpAddress"] = GenerateRandomIpAddress(),
            ["UserAgent"] = GenerateRandomUserAgent(),
            ["RequestId"] = Guid.NewGuid().ToString(),
            ["Timestamp"] = DateTime.UtcNow.ToString("O"),
            ["Version"] = "1.0.0"
        };

        if (_random.Next(2) == 0)
        {
            metadata["DeviceType"] = _random.Next(2) == 0 ? "Desktop" : "Mobile";
        }

        if (_random.Next(3) == 0)
        {
            metadata["Location"] = GenerateRandomLocation();
        }

        if (_random.Next(4) == 0)
        {
            metadata["Duration"] = _random.Next(100, 5000);
        }

        return metadata;
    }

    private string GenerateRandomIpAddress()
    {
        return $"{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}.{_random.Next(1, 255)}";
    }

    private string GenerateRandomUserAgent()
    {
        var userAgents = new[]
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15",
            "Mozilla/5.0 (Android 10; Mobile; rv:68.0) Gecko/68.0 Firefox/68.0"
        };
        return userAgents[_random.Next(userAgents.Length)];
    }

    private string GenerateRandomLocation()
    {
        var locations = new[]
        {
            "New York, NY", "Los Angeles, CA", "Chicago, IL", "Houston, TX", "Phoenix, AZ",
            "Philadelphia, PA", "San Antonio, TX", "San Diego, CA", "Dallas, TX", "San Jose, CA",
            "Austin, TX", "Jacksonville, FL", "Fort Worth, TX", "Columbus, OH", "Charlotte, NC"
        };
        return locations[_random.Next(locations.Length)];
    }

    private class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
