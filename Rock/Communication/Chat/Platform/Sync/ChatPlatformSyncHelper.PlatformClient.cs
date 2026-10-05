// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Session;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The transport to the chat platform, in its own file because it owns an HttpClient and every
    /// other part of the helper is stateless.
    /// </summary>
    public static partial class ChatPlatformSyncHelper
    {
        /// <summary>
        /// A conversation with the chat platform over one HttpClient: the credential exchange, the
        /// submission and status reads for one sync run, and the pushes the immediate sync keeps
        /// one client for the process to send.
        /// </summary>
        /// <remarks>
        /// Nothing here throws at its caller over an answer. The platform refuses with a 422 and its
        /// history row committed, so the body of a 422 is read like any other.
        /// </remarks>
        internal sealed class PlatformClient : IDisposable
        {
            // The submission surface takes the body as one raw text value; a JSON content type is
            // parsed on the way in and the function is never reached.
            private const string SubmitPath = "/rest/v1/rpc/sync_submit";

            private const string StatusPath = "/rest/v1/rpc/sync_status";

            private const string PushPath = "/rest/v1/rpc/sync_push";

            private const string ExchangePath = "/functions/v1/token-exchange";

            private const string UnregisterDevicePath = "/rest/v1/rpc/chat_unregister_device";

            // Estimates, revisited when the platform is measured at full scale.
            internal const int TransportAttempts = 3;

            private static readonly TimeSpan TransportRetryDelay = TimeSpan.FromSeconds( 2 );

            // A drain pass holds the church's pending row for about a second at the largest church
            // measured and up to about nine at five times it, so the two waits the attempts leave
            // outlast the longer. Estimates, from local measurement.
            private static readonly TimeSpan BusyRetryDelay = TimeSpan.FromSeconds( 5 );

            // An operator's backoff hours out is saved by the run for the schedule to honour, not
            // waited for inside it.
            internal static readonly TimeSpan BusyRetryCeiling = TimeSpan.FromSeconds( 30 );

            // HttpClient's own default, named so the Sync Now budget can count it.
            internal static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds( 100 );

            // A platform token lasts about five minutes, and slow submission attempts followed by the
            // poll can outlast it. An estimate: enough for one more request to arrive before expiry.
            private static readonly TimeSpan TokenRefreshMargin = TimeSpan.FromSeconds( 60 );

            // Closer than this is the time an answer took to arrive, not a clock worth naming.
            private static readonly TimeSpan ClockSkewWorthReporting = TimeSpan.FromSeconds( 30 );

            // Postgres's code for a statement cancelled by its timeout. The data API runs the call in
            // one transaction, so it rolled back whole and the same submission id may be sent again.
            private const string StatementTimeoutCode = "57014";

            private readonly ChatPlatformConfiguration _configuration;

            private readonly HttpMessageHandler _handler;

            private readonly HttpClient _httpClient;

            // A wait rather than a lock, because a push's sign-in is awaited, so pushes that arrive
            // together share one exchange.
            private readonly SemaphoreSlim _signIn = new SemaphoreSlim( 1, 1 );

            /// <summary>
            /// Creates the client.
            /// </summary>
            /// <param name="configuration">The church's chat settings.</param>
            /// <param name="handler">The transport, or null for the ordinary one.</param>
            public PlatformClient( ChatPlatformConfiguration configuration, HttpMessageHandler handler = null )
            {
                if ( configuration == null )
                {
                    throw new ArgumentNullException( nameof( configuration ) );
                }

                _configuration = configuration;
                _handler = handler;
                _httpClient = handler == null ? new HttpClient() : new HttpClient( handler );
                _httpClient.Timeout = RequestTimeout;

                Wait = duration => Thread.Sleep( duration );
                Clock = () => DateTime.UtcNow;
            }

            /// <summary>
            /// How the client waits. Replaced where a caller does not want a real wait.
            /// </summary>
            public Action<TimeSpan> Wait { get; set; }

            /// <summary>
            /// Where the client reads the time, for the elapsed half of a poll budget.
            /// </summary>
            public Func<DateTime> Clock { get; set; }

            /// <summary>
            /// The platform token the data calls are made under; never the church token, which the
            /// data API cannot verify.
            /// </summary>
            public string PlatformToken { get; internal set; }

            /// <summary>
            /// When the platform token expires by <see cref="Clock"/>, or null where the exchange did
            /// not say, in which case the token is never exchanged again.
            /// </summary>
            public DateTime? PlatformTokenExpiresAtUtc { get; internal set; }

            /// <summary>
            /// Mints the church's sync token and exchanges it for a platform token.
            /// </summary>
            /// <param name="failure">Why there is no token, when there is none.</param>
            /// <returns>True where the client now holds a platform token.</returns>
            /// <remarks>
            /// Called after the church is read, because a church token lasts minutes and the platform
            /// will not exchange one with under two left, and again by the client itself when the
            /// platform token is near its end.
            /// </remarks>
            public bool SignIn( out string failure )
            {
                return Exchange( MintChurchToken(), out failure );
            }

            /// <summary>
            /// Exchanges a church token for a platform token.
            /// </summary>
            /// <param name="churchToken">The church's sync token, or a person's token for a call made as that person.</param>
            /// <param name="failure">Why there is no token, when there is none.</param>
            /// <returns>True where the client now holds a platform token.</returns>
            public bool Exchange( string churchToken, out string failure )
            {
                PlatformToken = null;
                PlatformTokenExpiresAtUtc = null;

                if ( churchToken.IsNullOrWhiteSpace() )
                {
                    failure = "this church could not sign a request to the chat platform";
                    return false;
                }

                string refusal = null;

                // The same attempts as a submission: the exchange changes nothing, so it is always
                // safe to repeat.
                var unreached = SendWithRetry(
                    () => BuildExchangeRequest( churchToken ),
                    ( response, statusCode, body ) =>
                    {
                        if ( AcceptPlatformToken( response, body ) )
                        {
                            return;
                        }

                        var code = ( string ) body?["error"]?["code"];
                        refusal = "the chat platform refused this church's credential: "
                            + ( code.IsNotNullOrWhiteSpace() ? code : "HTTP " + statusCode )
                            + DescribeClockSkew( code, response.Headers.Date );
                    } );

                failure = unreached == null ? refusal : "the chat platform could not be reached to exchange this church's credential: " + unreached;
                return PlatformToken != null;
            }

            /// <summary>
            /// Takes one push token out of push, under the person's platform token this client
            /// exchanged for, so the platform removes it only where that person holds it.
            /// </summary>
            /// <param name="deviceToken">The push token.</param>
            /// <returns>True where it was removed, false where the person held no such token, null where the call failed.</returns>
            public bool? UnregisterDevice( string deviceToken )
            {
                bool? removed = null;
                var body = new JObject { ["p_token"] = deviceToken }.ToString( Formatting.None );

                // Safe to repeat: a second unregister of the same token removes nothing.
                SendWithRetry(
                    () => BuildDataRequest( UnregisterDevicePath, new StringContent( body, Encoding.UTF8, "application/json" ) ),
                    ( response, statusCode, answer ) =>
                    {
                        if ( response.IsSuccessStatusCode && answer?["removed"]?.Type == JTokenType.Boolean )
                        {
                            removed = ( bool ) answer["removed"];
                        }
                    } );

                return removed;
            }

            /// <summary>
            /// Sends a request until an answer arrives that says the call completed, waiting between
            /// attempts, and reads that answer.
            /// </summary>
            /// <param name="buildRequest">Builds the request for one attempt, because a request message cannot be sent twice.</param>
            /// <param name="read">Reads a completed answer from the response, its status code and its body.</param>
            /// <returns>Why the last attempt got no completed answer, or null where one did and was read.</returns>
            private string SendWithRetry( Func<HttpRequestMessage> buildRequest, Action<HttpResponseMessage, int, JObject> read )
            {
                string lastFailure = null;
                var retryDelay = TransportRetryDelay;

                for ( var attempt = 0; attempt < TransportAttempts; attempt++ )
                {
                    if ( attempt > 0 )
                    {
                        Wait( retryDelay );
                    }

                    retryDelay = TransportRetryDelay;

                    HttpResponseMessage response;
                    try
                    {
                        using ( var request = buildRequest() )
                        {
                            response = _httpClient.SendAsync( request ).GetAwaiter().GetResult();
                        }
                    }
                    catch ( Exception exception )
                    {
                        // The innermost message, because the wrappers above it name this code rather
                        // than what went wrong.
                        var innermost = exception;
                        while ( innermost.InnerException != null )
                        {
                            innermost = innermost.InnerException;
                        }

                        lastFailure = innermost.Message;
                        continue;
                    }

                    using ( response )
                    {
                        var statusCode = ( int ) response.StatusCode;
                        var body = ReadBody( response );

                        var unfinished = DescribeUnfinishedAnswer( statusCode, body );
                        if ( unfinished != null )
                        {
                            lastFailure = unfinished;
                            continue;
                        }

                        // Busy is the platform's own answer, so the last attempt's is read like any
                        // other and the run can name it.
                        var busyDelay = BusyRetryDelayFor( statusCode, body );
                        var isLastAttempt = attempt == TransportAttempts - 1;
                        if ( busyDelay.HasValue && !isLastAttempt )
                        {
                            retryDelay = busyDelay.Value;
                            continue;
                        }

                        read( response, statusCode, body );
                        return null;
                    }
                }

                return lastFailure;
            }

            /// <summary>
            /// Exchanges again when the platform token is near its end, keeping the token held where
            /// the exchange fails, since it may still be good for the request about to be made.
            /// </summary>
            private void RefreshTokenIfExpiring()
            {
                var isExpiring = PlatformTokenExpiresAtUtc.HasValue && PlatformTokenExpiresAtUtc.Value - Clock() < TokenRefreshMargin;
                if ( !isExpiring )
                {
                    return;
                }

                var heldToken = PlatformToken;
                var heldExpiry = PlatformTokenExpiresAtUtc;

                if ( !SignIn( out _ ) )
                {
                    PlatformToken = heldToken;
                    PlatformTokenExpiresAtUtc = heldExpiry;
                }
            }

            /// <summary>
            /// Where a refusal is about the church token's times, how far this server's clock is from
            /// the platform's, as a clause; otherwise an empty string.
            /// </summary>
            /// <remarks>
            /// The church token's times come from this server's clock, so a clock minutes out is
            /// refused on every run, and the code alone does not say the clock is what to fix.
            /// </remarks>
            private string DescribeClockSkew( string code, DateTimeOffset? platformTime )
            {
                var isAboutTokenTimes = code == "auth.stale_token" || code == "auth.invalid_token";
                if ( !isAboutTokenTimes || !platformTime.HasValue )
                {
                    return string.Empty;
                }

                var offset = platformTime.Value.UtcDateTime - Clock();
                if ( offset.Duration() <= ClockSkewWorthReporting )
                {
                    return string.Empty;
                }

                return string.Format( CultureInfo.InvariantCulture,
                    ", and this server's clock is {0:0} s {1} the chat platform",
                    offset.Duration().TotalSeconds,
                    offset > TimeSpan.Zero ? "behind" : "ahead of" );
            }

            /// <summary>
            /// Why an answer says the call never completed, so the same request may be sent again, or
            /// null where it is an answer to read.
            /// </summary>
            private static string DescribeUnfinishedAnswer( int statusCode, JObject body )
            {
                if ( statusCode == 502 || statusCode == 503 || statusCode == 504 )
                {
                    return "it answered HTTP " + statusCode;
                }

                if ( statusCode == 500 && ( string ) body?["code"] == StatementTimeoutCode )
                {
                    return "it was busy and cancelled the call at its statement timeout (" + StatementTimeoutCode + ")";
                }

                return null;
            }

            /// <summary>
            /// How long to wait before sending again where the drain held the church's pending row
            /// when the submission arrived, or null where the answer is not that one: long enough to
            /// outlast a drain pass, or until the time the answer advises, up to a ceiling.
            /// </summary>
            private TimeSpan? BusyRetryDelayFor( int statusCode, JObject body )
            {
                var isBusy = statusCode == 422 && ( string ) body?["error_code"] == BusyCode;
                if ( !isBusy )
                {
                    return null;
                }

                var advisedUntil = ReadTime( body["sync_backoff_until"] );
                var untilAdvised = advisedUntil.HasValue ? advisedUntil.Value.UtcDateTime - Clock() : TimeSpan.Zero;

                if ( untilAdvised <= BusyRetryDelay )
                {
                    return BusyRetryDelay;
                }

                return untilAdvised < BusyRetryCeiling ? untilAdvised : BusyRetryCeiling;
            }

            /// <summary>
            /// Submits one restatement and reads the acknowledgement, whatever status it arrives with.
            /// </summary>
            /// <param name="submissionId">The idempotency key, used for every attempt.</param>
            /// <param name="payload">The body, as the UTF-8 bytes it was written as.</param>
            /// <param name="headers">The submission headers.</param>
            /// <returns>The acknowledgement. Never null, and never an exception.</returns>
            /// <remarks>
            /// The same id on every attempt: a request that timed out may have arrived, and the
            /// platform answers a repeated id with the outcome it already recorded. A busy answer
            /// recorded nothing, so the same id is ingested once the drain lets go.
            /// </remarks>
            public Acknowledgement Submit( Guid submissionId, ArraySegment<byte> payload, IDictionary<string, string> headers )
            {
                Acknowledgement acknowledgement = null;

                var unreached = SendWithRetry(
                    () =>
                    {
                        RefreshTokenIfExpiring();

                        // Built per attempt, because a request message cannot be sent twice; the body
                        // buffer is wrapped, not copied.
                        var content = new ByteArrayContent( payload.Array, payload.Offset, payload.Count );
                        content.Headers.ContentType = new MediaTypeHeaderValue( "text/plain" ) { CharSet = "utf-8" };

                        var request = BuildDataRequest( SubmitPath, content );

                        foreach ( var header in headers )
                        {
                            request.Headers.TryAddWithoutValidation( header.Key, header.Value );
                        }

                        request.Headers.TryAddWithoutValidation( SubmissionIdHeader, submissionId.ToString() );

                        return request;
                    },
                    ( response, statusCode, body ) => acknowledgement = ReadAcknowledgement( submissionId, statusCode, body ) );

                return acknowledgement ?? new Acknowledgement { SubmissionId = submissionId, TransportDetail = unreached };
            }

            /// <summary>
            /// Reads what became of a submission, waiting inside the budget for the queue to reach it.
            /// </summary>
            /// <param name="submissionId">The submission to read.</param>
            /// <param name="budget">How long to keep asking.</param>
            /// <returns>The outcome, or null where nothing readable came back at all.</returns>
            public Outcome Poll( Guid submissionId, PollBudget budget )
            {
                var start = Clock();
                Outcome outcome = null;
                var attempts = 0;

                while ( true )
                {
                    // A read that could not be made leaves the last one standing.
                    var read = ReadStatus( submissionId );
                    if ( read != null )
                    {
                        outcome = read;
                    }

                    attempts++;

                    if ( outcome != null && outcome.Status.HasValue && outcome.Status.Value != SubmissionStatus.Accepted )
                    {
                        return outcome;
                    }

                    if ( attempts >= budget.MaxAttempts )
                    {
                        return outcome;
                    }

                    Wait( budget.Interval );

                    if ( Clock() - start >= budget.Duration )
                    {
                        return outcome;
                    }
                }
            }

            /// <summary>
            /// Reads what became of a submission, once.
            /// </summary>
            /// <param name="submissionId">The submission to read.</param>
            /// <returns>The outcome, or null where nothing readable came back.</returns>
            public Outcome ReadStatus( Guid submissionId )
            {
                RefreshTokenIfExpiring();

                var body = new JObject { ["p_submission_id"] = submissionId.ToString() };
                var content = new StringContent( body.ToString( Formatting.None ), Encoding.UTF8, "application/json" );

                HttpResponseMessage response;
                try
                {
                    response = _httpClient.SendAsync( BuildDataRequest( StatusPath, content ) ).GetAwaiter().GetResult();
                }
                catch
                {
                    // The run falls back to what the acknowledgement already told it.
                    return null;
                }

                using ( response )
                {
                    // A status this build does not know is no read at all.
                    var outcome = ReadOutcome( ReadBody( response ), submissionId );

                    return outcome != null && outcome.Status.HasValue ? outcome : null;
                }
            }

            /// <summary>
            /// Pushes the rows one save touched, once, signing in first where the token held is near
            /// its end.
            /// </summary>
            /// <param name="push">The rows and the moment they were read at.</param>
            /// <param name="cancellationToken">Ends the push when its time is up.</param>
            /// <returns>Applied where the platform took the push, and Pending otherwise. Never an exception.</returns>
            /// <remarks>
            /// One attempt and no waits: the full sync repairs a push that did not land, so waiting
            /// to send it again would only hold a thread.
            /// </remarks>
            public async Task<PushOutcome> PushAsync( PushBody push, CancellationToken cancellationToken )
            {
                try
                {
                    if ( !await EnsureSignedInAsync( cancellationToken ).ConfigureAwait( false ) )
                    {
                        return PushOutcome.Pending;
                    }

                    // JSON, unlike a submission's raw text, because a push is a few rows and the
                    // platform parses it on the way in.
                    var content = new StringContent( push.Body.ToString( Formatting.None ), Encoding.UTF8, "application/json" );

                    using ( var request = BuildDataRequest( PushPath, content ) )
                    {
                        request.Headers.TryAddWithoutValidation( ReadTimeHeader, FormatReadTime( push.ReadAtUtc ) );
                        request.Headers.TryAddWithoutValidation( ContractHeader, ChatWireContract.ComputedHash );

                        using ( var response = await _httpClient.SendAsync( request, cancellationToken ).ConfigureAwait( false ) )
                        {
                            if ( response.StatusCode == HttpStatusCode.OK )
                            {
                                return PushOutcome.Applied;
                            }

                            // A refusal names its code in the message, as the data API reports a raise.
                            var body = await ReadBodyAsync( response ).ConfigureAwait( false );
                            var code = ( string ) body?["message"] ?? ( string ) body?["code"] ?? "HTTP " + ( int ) response.StatusCode;

                            LogPushFailure( new InvalidOperationException( "The chat platform refused an immediate chat sync push: " + code ) );

                            return PushOutcome.Pending;
                        }
                    }
                }
                catch ( Exception exception )
                {
                    LogPushFailure( exception );
                    return PushOutcome.Pending;
                }
            }

            /// <summary>
            /// A response body as JSON, read without blocking, or null where it is not JSON. Dates are
            /// left as written, because Json.NET would turn an instant with an offset into a local time.
            /// </summary>
            private static async Task<JObject> ReadBodyAsync( HttpResponseMessage response )
            {
                var text = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait( false );
                if ( text.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                try
                {
                    using ( var reader = new JsonTextReader( new StringReader( text ) ) { DateParseHandling = DateParseHandling.None } )
                    {
                        return JObject.Load( reader );
                    }
                }
                catch ( JsonException )
                {
                    // A gateway that answered in its own words rather than the project's.
                    return null;
                }
            }

            /// <summary>
            /// Whether the client holds a platform token with enough time left for a request to
            /// arrive before it expires.
            /// </summary>
            private bool HasFreshToken
            {
                get
                {
                    var expiresAt = PlatformTokenExpiresAtUtc;

                    return PlatformToken != null && ( !expiresAt.HasValue || expiresAt.Value - Clock() >= TokenRefreshMargin );
                }
            }

            /// <summary>
            /// Makes sure the client holds a token with time left on it for a push, exchanging once
            /// and without waiting between attempts, since a push has seconds to land.
            /// </summary>
            private async Task<bool> EnsureSignedInAsync( CancellationToken cancellationToken )
            {
                if ( HasFreshToken )
                {
                    return true;
                }

                await _signIn.WaitAsync( cancellationToken ).ConfigureAwait( false );

                try
                {
                    if ( HasFreshToken )
                    {
                        return true;
                    }

                    var churchToken = MintChurchToken();
                    if ( churchToken == null )
                    {
                        LogPushFailure( new InvalidOperationException( "This church could not sign a request to the chat platform, so an immediate chat sync push was not sent." ) );
                        return false;
                    }

                    using ( var request = BuildExchangeRequest( churchToken ) )
                    using ( var response = await _httpClient.SendAsync( request, cancellationToken ).ConfigureAwait( false ) )
                    {
                        var body = await ReadBodyAsync( response ).ConfigureAwait( false );
                        if ( AcceptPlatformToken( response, body ) )
                        {
                            return true;
                        }

                        var code = ( string ) body?["error"]?["code"] ?? "HTTP " + ( int ) response.StatusCode;
                        LogPushFailure( new InvalidOperationException( "The chat platform refused this church's credential for an immediate chat sync push: " + code ) );
                        return false;
                    }
                }
                finally
                {
                    _signIn.Release();
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _httpClient.Dispose();
            }

            /// <summary>
            /// Whether this client was built for these settings and this transport, so the immediate
            /// sync may keep sending through it.
            /// </summary>
            /// <param name="configuration">The church's chat settings as just read.</param>
            /// <param name="handler">The transport a test holds, or null.</param>
            /// <returns>True where nothing the client was built from has changed.</returns>
            internal bool IsFor( ChatPlatformConfiguration configuration, HttpMessageHandler handler )
            {
                return _handler == handler
                    && _configuration.TenantId == configuration.TenantId
                    && _configuration.ProjectUrl == configuration.ProjectUrl
                    && _configuration.PublishableKey == configuration.PublishableKey
                    && _configuration.Kid == configuration.Kid
                    && _configuration.PrivateKey == configuration.PrivateKey;
            }

            /// <summary>
            /// The church's sync token, or null where this church cannot sign one.
            /// </summary>
            private string MintChurchToken()
            {
                var minted = ChatSessionHelper.TryMintSyncToken( new ChatSessionContext { Configuration = _configuration } );

                return minted.Success ? minted.ChurchToken : null;
            }

            /// <summary>
            /// Holds the platform token an exchange answered with, or holds nothing new where the
            /// answer carries none.
            /// </summary>
            /// <returns>True where the answer carried a token.</returns>
            private bool AcceptPlatformToken( HttpResponseMessage response, JObject body )
            {
                var token = ( string ) body?["access_token"];
                if ( !response.IsSuccessStatusCode || token.IsNullOrWhiteSpace() )
                {
                    return false;
                }

                // The expiry is set first, so a push that reads the token between the two writes
                // never sees a new token with the old token's expiry.
                var expiresIn = ( int? ) body["expires_in"];
                PlatformTokenExpiresAtUtc = expiresIn.HasValue ? Clock().AddSeconds( expiresIn.Value ) : ( DateTime? ) null;
                PlatformToken = token;

                return true;
            }

            /// <summary>
            /// The request that exchanges a church token for a platform token.
            /// </summary>
            private HttpRequestMessage BuildExchangeRequest( string churchToken )
            {
                var request = new HttpRequestMessage( HttpMethod.Post, Url( ExchangePath ) );
                request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + churchToken );
                request.Headers.TryAddWithoutValidation( "apikey", _configuration.PublishableKey );

                return request;
            }

            /// <summary>
            /// A data call with its body, carrying the project's key and the platform token.
            /// </summary>
            private HttpRequestMessage BuildDataRequest( string path, HttpContent content )
            {
                var request = new HttpRequestMessage( HttpMethod.Post, Url( path ) ) { Content = content };
                request.Headers.TryAddWithoutValidation( "apikey", _configuration.PublishableKey );
                request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + PlatformToken );

                return request;
            }

            /// <summary>
            /// A platform address, with one slash between the project and the path however the
            /// project url was typed.
            /// </summary>
            private string Url( string path )
            {
                return ( _configuration.ProjectUrl ?? string.Empty ).TrimEnd( '/' ) + path;
            }

            /// <summary>
            /// Reads the acknowledgement out of a response, for a refusal exactly as for an acceptance.
            /// </summary>
            private static Acknowledgement ReadAcknowledgement( Guid submissionId, int statusCode, JObject body )
            {
                if ( body == null )
                {
                    return new Acknowledgement
                    {
                        SubmissionId = submissionId,
                        HttpStatusCode = statusCode,
                        TransportDetail = "the chat platform answered with something that is not an acknowledgement"
                    };
                }

                // The submissions that cannot own a history row raise, and name their reason in the
                // error's message.
                var errorCode = ( string ) body["error_code"];
                if ( errorCode.IsNullOrWhiteSpace() )
                {
                    errorCode = ( string ) body["message"];
                }

                return new Acknowledgement
                {
                    SubmissionId = ReadGuid( body["submission_id"] ) ?? submissionId,
                    Status = ParseStatus( ( string ) body["status"] ),
                    ErrorCode = errorCode.IsNullOrWhiteSpace() ? null : errorCode,
                    HttpStatusCode = statusCode,
                    PreviousOutcome = ReadOutcome( body["previous_outcome"] as JObject, Guid.Empty ),
                    SyncBackoffUntil = ReadTime( body["sync_backoff_until"] )
                };
            }

            /// <summary>
            /// Reads one recorded outcome block, or null where there is none. A status read and the
            /// previous outcome an acknowledgement carries are the same shape.
            /// </summary>
            private static Outcome ReadOutcome( JObject outcome, Guid fallbackSubmissionId )
            {
                if ( outcome == null )
                {
                    return null;
                }

                return new Outcome
                {
                    SubmissionId = ReadGuid( outcome["submission_id"] ) ?? fallbackSubmissionId,
                    Status = ParseStatus( ( string ) outcome["status"] ),
                    ErrorCode = ( string ) outcome["error_code"]
                };
            }

            /// <summary>
            /// A response body as JSON, or null where it cannot be read or is not JSON.
            /// </summary>
            private static JObject ReadBody( HttpResponseMessage response )
            {
                try
                {
                    return ReadBodyAsync( response ).GetAwaiter().GetResult();
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// A guid from the wire, or null.
            /// </summary>
            private static Guid? ReadGuid( JToken token )
            {
                return Guid.TryParse( ( string ) token, out var parsed ) ? parsed : ( Guid? ) null;
            }

            /// <summary>
            /// An instant from the wire, or null. Parsed round-trip, so the platform's offset is kept.
            /// </summary>
            private static DateTimeOffset? ReadTime( JToken token )
            {
                var value = ( string ) token;
                if ( value.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                var isParsed = DateTimeOffset.TryParse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out var parsed );

                return isParsed ? parsed : ( DateTimeOffset? ) null;
            }
        }
    }
}
