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
        public string? AssignedTo { get; private set;  }
        public DateTimeOffset CreatedAt { get; private set;  }
        public DateTimeOffset UpdatedAt { get; private set;  }

        private Issue() //For EF Core
        {

        }
    }
}
