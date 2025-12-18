using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = Infrastructure.Postgres.Scaffolding.MyDbContext;
using api.Models;
using efscaffold.Entities; // Make sure your Board class is here

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardController : ControllerBase
{
    private readonly MyDbContext _db;

    public BoardController(MyDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var boards = await _db.Boards
            .Include(b => b.Player)
            .Include(b => b.Game)
            .Include(b => b.RepeatUntilGame)
            .ToListAsync();
        return Ok(boards);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var board = await _db.Boards
            .Include(b => b.Player)
            .Include(b => b.Game)
            .Include(b => b.RepeatUntilGame)
            .FirstOrDefaultAsync(b => b.BoardId == id);

        if (board == null) return NotFound();
        return Ok(board);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Board board)
    {
        board.BoardId = Guid.NewGuid();
        board.Timestamp = DateTime.UtcNow;

        // Optional: check if Player and Game exist
        var playerExists = await _db.Players.AnyAsync(p => p.PlayerId == board.PlayerId);
        var gameExists = await _db.Games.AnyAsync(g => g.GameId == board.GameId);
        if (!playerExists || !gameExists) return BadRequest("Player or Game not found.");

        _db.Boards.Add(board);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = board.BoardId }, board);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Board updatedBoard)
    {
        var board = await _db.Boards.FindAsync(id);
        if (board == null) return NotFound();

        board.ChosenNumbers = updatedBoard.ChosenNumbers;
        board.Price = updatedBoard.Price;
        board.IsRepeating = updatedBoard.IsRepeating;
        board.RepeatUntilGameId = updatedBoard.RepeatUntilGameId;
        // optional: update PlayerId/GameId if needed

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var board = await _db.Boards.FindAsync(id);
        if (board == null) return NotFound();

        _db.Boards.Remove(board);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
