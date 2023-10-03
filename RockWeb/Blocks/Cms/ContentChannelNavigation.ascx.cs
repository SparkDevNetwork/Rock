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

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "Page used to view a content item.",
        Order = 1 )]

    [ContentChannelTypesField(
        "Content Channel Types Include",
        Key = AttributeKey.ContentChannelTypesInclude,
        Description = "Select any specific content channel types to show in this block. Leave all unchecked to show all content channel types ( except for excluded content channel types )",
        IsRequired = false,
        Order = 2 )]

    [ContentChannelTypesField(
        "Content Channel Types Exclude",
        Key = AttributeKey.ContentChannelTypesExclude,
        Description = "Select content channel types to exclude from this block. Note that this setting is only effective if 'Content Channel Types Include' has no specific content channel types selected.",
        IsRequired = false,
        Order = 3 )]

    [ContentChannelsField(
        "Content Channels Filter",
        Key = AttributeKey.ContentChannelsFilter,
        Description = "Select the content channels you would like displayed. This setting will override the Content Channel Types Include/Exclude settings.",
        IsRequired = false,
        Order = 4 )]

    [BooleanField(
        "Show Category Filter",
        Description = "Should block add an option to allow filtering by category?",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowCategoryFilter,
        Order = 5 )]

    [CategoryField( "Parent Category",
        Description = "The parent category to use as the root category available for the user to pick from.",
        IsRequired = false,
        EntityType = typeof( Rock.Model.ContentChannel ),
        Key = AttributeKey.ParentCategory,
        Order = 6 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.CONTENT_CHANNEL_NAVIGATION )]
    public partial class ContentChannelNavigation : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ContentChannelTypesInclude = "ContentChannelTypesInclude";
            public const string ContentChannelTypesExclude = "ContentChannelTypesExclude";
            public const string ContentChannelsFilter = "ContentChannelsFilter";
            public const string ShowCategoryFilter = "ShowCategoryFilter";
            public const string ParentCategory = "ParentCategory";
        }

        #endregion Attribute Keys

        #region Fields

        private const string STATUS_FILTER_SETTING = "status-filter";
        private const string CATEGORY_FILTER_SETTING = "category-filter";
        private const string SELECTED_CHANNEL_SETTING = "selected-channel";

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

            ddlCategory.Visible = GetAttributeValue( AttributeKey.ShowCategoryFilter ).AsBoolean();
            if ( GetAttributeValue( AttributeKey.ShowCategoryFilter ).AsBoolean() )
            {
                BindCategories();
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
            base.OnLoad( e );
            RockPage.AddScriptLink( "~/Scripts/jquery.visible.min.js" );

            string eventTarget = this.Page.Request.Params["__EVENTTARGET"] ?? string.Empty;

            if ( !Page.IsPostBack )
            {
                var preferences = GetBlockPersonPreferences();

                BindFilter();

                tglStatus.Checked = preferences.GetValue( STATUS_FILTER_SETTING ).AsBoolean();

                SelectedChannelId = PageParameter( "ContentChannelId" ).AsIntegerOrNull();

                if ( !SelectedChannelId.HasValue )
                {
                    var selectedChannelGuid = PageParameter( "ContentChannelGuid" ).AsGuidOrNull();

                    if ( selectedChannelGuid.HasValue )
                    {
                        SelectedChannelId = ContentChannelCache.Get( selectedChannelGuid.Value )?.Id;
                    }
                }

                if ( !SelectedChannelId.HasValue )
                {
                    SelectedChannelId = preferences.GetValue( SELECTED_CHANNEL_SETTING ).AsIntegerOrNull();
                }

                if ( ddlCategory.Visible )
                {
                    var categoryGuid = PageParameter( "CategoryGuid" ).AsGuidOrNull();
                    if ( categoryGuid.HasValue )
                    {
                        var categoryId = CategoryCache.Get( categoryGuid.Value )?.Id;

                        preferences.SetValue( CATEGORY_FILTER_SETTING, categoryId.ToString() );
                        preferences.Save();

                        ddlCategory.SetValue( categoryId );
                    }
                    else
                    {
                        ddlCategory.SetValue( preferences.GetValue( CATEGORY_FILTER_SETTING ).AsIntegerOrNull() );
                    }
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
            ddlCategory.Visible = GetAttributeValue( AttributeKey.ShowCategoryFilter ).AsBoolean();
            if ( GetAttributeValue( AttributeKey.ShowCategoryFilter ).AsBoolean() )
            {
                BindCategories();
            }

            GetData();
        }

        /// <summary>
        /// Handles the SelectItem event of the ddlCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCategory_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            var categoryGuid = CategoryCache.Get( ddlCategory.SelectedValue.AsInteger() )?.Guid;
            var queryString = new Dictionary<string, string>();
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( CATEGORY_FILTER_SETTING, ddlCategory.SelectedValue );
            preferences.Save();

            if ( categoryGuid.HasValue )
            {
                queryString.Add( "CategoryGuid", categoryGuid.ToString() );
            }

            // Navigate to page with route parameters set so new Url is generated in browser 
            NavigateToCurrentPage( queryString );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tgl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tgl_CheckedChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( STATUS_FILTER_SETTING, tglStatus.Checked.ToString() );
            preferences.Save();

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
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( SELECTED_CHANNEL_SETTING, selectedChannelValue );
            preferences.Save();

            SelectedChannelId = selectedChannelValue.AsIntegerOrNull();

            var selectedChannelGuid = ContentChannelCache.Get( SelectedChannelId.Value ).Guid;
            var queryString = new Dictionary<string, string> { { "ContentChannelGuid", selectedChannelGuid.ToString() } };

            // Get CategoryGuid from Route
            var categoryGuid = PageParameter( "CategoryGuid" ).AsGuidOrNull();
            if ( !categoryGuid.HasValue )
            {
                var categoryId = ddlCategory.SelectedValueAsId();
                categoryGuid = CategoryCache.Get( categoryId.GetValueOrDefault() )?.Guid;
            }

            // if user has selected a category or one was provided as a query param add it to the new route params
            if ( categoryGuid.HasValue )
            {
                queryString.Add( "CategoryGuid", categoryGuid.ToString() );
            }

            // Navigate to page with route parameters set so new Url is generated in browser 
            NavigateToCurrentPage( queryString );
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
            gfFilter.SetFilterPreference( "Date Range", drpDateRange.DelimitedValues );
            gfFilter.SetFilterPreference( "Status", ddlStatus.SelectedValue );
            gfFilter.SetFilterPreference( "Title", tbTitle.Text );
            int personId = ppCreatedBy.PersonId ?? 0;
            gfFilter.SetFilterPreference( "Created By", personId.ToString() );

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
                            gfFilter.SetFilterPreference( MakeKeyUniqueToChannel( SelectedChannelId.Value, attribute.Key ), attribute.Name, values.ToJson() );
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
                NavigateToLinkedPage( AttributeKey.DetailPage, "ContentItemId", 0, "ContentChannelId", SelectedChannelId.Value );
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
                NavigateToLinkedPage( AttributeKey.DetailPage, "ContentItemId", contentItem.Id );
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
            drpDateRange.DelimitedValues = gfFilter.GetFilterPreference( "Date Range" );
            ddlStatus.BindToEnum<ContentChannelItemStatus>( true );
            int? statusID = gfFilter.GetFilterPreference( "Status" ).AsIntegerOrNull();
            if ( statusID.HasValue )
            {
                ddlStatus.SetValue( statusID.Value.ToString() );
            }

            tbTitle.Text = gfFilter.GetFilterPreference( "Title" );

            int? personId = gfFilter.GetFilterPreference( "Created By" ).AsIntegerOrNull();

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

        /// <summary>
        /// Gets the data to display.
        /// </summary>
        private void GetData()
        {
            var rockContext = new RockContext();
            var itemService = new ContentChannelItemService( rockContext );

            // Get all of the content channels
            var contentChannelsQry = new ContentChannelService( rockContext ).Queryable( "ContentChannelType" ).AsNoTracking();

            List<Guid> contentChannelGuidsFilter = GetAttributeValue( AttributeKey.ContentChannelsFilter ).SplitDelimitedValues().AsGuidList();
            List<Guid> contentChannelTypeGuidsInclude = GetAttributeValue( AttributeKey.ContentChannelTypesInclude ).SplitDelimitedValues().AsGuidList();
            List<Guid> contentChannelTypeGuidsExclude = GetAttributeValue( AttributeKey.ContentChannelTypesExclude ).SplitDelimitedValues().AsGuidList();

            if ( contentChannelGuidsFilter.Any() )
            {
                // if contentChannelGuidsFilter is specified, only get those content channels.
                // NOTE: This take precedence over all the other Include/Exclude settings.
                contentChannelsQry = contentChannelsQry.Where( a => contentChannelGuidsFilter.Contains( a.Guid ) );
            }
            else if ( contentChannelTypeGuidsInclude.Any() )
            {
                // if contentChannelTypeGuidsInclude is specified, only get contentChannelTypes that are in the contentChannelTypeGuidsInclude
                // NOTE: no need to factor in contentChannelTypeGuidsExclude since included would take precedence and the excluded ones would already not be included
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


            if ( GetAttributeValue( AttributeKey.ShowCategoryFilter ).AsBoolean() )
            {
                int? categoryId = null;
                var categoryGuid = PageParameter( "CategoryGuid" ).AsGuidOrNull();
                var selectedChannelGuid = PageParameter( "ContentChannelGuid" ).AsGuidOrNull();

                if ( selectedChannelGuid.HasValue )
                {
                    categoryId = CategoryCache.Get( categoryGuid.GetValueOrDefault() )?.Id;
                }
                else
                {
                    categoryId = ddlCategory.SelectedValueAsId();
                }

                var preferences = GetBlockPersonPreferences();

                preferences.SetValue( CATEGORY_FILTER_SETTING, categoryId.ToString() );
                preferences.Save();

                ddlCategory.SetValue( categoryId );

                var parentCategoryGuid = GetAttributeValue( AttributeKey.ParentCategory ).AsGuidOrNull();
                if ( ddlCategory.Visible && categoryId.HasValue )
                {
                    contentChannelsQry = contentChannelsQry.Where( a => a.Categories.Any( c => c.Id == categoryId ) );
                }
                else if ( parentCategoryGuid.HasValue )
                {
                    var parentCategoryId = CategoryCache.GetId( parentCategoryGuid.Value );
                    contentChannelsQry = contentChannelsQry.Where( a => a.Categories.Any( c => c.ParentCategoryId == parentCategoryId ) );
                }
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
                    DateStatus = DisplayDateStatus( i.StartDateTime ),
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
            drp.DelimitedValues = gfFilter.GetFilterPreference( "Date Range" );
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

            var status = gfFilter.GetFilterPreference( "Status" ).ConvertToEnumOrNull<ContentChannelItemStatus>();
            if ( status.HasValue )
            {
                isFiltered = true;
                itemQry = itemQry.Where( i => i.Status == status );
            }

            string title = gfFilter.GetFilterPreference( "Title" );
            if ( !string.IsNullOrWhiteSpace( title ) )
            {
                isFiltered = true;
                itemQry = itemQry.Where( i => i.Title.Contains( title ) );
            }

            int? personId = gfFilter.GetFilterPreference( "Created By" ).AsIntegerOrNull();
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

        protected string DisplayDateStatus( DateTime aDate )
        {
            return ( aDate > RockDateTime.Now ) ? "<i class='fa fa-clock'></i>" : string.Empty;
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

                        string savedValue = gfFilter.GetFilterPreference( MakeKeyUniqueToChannel( channel.Id, attribute.Key ) );
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

                    var dateStatusField = new BoundField();
                    gContentChannelItems.Columns.Add( dateStatusField );
                    dateStatusField.DataField = "DateStatus";
                    dateStatusField.HeaderText = "";
                    dateStatusField.HtmlEncode = false;

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
                    var securityField = new SecurityField();
                    gContentChannelItems.Columns.Add( securityField );
                    securityField.TitleField = "Title";
                    securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannelItem ) ).Id;

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

        /// <summary>
        /// Bind the category selector to the correct set of categories.
        /// </summary>
        private void BindCategories()
        {
            var parentCategoryGuid = GetAttributeValue( AttributeKey.ParentCategory ).AsGuidOrNull();

            var categories = new CategoryService( new RockContext() )
                .GetByEntityTypeId( EntityTypeCache.GetId( typeof( Rock.Model.ContentChannel ).FullName ) )
                .Where( c => ( !parentCategoryGuid.HasValue && !c.ParentCategoryId.HasValue ) ||
                             ( parentCategoryGuid.HasValue && c.ParentCategory != null && c.ParentCategory.Guid == parentCategoryGuid ) )
                .AsQueryable()
                .ToList();
            ddlCategory.Visible = categories.Any();
            ddlCategory.DataSource = categories;
            ddlCategory.DataTextField = "Name";
            ddlCategory.DataValueField = "Id";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert( 0, new ListItem() );
        }

        #endregion

    }

}