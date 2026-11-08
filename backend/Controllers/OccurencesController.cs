using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksApi.Data;
using TasksApi.Models;

namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OccurrencesController : ControllerBase
{
    private readonly CalendarDbContext _db;

    public OccurrencesController(CalendarDbContext db)
    {
        _db = db;
    }

    public record OccurrenceDto(
        int CalendarTaskId,
        DateTime OriginalStartTime,
        DateTime? ModifiedStartTime,
        DateTime? ModifiedEndTime,
        bool IsCancelled
    );

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? calendarTaskId)
    {
        var query = _db.Occurrences.AsNoTracking();

        if (calendarTaskId.HasValue)
        {
            query = query.Where(
                o => o.CalendarTaskId == calendarTaskId.Value);
        }

        var items = await query
            .Select(o => new
            {
                o.Id,
                o.CalendarTaskId,
                o.OriginalStartTime,
                o.ModifiedStartTime,
                o.ModifiedEndTime,
                o.IsCancelled
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Occurrences
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new
            {
                o.Id,
                o.CalendarTaskId,
                o.OriginalStartTime,
                o.ModifiedStartTime,
                o.ModifiedEndTime,
                o.IsCancelled
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(OccurrenceDto dto)
    {
        if (!await _db.CalendarTasks.AnyAsync(
            t => t.Id == dto.CalendarTaskId))
        {
            return BadRequest("A megadott feladat nem létezik.");
        }

        if (dto.ModifiedStartTime.HasValue !=
            dto.ModifiedEndTime.HasValue)
        {
            return BadRequest(
                "A módosított kezdési és befejezési időt együtt add meg.");
        }

        if (dto.ModifiedStartTime.HasValue &&
            dto.ModifiedEndTime <= dto.ModifiedStartTime)
        {
            return BadRequest(
                "A módosított befejezésnek a kezdés után kell lennie.");
        }

        var item = new Occurrence
        {
            CalendarTaskId = dto.CalendarTaskId,
            OriginalStartTime = dto.OriginalStartTime,
            ModifiedStartTime = dto.ModifiedStartTime,
            ModifiedEndTime = dto.ModifiedEndTime,
            IsCancelled = dto.IsCancelled
        };

        _db.Occurrences.Add(item);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "Ehhez a feladathoz már létezik kivétel ezzel az eredeti kezdési idővel.");
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            new
            {
                item.Id,
                item.CalendarTaskId,
                item.OriginalStartTime,
                item.ModifiedStartTime,
                item.ModifiedEndTime,
                item.IsCancelled
            });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, OccurrenceDto dto)
    {
        var item = await _db.Occurrences.FindAsync(id);

        if (item is null)
            return NotFound();

        if (!await _db.CalendarTasks.AnyAsync(
            t => t.Id == dto.CalendarTaskId))
        {
            return BadRequest("A megadott feladat nem létezik.");
        }

        if (dto.ModifiedStartTime.HasValue !=
            dto.ModifiedEndTime.HasValue)
        {
            return BadRequest(
                "A módosított kezdési és befejezési időt együtt add meg.");
        }

        if (dto.ModifiedStartTime.HasValue &&
            dto.ModifiedEndTime <= dto.ModifiedStartTime)
        {
            return BadRequest(
                "A módosított befejezésnek a kezdés után kell lennie.");
        }

        item.CalendarTaskId = dto.CalendarTaskId;
        item.OriginalStartTime = dto.OriginalStartTime;
        item.ModifiedStartTime = dto.ModifiedStartTime;
        item.ModifiedEndTime = dto.ModifiedEndTime;
        item.IsCancelled = dto.IsCancelled;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(
                "Már létezik kivétel ezzel a feladattal és eredeti kezdési idővel.");
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Occurrences.FindAsync(id);

        if (item is null)
            return NotFound();

        _db.Occurrences.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}