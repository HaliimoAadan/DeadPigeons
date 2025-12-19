namespace api.Models.Requests;

public sealed class PurchaseBoardRequestDto
{
    public Guid GameId { get; set; }
    public List<int> ChosenNumbers { get; set; } = new();
    public bool IsRepeating { get; set; }
    public Guid? RepeatUntilGameId { get; set; }
}