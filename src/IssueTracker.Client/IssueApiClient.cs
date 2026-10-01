using System.Net;
using System.Net.Http.Json;
using IssueTracker.Contracts;

namespace IssueTracker.Client
{
    public class IssueApiClient(HttpClient client) : IIssueApiClient
    {
        public async Task<IssueResponse?> GetAsync(int id, CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync($"api/issues/{id}", cancellationToken);

            if (response.StatusCode is HttpStatusCode.NotFound) return null;

            return await ReadRequiredAsync<IssueResponse>(response, cancellationToken);
        }

        public async Task<IReadOnlyList<IssueSummaryResponse>> ListAsync(CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync("api/issues", cancellationToken);

            return await ReadRequiredAsync<List<IssueSummaryResponse>>(response, cancellationToken);
        }

        public async Task<IssueResponse> CreateAsync(CreateIssueRequest request, CancellationToken cancellationToken)
        {
            using var response = await client.PostAsJsonAsync("api/issues", request, cancellationToken);

            return await ReadRequiredAsync<IssueResponse>(response, cancellationToken);
        }

        public async Task<IssueResponse> UpdateAsync(int id, UpdateIssueRequest request, CancellationToken cancellationToken)
        {
            using var response = await client.PutAsJsonAsync($"api/issues/{id}", request, cancellationToken);

            return await ReadRequiredAsync<IssueResponse>(response, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            using var response = await client.DeleteAsync($"api/issues/{id}", cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<IssueResponse> StartAsync(int id, string assignee, CancellationToken cancellationToken)
        {
            using var response = await client.PostAsJsonAsync($"api/issues/{id}/start",
                new StartIssueRequest(assignee), cancellationToken);

            return await ReadRequiredAsync<IssueResponse>(response, cancellationToken);
        }

        public async Task<IssueResponse> CloseAsync(int id, CancellationToken cancellationToken)
        {
            using var response = await client.PostAsync($"api/issues/{id}/close",
               content: null, cancellationToken);

            return await ReadRequiredAsync<IssueResponse>(response, cancellationToken);
        }

        // Checks the status code, then reads the JSON body; an empty body is an error rather than a hidden null.
        private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            await EnsureSuccessAsync(response, cancellationToken);

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new IssueApiException(
                    $"{Describe(response)} returned an empty body",
                    response.StatusCode,
                    responseBody: null);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new IssueApiException(
                $"{Describe(response)} returned {(int)response.StatusCode}",
                response.StatusCode,
                body
            );
        }

        private static string Describe(HttpResponseMessage response) =>
            $"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}";
    }
}
