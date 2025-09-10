namespace Shared.Models.Models;

public class BusinessTransaction
{
    public int Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}

public class CreateTransactionRequest
{
    public string TransactionType { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}
