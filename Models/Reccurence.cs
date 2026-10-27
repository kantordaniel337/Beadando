public class Recurrence
{
    public int Id { get; set; }

    public int Value { get; set; }

    public RecurrenceUnit Unit { get; set; }

    public ICollection<CalendarTask> Tasks { get; set; }
        = new List<CalendarTask>();
}