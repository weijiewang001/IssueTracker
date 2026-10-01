using System.Net;
using System.Net.Http.Json;
using IssueTracker.Contracts;

namespace IssueTracker.Client
{
    public class IssueApiClient(HttpClient client) : IIssueApiClient
    {
        public async Task<IssueResponse?> GetAsync(int id, CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync($"api/issues/{id}",
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IssueResponse>(cancellationToken);
        }

        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var response = await client.GetAsync("health", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

    }
}
