// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Marketing Campaign - Campaign List")]
    [Category("CMS")]
    [Description("Lists marketing campaigns.")]
    [LinkedPage("Detail Page")]
    public partial class ContentChannelList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gContentChannels.DataKeyNames = new string[] { "id" };

            gContentChannels.Actions.AddClick += gContentChannels_Add;
            gContentChannels.GridRebind += gContentChannels_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gContentChannels.Actions.ShowAdd = canAddEditDelete;
            gContentChannels.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }
        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentChannels_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentChannelId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentChannels_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentChannelId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentChannels_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );

            ContentChannel contentChannel = contentChannelService.Get( (int)e.RowKeyValue );

            if ( contentChannel != null )
            {
                string errorMessage;
                if ( !contentChannelService.CanDelete( contentChannel, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                contentChannelService.Delete( contentChannel );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gContentChannels_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            ContentChannelService contentChannelService = new ContentChannelService( new RockContext() );
            SortProperty sortProperty = gContentChannels.SortProperty;
            var qry = contentChannelService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Title,
                    EventGroupName = a.EventGroup.Name,
                    a.ContactFullName
                } );

            if ( sortProperty != null )
            {
                gContentChannels.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gContentChannels.DataSource = qry.OrderBy( p => p.Title ).ToList();
            }

            gContentChannels.DataBind();
        }

        #endregion
    }
}