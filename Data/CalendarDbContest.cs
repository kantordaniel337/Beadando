using Microsoft.EntityFrameworkCore;

public class CalendarDbContext : DbContext
{
    public CalendarDbContext(
        DbContextOptions<CalendarDbContext> options)
        : base(options)
    {
    }

    public DbSet<CalendarTask> CalendarTasks =>
        Set<CalendarTask>();

    public DbSet<Recurrence> Recurrences =>
        Set<Recurrence>();

    public DbSet<Occurrence> Occurrences =>
        Set<Occurrence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CalendarTask>()
            .HasOne(t => t.Recurrence)
            .WithMany(r => r.Tasks)
            .HasForeignKey(t => t.RecurrenceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Occurrence>()
            .HasOne(o => o.CalendarTask)
            .WithMany(t => t.Occurrences)
            .HasForeignKey(o => o.CalendarTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Occurrence>()
            .HasIndex(o => new
            {
                o.CalendarTaskId,
                o.OriginalStartTime
            })
            .IsUnique();

        modelBuilder.Entity<CalendarTask>()
            .Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<Recurrence>()
            .Property(r => r.Unit)
            .HasConversion<int>();
    }
}