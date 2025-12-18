using System.Text.Json.Serialization;
using api.Etc;
using api.Helpers;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag;
using Scalar.AspNetCore;
using Sieve.Models;
using Sieve.Services;
using MyDbContext = efscaffold.MyDbContext;

namespace api;

public class Program
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.InjectAppOptions();
        services.AddMyDbContext();
        services.AddControllers().AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            opts.JsonSerializerOptions.MaxDepth = 128;
        });
        
        services.AddOpenApiDocument(config =>
        {
            config.PostProcess = document =>
            {
                document.Info.Title = "Dead Pigeons API";
                document.Info.Version = "1.0.0";
                document.Info.Description = "API for the Dead Pigeons bingo project.";
            };
           // config.AddStringConstants(typeof(SieveConstants));
           
           config.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
           {
               Type = OpenApiSecuritySchemeType.ApiKey,
               Name = "Authorization",
               In = OpenApiSecurityApiKeyLocation.Header,
               Description = "Enter 'Bearer {your JWT token}'"
           });

           config.OperationProcessors.Add(
               new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });

        services.AddCors();
        //services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IWinningBoardService, WinningBoardService>();
        //services.AddScoped<ISeeder, SieveTestSeeder>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.Configure<SieveOptions>(options =>
        {
            options.CaseSensitive = false;
            options.DefaultPageSize = 10;
            options.MaxPageSize = 100;
        });
     //   services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
    }

    public static void Main()
    {
        DotNetEnv.Env.Load();

        var builder = WebApplication.CreateBuilder();
        
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

            builder.Services.AddDbContext<MyDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
            
        ConfigureServices(builder.Services);
        
        var app = builder.Build();
        
        app.UseExceptionHandler(config => { });
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.MapScalarApiReference(options => options.OpenApiRoutePattern = "/swagger/v1/swagger.json");
        app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(x => true));
        app.MapControllers();
        
        // Admins
        app.MapGet("/admins", async ([FromServices] MyDbContext dbContext) =>
        {
            var objects = await dbContext.Admins.ToListAsync();
            return Results.Ok(objects);
        });

        // Players
        app.MapGet("/players", async ([FromServices] MyDbContext dbContext) =>
        {
            var objects = await dbContext.Players.ToListAsync();
            return Results.Ok(objects);
        });

        // Games
        app.MapGet("/games", async ([FromServices] MyDbContext dbContext) =>
        {
            var objects = await dbContext.Games.ToListAsync();
            return Results.Ok(objects);
        });

        // Boards
        app.MapGet("/boards", async ([FromServices] MyDbContext dbContext) =>
        {
            var objects = await dbContext.Boards.ToListAsync();
            return Results.Ok(objects);
        });

        // Transactions
        app.MapGet("/transactions", async ([FromServices] MyDbContext dbContext) =>
        {
            var objects = await dbContext.Transactions.ToListAsync();
            return Results.Ok(objects);
        });

        // Winningboards
        app.MapGet("/winningboards", async ([FromServices] MyDbContext dbContext) =>
        {
            var objects = await dbContext.Winningboards.ToListAsync();
            return Results.Ok(objects);
        });
        
        //app.GenerateApiClientsFromOpenApi("/../../client/src/core/generated-client.ts").GetAwaiter().GetResult();

        if (app.Environment.IsDevelopment())
            using (var scope = app.Services.CreateScope())
            {
               // scope.ServiceProvider.GetRequiredService<ISeeder>().Seed().GetAwaiter().GetResult();
            }

        app.Run();
    }
}