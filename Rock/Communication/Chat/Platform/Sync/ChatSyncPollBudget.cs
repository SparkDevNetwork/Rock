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

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// How long a run waits to learn what became of the submission it just made.
    /// </summary>
    internal sealed class ChatSyncPollBudget
    {
        /// <summary>
        /// Creates a budget.
        /// </summary>
        /// <param name="interval">How long to wait between reads.</param>
        /// <param name="maxAttempts">How many reads at most.</param>
        /// <param name="duration">How long the whole wait may take.</param>
        public ChatSyncPollBudget( TimeSpan interval, int maxAttempts, TimeSpan duration )
        {
            Interval = interval;
            MaxAttempts = maxAttempts;
            Duration = duration;
        }

        /// <summary>
        /// How long to wait between reads.
        /// </summary>
        public TimeSpan Interval { get; }

        /// <summary>
        /// How many reads at most.
        /// </summary>
        public int MaxAttempts { get; }

        /// <summary>
        /// How long the whole wait may take.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// The budget for a sync someone pressed a button for. Longer than the scheduled one,
        /// because a person is waiting and there is nothing to fall back on. Every figure here is
        /// an estimate, revisited when the platform is measured at full scale.
        /// </summary>
        public static ChatSyncPollBudget Manual
        {
            get { return new ChatSyncPollBudget( TimeSpan.FromSeconds( 3 ), 20, TimeSpan.FromSeconds( 60 ) ); }
        }

        /// <summary>
        /// The budget for a sync the schedule started. Shorter, because nobody is watching and the
        /// acknowledgement's previous outcome is there to fall back on. Every figure here is an
        /// estimate, revisited when the platform is measured at full scale.
        /// </summary>
        public static ChatSyncPollBudget Scheduled
        {
            get { return new ChatSyncPollBudget( TimeSpan.FromSeconds( 5 ), 6, TimeSpan.FromSeconds( 30 ) ); }
        }
    }
}
