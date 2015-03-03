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
    [DisplayName( "Content Channel Items View" )]
    [Category( "CMS" )]
    [Description( "Block to display the content channels/items that user is authorized to view." )]

    [LinkedPage( "Detail Page", "Page used to view a content item." )]
    public partial class ContentChannelItemView : Rock.Web.UI.RockBlock
    {
        #region Fields

        #endregion

        #region Properties

        protected bool? StatusFilter { get; set; }
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
            SelectedChannelId = ViewState["SelectedChannelId"] as int?;

            //GetData();
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptChannels.ItemCommand += rptChannels_ItemCommand;

            gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
            gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;

            gContentChannelItems.DataKeyNames = new string[] { "Id" };
            gContentChannelItems.Actions.AddClick += gContentChannelItems_Add;
            gContentChannelItems.GridRebind += gContentChannelItems_GridRebind;
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
                BindFilter();

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
            StatusFilter = tglStatus.Checked;
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

        /// <summary>
        /// Gfs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                case "Status":
                    {
                        var status = e.Value.ConvertToEnumOrNull<ContentChannelItemStatus>();
                        if (status.HasValue)
                        {
                            {
                                e.Value = status.ConvertToString();
                            }
                        }
                        break;
                    }
                case "Title":
                    {
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
            gfFilter.SaveUserPreference( "Date Range", drpDateRange.DelimitedValues );
            gfFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            gfFilter.SaveUserPreference( "Title", tbTitle.Text );

            GetData();
        }

        /// <summary>
        /// Handles the Add event of the gContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gContentChannelItems_Add( object sender, EventArgs e )
        {
            if ( SelectedChannelId.HasValue )
            {
                NavigateToLinkedPage( "DetailPage", "contentItemId", 0, "contentChannelId", SelectedChannelId.Value );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentChannelItems_Edit( object sender, RowEventArgs e )
        {
            var contentItem = new ContentChannelItemService( new RockContext() ).Get( e.RowKeyId );
            if ( contentItem != null )
            {
                NavigateToLinkedPage( "DetailPage", "contentItemId", contentItem.Id );
            }
        }

        /// <summary>
        /// Handles the Click event of the deleteField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void gContentChannelItems_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var contentItemService = new ContentChannelItemService( rockContext );
            var contentItem = contentItemService.Get( e.RowKeyId );
            if ( contentItem != null )
            {
                string errorMessage;
                if ( !contentItemService.CanDelete( contentItem, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                contentItemService.Delete( contentItem );
                rockContext.SaveChanges();
            }

            GetData();
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gContentChannelItems_GridRebind( object sender, EventArgs e )
        {
            GetData();
        }

        #endregion

        #region Methods

        private void BindFilter()
        {
            drpDateRange.DelimitedValues = gfFilter.GetUserPreference( "Date Range" );
            ddlStatus.BindToEnum<ContentChannelItemStatus>( true );
            int? statusID = gfFilter.GetUserPreference( "Status" ).AsIntegerOrNull();
            if (statusID.HasValue)
            {
                ddlStatus.SetValue(statusID.Value.ToString());
            }

            tbTitle.Text = gfFilter.GetUserPreference( "Title" );
        }

        private void GetData()
        {
            var rockContext = new RockContext();
            var itemService = new ContentChannelItemService(rockContext);

            int personId = CurrentPerson != null ? CurrentPerson.Id : 0;

            // Get all of the content channels
            var allChannels = new ContentChannelService( rockContext ).Queryable( "ContentChannelType" )
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

            // Get the pending item counts for each channel
            itemService.Queryable()
                .Where( i => 
                    channelCounts.Keys.Contains( i.ContentChannelId ) &&
                    i.Status == ContentChannelItemStatus.PendingApproval )
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

            var contentChannels = qry.ToList();

            rptChannels.DataSource = contentChannels;
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

            if ( selectedChannel != null && contentChannels.Count > 0 )
            {
                // show the content item panel
                divItemPanel.Visible = true;
                
                AddColumns( selectedChannel );

                var itemQry = itemService.Queryable()
                    .Where( i => i.ContentChannelId == selectedChannel.Id );

                var drp = new DateRangePicker();
                drp.DelimitedValues = gfFilter.GetUserPreference( "Date Range" );
                if ( drp.LowerValue.HasValue )
                {
                    if ( selectedChannel.ContentChannelType.DateRangeType == ContentChannelDateType.SingleDate )
                    {
                        itemQry = itemQry.Where( i => i.StartDateTime >= drp.LowerValue.Value );
                    }
                    else
                    {
                        itemQry = itemQry.Where( i => i.ExpireDateTime.HasValue && i.ExpireDateTime.Value >= drp.LowerValue.Value );
                    }
                }
                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    itemQry = itemQry.Where( i => i.StartDateTime < upperDate );
                }

                var status = gfFilter.GetUserPreference( "Status" ).ConvertToEnumOrNull<ContentChannelItemStatus>();
                if ( status.HasValue )
                {
                    itemQry = itemQry.Where( i => i.Status == status );
                }

                string title = gfFilter.GetUserPreference( "Title" );
                if (!string.IsNullOrWhiteSpace(title))
                {
                    itemQry = itemQry.Where( i => i.Title.Contains( title ) );
                }

                var items = new List<ContentChannelItem>();
                foreach ( var item in itemQry.ToList() )
                {
                    if ( item.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                    {
                        items.Add( item );
                    }
                }

                SortProperty sortProperty = gContentChannelItems.SortProperty;
                if ( sortProperty != null )
                {
                    items = items.AsQueryable().Sort( sortProperty ).ToList();
                }
                else
                {
                    items = items.OrderByDescending( p => p.StartDateTime ).ToList();
                }

                gContentChannelItems.ObjectList = new Dictionary<string, object>();
                items.ForEach( i => gContentChannelItems.ObjectList.Add( i.Id.ToString(), i ) );

                gContentChannelItems.DataSource = items.Select( i => new
                {
                    i.Id,
                    i.Guid,
                    i.Title,
                    i.StartDateTime,
                    i.ExpireDateTime,
                    i.Priority,
                    Status = DisplayStatus( i.Status )
                } ).ToList();
                gContentChannelItems.DataBind();

                lContentChannelItems.Text = selectedChannel.Name + " Items";
            }
            else
            {
                divItemPanel.Visible = false;
            }

        }

        protected void AddColumns( ContentChannel channel)
        {
            // Remove all columns
            gContentChannelItems.Columns.Clear();

            if ( channel != null )
            {
                // Add Title column
                var titleField = new BoundField();
                titleField.DataField = "Title";
                titleField.HeaderText = "Title";
                titleField.SortExpression = "Title";
                gContentChannelItems.Columns.Add( titleField );

                // Add Attribute columns
                int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentChannelItem ) ).Id;
                string channelId = channel.Id.ToString();
                string channelTypeId = channel.ContentChannelTypeId.ToString();
                foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn && ( (
                            a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( channelTypeId )
                        ) || (
                            a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( channelId )
                        ) ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gContentChannelItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
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

                        gContentChannelItems.Columns.Add( boundField );
                    }
                }

                // Add Start column
                var startField = new DateTimeField();
                startField.DataField = "StartDateTime";
                startField.HeaderText = channel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ? "Start" : "Date";
                startField.SortExpression = "StartDateTime";
                gContentChannelItems.Columns.Add( startField );

                // Expire column
                if ( channel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange )
                {
                    var expireField = new DateTimeField();
                    expireField.DataField = "ExpireDateTime";
                    expireField.HeaderText = "Expire";
                    expireField.SortExpression = "ExpireDateTime";
                    gContentChannelItems.Columns.Add( expireField );
                }

                // Priority column
                var priorityField = new BoundField();
                priorityField.DataField = "Priority";
                priorityField.HeaderText = "Priority";
                priorityField.SortExpression = "Priority";
                priorityField.DataFormatString = "{0:N0}";
                priorityField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                gContentChannelItems.Columns.Add( priorityField );

                // Status column
                if ( channel.RequiresApproval )
                {
                    var statusField = new BoundField();
                    gContentChannelItems.Columns.Add( statusField );
                    statusField.DataField = "Status";
                    statusField.HeaderText = "Status";
                    statusField.SortExpression = "Status";
                    statusField.HtmlEncode = false;
                }

                bool canEditChannel = channel.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson );
                gContentChannelItems.Actions.ShowAdd = canEditChannel;
                gContentChannelItems.IsDeleteEnabled = canEditChannel;
                if ( canEditChannel )
                {

                    var deleteField = new DeleteField();
                    gContentChannelItems.Columns.Add( deleteField );
                    deleteField.Click += gContentChannelItems_Delete;
                }
            }

        }

        protected string DisplayStatus (ContentChannelItemStatus contentItemStatus)
        {
            string labelType = "default";
            if ( contentItemStatus == ContentChannelItemStatus.Approved )
            {
                labelType = "success";
            }
            else if ( contentItemStatus == ContentChannelItemStatus.Denied )
            {
                labelType = "danger";
            }

            return string.Format( "<span class='label label-{0}'>{1}</span>", labelType, contentItemStatus.ConvertToString() );
        }
        
        #endregion

    }

}