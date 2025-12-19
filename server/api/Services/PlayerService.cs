using efscaffold;
using efscaffold.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class PlayerService : IPlayerService
{
    private readonly MyDbContext _db;
    public PlayerService(MyDbContext db) => _db = db;

    public async Task<Player> CreateAsync(Player player)
    {
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        return player;
    }

    public async Task<Player?> GetByIdAsync(Guid id) =>
        await _db.Players
            .Include(p => p.Boards)
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.PlayerId == id);

    public async Task<IEnumerable<Player>> GetAllAsync() => await _db.Players.ToListAsync();

    public async Task<Player?> UpdateAsync(Player player)
    {
        var existing = await _db.Players.FindAsync(player.PlayerId);
        if (existing == null) return null;
        _db.Entry(existing).CurrentValues.SetValues(player);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null) return false;
        _db.Players.Remove(player);
        await _db.SaveChangesAsync();
        return true;
    }
}