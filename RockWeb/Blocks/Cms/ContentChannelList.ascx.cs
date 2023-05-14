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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Content Channel List" )]
    [Category( "CMS" )]
    [Description( "Lists content channels." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "991507B6-D222-45E5-BA0D-B61EA72DFB64" )]
    public partial class ContentChannelList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            public const string Type = "Type";
            public const string Categories = "Categories";
        }

        #endregion UserPreferenceKeys

        #region Page Parameter Keys

        /// <summary>
        /// The keys used in page parameters.
        /// </summary>
        private class PageParameterKey
        {
            /// <summary>
            /// The member identifier key.
            /// </summary>
            public const string ContentChannelId = "ContentChannelId";
        }

        #endregion

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
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannel ) ).Id;
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
                case UserPreferenceKey.Categories:
                    {
                        var categories = new List<string>();

                        foreach ( var idVal in e.Value.SplitDelimitedValues() )
                        {
                            int id = int.MinValue;
                            if ( int.TryParse( idVal, out id ) )
                            {
                                if ( id != 0 )
                                {
                                    var category = CategoryCache.Get( id );
                                    if ( category != null )
                                    {
                                        categories.Add( CategoryCache.Get( id ).Name );
                                    }
                                }
                            }
                        }

                        e.Value = categories.AsDelimited( ", " );
                        break;
                    }
                case UserPreferenceKey.Type:
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
            gfFilter.SetFilterPreference( UserPreferenceKey.Type, ddlType.SelectedValue );
            string categoryFilterValue = cpCategories.SelectedValuesAsInt()
                .Where( v => v != 0 )
                .Select( c => c.ToString() )
                .ToList()
                .AsDelimited( "," );

            gfFilter.SetFilterPreference( UserPreferenceKey.Categories, categoryFilterValue );
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentChannels_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ContentChannelId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentChannels_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ContentChannelId, e.RowKeyId );
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
                    mdGridWarning.Show( errorMessage, ModalAlertType.Warning );
                    return;
                }

                contentChannel.ParentContentChannels.Clear();
                contentChannel.ChildContentChannels.Clear();

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
            int? typeId = gfFilter.GetFilterPreference( "Type" ).AsIntegerOrNull();
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
            var qry = contentChannelService.Queryable()
                .Include( a => a.ContentChannelType )
                .Include( a => a.Items )
                .Where( a => a.ContentChannelType.ShowInChannelList == true );

            int? typeId = gfFilter.GetFilterPreference( UserPreferenceKey.Type ).AsIntegerOrNull();
            if ( typeId.HasValue )
            {
                qry = qry.Where( c => c.ContentChannelTypeId == typeId.Value );
            }

            var selectedCategoryIds = new List<int>();
            gfFilter.GetFilterPreference( UserPreferenceKey.Categories ).SplitDelimitedValues().ToList().ForEach( s => selectedCategoryIds.Add( int.Parse( s ) ) );
            if ( selectedCategoryIds.Any() )
            {
                qry = qry.Where( a => a.Categories.Any( c => selectedCategoryIds.Contains( c.Id ) ) );
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

            gContentChannels.EntityTypeId = EntityTypeCache.Get<ContentChannel>().Id;

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