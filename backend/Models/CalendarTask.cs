namespace TasksApi.Models;
public class CalendarTask
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int? RecurrenceId { get; set; }

    public Recurrence? Recurrence { get; set; }

    public ICollection<Occurrence> Occurrences { get; set; }
        = new List<Occurrence>();
}