﻿// <copyright>
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
    [DisplayName("Content Channel Item List")]
    [Category("CMS")]
    [Description("Lists content channel items.")]

    [ContextAware]
    [LinkedPage("Detail Page", order: 0)]
    [BooleanField("Filter Items For Current User", "Filters the items by those created by the current logged in user.", false, order: 1)]
    [BooleanField("Show Filters", "Allows you to show/hide the grids filters.", true, order: 2)]
    [BooleanField("Show Event Occurrences Column", "Determines if the column that lists event occurrences should be shown if any of the items has an event occurrence.", true, order: 3)]
    [BooleanField( "Show Priority Column", "Determines if the column that displays priority should be shown for content channels that have Priority enabled.", true, order: 4 )]
    [BooleanField( "Show Security Column", "Determines if the security column should be shown.", true, order: 5 )]
    [BooleanField( "Show Expire Column", "Determines if the expire column should be shown.", true, order: 6 )]
    [ContentChannelField("Content Channel", "If set the block will ignore content channel query parameters", false)]
    public partial class ContentChannelItemList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Fields

        private int? _channelId = null;
        private bool _manuallyOrdered = false;

        private int _typeId = 0;
        private Person _person = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // set person context
            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is Person )
                {
                    _person = contextEntity as Person;
                }
            }

            // set person if grid should be filtered by the current person
            if ( GetAttributeValue( "FilterItemsForCurrentUser" ).AsBoolean() )
            {
                _person = CurrentPerson;
            }

            gfFilter.Visible = GetAttributeValue( "ShowFilters" ).AsBoolean();
            
            if (string.IsNullOrWhiteSpace(GetAttributeValue("ContentChannel")))
            {
                _channelId = PageParameter("contentChannelId").AsIntegerOrNull();
            }
            else
            {
                _channelId = new ContentChannelService(new RockContext()).Get(GetAttributeValue("ContentChannel").AsGuid()).Id;
            }

            if ( _channelId != null )
            {
                upnlContent.Visible = true;

                string cssIcon = "fa fa-bullhorn";
                var contentChannel = new ContentChannelService( new RockContext() ).Get( _channelId.Value );
                if ( contentChannel != null )
                {
                    string startHeading = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ? "Start" : "Active";

                    _manuallyOrdered = contentChannel.ItemsManuallyOrdered;

                    var startDateTimeColumn = gItems.ColumnsWithDataField( "StartDateTime" ).OfType<DateTimeField>().FirstOrDefault();
                    var expireDateTimeColumn = gItems.ColumnsWithDataField( "ExpireDateTime" ).OfType<DateTimeField>().FirstOrDefault();
                    var startDateColumn = gItems.ColumnsWithDataField( "StartDateTime" ).OfType<DateField>().FirstOrDefault();
                    var expireDateColumn = gItems.ColumnsWithDataField( "ExpireDateTime" ).OfType<DateField>().FirstOrDefault();
                    var priorityColumn = gItems.ColumnsWithDataField( "Priority" ).FirstOrDefault();
                    
                    //// NOTE: The EventOccurrences Column's visibility is set in GridBind()

                    startDateTimeColumn.HeaderText = startHeading;
                    startDateColumn.HeaderText = startHeading;

                    ddlStatus.Visible = contentChannel.RequiresApproval && !contentChannel.ContentChannelType.DisableStatus;

                    if ( contentChannel.ContentChannelType.IncludeTime )
                    {
                        startDateTimeColumn.Visible = contentChannel.ContentChannelType.DateRangeType != ContentChannelDateType.NoDates;
                        expireDateTimeColumn.Visible = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange && GetAttributeValue( "ShowExpireColumn" ).AsBoolean();
                        startDateColumn.Visible = false;
                        expireDateColumn.Visible = false;
                    }
                    else
                    {
                        startDateTimeColumn.Visible = false;
                        expireDateTimeColumn.Visible = false;
                        startDateColumn.Visible = contentChannel.ContentChannelType.DateRangeType != ContentChannelDateType.NoDates;
                        expireDateColumn.Visible = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange && GetAttributeValue( "ShowExpireColumn" ).AsBoolean();
                    }

                    priorityColumn.Visible = !contentChannel.ContentChannelType.DisablePriority && GetAttributeValue( "ShowPriorityColumn" ).AsBoolean();

                    lContentChannel.Text = contentChannel.Name;
                    _typeId = contentChannel.ContentChannelTypeId;

                    if ( !string.IsNullOrWhiteSpace( contentChannel.IconCssClass ) )
                    {
                        cssIcon = contentChannel.IconCssClass;
                    }
                }

                lIcon.Text = string.Format( "<i class='{0}'></i>", cssIcon );

                // Block Security and special attributes (RockPage takes care of View)
                bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

                gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
                gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;

                gItems.DataKeyNames = new string[] { "Id" };
                gItems.AllowSorting = !_manuallyOrdered;
                gItems.Actions.ShowAdd = canAddEditDelete;
                gItems.IsDeleteEnabled = canAddEditDelete;
                gItems.Actions.AddClick += gItems_Add;
                gItems.GridRebind += gItems_GridRebind;
                gItems.GridReorder += GItems_GridReorder;
                gItems.EntityTypeId = EntityTypeCache.Get<ContentChannelItem>().Id;

                AddAttributeColumns();

                if ( contentChannel != null && contentChannel.RequiresApproval && !contentChannel.ContentChannelType.DisableStatus )
                {
                    var statusField = new BoundField();
                    gItems.Columns.Add( statusField );
                    statusField.DataField = "Status";
                    statusField.HeaderText = "Status";
                    statusField.SortExpression = "Status";
                    statusField.HtmlEncode = false;
                }

                var securityField = new SecurityField();
                gItems.Columns.Add( securityField );
                securityField.TitleField = "Title";
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannelItem ) ).Id;

                var securityColumn = gItems.Columns.OfType<SecurityField>().FirstOrDefault();
                if ( securityColumn != null )
                {
                    securityColumn.Visible = GetAttributeValue( "ShowSecurityColumn" ).AsBoolean();
                }

                var deleteField = new DeleteField();
                gItems.Columns.Add( deleteField );
                deleteField.Click += gItems_Delete;

                // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
                this.BlockUpdated += Block_BlockUpdated;
                this.AddConfigurationUpdateTrigger( upnlContent );
            }
            else
            {
                upnlContent.Visible = false;
            }
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

        #region Events

        /// <summary>
        /// Gfs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Status":
                    var status = e.Value.ConvertToEnumOrNull<ContentChannelItemStatus>();
                    if ( status.HasValue )
                    {
                        {
                            e.Value = status.ConvertToString();
                        }
                    }

                    break;

                case "Title":
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SaveUserPreference( "Date Range", drpDateRange.DelimitedValues );
            gfFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            gfFilter.SaveUserPreference( "Title", tbTitle.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gItems_Add( object sender, EventArgs e )
        {
            Dictionary<string, string> pageParams = new Dictionary<string, string>();
            pageParams.Add( "contentItemId", "0" );
            pageParams.Add( "contentChannelId", _channelId.ToString() );

            NavigateToLinkedPage( "DetailPage", pageParams );
        }

        /// <summary>
        /// Handles the Edit event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentItemId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var contentItemService = new ContentChannelItemService( rockContext );
            var contentItemAssociationService = new ContentChannelItemAssociationService( rockContext );
            var contentItemSlugService = new ContentChannelItemSlugService( rockContext );

            ContentChannelItem contentItem = contentItemService.Get( e.RowKeyId );

            if ( contentItem != null )
            {
                string errorMessage;
                if ( !contentItemService.CanDelete( contentItem, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    contentItemAssociationService.DeleteRange( contentItem.ChildItems );
                    contentItemAssociationService.DeleteRange( contentItem.ParentItems );
                    contentItemService.Delete( contentItem );
                    rockContext.SaveChanges();
                } );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gItems_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the GItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void GItems_GridReorder( object sender, GridReorderEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                bool isFiltered = false;
                var items = GetItems( rockContext, out isFiltered );

                if ( !isFiltered )
                {
                    var service = new ContentChannelItemService( rockContext );
                    service.Reorder( items, e.OldIndex, e.NewIndex );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            OnInit(e);
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

        protected void AddAttributeColumns()
        {
            // Remove attribute columns
            foreach ( var column in gItems.Columns.OfType<AttributeField>().ToList() )
            {
                gItems.Columns.Remove( column );
            }

            // Add attribute columns
            int entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannelItem ) ).Id;
            foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn && ( (
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( _typeId.ToString() )
                    ) || (
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( _channelId.ToString() )
                    ) ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                string dataFieldExpression = attribute.Key;
                bool columnExists = gItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField();
                    boundField.DataField = dataFieldExpression;
                    boundField.AttributeId = attribute.Id;
                    boundField.HeaderText = attribute.Name;

                    var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                    if ( attributeCache != null )
                    {
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                    }

                    gItems.Columns.Add( boundField );
                }
            }
        }

        private void BindFilter()
        {
            drpDateRange.DelimitedValues = gfFilter.GetUserPreference( "Date Range" );
            ddlStatus.BindToEnum<ContentChannelItemStatus>( true );
            int? statusID = gfFilter.GetUserPreference( "Status" ).AsIntegerOrNull();
            if ( statusID.HasValue )
            {
                ddlStatus.SetValue( statusID.Value.ToString() );
            }

            tbTitle.Text = gfFilter.GetUserPreference( "Title" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            bool isFiltered = false;
            var items = GetItems( new RockContext(), out isFiltered );
            var reorderField = gItems.ColumnsOfType<ReorderField>().FirstOrDefault();

            if ( _manuallyOrdered && !isFiltered )
            {
                if ( reorderField != null )
                {
                    reorderField.Visible = true;
                    gItems.AllowSorting = false;
                }
            }
            else
            {
                if ( reorderField != null )
                {
                    reorderField.Visible = false;
                    gItems.AllowSorting = true;
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
            }

            gItems.ObjectList = new Dictionary<string, object>();
            items.ForEach( i => gItems.ObjectList.Add( i.Id.ToString(), i ) );

            var gridList = items.Select( i => new
            {
                i.Id,
                i.Guid,
                i.Title,
                i.StartDateTime,
                i.ExpireDateTime,
                i.Priority,
                Status = DisplayStatus( i.Status ),
                Occurrences = i.EventItemOccurrences.Any()
            } ).ToList();

            // only show the Event Occurrences item if any of the displayed content channel items have any occurrences (and the block setting is enabled)
            var eventOccurrencesColumn = gItems.ColumnsWithDataField( "Occurrences" ).FirstOrDefault();
            var showEventOccurrencesColumnBlockSetting = GetAttributeValue( "ShowEventOccurrencesColumn" ).AsBoolean();
            eventOccurrencesColumn.Visible = showEventOccurrencesColumnBlockSetting && gridList.Any( a => a.Occurrences == true );

            gItems.DataSource = gridList;
            gItems.DataBind();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private List<ContentChannelItem> GetItems( RockContext rockContext, out bool isFiltered )
        {
            isFiltered = false;

            var items = new List<ContentChannelItem>();

            if ( _channelId.HasValue )
            {
                ContentChannelItemService contentItemService = new ContentChannelItemService( rockContext );
                var contentItems = contentItemService.Queryable()
                    .Where( c => c.ContentChannelId == _channelId.Value );

                var drp = new DateRangePicker();
                drp.DelimitedValues = gfFilter.GetUserPreference( "Date Range" );
                if ( drp.LowerValue.HasValue )
                {
                    isFiltered = true;
                    contentItems = contentItems.Where( i =>
                        ( i.ExpireDateTime.HasValue && i.ExpireDateTime.Value >= drp.LowerValue.Value ) ||
                        ( !i.ExpireDateTime.HasValue && i.StartDateTime >= drp.LowerValue.Value ) );
                }

                if ( drp.UpperValue.HasValue )
                {
                    isFiltered = true;
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    contentItems = contentItems.Where( i => i.StartDateTime <= upperDate );
                }

                var status = gfFilter.GetUserPreference( "Status" ).ConvertToEnumOrNull<ContentChannelItemStatus>();
                if ( status.HasValue )
                {
                    isFiltered = true;
                    contentItems = contentItems.Where( i => i.Status == status );
                }

                string title = gfFilter.GetUserPreference( "Title" );
                if ( !string.IsNullOrWhiteSpace( title ) )
                {
                    isFiltered = true;
                    contentItems = contentItems.Where( i => i.Title.Contains( title ) );
                }

                // if the block has a person context filter requests for just them
                if ( _person != null )
                {
                    isFiltered = true;
                    contentItems = contentItems.Where( i => i.CreatedByPersonAlias != null && i.CreatedByPersonAlias.PersonId == _person.Id );
                }

                // TODO: Checking security of every item will take longer and longer as more items are added.  
                // Eventually we should implement server-side paging so that we only need to check security for
                // the items on the current page
                foreach ( var item in contentItems.ToList() )
                {
                    if ( item.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                    {
                        items.Add( item );
                    }
                    else
                    {
                        isFiltered = true;
                    }
                }
            }

            if ( _manuallyOrdered && !isFiltered )
            {
                return items.OrderBy( i => i.Order ).ToList();
            }
            else
            {
                return items;
            }
        }

        /// <summary>
        /// Displays the status.
        /// </summary>
        /// <param name="contentItemStatus">The content item status.</param>
        /// <returns></returns>
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
