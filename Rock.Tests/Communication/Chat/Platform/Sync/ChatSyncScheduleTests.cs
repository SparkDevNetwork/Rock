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
    ///         who presses Sync Now is waiting for an answer and gets one. The schedule gap is the
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
    public class ChatSyncScheduleTests
    {
        #region Fields

        /// <summary>
        /// A Monday, so the weekday case below walks into its Friday.
        /// </summary>
        private static readonly DateTimeOffset Monday = new DateTimeOffset( 2026, 9, 21, 9, 0, 0, TimeSpan.Zero );

        #endregion Fields

        #region The schedule warning

        [TestMethod]
        public void AScheduleSlowerThanADay_IsSaidOutLoudAndTheExpressionIsLeftAlone()
        {
            // Two in the morning, every second day.
            var job = new ServiceJob { CronExpression = "0 0 2 1/2 * ? *" };

            var warning = ChatPlatformSync.ScheduleWarning( job.CronExpression, Monday );

            Assert.IsNotNull( warning, "a schedule leaving two days between runs said nothing about it" );
            StringAssert.Contains( warning, "24 hours" );
            Assert.AreEqual( "0 0 2 1/2 * ? *", job.CronExpression, "the job rewrote a schedule its administrator set" );
            Assert.IsNull( ChatPlatformSync.SkipReason( false, null, Monday ),
                "a slow schedule is a remark, not a reason to skip the run" );
        }

        [TestMethod]
        public void AScheduleAtTheMaximumGap_SaysNothing()
        {
            // Two in the morning, every day: twenty four hours exactly, and the boundary is not over it.
            Assert.IsNull( ChatPlatformSync.ScheduleWarning( "0 0 2 1/1 * ? *", Monday ) );
        }

        [TestMethod]
        public void AScheduleWellUnderTheMaximumGap_SaysNothing()
        {
            Assert.IsNull( ChatPlatformSync.ScheduleWarning( "0 0/15 * 1/1 * ? *", Monday ) );
        }

        /// <summary>
        /// The cell that separates a real schedule check from one that measured the next gap and
        /// stopped.
        /// </summary>
        [TestMethod]
        public void AWeekdayOnlySchedule_IsCaughtByTheWeekendItLeaves()
        {
            Assert.IsNotNull( ChatPlatformSync.ScheduleWarning( "0 0 2 ? * MON-FRI *", Monday ),
                "Friday to Monday is seventy two hours, and only the first gap of this schedule is twenty four" );
        }

        /// <summary>
        /// The cell that separates a walk across a week from one that stops after a handful of
        /// fires. The first fourteen gaps here are an hour, or the fourteen hours overnight, and
        /// Friday evening to Monday morning sits past all of them.
        /// </summary>
        [TestMethod]
        public void AnHourlyWeekdaySchedule_IsCaughtByTheWeekendItLeaves()
        {
            Assert.IsNotNull( ChatPlatformSync.ScheduleWarning( "0 0 8-18 ? * MON-FRI *", Monday ),
                "Friday evening to Monday morning is more than a day, and the first fourteen fires of this schedule never reach it" );
        }

        /// <summary>
        /// The same hours on every day leave an overnight, which is under a day, so saying so would
        /// be the warning firing on a schedule that meets the bound.
        /// </summary>
        [TestMethod]
        public void AnHourlyScheduleEveryDay_SaysNothing()
        {
            Assert.IsNull( ChatPlatformSync.ScheduleWarning( "0 0 8-18 * * ? *", Monday ) );
        }

        [TestMethod]
        public void AnExpressionThatWillNotParse_IsNotAWarningAboutTheSchedule()
        {
            Assert.IsNull( ChatPlatformSync.ScheduleWarning( "not a cron expression", Monday ),
                "a schedule this job cannot read is the scheduler's problem to report, not this job's" );
            Assert.IsNull( ChatPlatformSync.SkipReason( false, null, Monday ) );
        }

        #endregion The schedule warning

        #region The backoff

        [TestMethod]
        public void ABackoffInTheFuture_StopsTheNextScheduledRun()
        {
            var skipReason = ChatPlatformSync.SkipReason( false, Monday.AddMinutes( 10 ), Monday );

            Assert.IsNotNull( skipReason, "a run that did nothing still owes its administrator a reason" );
            StringAssert.Contains( skipReason, "backoff" );
        }

        [TestMethod]
        public void ABackoffInTheFuture_DoesNotStopAManualRun()
        {
            Assert.IsNull( ChatPlatformSync.SkipReason( true, Monday.AddMinutes( 10 ), Monday ),
                "someone pressed a button and is waiting for an answer" );
        }

        [TestMethod]
        public void ABackoffThatHasPassed_StopsNothing()
        {
            Assert.IsNull( ChatPlatformSync.SkipReason( false, Monday.AddMinutes( -1 ), Monday ) );
        }

        [TestMethod]
        public void NoBackoffAtAll_StopsNothing()
        {
            Assert.IsNull( ChatPlatformSync.SkipReason( false, null, Monday ) );
            Assert.IsNull( ChatPlatformSync.SkipReason( true, null, Monday ) );
        }

        /// <summary>
        /// A skipped run still says what the schedule looks like, because a church whose schedule is
        /// too slow and whose platform is asking for quiet has two things wrong and should be told
        /// both. The two answers are separate, so this asserts the line the job actually prints
        /// rather than that both answers exist.
        /// </summary>
        [TestMethod]
        public void ASkippedRunStillCarriesTheScheduleWarning()
        {
            var skipReason = ChatPlatformSync.SkipReason( false, Monday.AddMinutes( 10 ), Monday );
            var warning = ChatPlatformSync.ScheduleWarning( "0 0 2 1/2 * ? *", Monday );

            Assert.IsNotNull( skipReason );
            Assert.IsNotNull( warning );

            var line = ChatPlatformSync.Join( skipReason, warning );

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
