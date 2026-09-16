using IssueTracker.Domain.Issues;
using IssueTracker.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Service.Repositories;

public class IssueRepository(IssueTrackerDbContext context) : IIssueRepository
{
    public async Task<Issue?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await context.Issues.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Issue>> ListAsync(CancellationToken cancellationToken) =>
        await context.Issues
            .AsNoTracking()
            .OrderByDescending(i => i.UpdatedAt)
            .ToListAsync(cancellationToken);

    public void Add(Issue issue) => context.Issues.Add(issue);

    public void Remove(Issue issue) => context.Issues.Remove(issue);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        await context.SaveChangesAsync();
}