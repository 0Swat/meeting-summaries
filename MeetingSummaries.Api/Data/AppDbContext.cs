using Microsoft.EntityFrameworkCore;
using MeetingSummaries.Api.Models;

namespace MeetingSummaries.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MeetingSummary> MeetingSummaries => Set<MeetingSummary>();
    public DbSet<MeetingPoint> MeetingPoints => Set<MeetingPoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeetingSummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type)
                  .HasConversion<string>();
            entity.HasIndex(e => new { e.Type, e.Date })
                  .IsUnique();
        });

        modelBuilder.Entity<MeetingPoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content)
                  .IsRequired();
            entity.HasOne(e => e.Summary)
                  .WithMany(s => s.Points)
                  .HasForeignKey(e => e.SummaryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
