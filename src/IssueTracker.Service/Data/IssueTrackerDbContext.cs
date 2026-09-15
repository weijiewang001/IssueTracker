using Microsoft.EntityFrameworkCore;
using IssueTracker.Domain.Issues;

namespace IssueTracker.Service.Data
{
    public class IssueTrackerDbContext(DbContextOptions<IssueTrackerDbContext> options) : DbContext(options)
    {
        public DbSet<Issue> Issues => Set<Issue>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var issue = modelBuilder.Entity<Issue>();
            issue.ToTable("Issues");
            issue.HasKey(i => i.Id);
            issue.Property(i => i.Title).IsRequired().HasMaxLength(200);
            issue.Property(i => i.Description).HasMaxLength(4000);
            issue.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
            issue.Property(i => i.Priority).HasConversion<string>().HasMaxLength(20);
            issue.Property(i => i.ReportedBy).IsRequired().HasMaxLength(120);
            issue.Property(i => i.AssignedTo).HasMaxLength(120);
        }
    }
}
