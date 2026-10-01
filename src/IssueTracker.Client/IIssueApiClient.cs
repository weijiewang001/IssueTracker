using IssueTracker.Contracts;

namespace IssueTracker.Client
{
    public interface IIssueApiClient
    {
        Task<IssueResponse?> GetAsync(int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<IssueSummaryResponse>> ListAsync(CancellationToken cancellationToken);
        Task<IssueResponse> CreateAsync(CreateIssueRequest request, CancellationToken cancellationToken);
        Task<IssueResponse> UpdateAsync(int id, UpdateIssueRequest request, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
        Task<IssueResponse> StartAsync(int id, string assignee, CancellationToken cancellationToken);
        Task<IssueResponse> CloseAsync(int id, CancellationToken cancellationToken);
    }
}
