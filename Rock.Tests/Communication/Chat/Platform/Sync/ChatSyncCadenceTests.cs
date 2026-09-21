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

using Rock.Jobs;
using Rock.Model;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Whether a sync run happens at all, and what it says about the schedule it was started on.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Two separate questions that both answer before any work is done. The backoff is the
    ///         platform's advice about its own load and binds the schedule but not a person: someone
    ///         who presses Sync Now is waiting for an answer and gets one. The cadence is the
    ///         church's own setting, and this job only remarks on it. Rewriting the expression would
    ///         mean a job silently editing a row its administrator typed, which is not a thing a
    ///         church would forgive on the one occasion it guessed wrong.
    ///     </para>
    ///     <para>
    ///         The weekday cell is the one worth keeping. A schedule that fires every weekday
    ///         morning looks daily and leaves 72 hours between Friday and Monday, so an
    ///         implementation that measures only the next gap calls it healthy. Nothing else in this
    ///         file can tell those two implementations apart.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ChatSyncCadenceTests
    {
        #region Fields

        /// <summary>
        /// A Monday, so the weekday case below walks into its Friday.
        /// </summary>
        private static readonly DateTimeOffset Monday = new DateTimeOffset( 2026, 9, 21, 9, 0, 0, TimeSpan.Zero );

        #endregion Fields

        #region The cadence warning

        [TestMethod]
        public void ACadenceAboveADay_IsSaidOutLoudAndTheExpressionIsLeftAlone()
        {
            // Two in the morning, every second day.
            var job = new ServiceJob { CronExpression = "0 0 2 1/2 * ? *" };

            var warning = ChatPlatformSync.CadenceWarning( job.CronExpression, Monday );

            Assert.IsNotNull( warning, "a schedule leaving two days between runs said nothing about it" );
            StringAssert.Contains( warning, "24 hours" );
            Assert.AreEqual( "0 0 2 1/2 * ? *", job.CronExpression, "the job rewrote a schedule its administrator set" );
            Assert.IsNull( ChatPlatformSync.BackoffSkipMessage( false, null, Monday ),
                "a slow schedule is a remark, not a reason to skip the run" );
        }

        [TestMethod]
        public void ACadenceAtTheMaximum_SaysNothing()
        {
            // Two in the morning, every day: twenty four hours exactly, and the boundary is not over it.
            Assert.IsNull( ChatPlatformSync.CadenceWarning( "0 0 2 1/1 * ? *", Monday ) );
        }

        [TestMethod]
        public void ACadenceWellUnderTheMaximum_SaysNothing()
        {
            Assert.IsNull( ChatPlatformSync.CadenceWarning( "0 0/15 * 1/1 * ? *", Monday ) );
        }

        /// <summary>
        /// The cell that separates a real cadence check from one that measured the next gap and
        /// stopped.
        /// </summary>
        [TestMethod]
        public void AWeekdayOnlySchedule_IsCaughtByTheWeekendItLeaves()
        {
            Assert.IsNotNull( ChatPlatformSync.CadenceWarning( "0 0 2 ? * MON-FRI *", Monday ),
                "Friday to Monday is seventy two hours, and only the first gap of this schedule is twenty four" );
        }

        [TestMethod]
        public void AnExpressionThatWillNotParse_IsNotAWarningAboutCadence()
        {
            Assert.IsNull( ChatPlatformSync.CadenceWarning( "not a cron expression", Monday ),
                "a schedule this job cannot read is the scheduler's problem to report, not this job's" );
            Assert.IsNull( ChatPlatformSync.BackoffSkipMessage( false, null, Monday ) );
        }

        #endregion The cadence warning

        #region The backoff

        [TestMethod]
        public void ABackoffInTheFuture_StopsTheNextScheduledRun()
        {
            var skipMessage = ChatPlatformSync.BackoffSkipMessage( false, Monday.AddMinutes( 10 ), Monday );

            Assert.IsNotNull( skipMessage, "a run that did nothing still owes its administrator a reason" );
            StringAssert.Contains( skipMessage, "backoff" );
        }

        [TestMethod]
        public void ABackoffInTheFuture_DoesNotStopAManualRun()
        {
            Assert.IsNull( ChatPlatformSync.BackoffSkipMessage( true, Monday.AddMinutes( 10 ), Monday ),
                "someone pressed a button and is waiting for an answer" );
        }

        [TestMethod]
        public void ABackoffThatHasPassed_StopsNothing()
        {
            Assert.IsNull( ChatPlatformSync.BackoffSkipMessage( false, Monday.AddMinutes( -1 ), Monday ) );
        }

        [TestMethod]
        public void NoBackoffAtAll_StopsNothing()
        {
            Assert.IsNull( ChatPlatformSync.BackoffSkipMessage( false, null, Monday ) );
            Assert.IsNull( ChatPlatformSync.BackoffSkipMessage( true, null, Monday ) );
        }

        /// <summary>
        /// A skipped run still says what the schedule looks like, because a church whose cadence is
        /// too slow and whose platform is asking for quiet has two things wrong and should be told
        /// both. The two answers are separate, so this asserts the line the job actually prints
        /// rather than that both answers exist.
        /// </summary>
        [TestMethod]
        public void ASkippedRunStillCarriesTheCadenceWarning()
        {
            var skipMessage = ChatPlatformSync.BackoffSkipMessage( false, Monday.AddMinutes( 10 ), Monday );
            var warning = ChatPlatformSync.CadenceWarning( "0 0 2 1/2 * ? *", Monday );

            Assert.IsNotNull( skipMessage );
            Assert.IsNotNull( warning );

            var line = ChatPlatformSync.Join( skipMessage, warning );

            StringAssert.Contains( line, "backoff", "the reason the run did nothing was dropped from its result line" );
            StringAssert.Contains( line, "24 hours", "the remark about the schedule was dropped from its result line" );
        }

        /// <summary>
        /// The other half of the same join: a run that went ahead says its own sentence, and says
        /// nothing about the schedule when there is nothing to say.
        /// </summary>
        [TestMethod]
        public void AHealthyScheduleAddsNothingToTheResultLine()
        {
            Assert.AreEqual( "12 channels were sent.", ChatPlatformSync.Join( "12 channels were sent.", null ) );
        }

        #endregion The backoff
    }
}
