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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Sync;
using Rock.Jobs;
using Acknowledgement = Rock.Communication.Chat.Platform.Sync.ChatPlatformSyncHelper.Acknowledgement;
using Outcome = Rock.Communication.Chat.Platform.Sync.ChatPlatformSyncHelper.Outcome;
using SubmissionStatus = Rock.Communication.Chat.Platform.Sync.ChatPlatformSyncHelper.SubmissionStatus;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What the job reports about the submission it just made.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The drain runs on its own schedule, so a submission is very often still unapplied when the
    /// job that made it is ready to finish. Three answers in order: the polled outcome of this
    /// submission when there is one, the previous submission's outcome carried on the
    /// acknowledgement when there is not, and a plain statement that the restatement is queued when
    /// there is neither. The last of those is not a failure, and that is the part worth pinning: a
    /// job that failed because the drain had not run yet would turn a working church's job page red
    /// on most cycles and teach its administrator to ignore it.
    /// </para>
    /// <para>
    /// The fallback reports the previous submission and says so. Reporting it as though it were
    /// this run's result would be a lie that reads as a success on the cycle after a failure, and
    /// the previous outcome never decides whether this run failed.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ChatSyncStatusPathTests
    {
        #region The polled outcome

        [TestMethod]
        public void WhenThePollResolves_ThatOutcomeIsWhatTheJobReports()
        {
            var submissionId = Guid.NewGuid();
            var result = ChatPlatformSyncHelper.Resolve(
                AcceptedAck( submissionId, PreviousOutcome( SubmissionStatus.Failed ) ),
                Polled( submissionId, SubmissionStatus.Applied ) );

            Assert.IsFalse( result.IsFailure );
            StringAssert.Contains( result.Message, "applied" );
        }

        [TestMethod]
        public void WhenThePollResolvesToAFailure_TheJobFails()
        {
            var submissionId = Guid.NewGuid();
            var outcome = Polled( submissionId, SubmissionStatus.Failed );
            outcome.ErrorCode = "sync.apply_failed";

            var result = ChatPlatformSyncHelper.Resolve( AcceptedAck( submissionId, null ), outcome );

            Assert.IsTrue( result.IsFailure );
            StringAssert.Contains( result.Message, "sync.apply_failed" );
        }

        /// <summary>
        /// A submission the schedule replaced inside the poll window is recorded as failed, but the
        /// newer one carries everything it would have, so the run that made it did not fail.
        /// </summary>
        [TestMethod]
        public void WhenThePollFindsTheSubmissionSuperseded_TheRunDoesNotFailAndSaysItWasReplaced()
        {
            var submissionId = Guid.NewGuid();
            var outcome = Polled( submissionId, SubmissionStatus.Failed );
            outcome.ErrorCode = "sync.superseded";

            var result = ChatPlatformSyncHelper.Resolve( AcceptedAck( submissionId, null ), outcome );

            Assert.IsFalse( result.IsFailure, "a submission replaced by a newer one was reported as a failed run" );
            StringAssert.Contains( result.Message, "replaced by a newer submission" );
        }

        #endregion The polled outcome

        #region The fallback

        [TestMethod]
        public void WhenThePollIsStillAccepted_TheAcknowledgementsPreviousOutcomeIsReportedInstead()
        {
            var submissionId = Guid.NewGuid();
            var previous = PreviousOutcome( SubmissionStatus.Applied );

            var result = ChatPlatformSyncHelper.Resolve(
                AcceptedAck( submissionId, previous ),
                Polled( submissionId, SubmissionStatus.Accepted ) );

            Assert.IsFalse( result.IsFailure );
            StringAssert.Contains( result.Message, "previous" );
            StringAssert.Contains( result.Message, previous.SubmissionId.ToString() );
        }

        /// <summary>
        /// This run's own status decides whether it failed. The previous submission's failure is
        /// named, but a good run is not turned red by the one before it.
        /// </summary>
        [TestMethod]
        public void WhenThePreviousSubmissionFailed_TheRunDoesNotFailButSaysWhichSubmissionItIsTalkingAbout()
        {
            var submissionId = Guid.NewGuid();
            var previous = PreviousOutcome( SubmissionStatus.Failed );
            previous.ErrorCode = "sync.apply_failed";

            var result = ChatPlatformSyncHelper.Resolve(
                AcceptedAck( submissionId, previous ),
                Polled( submissionId, SubmissionStatus.Accepted ) );

            Assert.IsFalse( result.IsFailure, "an accepted run was failed because of the submission before it" );
            StringAssert.Contains( result.Message, "previous" );
            StringAssert.Contains( result.Message, previous.SubmissionId.ToString() );
            StringAssert.Contains( result.Message, "sync.apply_failed" );
        }

        /// <summary>
        /// A previous submission still at accepted was replaced by a newer one before the queue
        /// reached it, or is still waiting. Either way accepted is not a result.
        /// </summary>
        [TestMethod]
        public void WhenThePreviousSubmissionIsStillAccepted_ItIsReadAsSupersededOrQueued()
        {
            var submissionId = Guid.NewGuid();

            var result = ChatPlatformSyncHelper.Resolve(
                AcceptedAck( submissionId, PreviousOutcome( SubmissionStatus.Accepted ) ),
                null );

            Assert.IsFalse( result.IsFailure );
            StringAssert.Contains( result.Message, "superseded or still queued" );
            Assert.IsFalse( result.Message.Contains( "was accepted" ), "a submission that never got a result was reported as though accepted were one: " + result.Message );
        }

        [TestMethod]
        public void WhenThePreviousSubmissionWasSuperseded_ItIsNamedAsReplaced()
        {
            var submissionId = Guid.NewGuid();
            var previous = PreviousOutcome( SubmissionStatus.Failed );
            previous.ErrorCode = "sync.superseded";

            var result = ChatPlatformSyncHelper.Resolve( AcceptedAck( submissionId, previous ), null );

            Assert.IsFalse( result.IsFailure );
            StringAssert.Contains( result.Message, previous.SubmissionId + ", was replaced by a newer submission" );
        }

        [TestMethod]
        public void WhenThereIsNoPolledOutcomeAndNoPreviousOne_TheRunIsQueuedAndNotFailed()
        {
            var submissionId = Guid.NewGuid();

            var result = ChatPlatformSyncHelper.Resolve(
                AcceptedAck( submissionId, null ),
                Polled( submissionId, SubmissionStatus.Accepted ) );

            Assert.IsFalse( result.IsFailure );
            StringAssert.Contains( result.Message, "submitted, not yet applied" );
        }

        /// <summary>
        /// A poll that could not be read at all lands in the same place as one that never resolved.
        /// The submission was accepted; nothing about the reader's luck changes that.
        /// </summary>
        [TestMethod]
        public void WhenThePollCouldNotBeRead_TheRunStillReportsWhatTheAcknowledgementKnew()
        {
            var submissionId = Guid.NewGuid();

            var result = ChatPlatformSyncHelper.Resolve( AcceptedAck( submissionId, null ), null );

            Assert.IsFalse( result.IsFailure );
            Assert.IsNotNull( result.Message, "a run with nothing to report still owes its administrator a sentence" );
            StringAssert.Contains( result.Message, "submitted, not yet applied" );
        }

        #endregion The fallback

        #region A submission that never got that far

        [TestMethod]
        public void WhenTheSubmissionWasRefused_TheJobFailsWithTheNamedReasonAndNoPoll()
        {
            var submissionId = Guid.NewGuid();
            var ack = new Acknowledgement
            {
                SubmissionId = submissionId,
                Status = SubmissionStatus.Refused,
                ErrorCode = "sync.marks_regressed",
                HttpStatusCode = 422
            };

            var result = ChatPlatformSyncHelper.Resolve( ack, null );

            Assert.IsTrue( result.IsFailure );
            StringAssert.Contains( result.Message, "sync.marks_regressed" );
        }

        /// <summary>
        /// Busy on every attempt means the drain was still applying this church's previous
        /// submission. Nothing is wrong and nothing was recorded, and the next run sends a fresh
        /// restatement, so the run names it without failing.
        /// </summary>
        [TestMethod]
        public void WhenEveryAttemptWasBusy_TheRunNamesItAndDoesNotFail()
        {
            var ack = new Acknowledgement
            {
                SubmissionId = Guid.NewGuid(),
                Status = SubmissionStatus.Refused,
                ErrorCode = "sync.busy",
                HttpStatusCode = 422
            };

            var result = ChatPlatformSyncHelper.Resolve( ack, null );

            Assert.IsFalse( result.IsFailure, "a platform busy applying the previous submission was reported as a failed run" );
            StringAssert.Contains( result.Message, "still applying" );
            StringAssert.Contains( result.Message, "sync.busy" );
            StringAssert.Contains( result.Message, "the next sync" );
        }

        [TestMethod]
        public void WhenTheSubmissionNeverArrived_TheJobFailsAndCarriesTheTransportDetail()
        {
            var ack = new Acknowledgement { SubmissionId = Guid.NewGuid(), TransportDetail = "the host could not be resolved" };

            var result = ChatPlatformSyncHelper.Resolve( ack, null );

            Assert.IsTrue( result.IsFailure );
            Assert.IsNotNull( result.Message, "a run that never reached the platform still owes its administrator a sentence" );
            StringAssert.Contains( result.Message, "the host could not be resolved" );
        }

        [TestMethod]
        public void WhenThePlatformRefusedTheCredential_TheJobSaysSoRatherThanThatItCouldNotReadTheAnswer()
        {
            // The platform's gateway answers a token it cannot verify in its own shape, with no
            // submission status, and that is a statement about this church's credential rather
            // than about the version of Rock reading it.
            var ack = new Acknowledgement
            {
                SubmissionId = Guid.NewGuid(),
                ErrorCode = "No suitable key or wrong key type",
                HttpStatusCode = 401
            };

            var result = ChatPlatformSyncHelper.Resolve( ack, null );

            Assert.IsTrue( result.IsFailure );
            StringAssert.Contains( result.Message, "the chat platform refused this church's credential" );
            StringAssert.Contains( result.Message, "No suitable key or wrong key type" );
        }

        #endregion A submission that never got that far

        #region What the result names

        [TestMethod]
        public void TheResultBeginsWithTheSubmissionItSent()
        {
            // The id is how the job page, the chat blocks' Sync Now and a support request all find the
            // platform's own record of this run, and a Sync Now press never sees the id any other way.
            var submissionId = Guid.NewGuid();
            var rowCounts = new Dictionary<string, int> { { "channels", 3 }, { "aliases", 5 }, { "members", 8 }, { "badges", 1 } };

            var message = ChatPlatformSync.Describe( submissionId, rowCounts, "this restatement was applied" );

            Assert.IsTrue( message.StartsWith( "Submission " + submissionId + ":", StringComparison.Ordinal ), message );
            StringAssert.Contains( message, "3 channels, 5 people, 8 memberships and 1 badges were sent." );
            StringAssert.EndsWith( message, "this restatement was applied" );
        }

        #endregion What the result names

        #region Support

        private static Acknowledgement AcceptedAck( Guid submissionId, Outcome previous )
        {
            return new Acknowledgement
            {
                SubmissionId = submissionId,
                Status = SubmissionStatus.Accepted,
                HttpStatusCode = 200,
                PreviousOutcome = previous
            };
        }

        private static Outcome Polled( Guid submissionId, SubmissionStatus status )
        {
            return new Outcome { SubmissionId = submissionId, Status = status };
        }

        private static Outcome PreviousOutcome( SubmissionStatus status )
        {
            return new Outcome { SubmissionId = Guid.NewGuid(), Status = status };
        }

        #endregion Support
    }
}
