using IssueTracker.Domain.Issues;

namespace IssueTracker.Contracts;

public record CreateIssueRequest(
    string Title,
    string? Description,
    Priority Priority,
    string ReportedBy);

public record UpdateIssueRequest(
    string Title,
    string? Description,
    Priority Priority);

public record StartIssueRequest(string Assignee);

public record IssueSummaryResponse(
    int Id,
    string Title,
    IssueStatus Status,
    Priority Priority,
    string ReportedBy,
    string? AssignedTo,
    DateTimeOffset UpdatedAt);

public record IssueResponse(
    int Id,
    string Title,
    string Description,
    IssueStatus Status,
    Priority Priority,
    string ReportedBy,
    string? AssignedTo,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
