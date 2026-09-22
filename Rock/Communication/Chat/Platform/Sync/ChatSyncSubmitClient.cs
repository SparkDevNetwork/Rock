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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Carries a restatement to the chat platform and brings back what happened to it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Nothing here throws at its caller over an answer. The platform records a refusal on
    ///         its own history row and then sets the response status to say so, because the call
    ///         runs in one transaction and raising would take the record back with it. A client
    ///         that treated that status as an error would throw away the named reason sitting in
    ///         the body and leave the church with a run that failed for nothing it can read.
    ///     </para>
    ///     <para>
    ///         The submission id belongs to the caller and is used for every attempt. A request
    ///         that times out may or may not have arrived, and the platform answers a repeated id
    ///         with the outcome it already recorded; a fresh id would submit the same restatement
    ///         twice and write two history rows for one cycle.
    ///     </para>
    /// </remarks>
    internal sealed class ChatSyncSubmitClient : IDisposable
    {
        #region Constants

        /// <summary>
        /// The submission surface. It takes the whole request body as one raw text value, which is
        /// why the body is sent as text rather than as JSON: a JSON content type is parsed on the
        /// way in and the function is never reached.
        /// </summary>
        public const string SubmitPath = "/rest/v1/rpc/sync_submit";

        /// <summary>
        /// The surface a run reads its own outcome from.
        /// </summary>
        public const string StatusPath = "/rest/v1/rpc/sync_status";

        /// <summary>
        /// How many times one submission is attempted before the run gives up on the transport.
        /// An estimate, revisited when the platform is measured at full scale.
        /// </summary>
        private const int DefaultTransportAttempts = 3;

        /// <summary>
        /// How long to wait between transport attempts. An estimate, as above.
        /// </summary>
        private static readonly TimeSpan DefaultTransportRetryDelay = TimeSpan.FromSeconds( 2 );

        #endregion Constants

        #region Fields

        /// <summary>
        /// The idempotency key header. Set by this client rather than taken from a caller's
        /// dictionary, so a retry cannot quietly carry a different one.
        /// </summary>
        private const string SubmissionIdHeader = "x-sync-submission-id";

        private readonly ChatPlatformConfiguration _configuration;

        private readonly Func<string> _tokenFactory;

        private readonly HttpClient _httpClient;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a client for one church.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="tokenFactory">Mints a sync-scope church token. Asked once per request rather than once per client, because a church token is short lived and a run can outlast one.</param>
        /// <param name="handler">The transport, or null for the ordinary one.</param>
        public ChatSyncSubmitClient( ChatPlatformConfiguration configuration, Func<string> tokenFactory, HttpMessageHandler handler = null )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            if ( tokenFactory == null )
            {
                throw new ArgumentNullException( nameof( tokenFactory ) );
            }

            _configuration = configuration;
            _tokenFactory = tokenFactory;
            _httpClient = handler == null ? new HttpClient() : new HttpClient( handler );

            TransportAttempts = DefaultTransportAttempts;
            TransportRetryDelay = DefaultTransportRetryDelay;
            Wait = duration => Thread.Sleep( duration );
            Clock = () => DateTime.UtcNow;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// How many times one submission is attempted before the run gives up on the transport.
        /// </summary>
        public int TransportAttempts { get; set; }

        /// <summary>
        /// How long to wait between transport attempts.
        /// </summary>
        public TimeSpan TransportRetryDelay { get; set; }

        /// <summary>
        /// How the client waits. Replaced where a caller does not want a real wait.
        /// </summary>
        public Action<TimeSpan> Wait { get; set; }

        /// <summary>
        /// Where the client reads the time, for the elapsed half of a poll budget.
        /// </summary>
        public Func<DateTime> Clock { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Submits one restatement and reads the acknowledgement, whatever status it arrives with.
        /// </summary>
        /// <param name="submissionId">The idempotency key for this submission, used for every attempt.</param>
        /// <param name="payload">The body, already shaped, as the UTF-8 bytes it was written as.</param>
        /// <param name="headers">The submission headers, built elsewhere.</param>
        /// <returns>The acknowledgement. Never null, and never an exception.</returns>
        public ChatSyncAcknowledgement Submit( Guid submissionId, ArraySegment<byte> payload, IDictionary<string, string> headers )
        {
            var attempts = Math.Max( 1, TransportAttempts );
            string lastFailure = null;

            for ( var attempt = 0; attempt < attempts; attempt++ )
            {
                if ( attempt > 0 )
                {
                    Wait( TransportRetryDelay );
                }

                HttpResponseMessage response;
                try
                {
                    response = Send( BuildSubmitRequest( submissionId, payload, headers ) );
                }
                catch ( Exception exception )
                {
                    lastFailure = Describe( exception );
                    continue;
                }

                using ( response )
                {
                    return ReadAcknowledgement( submissionId, response );
                }
            }

            return ChatSyncAcknowledgement.Unreachable( submissionId, lastFailure );
        }

        /// <summary>
        /// Reads what became of a submission, waiting inside the budget for the queue to reach it.
        /// </summary>
        /// <param name="submissionId">The submission to read.</param>
        /// <param name="budget">How long to keep asking.</param>
        /// <returns>The outcome, or null where nothing readable came back at all.</returns>
        /// <remarks>
        /// An early read never errors: the platform commits the history row as accepted before the
        /// queue ever sees the submission, so there is always something to read. Two bounds rather
        /// than one, because a slow read would otherwise let a fixed number of tries run far past
        /// the time the caller was willing to spend.
        /// </remarks>
        public ChatSyncOutcome Poll( Guid submissionId, ChatSyncPollBudget budget )
        {
            var start = Clock();
            ChatSyncOutcome outcome = null;
            var attempts = 0;

            while ( true )
            {
                // A read that could not be made leaves the last one standing rather than ending
                // the wait, because one unlucky moment is not an answer.
                var read = ReadStatus( submissionId );
                if ( read != null )
                {
                    outcome = read;
                }

                attempts++;

                if ( outcome != null && IsResolved( outcome ) )
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
        public ChatSyncOutcome ReadStatus( Guid submissionId )
        {
            HttpResponseMessage response;
            try
            {
                response = Send( BuildStatusRequest( submissionId ) );
            }
            catch
            {
                // A run that cannot read its own outcome still made its submission. The caller
                // falls back to what the acknowledgement already told it.
                return null;
            }

            using ( response )
            {
                var body = ReadBody( response );
                if ( body == null )
                {
                    return null;
                }

                var status = ParseStatus( ( string ) body["status"] );
                if ( !status.HasValue )
                {
                    return null;
                }

                return new ChatSyncOutcome
                {
                    SubmissionId = ReadGuid( body["submission_id"] ) ?? submissionId,
                    Status = status,
                    ErrorCode = ( string ) body["error_code"]
                };
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient.Dispose();
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Builds one submission request. Called once per attempt, because a request message cannot
        /// be sent twice.
        /// </summary>
        private HttpRequestMessage BuildSubmitRequest( Guid submissionId, ArraySegment<byte> payload, IDictionary<string, string> headers )
        {
            // Wrapped rather than copied. Every attempt sends the buffer the body was written into;
            // only the wrapper is new, because a request message cannot be sent twice.
            var content = new ByteArrayContent( payload.Array ?? new byte[0], payload.Offset, payload.Count );
            content.Headers.ContentType = new MediaTypeHeaderValue( "text/plain" ) { CharSet = "utf-8" };

            var request = new HttpRequestMessage( HttpMethod.Post, Url( SubmitPath ) )
            {
                Content = content
            };

            if ( headers != null )
            {
                foreach ( var header in headers )
                {
                    if ( header.Key.Equals( SubmissionIdHeader, StringComparison.OrdinalIgnoreCase ) )
                    {
                        // The id is this client's to set, so that every attempt of one submission
                        // carries the same one.
                        continue;
                    }

                    request.Headers.TryAddWithoutValidation( header.Key, header.Value );
                }
            }

            request.Headers.TryAddWithoutValidation( SubmissionIdHeader, submissionId.ToString() );
            AddCredentials( request );

            return request;
        }

        /// <summary>
        /// The request that asks the platform what became of one submission.
        /// </summary>
        /// <param name="submissionId">The submission to ask about.</param>
        /// <returns>The request, credentialed.</returns>
        private HttpRequestMessage BuildStatusRequest( Guid submissionId )
        {
            var body = new JObject { ["p_submission_id"] = submissionId.ToString() };

            var request = new HttpRequestMessage( HttpMethod.Post, Url( StatusPath ) )
            {
                Content = new StringContent( body.ToString( Formatting.None ), Encoding.UTF8, "application/json" )
            };

            AddCredentials( request );

            return request;
        }

        /// <summary>
        /// The two things every call to the project carries: the key the project is addressed with,
        /// and the church token the call is made under.
        /// </summary>
        private void AddCredentials( HttpRequestMessage request )
        {
            request.Headers.TryAddWithoutValidation( "apikey", _configuration.PublishableKey );
            request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + _tokenFactory() );
        }

        /// <summary>
        /// A platform address, with exactly one slash between the project and the path however the
        /// administrator typed the project url.
        /// </summary>
        /// <param name="path">The path, which begins with a slash.</param>
        /// <returns>The address.</returns>
        private string Url( string path )
        {
            return ( _configuration.ProjectUrl ?? string.Empty ).TrimEnd( '/' ) + path;
        }

        /// <summary>
        /// Sends one request and waits for it.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        private HttpResponseMessage Send( HttpRequestMessage request )
        {
            try
            {
                return _httpClient.SendAsync( request ).GetAwaiter().GetResult();
            }
            catch ( AggregateException exception )
            {
                // Unwrapped so the caller sees what the transport said rather than the wrapper the
                // synchronous wait put around it.
                throw exception.InnerException ?? exception;
            }
        }

        /// <summary>
        /// Reads the acknowledgement out of a response, for a refusal exactly as for an acceptance.
        /// </summary>
        private ChatSyncAcknowledgement ReadAcknowledgement( Guid submissionId, HttpResponseMessage response )
        {
            var statusCode = ( int ) response.StatusCode;
            var body = ReadBody( response );

            if ( body == null )
            {
                return new ChatSyncAcknowledgement
                {
                    SubmissionId = submissionId,
                    HttpStatusCode = statusCode,
                    TransportDetail = "the chat platform answered with something that is not an acknowledgement"
                };
            }

            return new ChatSyncAcknowledgement
            {
                SubmissionId = ReadGuid( body["submission_id"] ) ?? submissionId,
                Status = ParseStatus( ( string ) body["status"] ),
                ErrorCode = ReadErrorCode( body ),
                HttpStatusCode = statusCode,
                PreviousOutcome = ReadOutcome( body["previous_outcome"] as JObject ),
                SyncBackoffUntil = ReadTime( body["sync_backoff_until"] )
            };
        }

        /// <summary>
        /// The named reason, from either shape the project can answer with: the acknowledgement's
        /// own field, or the error shape used by the four submissions that cannot own a history row
        /// and therefore raise instead of returning.
        /// </summary>
        private static string ReadErrorCode( JObject body )
        {
            var code = ( string ) body["error_code"];
            if ( code.IsNotNullOrWhiteSpace() )
            {
                return code;
            }

            var raised = ( string ) body["message"];

            return raised.IsNotNullOrWhiteSpace() ? raised : null;
        }

        /// <summary>
        /// Reads one recorded outcome, whether it came back from a status read or rode on an
        /// acknowledgement as the previous submission's.
        /// </summary>
        /// <param name="outcome">The block to read, or null.</param>
        /// <returns>The outcome, or null where there was no block.</returns>
        private static ChatSyncOutcome ReadOutcome( JObject outcome )
        {
            if ( outcome == null )
            {
                return null;
            }

            return new ChatSyncOutcome
            {
                SubmissionId = ReadGuid( outcome["submission_id"] ) ?? Guid.Empty,
                Status = ParseStatus( ( string ) outcome["status"] ),
                ErrorCode = ( string ) outcome["error_code"]
            };
        }

        /// <summary>
        /// Parses a response body, leaving every value as it was written.
        /// </summary>
        /// <remarks>
        /// Dates are deliberately not parsed on the way in. The reader's own date handling would
        /// turn a moment carrying an offset into a local time, which is the whole of what these
        /// values are for.
        /// </remarks>
        private static JObject ReadBody( HttpResponseMessage response )
        {
            string text;
            try
            {
                text = response.Content == null ? null : response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }

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
        /// A guid from the wire, or null where the value is missing or is not one.
        /// </summary>
        /// <param name="token">The value.</param>
        /// <returns>The guid, or null.</returns>
        private static Guid? ReadGuid( JToken token )
        {
            var value = ( string ) token;

            Guid parsed;
            return Guid.TryParse( value, out parsed ) ? parsed : ( Guid? ) null;
        }

        /// <summary>
        /// An instant from the wire, or null where the value is missing or cannot be read. Parsed
        /// round-trip, so the offset the platform wrote is the offset that comes back rather than
        /// this server's.
        /// </summary>
        /// <param name="token">The value.</param>
        /// <returns>The instant, or null.</returns>
        private static DateTimeOffset? ReadTime( JToken token )
        {
            var value = ( string ) token;
            if ( value.IsNullOrWhiteSpace() )
            {
                return null;
            }

            DateTimeOffset parsed;
            if ( !DateTimeOffset.TryParse( value, CultureInfo.InvariantCulture,
                     DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out parsed ) )
            {
                return null;
            }

            return parsed;
        }

        /// <summary>
        /// Whether the queue has finished with this submission, one way or the other.
        /// </summary>
        private static bool IsResolved( ChatSyncOutcome outcome )
        {
            return outcome.Status.HasValue && outcome.Status.Value != ChatSyncSubmissionStatus.Accepted;
        }

        /// <summary>
        /// What a transport failure is worth telling an administrator: the innermost message,
        /// because the wrappers above it name this code rather than what went wrong.
        /// </summary>
        /// <param name="exception">The failure.</param>
        /// <returns>The message.</returns>
        private static string Describe( Exception exception )
        {
            var innermost = exception;
            while ( innermost.InnerException != null )
            {
                innermost = innermost.InnerException;
            }

            return innermost.Message;
        }

        #endregion Private Methods

        #region What the platform said

        /// <summary>
        /// What the chat platform recorded about one submission.
        /// </summary>
        /// <remarks>
        /// The platform's own vocabulary, mirrored here so the four are a type rather than four
        /// strings. A submission is accepted when it passed the checks and was queued, refused when a
        /// check turned it away, applied when the queue drained it, and failed when draining it did not
        /// work often enough to give up. A value Rock does not know is never guessed at.
        /// </remarks>
        internal enum ChatSyncSubmissionStatus
        {
            Accepted,

            Refused,

            Applied,

            Failed
        }

        /// <summary>
        /// What became of one submission.
        /// </summary>
        /// <remarks>
        /// One shape for two readings: the answer to a direct status read, and the previous
        /// submission's result carried on every acknowledgement. They are the same four fields because
        /// the platform builds them from the same history row, and a job that had to hold two shapes
        /// for one fact would have two ways of describing it.
        /// </remarks>
        internal sealed class ChatSyncOutcome
        {
            /// <summary>
            /// The submission this describes.
            /// </summary>
            public Guid SubmissionId { get; set; }

            /// <summary>
            /// What was recorded, or null where the platform named a status this build of Rock does not
            /// know.
            /// </summary>
            public ChatSyncSubmissionStatus? Status { get; set; }

            /// <summary>
            /// The named reason a refused or failed submission carries. Null on the other two.
            /// </summary>
            public string ErrorCode { get; set; }
        }

        /// <summary>
        /// What came back from a submission.
        /// </summary>
        /// <remarks>
        /// Returned for a refusal exactly as for an acceptance, because the chat platform answers a
        /// refusal with a body and a status rather than an error. Nothing here is a reason to throw: a
        /// job has a result message to write and a backoff to honour, and it can do neither from a
        /// stack trace.
        /// </remarks>
        internal sealed class ChatSyncAcknowledgement
        {
            /// <summary>
            /// The submission this describes.
            /// </summary>
            public Guid SubmissionId { get; set; }

            /// <summary>
            /// What was recorded, or null where the platform named a status this build of Rock does not
            /// know.
            /// </summary>
            public ChatSyncSubmissionStatus? Status { get; set; }

            /// <summary>
            /// The named reason a refused or failed submission carries. Null on the other two.
            /// </summary>
            public string ErrorCode { get; set; }

            /// <summary>
            /// The HTTP status the answer arrived with, or null where no answer arrived.
            /// </summary>
            public int? HttpStatusCode { get; set; }

            /// <summary>
            /// The church's previous submission and what became of it.
            /// </summary>
            public ChatSyncOutcome PreviousOutcome { get; set; }

            /// <summary>
            /// The time the platform would rather not hear from this church before.
            /// </summary>
            public DateTimeOffset? SyncBackoffUntil { get; set; }

            /// <summary>
            /// Why the submission never got an answer. Null where one arrived.
            /// </summary>
            public string TransportDetail { get; set; }

            /// <summary>
            /// True where no answer arrived at all.
            /// </summary>
            public bool IsTransportFailure => !HttpStatusCode.HasValue;

            /// <summary>
            /// Whether what came back is the platform's own word, and so whether the backoff it
            /// carries, or the absence of one, is worth recording.
            /// </summary>
            /// <remarks>
            /// A submission that never arrived, or that was answered by something other than the
            /// platform, says nothing about the platform's load. Recording its silence as "no backoff"
            /// would lift advice the platform had given.
            /// </remarks>
            public bool CarriesBackoffAdvice => !IsTransportFailure && Status.HasValue;

            /// <summary>
            /// A submission that never reached the platform.
            /// </summary>
            /// <param name="submissionId">The submission that was attempted.</param>
            /// <param name="detail">What the transport said.</param>
            /// <returns>The acknowledgement.</returns>
            public static ChatSyncAcknowledgement Unreachable( Guid submissionId, string detail )
            {
                return new ChatSyncAcknowledgement
                {
                    SubmissionId = submissionId,
                    TransportDetail = detail
                };
            }
        }

        /// <summary>
        /// How long a run waits to learn what became of the submission it just made.
        /// </summary>
        internal sealed class ChatSyncPollBudget
        {
            /// <summary>
            /// Creates a budget.
            /// </summary>
            /// <param name="interval">How long to wait between reads.</param>
            /// <param name="maxAttempts">How many reads at most.</param>
            /// <param name="duration">How long the whole wait may take.</param>
            public ChatSyncPollBudget( TimeSpan interval, int maxAttempts, TimeSpan duration )
            {
                Interval = interval;
                MaxAttempts = maxAttempts;
                Duration = duration;
            }

            /// <summary>
            /// How long to wait between reads.
            /// </summary>
            public TimeSpan Interval { get; }

            /// <summary>
            /// How many reads at most.
            /// </summary>
            public int MaxAttempts { get; }

            /// <summary>
            /// How long the whole wait may take.
            /// </summary>
            public TimeSpan Duration { get; }

            /// <summary>
            /// The budget for a sync someone pressed a button for. Longer than the scheduled one,
            /// because a person is waiting and there is nothing to fall back on. Every figure here is
            /// an estimate, revisited when the platform is measured at full scale.
            /// </summary>
            public static ChatSyncPollBudget Manual
            {
                get { return new ChatSyncPollBudget( TimeSpan.FromSeconds( 3 ), 20, TimeSpan.FromSeconds( 60 ) ); }
            }

            /// <summary>
            /// The budget for a sync the schedule started. Shorter, because nobody is watching and the
            /// acknowledgement's previous outcome is there to fall back on. Every figure here is an
            /// estimate, revisited when the platform is measured at full scale.
            /// </summary>
            public static ChatSyncPollBudget Scheduled
            {
                get { return new ChatSyncPollBudget( TimeSpan.FromSeconds( 5 ), 6, TimeSpan.FromSeconds( 30 ) ); }
            }
        }

        /// <summary>
        /// Whether a sync run that saw this status worked.
        /// </summary>
        /// <param name="status">The recorded status.</param>
        /// <returns>True where the run worked.</returns>
        /// <remarks>
        /// Accepted counts as working. The restatement is stored and queued, and whether the queue
        /// has reached it yet is the drain's business rather than a fault of the run that submitted
        /// it.
        /// </remarks>
        public static bool IsJobSuccess( ChatSyncSubmissionStatus status )
        {
            switch ( status )
            {
                case ChatSyncSubmissionStatus.Accepted:
                case ChatSyncSubmissionStatus.Applied:
                    return true;
                case ChatSyncSubmissionStatus.Refused:
                case ChatSyncSubmissionStatus.Failed:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException( nameof( status ), status, "no run outcome is mapped for this recorded status" );
            }
        }
        /// <summary>
        /// The label the wire carries for this status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>The wire label.</returns>
        public static string WireValueFor( ChatSyncSubmissionStatus status )
        {
            return status.ToString().ToLowerInvariant();
        }
        /// <summary>
        /// The status a wire label names, or null where this build of Rock does not know it.
        /// </summary>
        /// <param name="value">The wire label.</param>
        /// <returns>The status, or null.</returns>
        /// <remarks>
        /// A label added to the platform's vocabulary after this build shipped reads as nothing
        /// rather than as the nearest guess. A run that reports it does not know beats a run that
        /// reports a success it has no evidence for.
        /// </remarks>
        public static ChatSyncSubmissionStatus? ParseStatus( string value )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return null;
            }
            ChatSyncSubmissionStatus parsed;
            if ( !Enum.TryParse( value, true, out parsed ) || !Enum.IsDefined( typeof( ChatSyncSubmissionStatus ), parsed ) )
            {
                return null;
            }
            // Enum.TryParse takes a number as well as a name, and a number is not a label the wire
            // could ever have carried.
            return WireValueFor( parsed ) == value.ToLowerInvariant() ? parsed : ( ChatSyncSubmissionStatus? ) null;
        }

        #endregion What the platform said
    }
}
