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
    public static void ConfigureServices(IServiceCollection services)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var postgreSqlContainer = new PostgreSqlBuilder().Build();
        postgreSqlContainer.StartAsync().GetAwaiter().GetResult();
        var connectionString = postgreSqlContainer.GetConnectionString();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        Program.ConfigureServices(services, configuration);
        services.RemoveAll(typeof(MyDbContext));
        services.AddScoped<MyDbContext>(factory =>
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            var ctx = new MyDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        });
        services.RemoveAll<TimeProvider>();
        var fakeTime = new FakeTimeProvider();
        services.AddSingleton<TimeProvider>(fakeTime);
        services.AddScoped<ISeeder, Seeder>();
    }
}