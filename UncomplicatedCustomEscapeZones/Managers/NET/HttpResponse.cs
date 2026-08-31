using System.Net;

namespace UncomplicatedEscapeZones.Managers.NET;

internal readonly struct HttpResponse
{
    internal HttpResponse(long statusCode, string body, string error)
    {
        StatusCode = statusCode;
        Body = body;
        Error = error;
    }

    /// <summary>
    ///     Gets the HTTP status code of the answer, or 0 if the server never answered
    /// </summary>
    public long StatusCode { get; }

    /// <summary>
    ///     Gets the body of the answer - can be null!
    /// </summary>
    public string Body { get; }

    /// <summary>
    ///     Gets the error given by the transport layer - can be null!
    /// </summary>
    public string Error { get; }

    /// <summary>
    ///     Gets whether the request reached the server and got an answer
    /// </summary>
    public bool Completed => StatusCode > 0;

    /// <summary>
    ///     Gets whether the server answered with a 2xx status code
    /// </summary>
    public bool IsSuccess => StatusCode is >= 200 and < 300;

    /// <summary>
    ///     Gets the <see cref="HttpStatusCode" /> of the answer
    /// </summary>
    public HttpStatusCode Status => Completed ? (HttpStatusCode)StatusCode : HttpStatusCode.ServiceUnavailable;

    /// <summary>
    ///     Gets a human readable reason of the outcome of the request
    /// </summary>
    public string Reason => Error ?? (Completed ? $"HTTP {StatusCode}" : "the server did not answer");
}