/*using api.Models;
using efscaffold;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly MyDbContext _db;

    public PlayersController(MyDbContext db)
    {
        _db = db;
    }

    // GET api/players?onlyActive=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerResponseDto>>> GetAll([FromQuery] bool? onlyActive)
    {
        var query = _db.Players.AsQueryable();

        if (onlyActive == true)
            query = query.Where(p => p.IsActive);

        var result = await query
            .Select(p => new PlayerResponseDto
            {
                PlayerId = p.PlayerId,
                FullName = ((p.FirstName ?? "") + " " + (p.LastName ?? "")).Trim(),
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(result);
    }

    // POST api/players
    [HttpPost]
    public async Task<ActionResult<PlayerResponseDto>> Create([FromBody] PlayerCreateDto dto)
    {
        var fullName = (dto.FullName ?? "").Trim();
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return BadRequest("FullName is required.");

        var firstName = parts[0];
        var lastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";

        var player = new efscaffold.Entities.Player
        {
            PlayerId = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = dto.Email.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            IsActive = dto.IsActive ?? false, // default inactive (safe)
            PasswordHash = string.Empty
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        return Ok(new PlayerResponseDto
        {
            PlayerId = player.PlayerId,
            FullName = ((player.FirstName ?? "") + " " + (player.LastName ?? "")).Trim(),
            Email = player.Email,
            PhoneNumber = player.PhoneNumber,
            IsActive = player.IsActive
        });
    }

    // PATCH api/players/{id}/status?isActive=false
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] bool isActive)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        player.IsActive = isActive;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
*/