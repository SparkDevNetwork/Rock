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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Calendar Item Occurrence Content Channel Item List")]
    [Category("Event")]
    [Description("Lists the content channel items associated to a particular calendar item occurrence.")]

    [LinkedPage("Detail Page")]
    public partial class CalendarContentChannelItemList : RockBlock, ISecondaryBlock
    {

        #region Properties

        private int? OccurrenceId { get; set; }
        private List<ContentChannel> ContentChannels { get; set; }
        private List<int> ExpandedPanels { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            OccurrenceId = ViewState["OccurrenceId"] as int?;

            string json = ViewState["ContentChannels"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ContentChannels = new List<ContentChannel>();
            }
            else
            {
                ContentChannels = JsonConvert.DeserializeObject<List<ContentChannel>>( json );
            }

            ExpandedPanels = ViewState["ExpandedPanels"] as List<int>;

            CreateGrids( new RockContext() );
            BindGrids();
        }

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
                var rockContext = new RockContext();

                OccurrenceId = PageParameter( "EventItemOccurrenceId" ).AsIntegerOrNull();
                ContentChannels = new List<ContentChannel>();
                ExpandedPanels = new List<int>();

                if ( OccurrenceId.HasValue && OccurrenceId.Value != 0 )
                {
                    var channels = new Dictionary<int, ContentChannel>();

                    var eventItemOccurrence = new EventItemOccurrenceService( rockContext ).Get( OccurrenceId.Value );
                    if ( eventItemOccurrence != null && eventItemOccurrence.EventItem != null && eventItemOccurrence.EventItem.EventCalendarItems != null )
                    {
                        eventItemOccurrence.EventItem.EventCalendarItems
                            .SelectMany( i => i.EventCalendar.ContentChannels )
                            .Select( c => c.ContentChannel )
                            .ToList()
                            .ForEach( c => channels.AddOrIgnore( c.Id, c ) );

                        ExpandedPanels = eventItemOccurrence.ContentChannelItems
                            .Where( i => i.ContentChannelItem != null )
                            .Select( i => i.ContentChannelItem.ContentChannelId )
                            .Distinct()
                            .ToList();
                    }

                    ContentChannels = channels.Select( c => c.Value ).ToList();
                }

                CreateGrids( rockContext );
                BindGrids();
            }

            base.OnLoad( e );
        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["OccurrenceId"] = OccurrenceId;
            ViewState["ContentChannels"] = JsonConvert.SerializeObject( ContentChannels, Formatting.None, jsonSetting );
            ViewState["ExpandedPanels"] = ExpandedPanels;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Add event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gItems_Add( object sender, EventArgs e )
        {
            var grid = ( (Control)sender ).DataKeysContainer;
            if ( grid != null )
            {
                int contentChannelId = grid.ID.Substring( 7 ).AsInteger();
                NavigateToDetailPage( 0, contentChannelId );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannelItemService contentItemService = new ContentChannelItemService( rockContext );

            ContentChannelItem contentItem = contentItemService.Get( e.RowKeyId );

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

            BindGrids();
        }

        /// <summary>
        /// Handles the GridRebind event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gItems_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrids();
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

        private void CreateGrids( RockContext rockContext )
        {
            if ( ContentChannels.Any() )
            {
                this.Visible = true;

                // TODO: security
                bool canEdit = true;

                phContentChannelGrids.Controls.Clear();

                foreach ( var contentChannel in ContentChannels )
                {
                    var pwItems = new PanelWidget();
                    phContentChannelGrids.Controls.Add( pwItems );
                    pwItems.ID = string.Format( "pwItems_{0}", contentChannel.Id );
                    pwItems.Title = contentChannel.Name;
                    pwItems.Expanded = ExpandedPanels.Contains( contentChannel.Id );

                    var divItems = new HtmlGenericControl( "div" );
                    pwItems.Controls.Add( divItems );
                    divItems.ID = string.Format( "divItems_{0}", contentChannel.Id );
                    divItems.AddCssClass( "grid" );
                    divItems.AddCssClass( "grid-panel" );

                    Grid gItems = new Grid();
                    divItems.Controls.Add( gItems );
                    gItems.ID = string.Format( "gItems_{0}", contentChannel.Id );
                    gItems.DataKeyNames = new string[] { "Id" };
                    gItems.EmptyDataText = "No Items Found";
                    gItems.RowItemText = "Item";
                    gItems.AllowSorting = true;
                    gItems.Actions.ShowAdd = canEdit;
                    gItems.IsDeleteEnabled = canEdit;
                    gItems.Actions.AddClick += gItems_Add;
                    gItems.RowSelected += gItems_Edit;
                    gItems.GridRebind += gItems_GridRebind;

                    gItems.Columns.Add( new RockBoundField
                    {
                        DataField = "Title",
                        HeaderText = "Title",
                        SortExpression = "Title"
                    } );

                    gItems.Columns.Add( new DateTimeField
                    {
                        DataField = "StartDateTime",
                        HeaderText = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ? "Start" : "Active",
                        SortExpression = "StartDateTime"
                    } );

                    if ( contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange )
                    {
                        gItems.Columns.Add( new DateTimeField
                        {
                            DataField = "ExpireDateTime",
                            HeaderText = "Expire",
                            SortExpression = "ExpireDateTime"
                        } );
                    }

                    if ( !contentChannel.ContentChannelType.DisablePriority )
                    {
                        var priorityField = new RockBoundField
                        {
                            DataField = "Priority",
                            HeaderText = "Priority",
                            SortExpression = "Priority",
                            DataFormatString = "{0:N0}",
                        };
                        priorityField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                        gItems.Columns.Add( priorityField );
                    }

                    // Add attribute columns
                    int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentChannelItem ) ).Id;
                    string qualifier = contentChannel.ContentChannelTypeId.ToString();
                    foreach ( var attribute in new AttributeService( rockContext ).Queryable()
                        .Where( a =>
                            a.EntityTypeId == entityTypeId &&
                            a.IsGridColumn &&
                            a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( qualifier ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name ) )
                    {
                        string dataFieldExpression = attribute.Key;
                        bool columnExists = gItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
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

                            gItems.Columns.Add( boundField );
                        }
                    }

                    if ( contentChannel.RequiresApproval )
                    {
                        var statusField = new BoundField();
                        gItems.Columns.Add( statusField );
                        statusField.DataField = "Status";
                        statusField.HeaderText = "Status";
                        statusField.SortExpression = "Status";
                        statusField.HtmlEncode = false;
                    }

                    var deleteField = new DeleteField();
                    gItems.Columns.Add( deleteField );
                    deleteField.Click += gItems_Delete;

                }
            }
            else
            {
                this.Visible = false;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrids()
        {
            if ( ContentChannels.Any() )
            {
                var allContentItems = new EventItemOccurrenceChannelItemService( new RockContext() )
                    .Queryable()
                    .Where( c => c.EventItemOccurrenceId == OccurrenceId.Value )
                    .Select( c => c.ContentChannelItem )
                    .ToList();

                foreach ( var contentChannel in ContentChannels )
                {
                    var pwItems = phContentChannelGrids.FindControl( string.Format( "pwItems_{0}", contentChannel.Id ) ) as PanelWidget;
                    if ( pwItems != null )
                    {
                        var gItems = pwItems.FindControl( string.Format( "gItems_{0}", contentChannel.Id ) ) as Grid;
                        if ( gItems != null )
                        {
                            var contentItems = allContentItems
                                .Where( c => c.ContentChannelId == contentChannel.Id );

                            var items = new List<ContentChannelItem>();
                            foreach ( var item in contentItems.ToList() )
                            {
                                if ( item.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                                {
                                    items.Add( item );
                                }
                            }

                            SortProperty sortProperty = gItems.SortProperty;
                            if ( sortProperty != null )
                            {
                                items = items.AsQueryable().Sort( sortProperty ).ToList();
                            }
                            else
                            {
                                items = items.OrderByDescending( p => p.StartDateTime ).ToList();
                            }

                            gItems.ObjectList = new Dictionary<string, object>();
                            items.ForEach( i => gItems.ObjectList.Add( i.Id.ToString(), i ) );
                            gItems.EntityTypeId = EntityTypeCache.Read<ContentChannelItem>().Id;

                            gItems.DataSource = items.Select( i => new
                            {
                                i.Id,
                                i.Guid,
                                i.Title,
                                i.StartDateTime,
                                i.ExpireDateTime,
                                i.Priority,
                                Status = DisplayStatus( i.Status )
                            } ).ToList();
                            gItems.DataBind();
                        }
                    }
                }
            }
        }

        private string DisplayStatus (ContentChannelItemStatus contentItemStatus)
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

        private void NavigateToDetailPage( int contentItemId, int? contentChannelId = null )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
            qryParams.Add( "EventItemId", PageParameter( "EventItemId" ) );
            qryParams.Add( "EventItemOccurrenceId", PageParameter( "EventItemOccurrenceId" ) );
            qryParams.Add( "ContentItemId", contentItemId.ToString() );
            if ( contentChannelId.HasValue )
            {
                qryParams.Add( "ContentChannelId", contentChannelId.Value.ToString() );
            }
            
            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        #endregion
    }
}