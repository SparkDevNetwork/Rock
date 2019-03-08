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

using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;
using System.Data.Entity;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block a menu of content channels/items that user is authorized to view.
    /// </summary>
    [DisplayName( "Content Channel Navigation" )]
    [Category( "CMS" )]
    [Description( "Block to display a menu of content channels/items that user is authorized to view." )]

    [LinkedPage( "Detail Page", "Page used to view a content item.", order: 1 )]

    [ContentChannelTypesField( "Content Channel Types Include", "Select any specific content channel types to show in this block. Leave all unchecked to show all content channel types ( except for excluded content channel types )", false, key: "ContentChannelTypesInclude", order: 2 )]
    [ContentChannelTypesField( "Content Channel Types Exclude", "Select content channel types to exclude from this block. Note that this setting is only effective if 'Content Channel Types Include' has no specific content channel types selected.", false, key: "ContentChannelTypesExclude", order: 3 )]
    public partial class ContentChannelNavigation : Rock.Web.UI.RockBlock
    {
        #region Fields

        private const string STATUS_FILTER_SETTING = "ContentChannelNavigation_StatusFilter";
        private const string SELECTED_CHANNEL_SETTING = "ContentChannelNavigation_SelectedChannelId";

        #endregion

        #region Properties

        protected int? SelectedChannelId { get; set; }
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            SelectedChannelId = ViewState["SelectedChannelId"] as int?;

            if ( SelectedChannelId.HasValue )
            {
                var channel = new ContentChannelService( new RockContext() ).Get( SelectedChannelId.Value );
                if ( channel != null )
                {
                    BindAttributes( channel );
                    AddDynamicControls( channel );
                }
            }
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
            gContentChannelItems.GridReorder += GContentChannelItems_GridReorder;

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
            base.OnLoad( e );
            RockPage.AddScriptLink( "~/Scripts/jquery.visible.min.js" );

            string eventTarget = this.Page.Request.Params["__EVENTTARGET"] ?? string.Empty;

            if ( !Page.IsPostBack )
            {
                BindFilter();

                tglStatus.Checked = GetUserPreference( STATUS_FILTER_SETTING ).AsBoolean();

                SelectedChannelId = PageParameter( "contentChannelId" ).AsIntegerOrNull();

                if ( !SelectedChannelId.HasValue )
                {
                    var selectedChannelGuid = PageParameter( "contentChannelGuid" ).AsGuidOrNull();

                    if ( selectedChannelGuid.HasValue )
                    {
                        SelectedChannelId = ContentChannelCache.Get( selectedChannelGuid.Value ).Id;
                    }
                }

                if ( !SelectedChannelId.HasValue )
                {
                    SelectedChannelId = GetUserPreference( SELECTED_CHANNEL_SETTING ).AsIntegerOrNull();
                }

                GetData();
            }
            else if ( eventTarget.StartsWith( gContentChannelItems.UniqueID ) )
            {
                // if we got a PostBack from the Grid (or child controls) make sure the Grid is bound
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
            SetUserPreference( STATUS_FILTER_SETTING, tglStatus.Checked.ToString() );
            GetData();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptChannels control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptChannels_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            string selectedChannelValue = e.CommandArgument.ToString();
            SetUserPreference( SELECTED_CHANNEL_SETTING, selectedChannelValue );

            SelectedChannelId = selectedChannelValue.AsIntegerOrNull();

            GetData();

            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "ScrollToGrid",
                "scrollToGrid();",
                true );
        }

        /// <summary>
        /// Gfs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( AvailableAttributes != null && SelectedChannelId.HasValue )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => MakeKeyUniqueToChannel( SelectedChannelId.Value, a.Key ) == e.Key );
                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch
                    {
                        // intentionally ignore
                    }
                }
            }

            if ( e.Key == "Date Range" )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == "Status" )
            {
                var status = e.Value.ConvertToEnumOrNull<ContentChannelItemStatus>();
                if ( status.HasValue )
                {
                    {
                        e.Value = status.ConvertToString();
                    }
                }
            }
            else if ( e.Key == "Title" )
            {
                return;
            }
            else if ( e.Key == "Created By" )
            {
                string personName = string.Empty;

                int? personId = e.Value.AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var personService = new PersonService( new RockContext() );
                    var person = personService.Get( personId.Value );
                    if ( person != null )
                    {
                        personName = person.FullName;
                    }
                }

                e.Value = personName;
            }
            else
            {
                e.Value = string.Empty;
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
            int personId = ppCreatedBy.PersonId ?? 0;
            gfFilter.SaveUserPreference( "Created By", personId.ToString() );

            if ( SelectedChannelId.HasValue && AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            gfFilter.SaveUserPreference( MakeKeyUniqueToChannel( SelectedChannelId.Value, attribute.Key ), attribute.Name, values.ToJson() );
                        }
                        catch
                        {
                            // intentionally ignore
                        }
                    }
                }
            }

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
            var contentItemAssociationService = new ContentChannelItemAssociationService( rockContext );
            var contentItemSlugService = new ContentChannelItemSlugService( rockContext );

            var contentItem = contentItemService.Get( e.RowKeyId );
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
                    contentItemSlugService.DeleteRange( contentItem.ContentChannelItemSlugs );
                    contentItemService.Delete( contentItem );
                    rockContext.SaveChanges();
                } );

            }

            GetData();
        }

        private void GContentChannelItems_GridReorder( object sender, GridReorderEventArgs e )
        {
            if ( SelectedChannelId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var selectedChannel = new ContentChannelService( rockContext ).Get( SelectedChannelId.Value );
                    if ( selectedChannel != null )
                    {
                        bool isFiltered = false;
                        var items = GetItems( rockContext, selectedChannel, out isFiltered );

                        if ( !isFiltered )
                        {
                            var service = new ContentChannelItemService( rockContext );
                            service.Reorder( items, e.OldIndex, e.NewIndex );
                            rockContext.SaveChanges();
                        }
                    }
                }
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
            if ( statusID.HasValue )
            {
                ddlStatus.SetValue( statusID.Value.ToString() );
            }

            tbTitle.Text = gfFilter.GetUserPreference( "Title" );

            int? personId = gfFilter.GetUserPreference( "Created By" ).AsIntegerOrNull();

            if ( personId.HasValue && personId.Value != 0 )
            {
                var personService = new PersonService( new RockContext() );
                var person = personService.Get( personId.Value );
                if ( person != null )
                {
                    ppCreatedBy.SetValue( person );
                }
            }
        }

        private void GetData()
        {
            var rockContext = new RockContext();
            var itemService = new ContentChannelItemService( rockContext );

            // Get all of the content channels
            var contentChannelsQry = new ContentChannelService( rockContext ).Queryable( "ContentChannelType" );

            List<Guid> contentChannelTypeGuidsInclude = GetAttributeValue( "ContentChannelTypesInclude" ).SplitDelimitedValues().AsGuidList();
            List<Guid> contentChannelTypeGuidsExclude = GetAttributeValue( "ContentChannelTypesExclude" ).SplitDelimitedValues().AsGuidList();

            if ( contentChannelTypeGuidsInclude.Any() )
            {
                // if contentChannelTypeGuidsInclude is specified, only get contentChannelTypes that are in the contentChannelTypeGuidsInclude
                // NOTE: no need to factor in contentChannelTypeGuidsExclude since included would take precendance and the excluded ones would already not be included
                contentChannelsQry = contentChannelsQry.Where( a => contentChannelTypeGuidsInclude.Contains( a.ContentChannelType.Guid ) || a.ContentChannelType.ShowInChannelList );
            }
            else if ( contentChannelTypeGuidsExclude.Any() )
            {
                contentChannelsQry = contentChannelsQry.Where( a => !contentChannelTypeGuidsExclude.Contains( a.ContentChannelType.Guid ) && a.ContentChannelType.ShowInChannelList );
            }
            else
            {
                contentChannelsQry = contentChannelsQry.Where( a => a.ContentChannelType.ShowInChannelList );
            }

            var contentChannelsList = contentChannelsQry.OrderBy( w => w.Name ).ToList();

            // Create variable for storing authorized channels and the count of active items
            var channelCounts = new Dictionary<int, int>();
            foreach ( var channel in contentChannelsList )
            {
                if ( channel.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    channelCounts.Add( channel.Id, 0 );
                }
            }

            // Get the pending approval item counts for each channel (if the channel requires approval)
            itemService.Queryable()
                .Where( i =>
                    channelCounts.Keys.Contains( i.ContentChannelId ) &&
                    i.Status == ContentChannelItemStatus.PendingApproval && i.ContentChannel.RequiresApproval )
                .GroupBy( i => i.ContentChannelId )
                .Select( i => new
                {
                    Id = i.Key,
                    Count = i.Count()
                } )
                .ToList()
                .ForEach( i => channelCounts[i.Id] = i.Count );

            // Create a query to return channel, the count of items, and the selected class
            var qry = contentChannelsList
                .Where( c => channelCounts.Keys.Contains( c.Id ) )
                .Select( c => new
                {
                    Channel = c,
                    Count = channelCounts[c.Id],
                    Class = ( SelectedChannelId.HasValue && SelectedChannelId.Value == c.Id ) ? "active" : ""
                } );

            // If displaying active only, update query to exclude those content channels without any items
            if ( tglStatus.Checked )
            {
                qry = qry.Where( c => c.Count > 0 );
            }

            var contentChannels = qry.ToList();

            rptChannels.DataSource = contentChannels;
            rptChannels.DataBind();

            ContentChannel selectedChannel = null;
            if ( SelectedChannelId.HasValue )
            {
                selectedChannel = contentChannelsList
                    .Where( w =>
                        w.Id == SelectedChannelId.Value &&
                        channelCounts.Keys.Contains( SelectedChannelId.Value ) )
                    .FirstOrDefault();
            }

            if ( selectedChannel != null && contentChannels.Count > 0 )
            {
                // show the content item panel
                divItemPanel.Visible = true;

                BindAttributes( selectedChannel );
                AddDynamicControls( selectedChannel );

                bool isFiltered = false;
                var items = GetItems( rockContext, selectedChannel, out isFiltered );

                var reorderFieldColumn = gContentChannelItems.ColumnsOfType<ReorderField>().FirstOrDefault();

                if ( selectedChannel.ItemsManuallyOrdered && !isFiltered )
                {
                    if ( reorderFieldColumn != null )
                    {
                        reorderFieldColumn.Visible = true;
                    }

                    gContentChannelItems.AllowSorting = false;
                }
                else
                {
                    if ( reorderFieldColumn != null )
                    {
                        reorderFieldColumn.Visible = false;
                    }

                    gContentChannelItems.AllowSorting = true;

                    SortProperty sortProperty = gContentChannelItems.SortProperty;
                    if ( sortProperty != null )
                    {
                        items = items.AsQueryable().Sort( sortProperty ).ToList();
                    }
                    else
                    {
                        items = items.OrderByDescending( p => p.StartDateTime ).ToList();
                    }
                }

                // Find any possible tags for the items
                var itemTags = new Dictionary<Guid, string>();
                if ( selectedChannel.IsTaggingEnabled )
                {
                    itemTags = items.ToDictionary( i => i.Guid, v => "" );
                    var entityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid() ).Id;
                    var testedTags = new Dictionary<int, string>();

                    foreach ( var taggedItem in new TaggedItemService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( i =>
                            i.EntityTypeId == entityTypeId &&
                            itemTags.Keys.Contains( i.EntityGuid ) )
                        .OrderBy( i => i.Tag.Name ) )
                    {
                        if ( !testedTags.ContainsKey( taggedItem.TagId ) )
                        {
                            testedTags.Add( taggedItem.TagId, taggedItem.Tag.IsAuthorized( Authorization.VIEW, CurrentPerson ) ? taggedItem.Tag.Name : string.Empty );
                        }

                        if ( testedTags[taggedItem.TagId].IsNotNullOrWhiteSpace() )
                        {
                            itemTags[taggedItem.EntityGuid] += string.Format( "<span class='tag'>{0}</span>", testedTags[taggedItem.TagId] );
                        }
                    }
                }

                gContentChannelItems.ObjectList = new Dictionary<string, object>();
                items.ForEach( i => gContentChannelItems.ObjectList.Add( i.Id.ToString(), i ) );

                var gridList = items.Select( i => new
                {
                    i.Id,
                    i.Guid,
                    i.Title,
                    i.StartDateTime,
                    i.ExpireDateTime,
                    i.Priority,
                    Status = DisplayStatus( i.Status ),
                    Tags = itemTags.GetValueOrNull( i.Guid ),
                    Occurrences = i.EventItemOccurrences.Any(),
                    CreatedByPersonName = i.CreatedByPersonAlias != null ? String.Format( "<a href={0}>{1}</a>", ResolveRockUrl( string.Format( "~/Person/{0}", i.CreatedByPersonAlias.PersonId ) ), i.CreatedByPersonName ) : String.Empty
                } ).ToList();

                // only show the Event Occurrences item if any of the displayed content channel items have any occurrences (and the block setting is enabled)
                var eventOccurrencesColumn = gContentChannelItems.ColumnsWithDataField( "Occurrences" ).FirstOrDefault();
                eventOccurrencesColumn.Visible = gridList.Any( a => a.Occurrences == true );

                gContentChannelItems.DataSource = gridList;
                gContentChannelItems.DataBind();

                lContentChannelItems.Text = selectedChannel.Name + " Items";
            }
            else
            {
                divItemPanel.Visible = false;
            }
        }

        private List<ContentChannelItem> GetItems( RockContext rockContext, ContentChannel selectedChannel, out bool isFiltered )
        {
            isFiltered = false;

            var items = new List<ContentChannelItem>();

            var contentChannelItemService = new ContentChannelItemService( rockContext );

            var itemQry = contentChannelItemService.Queryable()
                .Where( i => i.ContentChannelId == selectedChannel.Id );

            var drp = new DateRangePicker();
            drp.DelimitedValues = gfFilter.GetUserPreference( "Date Range" );
            if ( drp.LowerValue.HasValue )
            {
                isFiltered = true;
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
                isFiltered = true;
                DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                itemQry = itemQry.Where( i => i.StartDateTime <= upperDate );
            }

            var status = gfFilter.GetUserPreference( "Status" ).ConvertToEnumOrNull<ContentChannelItemStatus>();
            if ( status.HasValue )
            {
                isFiltered = true;
                itemQry = itemQry.Where( i => i.Status == status );
            }

            string title = gfFilter.GetUserPreference( "Title" );
            if ( !string.IsNullOrWhiteSpace( title ) )
            {
                isFiltered = true;
                itemQry = itemQry.Where( i => i.Title.Contains( title ) );
            }

            int? personId = gfFilter.GetUserPreference( "Created By" ).AsIntegerOrNull();
            if ( personId.HasValue && personId.Value != 0 )
            {
                isFiltered = true;
                itemQry = itemQry.Where( i => i.CreatedByPersonAlias.PersonId == personId );
            }

            // Filter query by any configured attribute filters
            if ( AvailableAttributes != null && AvailableAttributes.Any() )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    itemQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( itemQry, filterControl, attribute, contentChannelItemService, Rock.Reporting.FilterMode.SimpleFilter );
                }
            }

            foreach ( var item in itemQry.ToList() )
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

            if ( selectedChannel.ItemsManuallyOrdered && !isFiltered )
            {
                return items.OrderBy( i => i.Order ).ToList();
            }
            else
            {
                return items;
            }
        }

        protected void BindAttributes( ContentChannel channel )
        {
            AvailableAttributes = new List<AttributeCache>();
            int entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannelItem ) ).Id;
            string channelId = channel.Id.ToString();
            string channelTypeId = channel.ContentChannelTypeId.ToString();
            foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
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
                AvailableAttributes.Add( Rock.Web.Cache.AttributeCache.Get( attributeModel ) );
            }
        }

        protected void AddDynamicControls( ContentChannel channel )
        {
            // Remove all columns
            gContentChannelItems.Columns.Clear();
            phAttributeFilters.Controls.Clear();

            if ( channel != null )
            {
                // Add Reorder column
                var reorderField = new ReorderField();
                gContentChannelItems.Columns.Add( reorderField );

                // Add Title column
                var titleField = new BoundField();
                titleField.DataField = "Title";
                titleField.HeaderText = "Title";
                titleField.SortExpression = "Title";
                gContentChannelItems.Columns.Add( titleField );

                // Add Attribute columns
                int entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannelItem ) ).Id;
                string channelId = channel.Id.ToString();
                string channelTypeId = channel.ContentChannelTypeId.ToString();
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = ( IRockControl ) control;
                            rockControl.Label = attribute.Name;
                            rockControl.Help = attribute.Description;
                            phAttributeFilters.Controls.Add( control );
                        }
                        else
                        {
                            var wrapper = new RockControlWrapper();
                            wrapper.ID = control.ID + "_wrapper";
                            wrapper.Label = attribute.Name;
                            wrapper.Controls.Add( control );
                            phAttributeFilters.Controls.Add( wrapper );
                        }

                        string savedValue = gfFilter.GetUserPreference( MakeKeyUniqueToChannel( channel.Id, attribute.Key ) );
                        if ( !string.IsNullOrWhiteSpace( savedValue ) )
                        {
                            try
                            {
                                var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                            }
                            catch
                            {
                                // intentionally ignore
                            }
                        }
                    }

                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gContentChannelItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;
                        boundField.ItemStyle.HorizontalAlign = attribute.FieldType.Field.AlignValue;
                        gContentChannelItems.Columns.Add( boundField );
                    }
                }

                if ( channel.ContentChannelType.DateRangeType != ContentChannelDateType.NoDates )
                {
                    RockBoundField startDateTimeField;
                    RockBoundField expireDateTimeField;
                    if ( channel.ContentChannelType.IncludeTime )
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
                    startDateTimeField.HeaderText = channel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ? "Start" : "Date";
                    startDateTimeField.SortExpression = "StartDateTime";
                    gContentChannelItems.Columns.Add( startDateTimeField );

                    expireDateTimeField.DataField = "ExpireDateTime";
                    expireDateTimeField.HeaderText = "Expire";
                    expireDateTimeField.SortExpression = "ExpireDateTime";
                    if ( channel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange )
                    {
                        gContentChannelItems.Columns.Add( expireDateTimeField );
                    }
                }

                if ( !channel.ContentChannelType.DisablePriority )
                {
                    // Priority column
                    var priorityField = new BoundField();
                    priorityField.DataField = "Priority";
                    priorityField.HeaderText = "Priority";
                    priorityField.SortExpression = "Priority";
                    priorityField.DataFormatString = "{0:N0}";
                    priorityField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                    gContentChannelItems.Columns.Add( priorityField );
                }

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

                // Add occurrences Count column
                var occurrencesField = new BoolField();
                occurrencesField.DataField = "Occurrences";
                occurrencesField.HeaderText = "Event Occurrences";
                gContentChannelItems.Columns.Add( occurrencesField );

                // Add Created By column
                var createdByPersonNameField = new BoundField();
                createdByPersonNameField.DataField = "CreatedByPersonName";
                createdByPersonNameField.HeaderText = "Created By";
                createdByPersonNameField.HtmlEncode = false;
                gContentChannelItems.Columns.Add( createdByPersonNameField );

                // Add Tag Field
                if ( channel.IsTaggingEnabled )
                {
                    var tagField = new BoundField();
                    gContentChannelItems.Columns.Add( tagField );
                    tagField.DataField = "Tags";
                    tagField.HeaderText = "Tags";
                    tagField.ItemStyle.CssClass = "taglist";
                    tagField.HtmlEncode = false;
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

        private string MakeKeyUniqueToChannel( int channelId, string key )
        {
            return string.Format( "{0}-{1}", channelId, key );
        }

        protected string DisplayStatus( ContentChannelItemStatus contentItemStatus )
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