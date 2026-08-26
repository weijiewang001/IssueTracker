namespace IssueTracker.Domain.Issues
{
    public static class IssueStatusExtensions
    {
        extension(IssueStatus status)
        {
            public bool CanTransitionTo(IssueStatus next) =>
                (status, next) is
                    (IssueStatus.Open, IssueStatus.InProgress) or
                    (IssueStatus.Open, IssueStatus.Closed) or
                    (IssueStatus.InProgress, IssueStatus.Closed);
        }
    }

}
