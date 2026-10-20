public class Task{
    public int Id{get ; set ;}
    public string Nev{get ; set;} = string.Empty;
    public string Leiras{get ; set;} = string.Empty;
    public TaskOccurrence Occurrences { get; set; } = null;
}


