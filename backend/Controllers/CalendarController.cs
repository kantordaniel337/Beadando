using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksApi.Data;
using TasksApi.Models;

namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly CalendarDbContext _db;

    public CalendarController(CalendarDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetCalendar(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (to <= from)
            return BadRequest("'to' must be later than 'from'.");

        var tasks = await _db.CalendarTasks
            .AsNoTracking()
            .Include(t => t.Recurrence)
            .Include(t => t.Occurrences)
            .Where(t => t.StartTime < to)
            .ToListAsync();

        var result = new List<object>();

        foreach (var task in tasks)
        {
            var duration = task.EndTime - task.StartTime;


            if (task.Recurrence is null)
            {
                if (task.EndTime > from)
                {
                    result.Add(new
                    {
                        task.Id,
                        task.Name,
                        StartTime = task.StartTime,
                        EndTime = task.EndTime,
                        IsRecurring = false,
                        Recurrence = (object?)null,
                        Occurrence = (object?)null,
                        IsCancelled = false
                    });
                }

                continue;
            }

            var recurrence = task.Recurrence;


            var index = GetStartingIndex(
                task.StartTime, from, recurrence.Value,
                recurrence.Unit, duration);

            var exceptions = task.Occurrences
                .ToDictionary(o => o.OriginalStartTime);

            var generatedStarts = new HashSet<DateTime>();

            for (var i = index; ; i++)
            {
                var occurrenceStart = GetOccurrenceStart(
                    task.StartTime, recurrence.Value,
                    recurrence.Unit, i);

                if (occurrenceStart >= to)
                    break;

                if (occurrenceStart < task.StartTime)
                    continue;

                generatedStarts.Add(occurrenceStart);

                exceptions.TryGetValue(
                    occurrenceStart, out var exception);

                if (exception?.IsCancelled == true)
                    continue;

                var effectiveStart =
                    exception?.ModifiedStartTime ?? occurrenceStart;

                var effectiveEnd =
                    exception?.ModifiedEndTime
                    ?? effectiveStart.Add(duration);

                if (effectiveEnd <= from || effectiveStart >= to)
                    continue;

                result.Add(new
                {
                    task.Id,
                    task.Name,
                    StartTime = effectiveStart,
                    EndTime = effectiveEnd,
                    IsRecurring = true,
                    Recurrence = new
                    {
                        recurrence.Id,
                        recurrence.Value,
                        Unit = recurrence.Unit.ToString()
                    },
                    Occurrence = exception is null ? null : new
                    {
                        exception.Id,
                        exception.OriginalStartTime,
                        exception.ModifiedStartTime,
                        exception.ModifiedEndTime,
                        exception.IsCancelled
                    },
                    IsCancelled = false
                });
            }


            foreach (var exception in task.Occurrences)
            {
                if (generatedStarts.Contains(exception.OriginalStartTime)
                    || exception.IsCancelled
                    || !exception.ModifiedStartTime.HasValue)
                    continue;

                var effectiveStart = exception.ModifiedStartTime.Value;
                var effectiveEnd =
                    exception.ModifiedEndTime ?? effectiveStart.Add(duration);

                if (effectiveStart < to && effectiveEnd > from)
                {
                    result.Add(new
                    {
                        task.Id,
                        task.Name,
                        StartTime = effectiveStart,
                        EndTime = effectiveEnd,
                        IsRecurring = true,
                        Recurrence = new
                        {
                            recurrence.Id,
                            recurrence.Value,
                            Unit = recurrence.Unit.ToString()
                        },
                        Occurrence = new
                        {
                            exception.Id,
                            exception.OriginalStartTime,
                            exception.ModifiedStartTime,
                            exception.ModifiedEndTime,
                            exception.IsCancelled
                        },
                        IsCancelled = false
                    });
                }
            }
        }

        return Ok(result
            .OrderBy(item => item.GetType()
                .GetProperty("StartTime")!
                .GetValue(item)));
    }

    private static DateTime GetOccurrenceStart(
        DateTime originalStart,
        int value,
        RecurrenceUnit unit,
        int index)
    {
        var amount = checked(value * index);

        return unit switch
        {
            RecurrenceUnit.Minute => originalStart.AddMinutes(amount),
            RecurrenceUnit.Hour => originalStart.AddHours(amount),
            RecurrenceUnit.Day => originalStart.AddDays(amount),
            RecurrenceUnit.Week => originalStart.AddDays(amount * 7),
            RecurrenceUnit.Month => originalStart.AddMonths(amount),
            RecurrenceUnit.Year => originalStart.AddYears(amount),
            _ => throw new ArgumentOutOfRangeException(nameof(unit))
        };
    }

    private static int GetStartingIndex(
        DateTime originalStart,
        DateTime from,
        int value,
        RecurrenceUnit unit,
        TimeSpan duration)
    {
        if (from <= originalStart.Add(duration))
            return 0;

        var difference = from - originalStart;

        var estimate = unit switch
        {
            RecurrenceUnit.Minute =>
                (int)(difference.TotalMinutes / value),
            RecurrenceUnit.Hour =>
                (int)(difference.TotalHours / value),
            RecurrenceUnit.Day =>
                (int)(difference.TotalDays / value),
            RecurrenceUnit.Week =>
                (int)(difference.TotalDays / (7 * value)),
            RecurrenceUnit.Month =>
                ((from.Year - originalStart.Year) * 12
                    + from.Month - originalStart.Month) / value,
            RecurrenceUnit.Year =>
                (from.Year - originalStart.Year) / value,
            _ => 0
        };

        var index = Math.Max(0, estimate - 1);

        while (index > 0 &&
               GetOccurrenceStart(originalStart, value, unit, index)
                   .Add(duration) > from)
        {
            index--;
        }

        while (GetOccurrenceStart(originalStart, value, unit, index)
                   .Add(duration) <= from)
        {
            index++;
        }

        return index;
    }
}