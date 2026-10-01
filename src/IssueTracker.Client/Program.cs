using IssueTracker.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// HttpClient logs every request at Information, which floods the console while polling /health.
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

var baseAddress = builder.Configuration["IssueTracker:BaseAddress"] ?? "https://localhost:7260";

builder.Services.AddHttpClient<IIssueApiClient, IssueApiClient>(client =>
{
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
});

using var host = builder.Build();

var api = host.Services.GetRequiredService<IIssueApiClient>();

const int issueId = 2;

Console.WriteLine($"Waiting for the Issue Tracker API at {baseAddress}...");

if (!await WaitForApiAsync(api, TimeSpan.FromSeconds(60)))
{
    Console.WriteLine($"Could not reach the Issue Tracker API at {baseAddress}. Is IssueTracker.Service running?");
}
else
{
    try
    {
        var issue = await api.GetAsync(issueId, CancellationToken.None);

        Console.WriteLine(issue is null
            ? $"Issue #{issueId} was not found."
            : $"#{issue.Id} {issue.Title} [{issue.Status}]");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Request to the Issue Tracker API failed: {ex.Message}");
    }
}

Console.ReadLine();

// When the Client and Service are launched together (e.g. multiple startup projects in Visual Studio),
// the Service may still be migrating the database, so poll /health before making real requests.
static async Task<bool> WaitForApiAsync(IIssueApiClient api, TimeSpan timeout)
{
    using var cts = new CancellationTokenSource(timeout);
    try
    {
        while (!await api.IsHealthyAsync(cts.Token))
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
        }
        return true;
    }
    catch (OperationCanceledException)
    {
        return false;
    }
}
