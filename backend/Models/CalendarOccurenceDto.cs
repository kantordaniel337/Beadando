namespace TasksApi.Models;
public class CalendarOccurrenceDto
{
    public int CalendarTaskId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsModified { get; set; }
}