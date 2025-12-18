using api.Helpers;
using api.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = efscaffold.MyDbContext;
using efscaffold.Entities;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly MyDbContext _db;

    public PlayerController(MyDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var players = await _db.Players
            .Select(p => new PlayerResponseDto
            {
                PlayerId = p.PlayerId,
                Email = p.Email,
                FirstName = p.FirstName,
                LastName = p.LastName,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(players);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        // Token validation
        var token = Request.Headers["X-Auth-Token"].FirstOrDefault();
        if (token == null) return Unauthorized("No token");
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret)) return StatusCode(500, "JWT_SECRET missing");
        var playerId = JwtValidator.ValidateToken(token, secret);
        if (playerId == null || playerId != id.ToString()) return Unauthorized("Invalid token");

        var player = await _db.Players
            .Where(p => p.PlayerId == id)
            .Select(p => new PlayerResponseDto
            {
                PlayerId = p.PlayerId,
                Email = p.Email,
                FirstName = p.FirstName,
                LastName = p.LastName,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();

        if (player == null)
            return NotFound();

        return Ok(player);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Player updatedPlayer)
    {
        // Token validation
        var token = Request.Headers["X-Auth-Token"].FirstOrDefault();
        if (token == null) return Unauthorized("No token");
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret)) return StatusCode(500, "JWT_SECRET missing");
        var playerId = JwtValidator.ValidateToken(token, secret);
        if (playerId == null || playerId != id.ToString()) return Unauthorized("Invalid token");

        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        player.FirstName = updatedPlayer.FirstName;
        player.LastName = updatedPlayer.LastName;
        player.Email = updatedPlayer.Email;
        player.PhoneNumber = updatedPlayer.PhoneNumber;
        player.PasswordHash = updatedPlayer.PasswordHash;
        player.IsActive = updatedPlayer.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // Token validation
        var token = Request.Headers["X-Auth-Token"].FirstOrDefault();
        if (token == null) return Unauthorized("No token");
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret)) return StatusCode(500, "JWT_SECRET missing");
        var playerId = JwtValidator.ValidateToken(token, secret);
        if (playerId == null || playerId != id.ToString()) return Unauthorized("Invalid token");

        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        _db.Players.Remove(player);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
