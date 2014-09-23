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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block to display the content channels/items that user is authorized to view.
    /// </summary>
    [DisplayName( "My Content Items" )]
    [Category( "CMS" )]
    [Description( "Block to display the content channels/items that user is authorized to view." )]

    [LinkedPage( "Detail Page", "Page used to view a content item." )]
    public partial class MyContentItems : Rock.Web.UI.RockBlock
    {
        #region Fields

        #endregion

        #region Properties

        protected bool? StatusFilter { get; set; }
        protected bool? RoleFilter { get; set; }
        protected int? SelectedChannelId { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            StatusFilter = ViewState["StatusFilter"] as bool?;
            RoleFilter = ViewState["RoleFilter"] as bool?;
            SelectedChannelId = ViewState["SelectedChannelId"] as int?;

            GetData();
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptChannels.ItemCommand += rptChannels_ItemCommand;

            gContentItems.DataKeyNames = new string[] { "id" };
            gContentItems.Actions.ShowAdd = true;
            gContentItems.IsDeleteEnabled = false;
            gContentItems.Actions.AddClick += gContentItems_Add;
            gContentItems.GridRebind += gContentItems_GridRebind;
             
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                SelectedChannelId = PageParameter( "contentChannelId" ).AsIntegerOrNull();
                GetData();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["StatusFilter"] = StatusFilter;
            ViewState["RoleFilter"] = RoleFilter;
            ViewState["SelectedChannelId"] = SelectedChannelId;
            return base.SaveViewState();
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
            GetData();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tgl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tgl_CheckedChanged( object sender, EventArgs e )
        {
            StatusFilter = tglDisplay.Checked;
            RoleFilter = tglRole.Checked;
            GetData();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptChannels control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptChannels_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? channelId = e.CommandArgument.ToString().AsIntegerOrNull();
            if (channelId.HasValue)
            {
                SelectedChannelId = channelId.Value;
            }

            GetData();
        }

        void gContentItems_Add( object sender, EventArgs e )
        {
            if ( SelectedChannelId.HasValue )
            {
                NavigateToLinkedPage( "DetailPage", "contentItemId", 0, "contentChannelId", SelectedChannelId.Value );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gContentItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentItems_Edit( object sender, RowEventArgs e )
        {
            var contentItem = new ContentItemService( new RockContext() ).Get( e.RowKeyId );
            if ( contentItem != null )
            {
                NavigateToLinkedPage( "DetailPage", "contentItemId", contentItem.Id );
            }
        }
        

        /// <summary>
        /// Handles the GridRebind event of the gContentItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gContentItems_GridRebind( object sender, EventArgs e )
        {
            GetData();
        }

        #endregion

        #region Methods

        private void GetData()
        {
            var rockContext = new RockContext();
            var itemService = new ContentItemService(rockContext);

            int personId = CurrentPerson != null ? CurrentPerson.Id : 0;

            // Get all of the content channels
            var allChannels = new ContentChannelService( rockContext ).Queryable()
                .OrderBy( w => w.Name )
                .ToList();

            // Create variable for storing authorized channels and the count of active items
            var channelCounts = new Dictionary<int, int>();
            foreach ( var channel in allChannels )
            {
                if ( channel.IsAuthorized( Authorization.VIEW, CurrentPerson))
                {
                    channelCounts.Add( channel.Id, 0);
                }
            }

            // Get the item counts for each channel
            var itemCountQry = itemService.Queryable()
                .Where( i => channelCounts.Keys.Contains( i.ContentChannelId ));
            if ( RoleFilter.HasValue && RoleFilter.Value )
            {
                itemCountQry = itemCountQry.Where( w => w.CreatedByPersonAlias.PersonId == personId );
            }
            itemCountQry
                .GroupBy( i => i.ContentChannelId )
                .Select( i => new {
                    Id = i.Key,
                    Count = i.Count()
                })
                .ToList()
                .ForEach( i => channelCounts[i.Id] = i.Count );

            // Create a query to return channel, the count of items, and the selected class
            var qry = allChannels
                .Where( c => channelCounts.Keys.Contains( c.Id ) )
                .Select( c => new
                {
                    Channel = c,
                    Count = channelCounts[c.Id],
                    Class = ( SelectedChannelId.HasValue && SelectedChannelId.Value == c.Id ) ? "active" : ""
                } );

            // If displaying active only, update query to exclude those content channels without any items
            if ( StatusFilter.HasValue && StatusFilter.Value )
            {
                qry = qry.Where( c => c.Count > 0 );
            }

            rptChannels.DataSource = qry.ToList();
            rptChannels.DataBind();

            ContentChannel selectedChannel = null;
            if ( SelectedChannelId.HasValue )
            {
                selectedChannel = allChannels
                    .Where( w => 
                        w.Id == SelectedChannelId.Value &&
                        channelCounts.Keys.Contains( SelectedChannelId.Value ) )
                    .FirstOrDefault();
            }

            if ( selectedChannel != null )
            {

                var statusColumn = gContentItems.Columns.OfType<BoundField>().FirstOrDefault( c => c.HeaderText == "Status");
                if ( statusColumn != null )
                {
                    gContentItems.Columns.Remove( statusColumn );
                }

                if ( selectedChannel.RequiresApproval )
                {
                    var statusField = new BoundField();
                    gContentItems.Columns.Add( statusField );
                    statusField.DataField = "Status";
                    statusField.HeaderText = "Status";
                    statusField.SortExpression = "Status";
                    statusField.HtmlEncode = false;
                }

                AddAttributeColumns( selectedChannel );

                var itemQry = itemService.Queryable()
                    .Where( i => i.ContentChannelId == selectedChannel.Id );
                if ( RoleFilter.HasValue && RoleFilter.Value )
                {
                    itemQry = itemQry.Where( w => w.CreatedByPersonAlias.PersonId == personId );
                }

                var items = new List<ContentItem>();
                foreach ( var item in itemQry.ToList() )
                {
                    if ( item.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                    {
                        items.Add( item );
                    }
                }

                SortProperty sortProperty = gContentItems.SortProperty;
                if ( sortProperty != null )
                {
                    items = items.AsQueryable().Sort( sortProperty ).ToList();
                }
                else
                {
                    items = items.OrderByDescending( p => p.StartDateTime ).ToList();
                }

                gContentItems.ObjectList = new Dictionary<string, object>();
                items.ForEach( i => gContentItems.ObjectList.Add( i.Id.ToString(), i ) );

                gContentItems.DataSource = itemQry.ToList().Select( i => new
                {
                    i.Id,
                    i.Guid,
                    i.Title,
                    i.StartDateTime,
                    i.ExpireDateTime,
                    i.Priority,
                    Status = DisplayStatus( i.Status )
                } ).ToList();
                gContentItems.DataBind();
                gContentItems.Visible = true;

                lContentItem.Text = selectedChannel.Name + " Items";
            }
            else
            {
                gContentItems.Visible = false;
            }

        }

        protected void AddAttributeColumns( ContentChannel channel)
        {
            // Remove attribute columns
            foreach ( var column in gContentItems.Columns.OfType<AttributeField>().ToList() )
            {
                gContentItems.Columns.Remove( column );
            }

            if ( channel != null )
            {
                // Add attribute columns
                int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentItem ) ).Id;
                string qualifier = channel.ContentTypeId.ToString();
                foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "ContentTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifier ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gContentItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.HeaderText = attribute.Name;
                        boundField.SortExpression = string.Empty;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gContentItems.Columns.Add( boundField );
                    }
                }
            }
        }

        protected string DisplayStatus (ContentItemStatus contentItemStatus)
        {
            string labelType = "default";
            if ( contentItemStatus == ContentItemStatus.Approved )
            {
                labelType = "success";
            }
            else if ( contentItemStatus == ContentItemStatus.Denied )
            {
                labelType = "danger";
            }

            return string.Format( "<span class='label label-{0}'>{1}</span>", labelType, contentItemStatus.ConvertToString() );
        }
        
        #endregion

    }

}