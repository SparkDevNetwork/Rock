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
using System.Text;
using System.Threading;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Carries a restatement to the chat platform and brings back what happened to it.
    /// </summary>
    internal sealed class ChatSyncSubmitClient : IDisposable
    {
        #region Constants

        /// <summary>
        /// The submission surface.
        /// </summary>
        public const string SubmitPath = "/rest/v1/rpc/sync_submit";

        /// <summary>
        /// The surface a run reads its own outcome from.
        /// </summary>
        public const string StatusPath = "/rest/v1/rpc/sync_status";

        #endregion Constants

        #region Fields

        private readonly ChatPlatformConfiguration _configuration;

        private readonly Func<string> _tokenFactory;

        private readonly HttpClient _httpClient;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a client for one church.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="tokenFactory">Mints a sync-scope church token, asked once per request.</param>
        /// <param name="handler">The transport, or null for the ordinary one.</param>
        public ChatSyncSubmitClient( ChatPlatformConfiguration configuration, Func<string> tokenFactory, HttpMessageHandler handler = null )
        {
            _configuration = configuration;
            _tokenFactory = tokenFactory;
            _httpClient = handler == null ? new HttpClient() : new HttpClient( handler );

            Wait = d => Thread.Sleep( d );
            Clock = () => DateTime.UtcNow;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// How the client waits. Replaced in tests.
        /// </summary>
        public Action<TimeSpan> Wait { get; set; }

        /// <summary>
        /// Where the client reads the time. Replaced in tests.
        /// </summary>
        public Func<DateTime> Clock { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Submits one restatement.
        /// </summary>
        /// <param name="submissionId">The idempotency key for this submission.</param>
        /// <param name="payload">The body, already shaped.</param>
        /// <param name="headers">The submission headers, built elsewhere.</param>
        /// <returns>The acknowledgement.</returns>
        public ChatSyncAcknowledgement Submit( Guid submissionId, string payload, IDictionary<string, string> headers )
        {
            var request = new HttpRequestMessage( HttpMethod.Post, _configuration.ProjectUrl + SubmitPath )
            {
                Content = new StringContent( payload, Encoding.UTF8, "application/json" )
            };

            foreach ( var header in headers )
            {
                request.Headers.TryAddWithoutValidation( header.Key, header.Value );
            }

            request.Headers.TryAddWithoutValidation( "x-sync-submission-id", Guid.NewGuid().ToString() );
            request.Headers.TryAddWithoutValidation( "apikey", _configuration.PublishableKey );
            request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + _tokenFactory() );

            var response = _httpClient.SendAsync( request ).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var body = JObject.Parse( response.Content.ReadAsStringAsync().GetAwaiter().GetResult() );

            return new ChatSyncAcknowledgement
            {
                SubmissionId = submissionId,
                Status = ChatSyncOutcomeMapper.ParseStatus( ( string ) body["status"] ),
                ErrorCode = ( string ) body["error_code"],
                HttpStatusCode = ( int ) response.StatusCode
            };
        }

        /// <summary>
        /// Reads what became of a submission.
        /// </summary>
        /// <param name="submissionId">The submission to read.</param>
        /// <param name="budget">How long to keep asking.</param>
        /// <returns>The outcome, or null where nothing readable came back.</returns>
        public ChatSyncOutcome Poll( Guid submissionId, ChatSyncPollBudget budget )
        {
            var request = new HttpRequestMessage( HttpMethod.Post, _configuration.ProjectUrl + StatusPath )
            {
                Content = new StringContent( "{\"p_submission_id\":\"" + submissionId + "\"}", Encoding.UTF8, "application/json" )
            };

            request.Headers.TryAddWithoutValidation( "apikey", _configuration.PublishableKey );
            request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + _tokenFactory() );

            var response = _httpClient.SendAsync( request ).GetAwaiter().GetResult();
            var body = JObject.Parse( response.Content.ReadAsStringAsync().GetAwaiter().GetResult() );

            return new ChatSyncOutcome
            {
                SubmissionId = submissionId,
                Status = ChatSyncOutcomeMapper.ParseStatus( ( string ) body["status"] )
            };
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient.Dispose();
        }

        #endregion Methods
    }
}
