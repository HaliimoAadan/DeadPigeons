using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = Infrastructure.Postgres.Scaffolding.MyDbContext;
using api.Models;
using efscaffold.Entities; // Make sure your Player class is here

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
        var players = await _db.Players.ToListAsync();
        return Ok(players);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();
        return Ok(player);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Player player)
    {
        player.PlayerId = Guid.NewGuid();
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = player.PlayerId }, player);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Player updatedPlayer)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        // Update fields
        player.FirstName = updatedPlayer.FirstName;
        player.LastName = updatedPlayer.LastName;
        player.Email = updatedPlayer.Email;
        player.PhoneNumber = updatedPlayer.PhoneNumber;
        player.PasswordHash = updatedPlayer.PasswordHash;
        player.IsActive = updatedPlayer.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        _db.Players.Remove(player);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}