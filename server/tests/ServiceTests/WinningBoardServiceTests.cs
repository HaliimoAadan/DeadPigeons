using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Services;
using efscaffold;
using efscaffold.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace tests.ServiceTests;

public class WinningBoardServiceTests
{
    private readonly MyDbContext _dbContext;
    private readonly WinningBoardService _service;
    private readonly Game _game;
    private readonly Board _board;

    public WinningBoardServiceTests()
    {
        // Setup InMemory database
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new MyDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Seed required Game and Board
        _game = new Game
        {
            GameId = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.AddDays(1)
        };
        _board = new Board
        {
            BoardId = Guid.NewGuid(),
            GameId = _game.GameId,
            PlayerId = Guid.NewGuid(),
            ChosenNumbers = new List<int> { 1, 2, 3, 4, 5 },
            Price = 10,
            Timestamp = DateTime.UtcNow
        };
        _dbContext.Games.Add(_game);
        _dbContext.Boards.Add(_board);
        _dbContext.SaveChanges();

        _service = new WinningBoardService(_dbContext);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddWinningBoard()
    {
        var wb = new Winningboard
        {
            WinningboardId = Guid.NewGuid(),
            GameId = _game.GameId,
            BoardId = _board.BoardId,
            WinningNumbersMatched = 3,
            Timestamp = DateTime.UtcNow
        };

        var result = await _service.CreateAsync(wb);

        Assert.NotNull(result);
        Assert.Equal(wb.WinningboardId, result.WinningboardId);
        Assert.Single(_dbContext.Winningboards);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllWinningBoards()
    {
        var wb1 = await _service.CreateAsync(new Winningboard
        {
            WinningboardId = Guid.NewGuid(),
            GameId = _game.GameId,
            BoardId = _board.BoardId,
            WinningNumbersMatched = 3,
            Timestamp = DateTime.UtcNow
        });
        var wb2 = await _service.CreateAsync(new Winningboard
        {
            WinningboardId = Guid.NewGuid(),
            GameId = _game.GameId,
            BoardId = _board.BoardId,
            WinningNumbersMatched = 2,
            Timestamp = DateTime.UtcNow
        });

        var results = await _service.GetAllAsync();

        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnWinningBoard_WhenExists()
    {
        var wb = await _service.CreateAsync(new Winningboard
        {
            WinningboardId = Guid.NewGuid(),
            GameId = _game.GameId,
            BoardId = _board.BoardId,
            WinningNumbersMatched = 3,
            Timestamp = DateTime.UtcNow
        });

        var result = await _service.GetByIdAsync(wb.WinningboardId);

        Assert.NotNull(result);
        Assert.Equal(wb.WinningboardId, result!.WinningboardId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingWinningBoard()
    {
        var wb = await _service.CreateAsync(new Winningboard
        {
            WinningboardId = Guid.NewGuid(),
            GameId = _game.GameId,
            BoardId = _board.BoardId,
            WinningNumbersMatched = 2,
            Timestamp = DateTime.UtcNow
        });

        wb.WinningNumbersMatched = 3;
        var updated = await _service.UpdateAsync(wb);

        Assert.NotNull(updated);
        Assert.Equal(3, updated!.WinningNumbersMatched);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveWinningBoard()
    {
        var wb = await _service.CreateAsync(new Winningboard
        {
            WinningboardId = Guid.NewGuid(),
            GameId = _game.GameId,
            BoardId = _board.BoardId,
            WinningNumbersMatched = 2,
            Timestamp = DateTime.UtcNow
        });

        var deleted = await _service.DeleteAsync(wb.WinningboardId);

        Assert.True(deleted);
        var result = await _service.GetByIdAsync(wb.WinningboardId);
        Assert.Null(result);
    }
}
