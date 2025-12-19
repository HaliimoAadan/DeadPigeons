using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = efscaffold.MyDbContext;
using api.Models;
using api.Services;
using efscaffold.Entities;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WinningBoardController : ControllerBase
{
    private readonly IWinningBoardService _winningBoardService;
    private readonly MyDbContext _dbContext;

    public WinningBoardController(MyDbContext dbContext, IWinningBoardService winningBoardService)
    {
        _dbContext = dbContext;
        _winningBoardService = winningBoardService;
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
    
    [HttpPost("{gameId:guid}/compute-winningboards")]
    public async Task<IActionResult> ComputeWinningBoards(Guid gameId)
    {
        try
        {
            var results = await _winningBoardService.ComputeWinningBoardsAsync(gameId);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
    
    [HttpPost("{boardId:guid}/check")]
    public async Task<IActionResult> CheckBoard(Guid boardId)
    {
        try
        {
            var result = await _winningBoardService.CheckAndCreateWinningBoardAsync(boardId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}
