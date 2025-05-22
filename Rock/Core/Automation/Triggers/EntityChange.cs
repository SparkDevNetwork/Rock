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
using Rock.Reporting;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Automation.Triggers;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Core.Automation.Triggers
{
    /// <summary>
    /// Automation triggering when an entity is changed that matches a
    /// specified criteria.
    /// </summary>
    [DisplayName( "Entity Change" )]

    [Rock.SystemGuid.EntityTypeGuid( "0e20ff62-7b75-4fcc-8a77-1e62850136f5" )]
    internal partial class EntityChange : AutomationTriggerComponent
    {
        #region Keys

        internal static class ConfigurationKey
        {
            /// <summary>
            /// The unique identifier of the entity type being monitored for
            /// changes.
            /// </summary>
            public const string EntityType = "entityType";

            public const string TriggeredOn = "triggeredOn";

            public const string FilterMode = "filterMode";

            public const string SimpleCriteria = "simpleCriteria";

            public const string AdvancedCriteria = "advancedCriteria";
        }

        private static class OptionKey
        {
            public const string Properties = "properties";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override IEnumerable<AutomationValueDefinition> GetValueTypes( Dictionary<string, string> configuration, RockContext rockContext )
        {
            var entityTypeGuid = configuration.GetValueOrNull( ConfigurationKey.EntityType )?.AsGuidOrNull();
            var entityType = entityTypeGuid.HasValue
                ? EntityTypeCache.Get( entityTypeGuid.Value, rockContext )
                : null;

            return new List<AutomationValueDefinition>
            {
                new AutomationValueDefinition
                {
                    Key = AutomationRequest.KnownKeys.Entity,
                    Description = "The entity that was added, modified or deleted.",
                    Type = entityType?.GetEntityType() ?? typeof( IEntity )
                },
                new AutomationValueDefinition
                {
                    Key = "Person",
                    Description = "The person that added, modified or deleted the entity.",
                    Type = typeof( Person )
                },
                new AutomationValueDefinition
                {
                    Key = "OriginalValues",
                    Description = "The original values of the entity before it was modified. This is only vlaid if State is Modified.",
                    Type = typeof( IReadOnlyDictionary<string, object> )
                },
                new AutomationValueDefinition
                {
                    Key = "ModifiedProperties",
                    Description = "The names of the properties that were modified on the entity. This is only valid if State is Modified.",
                    Type = typeof( IReadOnlyList<string> )
                },
                new AutomationValueDefinition
                {
                    Key = "State",
                    Description = "The state of the entity during the save opearation.",
                    Type = typeof( EntityContextState )
                }
            };
        }

        /// <inheritdoc/>
        public override IEnumerable<ListItemBag> GetConfigurationDetails( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var entityTypeGuid = privateConfiguration.GetValueOrNull( ConfigurationKey.EntityType )?.AsGuidOrNull();
            var entityType = entityTypeGuid.HasValue
                ? EntityTypeCache.Get( entityTypeGuid.Value, rockContext )
                : null;
            var triggeredOn = ( EntityChangeModificationType ) ( privateConfiguration.GetValueOrNull( ConfigurationKey.TriggeredOn )?.AsIntegerOrNull() ?? 0 );
            var filterMode = privateConfiguration.GetValueOrNull( ConfigurationKey.FilterMode ).AsInteger();
            var simpleCriteria = privateConfiguration.GetValueOrNull( ConfigurationKey.SimpleCriteria )?.FromJsonOrNull<EntityChangeSimpleCriteriaBag>();

            var details = new List<ListItemBag>();

            if ( entityType != null )
            {
                details.Add( new ListItemBag
                {
                    Value = "Entity Type",
                    Text = entityType.FriendlyName
                } );
            }

            details.Add( new ListItemBag
            {
                Value = "Trigger On",
                Text = GetTriggerOnDescription( triggeredOn )
            } );

            details.Add( new ListItemBag
            {
                Value = "Trigger Criteria",
                Text = GetCriteriaDescription( filterMode, simpleCriteria )
            } );

            return details;
        }

        /// <inheritdoc/>
        public override IDisposable CreateTriggerMonitor( int automationTriggerId, Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var entityTypeGuid = privateConfiguration.GetValueOrNull( ConfigurationKey.EntityType )?.AsGuidOrNull();
            var entityType = entityTypeGuid.HasValue
                ? EntityTypeCache.Get( entityTypeGuid.Value, rockContext )
                : null;
            var type = entityType?.GetEntityType();
            var triggeredOn = ( EntityChangeModificationType ) ( privateConfiguration.GetValueOrNull( ConfigurationKey.TriggeredOn )?.AsIntegerOrNull() ?? 0 );
            var filterMode = privateConfiguration.GetValueOrNull( ConfigurationKey.FilterMode ).AsInteger();
            var simpleCriteria = privateConfiguration.GetValueOrNull( ConfigurationKey.SimpleCriteria )?.FromJsonOrNull<EntityChangeSimpleCriteriaBag>();
            var advancedCriteria = privateConfiguration.GetValueOrNull( ConfigurationKey.AdvancedCriteria );

            var criteria = new EntityChangeCriteria( type, filterMode, simpleCriteria, advancedCriteria );

            return new EntityChangeMonitor( automationTriggerId, type, triggeredOn, criteria );
        }

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var entityTypeGuid = privateConfiguration.GetValueOrNull( ConfigurationKey.EntityType )?.AsGuidOrNull();

            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/Automation/Triggers/entityChange.obs" ),
                Options = new Dictionary<string, string>
                {
                    [OptionKey.Properties] = GetPropertiesForEntityType( entityTypeGuid, rockContext ),
                },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfiguration( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var publicConfiguration = new Dictionary<string, string>( privateConfiguration );
            var entityTypeGuid = privateConfiguration.GetValueOrNull( ConfigurationKey.EntityType )?.AsGuidOrNull();
            ListItemBag entityTypeBag = null;

            if ( entityTypeGuid.HasValue )
            {
                entityTypeBag = EntityTypeCache.Get( entityTypeGuid.Value, rockContext )
                    ?.ToListItemBag();
            }

            publicConfiguration[ConfigurationKey.EntityType] = entityTypeBag?.ToCamelCaseJson( false, false );

            return publicConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfiguration( Dictionary<string, string> publicConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var privateConfiguration = new Dictionary<string, string>( publicConfiguration );
            var filterMode = privateConfiguration.GetValueOrDefault( ConfigurationKey.FilterMode, "0" ).AsInteger();

            if ( privateConfiguration.TryGetValue( ConfigurationKey.EntityType, out var entityTypeJson ) )
            {
                privateConfiguration[ConfigurationKey.EntityType] = entityTypeJson.FromJsonOrNull<ListItemBag>()
                    ?.Value
                    ?.AsGuidOrNull()
                    ?.ToString() ?? string.Empty;
            }
            else
            {
                privateConfiguration.Remove( ConfigurationKey.EntityType );
            }

            // Ensure we only save the correct criteria data.
            if ( filterMode == 1 )
            {
                privateConfiguration.Remove( ConfigurationKey.SimpleCriteria );
            }
            else
            {
                privateConfiguration.Remove( ConfigurationKey.AdvancedCriteria );
            }

            return privateConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> ExecuteComponentRequest( Dictionary<string, string> request, SecurityGrant securityGrant, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( request.TryGetValue( ConfigurationKey.EntityType, out var entityTypeGuid ) )
            {
                return new Dictionary<string, string>
                {
                    [OptionKey.Properties] = GetPropertiesForEntityType( entityTypeGuid.AsGuidOrNull(), rockContext ),
                };
            }

            return null;
        }

        /// <inheritdoc/>
        private string GetPropertiesForEntityType( Guid? entityTypeGuid, RockContext rockContext )
        {
            EntityTypeCache entityType = null;

            if ( entityTypeGuid.HasValue )
            {
                entityType = EntityTypeCache.Get( entityTypeGuid.Value, rockContext );
            }

            if ( entityType == null )
            {
                return "[]";
            }

            var fields = EntityHelper.GetEntityFields( entityType.GetEntityType(), false, true );
            var propertyNames = fields.Where( f => f.FieldKind == FieldKind.Property )
                .OrderBy( f => f.Name )
                .Select( f => f.Name )
                .ToList();

            return propertyNames.ToJson();
        }

        /// <summary>
        /// Get the detail content for the specified Modification Type options.
        /// </summary>
        /// <param name="triggerOn">The selected trigger on enum flags.</param>
        /// <returns>The text to display for the field.</returns>
        private static string GetTriggerOnDescription( EntityChangeModificationType triggerOn )
        {
            var triggerOnText = new List<string>();

            if ( triggerOn.HasFlag( EntityChangeModificationType.Added ) )
            {
                triggerOnText.Add( "Added" );
            }

            if ( triggerOn.HasFlag( EntityChangeModificationType.Modified ) )
            {
                triggerOnText.Add( "Modified" );
            }

            if ( triggerOn.HasFlag( EntityChangeModificationType.Deleted ) )
            {
                triggerOnText.Add( "Deleted" );
            }

            return string.Join( ", ", triggerOnText );
        }

        /// <summary>
        /// Get the detail content for the filter mode and criteria options.
        /// </summary>
        /// <param name="filterMode">The selected filter mode.</param>
        /// <param name="simpleCriteria">The simple criteria if <paramref name="filterMode"/> is <c>0</c>.</param>
        /// <returns>The text to display for the field.</returns>
        private static string GetCriteriaDescription( int filterMode, EntityChangeSimpleCriteriaBag simpleCriteria )
        {
            if ( filterMode == 1 )
            {
                return "Custom Query";
            }

            if ( simpleCriteria == null || simpleCriteria.Rules == null || simpleCriteria.Rules.Count == 0 )
            {
                return "No criteria";
            }

            var sb = new StringBuilder();

            sb.AppendLine( $"Trigger when **{( simpleCriteria.AreAllRulesRequired ? "all" : "any" )}** rules match" );

            foreach ( var rule in simpleCriteria.Rules )
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
        private static string GetRuleDescription( EntityChangeSimpleCriteriaRuleBag rule )
        {
            switch ( rule.ChangeType )
            {
                case EntityChangeSimpleChangeType.AnyChange:
                    return $"{rule.Property} has any change";

                case EntityChangeSimpleChangeType.HasSpecificValue:
                    return $"{rule.Property} has the value '{rule.UpdatedValue.ToStringSafe()}'";

                case EntityChangeSimpleChangeType.ChangedFromValue:
                    return $"{rule.Property} changed from '{rule.OriginalValue.ToStringSafe()}'";

                case EntityChangeSimpleChangeType.ChangedToValue:
                    return $"{rule.Property} changed to '{rule.UpdatedValue.ToStringSafe()}'";

                case EntityChangeSimpleChangeType.ChangedFromValueToValue:
                    return $"{rule.Property} changed from '{rule.OriginalValue.ToStringSafe()}' to '{rule.UpdatedValue.ToStringSafe()}'";

                default:
                    return "No filter set";
            }
        }

        #endregion
    }
}
