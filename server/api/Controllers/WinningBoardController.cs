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
        var winners = await _dbContext.Winningboards
            .Include(w => w.Board)
            .ThenInclude(b => b.Player)
            .Include(w => w.Game)
            .ToListAsync();

        return Ok(winners);
    }

    [HttpGet("by-game/{gameId:guid}")]
    public async Task<IActionResult> GetByGame(Guid gameId)
    {
        var winners = await _dbContext.Winningboards
            .Where(w => w.GameId == gameId && w.WinningNumbersMatched == 3)
            .Include(w => w.Board)
            .ThenInclude(b => b.Player)
            .ToListAsync();

        // return a clean DTO so the frontend doesn’t deal with circular graphs
        var dto = winners.Select(w => new {
            w.WinningboardId,
            w.GameId,
            w.BoardId,
            w.WinningNumbersMatched,
            w.Timestamp,
            Player = new {
                w.Board.Player.PlayerId,
                w.Board.Player.Email,
                w.Board.Player.FirstName,
                w.Board.Player.LastName
            }
        });

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Winningboard board)
    {
        if (board.WinningboardId == Guid.Empty)
            board.WinningboardId = Guid.NewGuid();

        _dbContext.Winningboards.Add(board);
        await _dbContext.SaveChangesAsync();
        return Ok (board);
        
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
