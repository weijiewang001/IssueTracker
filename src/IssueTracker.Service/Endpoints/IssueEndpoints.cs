using IssueTracker.Contracts;
using IssueTracker.Domain.Issues;
using IssueTracker.Service.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IssueTracker.Service.Endpoints
{
    public static class IssueEndpoints
    {
        public static IEndpointRouteBuilder MapIssueEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/issues").WithTags("Issues");

            group.MapGet("/", ListIssues);
            group.MapGet("/{id:int}", GetIssue).WithName("GetIssue");

            group.MapPost("/", CreateIssue);
            group.MapPut("/{id:int}", UpdateIssue);
            group.MapDelete("/{id:int}", DeleteIssue);

            group.MapPost("/{id:int}/start", StartIssue);
            group.MapPost("/{id:int}/close", CloseIssue);

            return routes;
        }

        private static async Task<Ok<IReadOnlyList<IssueSummaryResponse>>> ListIssues(
                IIssueRepository repository,
                ILogger<Program> logger,
                CancellationToken cancellationToken)
        {
            var issues = await repository.ListAsync(cancellationToken);
            logger.LogInformation("Listed {Count} issues.", issues.Count);

            return TypedResults.Ok<IReadOnlyList<IssueSummaryResponse>>(
                issues.Select(IssueMappings.ToSummary).ToList()
            );
        }

        private static async Task<Results<Ok<IssueResponse>, NotFound>> GetIssue(
            int id, IIssueRepository repository, CancellationToken cancellationToken
        )
        {
            var issue = await repository.GetByIdAsync(id, cancellationToken);
            return issue is null ? TypedResults.NotFound() : TypedResults.Ok(issue.ToResponse());
        }

        private static async Task<CreatedAtRoute<IssueResponse>> CreateIssue(
            CreateIssueRequest request,
            IIssueRepository repository,
            ILogger<Program> logger,
            CancellationToken cancellationToken)
        {
            var issue = new Issue(request.Title, request.Description ?? "",
                request.Priority, request.ReportedBy);
            repository.Add(issue);

            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created issue {IssueId} priority={Priority} by {Reporter}.",
                issue.Id, issue.Priority, issue.ReportedBy);

            return TypedResults.CreatedAtRoute(issue.ToResponse(), "GetIssue", new { id = issue.Id });
        }

        private static async Task<Results<Ok<IssueResponse>, NotFound>> UpdateIssue(
         int id, UpdateIssueRequest request, IIssueRepository repository,
         ILogger<Program> logger, CancellationToken cancellationToken)
        {
            var issue = await repository.GetByIdAsync(id, cancellationToken);

            if (issue is null) return TypedResults.NotFound();

            issue.Edit(request.Title, request.Description ?? "", request.Priority);

            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Updated issue {IssueId}.", issue.Id);

            return TypedResults.Ok(issue.ToResponse());
        }

        private static async Task<Results<NoContent, NotFound>> DeleteIssue(
        int id, IIssueRepository repository,
        ILogger<Program> logger, CancellationToken cancellationToken)
        {
            var issue = await repository.GetByIdAsync(id, cancellationToken);
            if (issue is null) return TypedResults.NotFound();

            repository.Remove(issue);
            await repository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Deleted issue {IssueId}.", id);

            return TypedResults.NoContent();
        }

        private static async Task<Results<Ok<IssueResponse>, NotFound>> StartIssue(
            int id, StartIssueRequest request, IIssueRepository repository,
            ILogger<Program> logger, CancellationToken cancellationToken)
        {
            var issue = await repository.GetByIdAsync(id, cancellationToken);
            if (issue is null) return TypedResults.NotFound();

            issue.Start(request.Assignee);

            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Issue {IssueId} started by {Assignee}.",
                issue.Id, issue.AssignedTo);

            return TypedResults.Ok(issue.ToResponse());
        }

        private static async Task<Results<Ok<IssueResponse>, NotFound>> CloseIssue(
            int id, IIssueRepository repository,
            ILogger<Program> logger, CancellationToken cancellationToken)
        {
            var issue = await repository.GetByIdAsync(id, cancellationToken);
            if (issue is null) return TypedResults.NotFound();

            issue.Close();

            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Issue {IssueId} closed.", issue.Id);

            return TypedResults.Ok(issue.ToResponse());
        }
    }

    public static class IssueMappings
    {
        public static IssueResponse ToResponse(this Issue issue) => new(
            issue.Id, issue.Title, issue.Description, issue.Status, issue.Priority,
            issue.ReportedBy, issue.AssignedTo, issue.CreatedAt, issue.UpdatedAt);

        public static IssueSummaryResponse ToSummary(this Issue issue) => new(
            issue.Id, issue.Title, issue.Status, issue.Priority,
            issue.ReportedBy, issue.AssignedTo, issue.UpdatedAt);
    }
}
