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
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Sync;
using Rock.ViewModels.Blocks.Communication.Chat.ChatSyncNow;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What Sync Now on the Chat Configuration and Group Type Detail blocks may do, and what it
    /// reports afterwards.
    /// </summary>
    /// <remarks>
    /// A press asks Rock to run the Chat Platform Sync job now and returns at once; the screen then
    /// checks on that run until it ends. The two things worth pinning are that a press never starts
    /// a run it should not, and that a check never reports somebody else's run as this press's
    /// result: a run that ended just before the press would otherwise read as its answer.
    /// </remarks>
    [TestClass]
    public class ChatSyncNowTests
    {
        private const int JobId = 42;

        #region A press

        [TestMethod]
        public void Request_WhenChatWasNeverEnabled_RefusesAndSaysSo()
        {
            var queued = new List<int>();

            var result = ChatPlatformSyncHelper.RequestSyncNow( new ChatPlatformConfiguration(), true, () => IdleJob( 900 ), queued.Add );

            Assert.IsTrue( result.IsRefused );
            Assert.IsFalse( result.IsForbidden );
            StringAssert.Contains( result.RefusalMessage, "not set up" );
            Assert.AreEqual( 0, queued.Count, "a press ran the sync for a church with no chat" );
        }

        [TestMethod]
        public void Request_WhenTheSigningKeyCannotBeRead_RefusesWithItsOwnReason()
        {
            var queued = new List<int>();
            var unreadable = Configured();
            unreadable.PrivateKey = null;

            var result = ChatPlatformSyncHelper.RequestSyncNow( unreadable, true, () => IdleJob( 900 ), queued.Add );
            var neverEnabled = ChatPlatformSyncHelper.RequestSyncNow( new ChatPlatformConfiguration(), true, () => IdleJob( 900 ), queued.Add );

            Assert.IsTrue( result.IsRefused );
            StringAssert.Contains( result.RefusalMessage, "signing key" );
            Assert.AreNotEqual( neverEnabled.RefusalMessage, result.RefusalMessage, "the two reasons read the same, so an administrator cannot tell which one to fix" );
            Assert.AreEqual( 0, queued.Count );
        }

        [TestMethod]
        public void Request_ForACallerWhoMayNotSave_IsForbidden()
        {
            var queued = new List<int>();

            var result = ChatPlatformSyncHelper.RequestSyncNow( Configured(), false, () => IdleJob( 900 ), queued.Add );

            Assert.IsTrue( result.IsRefused );
            Assert.IsTrue( result.IsForbidden );
            Assert.IsNull( result.Status );
            Assert.AreEqual( 0, queued.Count, "a caller who may not save started a sync" );
        }

        [TestMethod]
        public void Request_WhenTheSyncJobIsMissing_Refuses()
        {
            var queued = new List<int>();

            var result = ChatPlatformSyncHelper.RequestSyncNow( Configured(), true, () => null, queued.Add );

            Assert.IsTrue( result.IsRefused );
            Assert.IsFalse( result.IsForbidden );
            StringAssert.Contains( result.RefusalMessage, "Chat Platform Sync" );
            Assert.AreEqual( 0, queued.Count );
        }

        [TestMethod]
        public void Request_QueuesOneRunNowOfTheSyncJobAndNoOther()
        {
            var queued = new List<int>();

            var result = ChatPlatformSyncHelper.RequestSyncNow( Configured(), true, () => IdleJob( 900 ), queued.Add );

            Assert.IsFalse( result.IsRefused );
            CollectionAssert.AreEqual( new[] { JobId }, queued );
            Assert.AreEqual( 900, result.Status.RunMarker, "the press must be reported from the run after the newest one already recorded" );
            Assert.IsFalse( result.Status.IsFinished );
        }

        [TestMethod]
        public void Request_ForAJobThatHasNeverRun_ReportsFromTheFirstRun()
        {
            var queued = new List<int>();
            var job = IdleJob( 900 );
            job.LatestRunId = null;

            var result = ChatPlatformSyncHelper.RequestSyncNow( Configured(), true, () => job, queued.Add );

            CollectionAssert.AreEqual( new[] { JobId }, queued );
            Assert.AreEqual( 0, result.Status.RunMarker );
        }

        [TestMethod]
        public void Request_WhileTheJobHoldsItsLock_QueuesNothingAndSaysToPressAgain()
        {
            // Whatever holds the lock read the church before this press, so its result is not an
            // answer to the press. The lock is also held after a run's record has ended, while Rock
            // sends the job's notification, which is why the lock and not the record is what decides.
            var queued = new List<int>();
            var job = new ChatPlatformSyncHelper.JobSnapshot { JobId = JobId, IsRunning = true, LatestRunId = 900 };

            var result = ChatPlatformSyncHelper.RequestSyncNow( Configured(), true, () => job, queued.Add );

            Assert.IsTrue( result.IsRefused );
            Assert.IsFalse( result.IsForbidden );
            Assert.IsNull( result.Status, "a refused press was given a run to follow" );
            StringAssert.Contains( result.RefusalMessage, "already running" );
            StringAssert.Contains( result.RefusalMessage, "again" );
            Assert.AreEqual( 0, queued.Count, "a second run was asked for while one held the lock, and the lock would refuse it without a word" );
        }

        #endregion A press

        #region A check

        [TestMethod]
        public void Status_WithNoRunRecordedAfterTheMarker_IsNotFinished()
        {
            var result = ChatPlatformSyncHelper.GetSyncNowStatus( true, 900, () => null );

            Assert.IsFalse( result.IsRefused );
            Assert.IsFalse( result.Status.IsFinished );
            Assert.AreEqual( 900, result.Status.RunMarker );
        }

        [TestMethod]
        public void Status_WhileTheRunIsGoing_IsNotFinished()
        {
            var run = new ChatPlatformSyncHelper.RunSnapshot { Id = 901, HasEnded = false, Status = "Running" };

            var result = ChatPlatformSyncHelper.GetSyncNowStatus( true, 900, () => run );

            Assert.IsFalse( result.Status.IsFinished );
        }

        [TestMethod]
        public void Status_WhenTheRunHasEnded_ReportsItsOwnResult()
        {
            var run = Ended( 901, "Success", "Submission 1: 3 channels were sent. this restatement was applied" );

            var result = ChatPlatformSyncHelper.GetSyncNowStatus( true, 900, () => run );

            Assert.IsTrue( result.Status.IsFinished );
            Assert.IsFalse( result.Status.IsFailure );
            Assert.AreEqual( run.StatusMessage, result.Status.Message );
        }

        [TestMethod]
        public void Status_WhenTheRunEndedInAWarningOrAnException_IsAFailure()
        {
            foreach ( var status in new[] { "Warning", "Exception" } )
            {
                var result = ChatPlatformSyncHelper.GetSyncNowStatus( true, 900, () => Ended( 901, status, "the chat platform refused this restatement" ) );

                Assert.IsTrue( result.Status.IsFinished, status );
                Assert.IsTrue( result.Status.IsFailure, status );
                Assert.AreEqual( "the chat platform refused this restatement", result.Status.Message, status );
            }
        }

        [TestMethod]
        public void Status_NeverReportsARunRecordedAtOrBeforeTheMarker()
        {
            var result = ChatPlatformSyncHelper.GetSyncNowStatus( true, 900, () => Ended( 900, "Success", "an earlier run" ) );

            Assert.IsFalse( result.Status.IsFinished, "a run that ended before the press was reported as its result" );
            Assert.AreNotEqual( "an earlier run", result.Status.Message );
        }

        [TestMethod]
        public void Status_ForACallerWhoMayNotSave_IsForbidden()
        {
            var result = ChatPlatformSyncHelper.GetSyncNowStatus( false, 900, () => Ended( 901, "Success", "done" ) );

            Assert.IsTrue( result.IsForbidden );
            Assert.IsNull( result.Status );
        }

        [TestMethod]
        public void Status_ForACallerWhoMayNotSave_NeverReadsTheJobsHistory()
        {
            var reads = 0;

            ChatPlatformSyncHelper.GetSyncNowStatus( false, 900, () =>
            {
                reads++;
                return null;
            } );

            Assert.AreEqual( 0, reads, "a caller who may not press the button had the job's history read for them" );
        }

        #endregion A check

        #region What a refused press touches

        [TestMethod]
        public void Request_RefusedForAuthorityOrSetup_NeverReadsTheJobOrProbesItsLock()
        {
            // Reading the job includes probing its lock, and a probe that lands on the instant the
            // schedule fires takes the lock first and costs the church that scheduled run. A press that
            // is going to be refused has no business doing either.
            var reads = 0;
            var queued = new List<int>();
            Func<ChatPlatformSyncHelper.JobSnapshot> readJob = () =>
            {
                reads++;
                return IdleJob( 900 );
            };

            ChatPlatformSyncHelper.RequestSyncNow( Configured(), false, readJob, queued.Add );
            ChatPlatformSyncHelper.RequestSyncNow( new ChatPlatformConfiguration(), true, readJob, queued.Add );

            var unreadable = Configured();
            unreadable.PrivateKey = null;
            ChatPlatformSyncHelper.RequestSyncNow( unreadable, true, readJob, queued.Add );

            Assert.AreEqual( 0, reads );
            Assert.AreEqual( 0, queued.Count );
        }

        #endregion What a refused press touches

        #region The block's answer

        [TestMethod]
        public void ActionResult_AnswersAsBothBlocksDid()
        {
            var forbidden = ChatPlatformSyncHelper.ToActionResult( ChatPlatformSyncHelper.RequestSyncNow( Configured(), false, () => IdleJob( 900 ), id => { } ) );
            var refused = ChatPlatformSyncHelper.ToActionResult( ChatPlatformSyncHelper.RequestSyncNow( Configured(), true, () => null, id => { } ) );
            var waiting = ChatPlatformSyncHelper.RequestSyncNow( Configured(), true, () => IdleJob( 900 ), id => { } );
            var ok = ChatPlatformSyncHelper.ToActionResult( waiting );

            Assert.AreEqual( HttpStatusCode.Forbidden, forbidden.StatusCode );
            StringAssert.Contains( forbidden.Error, "not authorized" );
            Assert.AreEqual( HttpStatusCode.BadRequest, refused.StatusCode );
            StringAssert.Contains( refused.Error, "Chat Platform Sync" );
            Assert.AreEqual( HttpStatusCode.OK, ok.StatusCode );
            Assert.AreSame( waiting.Status, ok.Content );
            Assert.AreEqual( typeof( ChatSyncNowStatusBag ), ok.ContentClrType );
        }

        #endregion The block's answer

        #region Helpers

        private static ChatPlatformConfiguration Configured()
        {
            return new ChatPlatformConfiguration
            {
                PrivateKey = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"kid-1\",\"d\":\"secret-part\"}",
                TenantId = Guid.Parse( "11111111-1111-4111-8111-111111111111" ),
                ProjectUrl = "https://example.supabase.co",
                PublishableKey = "sb_publishable_test",
                Kid = "platform-kid-1"
            };
        }

        private static ChatPlatformSyncHelper.JobSnapshot IdleJob( int latestRunId )
        {
            return new ChatPlatformSyncHelper.JobSnapshot { JobId = JobId, IsRunning = false, LatestRunId = latestRunId };
        }

        private static ChatPlatformSyncHelper.RunSnapshot Ended( int id, string status, string message )
        {
            return new ChatPlatformSyncHelper.RunSnapshot { Id = id, HasEnded = true, Status = status, StatusMessage = message };
        }

        #endregion Helpers
    }
}
