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
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Cadence clamp, backpressure, and the poll that turns a queued submit
    /// into this run's result.
    /// </summary>
    [TestClass]
    public class ChatSyncRunnerTests
    {
        [TestMethod]
        public void ClampCronExpression_Weekly_BecomesDaily()
        {
            var now = new DateTimeOffset( 2026, 9, 17, 0, 0, 0, TimeSpan.Zero );
            var clamped = ChatSyncRunner.ClampCronExpression( "0 0 0 ? * SUN *", now );

            Assert.AreEqual( ChatSyncRunner.DailyCron, clamped );
        }

        [TestMethod]
        public void ClampCronExpression_Hourly_IsUnchanged()
        {
            var now = new DateTimeOffset( 2026, 9, 17, 0, 0, 0, TimeSpan.Zero );
            const string hourly = "0 0 0/1 1/1 * ? *";

            Assert.AreEqual( hourly, ChatSyncRunner.ClampCronExpression( hourly, now ) );
        }

        [TestMethod]
        public void ShouldDelay_HonoursBackoffOnScheduled_AndIgnoresItWhenManual()
        {
            var future = new DateTime( 2026, 9, 18, 0, 0, 0, DateTimeKind.Utc );
            var now = new DateTime( 2026, 9, 17, 0, 0, 0, DateTimeKind.Utc );

            Assert.IsTrue( ChatSyncRunner.ShouldDelay( future, isManual: false, now ) );
            Assert.IsFalse( ChatSyncRunner.ShouldDelay( future, isManual: true, now ) );
            Assert.IsFalse( ChatSyncRunner.ShouldDelay( now.AddMinutes( -1 ), isManual: false, now ) );
        }

        [TestMethod]
        public async Task PollUntilSettled_AcceptedThenApplied_ReturnsApplied()
        {
            var clock = new DateTime( 2026, 9, 17, 0, 0, 0, DateTimeKind.Utc );
            var calls = 0;
            var sleeps = new List<TimeSpan>();

            var outcome = await ChatSyncRunner.PollUntilSettledAsync(
                () =>
                {
                    calls++;
                    var status = calls == 1 ? "accepted" : "applied";
                    return Task.FromResult( new ChatSyncOutcome { HttpStatus = 200, Status = status } );
                },
                delay =>
                {
                    sleeps.Add( delay );
                    clock = clock.Add( delay );
                },
                () => clock );

            Assert.AreEqual( "applied", outcome.Status );
            Assert.AreEqual( 2, calls );
            Assert.AreEqual( ChatSyncRunner.InitialPollDelay, sleeps[0] );
        }

        [TestMethod]
        public async Task PollUntilSettled_StillAcceptedAtBudget_ReturnsThePendingOutcome()
        {
            var clock = new DateTime( 2026, 9, 17, 0, 0, 0, DateTimeKind.Utc );

            var outcome = await ChatSyncRunner.PollUntilSettledAsync(
                () => Task.FromResult( new ChatSyncOutcome { HttpStatus = 200, Status = "accepted" } ),
                delay => { clock = clock.Add( delay ); },
                () => clock );

            Assert.AreEqual( "accepted", outcome.Status );
            Assert.IsTrue( outcome.IsPending );
        }
    }
}
