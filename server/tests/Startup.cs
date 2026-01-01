using System.Collections.Generic;
using api;
using api.Etc;
using efscaffold;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;

namespace tests;

public class Startup
{
    private static readonly PostgreSqlContainer DbContainer = new PostgreSqlBuilder().Build();
    private static readonly object InitLock = new();
    private static bool _initialized;
    private static bool _containerStarted;
    public static void ConfigureServices(IServiceCollection services)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        if (!_containerStarted)
        {
            DbContainer.StartAsync().GetAwaiter().GetResult();
            _containerStarted = true;
        }
        var connectionString = DbContainer.GetConnectionString();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        Program.ConfigureServices(services, configuration);
        services.RemoveAll(typeof(MyDbContext));
        services.AddDbContext<MyDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        
        EnsureDatabaseCreated(services, connectionString);
        services.RemoveAll<TimeProvider>();
        var fakeTime = new FakeTimeProvider();
        services.AddSingleton<TimeProvider>(fakeTime);
        services.AddScoped<ISeeder, Seeder>();
    }
    
    private static void EnsureDatabaseCreated(IServiceCollection services, string connectionString)
    {
        if (_initialized)
        {
            return;
        }

        lock (InitLock)
        {
            if (_initialized)
            {
                return;
            }

            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseNpgsql(connectionString)
                .Options;
            using var ctx = new MyDbContext(options);
            ctx.Database.EnsureCreated();
            _initialized = true;
        }
    }
}