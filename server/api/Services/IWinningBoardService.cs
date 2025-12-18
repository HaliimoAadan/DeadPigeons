using efscaffold.Entities;

namespace api.Services;

public interface IWinningBoardService
{
    Task<Winningboard> CreateAsync(Winningboard wb);
    Task<Winningboard?> GetByIdAsync(Guid id);
    Task<IEnumerable<Winningboard>> GetAllAsync();
    Task<Winningboard?> UpdateAsync(Winningboard wb);
    Task<bool> DeleteAsync(Guid id);
}