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
        /// The lock object used to synchronize updates to <see cref="_monitors"/>.
        /// This is only used when making changes, not when reading the value.
        /// </summary>
        private static readonly object _monitorsLock = new object();

        /// <summary>
        /// The dictionary of chat message triggers that are currently registered, to execute automation events for any
        /// message events that are received.
        /// </summary>
        private static IReadOnlyDictionary<int, ChatMessageMonitor> _monitors = new Dictionary<int, ChatMessageMonitor>();

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

            lock ( _monitorsLock )
            {
                var monitors = DuplicateMonitors();

                monitors.Add( _triggerId, this );

                _monitors = monitors;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            //_monitors.TryRemove( _triggerId, out var monitor );

            lock ( _monitorsLock )
            {
                var monitors = DuplicateMonitors();

                monitors.Remove( _triggerId );

                _monitors = monitors;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Duplicates the current monitor dictionary. This is used to ensure that any changes we make to the dictionary
        /// or the list of monitors do not affect the current lookup table, which could cause a crash.
        /// </summary>
        /// <returns>A new dictionary that represents the same data as <see cref="_monitors"/>.</returns>
        private static Dictionary<int, ChatMessageMonitor> DuplicateMonitors()
        {
            var monitors = new Dictionary<int, ChatMessageMonitor>( _monitors.Count );

            foreach ( var kvp in _monitors )
            {
                monitors[kvp.Key] = kvp.Value;
            }

            return monitors;
        }

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
                    [ChatMessage.AutomationRequestValueKey.ChannelType] = messageEvent.ChannelType,
                    [ChatMessage.AutomationRequestValueKey.Channel] = messageEvent.Channel,
                    [ChatMessage.AutomationRequestValueKey.SenderPerson] = messageEvent.SenderPerson,
                    [ChatMessage.AutomationRequestValueKey.Message] = messageEvent.Message,
                    [ChatMessage.AutomationRequestValueKey.SentRockDateTime] = messageEvent.RockDateTime,
                    [ChatMessage.AutomationRequestValueKey.EventChannelMembers] = messageEvent.EventChannelMembers
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
