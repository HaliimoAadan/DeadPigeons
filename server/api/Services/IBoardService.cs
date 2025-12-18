using efscaffold.Entities;

namespace api;

public interface IBoardService
{
    Task<Board> CreateAsync(Board board);
    Task<Board?> GetByIdAsync(Guid id);
    Task<IEnumerable<Board>> GetAllAsync();
    Task<Board?> UpdateAsync(Board board);
    Task<bool> DeleteAsync(Guid id);
}