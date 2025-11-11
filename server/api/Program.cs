using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using api.DTOs.Requests.SieveProcessors;
using api.Etc;
using api.Services;
using dataccess;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Sieve.Models;
using Sieve.Services;
using Testcontainers.PostgreSql;

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
        });
        services.AddOpenApiDocument(config =>
        {
            config.AddStringConstants(typeof(SieveConstants));
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
        
        ConfigureServices(builder.Services);
        var app = builder.Build();
        app.UseExceptionHandler(config => { });
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.MapScalarApiReference(
            options => options.OpenApiRoutePattern = "/swagger/v1/swagger.json"
            );
        app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(x => true));
        app.MapControllers();
        app.GenerateApiClientsFromOpenApi("/../../client/src/core/generated-client.ts").GetAwaiter().GetResult();
        if (app.Environment.IsDevelopment())
            using (var scope = app.Services.CreateScope())
                scope.ServiceProvider.GetRequiredService<ISeeder>().Seed().GetAwaiter().GetResult();
        app.Run();
    }
}