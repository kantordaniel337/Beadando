using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksApi.Data;
using TasksApi.Models;


namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarTasksController : ControllerBase
{
    private readonly CalendarDbContext _db;

    public CalendarTasksController(CalendarDbContext db)
    {
        _db = db;
    }

    public record CalendarTaskDto(
        string Name,
        DateTime StartTime,
        DateTime EndTime,
        int? RecurrenceId
    );

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var tasks = await _db.CalendarTasks
            .AsNoTracking()
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.StartTime,
                t.EndTime,
                t.RecurrenceId
            })
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var task = await _db.CalendarTasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.StartTime,
                t.EndTime,
                t.RecurrenceId
            })
            .FirstOrDefaultAsync();

        if (task is null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CalendarTaskDto dto)
    {
        if (dto.EndTime <= dto.StartTime)
            return BadRequest("A befejezésnek a kezdés után kell lennie.");

        if (dto.RecurrenceId.HasValue &&
            !await _db.Recurrences.AnyAsync(
                r => r.Id == dto.RecurrenceId.Value))
        {
            return BadRequest("A megadott ismétlődési szabály nem létezik.");
        }

        var task = new CalendarTask
        {
            Name = dto.Name,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            RecurrenceId = dto.RecurrenceId
        };

        _db.CalendarTasks.Add(task);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = task.Id },
            new
            {
                task.Id,
                task.Name,
                task.StartTime,
                task.EndTime,
                task.RecurrenceId
            });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, CalendarTaskDto dto)
    {
        var task = await _db.CalendarTasks.FindAsync(id);

        if (task is null)
            return NotFound();

        if (dto.EndTime <= dto.StartTime)
            return BadRequest("A befejezésnek a kezdés után kell lennie.");

        if (dto.RecurrenceId.HasValue &&
            !await _db.Recurrences.AnyAsync(
                r => r.Id == dto.RecurrenceId.Value))
        {
            return BadRequest("A megadott ismétlődési szabály nem létezik.");
        }

        task.Name = dto.Name;
        task.StartTime = dto.StartTime;
        task.EndTime = dto.EndTime;
        task.RecurrenceId = dto.RecurrenceId;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.CalendarTasks.FindAsync(id);

        if (task is null)
            return NotFound();

        _db.CalendarTasks.Remove(task);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}