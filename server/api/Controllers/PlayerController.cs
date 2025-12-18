using api.Helpers;
using api.Models;
//using api.Models.Response;
using efscaffold;
using efscaffold.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[ApiController]
[Route("api/player")] // IMPORTANT: fara [controller]
public class PlayerController : ControllerBase
{
    private readonly MyDbContext _db;

    public PlayerController(MyDbContext db)
    {
        _db = db;
    }

    // -------------------------
    // Helper: token validation
    // -------------------------
    private IActionResult? ValidateToken(Guid targetPlayerId)
    {
        var token = Request.Headers["X-Auth-Token"].FirstOrDefault();
        if (token == null) return Unauthorized("No token");

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret))
            return StatusCode(500, "JWT_SECRET missing");

        var playerId = JwtValidator.ValidateToken(token, secret);
        if (playerId == null || playerId != targetPlayerId.ToString())
            return Unauthorized("Invalid token");

        return null;
    }

    // -------------------------
    // GET /api/player?onlyActive=true
    // -------------------------
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerResponseDto>>> GetAll(
        [FromQuery] bool? onlyActive)
    {
        var query = _db.Players.AsQueryable();

        if (onlyActive == true)
            query = query.Where(p => p.IsActive);

        var players = await query
            .Select(p => new PlayerResponseDto
            {
                PlayerId = p.PlayerId,
                FullName = ((p.FirstName ?? "") + " " + (p.LastName ?? "")).Trim(),
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(players); // ARRAY -> map() ok
    }

    // -------------------------
    // GET /api/player/{id}
    // -------------------------
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var auth = ValidateToken(id);
        if (auth != null) return auth;

        var player = await _db.Players
            .Where(p => p.PlayerId == id)
            .Select(p => new PlayerResponseDto
            {
                PlayerId = p.PlayerId,
                FullName = ((p.FirstName ?? "") + " " + (p.LastName ?? "")).Trim(),
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();

        return player == null ? NotFound() : Ok(player);
    }
    

    // -------------------------
    // PUT /api/player/{id}
    // -------------------------
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] Player updatedPlayer)
    {
        var auth = ValidateToken(id);
        if (auth != null) return auth;

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

    // -------------------------
    // DELETE /api/player/{id}
    // -------------------------
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var auth = ValidateToken(id);
        if (auth != null) return auth;

        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        _db.Players.Remove(player);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // -------------------------
    // PATCH /api/player/{id}/status
    // -------------------------
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromQuery] bool isActive)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        player.IsActive = isActive;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
