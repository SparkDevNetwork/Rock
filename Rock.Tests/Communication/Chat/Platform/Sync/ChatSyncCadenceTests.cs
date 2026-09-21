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

            var plan = ChatSyncRunGate.Plan( job, false, null, Monday );

            Assert.IsNotNull( plan.CadenceWarning, "a schedule leaving two days between runs said nothing about it" );
            StringAssert.Contains( plan.CadenceWarning, "24 hours" );
            Assert.AreEqual( "0 0 2 1/2 * ? *", job.CronExpression, "the job rewrote a schedule its administrator set" );
            Assert.IsTrue( plan.ShouldSubmit, "a slow schedule is a remark, not a reason to skip the run" );
        }

        [TestMethod]
        public void ACadenceAtTheMaximum_SaysNothing()
        {
            // Two in the morning, every day: twenty four hours exactly, and the boundary is not over it.
            var job = new ServiceJob { CronExpression = "0 0 2 1/1 * ? *" };

            var plan = ChatSyncRunGate.Plan( job, false, null, Monday );

            Assert.IsNull( plan.CadenceWarning );
        }

        [TestMethod]
        public void ACadenceWellUnderTheMaximum_SaysNothing()
        {
            var job = new ServiceJob { CronExpression = "0 0/15 * 1/1 * ? *" };

            var plan = ChatSyncRunGate.Plan( job, false, null, Monday );

            Assert.IsNull( plan.CadenceWarning );
        }

        /// <summary>
        /// The cell that separates a real cadence check from one that measured the next gap and
        /// stopped.
        /// </summary>
        [TestMethod]
        public void AWeekdayOnlySchedule_IsCaughtByTheWeekendItLeaves()
        {
            var job = new ServiceJob { CronExpression = "0 0 2 ? * MON-FRI *" };

            var plan = ChatSyncRunGate.Plan( job, false, null, Monday );

            Assert.IsNotNull( plan.CadenceWarning,
                "Friday to Monday is seventy two hours, and only the first gap of this schedule is twenty four" );
        }

        [TestMethod]
        public void AnExpressionThatWillNotParse_IsNotAWarningAboutCadence()
        {
            var job = new ServiceJob { CronExpression = "not a cron expression" };

            var plan = ChatSyncRunGate.Plan( job, false, null, Monday );

            Assert.IsNull( plan.CadenceWarning, "a schedule this job cannot read is the scheduler's problem to report, not this job's" );
            Assert.IsTrue( plan.ShouldSubmit );
        }

        #endregion The cadence warning

        #region The backoff

        [TestMethod]
        public void ABackoffInTheFuture_StopsTheNextScheduledRun()
        {
            var job = new ServiceJob { CronExpression = "0 0/15 * 1/1 * ? *" };

            var plan = ChatSyncRunGate.Plan( job, false, Monday.AddMinutes( 10 ), Monday );

            Assert.IsFalse( plan.ShouldSubmit );
            Assert.IsNotNull( plan.SkipMessage, "a run that did nothing still owes its administrator a reason" );
            StringAssert.Contains( plan.SkipMessage, "backoff" );
        }

        [TestMethod]
        public void ABackoffInTheFuture_DoesNotStopAManualRun()
        {
            var job = new ServiceJob { CronExpression = "0 0/15 * 1/1 * ? *" };

            var plan = ChatSyncRunGate.Plan( job, true, Monday.AddMinutes( 10 ), Monday );

            Assert.IsTrue( plan.ShouldSubmit, "someone pressed a button and is waiting for an answer" );
            Assert.IsNull( plan.SkipMessage );
        }

        [TestMethod]
        public void ABackoffThatHasPassed_StopsNothing()
        {
            var job = new ServiceJob { CronExpression = "0 0/15 * 1/1 * ? *" };

            var plan = ChatSyncRunGate.Plan( job, false, Monday.AddMinutes( -1 ), Monday );

            Assert.IsTrue( plan.ShouldSubmit );
        }

        [TestMethod]
        public void NoBackoffAtAll_StopsNothing()
        {
            var job = new ServiceJob { CronExpression = "0 0/15 * 1/1 * ? *" };

            Assert.IsTrue( ChatSyncRunGate.Plan( job, false, null, Monday ).ShouldSubmit );
            Assert.IsTrue( ChatSyncRunGate.Plan( job, true, null, Monday ).ShouldSubmit );
        }

        /// <summary>
        /// A skipped run still says what the schedule looks like, because a church whose cadence is
        /// too slow and whose platform is asking for quiet has two things wrong and should be told
        /// both.
        /// </summary>
        [TestMethod]
        public void ASkippedRunStillCarriesTheCadenceWarning()
        {
            var job = new ServiceJob { CronExpression = "0 0 2 1/2 * ? *" };

            var plan = ChatSyncRunGate.Plan( job, false, Monday.AddMinutes( 10 ), Monday );

            Assert.IsFalse( plan.ShouldSubmit );
            Assert.IsNotNull( plan.CadenceWarning );
        }

        #endregion The backoff
    }
}
