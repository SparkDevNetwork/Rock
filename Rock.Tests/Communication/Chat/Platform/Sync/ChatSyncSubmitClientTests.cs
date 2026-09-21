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
    /// The call that carries a restatement to the chat platform and brings back what happened to it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A refusal arrives with an ordinary body and a 422 status, because the platform sets that
    /// status itself rather than raising: raising would roll back the history row the refusal was
    /// just recorded on. A client written the usual way calls EnsureSuccessStatusCode, throws before
    /// it reads a line of that body, and the church is left with a job that failed for no stated
    /// reason while the platform knows exactly which check refused it. That is the defect these
    /// cells exist to catch, and it is invisible to any test that only ever stubs a 200.
    /// </para>
    /// <para>
    /// The transport cell is the other half. A request that times out leaves Rock unable to say
    /// whether the platform received it, so the retry has to carry the id the first attempt used;
    /// a fresh id on the second attempt writes two history rows for one cycle and applies the same
    /// restatement twice.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ChatSyncSubmitClientTests
    {
        #region Fields

        private const string ProjectUrl = "https://example.supabase.co";

        private const string PublishableKey = "sb_publishable_test";

        private const string Token = "stub.church.token";

        /// <summary>
        /// Not on this framework's enumeration, and the status a refusal arrives with.
        /// </summary>
        private const HttpStatusCode UnprocessableEntity = ( HttpStatusCode ) 422;

        #endregion Fields

        #region Reading the body

        /// <summary>
        /// The whole point of the slice's client cell: a refusal is a body, not an exception.
        /// </summary>
        [TestMethod]
        public void Submit_WhenThePlatformRefuses_ReadsTheBodyOfThe422AndDoesNotThrow()
        {
            var submissionId = Guid.NewGuid();
            var previousId = Guid.NewGuid();
            var handler = StubHandler.Returning( UnprocessableEntity, Refused( submissionId, previousId ) );

            using ( var client = Client( handler ) )
            {
                var ack = client.Submit( submissionId, "{}", Headers() );

                Assert.AreEqual( submissionId, ack.SubmissionId );
                Assert.AreEqual( ChatSyncSubmissionStatus.Refused, ack.Status );
                Assert.AreEqual( "sync.bad_counts", ack.ErrorCode );
                Assert.AreEqual( 422, ack.HttpStatusCode );
                Assert.IsFalse( ack.IsTransportFailure );

                Assert.IsNotNull( ack.PreviousOutcome );
                Assert.AreEqual( previousId, ack.PreviousOutcome.SubmissionId );
                Assert.AreEqual( ChatSyncSubmissionStatus.Applied, ack.PreviousOutcome.Status );
                Assert.IsNull( ack.PreviousOutcome.ErrorCode );

                Assert.IsNotNull( ack.SyncBackoffUntil );
                Assert.AreEqual( new DateTimeOffset( 2026, 9, 21, 11, 30, 0, TimeSpan.Zero ), ack.SyncBackoffUntil.Value );
            }
        }

        /// <summary>
        /// The ordinary return, for contrast with the cell above.
        /// </summary>
        [TestMethod]
        public void Submit_WhenThePlatformAccepts_ReportsAccepted()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.Returning( HttpStatusCode.OK, Accepted( submissionId ) );

            using ( var client = Client( handler ) )
            {
                var ack = client.Submit( submissionId, "{}", Headers() );

                Assert.AreEqual( ChatSyncSubmissionStatus.Accepted, ack.Status );
                Assert.IsNull( ack.ErrorCode );
                Assert.IsNull( ack.PreviousOutcome );
                Assert.AreEqual( 200, ack.HttpStatusCode );
            }
        }

        #endregion Reading the body

        #region The request

        /// <summary>
        /// The submission surface takes the whole request body as one raw text value. A client that
        /// posts it as JSON is parsed before the function is reached and refused on every cycle, and
        /// every other cell in this file passes against that client because the stub does not care
        /// what it was sent.
        /// </summary>
        [TestMethod]
        public void Submit_SendsTheRawBodyAsTextPlainUnderTheSyncCredential()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.Returning( HttpStatusCode.OK, Accepted( submissionId ) );

            using ( var client = Client( handler ) )
            {
                client.Submit( submissionId, "{\"channels\":[]}", Headers() );
            }

            var request = handler.Requests.Single();

            Assert.AreEqual( HttpMethod.Post, request.Method );
            Assert.AreEqual( ProjectUrl + "/rest/v1/rpc/sync_submit", request.Url );
            Assert.AreEqual( "text/plain", request.ContentType );
            Assert.AreEqual( "utf-8", request.CharSet );
            Assert.AreEqual( "{\"channels\":[]}", request.Body );
            Assert.AreEqual( PublishableKey, request.Header( "apikey" ) );
            Assert.AreEqual( "Bearer " + Token, request.Header( "Authorization" ) );
            Assert.AreEqual( submissionId.ToString(), request.Header( "x-sync-submission-id" ) );
            Assert.AreEqual( "20260921T000000Z", request.Header( "x-sync-read-at" ) );
        }

        #endregion The request

        #region Retry

        /// <summary>
        /// A timed-out request may or may not have arrived, and the platform's third ingest check
        /// exists to answer that: the same id comes back with the outcome already recorded.
        /// </summary>
        [TestMethod]
        public void Submit_WhenTheTransportFails_RetriesWithTheSameSubmissionId()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.ThrowingThenReturning( 1, HttpStatusCode.OK, Accepted( submissionId ) );

            using ( var client = NoWaitClient( handler ) )
            {
                var ack = client.Submit( submissionId, "{}", Headers() );

                Assert.AreEqual( ChatSyncSubmissionStatus.Accepted, ack.Status );
            }

            Assert.AreEqual( 2, handler.Requests.Count );
            CollectionAssert.AreEqual(
                new List<string> { submissionId.ToString(), submissionId.ToString() },
                handler.Requests.Select( r => r.Header( "x-sync-submission-id" ) ).ToList() );
        }

        /// <summary>
        /// A transport that never comes back is not an exception thrown at the job. The job has a
        /// result message to write and a backoff to honour, and it cannot do either from a stack
        /// trace.
        /// </summary>
        [TestMethod]
        public void Submit_WhenEveryAttemptFails_ReportsATransportFailureRatherThanThrowing()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.ThrowingThenReturning( 99, HttpStatusCode.OK, Accepted( submissionId ) );

            using ( var client = NoWaitClient( handler ) )
            {
                var ack = client.Submit( submissionId, "{}", Headers() );

                Assert.IsTrue( ack.IsTransportFailure );
                Assert.IsNull( ack.Status );
                Assert.IsNull( ack.HttpStatusCode );
                Assert.IsTrue( ack.TransportDetail.IsNotNullOrWhiteSpace() );
            }

            Assert.AreEqual( 3, handler.Requests.Count );
        }

        #endregion Retry

        #region Polling

        /// <summary>
        /// The poll stops the moment the drain has recorded something, rather than spending its
        /// whole budget.
        /// </summary>
        [TestMethod]
        public void Poll_StopsAtTheFirstResolvedOutcome()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.ReturningInTurn(
                Status( submissionId, "accepted" ),
                Status( submissionId, "applied" ),
                Status( submissionId, "applied" ) );

            using ( var client = NoWaitClient( handler ) )
            {
                var outcome = client.Poll( submissionId, ChatSyncPollBudget.Manual );

                Assert.AreEqual( ChatSyncSubmissionStatus.Applied, outcome.Status );
            }

            Assert.AreEqual( 2, handler.Requests.Count );
        }

        /// <summary>
        /// The manual budget is twenty tries three seconds apart, and it is the try ceiling that
        /// bites first: twenty tries spend 57 seconds of the 60 second budget.
        /// </summary>
        [TestMethod]
        public void Poll_WhenNothingResolves_SpendsTheManualBudgetAndReportsAccepted()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.Returning( HttpStatusCode.OK, Status( submissionId, "accepted" ) );
            var waited = TimeSpan.Zero;

            using ( var client = Client( handler ) )
            {
                var now = new DateTime( 2026, 9, 21, 0, 0, 0, DateTimeKind.Utc );
                client.Clock = () => now;
                client.Wait = d => { waited += d; now = now.Add( d ); };

                var outcome = client.Poll( submissionId, ChatSyncPollBudget.Manual );

                Assert.AreEqual( ChatSyncSubmissionStatus.Accepted, outcome.Status );
            }

            Assert.AreEqual( 20, handler.Requests.Count );
            Assert.AreEqual( TimeSpan.FromSeconds( 57 ), waited );
        }

        /// <summary>
        /// The scheduled budget is the shorter one, because nobody is watching it and it has the
        /// acknowledgement's previous outcome to fall back on.
        /// </summary>
        [TestMethod]
        public void Poll_WhenNothingResolves_SpendsTheShorterScheduledBudget()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.Returning( HttpStatusCode.OK, Status( submissionId, "accepted" ) );
            var waited = TimeSpan.Zero;

            using ( var client = Client( handler ) )
            {
                var now = new DateTime( 2026, 9, 21, 0, 0, 0, DateTimeKind.Utc );
                client.Clock = () => now;
                client.Wait = d => { waited += d; now = now.Add( d ); };

                client.Poll( submissionId, ChatSyncPollBudget.Scheduled );
            }

            Assert.AreEqual( 6, handler.Requests.Count );
            Assert.AreEqual( TimeSpan.FromSeconds( 25 ), waited );
        }

        /// <summary>
        /// The elapsed budget stops the loop before the try ceiling when a single read is slow, so
        /// the two bounds are not the same bound written twice.
        /// </summary>
        [TestMethod]
        public void Poll_WhenEachReadIsSlow_StopsOnTheElapsedBudgetRatherThanTheTryCeiling()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.Returning( HttpStatusCode.OK, Status( submissionId, "accepted" ) );

            using ( var client = Client( handler ) )
            {
                var now = new DateTime( 2026, 9, 21, 0, 0, 0, DateTimeKind.Utc );
                client.Clock = () => now;

                // Every read costs ten seconds of the wall clock on top of the interval.
                client.Wait = d => now = now.Add( d ).Add( TimeSpan.FromSeconds( 10 ) );

                client.Poll( submissionId, ChatSyncPollBudget.Manual );
            }

            Assert.IsTrue( handler.Requests.Count < 20,
                "the 60 second budget has to stop the loop before the twentieth try when each read costs 13 seconds" );
            Assert.AreEqual( 5, handler.Requests.Count );
        }

        #endregion Polling

        #region Support

        private static ChatSyncSubmitClient Client( HttpMessageHandler handler )
        {
            var configuration = new ChatPlatformConfiguration
            {
                TenantId = Guid.NewGuid(),
                ProjectUrl = ProjectUrl,
                PublishableKey = PublishableKey,
                PrivateKey = "{}"
            };

            return new ChatSyncSubmitClient( configuration, () => Token, handler );
        }

        /// <summary>
        /// A client whose waits cost nothing, for the cells that count attempts rather than time.
        /// </summary>
        private static ChatSyncSubmitClient NoWaitClient( HttpMessageHandler handler )
        {
            var client = Client( handler );
            client.Wait = d => { };
            return client;
        }

        private static IDictionary<string, string> Headers()
        {
            return new Dictionary<string, string>
            {
                ["x-sync-read-at"] = "20260921T000000Z",
                ["x-sync-contract"] = "de1f06d0",
                ["x-sync-rock-version"] = "20.0.0"
            };
        }

        private static string Accepted( Guid submissionId )
        {
            return "{\"submission_id\":\"" + submissionId + "\",\"status\":\"accepted\",\"error_code\":null,"
                + "\"previous_outcome\":null,\"sync_backoff_until\":null}";
        }

        private static string Refused( Guid submissionId, Guid previousId )
        {
            return "{\"submission_id\":\"" + submissionId + "\",\"status\":\"refused\",\"error_code\":\"sync.bad_counts\","
                + "\"previous_outcome\":{\"submission_id\":\"" + previousId + "\",\"status\":\"applied\","
                + "\"error_code\":null,\"drained_at\":\"2026-09-21T10:00:00+00:00\"},"
                + "\"sync_backoff_until\":\"2026-09-21T11:30:00+00:00\"}";
        }

        private static string Status( Guid submissionId, string status )
        {
            return "{\"submission_id\":\"" + submissionId + "\",\"status\":\"" + status + "\","
                + "\"error_code\":null,\"drained_at\":null}";
        }

        /// <summary>
        /// A transport that answers from a script and records what it was asked.
        /// </summary>
        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<int, HttpResponseMessage> _answer;

            private int _calls;

            public List<RecordedRequest> Requests { get; } = new List<RecordedRequest>();

            private StubHandler( Func<int, HttpResponseMessage> answer )
            {
                _answer = answer;
            }

            public static StubHandler Returning( HttpStatusCode code, string body )
            {
                return new StubHandler( call => Response( code, body ) );
            }

            public static StubHandler ReturningInTurn( params string[] bodies )
            {
                return new StubHandler( call => Response( HttpStatusCode.OK, bodies[Math.Min( call, bodies.Length - 1 )] ) );
            }

            public static StubHandler ThrowingThenReturning( int throwCount, HttpStatusCode code, string body )
            {
                return new StubHandler( call =>
                {
                    if ( call < throwCount )
                    {
                        throw new HttpRequestException( "the stub transport refused to connect" );
                    }

                    return Response( code, body );
                } );
            }

            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                var recorded = new RecordedRequest
                {
                    Method = request.Method,
                    Url = request.RequestUri.ToString(),
                    ContentType = request.Content?.Headers?.ContentType?.MediaType,
                    CharSet = request.Content?.Headers?.ContentType?.CharSet,
                    Body = request.Content == null ? null : request.Content.ReadAsStringAsync().Result
                };

                foreach ( var header in request.Headers )
                {
                    recorded.Headers[header.Key] = string.Join( ",", header.Value );
                }

                Requests.Add( recorded );

                var call = _calls;
                _calls++;

                return Task.FromResult( _answer( call ) );
            }

            private static HttpResponseMessage Response( HttpStatusCode code, string body )
            {
                return new HttpResponseMessage( code )
                {
                    Content = new StringContent( body, System.Text.Encoding.UTF8, "application/json" )
                };
            }
        }

        private sealed class RecordedRequest
        {
            public HttpMethod Method { get; set; }

            public string Url { get; set; }

            public string ContentType { get; set; }

            public string CharSet { get; set; }

            public string Body { get; set; }

            public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            public string Header( string name )
            {
                string value;
                return Headers.TryGetValue( name, out value ) ? value : null;
            }
        }

        #endregion Support
    }
}
