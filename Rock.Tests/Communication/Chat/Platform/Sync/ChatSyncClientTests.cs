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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Submit and status calls read the body on 4xx, and map 200 / 422 onto
    /// the job outcome the way ingest records them.
    /// </summary>
    [TestClass]
    public class ChatSyncClientTests
    {
        [TestMethod]
        public void Parse_200Accepted_IsHttpSuccessAndPending()
        {
            var outcome = ChatSyncClient.Parse( 200, "{\"submission_id\":\"11111111-1111-4111-8111-111111111111\",\"status\":\"accepted\",\"error_code\":null}" );

            Assert.IsTrue( outcome.IsHttpSuccess );
            Assert.IsTrue( outcome.IsPending );
            Assert.IsFalse( outcome.IsFailure );
            Assert.AreEqual( "accepted", outcome.Status );
        }

        [TestMethod]
        public void Parse_422Refused_ReadsTheBodyAndIsFailure()
        {
            var outcome = ChatSyncClient.Parse( 422, "{\"submission_id\":\"11111111-1111-4111-8111-111111111111\",\"status\":\"refused\",\"error_code\":\"sync.contract_mismatch\"}" );

            Assert.IsTrue( outcome.IsHttpRefusal );
            Assert.IsTrue( outcome.IsFailure );
            Assert.AreEqual( "refused", outcome.Status );
            Assert.AreEqual( "sync.contract_mismatch", outcome.ErrorCode );
            StringAssert.Contains( outcome.ToJobResult(), "sync.contract_mismatch" );
        }

        [TestMethod]
        public async Task SubmitAsync_SendsPlainTextAndTheSevenHeaders_AndReadsA422Body()
        {
            var handler = new RecordingHandler
            {
                Response = new HttpResponseMessage( ( HttpStatusCode ) 422 )
                {
                    Content = new StringContent( "{\"submission_id\":\"11111111-1111-4111-8111-111111111111\",\"status\":\"refused\",\"error_code\":\"sync.bad_counts\"}", Encoding.UTF8, "application/json" )
                }
            };

            var client = new ChatSyncClient(
                new HttpClient( handler ),
                new ChatPlatformConfiguration
                {
                    ProjectUrl = "http://example.test",
                    PublishableKey = "sb_publishable_test"
                },
                "token-1" );

            var submissionId = Guid.Parse( "11111111-1111-4111-8111-111111111111" );
            var outcome = await client.SubmitAsync( new ChatSyncSubmitRequest
            {
                SubmissionId = submissionId,
                ReadAt = "2026-09-17T12:00:00.000000Z",
                Body = "{\"aliases\":[],\"channels\":[],\"members\":[],\"badges\":[]}",
                CountsJson = "{\"aliases\":0,\"channels\":0,\"members\":0,\"badges\":0}",
                MarksJson = "{\"person\":1,\"person_alias\":1,\"group\":1,\"group_member\":1}",
                RockVersion = "21.0.1",
                IsUrgent = true
            } );

            Assert.AreEqual( "sync.bad_counts", outcome.ErrorCode );
            Assert.IsTrue( outcome.IsFailure );
            Assert.AreEqual( "text/plain", handler.LastRequest.Content.Headers.ContentType.MediaType );
            Assert.AreEqual( ChatWireContract.ComputedHash, Header( handler, "x-sync-contract" ) );
            Assert.AreEqual( submissionId.ToString( "D" ), Header( handler, "x-sync-submission-id" ) );
            Assert.AreEqual( "true", Header( handler, "x-sync-urgent" ) );
            Assert.IsTrue( Header( handler, "x-sync-counts" ).Contains( "aliases" ) );
            Assert.IsFalse( Header( handler, "x-sync-counts" ).Contains( "chat_aliases" ) );
        }

        private static string Header( RecordingHandler handler, string name )
        {
            return string.Join( ",", handler.LastRequest.Headers.GetValues( name ) );
        }

        private sealed class RecordingHandler : HttpMessageHandler
        {
            public HttpResponseMessage Response { get; set; }

            public HttpRequestMessage LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                LastRequest = request;
                return Task.FromResult( Response );
            }
        }
    }
}
