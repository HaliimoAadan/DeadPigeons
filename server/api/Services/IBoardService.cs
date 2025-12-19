using efscaffold.Entities;

namespace api.Services;

public interface IBoardService
{
    Task<Board> CreateAsync(Board board);
    Task<Board?> GetByIdAsync(Guid id);
    Task<IEnumerable<Board>> GetAllAsync();
    Task<Board?> UpdateAsync(Board board);
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Purchases a board using wallet rules (derived balance).
    /// </summary>
    Task<Board> PurchaseAsync(
        Guid playerId,
        Guid gameId,
        List<int> chosenNumbers,
        bool isRepeating,
        Guid? repeatUntilGameId,
        CancellationToken ct = default);
}