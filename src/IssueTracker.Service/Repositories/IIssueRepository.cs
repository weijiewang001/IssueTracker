using IssueTracker.Domain.Issues;

namespace IssueTracker.Service.Repositories;

public interface IIssueRepository
{
    Task<Issue?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Issue>> ListAsync(CancellationToken cancellationToken);
    void Add(Issue issue);
    void Remove(Issue issue);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}