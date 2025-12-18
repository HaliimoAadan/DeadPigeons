using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = efscaffold.MyDbContext;
using api.Models.Requests;
using api.Models.Response;
using efscaffold.Entities;
using PasswordHasher = api.Etc.PasswordHasher;

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
    public async Task<IActionResult> CreatePlayer([FromBody] RegisterPlayerRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        // Token validation
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized("No token provided");

        var token = authHeader.Substring("Bearer ".Length).Trim();

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secret)) return StatusCode(500, "JWT_SECRET missing");
        var adminId = JwtValidator.ValidateToken(token, secret);
        if (adminId == null) return Unauthorized("Invalid token");

        // Check email
        if (await _dbContext.Players.AnyAsync(p => p.Email == dto.Email))
            return BadRequest("Email already in use");
        
        if (dto.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters long");
        
        if (!dto.Email.Contains("@"))
            return BadRequest("Invalid email format");

        var player = new Player
        {
            PlayerId = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            IsActive = true
        };

        _dbContext.Players.Add(player);
        await _dbContext.SaveChangesAsync();

        return Ok(new { player.PlayerId, player.Email });
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

}
