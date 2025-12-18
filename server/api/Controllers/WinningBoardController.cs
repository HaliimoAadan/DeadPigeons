using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = Infrastructure.Postgres.Scaffolding.MyDbContext;
using api.Models;
using efscaffold.Entities;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WinningBoardController : ControllerBase
{
    private readonly MyDbContext _dbContext;

    public WinningBoardController(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var boards = await _dbContext.Winningboards.ToListAsync();
        return Ok(boards);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var board = await _dbContext.Winningboards.FindAsync(id);
        if (board == null) return NotFound();
        return Ok(board);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Winningboard board)
    {
        if (board.WinningboardId == Guid.Empty)
            board.WinningboardId = Guid.NewGuid();

        _dbContext.Winningboards.Add(board);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = board.WinningboardId }, board);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Winningboard updatedBoard)
    {
        var board = await _dbContext.Winningboards.FindAsync(id);
        if (board == null) return NotFound();

        board.GameId = updatedBoard.GameId;
        board.BoardId = updatedBoard.BoardId;
        board.WinningNumbersMatched = updatedBoard.WinningNumbersMatched;
        board.Timestamp = updatedBoard.Timestamp;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var board = await _dbContext.Winningboards.FindAsync(id);
        if (board == null) return NotFound();

        _dbContext.Winningboards.Remove(board);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
