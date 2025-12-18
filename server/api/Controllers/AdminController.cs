using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = efscaffold.MyDbContext;
using api.Models.Requests;
using api.Models.Response;
using api.Models;
using efscaffold.Entities;
using PasswordHasher = api.Etc.PasswordHasher;

// IMPORTANT: resolve conflict (FullName/PhoneNumber)
using PlayerResponseDto = api.Models.PlayerResponseDto;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly MyDbContext _dbContext;

    public AdminController(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var admins = await _dbContext.Admins
            .Select(a => new AdminResponseDto
            {
                AdminId = a.AdminId,
                Email = a.Email,
                FirstName = a.FirstName,
                LastName = a.LastName
            })
            .ToListAsync();

        return Ok(admins);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var admin = await _dbContext.Admins
            .Where(a => a.AdminId == id)
            .Select(a => new AdminResponseDto
            {
                AdminId = a.AdminId,
                Email = a.Email,
                FirstName = a.FirstName,
                LastName = a.LastName
            })
            .FirstOrDefaultAsync();

        if (admin == null) return NotFound();
        return Ok(admin);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Admin admin)
    {
        if (string.IsNullOrWhiteSpace(admin.Email))
            return BadRequest("Email is required");

        if (string.IsNullOrWhiteSpace(admin.PasswordHash))
            return BadRequest("PasswordHash is required");

        if (await _dbContext.Admins.AnyAsync(a => a.Email == admin.Email))
            return BadRequest("Email already exists");

        admin.AdminId = Guid.NewGuid();

        _dbContext.Admins.Add(admin);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = admin.AdminId }, admin);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Admin updatedAdmin)
    {
        if (string.IsNullOrWhiteSpace(updatedAdmin.Email))
            return BadRequest("Email cannot be empty");
        
        if (await _dbContext.Admins.AnyAsync(a => a.Email == updatedAdmin.Email && a.AdminId != id))
            return BadRequest("Email already in use");
        
        // Token validation
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized("No token provided");

        var token = authHeader.Substring("Bearer ".Length).Trim();

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret)) 
            return StatusCode(500, "JWT_SECRET missing");

        var adminId = JwtValidator.ValidateToken(token, secret);
        if (adminId == null) 
            return Unauthorized("Invalid token");

        var admin = await _dbContext.Admins.FindAsync(id);
        if (admin == null) return NotFound();

        admin.FirstName = updatedAdmin.FirstName;
        admin.LastName = updatedAdmin.LastName;
        admin.Email = updatedAdmin.Email;
        admin.PasswordHash = updatedAdmin.PasswordHash;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // Token validation
        var token = Request.Headers["X-Auth-Token"].FirstOrDefault();
        if (token == null) return Unauthorized("No token");
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret)) return StatusCode(500, "JWT_SECRET missing");
        var adminId = JwtValidator.ValidateToken(token, secret);
        if (adminId == null) return Unauthorized("Invalid token");

        var admin = await _dbContext.Admins.FindAsync(id);
        if (admin == null) return NotFound();

        _dbContext.Admins.Remove(admin);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("create-player")]
    public async Task<ActionResult<PlayerResponseDto>> CreatePlayer([FromBody] RegisterPlayerRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Token validation (admin only) - keep as you already do
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized("No token provided");

        var token = authHeader.Substring("Bearer ".Length).Trim();

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret)) return StatusCode(500, "JWT_SECRET missing");
        var adminId = JwtValidator.ValidateToken(token, secret);
        if (adminId == null) return Unauthorized("Invalid token");

        // Validation
        if (await _dbContext.Players.AnyAsync(p => p.Email == dto.Email))
            return BadRequest("Email already in use");

        if (dto.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters long");

        if (!dto.Email.Contains("@"))
            return BadRequest("Invalid email format");

        var player = new Player
        {
            PlayerId = Guid.NewGuid(),
            FirstName = dto.FirstName?.Trim(),
            LastName = dto.LastName?.Trim(),
            Email = dto.Email.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            PasswordHash = PasswordHasher.Hash(dto.Password),
            IsActive = true
        };

        _dbContext.Players.Add(player);
        await _dbContext.SaveChangesAsync();

        return Ok(new PlayerResponseDto
        {
            PlayerId = player.PlayerId,
            FullName = ((player.FirstName ?? "") + " " + (player.LastName ?? "")).Trim(),
            Email = player.Email,
            PhoneNumber = player.PhoneNumber,
            IsActive = player.IsActive
        });
    }

    
    [HttpPost("{gameId:guid}/publish-winning-numbers")]
    public async Task<IActionResult> PublishWinningNumbers(Guid gameId, [FromBody] WinningNumbersDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Token validation (admin only)
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized("No token provided");

        var token = authHeader.Substring("Bearer ".Length).Trim();
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret)) return StatusCode(500, "JWT_SECRET missing");

        var adminId = JwtValidator.ValidateToken(token, secret);
        if (adminId == null) return Unauthorized("Invalid token");

        // Additional safety check
        if (dto.Numbers.Distinct().Count() != 3 || dto.Numbers.Any(n => n < 1 || n > 16))
            return BadRequest("You must choose exactly 3 distinct numbers between 1 and 16.");

        var game = await _dbContext.Games.FindAsync(gameId);
        if (game == null) return NotFound("Game not found");
        if (game.DrawDate != null) return BadRequest("Winning numbers already published for this game.");

        game.WinningNumbers = dto.Numbers.OrderBy(n => n).ToList();
        game.DrawDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(new { game.GameId, game.WinningNumbers });
    }

    [HttpGet("games/{gameId:guid}/payout-overview")]
public async Task<ActionResult<AdminPayoutOverviewResponseDto>> GetPayoutOverview(Guid gameId)
{
    // Token validation (admin only) - same style as your other endpoints
    var authHeader = Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        return Unauthorized("No token provided");

    var token = authHeader.Substring("Bearer ".Length).Trim();
    var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
    if (string.IsNullOrWhiteSpace(secret)) return StatusCode(500, "JWT_SECRET missing");

    var adminId = JwtValidator.ValidateToken(token, secret);
    if (adminId == null) return Unauthorized("Invalid token");

    // 1) 404 if game does not exist
    var gameExists = await _dbContext.Games.AnyAsync(g => g.GameId == gameId);
    if (!gameExists) return NotFound("Game not found");

    // 2) Pool = sum of board prices for this game
    var totalPrizePool = await _dbContext.Boards
        .Where(b => b.GameId == gameId)
        .SumAsync(b => (decimal?)b.Price) ?? 0m;

    // 3) Total players = distinct players who bought boards in this game
    var totalPlayers = await _dbContext.Boards
        .Where(b => b.GameId == gameId)
        .Select(b => b.PlayerId)
        .Distinct()
        .CountAsync();

    // 4) Winners (distinct by BoardId)
    var winnersRaw = await _dbContext.Winningboards
        .Where(w => w.GameId == gameId)
        .Include(w => w.Board)
            .ThenInclude(b => b.Player)
        .ToListAsync();

    var winnersDistinct = winnersRaw
        .GroupBy(w => w.BoardId)
        .Select(g => g
            .OrderByDescending(x => x.WinningNumbersMatched)
            .ThenByDescending(x => x.Timestamp)
            .First())
        .ToList();

    var winnerCount = winnersDistinct.Count;

    // 5) Split 30/70 and payout per winner (cash offline)
    var profit30 = Math.Round(totalPrizePool * 0.30m, 2, MidpointRounding.AwayFromZero);
    var winnersPool70 = totalPrizePool - profit30;

    var payoutPerWinner = winnerCount > 0
        ? Math.Round(winnersPool70 / winnerCount, 2, MidpointRounding.AwayFromZero)
        : 0m;

    var paidOutTotal = payoutPerWinner * winnerCount;
    var remainder = winnersPool70 - paidOutTotal;

    var response = new AdminPayoutOverviewResponseDto
    {
        GameId = gameId,
        TotalPlayers = totalPlayers,
        TotalPrizePool = totalPrizePool,
        WinnerCount = winnerCount,
        Profit30Percent = profit30,
        WinnersPool70Percent = winnersPool70,
        PayoutPerWinner = payoutPerWinner,
        Remainder = remainder,
        Winners = winnersDistinct.Select(w => new AdminWinnerLineDto
        {
            WinningboardId = w.WinningboardId,
            BoardId = w.BoardId,
            PlayerId = w.Board.PlayerId,
            PlayerName = ((w.Board.Player.FirstName ?? "") + " " + (w.Board.Player.LastName ?? "")).Trim(),
            WinningNumbersMatched = w.WinningNumbersMatched,
            Timestamp = w.Timestamp,
            Payout = payoutPerWinner
        }).ToList()
    };

    return Ok(response);
}

[HttpPost("games/{gameId:guid}/mark-winner")]
public async Task<IActionResult> MarkWinner(Guid gameId, [FromBody] MarkWinnerRequestDto dto)
{
    // --- Admin token validation (same pattern as other admin endpoints)
    var authHeader = Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        return Unauthorized("No token provided");

    var token = authHeader.Substring("Bearer ".Length).Trim();
    var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
    if (string.IsNullOrWhiteSpace(secret))
        return StatusCode(500, "JWT_SECRET missing");

    var adminId = JwtValidator.ValidateToken(token, secret);
    if (adminId == null)
        return Unauthorized("Invalid token");

    // --- Validation
    if (dto.BoardId == Guid.Empty)
        return BadRequest("BoardId is required");

    if (dto.WinningNumbersMatched < 0)
        return BadRequest("WinningNumbersMatched must be >= 0");

    // --- Game exists?
    var gameExists = await _dbContext.Games.AnyAsync(g => g.GameId == gameId);
    if (!gameExists)
        return NotFound("Game not found");

    // --- Board exists and belongs to this game
    var board = await _dbContext.Boards.FirstOrDefaultAsync(b => b.BoardId == dto.BoardId);
    if (board == null)
        return NotFound("Board not found");

    if (board.GameId != gameId)
        return BadRequest("Board does not belong to this game");

    // --- Prevent duplicate winners
    var alreadyWinner = await _dbContext.Winningboards
        .AnyAsync(w => w.GameId == gameId && w.BoardId == dto.BoardId);

    if (alreadyWinner)
        return Conflict("This board is already marked as winner for this game");

    // --- Create winning board
    var winningBoard = new Winningboard
    {
        WinningboardId = Guid.NewGuid(),
        GameId = gameId,
        BoardId = dto.BoardId,
        WinningNumbersMatched = dto.WinningNumbersMatched,
        Timestamp = DateTime.UtcNow
    };

    _dbContext.Winningboards.Add(winningBoard);
    await _dbContext.SaveChangesAsync();

    return Ok(winningBoard);
}

    
}
