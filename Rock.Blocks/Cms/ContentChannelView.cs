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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Field.Types;
using Rock.Lava;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Security.SecurityGrantRules;
using Rock.Utility;
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentChannelView;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Cms
{
    [DisplayName( "Content Channel View" )]
    [Category( "CMS" )]
    [Description( "Block to display dynamic content channel items." )]
    [IconCssClass( "ti ti-article" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ConfigurationChangedReload( BlockReloadMode.Block )]

    #region Block Attributes

    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this content channel block.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EnabledLavaCommands )]

    [LinkedPage(
        "Detail Page",
        Description = "The page to navigate to for details.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.DetailPage )]

    [ContentChannelField(
        "Channel",
        Description = "The channel to display items from.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.Channel )]

    [EnumsField(
        "Status",
        Description = "Include items with the following status.",
        IsRequired = false,
        EnumSourceType = typeof( ContentChannelItemStatus ),
        DefaultValue = "2",
        Category = "CustomSetting",
        Key = AttributeKey.Status )]

    [CodeEditorField(
        "Template",
        Description = "The template to use when formatting the list of items.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 600,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.Template )]

    [IntegerField(
        "Count",
        Description = "The maximum number of items to display.",
        IsRequired = false,
        DefaultIntegerValue = 5,
        Category = "CustomSetting",
        Key = AttributeKey.Count )]

    [IntegerField(
        "Item Cache Duration",
        Description = "Number of seconds to cache the content items returned by the selected filter.",
        IsRequired = false,
        DefaultIntegerValue = 3600,
        Category = "CustomSetting",
        Key = AttributeKey.CacheDuration )]

    [IntegerField(
        "Output Cache Duration",
        Description = "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Category = "CustomSetting",
        Key = AttributeKey.OutputCacheDuration )]

    [CustomCheckboxListField(
        "Cache Tags",
        Description = "Cached tags are used to link cached content so that it can be expired as a group",
        IsRequired = false,
        Key = AttributeKey.CacheTags,
        Category = "CustomSetting" )]

    [IntegerField(
        "Filter Id",
        Description = "The data filter that is used to filter items",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Category = "CustomSetting",
        Key = AttributeKey.FilterId )]

    [BooleanField(
        "Query Parameter Filtering",
        Description = "Determines if block should evaluate the query string parameters for additional filter criteria.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.QueryParameterFiltering )]

    [TextField(
        "Order",
        Description = "The specifics of how items should be ordered. This value is set through configuration and should not be modified here.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.Order )]

    [BooleanField(
        "Merge Content",
        Description = "Should the content data and attribute values be merged using the Lava template engine.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.MergeContent )]

    [BooleanField(
        "Set Page Title",
        Description = "Determines if the block should set the page title with the channel name or content item.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.SetPageTitle )]

    [BooleanField(
        "RSS Autodiscover",
        Description = "Determines if a RSS autodiscover link should be added to the page head.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.RssAutodiscover )]

    [TextField(
        "Meta Description Attribute",
        Description = "Attribute to use for storing the description attribute.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.MetaDescriptionAttribute )]

    [TextField(
        "Meta Image Attribute",
        Description = "Attribute to use for storing the image attribute.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.MetaImageAttribute )]

    [BooleanField(
        "Enable Tag List",
        Description = "Determines if the ItemTagList lava parameter will be populated.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.EnableTagList )]

    [BooleanField(
        "Enable Archive Summary",
        Description = "When enabled an additional \"ArchiveSummary\" collection will be available in Lava to help create a summary list of content channel items by month/year. This collection will be cached using the same duration as the Item Cache and will hold the following properties: Month (int), MonthName, Year, Count.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.EnableArchiveSummary )]

    [EnumField(
        "Personalization",
        Description = "The setting determines how personalization effect the results shown. Ignore will not consider segments or request filters, Prioritize will add items with matching items to the top of the list (in order by the sort order) and Filter will only show items that match the current individuals segments and request filters.",
        EnumSourceType = typeof( PersonalizationFilterType ),
        Category = "CustomSetting",
        Key = AttributeKey.Personalization )]

    [TextField(
        "Context Filter Attribute",
        Description = "Item attribute to compare when filtering items using the block Context. If the block doesn't have a context, this setting will be ignored.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.ContextAttribute )]

    [ContextAware]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "0F84D787-0A18-472A-892B-2BA2DB7F5E2D" )]
    [Rock.SystemGuid.BlockTypeGuid( "4C2DE663-B2E1-48E1-917D-330D181548F0" )]
    public partial class ContentChannelView : RockBlockType, IHasCustomActions, IBreadCrumbBlock
    {
        #region Keys and Constants

        private static class AttributeKey
        {
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string DetailPage = "DetailPage";
            public const string Channel = "Channel";
            public const string Status = "Status";
            public const string Template = "Template";
            public const string Count = "Count";
            public const string CacheDuration = "CacheDuration";
            public const string OutputCacheDuration = "OutputCacheDuration";
            public const string CacheTags = "CacheTags";
            public const string FilterId = "FilterId";
            public const string QueryParameterFiltering = "QueryParameterFiltering";
            public const string Order = "Order";
            public const string MergeContent = "MergeContent";
            public const string SetPageTitle = "SetPageTitle";
            public const string RssAutodiscover = "RssAutodiscover";
            public const string MetaDescriptionAttribute = "MetaDescriptionAttribute";
            public const string MetaImageAttribute = "MetaImageAttribute";
            public const string EnableTagList = "EnableTagList";
            public const string EnableArchiveSummary = "EnableArchiveSummary";
            public const string Personalization = "Personalization";
            public const string ContextAttribute = "ContextAttribute";
        }

        private static class PageParameterKey
        {
            public const string Page = "Page";
            public const string Item = "Item";
            public const string Tag = "Tag";
            public const string Year = "Year";
            public const string Month = "Month";
        }

        private static class CacheKey
        {
            public const string Content = "Content";
            public const string Template = "Template";
            public const string Output = "Output";
            public const string Tags = "Tags";
            public const string ArchiveSummary = "DateFilter";
        }
        
        private const string OrderItemsByKeyValueDelimiter = "^";
        private const string OrderItemsByEntryDelimiter = "|";

        #endregion Keys and Constants

        #region RockBlockType Overrides

        protected override string GetInitialHtmlContent()
        {
            try
            {
                return RenderContent();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return $@"<div class='alert alert-danger'>
    <strong>Content Channel View Error</strong><br/>
    {ex.Message.EncodeHtml()}
</div>";
            }
        }

        #endregion RockBlockType Overrides

        #region Block Actions

        /// <summary>
        /// Gets the values and all other required details that will be needed to display the custom settings modal.
        /// </summary>
        /// <returns>A box that contains the custom settings values and additional data.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            return GetSettingsForContentChannel( GetAttributeValue( AttributeKey.Channel ).AsGuidOrNull() );
        }

        [BlockAction]
        public BlockActionResult GetCustomSettingsForContentChannel( Guid contentChannelGuid )
        {
            return GetSettingsForContentChannel( contentChannelGuid );
        }

        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<ContentChannelViewCustomSettingsBag, ContentChannelViewCustomSettingsOptionsBag> box )
        {
            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var block = new BlockService( RockContext ).Get( this.BlockId );
            block.LoadAttributes( RockContext );

            #region Layout / Initial Page Load

            box.IfValidProperty( nameof( box.Settings.ContentChannelGuid ),
                () => block.SetAttributeValue( AttributeKey.Channel, box.Settings.ContentChannelGuid.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.ContentChannelItemStatuses ),
                () => block.SetAttributeValue( AttributeKey.Status, box.Settings.ContentChannelItemStatuses.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Settings.LavaTemplate ),
                () => block.SetAttributeValue( AttributeKey.Template, box.Settings.LavaTemplate ) );

            #endregion Layout / Initial Page Load

            #region General

            box.IfValidProperty( nameof( box.Settings.ItemsPerPage ),
                () => block.SetAttributeValue( AttributeKey.Count, box.Settings.ItemsPerPage.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsPageTitleUpdateEnabled ),
                () => block.SetAttributeValue( AttributeKey.SetPageTitle, box.Settings.IsPageTitleUpdateEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsItemMergeFieldEnabled ),
                () => block.SetAttributeValue( AttributeKey.MergeContent, box.Settings.IsItemMergeFieldEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsItemTagListMergeFieldEnabled ),
                () => block.SetAttributeValue( AttributeKey.EnableTagList, box.Settings.IsItemTagListMergeFieldEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsArchiveSummaryMergeFieldEnabled ),
                () => block.SetAttributeValue( AttributeKey.EnableArchiveSummary, box.Settings.IsArchiveSummaryMergeFieldEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.DetailPage ),
                () => block.SetAttributeValue( AttributeKey.DetailPage, box.Settings.DetailPage.ToCommaDelimitedPageRouteValues() ) );

            box.IfValidProperty( nameof( box.Settings.ItemCacheDuration ),
                () => block.SetAttributeValue( AttributeKey.CacheDuration, box.Settings.ItemCacheDuration.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.OutputCacheDuration ),
                () => block.SetAttributeValue( AttributeKey.OutputCacheDuration, box.Settings.OutputCacheDuration.ToString() ) );
            
            box.IfValidProperty( nameof( box.Settings.CacheTags ),
                () => block.SetAttributeValue( AttributeKey.CacheTags, box.Settings.CacheTags?.AsDelimited( "," ) ) );

            #endregion General

            #region Filters & Sorting

            box.IfValidProperty( nameof( box.Settings.DataViewFilter ),
                () =>
                {
                    var dataViewFilter = DataFilterObsidianHelper.ToEntity( box.Settings.DataViewFilter, typeof( ContentChannelItem ), RockContext, RequestContext );

                    if ( dataViewFilter == null )
                    {
                        // Null stores an empty string. GetAttributeValue( FilterId ).AsIntegerOrNull()
                        // then returns null, skipping the data filter in GetContentChannelItemQuery.
                        block.SetAttributeValue( AttributeKey.FilterId, null );
                    }
                    else
                    {
                        var dataViewFilterService = new DataViewFilterService( RockContext );
                        var oldDataViewFilter = dataViewFilterService.Get( dataViewFilter.Guid );

                        if ( oldDataViewFilter != null )
                        {
                            // If another ContentChannelView block uses the same DataViewFilter don't delete it.
                            // In this case it likely means this block is a copy of, or was copied from another page/block.
                            // Instead we'll create a new DataViewFilter and remove references to the existing one(s).
                            var blockEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.BLOCK ).ToIntSafe();
                            var contentChannelViewWebFormsBlockTypeId = BlockTypeCache.GetId( Rock.SystemGuid.BlockType.CONTENT_CHANNEL_VIEW.AsGuid() ).ToIntSafe().ToString();
                            var contentChannelViewObsidianBlockTypeId = BlockCache.BlockTypeId.ToString();
                            
                            var countOfWebFormsContentChannelViewsUsingFilterId = new AttributeValueService( RockContext )
                                .GetByEntityTypeQualified( blockEntityTypeId, "BlockTypeId", contentChannelViewWebFormsBlockTypeId )
                                .Count( av => av.Attribute.Key == AttributeKey.FilterId && av.Value == oldDataViewFilter.Id.ToString() );

                            var countOfObsidianContentChannelViewsUsingFilterId = new AttributeValueService( RockContext )
                                .GetByEntityTypeQualified( blockEntityTypeId, "BlockTypeId", contentChannelViewObsidianBlockTypeId )
                                .Count( av => av.Attribute.Key == AttributeKey.FilterId && av.Value == oldDataViewFilter.Id.ToString() );

                            var countOfContentChannelViewsUsingFilterId = countOfWebFormsContentChannelViewsUsingFilterId + countOfObsidianContentChannelViewsUsingFilterId;

                            if ( countOfContentChannelViewsUsingFilterId == 1 )
                            {
                                // If we're the only block instance using this DataViewFilterId it's safe to delete the old one.
                                DeleteDataViewFilter( oldDataViewFilter, dataViewFilterService );
                            }
                        }

                        dataViewFilterService.Add( dataViewFilter );
                        RockContext.SaveChanges();
                        block.SetAttributeValue( AttributeKey.FilterId, dataViewFilter.Id.ToString() );
                    }
                } );

            box.IfValidProperty( nameof( box.Settings.ContextFilterAttributeKey ),
                () => block.SetAttributeValue( AttributeKey.ContextAttribute, box.Settings.ContextFilterAttributeKey ) );

            box.IfValidProperty( nameof( box.Settings.PersonalizationFilterType ),
                () => block.SetAttributeValue( AttributeKey.Personalization, box.Settings.PersonalizationFilterType.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsPageParameterFilteringEnabled ),
                () => block.SetAttributeValue( AttributeKey.QueryParameterFiltering, box.Settings.IsPageParameterFilteringEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.OrderItemsBy ),
                () => block.SetAttributeValue( AttributeKey.Order, OrderItemsByListItemBagsToString( box.Settings.OrderItemsBy ) ) );

            #endregion Filters & Sorting

            #region Social Sharing

            box.IfValidProperty( nameof( box.Settings.MetaDescriptionAttributeValueKey ),
                () => block.SetAttributeValue( AttributeKey.MetaDescriptionAttribute, box.Settings.MetaDescriptionAttributeValueKey ) );

            box.IfValidProperty( nameof( box.Settings.MetaImageAttributeValueKey ),
                () => block.SetAttributeValue( AttributeKey.MetaImageAttribute, box.Settings.MetaImageAttributeValueKey ) );

            box.IfValidProperty( nameof( box.Settings.IsSetRssAutodiscoverLinkEnabled ),
                () => block.SetAttributeValue( AttributeKey.RssAutodiscover, box.Settings.IsSetRssAutodiscoverLinkEnabled.ToString() ) );

            #endregion Social Sharing

            block.SaveAttributeValues( RockContext );

            // Clear the cache after saving custom settings so that any changes to the Lava template
            // or other settings will be reflected immediately
            // instead of showing stale cached content.
            ClearCache( GetCacheKey( CacheKey.Content ) );
            ClearCache( GetCacheKey( CacheKey.Output ) );
            ClearCache( GetCacheKey( CacheKey.Template ) );

            return ActionOk();
        }

        #endregion Block Actions

        #region IHasCustomActions Implementation

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "ti ti-edit",
                    Tooltip = "Edit Criteria",
                    ComponentFileUrl = "/Obsidian/Blocks/Cms/ContentChannelView/contentChannelViewCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion IHasCustomActions Implementation

        #region IBreadCrumbBlock Implementation

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var result = new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>()
            };

            if ( !GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean() )
            {
                return result;
            }

            var channelGuid = GetAttributeValue( AttributeKey.Channel ).AsGuidOrNull();
            if ( !channelGuid.HasValue )
            {
                return result;
            }

            var contentChannel = ContentChannelCache.Get( channelGuid.Value );
            if ( contentChannel == null )
            {
                return result;
            }

            var breadCrumbName = contentChannel.Name;
            var itemParam = pageReference.GetPageParameter( PageParameterKey.Item );

            if ( itemParam.IsNotNullOrWhiteSpace() )
            {
                var itemTitle = new ContentChannelItemService( RockContext )
                    .GetSelect( itemParam.AsInteger(), i => i.Title );

                if ( itemTitle.IsNotNullOrWhiteSpace() )
                {
                    breadCrumbName = itemTitle;
                }
            }

            result.BreadCrumbs.Add( new BreadCrumbLink( breadCrumbName, pageReference ) );

            return result;
        }

        #endregion IBreadCrumbBlock Implementation

        #region Private Methods

        private string OrderItemsByListItemBagsToString( List<ListItemBag> orderItemsBy )
        {
            if ( orderItemsBy == null || !orderItemsBy.Any() )
            {
                return null;
            }

            return orderItemsBy
                .Select( o => $"{o.Value}{OrderItemsByKeyValueDelimiter}{o.Text}" )
                .ToList()
                .AsDelimited( OrderItemsByEntryDelimiter );
        }

        private List<ListItemBag> OrderItemsByStringToListItemBags( string orderItemsBy )
        {
            if ( orderItemsBy.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var list = new List<ListItemBag>();

            var orderItemsByEntries = orderItemsBy
                .SplitDelimitedValues( OrderItemsByEntryDelimiter );

            foreach ( var orderItemsByEntry in orderItemsByEntries )
            {
                var keyValue = orderItemsByEntry.SplitDelimitedValues( OrderItemsByKeyValueDelimiter, StringSplitOptions.None );
                if ( keyValue.Length == 2 )
                {
                    list.Add( new ListItemBag
                    {
                        Value = keyValue[0],
                        Text = keyValue[1]
                    } );
                }
            }

            if ( list.Any() )
            {
                return list;
            }
            else
            {
                return null;
            }
        }

        private void DeleteDataViewFilter( DataViewFilter dataViewFilter, DataViewFilterService service )
        {
            if ( dataViewFilter != null )
            {
                foreach ( var childFilter in dataViewFilter.ChildFilters.ToList() )
                {
                    DeleteDataViewFilter( childFilter, service );
                }

                service.Delete( dataViewFilter );
            }
        }

        private BlockActionResult GetSettingsForContentChannel( Guid? contentChannelGuid )
        {
            var settings = new ContentChannelViewCustomSettingsBag();
            var options = new ContentChannelViewCustomSettingsOptionsBag();

            // Content Channel
            options.ContentChannels = ContentChannelCache.All()
                .OrderBy( c => c.Name )
                .Where( a => a.ContentChannelType.ShowInChannelList == true )
                .ToListItemBagList();
            ContentChannelCache contentChannel = null;
            if ( contentChannelGuid != null && options.ContentChannels.Any( c => c.Value.AsGuidOrNull() == contentChannelGuid ) )
            {
                settings.ContentChannelGuid = contentChannelGuid;
                contentChannel = ContentChannelCache.Get( contentChannelGuid.Value );

                if ( contentChannel != null )
                {
                    contentChannel.LoadAttributes();
                }
            }

            // Item Statuses
            if ( contentChannel != null && contentChannel.RequiresApproval && !contentChannel.ContentChannelType.DisableStatus )
            {
                options.ContentChannelItemStatuses = typeof( ContentChannelItemStatus ).ToEnumListItemBag();
                settings.ContentChannelItemStatuses = GetAttributeValue( AttributeKey.Status ).SplitDelimitedValues().AsEnumList<ContentChannelItemStatus>();
            }

            // Lava template
            settings.LavaTemplate = GetAttributeValue( AttributeKey.Template );

            // Items Per Page
            settings.ItemsPerPage = GetAttributeValue( AttributeKey.Count ).AsInteger(); // Get whatever is stored. Save specified or default to 5.

            // Set Page Title
            settings.IsPageTitleUpdateEnabled = GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean();

            // Merge Content
            settings.IsItemMergeFieldEnabled = GetAttributeValue( AttributeKey.MergeContent ).AsBoolean();

            // Enable Tag List
            settings.IsItemTagListMergeFieldEnabled = GetAttributeValue( AttributeKey.EnableTagList ).AsBoolean();

            // Enable Archive Summary
            settings.IsArchiveSummaryMergeFieldEnabled = GetAttributeValue( AttributeKey.EnableArchiveSummary ).AsBoolean();

            // Detail Page
            settings.DetailPage = GetAttributeValue( AttributeKey.DetailPage ).ToPageRouteValueBag();

            // Item Cache Duration
            settings.ItemCacheDuration = GetAttributeValue( AttributeKey.CacheDuration ).AsInteger();

            // Output Cache Duration
            settings.OutputCacheDuration = GetAttributeValue( AttributeKey.OutputCacheDuration ).AsInteger();

            // Cache Tags
            options.CacheTags = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS.AsGuid() )
                .DefinedValues
                .Where( dv => dv.IsActive )
                .ToListItemBagList();
            if ( options.CacheTags.Any() )
            {
                var selectedCacheTags = GetAttributeValue( AttributeKey.CacheTags ).SplitDelimitedValues();
                var vettedSelectedCacheTags = new List<string>();

                foreach ( var cacheTag in selectedCacheTags )
                {
                    if ( options.CacheTags.Any( c => c.Value == cacheTag ) )
                    {
                        vettedSelectedCacheTags.Add( cacheTag );
                    }
                }

                settings.CacheTags = vettedSelectedCacheTags.Any() ? vettedSelectedCacheTags : null;
            }

            // Data View Filter
            var dataViewFilterId = GetAttributeValue( AttributeKey.FilterId ).AsIntegerOrNull();
            if ( dataViewFilterId.HasValue )
            {
                var dataViewFilter = DataViewFilterCache.Get( dataViewFilterId.Value );
                if ( dataViewFilter != null )
                {
                    settings.DataViewFilter = DataFilterObsidianHelper.ToBag( dataViewFilter, typeof( ContentChannelItem ), RockContext, RequestContext );
                }
            }

            if ( contentChannel != null )
            {
                var itemAttributes = AttributeCache.AllForEntityType<ContentChannelItem>()
                    .Where( a => a.IsActive )
                    .Where( a =>
                        (
                            a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( contentChannel.ContentChannelTypeId.ToString() )
                        )
                        || (
                            a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( contentChannel.Id.ToString() )
                        )
                    )
                    .OrderByDescending( a => a.EntityTypeQualifierColumn )
                    .ThenBy( a => a.Order )
                    .ToList();

                // Context Filter Attribute
                options.ContextFilterAttributes = new List<ListItemBag>();
                foreach ( var attribute in itemAttributes )
                {
                    options.ContextFilterAttributes.Add( new ListItemBag
                    {
                        Text = attribute.Name,
                        Value = attribute.Key
                    } );
                }

                settings.ContextFilterAttributeKey = GetAttributeValue( AttributeKey.ContextAttribute );

                // Personalization
                if ( contentChannel.EnablePersonalization )
                {
                    options.IsPersonalizationVisible = true;
                    options.PersonalizationFilterTypes = typeof( PersonalizationFilterType ).ToEnumListItemBag();
                    settings.PersonalizationFilterType = GetAttributeValue( AttributeKey.Personalization ).ConvertToEnumOrNull<PersonalizationFilterType>() ?? PersonalizationFilterType.Ignore;
                }

                // Enable Query/Route Parameter Filtering
                settings.IsPageParameterFilteringEnabled = GetAttributeValue( AttributeKey.QueryParameterFiltering ).AsBoolean();

                // Order Items By
                var orderItemsByKeyOptions = new Dictionary<string, string>
                {
                    { "Title", "Title" },
                    { "Priority", "Priority" },
                    { "Status", "Status" },
                    { "StartDateTime", "Start" },
                    { "ExpireDateTime", "Expire" },
                    { "Order", "Order" }
                };

                foreach ( var attribute in itemAttributes )
                {
                    var computedKey = $"Attribute:{attribute.Key}";
                    if ( !orderItemsByKeyOptions.ContainsKey( computedKey ) )
                    {
                        orderItemsByKeyOptions.Add( computedKey, attribute.Name );
                    }
                }

                if ( orderItemsByKeyOptions.Any() )
                {
                    options.OrderItemsByKeyOptions = orderItemsByKeyOptions
                        .Select( kvp => new ListItemBag
                        {
                            Value = kvp.Key,
                            Text = kvp.Value
                        } )
                        .ToList();
                    options.OrderItemsByValueOptions = new List<ListItemBag>
                    {
                        new ListItemBag
                        {
                            Value = "0",
                            Text = "Ascending"
                        },
                        new ListItemBag
                        {
                            Value = "1",
                            Text = "Descending"
                        }
                    };

                    settings.OrderItemsBy = OrderItemsByStringToListItemBags( GetAttributeValue( AttributeKey.Order ) )?
                        // Only keep orderByEntries that have a valid key.
                        .Where( orderByListItemBag => orderItemsByKeyOptions.Any( keyOption => keyOption.Key == orderByListItemBag.Value ) )
                        .ToList();
                }

                // Meta Description Attribute
                var metaDescriptionAttributes = new Dictionary<string, string>();
                foreach ( var attribute in contentChannel.Attributes )
                {
                    var computedKey = $"C^{attribute.Key}";
                    if ( !metaDescriptionAttributes.ContainsKey( computedKey ) )
                    {
                        metaDescriptionAttributes.Add( computedKey, $"Channel: {attribute.Value.Name}" );
                    }
                }

                foreach ( var attribute in itemAttributes )
                {
                    var computedKey = $"I^{attribute.Key}";
                    if ( !metaDescriptionAttributes.ContainsKey( computedKey ) )
                    {
                        metaDescriptionAttributes.Add( computedKey, $"Item: {attribute.Name}" );
                    }
                }

                if ( metaDescriptionAttributes.Any() )
                {
                    options.MetaDescriptionAttributes = metaDescriptionAttributes
                        .Select( a => new ListItemBag
                        {
                            Value = a.Key,
                            Text = a.Value
                        } )
                        .ToList();

                    var metaDescriptionAttributeValue = GetAttributeValue( AttributeKey.MetaDescriptionAttribute );
                    if ( options.MetaDescriptionAttributes.Any( a => a.Value == metaDescriptionAttributeValue ) )
                    {
                        settings.MetaDescriptionAttributeValueKey = metaDescriptionAttributeValue;
                    }
                }

                // Meta Image Attribute
                var metaImageAttributes = new Dictionary<string, string>();
                foreach ( var attribute in contentChannel.Attributes )
                {
                    var computedKey = $"C^{attribute.Key}";

                    if ( !metaImageAttributes.ContainsKey( computedKey )
                            && attribute.Value.FieldType.Field is ImageFieldType )
                    {
                        metaImageAttributes.Add( computedKey, $"Channel: {attribute.Value.Name}" );
                    }
                }

                foreach ( var attribute in itemAttributes )
                {
                    var computedKey = $"I^{attribute.Key}";

                    if ( !metaImageAttributes.ContainsKey( computedKey )
                            && attribute.FieldType.Name == "Image" )
                    {
                        metaImageAttributes.Add( computedKey, $"Item: {attribute.Name}" );
                    }
                }

                if ( metaImageAttributes.Any() )
                {
                    options.MetaImageAttributes = metaImageAttributes
                        .Select( a => new ListItemBag
                        {
                            Value = a.Key,
                            Text = a.Value
                        } )
                        .ToList();

                    var metaImageAttributeValue = GetAttributeValue( AttributeKey.MetaImageAttribute );
                    if ( options.MetaImageAttributes.Any( a => a.Value == metaImageAttributeValue ) )
                    {
                        settings.MetaImageAttributeValueKey = metaImageAttributeValue;
                    }
                }

                // Set RSS Autodiscover Link
                options.IsSetRssAutodiscoverLinkVisible = contentChannel.EnableRss;
                if ( options.IsSetRssAutodiscoverLinkVisible )
                {
                    settings.IsSetRssAutodiscoverLinkEnabled = GetAttributeValue( AttributeKey.RssAutodiscover ).AsBoolean();
                }
            }

            return ActionOk( new CustomSettingsBox<ContentChannelViewCustomSettingsBag, ContentChannelViewCustomSettingsOptionsBag>
            {
                Settings = settings,
                Options = options,
                SecurityGrantToken = GetSecurityGrantToken()
            } );
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</returns>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new Rock.Security.SecurityGrant();
            securityGrant.AddRule( new DataViewFilterEditorSecurityGrantRule() { EntityTypeGuid = SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid() } );

            return securityGrant.ToToken();
        }

        private string RenderContent()
        {
            var isMergeContentEnabled = GetAttributeValue( AttributeKey.MergeContent ).AsBoolean();
            var isSetPageTitleEnabled = GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean();
            var isRssAutodiscoverEnabled = GetAttributeValue( AttributeKey.RssAutodiscover ).AsBoolean();
            var isQueryParameterFilteringEnabled = GetAttributeValue( AttributeKey.QueryParameterFiltering ).AsBoolean();
            var isTagListEnabled = GetAttributeValue( AttributeKey.EnableTagList ).AsBoolean();
            var isArchiveSummaryEnabled = GetAttributeValue( AttributeKey.EnableArchiveSummary ).AsBoolean();

            var metaDescriptionAttributeValue = GetAttributeValue( AttributeKey.MetaDescriptionAttribute );
            var metaImageAttributeValue = GetAttributeValue( AttributeKey.MetaImageAttribute );
            var outputCacheDuration = GetAttributeValue( AttributeKey.OutputCacheDuration ).AsIntegerOrNull();
            var paginationNumber = PageParameter( PageParameterKey.Page ).AsIntegerOrNull() ?? 1;

            if ( CanUseOutputCache( paginationNumber, isSetPageTitleEnabled, isRssAutodiscoverEnabled, isQueryParameterFilteringEnabled, metaDescriptionAttributeValue, metaImageAttributeValue, outputCacheDuration ) )
            {
                var cachedOutput = GetCachedItem<string>( GetCacheKey( CacheKey.Output ) );

                if ( cachedOutput != null )
                {
                    return cachedOutput;
                }
            }

            var linkedPages = new Dictionary<string, object>
            {
                ["DetailPage"] = this.GetLinkedPageUrl( AttributeKey.DetailPage ),
                ["DetailPageRoute"] = new PageReference( GetAttributeValue( AttributeKey.DetailPage ) ).Route ?? string.Empty
            };

            var contentItemResults = GetContent( isQueryParameterFilteringEnabled, isTagListEnabled, isArchiveSummaryEnabled );
            var contentItemList = contentItemResults.Items ?? new List<ContentChannelItem>();
            var tags = contentItemResults.Tags ?? new List<TagModel>();
            var archiveSummaries = contentItemResults.ArchiveSummaries ?? new List<ArchiveSummaryModel>();

            var pagination = new Pagination
            {
                ItemCount = contentItemList.Count,
                PageSize = GetAttributeValue( AttributeKey.Count ).AsInteger(),
                CurrentPage = paginationNumber,
                UrlTemplate = this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.Page] = "PageNum"
                } )
            };

            var currentPageContent = pagination.GetCurrentPageItems( contentItemList );
            var commonMergeFields = RequestContext.GetCommonMergeFields();

            if ( isMergeContentEnabled )
            {
                var itemMergeFields = new Dictionary<string, object>( commonMergeFields );

                if ( RequestContext.CurrentPerson != null )
                {
                    itemMergeFields["Person"] = RequestContext.CurrentPerson;
                }

                var enabledCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

                foreach ( var item in currentPageContent )
                {
                    itemMergeFields["Item"] = item;
                    item.Content = item.Content.ResolveMergeFields( itemMergeFields, enabledCommands );

                    foreach ( var attributeValue in item.AttributeValues )
                    {
                        attributeValue.Value.Value = attributeValue.Value.Value.ResolveMergeFields( itemMergeFields, enabledCommands );
                    }
                }
            }

            var mergeFields = new Dictionary<string, object>( commonMergeFields )
            {
                ["Pagination"] = pagination,
                ["LinkedPages"] = linkedPages,
                ["Items"] = currentPageContent,
                ["ItemTagList"] = tags,
                ["ArchiveSummary"] = archiveSummaries,
                ["RockVersion"] = Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber(),
                ["CurrentPageUrl"] = this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.Tag] = "TagTemplate"
                } ),
                ["ArchiveSummaryPageUrl"] = this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.Year] = "YearTemplate",
                    [PageParameterKey.Month] = "MonthTemplate"
                } )
            };

            mergeFields.TryAdd( "Person", RequestContext.CurrentPerson );

            ApplyPageSettings( contentItemList, isSetPageTitleEnabled, isRssAutodiscoverEnabled, metaDescriptionAttributeValue, metaImageAttributeValue );

            var template = GetLavaTemplate();
            var lavaContext = LavaService.NewRenderContext( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ).SplitDelimitedValues() );
            var renderResult = LavaService.RenderTemplate( template, lavaContext );

            if ( renderResult.HasErrors )
            {
                throw renderResult.GetLavaException();
            }

            var outputContent = renderResult.Text;

            if ( CanUseOutputCache( paginationNumber, isSetPageTitleEnabled, isRssAutodiscoverEnabled, isQueryParameterFilteringEnabled, metaDescriptionAttributeValue, metaImageAttributeValue, outputCacheDuration ) )
            {
                SetCachedItem( GetCacheKey( CacheKey.Output ), outputContent, outputCacheDuration.Value );
            }

            return outputContent;
        }

        private void ApplyPageSettings( List<ContentChannelItem> contentItemList, bool isSetPageTitleEnabled, bool isRssAutodiscoverEnabled, string metaDescriptionAttributeValue, string metaImageAttributeValue )
        {
            if ( !contentItemList.Any() )
            {
                return;
            }

            if ( isSetPageTitleEnabled )
            {
                var siteName = PageCache?.Layout?.Site?.Name;
                var pageTitle = PageParameter( PageParameterKey.Item ).IsNullOrWhiteSpace()
                    ? contentItemList.Select( c => c.ContentChannel?.Name ).FirstOrDefault()
                    : contentItemList.Select( c => c.Title ).FirstOrDefault();

                if ( pageTitle.IsNotNullOrWhiteSpace() )
                {
                    ResponseContext.SetPageTitle( pageTitle );
                    ResponseContext.SetBrowserTitle( siteName.IsNotNullOrWhiteSpace() ? $"{pageTitle} | {siteName}" : pageTitle );
                }
            }

            if ( isRssAutodiscoverEnabled )
            {
                var contentChannelId = contentItemList.Select( c => c.ContentChannelId ).FirstOrDefault();
                var title = contentItemList.Select( c => c.ContentChannel?.Name ).FirstOrDefault() ?? "RSS Feed";
                var href = $"{RequestContext.RootUrlPath.EnsureTrailingForwardslash()}GetChannelFeed.ashx?ChannelId={contentChannelId}";

                ResponseContext.AddHtmlElement(
                    $"content-channel-rss-{BlockId}",
                    "link",
                    null,
                    new Dictionary<string, string>
                    {
                        ["type"] = "application/rss+xml",
                        ["rel"] = "alternate",
                        ["title"] = title,
                        ["href"] = href
                    },
                    Rock.Enums.Net.ResponseElementLocation.Header );
            }

            if ( metaDescriptionAttributeValue.IsNotNullOrWhiteSpace() )
            {
                var description = GetMetaValueFromAttribute( metaDescriptionAttributeValue, contentItemList );

                if ( description.IsNotNullOrWhiteSpace() )
                {
                    ResponseContext.AddMetaTag( "description", null, description.SanitizeHtml( true ) );
                }
            }

            if ( metaImageAttributeValue.IsNotNullOrWhiteSpace() )
            {
                var imageAttributeValue = GetMetaValueFromAttribute( metaImageAttributeValue, contentItemList );

                if ( imageAttributeValue.IsNotNullOrWhiteSpace() )
                {
                    var imageUrl = FileUrlHelper.GetImageUrl(
                        imageAttributeValue.AsGuid(),
                        new GetImageUrlOptions
                        {
                            PublicAppRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" )
                        } );

                    if ( imageUrl.IsNotNullOrWhiteSpace() )
                    {
                        ResponseContext.AddMetaTag( "og:image", null, imageUrl );
                        ResponseContext.AddHtmlElement(
                            $"content-channel-image-{BlockId}",
                            "link",
                            null,
                            new Dictionary<string, string>
                            {
                                ["rel"] = "image_src",
                                ["href"] = imageUrl
                            },
                            Rock.Enums.Net.ResponseElementLocation.Header );
                    }
                }
            }
        }

        private string GetMetaValueFromAttribute( string input, List<ContentChannelItem> content )
        {
            var inputParts = input.Split( '^' );
            var attributeEntityType = inputParts.Length > 0 ? inputParts[0] : "C";
            var attributeKey = inputParts.Length > 1 ? inputParts[1] : string.Empty;

            if ( attributeEntityType == "C" )
            {
                var contentChannel = content.FirstOrDefault()?.ContentChannel;

                if ( contentChannel != null )
                {
                    if ( contentChannel.AttributeValues == null )
                    {
                        contentChannel.LoadAttributes( RockContext );
                    }

                    return contentChannel.GetAttributeValue( attributeKey );
                }
            }
            else
            {
                var firstContentChannelItem = content.FirstOrDefault();

                if ( firstContentChannelItem != null )
                {
                    if ( firstContentChannelItem.AttributeValues == null )
                    {
                        firstContentChannelItem.LoadAttributes( RockContext );
                    }

                    return firstContentChannelItem.GetAttributeValue( attributeKey );
                }
            }

            return string.Empty;
        }

        private ILavaTemplate GetLavaTemplate()
        {
            var cacheDuration = GetAttributeValue( AttributeKey.CacheDuration ).AsIntegerOrNull();
            var cacheKey = GetCacheKey( CacheKey.Template );

            if ( cacheDuration.HasValue && cacheDuration.Value > 0 )
            {
                var cachedTemplate = GetCachedItem<ILavaTemplate>( cacheKey );
                if ( cachedTemplate != null )
                {
                    return cachedTemplate;
                }
            }

            try
            {
                var parseResult = LavaService.ParseTemplate( GetAttributeValue( AttributeKey.Template ) );

                if ( parseResult.HasErrors )
                {
                    throw parseResult.GetLavaException();
                }

                if ( cacheDuration.HasValue && cacheDuration.Value > 0 )
                {
                    SetCachedItem( cacheKey, parseResult.Template, cacheDuration.Value );
                }

                return parseResult.Template;
            }
            catch ( Exception ex )
            {
                var parseResult = LavaService.ParseTemplate( $"Lava error: {ex.Message}" );
                return parseResult.Template;
            }
        }

        private ItemContentResults GetContent( bool isQueryParameterFilteringEnabled, bool isTagListEnabled, bool isArchiveSummaryEnabled )
        {
            var cacheDuration = GetAttributeValue( AttributeKey.CacheDuration ).AsIntegerOrNull();
            var contentChannelGuid = GetAttributeValue( AttributeKey.Channel ).AsGuidOrNull();
            var contentChannel = contentChannelGuid.HasValue
                ? ContentChannelCache.Get( contentChannelGuid.Value )
                : null;

            List<ContentChannelItem> items = null;
            List<TagModel> tags = null;
            List<ArchiveSummaryModel> archiveSummaries = null;

            if ( cacheDuration.HasValue && cacheDuration.Value > 0 )
            {
                items = GetCachedItem<List<ContentChannelItem>>( GetCacheKey( CacheKey.Content ) );
                tags = GetCachedItem<List<TagModel>>( GetCacheKey( CacheKey.Tags ) );
                archiveSummaries = GetCachedItem<List<ArchiveSummaryModel>>( GetCacheKey( CacheKey.ArchiveSummary ) );
            }

            if ( !contentChannelGuid.HasValue || contentChannel == null )
            {
                return new ItemContentResults
                {
                    Items = items ?? new List<ContentChannelItem>(),
                    Tags = tags ?? new List<TagModel>(),
                    ArchiveSummaries = archiveSummaries ?? new List<ArchiveSummaryModel>()
                };
            }

            if ( items == null || ( isQueryParameterFilteringEnabled && HasPageParameters() ) || contentChannel.EnablePersonalization )
            {
                var contentChannelItemService = new ContentChannelItemService( RockContext );
                var itemId = PageParameter( PageParameterKey.Item ).AsIntegerOrNull();
                var dataFilterId = GetAttributeValue( AttributeKey.FilterId ).AsIntegerOrNull();
                var statuses = ( GetAttributeValue( AttributeKey.Status ) ?? "2" )
                    .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( s => s.ConvertToEnumOrNull<ContentChannelItemStatus>() )
                    .Where( s => s.HasValue )
                    .Select( s => s.Value )
                    .ToList();

                var contentChannelItemQuery = GetContentChannelItemQuery(
                    RockContext,
                    contentChannelItemService,
                    contentChannelGuid.Value,
                    itemId,
                    dataFilterId,
                    isQueryParameterFilteringEnabled,
                    statuses );

                if ( contentChannelItemQuery == null )
                {
                    return new ItemContentResults
                    {
                        Items = new List<ContentChannelItem>(),
                        Tags = new List<TagModel>(),
                        ArchiveSummaries = new List<ArchiveSummaryModel>()
                    };
                }

                if ( isTagListEnabled )
                {
                    var tagQuery = new TaggedItemService( RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Include( ti => ti.Tag );

                    tags = tagQuery
                        .Where( ti => contentChannelItemQuery.Any( cci => cci.Guid == ti.EntityGuid ) )
                        .GroupBy( ti => new { ti.Tag.Id, ti.Tag.Guid, ti.Tag.Name } )
                        .Select( group => new TagModel
                        {
                            Id = group.Key.Id,
                            Guid = group.Key.Guid,
                            Name = group.Key.Name,
                            Count = group.Count()
                        } )
                        .ToList();

                    var selectedTag = PageParameter( PageParameterKey.Tag );

                    if ( selectedTag.IsNotNullOrWhiteSpace() && !selectedTag.Equals( "all", StringComparison.OrdinalIgnoreCase ) )
                    {
                        contentChannelItemQuery = contentChannelItemQuery.Where( cci => tagQuery.Any( t => t.Tag.Name == selectedTag && t.EntityGuid == cci.Guid ) );
                    }
                }

                if ( isArchiveSummaryEnabled )
                {
                    archiveSummaries = contentChannelItemQuery
                        .GroupBy( cci => new
                        {
                            cci.StartDateTime.Month,
                            cci.StartDateTime.Year
                        } )
                        .Select( cci => new ArchiveSummaryModel
                        {
                            Month = cci.Key.Month,
                            Year = cci.Key.Year,
                            Count = cci.Count()
                        } )
                        .ToList();

                    archiveSummaries.ForEach( cci => cci.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName( cci.Month ) );

                    var selectedYear = PageParameter( PageParameterKey.Year ).AsIntegerOrNull();
                    var selectedMonth = PageParameter( PageParameterKey.Month ).AsIntegerOrNull();

                    if ( selectedYear.HasValue )
                    {
                        contentChannelItemQuery = contentChannelItemQuery.Where( cci => cci.StartDateTime.Year == selectedYear.Value );
                    }

                    if ( selectedMonth.HasValue )
                    {
                        contentChannelItemQuery = contentChannelItemQuery.Where( cci => cci.StartDateTime.Month == selectedMonth.Value );
                    }
                }

                IQueryable<ContentChannelItem> matchedQuery;
                IQueryable<ContentChannelItem> nonMatchedQuery = null;
                var hasNonMatchedQuery = false;

                if ( contentChannel.EnablePersonalization )
                {
                    var personalizationFilterType = GetAttributeValue( AttributeKey.Personalization )
                        .ConvertToEnum<PersonalizationFilterType>( PersonalizationFilterType.Ignore );
                    var personalizationSegmentIds = RequestContext.PersonalizationSegmentIds?.ToList() ?? new List<int>();
                    var requestFilterIds = RequestContext.PersonalizationRequestFilterIds?.ToList() ?? new List<int>();

                    if ( personalizationFilterType == PersonalizationFilterType.Ignore )
                    {
                        matchedQuery = contentChannelItemQuery;
                    }
                    else
                    {
                        var allPersonalizedSegmentEntityIdsQry = GetPersonalizedEntityIdsQry( RockContext, PersonalizationType.Segment );
                        var matchedSegmentEntityIdsQry = GetPersonalizedEntityIdsQry( RockContext, PersonalizationType.Segment, personalizationSegmentIds );
                        var allPersonalizedRequestFilterEntityIdsQry = GetPersonalizedEntityIdsQry( RockContext, PersonalizationType.RequestFilter );
                        var matchedRequestFilterEntityIdsQry = GetPersonalizedEntityIdsQry( RockContext, PersonalizationType.RequestFilter, requestFilterIds );

                        if ( personalizationFilterType == PersonalizationFilterType.Filter )
                        {
                            contentChannelItemQuery = contentChannelItemQuery
                                .Where( cci => !allPersonalizedSegmentEntityIdsQry.Contains( cci.Id ) || matchedSegmentEntityIdsQry.Contains( cci.Id ) )
                                .Where( cci => !allPersonalizedRequestFilterEntityIdsQry.Contains( cci.Id ) || matchedRequestFilterEntityIdsQry.Contains( cci.Id ) );

                            matchedQuery = contentChannelItemQuery;
                        }
                        else
                        {
                            hasNonMatchedQuery = true;

                            var matchedPredicate = LinqPredicateBuilder.False<ContentChannelItem>();
                            matchedPredicate = matchedPredicate.Or( cci => matchedSegmentEntityIdsQry.Contains( cci.Id ) && ( matchedRequestFilterEntityIdsQry.Contains( cci.Id ) || !allPersonalizedRequestFilterEntityIdsQry.Contains( cci.Id ) ) );
                            matchedPredicate = matchedPredicate.Or( cci => !allPersonalizedSegmentEntityIdsQry.Contains( cci.Id ) && matchedRequestFilterEntityIdsQry.Contains( cci.Id ) );

                            matchedQuery = contentChannelItemQuery.Where( matchedPredicate );
                            nonMatchedQuery = contentChannelItemQuery.Where( matchedPredicate.Not() );
                        }
                    }
                }
                else
                {
                    matchedQuery = contentChannelItemQuery;
                }

                items = GetContentChannelItems( RockContext, matchedQuery );

                if ( hasNonMatchedQuery )
                {
                    var nonMatchedItems = GetContentChannelItems( RockContext, nonMatchedQuery );
                    if ( nonMatchedItems.Any() )
                    {
                        items.AddRange( nonMatchedItems );
                    }
                }

                if ( cacheDuration.HasValue && cacheDuration.Value > 0 && !isQueryParameterFilteringEnabled && !contentChannel.EnablePersonalization )
                {
                    SetCachedItem( GetCacheKey( CacheKey.Content ), items, cacheDuration.Value );
                    SetCachedItem( GetCacheKey( CacheKey.Tags ), tags ?? new List<TagModel>(), cacheDuration.Value );
                    SetCachedItem( GetCacheKey( CacheKey.ArchiveSummary ), archiveSummaries ?? new List<ArchiveSummaryModel>(), cacheDuration.Value );
                }
            }

            return new ItemContentResults
            {
                Items = items ?? new List<ContentChannelItem>(),
                Tags = tags ?? new List<TagModel>(),
                ArchiveSummaries = archiveSummaries ?? new List<ArchiveSummaryModel>()
            };
        }

        private List<ContentChannelItem> GetContentChannelItems( RockContext rockContext, IQueryable<ContentChannelItem> contentChannelItemQuery )
        {
            var items = new List<ContentChannelItem>( contentChannelItemQuery.Count() );

            foreach ( var item in contentChannelItemQuery )
            {
                if ( item.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    item.LoadAttributes( rockContext );
                    items.Add( item );
                }
            }

            items = ApplyContextFilter( items );

            var orderBy = GetAttributeValue( AttributeKey.Order );
            if ( orderBy.IsNullOrWhiteSpace() )
            {
                return items;
            }

            var fieldDirection = new List<string>();
            foreach ( var itemPair in orderBy.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.Split( '^' ) ) )
            {
                if ( itemPair.Length == 2 && itemPair[0].IsNotNullOrWhiteSpace() )
                {
                    var sortDirection = itemPair[1];
                    fieldDirection.Add( itemPair[0] + ( sortDirection == "1" ? " desc" : string.Empty ) );
                }
            }

            var columns = fieldDirection.AsDelimited( "," )
                .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

            var itemQry = items.AsQueryable();
            IOrderedQueryable<ContentChannelItem> orderedQry = null;

            for ( var columnIndex = 0; columnIndex < columns.Length; columnIndex++ )
            {
                var column = columns[columnIndex].Trim();
                var isAscending = true;

                if ( column.EndsWith( " desc", StringComparison.OrdinalIgnoreCase ) )
                {
                    column = column.Left( column.Length - 5 );
                    isAscending = false;
                }

                if ( column.StartsWith( "Attribute:", StringComparison.OrdinalIgnoreCase ) )
                {
                    var attributeKey = column.Substring( 10 );

                    if ( isAscending == true )
                    {
                        orderedQry = columnIndex == 0
                            ? itemQry.OrderBy( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue )
                            : orderedQry.ThenBy( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue );
                    }
                    else
                    {
                        orderedQry = columnIndex == 0
                            ? itemQry.OrderByDescending( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue )
                            : orderedQry.ThenByDescending( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue );
                    }
                }
                else if ( isAscending )
                {
                    orderedQry = columnIndex == 0 ? itemQry.OrderBy( column ) : orderedQry.ThenBy( column );
                }
                else
                {
                    orderedQry = columnIndex == 0 ? itemQry.OrderByDescending( column ) : orderedQry.ThenByDescending( column );
                }
            }

            return orderedQry?.ToList() ?? items;
        }

        private List<ContentChannelItem> ApplyContextFilter( List<ContentChannelItem> items )
        {
            var contextFilterAttributeKey = GetAttributeValue( AttributeKey.ContextAttribute );
            if ( contextFilterAttributeKey.IsNullOrWhiteSpace() )
            {
                return items;
            }

            var contextEntityGuid = GetContextEntity()?.Guid;
            if ( !contextEntityGuid.HasValue )
            {
                return items;
            }

            return items.Where( i =>
                i.AttributeValues.Any( av =>
                {
                    if ( av.Key != contextFilterAttributeKey )
                    {
                        return false;
                    }

                    var guids = av.Value?.Value.SplitDelimitedValues().AsGuidList();
                    return guids?.Any( g => g == contextEntityGuid.Value ) == true;
                } ) ).ToList();
        }

        private IQueryable<int> GetPersonalizedEntityIdsQry( RockContext rockContext, PersonalizationType personalizationType, List<int> segmentIds )
        {
            var entityTypeId = EntityTypeCache.Get<ContentChannelItem>().Id;

            return rockContext.Set<PersonalizedEntity>()
                .Where( pe => pe.PersonalizationType == personalizationType
                    && pe.EntityTypeId == entityTypeId
                    && segmentIds.Contains( pe.PersonalizationEntityId ) )
                .Select( a => a.EntityId );
        }

        private IQueryable<int> GetPersonalizedEntityIdsQry( RockContext rockContext, PersonalizationType personalizationType )
        {
            var entityTypeId = EntityTypeCache.Get<ContentChannelItem>().Id;

            return rockContext.Set<PersonalizedEntity>()
                .Where( pe => pe.PersonalizationType == personalizationType
                    && pe.EntityTypeId == entityTypeId )
                .Select( a => a.EntityId );
        }

        private IQueryable<ContentChannelItem> GetContentChannelItemQuery(
            RockContext rockContext,
            ContentChannelItemService contentChannelItemService,
            Guid channelGuid,
            int? itemId,
            int? dataFilterId,
            bool isQueryParameterFilteringEnabled,
            List<ContentChannelItemStatus> statuses )
        {
            var contentChannelInfo = new ContentChannelService( rockContext )
                .GetSelect( channelGuid, s => new
                {
                    s.Id,
                    s.RequiresApproval,
                    ContentChannelTypeDisableStatus = s.ContentChannelType.DisableStatus
                } );

            if ( contentChannelInfo == null )
            {
                return null;
            }

            var contentChannelItemQuery = contentChannelItemService
                .Queryable()
                .Include( a => a.ContentChannel )
                .Include( a => a.ContentChannelType )
                .Include( a => a.ContentChannelItemSlugs )
                .Where( i => i.ContentChannelId == contentChannelInfo.Id );

            if ( isQueryParameterFilteringEnabled && itemId.HasValue )
            {
                contentChannelItemQuery = contentChannelItemQuery.Where( i => i.Id == itemId.Value );
            }

            if ( contentChannelInfo.RequiresApproval && !contentChannelInfo.ContentChannelTypeDisableStatus && statuses.Any() )
            {
                contentChannelItemQuery = contentChannelItemQuery.Where( i => statuses.Contains( i.Status ) );
            }

            var itemType = typeof( ContentChannelItem );
            var paramExpression = contentChannelItemService.ParameterExpression;

            try
            {
                if ( dataFilterId.HasValue )
                {
                    var dataFilter = new DataViewFilterService( rockContext )
                        .Queryable( "ChildFilters" )
                        .FirstOrDefault( a => a.Id == dataFilterId.Value );
                    var whereExpression = dataFilter != null
                        ? dataFilter.GetExpression( itemType, contentChannelItemService, paramExpression )
                        : null;

                    contentChannelItemQuery = contentChannelItemQuery.Where( paramExpression, whereExpression, null );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( $"Error while trying to filter a channel by DataFilterId. This is likely due to a broken DataFilter for the '{channelGuid}' channel for block {BlockId} on page {PageCache?.Id}.", ex ) );
            }

            if ( !isQueryParameterFilteringEnabled )
            {
                return contentChannelItemQuery;
            }

            var pageParameters = RequestContext.GetPageParameters();
            if ( pageParameters == null || pageParameters.Count == 0 )
            {
                return contentChannelItemQuery;
            }

            var propertyFilter = new Rock.Reporting.DataFilter.PropertyFilter();
            var queryParameterContentChannelItemQuery = contentChannelItemService
                .Queryable()
                .Where( a => contentChannelItemQuery.Any( b => b.Id == a.Id ) );

            foreach ( var fieldParameterKey in pageParameters.Select( p => p.Key ) )
            {
                Expression queryParameterFilteringExpression = null;
                var entityFieldList = Rock.Reporting.EntityHelper.FindFromFieldName( itemType, fieldParameterKey );

                foreach ( var entityField in entityFieldList )
                {
                    var selection = new List<string>
                    {
                        entityField.UniqueName
                    };

                    var supportedComparisonTypes = entityField.FieldType.Field.FilterComparisonType;
                    var defaultComparisonType = ComparisonType.EqualTo;

                    foreach ( ComparisonType comparisonType in typeof( ComparisonType ).GetOrderedValues<ComparisonType>() )
                    {
                        if ( ( supportedComparisonTypes & comparisonType ) == comparisonType )
                        {
                            defaultComparisonType = comparisonType;
                            break;
                        }
                    }

                    selection.Add( defaultComparisonType.ConvertToInt().ToString() );
                    selection.Add( PageParameter( fieldParameterKey ) );

                    var entityFieldExpression = propertyFilter.GetExpression(
                        itemType,
                        contentChannelItemService,
                        paramExpression,
                        Newtonsoft.Json.JsonConvert.SerializeObject( selection ) );

                    queryParameterFilteringExpression = queryParameterFilteringExpression == null
                        ? entityFieldExpression
                        : Expression.OrElse( queryParameterFilteringExpression, entityFieldExpression );
                }

                if ( queryParameterFilteringExpression != null )
                {
                    queryParameterContentChannelItemQuery = queryParameterContentChannelItemQuery.Where( paramExpression, queryParameterFilteringExpression );
                }
            }

            return queryParameterContentChannelItemQuery;
        }

        private string GetCacheKey( string key )
        {
            if ( BlockCache?.PageId != null )
            {
                return $"Rock:Page:{BlockCache.PageId.Value}:Block:{BlockId}:ItemCache:{key}";
            }

            return $"Rock:Layout:{BlockCache?.LayoutId ?? 0}:Block:{BlockId}:ItemCache:{key}";
        }

        private static T GetCachedItem<T>( string key ) where T : class
        {
            return RockCache.Get( key, true ) as T;
        }

        private void SetCachedItem( string key, object value, int cacheDurationInSeconds )
        {
            RockCache.AddOrUpdate( key, null, value, TimeSpan.FromSeconds( cacheDurationInSeconds ), GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty );
        }
        
        private void ClearCache( string key )
        {
            RockCache.Remove( key, null );
        }

        private static bool CanUseOutputCache( int paginationNumber, bool isSetPageTitleEnabled, bool isRssAutodiscoverEnabled, bool isQueryParameterFilteringEnabled, string metaDescriptionAttributeValue, string metaImageAttributeValue, int? outputCacheDuration )
        {
            return outputCacheDuration.HasValue
                && outputCacheDuration.Value > 0
                && paginationNumber == 1
                && !( isSetPageTitleEnabled
                    || isRssAutodiscoverEnabled
                    || isQueryParameterFilteringEnabled
                    || metaDescriptionAttributeValue.IsNotNullOrWhiteSpace()
                    || metaImageAttributeValue.IsNotNullOrWhiteSpace() );
        }

        private bool HasPageParameters()
        {
            return RequestContext?.GetPageParameters()?.Any() == true;
        }

        #endregion Private Methods

        #region Helper Types

        private class TagModel : LavaDataObject
        {
            public int Id { get; set; }

            public Guid Guid { get; set; }

            public string Name { get; set; }

            public int Count { get; set; }
        }

        private class ArchiveSummaryModel : LavaDataObject
        {
            public int Month { get; set; }

            public string MonthName { get; set; }

            public int Year { get; set; }

            public int Count { get; set; }
        }

        private class ItemContentResults
        {
            public List<ContentChannelItem> Items { get; set; }

            public List<TagModel> Tags { get; set; }

            public List<ArchiveSummaryModel> ArchiveSummaries { get; set; }
        }

        public class Pagination : LavaDataObject
        {
            public int ItemCount { get; set; }

            public int PageSize { get; set; }

            public int CurrentPage { get; set; }

            public int PreviousPage
            {
                get
                {
                    CurrentPage = CurrentPage > TotalPages ? TotalPages : CurrentPage;
                    return CurrentPage > 1 ? CurrentPage - 1 : -1;
                }
            }

            public int NextPage
            {
                get
                {
                    CurrentPage = CurrentPage > TotalPages ? TotalPages : CurrentPage;
                    return CurrentPage < TotalPages ? CurrentPage + 1 : -1;
                }
            }

            public int TotalPages
            {
                get
                {
                    if ( PageSize == 0 )
                    {
                        return 1;
                    }

                    return Convert.ToInt32( Math.Abs( ItemCount / PageSize ) ) + ( ( ItemCount % PageSize ) > 0 ? 1 : 0 );
                }
            }

            public string UrlTemplate { get; set; }

            public List<PaginationPage> Pages
            {
                get
                {
                    var pages = new List<PaginationPage>();

                    for ( var i = 1; i <= TotalPages; i++ )
                    {
                        pages.Add( new PaginationPage( UrlTemplate, i ) );
                    }

                    return pages;
                }
            }

            public List<ContentChannelItem> GetCurrentPageItems( List<ContentChannelItem> allItems )
            {
                if ( PageSize > 0 )
                {
                    CurrentPage = CurrentPage > TotalPages ? TotalPages : CurrentPage;
                    return allItems.Skip( ( CurrentPage - 1 ) * PageSize ).Take( PageSize ).ToList();
                }

                return allItems;
            }
        }

        public class PaginationPage : LavaDataObject
        {
            public PaginationPage( string urlTemplate, int pageNumber )
            {
                UrlTemplate = urlTemplate;
                PageNumber = pageNumber;
            }

            private string UrlTemplate { get; set; }

            public int PageNumber { get; }

            public string PageUrl
            {
                get
                {
                    if ( UrlTemplate.IsNullOrWhiteSpace() )
                    {
                        return PageNumber.ToString();
                    }

                    if ( UrlTemplate.Contains( "{0}" ) )
                    {
                        return string.Format( UrlTemplate, PageNumber );
                    }

                    return UrlTemplate.Replace( "PageNum", PageNumber.ToString() );
                }
            }
        }

        #endregion Helper Types
    }
}
