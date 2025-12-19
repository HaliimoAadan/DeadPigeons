using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using api.Etc;
using api.Models;
using api.Models.Requests;
using efscaffold;
using efscaffold.Entities;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using ValidationException = Bogus.ValidationException;

namespace api.Services;

public class AuthService( 
       MyDbContext ctx,
       ILogger<AuthService> logger,
       TimeProvider timeProvider,
       AppOptions appOptions) : IAuthService
   {
       public async Task<JwtClaims> VerifyAndDecodeToken(string? token)
       {
           if (string.IsNullOrWhiteSpace(token))
               throw new ValidationException("No token attached!");
   
           var builder = CreateJwtBuilder();
   
           string jsonString;
   
           try
           {
               jsonString = builder.Decode(token)
                            ?? throw new ValidationException("Authentication failed!");
           }
           catch (Exception e)
           {
               logger.LogError(e, "JWT validation failed");
               throw new ValidationException("Failed to verify JWT");
           }
   
           var jwtClaims = JsonSerializer.Deserialize<JwtClaims>(jsonString,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new ValidationException("Authentication failed!");
   
           bool exists = jwtClaims.Role switch
           {
               "Admin" => ctx.Admins.Any(a => a.AdminId == jwtClaims.Id),
               "Player" => ctx.Players.Any(p => p.PlayerId == jwtClaims.Id),
               _ => false
           };
   
           if (!exists)
               throw new ValidationException("User does not exist!");
   
           return jwtClaims;
       }
   
       public async Task<JwtResponse> Login(LoginRequestDto dto)
       {
           // First try Admin
           var admin = ctx.Admins.FirstOrDefault(a => a.Email == dto.Email);
           if (admin != null)
           {
               if (!PasswordHasher.Verify(dto.Password, admin.PasswordHash))
                   throw new ValidationException("Invalid password");
   
               return new JwtResponse(CreateJwt(admin.AdminId, admin.Email, "Admin"));
           }
   
           // Then try Player
           var player = ctx.Players.FirstOrDefault(p => p.Email == dto.Email);
           if (player == null)
               throw new ValidationException("User not found");
   
           if (!PasswordHasher.Verify(dto.Password, player.PasswordHash))
               throw new ValidationException("Invalid password");
   
           return new JwtResponse(CreateJwt(player.PlayerId, player.Email, "Player"));
       }
   
       public async Task<JwtResponse> RegisterPlayer(RegisterRequestDto dto)
       {
           Validator.ValidateObject(dto, new ValidationContext(dto), true);
   
           if (ctx.Players.Any(p => p.Email == dto.Email))
               throw new ValidationException("Email already taken");
   
           var player = new Player
           {
               PlayerId = Guid.NewGuid(),
               FirstName = dto.FirstName,
               LastName = dto.LastName,
               Email = dto.Email,
               PhoneNumber = dto.PhoneNumber,
               PasswordHash = PasswordHasher.Hash(dto.Password),
               IsActive = true
           };
   
           ctx.Players.Add(player);
           await ctx.SaveChangesAsync();
   
           return new JwtResponse(CreateJwt(player.PlayerId, player.Email, "Player"));
       }
   
       public async Task<JwtResponse> RegisterAdmin(Admin admin)
       {
           if (ctx.Admins.Any(a => a.Email == admin.Email))
               throw new ValidationException("Email already taken");
   
           admin.AdminId = Guid.NewGuid();
           admin.PasswordHash = PasswordHasher.Hash(admin.PasswordHash);
   
           ctx.Admins.Add(admin);
           await ctx.SaveChangesAsync();
   
           return new JwtResponse(CreateJwt(admin.AdminId, admin.Email, "Admin"));
       }
   
       private JwtBuilder CreateJwtBuilder() =>
           JwtBuilder.Create()
               .WithAlgorithm(new HMACSHA512Algorithm())
               .WithSecret(appOptions.JwtSecret)
               .WithUrlEncoder(new JwtBase64UrlEncoder())
               .WithJsonSerializer(new JsonNetSerializer())
               .MustVerifySignature();
   
       private string CreateJwt(Guid id, string email, string role) =>
           CreateJwtBuilder()
               .AddClaim("Id", id)
               .AddClaim("Email", email)
               .AddClaim("Role", role)
               .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(6).ToUnixTimeSeconds())
               .Encode();
}