using api.Models;
using Infrastructure.Postgres.Scaffolding;
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

    // POST /api/Transactions
    // Creates a new transaction with default status = Pending
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto, CancellationToken ct)
    {
        // Validate PlayerId
        if (dto.PlayerId == Guid.Empty)
            return BadRequest(new { message = "PlayerId is required." });

        // Validate MobilePay request id
        dto.MobilePayReqId = (dto.MobilePayReqId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(dto.MobilePayReqId))
            return BadRequest(new { message = "MobilePayReqId is required." });

        // Validate amount
        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        // Ensure player exists
        var playerExists = await _db.Players.AnyAsync(p => p.PlayerId == dto.PlayerId, ct);
        if (!playerExists)
            return BadRequest(new { message = "Player does not exist." });

        // Ensure transaction number is unique
        var exists = await _db.Transactions.AnyAsync(t => t.MobilepayReqId == dto.MobilePayReqId, ct);
        if (exists)
            return Conflict(new { message = "Transaction number already exists." });

        // Create transaction entity
        var entity = new Transaction
        {
            TransactionId = Guid.NewGuid(), // Required because ValueGeneratedNever is used
            PlayerId = dto.PlayerId,
            MobilepayReqId = dto.MobilePayReqId,
            Amount = dto.Amount,
            Status = "Pending"
            // Timestamp is set by the database (DEFAULT now())
        };

        _db.Transactions.Add(entity);
        await _db.SaveChangesAsync(ct);

        // Map to response DTO
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
    // Returns a single transaction by MobilePay request id
    [HttpGet("by-number/{mobilePayReqId}")]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNumber([FromRoute] string mobilePayReqId, CancellationToken ct)
    {
        mobilePayReqId = (mobilePayReqId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(mobilePayReqId))
            return NotFound();

        var tx = await _db.Transactions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.MobilepayReqId == mobilePayReqId, ct);

        if (tx is null)
            return NotFound();

        var response = new TransactionResponseDto
        {
            TransactionId = tx.TransactionId,
            PlayerId = tx.PlayerId,
            MobilePayReqId = tx.MobilepayReqId,
            Amount = tx.Amount,
            Status = tx.Status,
            Timestamp = tx.Timestamp
        };

        return Ok(response);
    }

    // PATCH /api/Transactions/{id}/status
// Updates a transaction status (Pending / Approved / Rejected)
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateTransactionStatusDto dto, CancellationToken ct)
    {
        var newStatus = (dto.Status ?? string.Empty).Trim();

        // Validate allowed statuses
        if (newStatus is not ("Pending" or "Approved" or "Rejected"))
            return BadRequest(new { message = "Invalid status. Allowed: Pending, Approved, Rejected." });

        var tx = await _db.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id, ct);
        if (tx is null)
            return NotFound();

        tx.Status = newStatus;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // GET /api/Transactions?status=Pending&search=MP-0002
    // Returns transactions for admin review (includes player info)
    [HttpGet]
    [ProducesResponseType(typeof(List<AdminTransactionListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] string? search, CancellationToken ct)
    {
        var q = _db.Transactions
            .AsNoTracking()
            .Include(t => t.Player)
            .AsQueryable();

        // Optional status filter (e.g., Pending / Approved / Rejected)
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            q = q.Where(t => t.Status == normalizedStatus);
        }

        // Optional search filter (matches transaction number or player name/email)
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
}
