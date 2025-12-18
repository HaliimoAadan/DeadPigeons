/*using efscaffold.Entities;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace api.Etc;

/// <summary>
/// Custom Sieve processor for Dead Pigeons game entities.
/// All filterable and sortable properties use SieveConstants to avoid magic strings.
/// </summary>
public class ApplicationSieveProcessor : SieveProcessor
{
    public ApplicationSieveProcessor(IOptions<SieveOptions> options) : base(options) { }

    protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
    {
        // ================= PLAYER PROPERTIES =================
        mapper.Property<Player>(p => p.PlayerId)
            .CanFilter().CanSort().HasName(SieveConstants.PlayerId);
        mapper.Property<Player>(p => p.FirstName)
            .CanFilter().CanSort().HasName(SieveConstants.PlayerFirstName);
        mapper.Property<Player>(p => p.LastName)
            .CanFilter().CanSort().HasName(SieveConstants.PlayerLastName);
        mapper.Property<Player>(p => p.Email)
            .CanFilter().CanSort().HasName(SieveConstants.PlayerEmail);
        mapper.Property<Player>(p => p.PhoneNumber)
            .CanFilter().CanSort().HasName(SieveConstants.PlayerPhoneNumber);
        mapper.Property<Player>(p => p.IsActive)
            .CanFilter().CanSort().HasName(SieveConstants.PlayerIsActive);

        // ================= BOARD PROPERTIES =================
        mapper.Property<Board>(b => b.BoardId)
            .CanFilter().CanSort().HasName(SieveConstants.BoardId);
        mapper.Property<Board>(b => b.PlayerId)
            .CanFilter().CanSort().HasName(SieveConstants.BoardPlayerId);
        mapper.Property<Board>(b => b.GameId)
            .CanFilter().CanSort().HasName(SieveConstants.BoardGameId);
        mapper.Property<Board>(b => b.ChosenNumbers)
            .CanFilter().HasName(SieveConstants.BoardChosenNumbers);
        mapper.Property<Board>(b => b.Price)
            .CanFilter().CanSort().HasName(SieveConstants.BoardPrice);
        mapper.Property<Board>(b => b.IsRepeating)
            .CanFilter().CanSort().HasName(SieveConstants.BoardIsRepeating);
        mapper.Property<Board>(b => b.RepeatUntilGameId)
            .CanFilter().CanSort().HasName(SieveConstants.BoardRepeatUntilGameId);
        mapper.Property<Board>(b => b.Timestamp)
            .CanFilter().CanSort().HasName(SieveConstants.BoardTimestamp);

        // ================= GAME PROPERTIES =================
        mapper.Property<Game>(g => g.GameId)
            .CanFilter().CanSort().HasName(SieveConstants.GameId);
        mapper.Property<Game>(g => g.DrawDate)
            .CanFilter().CanSort().HasName(SieveConstants.GameDrawDate);
        mapper.Property<Game>(g => g.ExpirationDate)
            .CanFilter().CanSort().HasName(SieveConstants.GameExpirationDate);
        mapper.Property<Game>(g => g.WinningNumbers)
            .CanFilter().HasName(SieveConstants.GameWinningNumbers);

        // ================= TRANSACTION PROPERTIES =================
        mapper.Property<Transaction>(t => t.TransactionId)
            .CanFilter().CanSort().HasName(SieveConstants.TransactionId);
        mapper.Property<Transaction>(t => t.PlayerId)
            .CanFilter().CanSort().HasName(SieveConstants.TransactionPlayerId);
        mapper.Property<Transaction>(t => t.Amount)
            .CanFilter().CanSort().HasName(SieveConstants.TransactionAmount);
        mapper.Property<Transaction>(t => t.MobilepayReqId)
            .CanFilter().CanSort().HasName(SieveConstants.TransactionMobilepayReqId);
        mapper.Property<Transaction>(t => t.Status)
            .CanFilter().CanSort().HasName(SieveConstants.TransactionStatus);
        mapper.Property<Transaction>(t => t.Timestamp)
            .CanFilter().CanSort().HasName(SieveConstants.TransactionTimestamp);

        // ================= WINNINGBOARD PROPERTIES =================
        mapper.Property<Winningboard>(w => w.WinningboardId)
            .CanFilter().CanSort().HasName(SieveConstants.WinningboardId);
        mapper.Property<Winningboard>(w => w.BoardId)
            .CanFilter().CanSort().HasName(SieveConstants.WinningboardBoardId);
        mapper.Property<Winningboard>(w => w.GameId)
            .CanFilter().CanSort().HasName(SieveConstants.WinningboardGameId);
        mapper.Property<Winningboard>(w => w.WinningNumbersMatched)
            .CanFilter().CanSort().HasName(SieveConstants.WinningboardNumbersMatched);
        mapper.Property<Winningboard>(w => w.Timestamp)
            .CanFilter().CanSort().HasName(SieveConstants.WinningboardTimestamp);

        return mapper;
    }
}
*/