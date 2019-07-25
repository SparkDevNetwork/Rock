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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Web.UI.Controls;

namespace Rock.Web.UI
{
    /// <summary>
    /// Manages the display of block-level notifications.
    /// </summary>
    public class RockBlockNotificationManager
    {
        /// <summary>
        /// A NotificationBox control that is used to display the notifications.
        /// This block should not be a child of the DetailContainerControl.
        /// </summary>
        public NotificationBox NotificationControl { get; set; }

        /// <summary>
        /// The container control for the content of the block.
        /// The visibility of this content may be toggled when some types of notification are displayed.
        /// </summary>
        public Control DetailContainerControl { get; set; }

        /// <summary>
        /// The block that is 
        /// </summary>
        public RockBlock Block { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockBlockNotificationManager"/> class.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="notificationControl">The notification control.</param>
        /// <param name="detailContainerControl">The detail container control.</param>
        /// <exception cref="System.ArgumentNullException">
        /// block
        /// or
        /// detailContainerControl
        /// or
        /// notificationControl
        /// </exception>
        /// <exception cref="System.Exception">NotificationControl cannot be a child of DetailContainerControl.</exception>
        public RockBlockNotificationManager( RockBlock block, NotificationBox notificationControl, Control detailContainerControl )
        {
            Block = block;

            if ( Block == null )
            {
                throw new ArgumentNullException( "block" );
            }

            DetailContainerControl = detailContainerControl;

            if ( DetailContainerControl == null )
            {
                throw new ArgumentNullException( "detailContainerControl" );
            }

            NotificationControl = notificationControl;

            if ( NotificationControl == null )
            {
                throw new ArgumentNullException( "notificationControl" );
            }

            // Verify that the notification control is not a child of the detail container.
            // This would cause the notification to be hidden when the content is disallowed.
            var invalidParent = notificationControl.FindFirstParentWhere( x => x.ID == detailContainerControl.ID );

            if ( invalidParent != null )
            {
                throw new Exception( "NotificationControl cannot be a child of DetailContainerControl." );
            }

            // Set the initial state of the controls.
            this.Clear();
        }

        /// <summary>
        /// Show a notification message for the block.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="notificationType">Type of the notification.</param>
        /// <param name="hideBlockContent">if set to <c>true</c> [hide block content].</param>
        public void ShowNotification( string message, NotificationBoxType notificationType = NotificationBoxType.Info, bool hideBlockContent = false )
        {
            NotificationControl.Text = message;
            NotificationControl.NotificationBoxType = notificationType;

            NotificationControl.Visible = true;
            DetailContainerControl.Visible = !hideBlockContent;
        }

        /// <summary>
        /// Reset the notification message for the block.
        /// </summary>
        public void Clear()
        {
            NotificationControl.Visible = false;
            DetailContainerControl.Visible = true;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ShowError( string message )
        {
            this.ShowNotification( message, NotificationBoxType.Danger );
        }

        /// <summary>
        /// Shows the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="writeToLog">if set to <c>true</c> [write to log].</param>
        public void ShowException( Exception ex, bool writeToLog = true )
        {
            this.ShowNotification( ex.Message, NotificationBoxType.Danger );

            if ( writeToLog )
            {
                Block.LogException( ex );
            }
        }

        /// <summary>
        /// Shows the success.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ShowSuccess( string message )
        {
            this.ShowNotification( message, NotificationBoxType.Success );
        }

        /// <summary>
        /// Show a fatal error that prevents the block content from being displayed.
        /// </summary>
        /// <param name="message"></param>
        public void ShowFatal( string message )
        {
            this.ShowNotification( message, NotificationBoxType.Danger, true );
        }

        /// <summary>
        /// Show a fatal error indicating that the user does not have permision to access this content.
        /// </summary>
        public void ShowMessageUnauthorized()
        {
            this.ShowNotification( "Sorry, you are not authorized to view this content.", NotificationBoxType.Danger, true );
        }

        /// <summary>
        /// Show a fatal error indicating that there is no content available in this block for the current context settings.
        /// </summary>
        public void ShowMessageNoContent()
        {
            this.ShowNotification( "There is no content to show in this context.", NotificationBoxType.Info, true );
        }

        /// <summary>
        /// Show a notification that edit mode is not allowed.
        /// </summary>
        /// <param name="itemFriendlyName"></param>
        public void ShowMessageEditModeDisallowed( string itemFriendlyName )
        {
            this.ShowNotification( EditModeMessage.ReadOnlyEditActionNotAllowed( itemFriendlyName ), NotificationBoxType.Info, false );
        }
    }
}