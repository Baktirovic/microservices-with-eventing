using Microsoft.AspNetCore.Mvc;
using Shared.Models.Models;
using Shared.Models.SeedData;
using MassTransit;

namespace BusinessLogic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    //private static readonly List<BusinessTransaction> _transactions = new();
    
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(IPublishEndpoint publishEndpoint, ILogger<TransactionsController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }


    [HttpPost]
    public async Task<ActionResult<BusinessTransaction>> CreateTransaction()
    {
        var users = SeedDataProvider.GetSeedUsers();
        var accounts = SeedDataProvider.GetSeedAccounts(users);
        
        var random = new Random();
        var randomUser = users[random.Next(users.Count)];
        var userAccounts = accounts.Where(a => a.UserId == randomUser.Id).ToList();
        
        if (!userAccounts.Any())
        {
            return BadRequest("No accounts found for the selected user");
        }
        
        var randomAccount = userAccounts[random.Next(userAccounts.Count)];
        
        var transactionTypes = new[] { "Deposit", "Withdrawal", "Transfer", "Payment", "Refund" };
        var randomType = transactionTypes[random.Next(transactionTypes.Length)];
        var randomAmount = Math.Round((decimal)(random.NextDouble() * 1000 + 10), 2);
        var ammount = random.Next(100, 10000);

        var logEvent = new RandomLogEvent
        {
            UserId = randomUser.Id,
            EventType = "TransactionCreated",
            Action = "CreateTransaction",
            Message = $"Transaction for {randomType} with ammount of {ammount}",
            Severity = "Info",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        };

        await _publishEndpoint.Publish(logEvent); 

        return CreatedAtAction(nameof(CreateTransaction), new { id =0 });
    }

}

