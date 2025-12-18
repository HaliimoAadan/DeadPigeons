using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = efscaffold.MyDbContext;
using api.Models.Requests;
using api.Models.Response;
using efscaffold.Entities;
using PasswordHasher = api.Etc.PasswordHasher;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MyDbContext _db;
    
    public AuthController(MyDbContext db)
    {
        _db = db;
    }

    [HttpPost("login/admin")]
    public async Task<IActionResult> AdminLogin([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Email == dto.Email);
        if (admin == null)
            return Unauthorized("Invalid credentials");

        if (string.IsNullOrWhiteSpace(admin.PasswordHash))
            return StatusCode(500, "Password not set for admin");

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret))
            return StatusCode(500, "JWT_SECRET missing");

        if (!PasswordHasher.Verify(dto.Password, admin.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = JwtHelper.GenerateToken(admin.AdminId.ToString(), secret);
        return Ok(new JwtResponseDto(token));
    }

    [HttpPost("login/player")]
    public async Task<IActionResult> PlayerLogin([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var player = await _db.Players.FirstOrDefaultAsync(p => p.Email == dto.Email);
        if (player == null)
            return Unauthorized("Invalid credentials");

        if (string.IsNullOrWhiteSpace(player.PasswordHash))
            return StatusCode(500, "Password not set for player");

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret))
            return StatusCode(500, "JWT_SECRET missing");

        if (!PasswordHasher.Verify(dto.Password, player.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = JwtHelper.GenerateToken(player.PlayerId.ToString(), secret);
        return Ok(new JwtResponseDto(token));
    }

}
