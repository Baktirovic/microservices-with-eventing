using Microsoft.AspNetCore.Mvc;
using Shared.Models.Models;

namespace BusinessLogic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private static readonly List<BusinessTransaction> _transactions = new();
    private static int _nextId = 1;

    [HttpGet]
    public ActionResult<IEnumerable<BusinessTransaction>> GetTransactions()
    {
        return Ok(_transactions.OrderByDescending(t => t.ProcessedAt));
    }

    [HttpGet("{id}")]
    public ActionResult<BusinessTransaction> GetTransaction(int id)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        if (transaction == null)
        {
            return NotFound();
        }
        return Ok(transaction);
    }

    [HttpPost]
    public ActionResult<BusinessTransaction> CreateTransaction(CreateTransactionRequest request)
    {
        var transaction = new BusinessTransaction
        {
            Id = _nextId++,
            TransactionType = request.TransactionType,
            AccountId = request.AccountId,
            Amount = request.Amount,
            Description = request.Description,
            ProcessedAt = DateTime.UtcNow,
            Status = "Processed",
            ReferenceNumber = request.ReferenceNumber ?? Guid.NewGuid().ToString()
        };

        _transactions.Add(transaction);
        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
    }

    [HttpGet("account/{accountId}")]
    public ActionResult<IEnumerable<BusinessTransaction>> GetTransactionsByAccount(int accountId)
    {
        var transactions = _transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.ProcessedAt);
        
        return Ok(transactions);
    }

    [HttpGet("type/{transactionType}")]
    public ActionResult<IEnumerable<BusinessTransaction>> GetTransactionsByType(string transactionType)
    {
        var transactions = _transactions
            .Where(t => t.TransactionType == transactionType)
            .OrderByDescending(t => t.ProcessedAt);
        
        return Ok(transactions);
    }
}
