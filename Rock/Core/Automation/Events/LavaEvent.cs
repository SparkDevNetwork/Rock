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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Data;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.Core.Automation.Events
{
    /// <summary>
    /// Executes a Lava template.
    /// </summary>
    [DisplayName( "Lava Event" )]

    [Rock.SystemGuid.EntityTypeGuid( "1588d51c-2dd6-4a32-acef-7f27acfee38a" )]
    internal partial class LavaEvent : AutomationEventComponent
    {
        #region Keys

        private static class ConfigurationKey
        {
            /// <summary>
            /// The Lava template to merge.
            /// </summary>
            public const string Template = "template";

            /// <summary>
            /// The Lava commands that are enabled for this template as a comma separated list.
            /// </summary>
            public const string EnabledLavaCommands = "enabledLavaCommands";
        }

        #endregion Keys

        #region Properties

        /// <inheritdoc/>
        public override string IconCssClass => "ti ti-mountain";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string GetEventName( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            return "Lava Event";
        }

        /// <inheritdoc/>
        public override string GetEventDescription( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            return "Executes a Lava template with the provided merge fields.";
        }

        /// <inheritdoc/>
        public override AutomationEventExecutor CreateExecutor( int automationEventId, Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var template = privateConfiguration.GetValueOrNull( ConfigurationKey.Template );
            var enabledLavaCommands = privateConfiguration.GetValueOrNull( ConfigurationKey.EnabledLavaCommands );

            if ( template.IsNotNullOrWhiteSpace() )
            {
                return new LavaEventExecutor( automationEventId, template, enabledLavaCommands );
            }

            return null;
        }

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/Automation/Events/lavaEvent.obs" ),
                Options = new Dictionary<string, string>
                {
                },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfiguration( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var publicConfiguration = new Dictionary<string, string>( privateConfiguration );

            if ( publicConfiguration.TryGetValue( ConfigurationKey.EnabledLavaCommands, out var enabledLavaCommands ) )
            {
                publicConfiguration[ConfigurationKey.EnabledLavaCommands] = enabledLavaCommands
                    .Split( ',' )
                    .Select( c => new ListItemBag
                    {
                        Value = c,
                        Text = c.SplitCase()
                    } )
                    .ToList()
                    .ToCamelCaseJson( false, false );
            }

            return publicConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfiguration( Dictionary<string, string> publicConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var privateConfiguration = new Dictionary<string, string>();

            if ( publicConfiguration.TryGetValue( ConfigurationKey.Template, out var template ) )
            {
                privateConfiguration[ConfigurationKey.Template] = template;
            }

            if ( publicConfiguration.TryGetValue( ConfigurationKey.EnabledLavaCommands, out var enabledLavaCommands ) )
            {
                privateConfiguration[ConfigurationKey.EnabledLavaCommands] = enabledLavaCommands
                    .FromJsonOrNull<List<ListItemBag>>()
                    ?.Select( c => c.Value )
                    .JoinStrings( "," )
                    ?? string.Empty;
            }

            return privateConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> ExecuteComponentRequest( Dictionary<string, string> request, SecurityGrant securityGrant, RockContext rockContext, RockRequestContext requestContext )
        {
            return null;
        }

        #endregion
    }
}
