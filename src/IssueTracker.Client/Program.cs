using IssueTracker.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

var builder = Host.CreateApplicationBuilder(args);

var baseAddress = builder.Configuration["IssueTracker:BaseAddress"] ?? "https://localhost:7260";

builder.Services
    .AddHttpClient<IIssueApiClient, IssueApiClient>(client => {
        client.BaseAddress = new Uri(baseAddress);
        // No client.Timeout: HttpClient's timeout wraps the whole resilience pipeline, so a 30s value
        // would cut retries short. TotalRequestTimeout below is the single overall limit instead.
    })
    .AddStandardResilienceHandler(options => {
        options.Retry.MaxRetryAttempts = 5;
        // Retrying a POST/PUT/DELETE that already reached the Service can create duplicate issues.
        // So: safe methods retry on any transient failure, unsafe ones only when the connection could
        // not be made at all (the request never left, e.g. the Service is still starting up).
        options.Retry.ShouldHandle = args =>
        {
            if (args.Outcome.Exception is HttpRequestException { HttpRequestError: HttpRequestError.ConnectionError })
            {
                return ValueTask.FromResult(true);
            }

            var method = args.Context.GetRequestMessage()?.Method;
            var isSafe = method == HttpMethod.Get || method == HttpMethod.Head || method == HttpMethod.Options;

            return ValueTask.FromResult(isSafe && HttpClientResiliencePredicates.IsTransient(args.Outcome));
        };
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
        // 5 exponential back-off retries wait roughly 2 + 4 + 8 + 16 + 32 seconds.
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);
    });

// Transient, not singleton: a singleton would hold the typed client's HttpClient for the app's lifetime.
builder.Services.AddTransient<IssueWorkflow>();

using var host = builder.Build();

using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(2));
var cancellationToken = cancellation.Token;

var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    var workflow = host.Services.GetRequiredService<IssueWorkflow>();

    await workflow.RunAsync(cancellationToken);
}
catch (IssueApiException ex)
{
    logger.LogError(ex, "API call failed with status {Status}.", (int)ex.HttpStatusCode);
    return;
}
catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException)
{
    // Thrown once the resilience handler has used up its retries, e.g. the Service is not running.
    logger.LogError(ex, "Could not reach the Issue Tracker API at {BaseAddress}.", baseAddress);
    return;
}
catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
{
    logger.LogError("The workflow did not finish within 2 minutes.");
    return;
}

Console.ReadLine();
