namespace TasksApi.Models;
public class Occurrence
{
    public int Id { get; set; }

    public int CalendarTaskId { get; set; }

    public CalendarTask CalendarTask { get; set; } = null!;

    public DateTime OriginalStartTime { get; set; }

    public DateTime? ModifiedStartTime { get; set; }

    public DateTime? ModifiedEndTime { get; set; }

    public bool IsCancelled { get; set; }
}