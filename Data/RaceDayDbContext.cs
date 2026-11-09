using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RaceEvent> Events => Set<RaceEvent>();
    public DbSet<EventCategory> Categories => Set<EventCategory>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.Role).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.Email).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<RaceEvent>().Property(x => x.Name).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<RaceEvent>().Property(x => x.EventType).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<EventCategory>()
            .HasOne(x => x.Event).WithMany(x => x.Categories)
            .HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.Participant).WithMany()
            .HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.Event).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.Category).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrollment>().HasIndex(x => new { x.ParticipantId, x.EventId, x.CategoryId }).IsUnique();
        modelBuilder.Entity<RaceResult>()
            .HasOne(x => x.Enrollment).WithOne(x => x.Result)
            .HasForeignKey<RaceResult>(x => x.EnrollmentId).OnDelete(DeleteBehavior.Cascade);
    }
}
