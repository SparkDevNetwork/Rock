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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Data;
using Rock.Jobs;
using Rock.SystemKey;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Web;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The Chat Platform Sync job's whole run, against a stand-in for the platform.
    /// </summary>
    /// <remarks>
    /// Every part of the run has its own tests, and the run once shipped sending the church's own
    /// signed token where the platform's token belonged, with all of them green: nothing followed a
    /// token from the exchange to the submission. This does. The immediate sync signs in and sends
    /// through the same client, so it rests on this too.
    /// </remarks>
    [TestClass]
    public class ChatPlatformSyncRunTests : DatabaseTestsBase
    {
        private const string ProjectUrl = "https://example.supabase.co";

        private const string PublishableKey = "sb_publishable_test";

        private const string ExchangedToken = "exchanged.platform.token";

        [TestMethod]
        public void ARunSignsInExchangesSubmitsAndPollsAndSendsTheExchangedTokenNotTheChurchToken()
        {
            var storedSetting = SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION );

            try
            {
                using ( var fixture = new ChatSyncProjectionFixture() )
                {
                    var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Run channel" );
                    fixture.AddMember( channel, fixture.AddPerson( "Runner" ) );

                    var handler = new PlatformStandIn();
                    ChatPlatformSync.RunResult result;

                    using ( var rockContext = new RockContext() )
                    {
                        result = ChatPlatformSync.Run( rockContext, Configuration(), false, handler );
                    }

                    CollectionAssert.AreEqual(
                        new[] { "/functions/v1/token-exchange", "/rest/v1/rpc/sync_submit", "/rest/v1/rpc/sync_status" },
                        handler.Requests.Select( r => r.Path ).ToList(),
                        "a run signs in once, submits once, and reads its own outcome" );

                    var exchange = handler.Requests[0];
                    var churchToken = exchange.Bearer;
                    Assert.IsFalse( string.IsNullOrEmpty( churchToken ), "the exchange carries the church's signed token" );
                    Assert.AreNotEqual( ExchangedToken, churchToken );

                    Assert.AreEqual( ExchangedToken, handler.Requests[1].Bearer,
                        "the submission carries the token the exchange granted, never the church's own" );
                    Assert.AreEqual( ExchangedToken, handler.Requests[2].Bearer,
                        "and so does the read of its outcome" );
                    Assert.IsFalse( result.IsFailure, result.Message );
                }
            }
            finally
            {
                // A run saves the platform's backoff advice into the stored settings.
                SystemSettings.SetValue( SystemSetting.CHAT_PLATFORM_CONFIGURATION, storedSetting );
            }
        }

        #region Support

        private static ChatPlatformConfiguration Configuration()
        {
            const string kid = "kid-run-test";

            return new ChatPlatformConfiguration
            {
                TenantId = Guid.NewGuid(),
                ProjectUrl = ProjectUrl,
                PublishableKey = PublishableKey,
                Kid = kid,
                PrivateKey = CreatePrivateJwk( kid ),
                AreChatProfilesVisible = true,
                IsOpenDirectMessagingAllowed = true,
                ChatBadgeDataViewGuids = new List<Guid>()
            };
        }

        private static string CreatePrivateJwk( string kid )
        {
            using ( var ecdsa = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            {
                var key = new ECDsaSecurityKey( ecdsa ) { KeyId = kid };
                var jwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey( key );
                jwk.Kid = kid;
                jwk.Use = "sig";
                jwk.Alg = "ES256";
                return JsonConvert.SerializeObject( jwk );
            }
        }

        /// <summary>
        /// Answers each platform call by its path, the way the platform would, and records it.
        /// </summary>
        private sealed class PlatformStandIn : HttpMessageHandler
        {
            public List<RecordedRequest> Requests { get; } = new List<RecordedRequest>();

            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                var path = request.RequestUri.AbsolutePath;
                var body = request.Content == null ? null : request.Content.ReadAsStringAsync().Result;
                var authorization = request.Headers.Authorization;

                Requests.Add( new RecordedRequest
                {
                    Path = path,
                    Bearer = authorization != null && authorization.Scheme == "Bearer" ? authorization.Parameter : null
                } );

                string answer;

                if ( path == "/functions/v1/token-exchange" )
                {
                    answer = "{\"access_token\":\"" + ExchangedToken + "\",\"token_type\":\"bearer\",\"expires_in\":300}";
                }
                else if ( path == "/rest/v1/rpc/sync_submit" )
                {
                    var submissionId = request.Headers.GetValues( "x-sync-submission-id" ).First();
                    answer = "{\"submission_id\":\"" + submissionId + "\",\"status\":\"accepted\",\"error_code\":null,"
                        + "\"previous_outcome\":null,\"sync_backoff_until\":null}";
                }
                else if ( path == "/rest/v1/rpc/sync_status" )
                {
                    var submissionId = ( string ) JObject.Parse( body )["p_submission_id"];
                    answer = "{\"submission_id\":\"" + submissionId + "\",\"status\":\"applied\",\"error_code\":null,"
                        + "\"drained_at\":\"2026-09-28T10:00:00+00:00\"}";
                }
                else
                {
                    return Task.FromResult( new HttpResponseMessage( HttpStatusCode.NotFound ) );
                }

                return Task.FromResult( new HttpResponseMessage( HttpStatusCode.OK )
                {
                    Content = new StringContent( answer, System.Text.Encoding.UTF8, "application/json" )
                } );
            }
        }

        private sealed class RecordedRequest
        {
            public string Path { get; set; }

            public string Bearer { get; set; }
        }

        #endregion Support
    }
}
