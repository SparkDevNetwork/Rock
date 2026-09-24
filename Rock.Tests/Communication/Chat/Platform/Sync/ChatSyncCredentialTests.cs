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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The credential a sync is made under. The church signs its own token, and the platform's
    /// data API cannot verify a church's signature, so that token is exchanged for a platform token
    /// first, exactly as a person's is. A sync sent under the church token itself is refused on
    /// every call.
    /// </summary>
    [TestClass]
    public class ChatSyncCredentialTests
    {
        private const string ProjectUrl = "https://example.supabase.co";
        private const string PublishableKey = "sb_publishable_test";
        private const string ChurchToken = "church.signed.token";
        private const string PlatformToken = "platform.signed.token";

        [TestMethod]
        public void Exchange_PostsTheChurchTokenAndReturnsThePlatformToken()
        {
            var handler = new ExchangeStub( HttpStatusCode.OK, "{\"access_token\":\"" + PlatformToken + "\",\"token_type\":\"bearer\",\"expires_in\":300}" );

            var token = ChatSyncCredential.Exchange( Configuration(), ChurchToken, out var failure, handler );

            Assert.AreEqual( PlatformToken, token );
            Assert.IsNull( failure );
            var request = handler.Requests.Single();
            Assert.AreEqual( HttpMethod.Post, request.Method );
            Assert.AreEqual( ProjectUrl + "/functions/v1/token-exchange", request.RequestUri.ToString() );
            Assert.AreEqual( "Bearer " + ChurchToken, string.Join( ",", request.Headers.GetValues( "Authorization" ) ) );
            Assert.AreEqual( PublishableKey, string.Join( ",", request.Headers.GetValues( "apikey" ) ) );
        }

        [TestMethod]
        public void Exchange_WhenRefused_ReturnsNoTokenAndThePlatformsReason()
        {
            var handler = new ExchangeStub( HttpStatusCode.Unauthorized, "{\"error\":{\"code\":\"auth.invalid_token\",\"message\":\"auth.invalid_token\"}}" );

            var token = ChatSyncCredential.Exchange( Configuration(), ChurchToken, out var failure, handler );

            Assert.IsNull( token );
            StringAssert.Contains( failure, "the chat platform refused this church's credential" );
            StringAssert.Contains( failure, "auth.invalid_token" );
        }

        [TestMethod]
        public void Exchange_WhenTheChurchCouldNotSign_CallsNothing()
        {
            var handler = new ExchangeStub( HttpStatusCode.OK, "{\"access_token\":\"" + PlatformToken + "\"}" );

            var token = ChatSyncCredential.Exchange( Configuration(), null, out var failure, handler );

            Assert.IsNull( token );
            Assert.IsNotNull( failure );
            Assert.AreEqual( 0, handler.Requests.Count );
        }

        #region Helpers

        private static ChatPlatformConfiguration Configuration()
        {
            return new ChatPlatformConfiguration
            {
                TenantId = Guid.NewGuid(),
                ProjectUrl = ProjectUrl + "/",
                PublishableKey = PublishableKey,
                PrivateKey = "{}"
            };
        }

        private sealed class ExchangeStub : HttpMessageHandler
        {
            private readonly HttpStatusCode _code;
            private readonly string _body;

            public ExchangeStub( HttpStatusCode code, string body )
            {
                _code = code;
                _body = body;
            }

            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                Requests.Add( request );

                return Task.FromResult( new HttpResponseMessage( _code )
                {
                    Content = new StringContent( _body, System.Text.Encoding.UTF8, "application/json" )
                } );
            }
        }

        #endregion Helpers
    }
}
