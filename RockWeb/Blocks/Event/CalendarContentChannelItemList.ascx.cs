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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Calendar Item Occurrence Content Channel Item List" )]
    [Category( "Event" )]
    [Description( "Lists the content channel items associated to a particular calendar item occurrence." )]

    [LinkedPage( "Detail Page" )]
    [Rock.SystemGuid.BlockTypeGuid( "8418C3B8-5E87-469F-BAE9-E15C32873FBD" )]
    public partial class CalendarContentChannelItemList : RockBlock, ISecondaryBlock
    {

        #region Properties

        private bool IsAllowingPredictableIds => !PageCache.Layout.Site.DisablePredictableIds;

        private int? OccurrenceId { get; set; }
        private List<ContentChannelData> ContentChannels { get; set; }
        private List<int> ExpandedPanels { get; set; }

        #endregion Properties

        #region Keys

        private class PageParameterKey
        {
            public const string EventCalendarId = "EventCalendarId";
            public const string EventItemId = "EventItemId";
            public const string EventItemOccurrenceId = "EventItemOccurrenceId";
            public const string ContentItemId = "ContentItemId";
            public const string ContentChannelId = "ContentChannelId";
        }

        #endregion Keys

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
                ContentChannels = new List<ContentChannelData>();
            }
            else
            {
                ContentChannels = JsonConvert.DeserializeObject<List<ContentChannelData>>( json ) ?? new List<ContentChannelData>();
            }

            ExpandedPanels = ViewState["ExpandedPanels"] as List<int> ?? new List<int>();

            using ( var rockContext = new RockContext() )
            {
                CreateGrids( rockContext );
            }
            
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
                using ( var rockContext = new RockContext() )
                {
                    var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                    OccurrenceId = eventItemOccurrenceService.GetQueryableByKey(
                        PageParameter( PageParameterKey.EventItemOccurrenceId ),
                        IsAllowingPredictableIds
                    )
                    .Select( io => io.Id )
                    .FirstOrDefault();

                    ContentChannels = new List<ContentChannelData>();
                    ExpandedPanels = new List<int>();

                    if ( OccurrenceId.HasValue && OccurrenceId.Value != 0 )
                    {
                        var eventItemOccurrence = eventItemOccurrenceService.Get( OccurrenceId.Value );
                        if ( eventItemOccurrence != null )
                        {
                            var eventCalendarIds = new EventCalendarItemService( rockContext ).Queryable()
                                .Where( i => i.EventItemId == eventItemOccurrence.EventItemId )
                                .Select( i => i.EventCalendarId )
                                .Distinct()
                                .ToList();

                            var contentChannelIds = new EventCalendarContentChannelService( rockContext ).Queryable()
                                .Where( i => eventCalendarIds.Contains( i.EventCalendarId ) )
                                .Select( i => i.ContentChannelId )
                                .Distinct()
                                .ToList();

                            var uniqueChannels = new ContentChannelService( rockContext ).Queryable()
                                .AsNoTracking()
                                .Include( c => c.ContentChannelType )
                                .Where( c => contentChannelIds.Contains( c.Id ) )
                                .ToList();

                            ExpandedPanels = eventItemOccurrence.ContentChannelItems
                                .Where( i => i.ContentChannelItem != null )
                                .Select( i => i.ContentChannelItem.ContentChannelId )
                                .Distinct()
                                .ToList();

                            foreach ( var contentChannel in uniqueChannels )
                            {
                                if ( !contentChannel.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                                {
                                    continue;
                                }

                                var contentChannelType = contentChannel.ContentChannelType;
                                if ( contentChannelType == null )
                                {
                                    continue;
                                }

                                ContentChannels.Add( new ContentChannelData
                                {
                                    Id = contentChannel.Id,
                                    Name = contentChannel.Name,
                                    IconCssClass = contentChannel.IconCssClass,
                                    ContentChannelTypeId = contentChannel.ContentChannelTypeId,
                                    DateRangeType = contentChannelType.DateRangeType,
                                    IncludeTime = contentChannelType.IncludeTime,
                                    DisablePriority = contentChannelType.DisablePriority,
                                    DisableStatus = contentChannelType.DisableStatus,
                                    RequiresApproval = contentChannel.RequiresApproval
                                } );
                            }
                        }
                    }

                    CreateGrids( rockContext );
                }
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
            var grid = ( ( Control ) sender ).DataKeysContainer;
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
            using ( var rockContext = new RockContext() )
            {
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

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbtnLinkExisting_Click( object sender, EventArgs e )
        {
            LinkButton lb = ( LinkButton ) sender;
            int contentChannelId = lb.CommandArgument.AsInteger();

            //
            // Build a list of the existing Content Channel Items that are
            // not expired yet and not currently linked to this even occurrence.
            //
            ddlLinkExistingItems.Items.Clear();
            ddlLinkExistingItems.Items.Add( new ListItem() );
            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemService = new ContentChannelItemService( rockContext );
                DateTime now = RockDateTime.Now;

                var items = contentChannelItemService.Queryable()
                    .AsNoTracking()
                    .Include( i => i.ContentChannelType )
                    .Where( i => i.ContentChannelId == contentChannelId )
                    .Where( i => !i.ExpireDateTime.HasValue || i.ExpireDateTime.Value >= now )
                    .Where( i => !i.EventItemOccurrences.Any( o => o.EventItemOccurrenceId == OccurrenceId ) )
                    .OrderBy( i => i.Title );

                //
                // Add each item to the list and format the name to be the
                // title and, conditionally, the start and end date/times.
                //
                foreach ( var item in items )
                {
                    bool includeTime = item.ContentChannelType.IncludeTime;
                    string title = item.Title;
                    string startDateText = null;
                    string endDateText = null;

                    if ( item.ContentChannelType.DateRangeType == ContentChannelDateType.SingleDate )
                    {
                        startDateText = item.StartDateTime.ToShortDateString();
                    }
                    else if ( item.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange )
                    {
                        startDateText = item.StartDateTime.ToShortDateString();
                        endDateText = item.ExpireDateTime.HasValue ? item.ExpireDateTime.Value.ToShortDateString() : null;
                    }

                    if ( endDateText != null )
                    {
                        title += string.Format( " ({0} - {1})", startDateText, endDateText );
                    }
                    else if ( startDateText != null )
                    {
                        title += string.Format( " ({0})", startDateText );
                    }

                    ddlLinkExistingItems.Items.Add( new ListItem( title, item.Id.ToString() ) );
                }
            }

            mdlLinkExisting.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the control. Creates a new linkage between
        /// this event occurrence and the selected content channel item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdlLinkExisting_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceChannelItem = new EventItemOccurrenceChannelItem();

                occurrenceChannelItem.ContentChannelItemId = ddlLinkExistingItems.SelectedValue.AsInteger();
                occurrenceChannelItem.EventItemOccurrenceId = OccurrenceId.Value;
                new EventItemOccurrenceChannelItemService( rockContext ).Add( occurrenceChannelItem );

                rockContext.SaveChanges();
            }

            mdlLinkExisting.Hide();
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

                phContentChannelGrids.Controls.Clear();

                foreach ( var contentChannel in ContentChannels )
                {
                    bool canEdit = UserCanEdit
                        || ContentChannelCache.Get( contentChannel.Id )?.IsAuthorized( Authorization.EDIT, CurrentPerson ) == true;

                    string iconClass = "ti ti-speakerphone";
                    if ( !string.IsNullOrWhiteSpace( contentChannel.IconCssClass ) )
                    {
                        iconClass = contentChannel.IconCssClass;
                    }

                    var pwItems = new PanelWidget();
                    phContentChannelGrids.Controls.Add( pwItems );
                    pwItems.ID = string.Format( "pwItems_{0}", contentChannel.Id );
                    pwItems.Title = string.Format( "<i class='{0}'></i> {1}", iconClass, contentChannel.Name );
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

                    //
                    // Add the "Link Existing Item" button.
                    //
                    if ( OccurrenceId.HasValue )
                    {
                        var lbtnLinkExisting = new LinkButton();
                        lbtnLinkExisting.Attributes.Add( "title", "Link Existing Item" );
                        lbtnLinkExisting.CommandArgument = contentChannel.Id.ToString();
                        lbtnLinkExisting.CssClass = "btn btn-grid-action btn-default btn-sm";
                        lbtnLinkExisting.Click += lbtnLinkExisting_Click;
                        lbtnLinkExisting.Text = "<i class='ti ti-link'></i>";

                        gItems.Actions.AddCustomActionControl( lbtnLinkExisting );
                    }

                    gItems.Columns.Add( new RockBoundField
                    {
                        DataField = "Title",
                        HeaderText = "Title",
                        SortExpression = "Title"
                    } );

                    if ( contentChannel.DateRangeType != ContentChannelDateType.NoDates )
                    {
                        RockBoundField startDateTimeField;
                        RockBoundField expireDateTimeField;
                        if ( contentChannel.IncludeTime )
                        {
                            startDateTimeField = new DateTimeField();
                            expireDateTimeField = new DateTimeField();
                        }
                        else
                        {
                            startDateTimeField = new DateField();
                            expireDateTimeField = new DateField();
                        }

                        startDateTimeField.DataField = "StartDateTime";
                        startDateTimeField.HeaderText = contentChannel.DateRangeType == ContentChannelDateType.DateRange ? "Start" : "Active";
                        startDateTimeField.SortExpression = "StartDateTime";
                        gItems.Columns.Add( startDateTimeField );

                        expireDateTimeField.DataField = "ExpireDateTime";
                        expireDateTimeField.HeaderText = "Expire";
                        expireDateTimeField.SortExpression = "ExpireDateTime";
                        if ( contentChannel.DateRangeType == ContentChannelDateType.DateRange )
                        {
                            gItems.Columns.Add( expireDateTimeField );
                        }
                    }

                    if ( !contentChannel.DisablePriority )
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
                    int entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannelItem ) ).Id;
                    string qualifier = contentChannel.ContentChannelTypeId.ToString();
                    foreach ( var attributeCache in new AttributeService( rockContext ).GetByEntityTypeQualifier( entityTypeId, "ContentChannelTypeId", qualifier, false )
                        .Where( a => a.IsGridColumn )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name ).ToAttributeCacheList() )
                    {
                        string dataFieldExpression = attributeCache.Key;
                        bool columnExists = gItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                        if ( !columnExists )
                        {
                            AttributeField boundField = new AttributeField();
                            boundField.DataField = dataFieldExpression;
                            boundField.AttributeId = attributeCache.Id;
                            boundField.HeaderText = attributeCache.Name;

                            if ( attributeCache != null )
                            {
                                boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                            }

                            gItems.Columns.Add( boundField );
                        }
                    }

                    if ( contentChannel.RequiresApproval && !contentChannel.DisableStatus )
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
                List<ContentChannelItem> allContentItems;
                using ( var rockContext = new RockContext() )
                {
                    allContentItems = new EventItemOccurrenceChannelItemService( rockContext ).Queryable()
                        .AsNoTracking()
                        .Where( c => c.EventItemOccurrenceId == OccurrenceId.Value )
                        .Select( c => c.ContentChannelItem )
                        .ToList();
                }

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

                            var items = contentItems.ToList();

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
                            gItems.EntityTypeId = EntityTypeCache.Get<ContentChannelItem>().Id;

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

        private string DisplayStatus( ContentChannelItemStatus contentItemStatus )
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
            using ( var rockContext = new RockContext() )
            {
                var eventCalendarService = new EventCalendarService( rockContext );
                var eventItemService = new EventItemService( rockContext );
                var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                var contentChannelService = new ContentChannelService( rockContext );
                var contentChannelItemService = new ContentChannelItemService( rockContext );

                var eventCalendarIdKey = eventCalendarService.GetNoTracking(
                    PageParameter( PageParameterKey.EventCalendarId ),
                    IsAllowingPredictableIds
                )?.IdKey;

                if ( !string.IsNullOrEmpty( eventCalendarIdKey ) )
                {
                    qryParams[PageParameterKey.EventCalendarId] = eventCalendarIdKey;
                }

                var eventItemIdKey = eventItemService.GetNoTracking(
                    PageParameter( PageParameterKey.EventItemId ),
                    IsAllowingPredictableIds
                )?.IdKey;

                if ( !string.IsNullOrEmpty( eventItemIdKey ) )
                {
                    qryParams[PageParameterKey.EventItemId] = eventItemIdKey;
                }

                var eventItemOccurrenceIdKey = eventItemOccurrenceService.GetNoTracking(
                    PageParameter( PageParameterKey.EventItemOccurrenceId ),
                    IsAllowingPredictableIds
                )?.IdKey;

                if ( !string.IsNullOrEmpty( eventItemOccurrenceIdKey ) )
                {
                    qryParams[PageParameterKey.EventItemOccurrenceId] = eventItemOccurrenceIdKey;
                }

                var contentItemIdKey = contentChannelItemService.GetNoTracking( contentItemId )?.IdKey;
                if ( !string.IsNullOrEmpty( contentItemIdKey ) )
                {
                    qryParams[PageParameterKey.ContentItemId] = contentItemIdKey;
                }

                if ( contentChannelId.HasValue )
                {
                    var contentChannelIdKey = contentChannelService.GetNoTracking( contentChannelId.Value )?.IdKey;
                    if ( !string.IsNullOrEmpty( contentChannelIdKey ) )
                    {
                        qryParams[PageParameterKey.ContentChannelId] = contentChannelIdKey;
                    }
                }
            }

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Represents the minimum needed content channel data.
        /// </summary>
        private sealed class ContentChannelData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string IconCssClass { get; set; }
            public int ContentChannelTypeId { get; set; }
            public ContentChannelDateType DateRangeType { get; set; }
            public bool IncludeTime { get; set; }
            public bool DisablePriority { get; set; }
            public bool DisableStatus { get; set; }
            public bool RequiresApproval { get; set; }
        }

        #endregion
    }
}