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
using Rock.Web.Cache;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Content Channel List" )]
    [Category( "CMS" )]
    [Description( "Lists content channels." )]

    [LinkedPage( "Detail Page" )]
    public partial class ContentChannelList : RockBlock, ISecondaryBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
            gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gContentChannels.DataKeyNames = new string[] { "Id" };
            gContentChannels.Actions.ShowAdd = canAddEditDelete;
            gContentChannels.IsDeleteEnabled = canAddEditDelete;
            gContentChannels.Actions.AddClick += gContentChannels_Add;
            gContentChannels.GridRebind += gContentChannels_GridRebind;

            var securityField = gContentChannels.Columns.OfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentChannel ) ).Id;
            }

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
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Gfs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Type":
                    {
                        int? typeId = e.Value.AsIntegerOrNull();
                        if ( typeId.HasValue )
                        {
                            var contentType = new ContentChannelTypeService( new RockContext() ).Get( typeId.Value );
                            if ( contentType != null )
                            {
                                e.Value = contentType.Name;
                            }
                        }
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SaveUserPreference( "Type", ddlType.SelectedValue );
            BindGrid();
        }

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
            NavigateToLinkedPage( "DetailPage", "contentChannelId", e.RowKeyId );
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

            ContentChannel contentChannel = contentChannelService.Get( e.RowKeyId );

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

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        private void BindFilter()
        {
            int? typeId = gfFilter.GetUserPreference( "Type" ).AsIntegerOrNull();
            ddlType.Items.Clear();
            ddlType.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var contentType in new ContentChannelTypeService( new RockContext() ).Queryable().OrderBy( c => c.Name ) )
            {
                var li = new ListItem( contentType.Name, contentType.Id.ToString() );
                li.Selected = typeId.HasValue && contentType.Id == typeId.Value;
                ddlType.Items.Add( li );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            ContentChannelService contentChannelService = new ContentChannelService( new RockContext() );
            SortProperty sortProperty = gContentChannels.SortProperty;
            var qry = contentChannelService.Queryable( "ContentChannelType,Items" );

            int? typeId = gfFilter.GetUserPreference( "Type" ).AsIntegerOrNull();
            if ( typeId.HasValue )
            {
                qry = qry.Where( c => c.ContentChannelTypeId == typeId.Value );
            }

            gContentChannels.ObjectList = new Dictionary<string, object>();

            var channels = new List<ContentChannel>();
            foreach ( var channel in qry.ToList() )
            {
                if ( channel.IsAuthorized(Rock.Security.Authorization.VIEW, CurrentPerson))
                {
                    channels.Add( channel );
                    gContentChannels.ObjectList.Add( channel.Id.ToString(), channel );
                }
            }

            var now = RockDateTime.Now;
            var items = channels.Select( c => new
            {
                c.Id,
                c.Name,
                ContentChannelType = c.ContentChannelType.Name,
                c.EnableRss,
                c.ChannelUrl,
                ItemLastCreated = c.Items.Max( i => i.CreatedDateTime ),
                TotalItems = c.Items.Count(),
                ActiveItems = c.Items
                    .Where( i =>
                        ( i.StartDateTime.CompareTo( now ) < 0 ) &&
                        ( !i.ExpireDateTime.HasValue || i.ExpireDateTime.Value.CompareTo( now ) > 0 ) &&
                        ( i.ApprovedByPersonAliasId.HasValue || !c.RequiresApproval )
                    ).Count()
            } ).AsQueryable();

            gContentChannels.EntityTypeId = EntityTypeCache.Read<ContentChannel>().Id;

            if ( sortProperty != null )
            {
                gContentChannels.DataSource = items.Sort( sortProperty ).ToList();
            }
            else
            {
                gContentChannels.DataSource = items.OrderBy( p => p.Name ).ToList();
            }

            gContentChannels.DataBind();
        }


        #endregion
    }
}