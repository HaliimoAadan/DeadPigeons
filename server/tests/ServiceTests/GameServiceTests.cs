using System;
using System.Linq;
using System.Threading.Tasks;
using api;
using api.Services;
using efscaffold;
using efscaffold.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests.ServiceTests
{
    public class GameServiceTests
    {
        private readonly IGameService _gameService;
        private readonly MyDbContext _dbContext;

        public GameServiceTests(IServiceProvider provider)
        {
            _gameService = provider.GetRequiredService<IGameService>();
            _dbContext = provider.GetRequiredService<MyDbContext>();
            
            _dbContext.Database.EnsureCreated();
        }
        
        // Happy path tests

        [Fact]
        public async Task CreateAsync_Should_AddGameToDatabase()
        {
            var game = new Game
            {
                GameId = Guid.NewGuid(),
                ExpirationDate = DateTime.UtcNow.AddYears(20)
            };

            var result = await _gameService.CreateAsync(game);

            Assert.NotNull(result);
            var savedGame = await _dbContext.Games.FindAsync(result.GameId);
            Assert.NotNull(savedGame);
            Assert.Equal(game.GameId, savedGame!.GameId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnGame_WhenExists()
        {
            var game = new Game
            {
                GameId = Guid.NewGuid(),
                ExpirationDate = DateTime.UtcNow.AddYears(20)
            };
            _dbContext.Games.Add(game);
            await _dbContext.SaveChangesAsync();

            var result = await _gameService.GetByIdAsync(game.GameId);

            Assert.NotNull(result);
            Assert.Equal(game.GameId, result!.GameId);
        }

        [Fact]
        public async Task GetAllAsync_Should_ReturnAllGames()
        {
            _dbContext.Games.Add(new Game { GameId = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddYears(20) });
            await _dbContext.SaveChangesAsync();

            var result = await _gameService.GetAllAsync();

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task UpdateAsync_Should_UpdateExistingGame()
        {
            var game = new Game
            {
                GameId = Guid.NewGuid(),
                ExpirationDate = DateTime.UtcNow.AddYears(20)
            };
            _dbContext.Games.Add(game);
            await _dbContext.SaveChangesAsync();

            game.ExpirationDate = DateTime.UtcNow.AddYears(10);
            var result = await _gameService.UpdateAsync(game);

            Assert.NotNull(result);
            Assert.Equal(game.ExpirationDate, result!.ExpirationDate);
        }

        [Fact]
        public async Task DeleteAsync_Should_RemoveGame_WhenExists()
        {
            var game = new Game
            {
                GameId = Guid.NewGuid(),
                ExpirationDate = DateTime.UtcNow.AddYears(20)
            };
            _dbContext.Games.Add(game);
            await _dbContext.SaveChangesAsync();

            var deleted = await _gameService.DeleteAsync(game.GameId);

            Assert.True(deleted);
            var exists = await _dbContext.Games.FindAsync(game.GameId);
            Assert.Null(exists);
        }
        
        // Unhappy path tests

        [Fact]
        public async Task GetByIdAsync_Should_ReturnNull_WhenNotFound()
        {
            var result = await _gameService.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_Should_ReturnNull_WhenGameDoesNotExist()
        {
            var game = new Game { GameId = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddYears(20) };
            var result = await _gameService.UpdateAsync(game);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_Should_ReturnFalse_WhenGameDoesNotExist()
        {
            var result = await _gameService.DeleteAsync(Guid.NewGuid());
            Assert.False(result);
        }

    }
}
