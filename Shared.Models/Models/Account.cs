namespace Shared.Models.Models;

public class Account
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateAccountRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
}

public class UpdateAccountRequest
{
    public string AccountHolderName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

