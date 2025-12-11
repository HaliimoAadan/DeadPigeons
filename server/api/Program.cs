using System.Text.Json.Serialization;
using api.Etc;
using api.Services;
using dataccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Sieve.Models;
using Sieve.Services;
using MyDbContext = Infrastructure.Postgres.Scaffolding.MyDbContext;

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
            opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
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
        });

        services.AddCors();
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISeeder, SieveTestSeeder>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.Configure<SieveOptions>(options =>
        {
            options.CaseSensitive = false;
            options.DefaultPageSize = 10;
            options.MaxPageSize = 100;
        });
        services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
    }

    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddDbContext<MyDbContext>(conf =>
        {
            conf.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        ConfigureServices(builder.Services);

        var app = builder.Build();

        // Admins
        app.MapGet("/admins", async ([FromServices] MyDbContext dbContext) =>
            {
                var objects = await dbContext.Admins.ToListAsync();
                return Results.Ok(objects);
            })
            .WithTags("Admins");

        /*// Players - GET all
        app.MapGet("/players", async ([FromServices] MyDbContext db) =>
            {
                var result = await db.Players
                    .Select(p => new api.Models.PlayerResponseDto
                    {
                        PlayerId = p.PlayerId,
                        FullName = p.FirstName + " " + p.LastName,
                        Email = p.Email,
                        PhoneNumber = p.PhoneNumber,
                        IsActive = p.IsActive
                    })
                    .ToListAsync();

                return Results.Ok(result);
            })
            .WithTags("Players");

        // Create Player
        app.MapPost("/players", async (
            api.Models.PlayerCreateDto dto,
            MyDbContext db
        ) =>
        {
            var player = new efscaffold.Entities.Player
            {
                PlayerId = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsActive = dto.IsActive,
                PasswordHash = string.Empty // <--- IMPORTANT
            };

            db.Players.Add(player);
            await db.SaveChangesAsync();

            return Results.Ok(new api.Models.PlayerResponseDto
            {
                PlayerId = player.PlayerId,
                FullName = $"{player.FirstName} {player.LastName}",
                Email = player.Email,
                PhoneNumber = player.PhoneNumber,
                IsActive = player.IsActive
            });
        });


        // Update Player Active Status
        app.MapPatch("/players/{id:guid}/status", async (
                Guid id,
                bool isActive,
                MyDbContext db
            ) =>
            {
                var player = await db.Players.FindAsync(id);
                if (player == null)
                    return Results.NotFound();

                player.IsActive = isActive;
                await db.SaveChangesAsync();

                return Results.Ok(new { message = "Status updated", isActive });
            })
            .WithTags("Players");*/

        // Games
        app.MapGet("/games", async ([FromServices] MyDbContext dbContext) =>
            {
                var objects = await dbContext.Games.ToListAsync();
                return Results.Ok(objects);
            })
            .WithTags("Games");

        // Boards
        app.MapGet("/boards", async ([FromServices] MyDbContext dbContext) =>
            {
                var objects = await dbContext.Boards.ToListAsync();
                return Results.Ok(objects);
            })
            .WithTags("Boards");

        // Transactions
        app.MapGet("/transactions", async ([FromServices] MyDbContext dbContext) =>
            {
                var objects = await dbContext.Transactions.ToListAsync();
                return Results.Ok(objects);
            })
            .WithTags("Transactions");

        // Winningboards
        app.MapGet("/winningboards", async ([FromServices] MyDbContext dbContext) =>
            {
                var objects = await dbContext.Winningboards.ToListAsync();
                return Results.Ok(objects);
            })
            .WithTags("Winningboards");

        app.UseExceptionHandler(config => { });
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.MapScalarApiReference(options => options.OpenApiRoutePattern = "/swagger/v1/swagger.json");
        app.UseCors(config => config
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin()
            .SetIsOriginAllowed(_ => true));
        app.MapControllers();
        app.GenerateApiClientsFromOpenApi("/../../client/src/core/generated-client.ts")
            .GetAwaiter()
            .GetResult();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<ISeeder>().Seed().GetAwaiter().GetResult();
        }

        app.Run();
    }
}
