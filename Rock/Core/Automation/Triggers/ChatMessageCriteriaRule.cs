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
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Rock.Communication.Chat.Sync;
using Rock.Core.Automation.Events;
using Rock.Enums.Core.Automation.Triggers;
using Rock.Logging;
using Rock.ViewModels.Core.Automation.Triggers;
using Rock.Web.Cache;

namespace Rock.Core.Automation.Triggers
{
    /// <summary>
    /// A single criteria rule for the Chat Message trigger criteria. This handles the comparison of a Chat-to-Rock
    /// message event that represents a message event received from the external chat system.
    /// </summary>
    internal class ChatMessageCriteriaRule
    {
        #region Fields

        /// <summary>
        /// The logger instance that will handle logging diagnostic messages.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The type of rule to use to determine if a chat message should trigger the events.
        /// </summary>
        private readonly ChatMessageCriteriaRuleType _ruleType;

        /// <summary>
        /// The criteria value to use to determine if a chat message should trigger the events.
        /// </summary>
        private readonly string _ruleCriteriaValue;

        /// <summary>
        /// The list of identifiers for channel types the message must be sent within, in order to trigger the events.
        /// </summary>
        private readonly Lazy<HashSet<int>> _channelTypeIds;

        /// <summary>
        /// The list of identifiers for channels the message must be sent within, in order to trigger the events.
        /// </summary>
        private readonly Lazy<HashSet<int>> _channelIds;

        /// <summary>
        /// A compiled Regex pattern the message must match, in order to trigger the events.
        /// </summary>
        private readonly Regex _messageRegex;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessageCriteriaRule"/> class.
        /// </summary>
        /// <param name="automationTriggerId">The identifier of the automation trigger to which this rule belongs.</param>
        /// <param name="rule">The definition of the rule that will be processed.</param>
        public ChatMessageCriteriaRule( int automationTriggerId, ChatMessageCriteriaRuleBag rule )
        {
            _logger = RockLogger.LoggerFactory.CreateLogger<LavaEventExecutor>();
            _ruleType = rule.RuleType;
            _ruleCriteriaValue = rule.CriteriaValue;

            switch ( _ruleType )
            {
                case ChatMessageCriteriaRuleType.ChannelType:
                    {
                        _channelTypeIds = new Lazy<HashSet<int>>( () =>
                        {
                            var channelTypeIds = new HashSet<int>();
                            _ruleCriteriaValue
                                ?.SplitDelimitedValues()
                                ?.AsGuidList()
                                ?.ForEach( g =>
                                {
                                    var groupTypeId = GroupTypeCache.Get( g )?.Id;
                                    if ( groupTypeId.HasValue )
                                    {
                                        channelTypeIds.Add( groupTypeId.Value );
                                    }
                                } );

                            return channelTypeIds;
                        } );

                        break;
                    }

                case ChatMessageCriteriaRuleType.Channel:
                    {
                        _channelIds = new Lazy<HashSet<int>>( () =>
                        {
                            var channelIds = new HashSet<int>();
                            _ruleCriteriaValue
                                ?.SplitDelimitedValues()
                                ?.AsGuidList()
                                ?.ForEach( g =>
                                {
                                    var groupId = GroupCache.Get( g )?.Id;
                                    if ( groupId.HasValue )
                                    {
                                        channelIds.Add( groupId.Value );
                                    }
                                } );

                            return channelIds;
                        } );

                        break;
                    }

                case ChatMessageCriteriaRuleType.MessagePattern:
                    {
                        if ( string.IsNullOrEmpty( rule?.CriteriaValue ) )
                        {
                            _logger.LogError(
                                "Chat Message Automation Trigger with ID {AutomationTriggerId} contains an empty Regex pattern. No messages will be matched.",
                                automationTriggerId
                            );
                        }
                        else
                        {
                            try
                            {
                                _messageRegex = new Regex( rule.CriteriaValue, RegexOptions.Compiled );
                            }
                            catch ( Exception ex )
                            {
                                _logger.LogError(
                                    ex,
                                    "Chat Message Automation Trigger with ID {AutomationTriggerId} contains an invalid Regex pattern: {Pattern}",
                                    automationTriggerId,
                                    rule.CriteriaValue
                                );
                            }
                        }

                        if ( _messageRegex == null )
                        {
                            // Fallback: match nothing.
                            _messageRegex = new Regex( @"a^", RegexOptions.Compiled );
                        }

                        break;
                    }
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Determines if the Chat-to-Rock message event matches this rule.
        /// </summary>
        /// <param name="messageEvent">The object that represents a message event received from the external chat system.</param>
        /// <returns><c>true</c> if the event matches this rule; <c>false</c> if it did not match or the rule was not valid.</returns>
        public bool IsMatch( ChatToRockMessageEvent messageEvent )
        {
            if ( messageEvent == null )
            {
                return false;
            }

            switch ( _ruleType )
            {
                case ChatMessageCriteriaRuleType.ChannelType:
                    {
                        return messageEvent.ChannelType != null
                            && _channelTypeIds.Value.Contains( messageEvent.ChannelType.Id );
                    }

                case ChatMessageCriteriaRuleType.Channel:
                    {
                        return messageEvent.Channel != null
                            && _channelIds.Value.Contains( messageEvent.Channel.Id );
                    }

                case ChatMessageCriteriaRuleType.MessageContains:
                    {
                        var containsText = _ruleCriteriaValue?.ToUpper() ?? string.Empty;
                        var messageText = messageEvent.Message?.ToUpper() ?? string.Empty;

                        return messageText.Contains( containsText );
                    }

                case ChatMessageCriteriaRuleType.MessagePattern:
                    {
                        return messageEvent.Message != null
                            && _messageRegex?.IsMatch( messageEvent.Message ) == true;
                    }

                default:
                    return false;
            }
        }

        #endregion Methods
    }
}
