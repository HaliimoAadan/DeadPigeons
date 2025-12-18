namespace api.Models.Requests;

public class MarkWinnerRequestDto
{
    public Guid BoardId { get; set; }
    public int WinningNumbersMatched { get; set; }
}