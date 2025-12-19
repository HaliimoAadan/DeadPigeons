using efscaffold;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace tests;

public class TestStartup
{
    private static readonly PostgreSqlContainer _dbContainer =
        new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public void ConfigureServices(IServiceCollection services)
    {
        _dbContainer.StartAsync().GetAwaiter().GetResult();

        services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(_dbContainer.GetConnectionString()));
    }
}