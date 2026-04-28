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
using System.ComponentModel;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Administration.NotificationList;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Displays unread notifications for the current person and allows them
    /// to be marked as read.
    /// </summary>
    [DisplayName( "Notification List" )]
    [Category( "Core" )]
    [Description( "Displays Notifications." )]
    [Rock.SystemGuid.EntityTypeGuid( "A0ABC8AA-ECD7-4D2B-91D6-19FD66E26DE7" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "584046A5-7374-4AB4-B5CF-032825B1E847" )]
    [Rock.SystemGuid.BlockTypeGuid( "9C0FD17D-677D-4A37-A61F-54C370954E83" )]
    [InitialBlockHeight( 0 )]
    public class NotificationList : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<NotificationListBag, NotificationListOptionsBag>();

            var currentPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

            if ( !currentPersonAliasId.HasValue )
            {
                return box;
            }

            // Get all unread notifications for the current person, newest first.
            var notificationItems = new NotificationRecipientService( RockContext ).Queryable()
                .Where( n => n.PersonAliasId == currentPersonAliasId.Value && n.Read == false )
                .OrderByDescending( n => n.Notification.SentDateTime )
                .Select( n => new NotificationItemBag
                {
                    Guid = n.Guid,
                    Title = n.Notification.Title,
                    Message = n.Notification.Message,
                    IconCssClass = n.Notification.IconCssClass,
                    Classification = n.Notification.Classification
                } )
                .ToList();

            box.Bag = new NotificationListBag
            {
                Notifications = notificationItems
            };

            return box;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Marks the specified notification recipient as read.
        /// </summary>
        /// <param name="notificationRecipientGuid">The unique identifier of the notification recipient to mark as read.</param>
        /// <returns>A block action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult MarkAsRead( Guid notificationRecipientGuid )
        {
            var notificationRecipient = new NotificationRecipientService( RockContext )
                .Get( notificationRecipientGuid );

            if ( notificationRecipient == null )
            {
                return ActionNotFound();
            }

            notificationRecipient.Read = true;
            notificationRecipient.ReadDateTime = RockDateTime.Now;

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}
