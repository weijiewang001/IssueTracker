using System.Net;

namespace IssueTracker.Client
{
    public class IssueApiException(string message, HttpStatusCode httpStatusCode, string? responseBody)
        : Exception(message)
    {
        public HttpStatusCode HttpStatusCode { get; } = httpStatusCode;

        public string? ResponseBody { get; } = responseBody;
    }
}
