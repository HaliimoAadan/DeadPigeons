using efscaffold.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class TransactionService : ITransactionService
{
    private readonly MyDbContext _db;
    public TransactionService(MyDbContext db) => _db = db;

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id) =>
        await _db.Transactions
            .Include(t => t.Player)
            .FirstOrDefaultAsync(t => t.TransactionId == id);

    public async Task<IEnumerable<Transaction>> GetAllAsync() => await _db.Transactions.ToListAsync();

    public async Task<Transaction?> UpdateAsync(Transaction transaction)
    {
        var existing = await _db.Transactions.FindAsync(transaction.TransactionId);
        if (existing == null) return null;
        _db.Entry(existing).CurrentValues.SetValues(transaction);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var transaction = await _db.Transactions.FindAsync(id);
        if (transaction == null) return false;
        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();
        return true;
    }
}