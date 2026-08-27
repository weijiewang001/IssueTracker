using IssueTracker.Domain.Common;

namespace IssueTracker.Domain.Issues
{
    public sealed class Issue
    {
        public int Id { get; private set; }
        public string Title
        {
            get;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new DomainException("Issue title cannot be empty");
                }
                field = value.Trim();
            }
        } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public IssueStatus Status { get; private set; }
        public Priority Priority { get; private set; }
        public string ReportedBy { get; private set; } = string.Empty;
        public string? AssignedTo { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private Issue() //For EF Core
        {

        }

        public Issue(string title, string description, Priority priority, string reportedBy)
        {
            if (string.IsNullOrWhiteSpace(reportedBy))
            {
                throw new DomainException("Issue reporter is required");
            }
            Title = title;
            Description = description?.Trim() ?? string.Empty;
            Priority = priority;
            ReportedBy = reportedBy.Trim();
            Status = IssueStatus.Open;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;

        }

        public void Edit(string title, string description, Priority priority)
        {
            if (Status == IssueStatus.Closed)
            {
                throw new DomainException("Cannot edit a closed issue");
            }

            Title = title;
            Description = description.Trim();
            Priority = priority;
            Touch();
        }

        public void Start(string assigne)
        {
            if (!Status.CanTransitionTo(IssueStatus.InProgress))
            {
                throw new DomainException($"Cannot start an issue from status {Status}");
            }
            if (string.IsNullOrWhiteSpace(assigne))
            {
                throw new DomainException("An assignee is required to start an issue");
            }

            AssignedTo = assigne;
            Status = IssueStatus.InProgress;
            Touch();

        }

        public void Close()
        {
            if (Status == IssueStatus.Closed)
            {
                throw new DomainException("Issue is already closed");
            }
            if (!Status.CanTransitionTo(IssueStatus.Closed))
            {
                throw new DomainException($"Cannot close an issue from status {Status}");
            }

            Status = IssueStatus.Closed;
            Touch();
        }

        public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
