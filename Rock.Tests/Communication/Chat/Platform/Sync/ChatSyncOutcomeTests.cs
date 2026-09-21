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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Contract;
using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What a recorded submission status means to the job that submitted it.
    /// </summary>
    /// <remarks>
    /// Four statuses and two HTTP codes, paired by the platform rather than by Rock: accepted and
    /// applied return 200, refused and failed return 422. The pairing is asserted here in full
    /// because it is the whole of what a job run reports, and because a mapping written from the
    /// two that are easy to remember leaves applied or failed falling through to whichever answer
    /// the default arm gives.
    /// </remarks>
    [TestClass]
    public class ChatSyncOutcomeTests
    {
        #region The four statuses

        [TestMethod]
        public void HttpStatus_FollowsTheRecordedStatusAcrossAllFour()
        {
            Assert.AreEqual( 200, ChatSyncOutcomeMapper.HttpStatusFor( ChatSyncSubmissionStatus.Accepted ) );
            Assert.AreEqual( 200, ChatSyncOutcomeMapper.HttpStatusFor( ChatSyncSubmissionStatus.Applied ) );
            Assert.AreEqual( 422, ChatSyncOutcomeMapper.HttpStatusFor( ChatSyncSubmissionStatus.Refused ) );
            Assert.AreEqual( 422, ChatSyncOutcomeMapper.HttpStatusFor( ChatSyncSubmissionStatus.Failed ) );
        }

        [TestMethod]
        public void JobSuccess_IsTheTwoStatusesThatReturnTwoHundred()
        {
            Assert.IsTrue( ChatSyncOutcomeMapper.IsJobSuccess( ChatSyncSubmissionStatus.Accepted ) );
            Assert.IsTrue( ChatSyncOutcomeMapper.IsJobSuccess( ChatSyncSubmissionStatus.Applied ) );
            Assert.IsFalse( ChatSyncOutcomeMapper.IsJobSuccess( ChatSyncSubmissionStatus.Refused ) );
            Assert.IsFalse( ChatSyncOutcomeMapper.IsJobSuccess( ChatSyncSubmissionStatus.Failed ) );
        }

        /// <summary>
        /// The one that stops the two halves drifting: every status the mapper answers for is a
        /// status the platform can record, and every status the platform can record is answered
        /// for. The list comes from the artifact rather than from this file.
        /// </summary>
        [TestMethod]
        public void EveryRecordedStatusInTheArtifactIsOneThisMapperAnswersFor()
        {
            var contract = JObject.Parse( ChatWireContract.Json );
            var published = contract["enum_types"]
                .Children<JObject>()
                .Single( t => ( string ) t["name"] == "chat_sync_submission_status" )["values"]
                .Select( v => ( string ) v )
                .OrderBy( v => v )
                .ToList();

            var mirrored = Enum.GetValues( typeof( ChatSyncSubmissionStatus ) )
                .Cast<ChatSyncSubmissionStatus>()
                .Select( ChatSyncOutcomeMapper.WireValueFor )
                .OrderBy( v => v )
                .ToList();

            CollectionAssert.AreEqual( published, mirrored,
                "the four recorded statuses are the platform's vocabulary, and a Rock member that is not one of them cannot come back off the wire" );
        }

        [TestMethod]
        public void ParsingAStatusRockDoesNotKnow_ReportsNothingRatherThanGuessing()
        {
            Assert.IsNull( ChatSyncOutcomeMapper.ParseStatus( "quarantined" ) );
            Assert.IsNull( ChatSyncOutcomeMapper.ParseStatus( null ) );
            Assert.AreEqual( ChatSyncSubmissionStatus.Applied, ChatSyncOutcomeMapper.ParseStatus( "applied" ) );
        }

        #endregion The four statuses

        #region Through an acknowledgement

        [TestMethod]
        public void AnAcknowledgementReportsSuccessFromItsStatus()
        {
            Assert.IsTrue( Ack( ChatSyncSubmissionStatus.Accepted, 200 ).IsJobSuccess );
            Assert.IsFalse( Ack( ChatSyncSubmissionStatus.Refused, 422 ).IsJobSuccess );
        }

        /// <summary>
        /// A submission that never reached the platform is not a success, whatever else is missing
        /// from it.
        /// </summary>
        [TestMethod]
        public void ATransportFailureIsNotASuccess()
        {
            var ack = ChatSyncAcknowledgement.Unreachable( Guid.NewGuid(), "the host could not be resolved" );

            Assert.IsFalse( ack.IsJobSuccess );
            Assert.IsTrue( ack.IsTransportFailure );
        }

        /// <summary>
        /// The platform's own answer carries backoff advice whether or not it names a backoff: an
        /// acceptance naming none is the platform saying there is none, and that is advice too.
        /// </summary>
        [TestMethod]
        public void ThePlatformsOwnAnswer_CarriesBackoffAdvice_WhetherOrNotItNamesABackoff()
        {
            var refusedWithBackoff = Ack( ChatSyncSubmissionStatus.Refused, 422 );
            refusedWithBackoff.SyncBackoffUntil = new DateTimeOffset( 2026, 9, 21, 11, 30, 0, TimeSpan.Zero );

            var acceptedWithout = Ack( ChatSyncSubmissionStatus.Accepted, 200 );

            Assert.IsTrue( refusedWithBackoff.CarriesBackoffAdvice, "a refusal that names a backoff is the platform asking for quiet, and the run has to record it" );
            Assert.IsTrue( acceptedWithout.CarriesBackoffAdvice, "an acceptance naming no backoff is the platform saying there is none, and a backoff already stored has to be cleared by it" );
        }

        /// <summary>
        /// An answer that is not the platform's carries no advice. Recording its silence as "no
        /// backoff" would clear advice the platform had given, so a run that never reached the
        /// platform, or was answered by a gateway in its own words, leaves the stored backoff alone.
        /// </summary>
        [TestMethod]
        public void AnAnswerThatIsNotThePlatforms_CarriesNoBackoffAdvice()
        {
            var unreachable = ChatSyncAcknowledgement.Unreachable( Guid.NewGuid(), "the host could not be resolved" );

            var unreadable = new ChatSyncAcknowledgement
            {
                SubmissionId = Guid.NewGuid(),
                HttpStatusCode = 502,
                TransportDetail = "the chat platform answered with something that is not an acknowledgement"
            };

            Assert.IsFalse( unreachable.CarriesBackoffAdvice, "a run that never reached the platform would clear the backoff the platform had asked for" );
            Assert.IsFalse( unreadable.CarriesBackoffAdvice, "a gateway answering in its own words is not the platform lifting its backoff" );
        }

        #endregion Through an acknowledgement

        #region Support

        private static ChatSyncAcknowledgement Ack( ChatSyncSubmissionStatus status, int httpStatusCode )
        {
            return new ChatSyncAcknowledgement
            {
                SubmissionId = Guid.NewGuid(),
                Status = status,
                HttpStatusCode = httpStatusCode
            };
        }

        #endregion Support
    }
}
