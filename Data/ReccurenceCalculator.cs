namespace TasksApi.Data;
using TasksApi.Models;

public static class RecurrenceCalculator
{
    public static DateTime GetOccurrenceStart(
        DateTime originalStart,
        int index,
        int value,
        RecurrenceUnit unit)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        int interval = checked(index * value);

        return unit switch
        {
            RecurrenceUnit.Minute =>
                originalStart.AddMinutes(interval),

            RecurrenceUnit.Hour =>
                originalStart.AddHours(interval),

            RecurrenceUnit.Day =>
                originalStart.AddDays(interval),

            RecurrenceUnit.Week =>
                originalStart.AddDays(checked(interval * 7)),

            RecurrenceUnit.Month =>
                originalStart.AddMonths(interval),

            RecurrenceUnit.Year =>
                originalStart.AddMonths(checked(interval * 12)),

            _ => throw new ArgumentOutOfRangeException(nameof(unit))
        };
    }
}