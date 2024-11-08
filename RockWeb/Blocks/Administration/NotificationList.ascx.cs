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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.Entity;
using System.Web.UI.HtmlControls;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Displays notifications to the user and allows them to be marked as read.
    /// </summary>
    [DisplayName( "Notification List" )]
    [Category( "Core" )]
    [Description( "Displays Notifications." )]
    [Rock.SystemGuid.BlockTypeGuid( "9C0FD17D-677D-4A37-A61F-54C370954E83" )]
    public partial class NotificationList : Rock.Web.UI.RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the number of notifications currently being displayed.
        /// </summary>
        public int NotificationCount
        {
            get { return ViewState["NotificationCount"] as int? ?? 0; }
            set { ViewState["NotificationCount"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( CurrentPersonAliasId != null )
                {
                    var rockContext = new RockContext();
                    var notificationRecipientService = new NotificationRecipientService( rockContext );
                    var notificationItems = notificationRecipientService
                        .Queryable()
                        .AsNoTracking()
                        .Where( n => n.PersonAliasId == CurrentPersonAliasId && n.Read == false )
                        .OrderByDescending( n => n.Notification.SentDateTime )
                        .ToList();

                    ViewState["NotificationCount"] = notificationItems.Count;

                    if ( notificationItems.Count == 0 )
                    {
                        HidePanel();
                    }

                    rptNotifications.DataSource = notificationItems;
                    rptNotifications.DataBind();
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            OnLoad( e );
        }

        /// <summary>
        /// Handles the ItemCommand event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptProjects_ItemCommand( object Sender, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Close" )
            {
                var notificationRecipientGuid = e.CommandArgument.ToString().AsGuidOrNull();
                if ( notificationRecipientGuid.HasValue )
                {
                    var rockContext = new RockContext();
                    var notificationRecipientService = new NotificationRecipientService( rockContext );
                    var notificationItem = notificationRecipientService.Get( notificationRecipientGuid.Value );
                    if ( notificationItem != null )
                    {
                        notificationItem.Read = true;
                        notificationItem.ReadDateTime = RockDateTime.Now;
                    }

                    var toHide = e.Item.FindControl( "rptNotificationAlert" );
                    if ( toHide != null )
                    {
                        toHide.Visible = false;
                    }

                    rockContext.SaveChanges();

                    NotificationCount--;
                    if ( NotificationCount == 0 )
                    {
                        HidePanel();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptNotifications_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var notificationRecipient = e.Item.DataItem as NotificationRecipient;
            var div = e.Item.FindControl( "rptNotificationAlert" ) as HtmlGenericControl;
            string alertType = notificationRecipient.Notification.Classification.ToString().ToLowerInvariant();
            div.AddCssClass( "alert-" + alertType );

            var icon = e.Item.FindControl( "iIconCssClass" ) as HtmlGenericControl;
            icon.Visible = !string.IsNullOrWhiteSpace( notificationRecipient.Notification.IconCssClass );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hides the top-most content panel of this block so it is no longer visible.
        /// </summary>
        protected void HidePanel()
        {
            upnlContent.Visible = false;
        }

        #endregion
    }
}