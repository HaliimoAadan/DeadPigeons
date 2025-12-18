using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDbContext = efscaffold.MyDbContext;
using api.Models;
using efscaffold.Entities;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly MyDbContext _dbContext;

    public TransactionController(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _dbContext.Transactions.ToListAsync();
        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var transaction = await _dbContext.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Transaction transaction)
    {
        if (transaction.TransactionId == Guid.Empty)
            transaction.TransactionId = Guid.NewGuid();

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = transaction.TransactionId }, transaction);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Transaction updatedTransaction)
    {
        var transaction = await _dbContext.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();

        transaction.PlayerId = updatedTransaction.PlayerId;
        transaction.MobilepayReqId = updatedTransaction.MobilepayReqId;
        transaction.Amount = updatedTransaction.Amount;
        transaction.Status = updatedTransaction.Status;
        transaction.Timestamp = updatedTransaction.Timestamp;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var transaction = await _dbContext.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();

        _dbContext.Transactions.Remove(transaction);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
