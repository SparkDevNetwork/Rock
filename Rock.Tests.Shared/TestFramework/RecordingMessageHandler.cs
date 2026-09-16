using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// An in-process <see cref="HttpMessageHandler"/> that lets a test stub
    /// canned responses for specific (HTTP verb, path) pairs and inspect the
    /// requests that were sent. Use it in place of a real HTTP server when
    /// unit-testing code that talks through an <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    /// Not thread-safe. Tests are expected to configure the handler on the
    /// test thread before invoking the system under test, and read
    /// <see cref="Requests"/> on the same thread after it returns.
    /// </remarks>
    public sealed class RecordingMessageHandler : HttpMessageHandler
    {
        private readonly List<Stub> _stubs = new List<Stub>();

        private readonly List<RecordedRequest> _requests = new List<RecordedRequest>();

        /// <summary>
        /// The requests observed by this handler in the order they were made.
        /// </summary>
        public IReadOnlyList<RecordedRequest> Requests => _requests;

        /// <summary>
        /// Configures a canned response for the given (method, path) pair.
        /// A later call for the same pair replaces the earlier stub.
        /// </summary>
        /// <param name="method">The HTTP verb to match.</param>
        /// <param name="path">The absolute URL path to match, including the leading slash.</param>
        /// <param name="statusCode">The HTTP status code to return.</param>
        /// <param name="body">Optional response body. When <c>null</c> the response carries an empty <c>Content</c> instead of no <c>Content</c>, mirroring what real HTTP responses do.</param>
        /// <param name="delay">Optional delay applied before the response is returned. Honors the caller's <see cref="CancellationToken"/>.</param>
        public void SetResponse( HttpMethod method, string path, HttpStatusCode statusCode, string body = null, TimeSpan? delay = null )
        {
            _stubs.RemoveAll( s => s.Method == method && string.Equals( s.Path, path, StringComparison.OrdinalIgnoreCase ) );

            _stubs.Add( new Stub
            {
                Method = method,
                Path = path,
                StatusCode = statusCode,
                Body = body,
                Delay = delay
            } );
        }

        /// <summary>
        /// Configures the handler to throw <paramref name="exception"/> for
        /// every request, mimicking a network-level failure regardless of
        /// which endpoint is invoked. Any previously-configured stubs are
        /// discarded.
        /// </summary>
        public void SetAlwaysThrow( Exception exception )
        {
            _stubs.Clear();

            _stubs.Add( new Stub
            {
                // Null Method / Path act as wildcards.
                Method = null,
                Path = null,
                Exception = exception
            } );
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            var body = request.Content != null
                ? await request.Content.ReadAsStringAsync().ConfigureAwait( false )
                : null;

            var headers = request.Headers.ToDictionary(
                h => h.Key,
                h => h.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase );

            _requests.Add( new RecordedRequest
            {
                Method = request.Method,
                Path = request.RequestUri.AbsolutePath,
                Body = body,
                Headers = headers
            } );

            var stub = _stubs.FirstOrDefault( s =>
                ( s.Method == null || s.Method == request.Method ) &&
                ( s.Path == null || string.Equals( s.Path, request.RequestUri.AbsolutePath, StringComparison.OrdinalIgnoreCase ) ) );

            if ( stub == null )
            {
                throw new InvalidOperationException( $"No stub configured for {request.Method} {request.RequestUri.AbsolutePath}." );
            }

            if ( stub.Delay.HasValue )
            {
                // Task.Delay honors the token, so a cancelled token surfaces
                // as OperationCanceledException from SendAsync -- exactly
                // what HttpClient's own network-layer cancellation does.
                await Task.Delay( stub.Delay.Value, cancellationToken ).ConfigureAwait( false );
            }

            if ( stub.Exception != null )
            {
                throw stub.Exception;
            }

            // Real HTTP responses always carry a Content object (even when
            // empty). Callers that read Content.ReadAsStringAsync
            // unconditionally would NRE if we left Content unset for empty-
            // body responses, so mirror the real-world shape here.
            var response = new HttpResponseMessage( stub.StatusCode )
            {
                Content = new StringContent( stub.Body ?? string.Empty )
            };

            return response;
        }

        private sealed class Stub
        {
            public HttpMethod Method;

            public string Path;

            public HttpStatusCode StatusCode;

            public string Body;

            public TimeSpan? Delay;

            public Exception Exception;
        }
    }

    /// <summary>
    /// A snapshot of a request that passed through <see cref="RecordingMessageHandler"/>.
    /// Held after the request has completed so tests can assert on what was
    /// actually sent.
    /// </summary>
    public sealed class RecordedRequest
    {
        public HttpMethod Method { get; set; }

        public string Path { get; set; }

        public string Body { get; set; }

        public IReadOnlyDictionary<string, string[]> Headers { get; set; }
    }
}
