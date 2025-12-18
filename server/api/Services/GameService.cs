using efscaffold.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class GameService : IGameService
{
    private readonly MyDbContext _db;
    public GameService(MyDbContext db) => _db = db;

    public async Task<Game> CreateAsync(Game game)
    {
        _db.Games.Add(game);
        await _db.SaveChangesAsync();
        return game;
    }

    public async Task<Game?> GetByIdAsync(Guid id) =>
        await _db.Games
            .Include(g => g.BoardGames)
            .Include(g => g.Winningboards)
            .FirstOrDefaultAsync(g => g.GameId == id);

    public async Task<IEnumerable<Game>> GetAllAsync() => await _db.Games.ToListAsync();

    public async Task<Game?> UpdateAsync(Game game)
    {
        var existing = await _db.Games.FindAsync(game.GameId);
        if (existing == null) return null;
        _db.Entry(existing).CurrentValues.SetValues(game);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game == null) return false;
        _db.Games.Remove(game);
        await _db.SaveChangesAsync();
        return true;
    }
}