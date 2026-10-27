using Microsoft.EntityFrameworkCore;
using TasksApi.Data;
using TasksApi.Models;

public class CalendarService
{
    private readonly CalendarDbContext _dbContext;

    public CalendarService(CalendarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CalendarOccurrenceDto>> GetOccurrencesAsync(
        int taskId,
        DateTime rangeStart,
        DateTime rangeEnd)
    {
        if (rangeEnd <= rangeStart)
            throw new ArgumentException("Érvénytelen időszak.");

        var task = await _dbContext.CalendarTasks
            .Include(t => t.Recurrence)
            .Include(t => t.Occurrences)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return new List<CalendarOccurrenceDto>();

        var result = new List<CalendarOccurrenceDto>();

        var duration = task.EndTime - task.StartTime;

        if (task.Recurrence == null)
        {
            AddIfInRange(
                result,
                task,
                task.StartTime,
                task.EndTime,
                false,
                rangeStart,
                rangeEnd);
        }
        else
        {
            var recurrence = task.Recurrence;


            for (int index = 0; ; index++)
            {
                DateTime start;

                try
                {
                    start = RecurrenceCalculator.GetOccurrenceStart(
                        task.StartTime,
                        index,
                        recurrence.Value,
                        recurrence.Unit);
                }
                catch (ArgumentOutOfRangeException)
                {
                    break;
                }
                catch (OverflowException)
                {
                    break;
                }

                if (start >= rangeEnd)
                    break;

                DateTime end = start + duration;

                var exception = task.Occurrences.FirstOrDefault(
                    o => o.OriginalStartTime == start);

                if (exception?.IsCancelled == true)
                    continue;

                DateTime actualStart =
                    exception?.ModifiedStartTime ?? start;

                DateTime actualEnd =
                    exception?.ModifiedEndTime ?? end;

                AddIfInRange(
                    result,
                    task,
                    actualStart,
                    actualEnd,
                    exception != null,
                    rangeStart,
                    rangeEnd);
            }
        }


        foreach (var exception in task.Occurrences)
        {
            if (exception.IsCancelled)
                continue;

            DateTime originalStart = exception.OriginalStartTime;

            if (result.Any(r =>
                r.StartTime ==
                (exception.ModifiedStartTime ?? originalStart)))
            {
                continue;
            }

            if (exception.ModifiedStartTime is not DateTime movedStart)
                continue;

            DateTime originalEnd = originalStart + duration;
            DateTime movedEnd = exception.ModifiedEndTime ?? originalEnd;

            AddIfInRange(
                result,
                task,
                movedStart,
                movedEnd,
                true,
                rangeStart,
                rangeEnd);
        }

        return result
            .OrderBy(o => o.StartTime)
            .ToList();
    }
    
    public async Task ModifyOccurrenceAsync(
        int taskId,
        DateTime originalStart,
        DateTime newStart,
        DateTime newEnd)
    {
        if (newEnd <= newStart)
            throw new ArgumentException("A befejezésnek a kezdés után kell lennie.");

        var task = await _dbContext.CalendarTasks
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new InvalidOperationException("A feladat nem található.");

        var occurrence = await _dbContext.Occurrences
            .FirstOrDefaultAsync(o =>
                o.CalendarTaskId == taskId &&
                o.OriginalStartTime == originalStart);

        if (occurrence == null)
        {
            occurrence = new Occurrence
            {
                CalendarTaskId = taskId,
                OriginalStartTime = originalStart
            };

            _dbContext.Occurrences.Add(occurrence);
        }

        occurrence.ModifiedStartTime = newStart;
        occurrence.ModifiedEndTime = newEnd;
        occurrence.IsCancelled = false;

        await _dbContext.SaveChangesAsync();
    }

    public async Task CancelOccurrenceAsync(
        int taskId,
        DateTime originalStart)
    {
        var task = await _dbContext.CalendarTasks
            .Include(t => t.Recurrence)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new InvalidOperationException("A feladat nem található.");

        var occurrence = await _dbContext.Occurrences
            .FirstOrDefaultAsync(o =>
                o.CalendarTaskId == taskId &&
                o.OriginalStartTime == originalStart);

        if (occurrence == null)
        {
            occurrence = new Occurrence
            {
                CalendarTaskId = taskId,
                OriginalStartTime = originalStart
            };

            _dbContext.Occurrences.Add(occurrence);
        }

        occurrence.IsCancelled = true;

        await _dbContext.SaveChangesAsync();
    }

    private static void AddIfInRange(
        List<CalendarOccurrenceDto> result,
        CalendarTask task,
        DateTime start,
        DateTime end,
        bool isModified,
        DateTime rangeStart,
        DateTime rangeEnd)
    {

        if (start < rangeEnd && end > rangeStart)
        {
            result.Add(new CalendarOccurrenceDto
            {
                CalendarTaskId = task.Id,
                Name = task.Name,
                StartTime = start,
                EndTime = end,
                IsModified = isModified
            });
        }
    }
}