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
using System.Linq;
using System.Linq.Dynamic.Core;

using Microsoft.Extensions.Logging;

using Rock.Communication.Chat.Sync;
using Rock.Logging;
using Rock.ViewModels.Core.Automation.Triggers;

namespace Rock.Core.Automation.Triggers
{
    /// <summary>
    /// Represents the criteria defined for a single Chat Message automation
    /// trigger. This is cached to get the best performance we can and is
    /// replaced when the configuration for the trigger has changed.
    /// </summary>
    internal class ChatMessageCriteria
    {
        #region Fields

        /// <summary>
        /// This indicates if all rules must match or if only one needs to match.
        /// </summary>
        private readonly bool _areAllRulesRequired;

        /// <summary>
        /// The rules that are used to filter the chat messages.
        /// </summary>
        private readonly List<ChatMessageCriteriaRule> _rules;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Creates a new instance of the <see cref="ChatMessageCriteria"/> class.
        /// </summary>
        /// <param name="automationTriggerId">The identifier of the automation trigger to which this criteria belongs.</param>
        /// <param name="criteria">The bag representing the criteria.</param>
        public ChatMessageCriteria( int automationTriggerId, ChatMessageCriteriaBag criteria )
        {
            _areAllRulesRequired = criteria?.AreAllRulesRequired ?? false;

            _rules = criteria?.Rules
                .Where( r => r.CriteriaValue.IsNotNullOrWhiteSpace() )
                .Select( r => new ChatMessageCriteriaRule( automationTriggerId, r ) )
                .ToList();
        }

        /// <summary>
        /// Determines if the Chat-to-Rock message event matches the criteria defined in this instance.
        /// </summary>
        /// <param name="messageEvent">The object that represents a message event received from the external chat system.</param>
        /// <returns><c>true</c> if the event matches the criteria; otherwise <c>false</c>.</returns>
        public bool IsMatch( ChatToRockMessageEvent messageEvent )
        {
            try
            {
                // No rules means there is a match.
                if ( _rules == null || _rules.Count == 0 )
                {
                    return true;
                }

                if ( _areAllRulesRequired )
                {
                    return _rules.All( r => r.IsMatch( messageEvent ) );
                }
                else
                {
                    return _rules.Any( r => r.IsMatch( messageEvent ) );
                }
            }
            catch ( Exception ex )
            {
                var logger = RockLogger.LoggerFactory.CreateLogger<ChatMessageCriteria>();

                logger.LogError( ex, "Error processing chat message criteria: {error}", ex.Message );

                return false;
            }
        }

        #endregion Methods
    }
}
