using efscaffold;
using efscaffold.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class AdminService : IAdminService
{
    private readonly MyDbContext _db;
    public AdminService(MyDbContext db) => _db = db;

    public async Task<Admin> CreateAsync(Admin admin)
    {
        _db.Admins.Add(admin);
        await _db.SaveChangesAsync();
        return admin;
    }

    public async Task<Admin?> GetByIdAsync(Guid id) =>
        await _db.Admins.FirstOrDefaultAsync(a => a.AdminId == id);

    public async Task<IEnumerable<Admin>> GetAllAsync() => await _db.Admins.ToListAsync();

    public async Task<Admin?> UpdateAsync(Admin admin)
    {
        var existing = await _db.Admins.FindAsync(admin.AdminId);
        if (existing == null) return null;
        _db.Entry(existing).CurrentValues.SetValues(admin);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var admin = await _db.Admins.FindAsync(id);
        if (admin == null) return false;
        _db.Admins.Remove(admin);
        await _db.SaveChangesAsync();
        return true;
    }
}