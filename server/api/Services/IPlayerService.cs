using efscaffold.Entities;

namespace api.Services;

public interface IPlayerService
{
    Task<Player> CreateAsync(Player player);
    Task<Player?> GetByIdAsync(Guid id);
    Task<IEnumerable<Player>> GetAllAsync();
    Task<Player?> UpdateAsync(Player player);
    Task<bool> DeleteAsync(Guid id);
}