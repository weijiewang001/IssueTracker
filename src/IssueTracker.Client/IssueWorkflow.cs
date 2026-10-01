using IssueTracker.Contracts;
using IssueTracker.Domain.Issues;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Client;

public class IssueWorkflow(IIssueApiClient api, ILogger<IssueWorkflow> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var created = await api.CreateAsync(new CreateIssueRequest(
            Title: "Search results stop after page 5",
            Description: "Paginated search silently drops results beyond page 5.",
            Priority: Priority.High,
            ReportedBy: "filip"), cancellationToken);
        logger.LogInformation("Created issue {IssueId} [{Status}].", created.Id, created.Status);

        var started = await api.StartAsync(created.Id, "anna", cancellationToken);
        logger.LogInformation("Started issue {IssueId} [{Status}].", started.Id, started.Status);

        var closed = await api.CloseAsync(created.Id, cancellationToken);
        logger.LogInformation("Closed issue {IssueId} [{Status}].", closed.Id, closed.Status);

        Console.WriteLine("Stop the service in the next 5 seconds, then restart it.");
        for (var i = 5; i > 0; i--)
        {
            Console.WriteLine($"  ListAsync in {i}...");
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        var issues = await api.ListAsync(cancellationToken);
        Console.WriteLine($"There are {issues.Count} issue(s) on the service.");
    }
}