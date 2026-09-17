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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Submits a restatement and polls its recorded outcome. A 4xx is a named
    /// body, not an exception: ingest returns 422 with the acknowledgement still
    /// in the body so a thrown status would drop the reason the job has to show.
    /// </summary>
    internal sealed class ChatSyncClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly ChatPlatformConfiguration _configuration;
        private readonly string _accessToken;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a client against the church's project. The caller owns the
        /// <see cref="HttpClient"/> lifetime.
        /// </summary>
        internal ChatSyncClient( HttpClient httpClient, ChatPlatformConfiguration configuration, string accessToken )
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _accessToken = accessToken;
        }

        #endregion

        #region Methods

        /// <summary>
        /// POSTs the raw body as <c>text/plain</c> with the submit headers.
        /// Reads the body on every status, including 4xx.
        /// </summary>
        internal Task<ChatSyncOutcome> SubmitAsync( ChatSyncSubmitRequest request )
        {
            var url = Combine( _configuration.ProjectUrl, "/rest/v1/rpc/sync_submit" );
            var message = new HttpRequestMessage( HttpMethod.Post, url );
            ApplyAuth( message );
            message.Headers.TryAddWithoutValidation( "x-sync-submission-id", request.SubmissionId.ToString( "D" ) );
            message.Headers.TryAddWithoutValidation( "x-sync-read-at", request.ReadAt );
            message.Headers.TryAddWithoutValidation( "x-sync-counts", request.CountsJson );
            message.Headers.TryAddWithoutValidation( "x-sync-marks", request.MarksJson );
            message.Headers.TryAddWithoutValidation( "x-sync-rock-version", request.RockVersion );
            message.Headers.TryAddWithoutValidation( "x-sync-contract", ChatWireContract.ComputedHash );
            if ( request.IsUrgent )
            {
                message.Headers.TryAddWithoutValidation( "x-sync-urgent", "true" );
            }

            message.Content = new StringContent( request.Body ?? string.Empty, Encoding.UTF8, "text/plain" );
            return SendAsync( message );
        }

        /// <summary>
        /// Reads one submission's recorded outcome. Same 4xx rule as submit.
        /// </summary>
        internal Task<ChatSyncOutcome> StatusAsync( Guid submissionId )
        {
            var url = Combine( _configuration.ProjectUrl, "/rest/v1/rpc/sync_status" );
            var message = new HttpRequestMessage( HttpMethod.Post, url );
            ApplyAuth( message );
            var json = string.Format( "{{\"p_submission_id\":\"{0}\"}}", submissionId.ToString( "D" ) );
            message.Content = new StringContent( json, Encoding.UTF8, "application/json" );
            return SendAsync( message );
        }

        #endregion

        #region Private Methods

        private void ApplyAuth( HttpRequestMessage message )
        {
            message.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", _accessToken );
            if ( !string.IsNullOrWhiteSpace( _configuration.PublishableKey ) )
            {
                message.Headers.TryAddWithoutValidation( "apikey", _configuration.PublishableKey );
            }

            message.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
        }

        private async Task<ChatSyncOutcome> SendAsync( HttpRequestMessage message )
        {
            var response = await _httpClient.SendAsync( message ).ConfigureAwait( false );
            var body = response.Content == null
                ? null
                : await response.Content.ReadAsStringAsync().ConfigureAwait( false );

            return Parse( ( int ) response.StatusCode, body );
        }

        /// <summary>
        /// Reads an acknowledgement or a raised error. Never throws on 4xx.
        /// </summary>
        internal static ChatSyncOutcome Parse( int httpStatus, string body )
        {
            var outcome = new ChatSyncOutcome
            {
                HttpStatus = httpStatus,
                RawBody = body
            };

            if ( string.IsNullOrWhiteSpace( body ) )
            {
                return outcome;
            }

            JObject json;
            try
            {
                json = JObject.Parse( body );
            }
            catch ( Exception )
            {
                return outcome;
            }

            var submission = json.Value<string>( "submission_id" );
            Guid parsed;
            if ( !string.IsNullOrWhiteSpace( submission ) && Guid.TryParse( submission, out parsed ) )
            {
                outcome.SubmissionId = parsed;
            }

            outcome.Status = json.Value<string>( "status" );
            outcome.ErrorCode = json.Value<string>( "error_code" )
                ?? json.Value<string>( "message" )
                ?? json.Value<string>( "code" );

            var backoff = json.Value<string>( "sync_backoff_until" );
            DateTime backoffUtc;
            if ( !string.IsNullOrWhiteSpace( backoff ) && DateTime.TryParse( backoff, null, System.Globalization.DateTimeStyles.RoundtripKind, out backoffUtc ) )
            {
                outcome.SyncBackoffUntil = backoffUtc.ToUniversalTime();
            }

            return outcome;
        }

        private static string Combine( string projectUrl, string path )
        {
            return ( projectUrl ?? string.Empty ).TrimEnd( '/' ) + path;
        }

        #endregion
    }

    /// <summary>
    /// The body and headers of one submit.
    /// </summary>
    internal sealed class ChatSyncSubmitRequest
    {
        public Guid SubmissionId { get; set; }

        public string ReadAt { get; set; }

        public string Body { get; set; }

        public string CountsJson { get; set; }

        public string MarksJson { get; set; }

        public string RockVersion { get; set; }

        public bool IsUrgent { get; set; }
    }
}
