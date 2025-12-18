/*using System;
using System.Linq;
using Bogus;
using Infrastructure.Postgres.Scaffolding;
using efscaffold.Entities;
using System.Threading.Tasks;

namespace api.Etc;

public class SieveTestSeeder : ISeeder
{
    private readonly MyDbContext _ctx;
    private readonly TimeProvider _timeProvider;

    public SieveTestSeeder(MyDbContext ctx, TimeProvider timeProvider)
    {
        _ctx = ctx;
        _timeProvider = timeProvider;
    }

    public async Task Seed()
    {
        await _ctx.Database.EnsureCreatedAsync();

        // Clear existing data
        _ctx.Winningboards.RemoveRange(_ctx.Winningboards);
        _ctx.Transactions.RemoveRange(_ctx.Transactions);
        _ctx.Boards.RemoveRange(_ctx.Boards);
        _ctx.Games.RemoveRange(_ctx.Games);
        _ctx.Players.RemoveRange(_ctx.Players);
        await _ctx.SaveChangesAsync();

        Randomizer.Seed = new Random(12345);

        // ==================== Players ====================
        var playerFaker = new Faker<Player>()
            .RuleFor(p => p.PlayerId, f => Guid.NewGuid())
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(p => p.PasswordHash, f => f.Random.Hash())
            .RuleFor(p => p.IsActive, f => f.Random.Bool());

        var players = playerFaker.Generate(50);
        _ctx.Players.AddRange(players);
        await _ctx.SaveChangesAsync();
        
        // ==================== Transactions ====================
        var transactionFaker = new Faker<Transaction>()
            .RuleFor(t => t.TransactionId, f => Guid.NewGuid())
            .RuleFor(t => t.PlayerId, f => f.PickRandom(players).PlayerId)
            .RuleFor(t => t.Amount, f => f.Random.Decimal(20, 500))
            .RuleFor(t => t.MobilepayReqId, f => f.Random.AlphaNumeric(10))
            .RuleFor(t => t.Status, f => f.PickRandom("Pending", "Approved"))
            .RuleFor(t => t.Timestamp, f => _timeProvider.GetUtcNow().DateTime);

        var transactions = transactionFaker.Generate(50);
        _ctx.Transactions.AddRange(transactions);
        await _ctx.SaveChangesAsync();

        // ==================== Winningboards ====================
        var winningFaker = new Faker<Winningboard>()
            .RuleFor(w => w.WinningboardId, f => Guid.NewGuid())
            //.RuleFor(w => w.BoardId, f => f.PickRandom(boards).BoardId)
           // .RuleFor(w => w.GameId, f => f.PickRandom(games).GameId)
            .RuleFor(w => w.WinningNumbersMatched, f => f.Random.Int(0, 3))
            .RuleFor(w => w.Timestamp, f => _timeProvider.GetUtcNow().DateTime);

        var winningboards = winningFaker.Generate(30);
        _ctx.Winningboards.AddRange(winningboards);
        await _ctx.SaveChangesAsync();

        _ctx.ChangeTracker.Clear();
    }
}*/