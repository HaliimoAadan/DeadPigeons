using efscaffold;
using efscaffold.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class WinningBoardService : IWinningBoardService
{
    private readonly MyDbContext _db;
    public WinningBoardService(MyDbContext db) => _db = db;

    public async Task<Winningboard> CreateAsync(Winningboard wb)
    {
        _db.Winningboards.Add(wb);
        await _db.SaveChangesAsync();
        return wb;
    }

    public async Task<Winningboard?> GetByIdAsync(Guid id) =>
        await _db.Winningboards
            .Include(w => w.Board)
            .Include(w => w.Game)
            .FirstOrDefaultAsync(w => w.WinningboardId == id);

    public async Task<IEnumerable<Winningboard>> GetAllAsync() => await _db.Winningboards.ToListAsync();

    public async Task<Winningboard?> UpdateAsync(Winningboard wb)
    {
        var existing = await _db.Winningboards.FindAsync(wb.WinningboardId);
        if (existing == null) return null;
        _db.Entry(existing).CurrentValues.SetValues(wb);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var wb = await _db.Winningboards.FindAsync(id);
        if (wb == null) return false;
        _db.Winningboards.Remove(wb);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Winningboard> CheckAndCreateWinningBoardAsync(Guid boardId)
    {
        var board = await _db.Boards.Include(b => b.Game).FirstOrDefaultAsync(b => b.BoardId == boardId);
        if (board == null) throw new ArgumentException("Board not found");

        var game = board.Game;
        if (game.WinningNumbers == null || game.WinningNumbers.Count != 3)
            throw new InvalidOperationException("Game winning numbers not set correctly");

        // Count how many of the 3 admin numbers exist in player's chosen numbers
        int matches = game.WinningNumbers.Count(n => board.ChosenNumbers.Contains(n));

        var winningBoard = new Winningboard
        {
            WinningboardId = Guid.NewGuid(),
            GameId = board.GameId,
            BoardId = board.BoardId,
            WinningNumbersMatched = matches,
            Timestamp = DateTime.UtcNow
        };

        _db.Winningboards.Add(winningBoard);
        await _db.SaveChangesAsync();

        return winningBoard;
    }

    public async Task<List<Winningboard>> ComputeWinningBoardsAsync(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.BoardGames)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
            throw new Exception("Game not found");

        if (game.WinningNumbers == null || game.WinningNumbers.Count != 3)
            throw new Exception("Winning numbers not set for this game");

        var winningBoards = new List<Winningboard>();

        foreach (var board in game.BoardGames)
        {
            var matchedNumbers = board.ChosenNumbers.Intersect(game.WinningNumbers).Count();

            if (matchedNumbers == 3) 
            {
                var winningBoard = new Winningboard
                {
                    WinningboardId = Guid.NewGuid(),
                    GameId = game.GameId,
                    BoardId = board.BoardId,
                    WinningNumbersMatched = matchedNumbers,
                    Timestamp = DateTime.UtcNow
                };

                _db.Winningboards.Add(winningBoard);
                winningBoards.Add(winningBoard);
            }
        }

        await _db.SaveChangesAsync();
        return winningBoards;
    }
}