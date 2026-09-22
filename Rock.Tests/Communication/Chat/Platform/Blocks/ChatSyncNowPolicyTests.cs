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

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Blocks
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
    public class ChatSyncNowPolicyTests
    {
        private const int JobId = 42;

        #region A press

        [TestMethod]
        public void Request_WhenChatWasNeverEnabled_RefusesAndSaysSo()
        {
            var queued = new List<int>();

            var result = ChatSyncNowPolicy.Request( new ChatPlatformConfiguration(), true, () => IdleJob( 900 ), queued.Add );

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

            var result = ChatSyncNowPolicy.Request( unreadable, true, () => IdleJob( 900 ), queued.Add );
            var neverEnabled = ChatSyncNowPolicy.Request( new ChatPlatformConfiguration(), true, () => IdleJob( 900 ), queued.Add );

            Assert.IsTrue( result.IsRefused );
            StringAssert.Contains( result.RefusalMessage, "signing key" );
            Assert.AreNotEqual( neverEnabled.RefusalMessage, result.RefusalMessage, "the two reasons read the same, so an administrator cannot tell which one to fix" );
            Assert.AreEqual( 0, queued.Count );
        }

        [TestMethod]
        public void Request_ForACallerWhoMayNotSave_IsForbidden()
        {
            var queued = new List<int>();

            var result = ChatSyncNowPolicy.Request( Configured(), false, () => IdleJob( 900 ), queued.Add );

            Assert.IsTrue( result.IsRefused );
            Assert.IsTrue( result.IsForbidden );
            Assert.IsNull( result.Status );
            Assert.AreEqual( 0, queued.Count, "a caller who may not save started a sync" );
        }

        [TestMethod]
        public void Request_WhenTheSyncJobIsMissing_Refuses()
        {
            var queued = new List<int>();

            var result = ChatSyncNowPolicy.Request( Configured(), true, () => null, queued.Add );

            Assert.IsTrue( result.IsRefused );
            Assert.IsFalse( result.IsForbidden );
            StringAssert.Contains( result.RefusalMessage, "Chat Platform Sync" );
            Assert.AreEqual( 0, queued.Count );
        }

        [TestMethod]
        public void Request_QueuesOneRunNowOfTheSyncJobAndNoOther()
        {
            var queued = new List<int>();

            var result = ChatSyncNowPolicy.Request( Configured(), true, () => IdleJob( 900 ), queued.Add );

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

            var result = ChatSyncNowPolicy.Request( Configured(), true, () => job, queued.Add );

            CollectionAssert.AreEqual( new[] { JobId }, queued );
            Assert.AreEqual( 0, result.Status.RunMarker );
        }

        [TestMethod]
        public void Request_WhileARunIsInFlight_QueuesNothingAndFollowsThatRun()
        {
            var queued = new List<int>();
            var job = new ChatSyncNowPolicy.JobSnapshot { JobId = JobId, IsRunning = true, LatestRunId = 900, IsLatestRunEnded = false };

            var result = ChatSyncNowPolicy.Request( Configured(), true, () => job, queued.Add );

            Assert.IsFalse( result.IsRefused );
            Assert.AreEqual( 0, queued.Count, "a second run was asked for while one held the lock, and the lock would refuse it without a word" );
            Assert.AreEqual( 899, result.Status.RunMarker, "the press must report the run already in flight" );
        }

        [TestMethod]
        public void Request_WhileARunHoldsTheLockButIsNotRecordedYet_WaitsForTheNextRecord()
        {
            var queued = new List<int>();
            var job = new ChatSyncNowPolicy.JobSnapshot { JobId = JobId, IsRunning = true, LatestRunId = 900, IsLatestRunEnded = true };

            var result = ChatSyncNowPolicy.Request( Configured(), true, () => job, queued.Add );

            Assert.AreEqual( 0, queued.Count );
            Assert.AreEqual( 900, result.Status.RunMarker, "the newest record has ended, so it is not the run holding the lock" );
        }

        #endregion A press

        #region A check

        [TestMethod]
        public void Status_WithNoRunRecordedAfterTheMarker_IsNotFinished()
        {
            var result = ChatSyncNowPolicy.Status( true, 900, () => null );

            Assert.IsFalse( result.IsRefused );
            Assert.IsFalse( result.Status.IsFinished );
            Assert.AreEqual( 900, result.Status.RunMarker );
        }

        [TestMethod]
        public void Status_WhileTheRunIsGoing_IsNotFinished()
        {
            var run = new ChatSyncNowPolicy.RunSnapshot { Id = 901, HasEnded = false, Status = "Running" };

            var result = ChatSyncNowPolicy.Status( true, 900, () => run );

            Assert.IsFalse( result.Status.IsFinished );
        }

        [TestMethod]
        public void Status_WhenTheRunHasEnded_ReportsItsOwnResult()
        {
            var run = Ended( 901, "Success", "Submission 1: 3 channels were sent. this restatement was applied" );

            var result = ChatSyncNowPolicy.Status( true, 900, () => run );

            Assert.IsTrue( result.Status.IsFinished );
            Assert.IsFalse( result.Status.IsFailure );
            Assert.AreEqual( run.StatusMessage, result.Status.Message );
        }

        [TestMethod]
        public void Status_WhenTheRunEndedInAWarningOrAnException_IsAFailure()
        {
            foreach ( var status in new[] { "Warning", "Exception" } )
            {
                var result = ChatSyncNowPolicy.Status( true, 900, () => Ended( 901, status, "the chat platform refused this restatement" ) );

                Assert.IsTrue( result.Status.IsFinished, status );
                Assert.IsTrue( result.Status.IsFailure, status );
                Assert.AreEqual( "the chat platform refused this restatement", result.Status.Message, status );
            }
        }

        [TestMethod]
        public void Status_NeverReportsARunRecordedAtOrBeforeTheMarker()
        {
            var result = ChatSyncNowPolicy.Status( true, 900, () => Ended( 900, "Success", "an earlier run" ) );

            Assert.IsFalse( result.Status.IsFinished, "a run that ended before the press was reported as its result" );
            Assert.AreNotEqual( "an earlier run", result.Status.Message );
        }

        [TestMethod]
        public void Status_ForACallerWhoMayNotSave_IsForbidden()
        {
            var result = ChatSyncNowPolicy.Status( false, 900, () => Ended( 901, "Success", "done" ) );

            Assert.IsTrue( result.IsForbidden );
            Assert.IsNull( result.Status );
        }

        [TestMethod]
        public void Status_ForACallerWhoMayNotSave_NeverReadsTheJobsHistory()
        {
            var reads = 0;

            ChatSyncNowPolicy.Status( false, 900, () =>
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
            Func<ChatSyncNowPolicy.JobSnapshot> readJob = () =>
            {
                reads++;
                return IdleJob( 900 );
            };

            ChatSyncNowPolicy.Request( Configured(), false, readJob, queued.Add );
            ChatSyncNowPolicy.Request( new ChatPlatformConfiguration(), true, readJob, queued.Add );

            var unreadable = Configured();
            unreadable.PrivateKey = null;
            ChatSyncNowPolicy.Request( unreadable, true, readJob, queued.Add );

            Assert.AreEqual( 0, reads );
            Assert.AreEqual( 0, queued.Count );
        }

        #endregion What a refused press touches

        #region Group Type Detail

        [TestMethod]
        public void ChatSection_IsShownWhenEitherChatProviderIsConfigured()
        {
            Assert.IsTrue( ChatSyncNowPolicy.IsChatSectionShown( true, new ChatPlatformConfiguration() ), "a church on the previous provider lost its chat settings" );
            Assert.IsTrue( ChatSyncNowPolicy.IsChatSectionShown( false, Configured() ), "a church on this platform sees no chat settings on a group type" );
            Assert.IsFalse( ChatSyncNowPolicy.IsChatSectionShown( false, new ChatPlatformConfiguration() ) );
        }

        [TestMethod]
        public void ChatPlatformConfigured_IsTrueOnlyWhenThisPlatformCanRun()
        {
            var unreadable = Configured();
            unreadable.PrivateKey = null;

            Assert.IsTrue( ChatSyncNowPolicy.IsChatPlatformConfigured( Configured() ) );
            Assert.IsFalse( ChatSyncNowPolicy.IsChatPlatformConfigured( unreadable ) );
            Assert.IsFalse( ChatSyncNowPolicy.IsChatPlatformConfigured( new ChatPlatformConfiguration() ) );
        }

        #endregion Group Type Detail

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

        private static ChatSyncNowPolicy.JobSnapshot IdleJob( int latestRunId )
        {
            return new ChatSyncNowPolicy.JobSnapshot { JobId = JobId, IsRunning = false, LatestRunId = latestRunId, IsLatestRunEnded = true };
        }

        private static ChatSyncNowPolicy.RunSnapshot Ended( int id, string status, string message )
        {
            return new ChatSyncNowPolicy.RunSnapshot { Id = id, HasEnded = true, Status = status, StatusMessage = message };
        }

        #endregion Helpers
    }
}
