/*using api.Models;
using efscaffold;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using efscaffold.Entities;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TransactionsController : ControllerBase
{
    private readonly MyDbContext _db;

    public TransactionsController(MyDbContext db)
    {
        _db = db;
    }

    // =========================
    // PLAYER SIDE
    // =========================

    // POST /api/Transactions/submit
    // Player submits transaction number + calculated amount.
    // Status is always set to Pending.
    [HttpPost("submit")]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
   
    public async Task<IActionResult> Submit(
        [FromHeader(Name = "X-Player-Id")] Guid playerId,
        [FromBody] SubmitTransactionDto dto,
        CancellationToken ct)

    {
        if (string.IsNullOrWhiteSpace(dto.MobilePayReqId))
            return BadRequest(new { message = "Transaction number is required." });

        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        if (playerId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid X-Player-Id header." });


        var playerExists = await _db.Players.AnyAsync(p => p.PlayerId == playerId, ct);
        if (!playerExists)
            return NotFound(new { message = "Player does not exist." });

        var duplicate = await _db.Transactions.AnyAsync(t => t.MobilepayReqId == dto.MobilePayReqId, ct);
        if (duplicate)
            return Conflict(new { message = "Transaction number already exists." });

        var entity = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            PlayerId = playerId,
            MobilepayReqId = dto.MobilePayReqId.Trim(),
            Amount = dto.Amount,
            Status = "Pending"
            // Timestamp is set by DB (DEFAULT now())
        };

        _db.Transactions.Add(entity);
        await _db.SaveChangesAsync(ct);

        var response = new TransactionResponseDto
        {
            TransactionId = entity.TransactionId,
            PlayerId = entity.PlayerId,
            MobilePayReqId = entity.MobilepayReqId,
            Amount = entity.Amount,
            Status = entity.Status,
            Timestamp = entity.Timestamp
        };

        return Created($"/api/Transactions/by-number/{entity.MobilepayReqId}", response);
    }

    // =========================
    // ADMIN SIDE
    // =========================

    // POST /api/Transactions
    // Admin / system creates a transaction directly
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto, CancellationToken ct)
    {
        if (dto.PlayerId == Guid.Empty)
            return BadRequest(new { message = "PlayerId is required." });

        dto.MobilePayReqId = (dto.MobilePayReqId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(dto.MobilePayReqId))
            return BadRequest(new { message = "MobilePayReqId is required." });

        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        var playerExists = await _db.Players.AnyAsync(p => p.PlayerId == dto.PlayerId, ct);
        if (!playerExists)
            return BadRequest(new { message = "Player does not exist." });

        var exists = await _db.Transactions.AnyAsync(t => t.MobilepayReqId == dto.MobilePayReqId, ct);
        if (exists)
            return Conflict(new { message = "Transaction number already exists." });

        var entity = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            PlayerId = dto.PlayerId,
            MobilepayReqId = dto.MobilePayReqId,
            Amount = dto.Amount,
            Status = "Pending"
        };

        _db.Transactions.Add(entity);
        await _db.SaveChangesAsync(ct);

        var response = new TransactionResponseDto
        {
            TransactionId = entity.TransactionId,
            PlayerId = entity.PlayerId,
            MobilePayReqId = entity.MobilepayReqId,
            Amount = entity.Amount,
            Status = entity.Status,
            Timestamp = entity.Timestamp
        };

        return Created($"/api/Transactions/by-number/{entity.MobilepayReqId}", response);
    }

    // GET /api/Transactions/by-number/{mobilePayReqId}
    [HttpGet("by-number/{mobilePayReqId}")]
    public async Task<IActionResult> GetByNumber(string mobilePayReqId, CancellationToken ct)
    {
        var tx = await _db.Transactions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.MobilepayReqId == mobilePayReqId.Trim(), ct);

        if (tx is null)
            return NotFound();

        return Ok(new TransactionResponseDto
        {
            TransactionId = tx.TransactionId,
            PlayerId = tx.PlayerId,
            MobilePayReqId = tx.MobilepayReqId,
            Amount = tx.Amount,
            Status = tx.Status,
            Timestamp = tx.Timestamp
        });
    }

    // PATCH /api/Transactions/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateTransactionStatusDto dto, CancellationToken ct)
    {
        var newStatus = (dto.Status ?? string.Empty).Trim();

        if (newStatus is not ("Pending" or "Approved" or "Rejected"))
            return BadRequest(new { message = "Invalid status." });

        var tx = await _db.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id, ct);
        if (tx is null)
            return NotFound();

        tx.Status = newStatus;
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // GET /api/Transactions
    [HttpGet]
    public async Task<IActionResult> List(string? status, string? search, CancellationToken ct)
    {
        var q = _db.Transactions
            .AsNoTracking()
            .Include(t => t.Player)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(t => t.Status == status.Trim());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(t =>
                t.MobilepayReqId.ToLower().Contains(s) ||
                (t.Player.FirstName + " " + t.Player.LastName).ToLower().Contains(s) ||
                t.Player.Email.ToLower().Contains(s)
            );
        }

        var result = await q
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new AdminTransactionListItemDto
            {
                TransactionId = t.TransactionId,
                MobilePayReqId = t.MobilepayReqId,
                PlayerId = t.PlayerId,
                PlayerFirstName = t.Player.FirstName,
                PlayerLastName = t.Player.LastName,
                PlayerEmail = t.Player.Email,
                Amount = t.Amount,
                Status = t.Status,
                Timestamp = t.Timestamp
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    // =========================
    // TEMP PLAYER IDENTIFICATION
    // =========================
    private bool TryGetPlayerId(out Guid playerId)
    {
        // TEMPORARY (dev only):
        // Player identity is taken from a custom header.
        // This avoids exposing playerId fields in the UI.
        //
        // Will be replaced after authentication is implemented:
        // var playerId = Guid.Parse(User.FindFirst("playerId")!.Value);
        //
        // Expected header:
        // X-Player-Id: <player-guid>

        if (Request.Headers.TryGetValue("X-Player-Id", out var headerValue) &&
            Guid.TryParse(headerValue.ToString(), out playerId))
        {
            return true;
        }

        playerId = Guid.Empty;
        return false;
    }
}
*/