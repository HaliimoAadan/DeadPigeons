using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using api.DTOs.Requests;
using dataccess;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using ValidationException = Bogus.ValidationException;

namespace api.Services;

public class AuthService(MyDbContext ctx, TimeProvider timeProvider, AppOptions appOptions) : IAuthService
{

    private async Task<string> CreateJwt(Libraryuser user)
    {
        return JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .WithSecret(appOptions.JwtSecret)
            .WithUrlEncoder(new JwtBase64UrlEncoder())
            .WithJsonSerializer(new JsonNetSerializer())
            .AddClaim(nameof(Libraryuser.Id), user.Id)
            .Encode();
    }

    public async Task<JwtClaims> VerifyAndDecodeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("No token attached!");

        var json = JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA512Algorithm())
                .WithSecret(appOptions.JwtSecret)
                .WithUrlEncoder(new JwtBase64UrlEncoder())
                .WithJsonSerializer(new JsonNetSerializer())
                .MustVerifySignature()
                .Decode(token)
                   ?? throw new ValidationException("Authentication failed!");

        var jwtClaims = JsonSerializer.Deserialize<JwtClaims>(json, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new ValidationException("Authentication failed!");
        _ = ctx.Libraryusers.FirstOrDefault(u => u.Id == jwtClaims.Id)
            ?? throw new ValidationException("Authentication is valid, but user is not found!");
        return jwtClaims;
    }

    public async Task<JwtResponse> Login(LoginRequestDto dto)
    {
        var user = ctx.Libraryusers.FirstOrDefault(u => u.Email == dto.Email)
                            ?? throw new ValidationException("User is not found!");
        var passwordsMatch = user.Passwordhash == 
                             System.Security.Cryptography.SHA512.HashData(
                                 System.Text.Encoding.UTF8.GetBytes(dto.Password + user.Salt))
                                 .Aggregate("", (current, b) => current + b.ToString("x2"));
        if (!passwordsMatch)
            throw new ValidationException("Password is incorrect!");
        var token = await CreateJwt(user);
        return new JwtResponse(token);
    }

    public async Task<JwtResponse> Register(RegisterRequestDto dto)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), true);

        var isEmailTaken = ctx.Libraryusers.Any(u => u.Email == dto.Email);
        if (isEmailTaken)
            throw new ValidationException("Email is already taken");

        var salt = Guid.NewGuid().ToString();
        var hash = System.Security.Cryptography.SHA512.HashData(
            System.Text.Encoding.UTF8.GetBytes(dto.Password + salt));
        var user = new Libraryuser()
        {
            Email = dto.Email,
            Createdat = timeProvider.GetUtcNow().DateTime.ToUniversalTime(),
            Id = Guid.NewGuid().ToString(),
            Salt = salt,
            Passwordhash = hash.Aggregate("", (current, b) => current + b.ToString("x2")),
            Role = "User"
        };
        ctx.Libraryusers.Add(user);
        await ctx.SaveChangesAsync();
        
        var token = await CreateJwt(user);
        return new JwtResponse(token);
    }
    
}