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
}