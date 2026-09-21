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
    internal sealed class ChatSyncPollBudget
    {
        public ChatSyncPollBudget( TimeSpan interval, int maxAttempts, TimeSpan duration )
        {
            Interval = interval;
            MaxAttempts = maxAttempts;
            Duration = duration;
        }

        public TimeSpan Interval { get; }

        public int MaxAttempts { get; }

        public TimeSpan Duration { get; }

        public static ChatSyncPollBudget Manual
        {
            get { return new ChatSyncPollBudget( TimeSpan.FromSeconds( 3 ), 20, TimeSpan.FromSeconds( 60 ) ); }
        }

        public static ChatSyncPollBudget Scheduled
        {
            get { return new ChatSyncPollBudget( TimeSpan.FromSeconds( 5 ), 6, TimeSpan.FromSeconds( 30 ) ); }
        }
    }
}
