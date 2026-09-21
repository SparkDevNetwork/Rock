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
    internal sealed class ChatSyncSubmitClient : IDisposable
    {
        #region Constants

        public const string SubmitPath = "/rest/v1/rpc/sync_submit";

        public const string StatusPath = "/rest/v1/rpc/sync_status";

        private const int DefaultTransportAttempts = 3;

        private static readonly TimeSpan DefaultTransportRetryDelay = TimeSpan.FromSeconds( 2 );

        #endregion Constants

        #region Fields

        private const string SubmissionIdHeader = "x-sync-submission-id";

        private readonly ChatPlatformConfiguration _configuration;

        private readonly Func<string> _tokenFactory;

        private readonly HttpClient _httpClient;

        #endregion Fields

        #region Constructors

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

        public int TransportAttempts { get; set; }

        public TimeSpan TransportRetryDelay { get; set; }

        public Action<TimeSpan> Wait { get; set; }

        public Func<DateTime> Clock { get; set; }

        #endregion Properties

        #region Methods

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

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        #endregion Methods

        #region Private Methods

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

        private void AddCredentials( HttpRequestMessage request )
        {
            request.Headers.TryAddWithoutValidation( "apikey", _configuration.PublishableKey );
            request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + _tokenFactory() );
        }

        private string Url( string path )
        {
            return ( _configuration.ProjectUrl ?? string.Empty ).TrimEnd( '/' ) + path;
        }

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

        private static Guid? ReadGuid( JToken token )
        {
            var value = ( string ) token;

            Guid parsed;
            return Guid.TryParse( value, out parsed ) ? parsed : ( Guid? ) null;
        }

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

        private static bool IsResolved( ChatSyncOutcome outcome )
        {
            return outcome.Status.HasValue && outcome.Status.Value != ChatSyncSubmissionStatus.Accepted;
        }

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

        internal enum ChatSyncSubmissionStatus
        {
            Accepted,

            Refused,

            Applied,

            Failed
        }

        internal sealed class ChatSyncOutcome
        {
            public Guid SubmissionId { get; set; }

            public ChatSyncSubmissionStatus? Status { get; set; }

            public string ErrorCode { get; set; }
        }

        internal sealed class ChatSyncAcknowledgement
        {
            public Guid SubmissionId { get; set; }

            public ChatSyncSubmissionStatus? Status { get; set; }

            public string ErrorCode { get; set; }

            public int? HttpStatusCode { get; set; }

            public ChatSyncOutcome PreviousOutcome { get; set; }

            public DateTimeOffset? SyncBackoffUntil { get; set; }

            public string TransportDetail { get; set; }

            public bool IsTransportFailure => !HttpStatusCode.HasValue;

            public bool CarriesBackoffAdvice => !IsTransportFailure && Status.HasValue;

            public static ChatSyncAcknowledgement Unreachable( Guid submissionId, string detail )
            {
                return new ChatSyncAcknowledgement
                {
                    SubmissionId = submissionId,
                    TransportDetail = detail
                };
            }
        }

        internal sealed class ChatSyncPollBudget
        {
            public ChatSyncPollBudget( TimeSpan interval, int maxAttempts, TimeSpan duration )
            {
                Interval = interval;
                MaxAttempts = maxAttempts;
                Duration = duration;
            }

            public TimeSpan Interval { get; }

            public int MaxAttempts { get; }

            public TimeSpan Duration { get; }

            public static ChatSyncPollBudget Manual
            {
                get { return new ChatSyncPollBudget( TimeSpan.FromSeconds( 3 ), 20, TimeSpan.FromSeconds( 60 ) ); }
            }

            public static ChatSyncPollBudget Scheduled
            {
                get { return new ChatSyncPollBudget( TimeSpan.FromSeconds( 5 ), 6, TimeSpan.FromSeconds( 30 ) ); }
            }
        }

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
        public static string WireValueFor( ChatSyncSubmissionStatus status )
        {
            return status.ToString().ToLowerInvariant();
        }
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
