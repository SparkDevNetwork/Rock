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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Session;
using Rock.Data;

using static Rock.Tests.Communication.Chat.Platform.Session.ChatSessionFixture;

namespace Rock.Tests.Communication.Chat.Platform.Session
{
    /// <summary>
    /// Signing out of Rock takes this browser out of push. The Chat block is not on the page that
    /// signs out, so Rock makes the call the client cannot: it signs a token for the person signing
    /// out, exchanges it, and unregisters the one device token this browser kept in its cookie,
    /// leaving every other device of the person alone.
    /// </summary>
    [TestClass]
    public class ChatSessionHelperPushTests
    {
        private RockContext _rockContext;

        [TestInitialize]
        public void TestInitialize()
        {
            _rockContext = CreateRockContextMock().Object;
        }

        [TestMethod]
        public void UnregisterPushDevice_WithTheBrowsersToken_ExchangesThenUnregistersExactlyThatToken()
        {
            var handler = new RecordingHandler(
                () => Json( HttpStatusCode.OK, "{\"access_token\":\"platform-token\",\"token_type\":\"bearer\",\"expires_in\":300}" ),
                () => Json( HttpStatusCode.OK, "{\"token\":\"fcm-token-1\",\"removed\":true}" ) );

            var outcome = ChatSessionHelper.UnregisterPushDevice( Adult(), "fcm-token-1", SigningConfig(), _rockContext, handler );

            Assert.AreEqual( ChatPushUnregisterOutcome.Removed, outcome );
            Assert.AreEqual( 2, handler.Requests.Count );
            Assert.IsTrue( handler.Requests[0].Url.EndsWith( "/functions/v1/token-exchange" ), handler.Requests[0].Url );
            Assert.IsTrue( handler.Requests[1].Url.EndsWith( "/rest/v1/rpc/chat_unregister_device" ), handler.Requests[1].Url );
            Assert.AreEqual( "Bearer platform-token", handler.Requests[1].Authorization );
            Assert.IsTrue( JToken.DeepEquals( new JObject { ["p_token"] = "fcm-token-1" }, JObject.Parse( handler.Requests[1].Body ) ), handler.Requests[1].Body );
        }

        [TestMethod]
        public void UnregisterPushDevice_ATokenThePersonNoLongerHolds_IsNotRemoved()
        {
            var handler = new RecordingHandler(
                () => Json( HttpStatusCode.OK, "{\"access_token\":\"platform-token\",\"token_type\":\"bearer\",\"expires_in\":300}" ),
                () => Json( HttpStatusCode.OK, "{\"token\":\"fcm-token-1\",\"removed\":false}" ) );

            var outcome = ChatSessionHelper.UnregisterPushDevice( Adult(), "fcm-token-1", SigningConfig(), _rockContext, handler );

            Assert.AreEqual( ChatPushUnregisterOutcome.NotHeld, outcome );
        }

        [TestMethod]
        public void UnregisterPushDevice_NoTokenInTheBrowser_CallsNothing()
        {
            var handler = new RecordingHandler();

            foreach ( var token in new[] { null, "", "   " } )
            {
                Assert.AreEqual( ChatPushUnregisterOutcome.NoToken, ChatSessionHelper.UnregisterPushDevice( Adult(), token, SigningConfig(), _rockContext, handler ) );
            }

            Assert.AreEqual( 0, handler.Requests.Count );
        }

        [TestMethod]
        public void UnregisterPushDevice_NoPersonSignedIn_CallsNothing()
        {
            var handler = new RecordingHandler();

            var outcome = ChatSessionHelper.UnregisterPushDevice( null, "fcm-token-1", SigningConfig(), _rockContext, handler );

            Assert.AreEqual( ChatPushUnregisterOutcome.Refused, outcome );
            Assert.AreEqual( 0, handler.Requests.Count );
        }

        [TestMethod]
        public void UnregisterPushDevice_APersonTheGatesRefuse_CallsNothing()
        {
            var person = Adult();
            AddBanListMember( _rockContext, person.Id );
            var handler = new RecordingHandler();

            var outcome = ChatSessionHelper.UnregisterPushDevice( person, "fcm-token-1", SigningConfig(), _rockContext, handler );

            Assert.AreEqual( ChatPushUnregisterOutcome.Refused, outcome );
            Assert.AreEqual( 0, handler.Requests.Count );
        }

        [TestMethod]
        public void UnregisterPushDevice_AnExchangeThePlatformRefuses_FailsWithoutThrowing()
        {
            var handler = new RecordingHandler( () => Json( HttpStatusCode.Unauthorized, "{\"error\":{\"code\":\"auth.bad_credential\"}}" ) );

            var outcome = ChatSessionHelper.UnregisterPushDevice( Adult(), "fcm-token-1", SigningConfig(), _rockContext, handler );

            Assert.AreEqual( ChatPushUnregisterOutcome.Failed, outcome );
            Assert.IsFalse( handler.Requests.Any( r => r.Url.EndsWith( "/rest/v1/rpc/chat_unregister_device" ) ) );
        }

        [TestMethod]
        public void UnregisterPushDevice_APlatformThatCannotBeReached_FailsWithoutThrowing()
        {
            var handler = new RecordingHandler( () => throw new HttpRequestException( "the stub transport refused to connect" ) );

            var outcome = ChatSessionHelper.UnregisterPushDevice( Adult(), "fcm-token-1", SigningConfig(), _rockContext, handler, wait => { } );

            Assert.AreEqual( ChatPushUnregisterOutcome.Failed, outcome );
        }

        [TestMethod]
        public void UnregisterPushDevice_AnUnregisterThePlatformRefuses_FailsWithoutThrowing()
        {
            var handler = new RecordingHandler(
                () => Json( HttpStatusCode.OK, "{\"access_token\":\"platform-token\",\"token_type\":\"bearer\",\"expires_in\":300}" ),
                () => Json( ( HttpStatusCode ) 503, "{\"code\":\"PT503\",\"message\":\"rpc.maintenance\"}" ) );

            var outcome = ChatSessionHelper.UnregisterPushDevice( Adult(), "fcm-token-1", SigningConfig(), _rockContext, handler, wait => { } );

            Assert.AreEqual( ChatPushUnregisterOutcome.Failed, outcome );
        }

        #region Support

        private static HttpResponseMessage Json( HttpStatusCode code, string body )
        {
            return new HttpResponseMessage( code ) { Content = new StringContent( body, Encoding.UTF8, "application/json" ) };
        }

        /// <summary>
        /// A request as the transport saw it.
        /// </summary>
        private sealed class Recorded
        {
            public string Url { get; set; }

            public string Authorization { get; set; }

            public string Body { get; set; }
        }

        /// <summary>
        /// A transport that answers from a script, in turn, repeating the last answer, and records
        /// what it was asked.
        /// </summary>
        private sealed class RecordingHandler : HttpMessageHandler
        {
            private readonly Func<HttpResponseMessage>[] _answers;

            private int _calls;

            public RecordingHandler( params Func<HttpResponseMessage>[] answers )
            {
                _answers = answers;
            }

            public List<Recorded> Requests { get; } = new List<Recorded>();

            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                Requests.Add( new Recorded
                {
                    Url = request.RequestUri.ToString(),
                    Authorization = request.Headers.Authorization?.ToString(),
                    Body = request.Content?.ReadAsStringAsync().Result
                } );

                if ( _answers.Length == 0 )
                {
                    throw new InvalidOperationException( "no call was expected" );
                }

                var answer = _answers[Math.Min( _calls++, _answers.Length - 1 )];
                return Task.FromResult( answer() );
            }
        }

        #endregion Support
    }
}
