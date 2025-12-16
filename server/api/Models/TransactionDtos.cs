using System.ComponentModel.DataAnnotations;

namespace api.Models;

public sealed class CreateTransactionDto
{
    [Required]
    public Guid PlayerId { get; set; }

    [Required]
    public string MobilePayReqId { get; set; } = string.Empty;

    // Note: Validation is handled in the controller to avoid culture-specific Range issues.
    public decimal Amount { get; set; }
}


public sealed class UpdateTransactionStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty; // Pending/Approved/Rejected
}

public sealed class TransactionResponseDto
{
    public Guid TransactionId { get; set; }
    public Guid PlayerId { get; set; }
    public string MobilePayReqId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}

public sealed class AdminTransactionListItemDto
{
    public Guid TransactionId { get; set; }
    public string MobilePayReqId { get; set; } = string.Empty;

    public Guid PlayerId { get; set; }
    public string PlayerFirstName { get; set; } = string.Empty;
    public string PlayerLastName { get; set; } = string.Empty;
    public string PlayerEmail { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}
public sealed class SubmitTransactionDto
{
    [Required]
    public string MobilePayReqId { get; set; } = string.Empty;

    // The player doesn't type this manually; UI sends the calculated board price.
    public decimal Amount { get; set; }
}