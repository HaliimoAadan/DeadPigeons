using efscaffold.Entities;

namespace api.Services;

public interface IAdminService
{
    Task<Admin> CreateAsync(Admin admin);
    Task<Admin?> GetByIdAsync(Guid id);
    Task<IEnumerable<Admin>> GetAllAsync();
    Task<Admin?> UpdateAsync(Admin admin);
    Task<bool> DeleteAsync(Guid id);
}