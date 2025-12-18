using efscaffold;
using efscaffold.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class BoardService : IBoardService
{
    private readonly MyDbContext _db;
    public BoardService(MyDbContext db) => _db = db;

    public async Task<Board> CreateAsync(Board board)
    {
        _db.Boards.Add(board);
        await _db.SaveChangesAsync();
        return board;
    }

    public async Task<Board?> GetByIdAsync(Guid id) =>
        await _db.Boards
            .Include(b => b.Player)
            .Include(b => b.Game)
            .Include(b => b.Winningboards)
            .FirstOrDefaultAsync(b => b.BoardId == id);

    public async Task<IEnumerable<Board>> GetAllAsync() => await _db.Boards.ToListAsync();

    public async Task<Board?> UpdateAsync(Board board)
    {
        var existing = await _db.Boards.FindAsync(board.BoardId);
        if (existing == null) return null;
        _db.Entry(existing).CurrentValues.SetValues(board);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var board = await _db.Boards.FindAsync(id);
        if (board == null) return false;
        _db.Boards.Remove(board);
        await _db.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
/// Creates a board using wallet rules:
/// balance = SUM(Approved transactions) - SUM(board prices).
/// Balance is never allowed to go negative.
/// </summary>
public async Task<Board> PurchaseAsync(
    Guid playerId,
    Guid gameId,
    List<int> chosenNumbers,
    bool isRepeating,
    Guid? repeatUntilGameId,
    CancellationToken ct = default)
{
    ValidateChosenNumbersOrThrow(chosenNumbers);

    // Validate player exists
    var playerExists = await _db.Players
        .AnyAsync(p => p.PlayerId == playerId, ct);
    if (!playerExists)
        throw new InvalidOperationException("Player does not exist.");

    // Validate game exists
    var gameExists = await _db.Games
        .AnyAsync(g => g.GameId == gameId, ct);
    if (!gameExists)
        throw new InvalidOperationException("Game does not exist.");

    // Validate repeat-until game if provided
    if (repeatUntilGameId is not null)
    {
        var repeatGameExists = await _db.Games
            .AnyAsync(g => g.GameId == repeatUntilGameId, ct);
        if (!repeatGameExists)
            throw new InvalidOperationException("RepeatUntilGameId does not exist.");
    }

    var price = CalculatePrice(chosenNumbers.Count);

    // MONEY IN
    var approvedDeposits = await _db.Transactions
        .AsNoTracking()
        .Where(t => t.PlayerId == playerId && t.Status == "Approved")
        .Select(t => (decimal?)t.Amount)
        .SumAsync(ct) ?? 0m;

    // MONEY OUT
    var spent = await _db.Boards
        .AsNoTracking()
        .Where(b => b.PlayerId == playerId)
        .Select(b => (decimal?)b.Price)
        .SumAsync(ct) ?? 0m;

    var balance = approvedDeposits - spent;

    if (balance < price)
        throw new InvalidOperationException("Insufficient balance.");

    var board = new Board
    {
        BoardId = Guid.NewGuid(),
        PlayerId = playerId,
        GameId = gameId,
        ChosenNumbers = chosenNumbers.OrderBy(x => x).ToList(),
        Price = price,
        IsRepeating = isRepeating,
        RepeatUntilGameId = repeatUntilGameId,
        Timestamp = DateTime.UtcNow
    };

    _db.Boards.Add(board);
    await _db.SaveChangesAsync(ct);

    return board;
}
    
    private static void ValidateChosenNumbersOrThrow(List<int> numbers)
    {
        if (numbers is null)
            throw new ArgumentException("ChosenNumbers is required.");

        if (numbers.Count < 5 || numbers.Count > 8)
            throw new ArgumentException("ChosenNumbers must contain between 5 and 8 numbers.");

        if (numbers.Any(n => n < 1 || n > 16))
            throw new ArgumentException("All numbers must be between 1 and 16.");

        if (numbers.Distinct().Count() != numbers.Count)
            throw new ArgumentException("ChosenNumbers must be unique.");
    }

    private static decimal CalculatePrice(int count) => count switch
    {
        5 => 20m,
        6 => 40m,
        7 => 80m,
        8 => 160m,
        _ => throw new ArgumentOutOfRangeException(nameof(count))
    };


}

