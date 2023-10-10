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
using Rock.Lava;
using Rock.Logging;
using Rock.Model;
using Rock.Model.CMS.ContentChannelItem.Options;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Content Channel Item List")]
    [Category("CMS")]
    [Description("Lists content channel items.")]

    #region Block Attributes

    [ContextAware]

    [LinkedPage(
        "Detail Page",
        Order = 0,
        Key = AttributeKey.DetailPage,
        Category = "Pages" )]

    [BooleanField(
        "Filter Items For Current User",
        Description = "Filters the items by those created by the current logged in user.",
        DefaultBooleanValue = false,
        Order = 1,
        Key = AttributeKey.FilterItemsForCurrentUser )]
    [BooleanField(
        "Show Filters",
        Description = "Allows you to show/hide the grids filters.",
        DefaultBooleanValue = true,
        Order = 2,
        Key = AttributeKey.ShowFilters )]
    [BooleanField(
        "Show Event Occurrences Column",
        Description = "Determines if the column that lists event occurrences should be shown if any of the items has an event occurrence.",
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKey.ShowEventOccurrencesColumn )]
    [BooleanField(
        "Show Priority Column",
        Description = "Determines if the column that displays priority should be shown for content channels that have Priority enabled.",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.ShowPriorityColumn )]
    [BooleanField(
        "Show Security Column",
        Description = "Determines if the security column should be shown.",
        DefaultBooleanValue = true,
        Order = 5,
        Key = AttributeKey.ShowSecurityColumn )]
    [BooleanField(
        "Show Expire Column",
        Description = "Determines if the expire column should be shown.",
        DefaultBooleanValue = true,
        Order = 6,
        Key = AttributeKey.ShowExpireColumn )]
    [ContentChannelField(
        "Content Channel",
        Description = "If set the block will ignore content channel query parameters",
        IsRequired = false,
        Key = AttributeKey.ContentChannel )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE" )]
    public partial class ContentChannelItemList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string FilterItemsForCurrentUser = "FilterItemsForCurrentUser";
            public const string ShowFilters = "ShowFilters";
            public const string ShowEventOccurrencesColumn = "ShowEventOccurrencesColumn";
            public const string ShowPriorityColumn = "ShowPriorityColumn";
            public const string ShowSecurityColumn = "ShowSecurityColumn";
            public const string ShowExpireColumn = "ShowExpireColumn";
            public const string ContentChannel = "ContentChannel";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string ContentChannelId = "contentChannelId";
            public const string ContentChannelIdKey = "ContentChannelIdKey";
        }

        #endregion

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
            if ( GetAttributeValue( AttributeKey.FilterItemsForCurrentUser ).AsBoolean() )
            {
                _person = CurrentPerson;
            }

            gfFilter.Visible = GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean();
            
            if (string.IsNullOrWhiteSpace(GetAttributeValue( AttributeKey.ContentChannel)))
            {
                _channelId = PageParameter( PageParameterKey.ContentChannelId ).AsIntegerOrNull();
            }
            else
            {
                _channelId = new ContentChannelService( new RockContext() ).Get( GetAttributeValue( AttributeKey.ContentChannel ).AsGuid() ).Id;
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
                        expireDateTimeColumn.Visible = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange && GetAttributeValue( AttributeKey.ShowExpireColumn ).AsBoolean();
                        startDateColumn.Visible = false;
                        expireDateColumn.Visible = false;
                    }
                    else
                    {
                        startDateTimeColumn.Visible = false;
                        expireDateTimeColumn.Visible = false;
                        startDateColumn.Visible = contentChannel.ContentChannelType.DateRangeType != ContentChannelDateType.NoDates;
                        expireDateColumn.Visible = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange && GetAttributeValue( AttributeKey.ShowExpireColumn ).AsBoolean();
                    }

                    priorityColumn.Visible = !contentChannel.ContentChannelType.DisablePriority && GetAttributeValue( AttributeKey.ShowPriorityColumn ).AsBoolean();

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
                    securityColumn.Visible = GetAttributeValue( AttributeKey.ShowSecurityColumn ).AsBoolean();
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
            gfFilter.SetFilterPreference( "Date Range", drpDateRange.DelimitedValues );
            gfFilter.SetFilterPreference( "Status", ddlStatus.SelectedValue );
            gfFilter.SetFilterPreference( "Title", tbTitle.Text );

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
            pageParams.Add( "ContentItemId", "0" );
            pageParams.Add( "ContentChannelId", _channelId.ToString() );

            NavigateToLinkedPage( "DetailPage", pageParams );
        }

        /// <summary>
        /// Handles the Edit event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ContentItemId", e.RowKeyId );
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

        /// <summary>
        /// Handles the RowDataBound event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gItems_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.DataItem is ContentChannelGridItem contentChannelItem )
            {
                var contentChannel = ContentChannelCache.Get( contentChannelItem.ContentChannelId );

                if ( contentChannel?.ContentLibraryConfiguration?.IsEnabled == true )
                {
                    if ( contentChannelItem.IsUploadedToContentLibrary )
                    {
                        var lbUpdateToContentLibrary = e.Row.FindControl( "lbUpdateToContentLibrary" ) as LinkButton;
                        lbUpdateToContentLibrary.Visible = true;
                    }
                    else if ( contentChannelItem.IsDownloadedFromContentLibrary )
                    {
                        var lbDownloadFromContentLibrary = e.Row.FindControl( "lbDownloadFromContentLibrary" ) as LinkButton;
                        lbDownloadFromContentLibrary.Visible = true;
                    }
                    else if ( !contentChannelItem.ContentLibrarySourceIdentifier.HasValue )
                    {
                        var lbUploadToContentLibrary = e.Row.FindControl( "lbUploadToContentLibrary" ) as LinkButton;
                        lbUploadToContentLibrary.Visible = true;
                    }
                }
            }
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
            drpDateRange.DelimitedValues = gfFilter.GetFilterPreference( "Date Range" );
            ddlStatus.BindToEnum<ContentChannelItemStatus>( true );
            int? statusID = gfFilter.GetFilterPreference( "Status" ).AsIntegerOrNull();
            if ( statusID.HasValue )
            {
                ddlStatus.SetValue( statusID.Value.ToString() );
            }

            tbTitle.Text = gfFilter.GetFilterPreference( "Title" );
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

            var gridList = items.Select( i => new ContentChannelGridItem
            {
                Id = i.Id,
                Guid = i.Guid,
                ContentChannelId = i.ContentChannelId,
                Title = i.Title,
                StartDateTime = i.StartDateTime,
                ExpireDateTime = i.ExpireDateTime,
                Priority = i.Priority,
                Status = DisplayStatus( i.Status ),
                DateStatus = DisplayDateStatus( i.StartDateTime ),
                Occurrences = i.EventItemOccurrences.Any(),
                IsContentLibraryOwner = i.IsContentLibraryOwner ?? false,
                ContentLibrarySourceIdentifier = i.ContentLibrarySourceIdentifier,
                IsDownloadedFromContentLibrary = i.IsDownloadedFromContentLibrary,
                IsUploadedToContentLibrary = i.IsUploadedToContentLibrary,
                ContentLibraryLicenseTypeGuid = i.ContentLibraryLicenseTypeValueId.HasValue ? DefinedValueCache.Get( i.ContentLibraryLicenseTypeValueId.Value )?.Guid : null
            } ).ToList();

            // only show the Event Occurrences item if any of the displayed content channel items have any occurrences (and the block setting is enabled)
            var eventOccurrencesColumn = gItems.ColumnsWithDataField( "Occurrences" ).FirstOrDefault();
            var showEventOccurrencesColumnBlockSetting = GetAttributeValue( AttributeKey.ShowEventOccurrencesColumn ).AsBoolean();
            eventOccurrencesColumn.Visible = showEventOccurrencesColumnBlockSetting && gridList.Any( a => a.Occurrences == true );

            gItems.DataSource = gridList;
            gItems.DataBind();

            // Content Library column.
            var isContentLibraryEnabled = _channelId.HasValue && ContentChannelCache.Get( _channelId.Value )?.ContentLibraryConfiguration?.IsEnabled == true;
            rtfContentLibrary.Visible = isContentLibraryEnabled;
            bDownloadFromLibrary.Visible = isContentLibraryEnabled;
        }

        protected string DisplayDateStatus( DateTime aDate )
        {
            return ( aDate > RockDateTime.Now ) ? "<i class='fa fa-clock'></i>" : string.Empty;
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
                drp.DelimitedValues = gfFilter.GetFilterPreference( "Date Range" );
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

                var status = gfFilter.GetFilterPreference( "Status" ).ConvertToEnumOrNull<ContentChannelItemStatus>();
                if ( status.HasValue )
                {
                    isFiltered = true;
                    contentItems = contentItems.Where( i => i.Status == status );
                }

                string title = gfFilter.GetFilterPreference( "Title" );
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

        #region Helper Classes

        [LavaType]
        private class ContentChannelGridItem : LavaDataObject
        {
            public int Id { get; internal set; }
            public Guid Guid { get; internal set; }
            public string Title { get; internal set; }
            public DateTime StartDateTime { get; internal set; }
            public DateTime? ExpireDateTime { get; internal set; }
            public int Priority { get; internal set; }
            public string Status { get; internal set; }
            public string DateStatus { get; internal set; }
            public bool Occurrences { get; internal set; }
            public bool IsContentLibraryOwner { get; internal set; }
            public Guid? ContentLibrarySourceIdentifier { get; internal set; }
            public int ContentChannelId { get; internal set; }
            public bool IsDownloadedFromContentLibrary { get; internal set; }
            public bool IsUploadedToContentLibrary { get; internal set; }
            public Guid? ContentLibraryLicenseTypeGuid { get; internal set; }
        }

        #endregion

        protected void lbUpdateToContentLibrary_Command( object sender, CommandEventArgs e )
        {
            var item = e.CommandArgument.ToStringSafe().FromJsonOrNull<ContentLibraryItemData>();
            hfItemId.Value = item.Id.ToString();
            lUpdateName.Text = item.Name;
            mdUpdateContentLibrary.Show();
        }

        protected void lbUploadToContentLibrary_Command( object sender, CommandEventArgs e )
        {
            var item = e.CommandArgument.ToStringSafe().FromJsonOrNull<ContentLibraryItemData>();
            hfItemId.Value = item.Id.ToString();
            lUploadName.Text = item.Name;
            var licenseGuid = ContentChannelCache.Get( _channelId.Value )?.ContentLibraryConfiguration?.LicenseTypeValueGuid ?? Rock.SystemGuid.DefinedValue.LIBRARY_LICENSE_TYPE_OPEN.AsGuid();
            aLibraryLicense.HRef = $"https://rockrms.com/library/licenses?utm_source=rock-item-uploaded";
            aLibraryLicense.InnerHtml = $"{ DefinedValueCache.Get( licenseGuid ).Value } License";
            mdUploadContentLibrary.Show();
        }

        protected void mdUpdateContentLibrary_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                try
                {
                    var contentChannelItemId = hfItemId.Value.AsInteger();
                    var contentChannelItemService = new ContentChannelItemService( rockContext );
                    contentChannelItemService.UploadToContentLibrary(
                        new ContentLibraryItemUploadOptions
                        {
                            ContentChannelItemId = contentChannelItemId,
                            UploadedByPersonAliasId = CurrentPersonAliasId
                        } );
                }
                catch ( AddToContentLibraryException ex )
                {
                    RockLogger.Log.Error( RockLogDomains.Cms, ex, ex.Message );
                    mdGridWarning.Show( ex.Message.ConvertCrLfToHtmlBr(), ModalAlertType.Alert );
                }
            }

            BindGrid();
            mdUpdateContentLibrary.Hide();
        }

        protected void mdUploadContentLibrary_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                try
                {
                    var contentChannelItemId = hfItemId.Value.AsInteger();
                    var contentChannelItemService = new ContentChannelItemService( rockContext );
                    contentChannelItemService.UploadToContentLibrary(
                        new ContentLibraryItemUploadOptions
                        {
                            ContentChannelItemId = contentChannelItemId,
                            UploadedByPersonAliasId = CurrentPersonAliasId
                        } );
                }
                catch ( AddToContentLibraryException ex )
                {
                    RockLogger.Log.Error( RockLogDomains.Cms, ex, ex.Message );
                    mdGridWarning.Show( ex.Message.ConvertCrLfToHtmlBr(), ModalAlertType.Alert );
                }
            }

            BindGrid();
            mdUploadContentLibrary.Hide();
        }

        protected void bDownloadFromLibrary_Click( object sender, EventArgs e )
        {
            var idKey = string.Empty;

            if ( _channelId.HasValue )
            {
                var contentChannel = ContentChannelCache.Get( _channelId.Value );
                idKey = contentChannel.IdKey;
            }

            NavigateToPage(
                Rock.SystemGuid.Page.LIBRARY_VIEWER.AsGuid(),
                Rock.SystemGuid.PageRoute.LIBRARY_VIEWER.AsGuid(),
                new Dictionary<string, string>
                {
                    { PageParameterKey.ContentChannelIdKey, idKey }
                } );
        }

        protected void lbDownloadFromContentLibrary_Command( object sender, CommandEventArgs e )
        {
            var item = e.CommandArgument.ToStringSafe().FromJsonOrNull<ContentLibraryItemData>();
            hfItemId.Value = item.Id.ToString();
            nbRedownloadWarning.Text = $"The action you are about to perform will overwrite the existing content of the item \"{ item.Name }\". Any changes will be lost. Are you sure you want to proceed with the update?";
            mdRedownloadContentLibrary.Show();
        }

        protected void mdRedownloadContentLibrary_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemId = hfItemId.Value.AsInteger();
                var contentChannelItemService = new ContentChannelItemService( rockContext );
                
                var contentLibraryItemGuid = contentChannelItemService.AsNoFilter().AsNoTracking().Where( i => i.Id ==  contentChannelItemId ).Select( i => i.ContentLibrarySourceIdentifier ).FirstOrDefault();
                var result = contentChannelItemService.AddFromContentLibrary( new Rock.Model.CMS.ContentChannelItem.Options.ContentLibraryItemDownloadOptions
                {
                    ContentLibraryItemGuidToDownload = contentLibraryItemGuid.Value,
                    DownloadIntoContentChannelGuid = ContentChannelCache.Get( _channelId.Value ).Guid,
                    CurrentPersonPerformingDownload = this.CurrentPerson
                } );
            }

            BindGrid();
            mdRedownloadContentLibrary.Hide();
        }

        #region Helper Classes

        public class ContentLibraryItemData
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
}