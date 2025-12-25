using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = efscaffold.MyDbContext;
using api.Models;
using api.Models.Requests;
using api.Services;

using efscaffold.Entities;

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
    
    /// <summary>
    /// Purchases a board using wallet rules (derived balance).
    /// This endpoint is the one the frontend should use for players.
    /// </summary>
    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase(
        [FromHeader(Name = "X-Player-Id")] Guid playerId,
        [FromBody] PurchaseBoardRequestDto dto,
        [FromServices] IBoardService boardService,
        CancellationToken ct)
    {
        if (playerId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid X-Player-Id header." });

        if (dto is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var board = await boardService.PurchaseAsync(
                playerId: playerId,
                gameId: dto.GameId,
                chosenNumbers: dto.ChosenNumbers,
                isRepeating: dto.IsRepeating,
                repeatUntilGameId: dto.RepeatUntilGameId,
                ct: ct
            );

            return CreatedAtAction(nameof(Get), new { id = board.BoardId }, board);
        }
        catch (ArgumentException ex)
        {
            // Validation errors (numbers, input, etc.)
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Business rule violations (insufficient balance, invalid state)
            return BadRequest(new { message = ex.Message });
        }
    }


}
