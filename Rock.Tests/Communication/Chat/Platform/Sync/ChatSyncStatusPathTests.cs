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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Sync;

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
    /// this run's result would be a lie that reads as a success on the cycle after a failure.
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
            var result = ChatSyncStatusPath.Resolve(
                AcceptedAck( submissionId, PreviousOutcome( ChatSyncSubmissionStatus.Failed ) ),
                Outcome( submissionId, ChatSyncSubmissionStatus.Applied ) );

            Assert.IsFalse( result.IsFailure );
            StringAssert.Contains( result.Message, "applied" );
        }

        [TestMethod]
        public void WhenThePollResolvesToAFailure_TheJobFails()
        {
            var submissionId = Guid.NewGuid();
            var outcome = Outcome( submissionId, ChatSyncSubmissionStatus.Failed );
            outcome.ErrorCode = "sync.apply_failed";

            var result = ChatSyncStatusPath.Resolve( AcceptedAck( submissionId, null ), outcome );

            Assert.IsTrue( result.IsFailure );
            StringAssert.Contains( result.Message, "sync.apply_failed" );
        }

        #endregion The polled outcome

        #region The fallback

        [TestMethod]
        public void WhenThePollIsStillAccepted_TheAcknowledgementsPreviousOutcomeIsReportedInstead()
        {
            var submissionId = Guid.NewGuid();
            var previous = PreviousOutcome( ChatSyncSubmissionStatus.Applied );

            var result = ChatSyncStatusPath.Resolve(
                AcceptedAck( submissionId, previous ),
                Outcome( submissionId, ChatSyncSubmissionStatus.Accepted ) );

            Assert.IsFalse( result.IsFailure );
            StringAssert.Contains( result.Message, "previous" );
            StringAssert.Contains( result.Message, previous.SubmissionId.ToString() );
        }

        [TestMethod]
        public void WhenThePreviousSubmissionFailed_TheJobFailsAndSaysWhichSubmissionItIsTalkingAbout()
        {
            var submissionId = Guid.NewGuid();
            var previous = PreviousOutcome( ChatSyncSubmissionStatus.Failed );
            previous.ErrorCode = "sync.apply_failed";

            var result = ChatSyncStatusPath.Resolve(
                AcceptedAck( submissionId, previous ),
                Outcome( submissionId, ChatSyncSubmissionStatus.Accepted ) );

            Assert.IsTrue( result.IsFailure );
            StringAssert.Contains( result.Message, "previous" );
            StringAssert.Contains( result.Message, previous.SubmissionId.ToString() );
            StringAssert.Contains( result.Message, "sync.apply_failed" );
        }

        [TestMethod]
        public void WhenThereIsNoPolledOutcomeAndNoPreviousOne_TheRunIsQueuedAndNotFailed()
        {
            var submissionId = Guid.NewGuid();

            var result = ChatSyncStatusPath.Resolve(
                AcceptedAck( submissionId, null ),
                Outcome( submissionId, ChatSyncSubmissionStatus.Accepted ) );

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

            var result = ChatSyncStatusPath.Resolve( AcceptedAck( submissionId, null ), null );

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
            var ack = new ChatSyncAcknowledgement
            {
                SubmissionId = submissionId,
                Status = ChatSyncSubmissionStatus.Refused,
                ErrorCode = "sync.marks_regressed",
                HttpStatusCode = 422
            };

            var result = ChatSyncStatusPath.Resolve( ack, null );

            Assert.IsTrue( result.IsFailure );
            StringAssert.Contains( result.Message, "sync.marks_regressed" );
        }

        [TestMethod]
        public void WhenTheSubmissionNeverArrived_TheJobFailsAndCarriesTheTransportDetail()
        {
            var ack = ChatSyncAcknowledgement.Unreachable( Guid.NewGuid(), "the host could not be resolved" );

            var result = ChatSyncStatusPath.Resolve( ack, null );

            Assert.IsTrue( result.IsFailure );
            Assert.IsNotNull( result.Message, "a run that never reached the platform still owes its administrator a sentence" );
            StringAssert.Contains( result.Message, "the host could not be resolved" );
        }

        #endregion A submission that never got that far

        #region Support

        private static ChatSyncAcknowledgement AcceptedAck( Guid submissionId, ChatSyncOutcome previous )
        {
            return new ChatSyncAcknowledgement
            {
                SubmissionId = submissionId,
                Status = ChatSyncSubmissionStatus.Accepted,
                HttpStatusCode = 200,
                PreviousOutcome = previous
            };
        }

        private static ChatSyncOutcome Outcome( Guid submissionId, ChatSyncSubmissionStatus status )
        {
            return new ChatSyncOutcome { SubmissionId = submissionId, Status = status };
        }

        private static ChatSyncOutcome PreviousOutcome( ChatSyncSubmissionStatus status )
        {
            return new ChatSyncOutcome { SubmissionId = Guid.NewGuid(), Status = status };
        }

        #endregion Support
    }
}
