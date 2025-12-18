using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = Infrastructure.Postgres.Scaffolding.MyDbContext;
using api.Models;
using efscaffold.Entities;

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
        var admins = await _dbContext.Admins.ToListAsync();
        return Ok(admins);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var admin = await _dbContext.Admins.FindAsync(id);
        if (admin == null) return NotFound();
        return Ok(admin);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Admin admin)
    {
        if (admin.AdminId == Guid.Empty)
            admin.AdminId = Guid.NewGuid();

        _dbContext.Admins.Add(admin);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = admin.AdminId }, admin);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Admin updatedAdmin)
    {
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
        var admin = await _dbContext.Admins.FindAsync(id);
        if (admin == null) return NotFound();

        _dbContext.Admins.Remove(admin);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}