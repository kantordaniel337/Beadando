namespace TasksWpf.Models;

public class CalendarTaskDto
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int? RecurrenceId { get; set; }
}