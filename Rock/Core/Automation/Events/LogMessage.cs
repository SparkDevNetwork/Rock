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

using Microsoft.Extensions.Logging;

using Rock.Data;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;

namespace Rock.Core.Automation.Events
{
    /// <summary>
    /// Logs a message to the Rock event log.
    /// </summary>
    [DisplayName( "Log Message" )]

    [Rock.SystemGuid.EntityTypeGuid( "7b295a55-f2ba-4e70-820b-04e1f8d7bcc6" )]
    internal partial class LogMessage : AutomationEventComponent
    {
        #region Keys

        private static class ConfigurationKey
        {
            /// <summary>
            /// The category to log the message to.
            /// </summary>
            public const string Category = "category";

            /// <summary>
            /// The severity level to use when logging the message.
            /// </summary>
            public const string Level = "level";

            /// <summary>
            /// The message to log. This can use Lava to customize the message.
            /// </summary>
            public const string Message = "message";
        }

        #endregion Keys

        #region Properties

        /// <inheritdoc/>
        public override string IconCssClass => "ti ti-file-import";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string GetEventName( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            return "Log Message";
        }

        /// <inheritdoc/>
        public override string GetEventDescription( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var category = privateConfiguration.GetValueOrNull( ConfigurationKey.Category );
            var level = privateConfiguration.GetValueOrDefault( ConfigurationKey.Level, "" ).ConvertToEnum<LogLevel>( LogLevel.Information );

            if ( category.IsNotNullOrWhiteSpace() )
            {
                return $"Logs a message with a level of '{level}' to the category '{category}'";
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override AutomationEventExecutor CreateExecutor( int automationEventId, Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var category = privateConfiguration.GetValueOrNull( ConfigurationKey.Category );
            var level = privateConfiguration.GetValueOrDefault( ConfigurationKey.Level, "" ).ConvertToEnum<LogLevel>( LogLevel.Information );
            var message = privateConfiguration.GetValueOrNull( ConfigurationKey.Message );

            if ( category.IsNotNullOrWhiteSpace() && message.IsNotNullOrWhiteSpace() )
            {
                return new LogMessageExecutor( category, level, message );
            }

            return null;
        }

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/Automation/Events/logMessage.obs" ),
                Options = new Dictionary<string, string>
                {
                },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfiguration( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var publicConfiguration = new Dictionary<string, string>( privateConfiguration );

            if ( !publicConfiguration.ContainsKey( ConfigurationKey.Category ) )
            {
                publicConfiguration[ConfigurationKey.Category] = typeof( LogMessage ).FullName;
            }

            if ( !publicConfiguration.ContainsKey( ConfigurationKey.Level ) )
            {
                publicConfiguration[ConfigurationKey.Level] = ( ( int ) LogLevel.Information ).ToString();
            }

            return publicConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfiguration( Dictionary<string, string> publicConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var privateConfiguration = new Dictionary<string, string>();

            if ( publicConfiguration.TryGetValue( ConfigurationKey.Category, out var category ) )
            {
                privateConfiguration[ConfigurationKey.Category] = category;
            }

            if ( publicConfiguration.TryGetValue( ConfigurationKey.Level, out var level ) )
            {
                privateConfiguration[ConfigurationKey.Level] = level;
            }

            if ( publicConfiguration.TryGetValue( ConfigurationKey.Message, out var message ) )
            {
                privateConfiguration[ConfigurationKey.Message] = message;
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
