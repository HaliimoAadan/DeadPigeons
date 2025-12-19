using api.Services;
using efscaffold;
using efscaffold.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace tests.ServiceTests;

public class AdminServiceTests
{
    private readonly IAdminService _service;
    private readonly MyDbContext _db;

    public AdminServiceTests(IServiceProvider provider)
    {
        _db = provider.GetRequiredService<MyDbContext>();
        _service = new AdminService(_db);

        _db.Database.EnsureCreated();
    }

    // Happy path tests
    private Admin NewAdmin()
    {
        return new Admin
        {
            AdminId = Guid.NewGuid(),
            Email = $"admin-{Guid.NewGuid()}@test.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Admin"
        };
    }

    [Fact]
    public async Task CreateAsync_Should_Save_Admin()
    {
        var admin = NewAdmin();

        var created = await _service.CreateAsync(admin);

        var inDb = await _db.Admins.FindAsync(created.AdminId);

        Assert.NotNull(inDb);
        Assert.Equal(admin.Email, inDb!.Email);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Admin_When_Exists()
    {
        var admin = NewAdmin();
        await _service.CreateAsync(admin);

        var result = await _service.GetByIdAsync(admin.AdminId);

        Assert.NotNull(result);
        Assert.Equal(admin.Email, result!.Email);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Admin()
    {
        var admin = NewAdmin();
        await _service.CreateAsync(admin);

        admin.Email = $"updated-{Guid.NewGuid()}@test.com";

        var updated = await _service.UpdateAsync(admin);

        Assert.NotNull(updated);
        Assert.Equal(admin.Email, updated!.Email);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_Null_When_Not_Exists()
    {
        var admin = NewAdmin();
        admin.AdminId = Guid.NewGuid(); 

        var result = await _service.UpdateAsync(admin);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Admin()
    {
        var admin = NewAdmin();
        await _service.CreateAsync(admin);

        var result = await _service.DeleteAsync(admin.AdminId);

        Assert.True(result);
        Assert.Null(await _service.GetByIdAsync(admin.AdminId));
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_False_When_Not_Exists()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid());
        Assert.False(result);
    }
}