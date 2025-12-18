using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = Infrastructure.Postgres.Scaffolding.MyDbContext;
using api.Models;
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
    public async Task<IActionResult> Create([FromBody] Game game)
    {
        if (game.GameId == Guid.Empty)
            game.GameId = Guid.NewGuid();

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = game.GameId }, game);
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
}