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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Sync;

using PlatformClient = Rock.Communication.Chat.Platform.Sync.ChatPlatformSyncHelper.PlatformClient;
using PollBudget = Rock.Communication.Chat.Platform.Sync.ChatPlatformSyncHelper.PollBudget;
using SubmissionStatus = Rock.Communication.Chat.Platform.Sync.ChatPlatformSyncHelper.SubmissionStatus;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// One run's conversation with the chat platform: the credential exchange, the submission and
    /// the status reads.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A refusal arrives with an ordinary body and a 422 status, because the platform sets that
    /// status itself rather than raising: raising would roll back the history row the refusal was
    /// just recorded on. A client that calls EnsureSuccessStatusCode throws before it reads a line
    /// of that body, which is invisible to any test that only ever stubs a 200.
    /// </para>
    /// <para>
    /// A request that times out leaves Rock unable to say whether the platform received it, so the
    /// retry has to carry the id the first attempt used.
    /// </para>
    /// <para>
    /// The church signs its own token, and the platform's data API cannot verify a church's
    /// signature, so that token is exchanged for a platform token first. A sync sent under the
    /// church token itself is refused on every call.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ChatSyncPlatformClientTests
    {
        #region Fields

        private const string ProjectUrl = "https://example.supabase.co";

        private const string PublishableKey = "sb_publishable_test";

        private const string ChurchToken = "church.signed.token";

        // A platform token, as the exchange hands back. The job once sent the church token where
        // this belonged, which the platform refuses.
        private const string Token = "stub.platform.token";

        private const string Kid = "kid-test-1";

        /// <summary>
        /// Not on this framework's enumeration, and the status a refusal arrives with.
        /// </summary>
        private const HttpStatusCode UnprocessableEntity = ( HttpStatusCode ) 422;

        #endregion Fields

        #region The credential

        [TestMethod]
        public void Exchange_PostsTheChurchTokenAndHoldsThePlatformToken()
        {
            var handler = StubHandler.Returning( HttpStatusCode.OK, "{\"access_token\":\"" + Token + "\",\"token_type\":\"bearer\",\"expires_in\":300}" );

            using ( var client = new PlatformClient( Configuration( ProjectUrl + "/" ), handler ) )
            {
                var exchanged = client.Exchange( ChurchToken, out var failure );

                Assert.IsTrue( exchanged );
                Assert.IsNull( failure );
                Assert.AreEqual( Token, client.PlatformToken );
            }

            var request = handler.Requests.Single();
            Assert.AreEqual( HttpMethod.Post, request.Method );
            Assert.AreEqual( ProjectUrl + "/functions/v1/token-exchange", request.Url );
            Assert.AreEqual( "Bearer " + ChurchToken, request.Header( "Authorization" ) );
            Assert.AreEqual( PublishableKey, request.Header( "apikey" ) );
        }

        [TestMethod]
        public void Exchange_WhenRefused_HoldsNoTokenAndGivesThePlatformsReason()
        {
            var handler = StubHandler.Returning( HttpStatusCode.Unauthorized, "{\"error\":{\"code\":\"auth.invalid_token\",\"message\":\"auth.invalid_token\"}}" );

            using ( var client = new PlatformClient( Configuration( ProjectUrl + "/" ), handler ) )
            {
                var exchanged = client.Exchange( ChurchToken, out var failure );

                Assert.IsFalse( exchanged );
                Assert.IsNull( client.PlatformToken );
                StringAssert.Contains( failure, "the chat platform refused this church's credential" );
                StringAssert.Contains( failure, "auth.invalid_token" );
            }
        }

        [TestMethod]
        public void SignIn_WhenTheChurchCannotSign_CallsNothing()
        {
            var handler = StubHandler.Returning( HttpStatusCode.OK, "{\"access_token\":\"" + Token + "\"}" );

            using ( var client = new PlatformClient( Configuration( ProjectUrl + "/" ), handler ) )
            {
                var signedIn = client.SignIn( out var failure );

                Assert.IsFalse( signedIn );
                Assert.IsNotNull( failure );
                Assert.IsNull( client.PlatformToken );
            }

            Assert.AreEqual( 0, handler.Requests.Count );
        }

        /// <summary>
        /// One client carries the whole run: the church's sync token is minted once and exchanged,
        /// and the submission goes out under the platform token the exchange returned.
        /// </summary>
        [TestMethod]
        public void SignIn_ExchangesOneSyncTokenAndTheSubmissionCarriesThePlatformToken()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.ReturningInTurn(
                "{\"access_token\":\"" + Token + "\",\"token_type\":\"bearer\",\"expires_in\":300}",
                Accepted( submissionId ) );

            var configuration = Configuration( ProjectUrl );
            configuration.PrivateKey = CreatePrivateJwk( Kid );
            configuration.Kid = Kid;

            using ( var client = new PlatformClient( configuration, handler ) )
            {
                Assert.IsTrue( client.SignIn( out var failure ), failure );

                client.Submit( submissionId, Body( "{}" ), Headers() );
            }

            Assert.AreEqual( 2, handler.Requests.Count, "the run did not make exactly one exchange and one submission" );

            var exchange = handler.Requests[0];
            Assert.AreEqual( ProjectUrl + "/functions/v1/token-exchange", exchange.Url );

            var churchToken = new JwtSecurityTokenHandler().ReadJwtToken( exchange.Header( "Authorization" ).Substring( "Bearer ".Length ) );
            Assert.AreEqual( "sync", churchToken.Payload["scp"].ToString(), "the exchange was not handed a sync-scope church token" );

            var submit = handler.Requests[1];
            Assert.AreEqual( ProjectUrl + "/rest/v1/rpc/sync_submit", submit.Url );
            Assert.AreEqual( "Bearer " + Token, submit.Header( "Authorization" ), "the submission did not go out under the platform token" );
        }

        /// <summary>
        /// A church token signed by a clock the platform disagrees with is refused, and the reason
        /// that reaches an administrator should name the clock, which is the thing to fix.
        /// </summary>
        [TestMethod]
        [DataRow( "auth.stale_token" )]
        [DataRow( "auth.invalid_token" )]
        public void Exchange_WhenRefusedAndThisClockIsBehind_SaysHowFarBehind( string code )
        {
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );
            var handler = StubHandler.AnsweringInTurn( Answer( HttpStatusCode.Unauthorized, ErrorBody( code ), now.AddSeconds( 240 ) ) );

            using ( var client = new PlatformClient( Configuration( ProjectUrl ), handler ) { Clock = () => now } )
            {
                Assert.IsFalse( client.Exchange( ChurchToken, out var failure ) );

                StringAssert.Contains( failure, code );
                StringAssert.Contains( failure, "this server's clock is 240 s behind the chat platform" );
            }
        }

        [TestMethod]
        public void Exchange_WhenRefusedAndThisClockIsAhead_SaysHowFarAhead()
        {
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );
            var handler = StubHandler.AnsweringInTurn( Answer( HttpStatusCode.Unauthorized, ErrorBody( "auth.invalid_token" ), now.AddSeconds( -90 ) ) );

            using ( var client = new PlatformClient( Configuration( ProjectUrl ), handler ) { Clock = () => now } )
            {
                Assert.IsFalse( client.Exchange( ChurchToken, out var failure ) );

                StringAssert.Contains( failure, "this server's clock is 90 s ahead of the chat platform" );
            }
        }

        /// <summary>
        /// A few seconds apart is the time the answer took to arrive, not a clock to fix.
        /// </summary>
        [TestMethod]
        public void Exchange_WhenRefusedAndTheClocksAgree_SaysNothingAboutTheClock()
        {
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );
            var handler = StubHandler.AnsweringInTurn( Answer( HttpStatusCode.Unauthorized, ErrorBody( "auth.stale_token" ), now.AddSeconds( 10 ) ) );

            using ( var client = new PlatformClient( Configuration( ProjectUrl ), handler ) { Clock = () => now } )
            {
                Assert.IsFalse( client.Exchange( ChurchToken, out var failure ) );

                StringAssert.Contains( failure, "auth.stale_token" );
                Assert.IsFalse( failure.Contains( "clock" ), failure );
            }
        }

        /// <summary>
        /// The exchange is one more request of the run, and one dropped connection should cost it no
        /// more than it costs the submission.
        /// </summary>
        [TestMethod]
        public void Exchange_WhenTheTransportFailsOnce_RetriesAndHoldsTheToken()
        {
            var handler = StubHandler.ThrowingThenReturning( 1, HttpStatusCode.OK, Exchanged( Token ) );

            using ( var client = NoWaitClient( handler ) )
            {
                Assert.IsTrue( client.Exchange( ChurchToken, out var failure ), failure );
                Assert.AreEqual( Token, client.PlatformToken );
            }

            Assert.AreEqual( 2, handler.Requests.Count );
        }

        [TestMethod]
        public void Exchange_WhenTheGatewayIsUnavailableOnce_RetriesAndHoldsTheToken()
        {
            var handler = StubHandler.AnsweringInTurn(
                Answer( HttpStatusCode.ServiceUnavailable, "<html>unavailable</html>" ),
                Answer( HttpStatusCode.OK, Exchanged( Token ) ) );

            using ( var client = NoWaitClient( handler ) )
            {
                Assert.IsTrue( client.Exchange( ChurchToken, out var failure ), failure );
                Assert.AreEqual( Token, client.PlatformToken );
            }

            Assert.AreEqual( 2, handler.Requests.Count );
        }

        /// <summary>
        /// A platform token lasts about five minutes, and slow submission attempts followed by the
        /// poll can outlast it, so a token near its end is exchanged again before it is sent.
        /// </summary>
        [TestMethod]
        public void Submit_WhenThePlatformTokenIsAboutToExpire_ExchangesAgainFirst()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.ReturningInTurn( Exchanged( "first.token" ), Exchanged( "second.token" ), Accepted( submissionId ) );
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );

            using ( var client = SigningClient( handler, () => now ) )
            {
                Assert.IsTrue( client.SignIn( out var failure ), failure );

                now = now.AddSeconds( 250 );
                var ack = client.Submit( submissionId, Body( "{}" ), Headers() );

                Assert.AreEqual( SubmissionStatus.Accepted, ack.Status );
            }

            Assert.AreEqual( 3, handler.Requests.Count, "the run did not exchange again before a submission under a token with 50 s left" );
            Assert.AreEqual( ProjectUrl + "/functions/v1/token-exchange", handler.Requests[1].Url );
            Assert.AreEqual( "Bearer second.token", handler.Requests[2].Header( "Authorization" ) );
        }

        [TestMethod]
        public void Submit_WhenThePlatformTokenHasTimeLeft_DoesNotExchangeAgain()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.ReturningInTurn( Exchanged( "first.token" ), Accepted( submissionId ) );
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );

            using ( var client = SigningClient( handler, () => now ) )
            {
                Assert.IsTrue( client.SignIn( out var failure ), failure );

                now = now.AddSeconds( 200 );
                client.Submit( submissionId, Body( "{}" ), Headers() );
            }

            Assert.AreEqual( 2, handler.Requests.Count );
            Assert.AreEqual( "Bearer first.token", handler.Requests[1].Header( "Authorization" ) );
        }

        [TestMethod]
        public void ReadStatus_WhenThePlatformTokenIsAboutToExpire_ExchangesAgainFirst()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.ReturningInTurn( Exchanged( "first.token" ), Exchanged( "second.token" ), Status( submissionId, "applied" ) );
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );

            using ( var client = SigningClient( handler, () => now ) )
            {
                Assert.IsTrue( client.SignIn( out var failure ), failure );

                now = now.AddSeconds( 250 );
                var outcome = client.ReadStatus( submissionId );

                Assert.AreEqual( SubmissionStatus.Applied, outcome.Status );
            }

            Assert.AreEqual( 3, handler.Requests.Count, "the run did not exchange again before a status read under a token with 50 s left" );
            Assert.AreEqual( ProjectUrl + "/rest/v1/rpc/sync_status", handler.Requests[2].Url );
            Assert.AreEqual( "Bearer second.token", handler.Requests[2].Header( "Authorization" ) );
        }

        #endregion The credential

        #region Reading the body

        /// <summary>
        /// A refusal is a body, not an exception.
        /// </summary>
        [TestMethod]
        public void Submit_WhenThePlatformRefuses_ReadsTheBodyOfThe422AndDoesNotThrow()
        {
            var submissionId = Guid.NewGuid();
            var previousId = Guid.NewGuid();
            var handler = StubHandler.Returning( UnprocessableEntity, Refused( submissionId, previousId ) );

            using ( var client = Client( handler ) )
            {
                var ack = client.Submit( submissionId, Body( "{}" ), Headers() );

                Assert.AreEqual( submissionId, ack.SubmissionId );
                Assert.AreEqual( SubmissionStatus.Refused, ack.Status );
                Assert.AreEqual( "sync.bad_counts", ack.ErrorCode );
                Assert.AreEqual( 422, ack.HttpStatusCode );
                Assert.IsFalse( ack.IsTransportFailure );

                Assert.IsNotNull( ack.PreviousOutcome );
                Assert.AreEqual( previousId, ack.PreviousOutcome.SubmissionId );
                Assert.AreEqual( SubmissionStatus.Applied, ack.PreviousOutcome.Status );
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
                var ack = client.Submit( submissionId, Body( "{}" ), Headers() );

                Assert.AreEqual( SubmissionStatus.Accepted, ack.Status );
                Assert.IsNull( ack.ErrorCode );
                Assert.IsNull( ack.PreviousOutcome );
                Assert.AreEqual( 200, ack.HttpStatusCode );
            }
        }

        #endregion Reading the body

        #region The request

        /// <summary>
        /// The submission surface takes the whole request body as one raw text value. A client that
        /// posts it as JSON is parsed before the function is reached and refused on every cycle.
        /// </summary>
        [TestMethod]
        public void Submit_SendsTheRawBodyAsTextPlainUnderTheSyncCredential()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.Returning( HttpStatusCode.OK, Accepted( submissionId ) );

            using ( var client = Client( handler ) )
            {
                client.Submit( submissionId, Body( "{\"channels\":[]}" ), Headers() );
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
        /// A timed-out request may or may not have arrived, and the platform answers a repeated id
        /// with the outcome it already recorded.
        /// </summary>
        [TestMethod]
        public void Submit_WhenTheTransportFails_RetriesWithTheSameSubmissionId()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.ThrowingThenReturning( 1, HttpStatusCode.OK, Accepted( submissionId ) );

            using ( var client = NoWaitClient( handler ) )
            {
                var ack = client.Submit( submissionId, Body( "{}" ), Headers() );

                Assert.AreEqual( SubmissionStatus.Accepted, ack.Status );
            }

            Assert.AreEqual( 2, handler.Requests.Count );
            CollectionAssert.AreEqual(
                new List<string> { submissionId.ToString(), submissionId.ToString() },
                handler.Requests.Select( r => r.Header( "x-sync-submission-id" ) ).ToList() );
        }

        /// <summary>
        /// A transport that never comes back is not an exception thrown at the job, which has a
        /// result to write and a backoff to honour.
        /// </summary>
        [TestMethod]
        public void Submit_WhenEveryAttemptFails_ReportsATransportFailureRatherThanThrowing()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.ThrowingThenReturning( 99, HttpStatusCode.OK, Accepted( submissionId ) );

            using ( var client = NoWaitClient( handler ) )
            {
                var ack = client.Submit( submissionId, Body( "{}" ), Headers() );

                Assert.IsTrue( ack.IsTransportFailure );
                Assert.IsNull( ack.Status );
                Assert.IsNull( ack.HttpStatusCode );
                Assert.IsTrue( ack.TransportDetail.IsNotNullOrWhiteSpace() );
            }

            Assert.AreEqual( 3, handler.Requests.Count );
        }

        /// <summary>
        /// A gateway answer means the call never completed, so the same id may be sent again.
        /// </summary>
        [TestMethod]
        [DataRow( 502 )]
        [DataRow( 503 )]
        [DataRow( 504 )]
        public void Submit_WhenTheGatewayAnswersAnError_RetriesWithTheSameSubmissionId( int code )
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.AnsweringInTurn(
                Answer( ( HttpStatusCode ) code, "<html>gateway</html>" ),
                Answer( HttpStatusCode.OK, Accepted( submissionId ) ) );

            using ( var client = NoWaitClient( handler ) )
            {
                var ack = client.Submit( submissionId, Body( "{}" ), Headers() );

                Assert.AreEqual( SubmissionStatus.Accepted, ack.Status );
            }

            Assert.AreEqual( 2, handler.Requests.Count );
            Assert.AreEqual( submissionId.ToString(), handler.Requests[1].Header( "x-sync-submission-id" ) );
        }

        /// <summary>
        /// A submission that arrives while the drain holds the church's inbox row waits on the lock
        /// until the statement timeout cancels it. The whole call rolled back, so a retry is safe.
        /// </summary>
        [TestMethod]
        public void Submit_WhenTheStatementTimesOut_RetriesWithTheSameSubmissionId()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.AnsweringInTurn(
                Answer( HttpStatusCode.InternalServerError, StatementTimeout() ),
                Answer( HttpStatusCode.OK, Accepted( submissionId ) ) );

            using ( var client = NoWaitClient( handler ) )
            {
                var ack = client.Submit( submissionId, Body( "{}" ), Headers() );

                Assert.AreEqual( SubmissionStatus.Accepted, ack.Status );
            }

            Assert.AreEqual( 2, handler.Requests.Count );
            Assert.AreEqual( submissionId.ToString(), handler.Requests[1].Header( "x-sync-submission-id" ) );
        }

        [TestMethod]
        public void Submit_WhenEveryAttemptTimesOut_ReportsATransportFailureNamingTheTimeout()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.AnsweringInTurn( Answer( HttpStatusCode.InternalServerError, StatementTimeout() ) );

            using ( var client = NoWaitClient( handler ) )
            {
                var ack = client.Submit( submissionId, Body( "{}" ), Headers() );

                Assert.IsTrue( ack.IsTransportFailure );
                StringAssert.Contains( ack.TransportDetail, "57014" );
                StringAssert.StartsWith( ChatPlatformSyncHelper.Resolve( ack, null ).Message, "the chat platform could not be reached" );
            }

            Assert.AreEqual( 3, handler.Requests.Count );
        }

        /// <summary>
        /// Any other server error is not known to have rolled back, so it is reported, not repeated.
        /// </summary>
        [TestMethod]
        public void Submit_WhenThePlatformAnswersAnotherServerError_DoesNotRetry()
        {
            var submissionId = Guid.NewGuid();
            var handler = StubHandler.AnsweringInTurn(
                Answer( HttpStatusCode.InternalServerError, "{\"code\":\"XX000\",\"details\":null,\"hint\":null,\"message\":\"internal error\"}" ) );

            using ( var client = NoWaitClient( handler ) )
            {
                var ack = client.Submit( submissionId, Body( "{}" ), Headers() );

                Assert.AreEqual( 500, ack.HttpStatusCode );
                Assert.IsFalse( ack.IsTransportFailure );
            }

            Assert.AreEqual( 1, handler.Requests.Count );
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
                var outcome = client.Poll( submissionId, PollBudget.Manual );

                Assert.AreEqual( SubmissionStatus.Applied, outcome.Status );
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

                var outcome = client.Poll( submissionId, PollBudget.Manual );

                Assert.AreEqual( SubmissionStatus.Accepted, outcome.Status );
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

                client.Poll( submissionId, PollBudget.Scheduled );
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

                client.Poll( submissionId, PollBudget.Manual );
            }

            Assert.IsTrue( handler.Requests.Count < 20,
                "the 60 second budget has to stop the loop before the twentieth try when each read costs 13 seconds" );
            Assert.AreEqual( 5, handler.Requests.Count );
        }

        #endregion Polling

        #region Support

        private static ChatPlatformConfiguration Configuration( string projectUrl )
        {
            return new ChatPlatformConfiguration
            {
                TenantId = Guid.NewGuid(),
                ProjectUrl = projectUrl,
                PublishableKey = PublishableKey,
                PrivateKey = "{}"
            };
        }

        /// <summary>
        /// A client already holding a platform token, for the cells about the submission and polls.
        /// </summary>
        private static PlatformClient Client( HttpMessageHandler handler )
        {
            return new PlatformClient( Configuration( ProjectUrl ), handler ) { PlatformToken = Token };
        }

        /// <summary>
        /// A client whose waits cost nothing, for the cells that count attempts rather than time.
        /// </summary>
        private static PlatformClient NoWaitClient( HttpMessageHandler handler )
        {
            var client = Client( handler );
            client.Wait = d => { };
            return client;
        }

        /// <summary>
        /// A client that signs its own church tokens, so it can exchange again during a run.
        /// </summary>
        private static PlatformClient SigningClient( HttpMessageHandler handler, Func<DateTime> clock )
        {
            var configuration = Configuration( ProjectUrl );
            configuration.PrivateKey = CreatePrivateJwk( Kid );
            configuration.Kid = Kid;

            return new PlatformClient( configuration, handler ) { Clock = clock, Wait = d => { } };
        }

        private static string Exchanged( string token )
        {
            return "{\"access_token\":\"" + token + "\",\"token_type\":\"bearer\",\"expires_in\":300}";
        }

        private static string ErrorBody( string code )
        {
            return "{\"error\":{\"code\":\"" + code + "\",\"message\":\"" + code + "\",\"request_id\":\"r\"}}";
        }

        /// <summary>
        /// The body the data API answers with when the statement timeout cancels a call.
        /// </summary>
        private static string StatementTimeout()
        {
            return "{\"code\":\"57014\",\"details\":null,\"hint\":null,\"message\":\"canceling statement due to statement timeout\"}";
        }

        /// <summary>
        /// One scripted answer, with the time the platform says it answered at when one is given.
        /// </summary>
        private static Func<HttpResponseMessage> Answer( HttpStatusCode code, string body, DateTime? platformTimeUtc = null )
        {
            return () =>
            {
                var response = StubHandler.Response( code, body );

                if ( platformTimeUtc.HasValue )
                {
                    response.Headers.Date = new DateTimeOffset( platformTimeUtc.Value );
                }

                return response;
            };
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

        /// <summary>
        /// A body in the form the job hands the client: the UTF-8 bytes it was written as.
        /// </summary>
        private static ArraySegment<byte> Body( string json )
        {
            return new ArraySegment<byte>( Encoding.UTF8.GetBytes( json ) );
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

            /// <summary>
            /// Answers in turn from the script, repeating the last answer once the script runs out.
            /// </summary>
            public static StubHandler AnsweringInTurn( params Func<HttpResponseMessage>[] answers )
            {
                return new StubHandler( call => answers[Math.Min( call, answers.Length - 1 )]() );
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

            public static HttpResponseMessage Response( HttpStatusCode code, string body )
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
