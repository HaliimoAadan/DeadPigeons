using api.Models;
using efscaffold;
using efscaffold.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models.Requests;

namespace api.Controllers;

[ApiController]
[Route("api/transaction")]
public class TransactionController : ControllerBase
{
    private readonly MyDbContext _db;

    public TransactionController(MyDbContext db)
    {
        _db = db;
    }

    // =========================
    // CRUD (required by spec)
    // =========================

    // GET /api/transaction
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _db.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new TransactionResponseDto
            {
                TransactionId = t.TransactionId,
                PlayerId = t.PlayerId,
                MobilePayReqId = t.MobilepayReqId,
                Amount = t.Amount,
                Status = t.Status,
                Timestamp = t.Timestamp
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    // GET /api/transaction/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tx = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionId == id)
            .Select(t => new TransactionResponseDto
            {
                TransactionId = t.TransactionId,
                PlayerId = t.PlayerId,
                MobilePayReqId = t.MobilepayReqId,
                Amount = t.Amount,
                Status = t.Status,
                Timestamp = t.Timestamp
            })
            .FirstOrDefaultAsync(ct);

        return tx is null ? NotFound() : Ok(tx);
    }

    // POST /api/transaction
    // Admin/system creates a transaction directly (Pending)
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

        return CreatedAtAction(nameof(GetById), new { id = entity.TransactionId }, response);
    }

    // PUT /api/transaction/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionDto dto, CancellationToken ct)
    {
        dto.MobilePayReqId = (dto.MobilePayReqId ?? string.Empty).Trim();
        var newStatus = (dto.Status ?? string.Empty).Trim();

        if (dto.PlayerId == Guid.Empty)
            return BadRequest(new { message = "PlayerId is required." });

        if (string.IsNullOrWhiteSpace(dto.MobilePayReqId))
            return BadRequest(new { message = "MobilePayReqId is required." });

        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        if (newStatus is not ("Pending" or "Approved" or "Rejected"))
            return BadRequest(new { message = "Invalid status." });

        var tx = await _db.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id, ct);
        if (tx is null) return NotFound();

        // (Optional safety) keep audit integrity
        if (tx.Status == "Approved")
            return BadRequest(new { message = "Approved transactions cannot be edited." });

        if (!string.Equals(tx.MobilepayReqId, dto.MobilePayReqId, StringComparison.OrdinalIgnoreCase))
        {
            var dup = await _db.Transactions.AnyAsync(
                t => t.MobilepayReqId == dto.MobilePayReqId && t.TransactionId != id, ct);
            if (dup)
                return Conflict(new { message = "Transaction number already exists." });
        }

        tx.PlayerId = dto.PlayerId;
        tx.MobilepayReqId = dto.MobilePayReqId;
        tx.Amount = dto.Amount;
        tx.Status = newStatus;

        if (dto.Timestamp.HasValue)
            tx.Timestamp = dto.Timestamp.Value;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE /api/transaction/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var tx = await _db.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id, ct);
        if (tx is null) return NotFound();

        // (Optional safety) prevent deleting approved transactions
        if (tx.Status == "Approved")
            return BadRequest(new { message = "Approved transactions cannot be deleted." });

        _db.Transactions.Remove(tx);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // =========================
    // PLAYER SIDE (extra endpoint)
    // =========================

    // POST /api/transaction/submit
    // Player submits transaction number + amount. Status forced Pending.
    [HttpPost("submit")]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(
        [FromHeader(Name = "X-Player-Id")] Guid playerId,
        [FromBody] SubmitTransactionDto dto,
        CancellationToken ct)
    {
        if (playerId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid X-Player-Id header." });

        dto.MobilePayReqId = (dto.MobilePayReqId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(dto.MobilePayReqId))
            return BadRequest(new { message = "Transaction number is required." });

        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

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
            MobilepayReqId = dto.MobilePayReqId,
            Amount = dto.Amount,
            Status = "Pending"
        };

        _db.Transactions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Created($"/api/transaction/by-number/{entity.MobilepayReqId}", new TransactionResponseDto
        {
            TransactionId = entity.TransactionId,
            PlayerId = entity.PlayerId,
            MobilePayReqId = entity.MobilepayReqId,
            Amount = entity.Amount,
            Status = entity.Status,
            Timestamp = entity.Timestamp
        });
    }

    // =========================
    // ADMIN SIDE (extra endpoints)
    // =========================

    // GET /api/transaction/by-number/{mobilePayReqId}
    [HttpGet("by-number/{mobilePayReqId}")]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNumber(string mobilePayReqId, CancellationToken ct)
    {
        var key = (mobilePayReqId ?? string.Empty).Trim();

        var tx = await _db.Transactions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.MobilepayReqId == key, ct);

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

    // PATCH /api/transaction/{id}/status
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTransactionStatusDto dto, CancellationToken ct)
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

    // GET /api/transaction/list?status=Approved&search=abc
    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] string? search, CancellationToken ct)
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
                ((t.Player.FirstName ?? "") + " " + (t.Player.LastName ?? "")).ToLower().Contains(s) ||
                (t.Player.Email ?? "").ToLower().Contains(s)
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
    
    // GET /api/transaction/me/balance
    [HttpGet("me/balance")]
    public async Task<IActionResult> GetMyBalance(
        [FromHeader(Name = "X-Player-Id")] Guid playerId,
        CancellationToken ct)
    {
        if (playerId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid X-Player-Id header." });

        var approvedDeposits = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.PlayerId == playerId && t.Status == "Approved")
            .Select(t => (decimal?)t.Amount)
            .SumAsync(ct) ?? 0m;

        var spent = await _db.Boards
            .AsNoTracking()
            .Where(b => b.PlayerId == playerId)
            .Select(b => (decimal?)b.Price)
            .SumAsync(ct) ?? 0m;

        var balance = approvedDeposits - spent;

        return Ok(new
        {
            playerId,
            approvedDeposits,
            spent,
            balance
        });
    }

}
