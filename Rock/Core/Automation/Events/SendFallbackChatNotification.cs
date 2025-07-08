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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.Core.Automation.Events
{
    /// <summary>
    /// Sends a System Communication as a fallback to Chat members who don't receive device notifications.
    /// </summary>
    [DisplayName( "Send Fallback Chat Notification" )]

    [Rock.SystemGuid.EntityTypeGuid( "C50ED947-CD1B-4956-9D30-65592A721D5C" )]
    internal partial class SendFallbackChatNotification : AutomationEventComponent
    {
        #region Keys

        private static class ConfigurationKey
        {
            /// <summary>
            /// The unique identifier of the system communication that will be sent.
            /// </summary>
            public const string SystemCommunication = "systemCommunication";

            /// <summary>
            /// The number of minutes the system will suppress notifications if the recipient has already received a
            /// recent notification and has not yet read the chat message that triggered it.
            /// </summary>
            public const string NotificationSuppressionMinutes = "notificationSuppressionMinutes";
        }

        private static class OptionKey
        {
            public const string SystemCommunicationItems = "systemCommunicationItems";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string IconCssClass => "ti ti-message-circle-up";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string GetEventName( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            return "Send Fallback Chat Notification";
        }

        /// <inheritdoc/>
        public override string GetEventDescription( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            return "Sends a System Communication as a fallback to Chat members who don't receive device notifications.";
        }

        /// <inheritdoc/>
        public override AutomationEventExecutor CreateExecutor( int automationEventId, Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var systemCommunicationGuid = privateConfiguration.GetValueOrNull( ConfigurationKey.SystemCommunication )?.AsGuidOrNull();
            var notificationSuppressionMinutes = privateConfiguration.GetValueOrNull( ConfigurationKey.NotificationSuppressionMinutes )?.AsIntegerOrNull();

            if ( systemCommunicationGuid.HasValue )
            {
                return new SendFallbackChatNotificationExecutor( systemCommunicationGuid.Value, notificationSuppressionMinutes ?? 60 );
            }

            return null;
        }

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/Automation/Events/sendFallbackChatNotification.obs" ),
                Options = new Dictionary<string, string>
                {
                    [OptionKey.SystemCommunicationItems] = GetSystemCommunicationListItemBags( rockContext )
                },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> ExecuteComponentRequest( Dictionary<string, string> request, SecurityGrant securityGrant, RockContext rockContext, RockRequestContext requestContext )
        {
            return null;
        }

        /// <summary>
        /// Gets the serialized list of <see cref="ListItemBag"/>s representing the <see cref="SystemCommunication"/>s
        /// that may be sent for the alternative chat notification.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A serialized list of <see cref="ListItemBag"/>s.</returns>
        private string GetSystemCommunicationListItemBags( RockContext rockContext )
        {
            var listItemBags = new SystemCommunicationService( rockContext )
                .Queryable()
                .AsNoTracking()
                //.Where( sc => sc.CategoryId == TBD... ) // TODO (Jason): Should probably limit to a newly-added category.
                .Select( sc => new ListItemBag
                {
                    Value = sc.Guid.ToString(),
                    Text = sc.Title
                } )
                .OrderBy( b => b.Text )
                .ToList();

            return listItemBags.ToCamelCaseJson( false, false );
        }

        #endregion
    }
}
