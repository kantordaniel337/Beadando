using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksApi.Data;
using TasksApi.Models;

namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecurrencesController : ControllerBase
{
    private readonly CalendarDbContext _db;

    public RecurrencesController(CalendarDbContext db)
    {
        _db = db;
    }

    public record RecurrenceDto(int Value, RecurrenceUnit Unit);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Recurrences
            .AsNoTracking()
            .Select(r => new
            {
                r.Id,
                r.Value,
                r.Unit
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Recurrences
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.Value,
                r.Unit
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RecurrenceDto dto)
    {
        if (dto.Value < 1)
            return BadRequest("Az ismétlődési érték legalább 1 legyen.");

        if (!Enum.IsDefined(typeof(RecurrenceUnit), dto.Unit))
            return BadRequest("Érvénytelen ismétlődési egység.");

        var item = new Recurrence
        {
            Value = dto.Value,
            Unit = dto.Unit
        };

        _db.Recurrences.Add(item);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            new { item.Id, item.Value, item.Unit });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, RecurrenceDto dto)
    {
        var item = await _db.Recurrences.FindAsync(id);

        if (item is null)
            return NotFound();

        if (dto.Value < 1)
            return BadRequest("Az ismétlődési érték legalább 1 legyen.");

        if (!Enum.IsDefined(typeof(RecurrenceUnit), dto.Unit))
            return BadRequest("Érvénytelen ismétlődési egység.");

        item.Value = dto.Value;
        item.Unit = dto.Unit;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Recurrences.FindAsync(id);

        if (item is null)
            return NotFound();

        _db.Recurrences.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}