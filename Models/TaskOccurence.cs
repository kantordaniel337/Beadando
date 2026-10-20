public class TaskOccurence {
  public int Id { get; set; }
  public int ReccurenceId { get; set; }
  public Reccurence Recurrence {get; set;} = null;
  public int FrequencyId {get; set; }
  public Frequency Frequency {get; set;} = null;
}