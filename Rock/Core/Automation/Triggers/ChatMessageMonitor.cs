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
using System.Collections.Concurrent;
using System.Collections.Generic;

using Rock.Communication.Chat.Sync;
using Rock.Web.Cache;

namespace Rock.Core.Automation.Triggers
{
    /// <summary>
    /// The monitor for the <see cref="ChatMessage"/> trigger. This does not actually monitor for chat messages, but
    /// is responsible for executing automation events for any currently registered triggers, as message events are
    /// received.
    /// </summary>
    internal sealed class ChatMessageMonitor : IDisposable
    {
        #region Fields

        /// <summary>
        /// The dictionary of chat message triggers that are currently registered, to execute automation events for any
        /// message events that are received.
        /// </summary>
        private static readonly ConcurrentDictionary<int, ChatMessageMonitor> _monitors = new ConcurrentDictionary<int, ChatMessageMonitor>();

        /// <summary>
        /// The identifier of the automation trigger that this monitor is registered for.
        /// </summary>
        private readonly int _triggerId;

        /// <summary>
        /// The criteria object that will handle checking chat messages to see if they match and the events should be executed.
        /// </summary>
        private readonly ChatMessageCriteria _criteria;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ChatMessageMonitor"/> class.
        /// </summary>
        /// <param name="triggerId">The automation trigger identifier this monitor represents.</param>
        /// <param name="criteria">The criteria that will be used to check if entity entries match.</param>
        public ChatMessageMonitor( int triggerId, ChatMessageCriteria criteria )
        {
            _triggerId = triggerId;
            _criteria = criteria;

            _monitors.TryAdd( _triggerId, this );
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _monitors.TryRemove( _triggerId, out var monitor );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the Chat-to-Rock message event and executes any automation events that are attached to monitored triggers.
        /// </summary>
        /// <param name="messageEvent">The Chat-to</param>
        public static void ProcessMessageEvent( ChatToRockMessageEvent messageEvent )
        {
            var request = new AutomationRequest
            {
                Values = new Dictionary<string, object>
                {
                    ["ChannelType"] = messageEvent.ChannelType,
                    ["Channel"] = messageEvent.Channel,
                    ["Person"] = messageEvent.Person,
                    ["Message"] = messageEvent.Message,
                    ["MemberChatPersonKeys"] = messageEvent.MemberChatPersonKeys
                }
            };

            var monitors = _monitors.Values;

            foreach ( var monitor in monitors )
            {
                if ( monitor._criteria.IsMatch( messageEvent ) )
                {
                    AutomationEventCache.ExecuteEvents( monitor._triggerId, request );
                }
            }
        }

        #endregion
    }
}
