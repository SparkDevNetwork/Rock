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
using System.ComponentModel;
using System.Linq;
using System.Text;

using Rock.Data;
using Rock.Enums.Core.Automation.Triggers;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Automation.Triggers;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Core.Automation.Triggers
{
    /// <summary>
    /// Automation triggering when a chat message is sent or updated within the external chat system.
    /// </summary>
    [DisplayName( "Chat Message" )]

    [Rock.SystemGuid.EntityTypeGuid( "D538F5D7-62A9-471C-907F-7AA7BA02ABA3" )]
    internal class ChatMessage : AutomationTriggerComponent
    {
        #region Keys

        private static class ConfigurationKey
        {
            public const string Criteria = "criteria";
        }

        private static class OptionKey
        {
            public const string GroupTypeGuids = "groupTypeGuids";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override IEnumerable<AutomationValueDefinition> GetValueTypes( Dictionary<string, string> configuration, RockContext rockContext )
        {
            return new List<AutomationValueDefinition>
            {
                new AutomationValueDefinition
                {
                    Key = "ChannelType",
                    Description = "The channel type of the channel where the message was sent.",
                    Type = typeof( GroupType )
                },
                new AutomationValueDefinition
                {
                    Key = "Channel",
                    Description = "The channel where the message was sent.",
                    Type = typeof( Group )
                },
                new AutomationValueDefinition
                {
                    Key = "Person",
                    Description = "The person who sent the message.",
                    Type = typeof( Person )
                },
                new AutomationValueDefinition
                {
                    Key = "Message",
                    Description = "The message that was sent.",
                    Type = typeof( string )
                }
            };
        }

        /// <inheritdoc/>
        public override IEnumerable<ListItemBag> GetConfigurationDetails( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var criteria = privateConfiguration.GetValueOrNull( ConfigurationKey.Criteria )?.FromJsonOrNull<ChatMessageCriteriaBag>();

            var details = new List<ListItemBag>
            {
                new ListItemBag
                {
                    Value = "Trigger Criteria",
                    Text = GetCriteriaDescription( criteria )
                }
            };

            return details;
        }

        /// <inheritdoc/>
        public override IDisposable CreateTriggerMonitor( int automationTriggerId, Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var criteria = privateConfiguration.GetValueOrNull( ConfigurationKey.Criteria )?.FromJsonOrNull<ChatMessageCriteriaBag>();

            var chatMessageCriteria = new ChatMessageCriteria( automationTriggerId, criteria );

            return new ChatMessageMonitor( automationTriggerId, chatMessageCriteria );
        }

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/Automation/Triggers/chatMessage.obs" ),
                Options = new Dictionary<string, string>
                {
                    [OptionKey.GroupTypeGuids] = GetChatEnabledGroupTypeGuids()
                }
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfiguration( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var publicConfiguration = new Dictionary<string, string>( privateConfiguration );

            var criteria = publicConfiguration.GetValueOrNull( ConfigurationKey.Criteria )?.FromJsonOrNull<ChatMessageCriteriaBag>();
            var criteriaRules = criteria?.Rules;

            if ( criteriaRules?.Any() == true )
            {
                var entityListRuleTypes = new HashSet<ChatMessageCriteriaRuleType>
                {
                    ChatMessageCriteriaRuleType.ChannelType,
                    ChatMessageCriteriaRuleType.Channel
                };

                // Iterate backwards for ease-of-removal of any invalid rules.
                for ( int i = criteriaRules.Count - 1; i >= 0; i-- )
                {
                    var rule = criteriaRules[i];
                    if ( rule == null )
                    {
                        criteriaRules.RemoveAt( i );
                        continue;
                    }

                    if ( !entityListRuleTypes.Contains( rule.RuleType ) )
                    {
                        rule.FriendlyCriteriaValue = rule.CriteriaValue;
                        continue;
                    }

                    var entityGuids = rule.CriteriaValue?.SplitDelimitedValues().AsGuidList();
                    if ( entityGuids?.Any() != true )
                    {
                        criteriaRules.RemoveAt( i );
                        continue;
                    }

                    var entityListItemBags = new List<ListItemBag>();
                    var entityNames = new List<string>();

                    foreach ( var entityGuid in entityGuids )
                    {
                        ListItemBag entityListItemBag = null;

                        if ( rule.RuleType == ChatMessageCriteriaRuleType.ChannelType )
                        {
                            entityListItemBag = GroupTypeCache.Get( entityGuid, rockContext )?.ToListItemBag();
                        }
                        else if ( rule.RuleType == ChatMessageCriteriaRuleType.Channel )
                        {
                            entityListItemBag = GroupCache.Get( entityGuid, rockContext )?.ToListItemBag();
                        }

                        if ( entityListItemBag != null )
                        {
                            entityListItemBags.Add( entityListItemBag );
                            entityNames.Add( entityListItemBag.Text );
                        }
                    }

                    // If we somehow have no valid entities represented (maybe they've been deleted?), remove the rule.
                    if ( !entityListItemBags.Any() )
                    {
                        criteriaRules.RemoveAt( i );
                        continue;
                    }

                    rule.CriteriaValue = entityListItemBags.ToCamelCaseJson( false, false );
                    rule.FriendlyCriteriaValue = entityNames.JoinStrings( ", " );
                }
            }

            publicConfiguration[ConfigurationKey.Criteria] = criteria?.ToCamelCaseJson( false, false );

            return publicConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfiguration( Dictionary<string, string> publicConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var privateConfiguration = new Dictionary<string, string>( publicConfiguration );

            var criteria = privateConfiguration.GetValueOrNull( ConfigurationKey.Criteria )?.FromJsonOrNull<ChatMessageCriteriaBag>();
            var criteriaRules = criteria?.Rules;

            if ( criteriaRules?.Any() == true )
            {
                var entityListRuleTypes = new HashSet<ChatMessageCriteriaRuleType>
                {
                    ChatMessageCriteriaRuleType.ChannelType,
                    ChatMessageCriteriaRuleType.Channel
                };

                // Iterate backwards for ease-of-removal of any invalid rules.
                for ( int i = criteriaRules.Count - 1; i >= 0; i-- )
                {
                    var rule = criteriaRules[i];
                    if ( rule == null )
                    {
                        criteriaRules.RemoveAt( i );
                        continue;
                    }

                    // No need to save the friendly value to the database; we'll re-hydrate this value on demand.
                    rule.FriendlyCriteriaValue = null;

                    if ( !entityListRuleTypes.Contains( rule.RuleType ) )
                    {
                        continue;
                    }

                    var entityGuids = rule.CriteriaValue?.FromJsonOrNull<List<ListItemBag>>()
                        ?.Select( item => item?.Value )
                        ?.AsGuidList();

                    if ( entityGuids?.Any() != true )
                    {
                        criteriaRules.RemoveAt( i );
                        continue;
                    }

                    rule.CriteriaValue = entityGuids.AsDelimited( "," );
                }
            }

            privateConfiguration[ConfigurationKey.Criteria] = criteria?.ToCamelCaseJson( false, false );

            return privateConfiguration;
        }

        /// <summary>
        /// Gets the Guids of the chat-enabled <see cref="GroupType"/>s.
        /// </summary>
        /// <returns>The Guids of the chat-enabled <see cref="GroupType"/>s.</returns>
        private string GetChatEnabledGroupTypeGuids()
        {
            var groupTypeGuids = GroupTypeCache.All()
                .Where( gt => gt.IsChatAllowed )
                .Select( gt => gt.Guid )
                .ToList();

            return groupTypeGuids.ToJson();
        }

        /// <summary>
        /// Get the detail content for the criteria options.
        /// </summary>
        /// <param name="criteria">The <see cref="ChatMessageCriteriaBag"/>.</param>
        /// <returns>The text to display for the field.</returns>
        private static string GetCriteriaDescription( ChatMessageCriteriaBag criteria )
        {
            if ( criteria == null || criteria.Rules == null || criteria.Rules.Count == 0 )
            {
                return "No criteria";
            }

            var sb = new StringBuilder();

            sb.AppendLine( $"Trigger when **{( criteria.AreAllRulesRequired ? "all" : "any" )}** rules match" );

            foreach ( var rule in criteria.Rules )
            {
                sb.AppendLine( $"* {GetRuleDescription( rule )}" );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get the detail content for the specified rule.
        /// </summary>
        /// <param name="rule">The rule to transform into a descriptive text string.</param>
        /// <returns>The text that describes the rule.</returns>
        private static string GetRuleDescription( ChatMessageCriteriaRuleBag rule )
        {
            switch ( rule.RuleType )
            {
                case ChatMessageCriteriaRuleType.ChannelType:
                    {
                        var groupTypeNames = new HashSet<string>();
                        rule.CriteriaValue?.SplitDelimitedValues()
                            .AsGuidList()
                            .ForEach( g =>
                            {
                                var groupTypeName = GroupTypeCache.Get( g )?.Name;
                                if ( groupTypeName.IsNotNullOrWhiteSpace() )
                                {
                                    groupTypeNames.Add( groupTypeName );
                                }
                            } );

                        return $"Message sent within Channel {"Type".PluralizeIf( groupTypeNames.Count > 1 )}: {groupTypeNames.JoinStrings( ", " )}";
                    }

                case ChatMessageCriteriaRuleType.Channel:
                    {
                        var groupNames = new HashSet<string>();
                        rule.CriteriaValue?.SplitDelimitedValues()
                            .AsGuidList()
                            .ForEach( g =>
                            {
                                var groupTypeName = GroupCache.Get( g )?.Name;
                                if ( groupTypeName.IsNotNullOrWhiteSpace() )
                                {
                                    groupNames.Add( groupTypeName );
                                }
                            } );

                        return $"Message sent within {"Channel".PluralizeIf( groupNames.Count > 1 )}: {groupNames.JoinStrings( ", " )}";
                    }

                case ChatMessageCriteriaRuleType.MessageContains:
                    return $"Message contains case-insensitive value: _{rule.CriteriaValue}_";

                case ChatMessageCriteriaRuleType.MessagePattern:
                    return $"Message matches Regex pattern: _{rule.CriteriaValue}_";

                default:
                    return "No filter set";
            }
        }

        #endregion Methods
    }
}
