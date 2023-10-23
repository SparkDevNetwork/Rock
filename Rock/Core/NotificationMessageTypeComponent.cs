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

using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Core;
using Rock.Web.Cache;

namespace Rock.Core
{
    /// <summary>
    /// Base class for notification message type components.
    /// </summary>
    internal abstract class NotificationMessageTypeComponent : Rock.Extension.Component
    {
        #region Properties

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults { get; } = new Dictionary<string, string>
        {
            { "Active", "True" },
            { "Order", "0" }
        };

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive => true;

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order => 0;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the notification message metadata. This provides additional
        /// details about how the message should be displayed.
        /// </summary>
        /// <param name="message">The notification message that will be displayed.</param>
        /// <returns>A new instance of <see cref="NotificationMessageActionBag"/> that describes the action.</returns>
        public virtual NotificationMessageMetadataBag GetMetadata( NotificationMessage message )
        {
            return null;
        }

        /// <summary>
        /// Gets the action that should be performed when interacting with the
        /// notification message. This can and probably will result in different
        /// actions depending on the site.
        /// </summary>
        /// <param name="message">The notification message that will be interacted with.</param>
        /// <param name="site">The site that the action will be performed on.</param>
        /// <param name="context">The context that describes the current network request, may be <c>null</c>.</param>
        /// <returns>A new instance of <see cref="NotificationMessageActionBag"/> that describes the action.</returns>
        public abstract NotificationMessageActionBag GetActionForNotificationMessage( NotificationMessage message, SiteCache site, RockRequestContext context );

        /// <summary>
        /// Deletes any obsolete notification message types. Such as any types
        /// that correspond to other entities that no longer exist in Rock.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds to use for SQL commands.</param>
        /// <returns>The number of message types that were deleted.</returns>
        public abstract int DeleteObsoleteNotificationMessageTypes( int commandTimeout = 30 );

        /// <summary>
        /// Deletes any obsolete notification messages. Such as any messages
        /// that correspond to other entities that no longer exist in Rock.
        /// </summary>
        /// <remarks>
        /// The cleanup job will automatically delete expired messages.
        /// </remarks>
        /// <param name="commandTimeout">The timeout in seconds to use for SQL commands.</param>
        /// <returns>The number of messages that were deleted.</returns>
        public abstract int DeleteObsoleteNotificationMessages( int commandTimeout = 30 );

        /// <summary>
        /// Performs any additional cleanup. This is called at the end of the
        /// notificaiton message cleanup process. Components can use this to
        /// delete any related data tied to messages that no longer exist or
        /// for any other cleanup they need to perform.
        /// </summary>
        /// <param name="commandTimeout">The timeout in seconds to use for SQL commands.</param>
        public virtual void PerformCleanup( int commandTimeout = 30 )
        {
        }

        #endregion
    }
}
