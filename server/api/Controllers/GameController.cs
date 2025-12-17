using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = efscaffold.MyDbContext;
using api.Models;
using api.Models.Requests;
using efscaffold.Entities;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly MyDbContext _dbContext;

    public GameController(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var games = await _dbContext.Games.ToListAsync();
        return Ok(games);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var game = await _dbContext.Games.FindAsync(id);
        if (game == null) return NotFound();
        return Ok(game);
    }

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGameDto dto)
    {
        var game = new Game
        {
            ExpirationDate = dto?.ExpirationDate ?? DateTime.UtcNow.AddYears(20),
        };

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = game.GameId }, new
        {
            game.GameId,
            game.ExpirationDate
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Game updatedGame)
    {
        var game = await _dbContext.Games.FindAsync(id);
        if (game == null) return NotFound();

        game.WinningNumbers = updatedGame.WinningNumbers;
        game.DrawDate = updatedGame.DrawDate;
        game.ExpirationDate = updatedGame.ExpirationDate;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var game = await _dbContext.Games.FindAsync(id);
        if (game == null) return NotFound();

        _dbContext.Games.Remove(game);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpPost("{gameId:guid}/publish-winning-numbers")]
    public async Task<IActionResult> PublishWinningNumbers(Guid gameId, [FromBody] WinningNumbersDto dto)
    {
        // Admin token validation
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized("No token provided");

        var token = authHeader.Substring("Bearer ".Length).Trim();
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret)) return StatusCode(500, "JWT_SECRET missing");

        var adminId = JwtValidator.ValidateToken(token, secret);
        if (adminId == null) return Unauthorized("Invalid token");

        // Ensure exactly 3 numbers are selected
        if (dto.Numbers.Distinct().Count() != 3 || dto.Numbers.Any(n => n < 1 || n > 16))
            return BadRequest("You must select exactly 3 distinct numbers between 1 and 16.");

        var game = await _dbContext.Games.FindAsync(gameId);
        if (game == null) return NotFound("Game not found");

        if (game.DrawDate != null)
            return BadRequest("Winning numbers already published for this game.");

        // Save the winning numbers (sorted)
        game.WinningNumbers = dto.Numbers.OrderBy(n => n).ToList();
        game.DrawDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(new { game.GameId, game.WinningNumbers });
    }
}