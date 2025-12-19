using efscaffold.Entities;

namespace api.Services;

public interface IGameService
{
    Task<Game> CreateAsync(Game game);
    Task<Game?> GetByIdAsync(Guid id);
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> UpdateAsync(Game game);
    Task<bool> DeleteAsync(Guid id);
}