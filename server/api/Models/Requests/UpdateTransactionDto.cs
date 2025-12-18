namespace api.Models.Requests;

public sealed class UpdateTransactionDto
{
    public Guid PlayerId { get; set; }
    public string MobilePayReqId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? Timestamp { get; set; } // ✅ DateTime, nu DateTimeOffset
}