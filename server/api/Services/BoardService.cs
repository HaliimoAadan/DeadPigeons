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
}