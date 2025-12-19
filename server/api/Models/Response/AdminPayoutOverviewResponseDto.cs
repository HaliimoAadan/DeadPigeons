namespace api.Models.Response;

public class AdminPayoutOverviewResponseDto
{
    public Guid GameId { get; set; }

    public int TotalPlayers { get; set; }
    public decimal TotalPrizePool { get; set; }

    public int WinnerCount { get; set; }
    public decimal Profit30Percent { get; set; }
    public decimal WinnersPool70Percent { get; set; }

    // Cash offline payout (NOT added to balance)
    public decimal PayoutPerWinner { get; set; }

    // If rounding creates leftover cents
    public decimal Remainder { get; set; }

    public List<AdminWinnerLineDto> Winners { get; set; } = new();
}

public class AdminWinnerLineDto
{
    public Guid WinningboardId { get; set; }
    public Guid BoardId { get; set; }
    public Guid PlayerId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public int WinningNumbersMatched { get; set; }
    public DateTime Timestamp { get; set; }

    public decimal Payout { get; set; }
}