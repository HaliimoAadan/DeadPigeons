using api;
using api.Services;
using efscaffold;
using efscaffold.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.DependencyInjection;

namespace tests.ServiceTests;

public class BoardServiceTests
{
    private readonly IBoardService _service;
    private readonly MyDbContext _db;

    public BoardServiceTests(IServiceProvider provider)
    {
        _db = provider.GetRequiredService<MyDbContext>();
        _service = new BoardService(_db);

        _db.Database.EnsureCreated();
    }

    private async Task<(Player player, Game game)> SeedPlayerAndGame()
    {
        var player = new Player
        {
            PlayerId = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Player",
            Email = Guid.NewGuid() + "@test.com",
            PhoneNumber = "12345678",
            PasswordHash = "hashed",
            IsActive = true
        };

        var game = new Game
        {
            GameId = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.AddYears(20)
        };

        _db.Players.Add(player);
        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        return (player, game);
    }

    [Fact]
    public async Task CreateAsync_HappyPath_CreatesBoard()
    {
        // Arrange
        var (player, game) = await SeedPlayerAndGame();

        var board = new Board
        {
            BoardId = Guid.NewGuid(),
            PlayerId = player.PlayerId,
            GameId = game.GameId,
            ChosenNumbers = new List<int> { 1, 2, 3, 4, 5 },
            Price = 20,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _service.CreateAsync(board);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(board.BoardId, result.BoardId);
        Assert.Equal(5, result.ChosenNumbers.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsBoard()
    {
        var (player, game) = await SeedPlayerAndGame();

        var board = new Board
        {
            BoardId = Guid.NewGuid(),
            PlayerId = player.PlayerId,
            GameId = game.GameId,
            ChosenNumbers = new List<int> { 1, 2, 3, 4, 5 },
            Price = 20,
            Timestamp = DateTime.UtcNow
        };

        await _service.CreateAsync(board);

        var result = await _service.GetByIdAsync(board.BoardId);

        Assert.NotNull(result);
        Assert.Equal(board.BoardId, result!.BoardId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesBoard()
    {
        var (player, game) = await SeedPlayerAndGame();

        var board = new Board
        {
            BoardId = Guid.NewGuid(),
            PlayerId = player.PlayerId,
            GameId = game.GameId,
            ChosenNumbers = new List<int> { 1, 2, 3, 4, 5 },
            Price = 20,
            Timestamp = DateTime.UtcNow
        };

        await _service.CreateAsync(board);

        board.Price = 40;

        var updated = await _service.UpdateAsync(board);

        Assert.NotNull(updated);
        Assert.Equal(40, updated!.Price);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ReturnsTrue()
    {
        var (player, game) = await SeedPlayerAndGame();

        var board = new Board
        {
            BoardId = Guid.NewGuid(),
            PlayerId = player.PlayerId,
            GameId = game.GameId,
            ChosenNumbers = new List<int> { 1, 2, 3, 4, 5 },
            Price = 20,
            Timestamp = DateTime.UtcNow
        };

        await _service.CreateAsync(board);

        var deleted = await _service.DeleteAsync(board.BoardId);

        Assert.True(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid());
        Assert.False(result);
    }
}
