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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Blocks.Cms.ContentChannelNavigation;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a menu of content channels/items that the user is authorized to view.
    /// </summary>
    [DisplayName( "Content Channel Navigation" )]
    [Category( "CMS" )]
    [Description( "Block to display a menu of content channels/items that user is authorized to view." )]
    [IconCssClass( "ti ti-speakerphone" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

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

    [CategoryField(
        "Parent Category",
        Description = "The parent category to use as the root category available for the user to pick from.",
        IsRequired = false,
        EntityType = typeof( Rock.Model.ContentChannel ),
        Key = AttributeKey.ParentCategory,
        Order = 6 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "383CE8C7-888D-4E99-A07D-C9503381407F" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "55ACA1A1-E774-4C88-9425-50F79B680940" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.CONTENT_CHANNEL_NAVIGATION )]
    public class ContentChannelNavigation : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ContentChannelTypesInclude = "ContentChannelTypesInclude";
            public const string ContentChannelTypesExclude = "ContentChannelTypesExclude";
            public const string ContentChannelsFilter = "ContentChannelsFilter";
            public const string ShowCategoryFilter = "ShowCategoryFilter";
            public const string ParentCategory = "ParentCategory";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string ContentChannelId = "ContentChannelId";
            public const string ContentChannelGuid = "ContentChannelGuid";
            public const string CategoryGuid = "CategoryGuid";
        }

        private static class PersonPreferenceKey
        {
            public const string FilterGridSettings = "filter-grid-settings";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the grid filter settings for all channels, keyed by channel GUID.
        /// </summary>
        private Dictionary<string, GridFilterSettings> AllGridFilterSettings => GetBlockPersonPreferences()
            .GetValue( PersonPreferenceKey.FilterGridSettings )
            .FromJsonOrNull<Dictionary<string, GridFilterSettings>>()
            ?? new Dictionary<string, GridFilterSettings>();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<ContentChannelNavigationBag, ContentChannelNavigationOptionsBag>();
            var isCategoryFilterShown = GetAttributeValue( AttributeKey.ShowCategoryFilter ).AsBoolean();

            // Resolve selected category from page parameter.
            Guid? selectedCategoryGuid = PageParameter( PageParameterKey.CategoryGuid ).AsGuidOrNull();

            // Resolve selected channel from page parameters only.
            // If no page parameter, the client reads from person preferences.
            Guid? selectedChannelGuid = PageParameter( PageParameterKey.ContentChannelGuid ).AsGuidOrNull();

            if ( !selectedChannelGuid.HasValue )
            {
                var channelId = PageParameter( PageParameterKey.ContentChannelId ).AsIntegerOrNull();
                if ( channelId.HasValue )
                {
                    selectedChannelGuid = ContentChannelCache.Get( channelId.Value )?.Guid;
                }
            }

            box.Bag = new ContentChannelNavigationBag
            {
                SelectedChannelGuid = selectedChannelGuid,
                SelectedCategoryGuid = selectedCategoryGuid
            };

            var categories = isCategoryFilterShown ? GetCategoryItems() : new List<ListItemBag>();

            box.Options = new ContentChannelNavigationOptionsBag
            {
                IsCategoryFilterShown = isCategoryFilterShown && categories.Any(),
                CategoryItems = categories
            };

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Gets the navigation URLs for the block.
        /// </summary>
        /// <returns>A dictionary of navigation URL keys and values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    ["ContentItemId"] = "((Key))"
                } )
            };
        }

        /// <summary>
        /// Gets the list of category items for the category dropdown.
        /// </summary>
        /// <returns>A list of category items as ListItemBag.</returns>
        private List<ListItemBag> GetCategoryItems()
        {
            var parentCategoryGuid = GetAttributeValue( AttributeKey.ParentCategory ).AsGuidOrNull();

            var categories = new CategoryService( RockContext )
                .GetByEntityTypeId( EntityTypeCache.Get( typeof( Rock.Model.ContentChannel ) )?.Id )
                .Where( c => ( !parentCategoryGuid.HasValue && !c.ParentCategoryId.HasValue ) ||
                             ( parentCategoryGuid.HasValue && c.ParentCategory != null && c.ParentCategory.Guid == parentCategoryGuid ) )
                .OrderBy( c => c.Name )
                .Select( c => new ListItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name
                } )
                .ToList();

            return categories;
        }

        /// <summary>
        /// Gets the content channel bags with pending counts, optionally filtered by category.
        /// </summary>
        /// <param name="categoryId">The optional category ID to filter by.</param>
        /// <returns>A list of channel bags.</returns>
        private List<ContentChannelNavigationChannelBag> GetChannelBags( int? categoryId )
        {
            var contentChannelsQry = new ContentChannelService( RockContext ).Queryable()
                .Include( a => a.ContentChannelType )
                .AsNoTracking();

            // Apply channel/type filters based on block settings.
            var contentChannelGuidsFilter = GetAttributeValue( AttributeKey.ContentChannelsFilter ).SplitDelimitedValues().AsGuidList();
            var contentChannelTypeGuidsInclude = GetAttributeValue( AttributeKey.ContentChannelTypesInclude ).SplitDelimitedValues().AsGuidList();
            var contentChannelTypeGuidsExclude = GetAttributeValue( AttributeKey.ContentChannelTypesExclude ).SplitDelimitedValues().AsGuidList();

            if ( contentChannelGuidsFilter.Any() )
            {
                // ContentChannelsFilter takes precedence over Include/Exclude.
                contentChannelsQry = contentChannelsQry.Where( a => contentChannelGuidsFilter.Contains( a.Guid ) );
            }
            else if ( contentChannelTypeGuidsInclude.Any() )
            {
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

            // Apply category filter if enabled and a category is selected.
            if ( GetAttributeValue( AttributeKey.ShowCategoryFilter ).AsBoolean() )
            {
                var parentCategoryGuid = GetAttributeValue( AttributeKey.ParentCategory ).AsGuidOrNull();

                if ( categoryId.HasValue )
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

            // Filter by VIEW authorization.
            var authorizedChannels = contentChannelsList
                .Where( c => c.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();

            var authorizedChannelIds = authorizedChannels.Select( c => c.Id ).ToList();

            // Get pending approval item counts for channels that require approval.
            var pendingCounts = new ContentChannelItemService( RockContext ).Queryable()
                .Where( i =>
                    authorizedChannelIds.Contains( i.ContentChannelId ) &&
                    i.Status == ContentChannelItemStatus.PendingApproval &&
                    i.ContentChannel.RequiresApproval )
                .GroupBy( i => i.ContentChannelId )
                .Select( g => new
                {
                    Id = g.Key,
                    Count = g.Count()
                } )
                .ToDictionary( x => x.Id, x => x.Count );

            return authorizedChannels.Select( c =>
            {
                pendingCounts.TryGetValue( c.Id, out var count );

                return new ContentChannelNavigationChannelBag
                {
                    Guid = c.Guid,
                    Name = c.Name,
                    IconCssClass = c.IconCssClass,
                    PendingCount = count
                };
            } ).ToList();
        }

        /// <summary>
        /// Gets the grid attributes for the specified content channel.
        /// These are attributes marked as IsGridColumn qualified by ContentChannelTypeId or ContentChannelId.
        /// </summary>
        /// <param name="channel">The content channel.</param>
        /// <returns>A list of attribute caches for grid display.</returns>
        private List<AttributeCache> GetGridAttributes( ContentChannel channel )
        {
            int entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannelItem ) ).Id;

            return AttributeCache.All().AsQueryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn && ( (
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( channel.ContentChannelTypeId.ToString() )
                    ) || (
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( channel.Id.ToString() )
                    ) ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Builds the grid builder for the specified content channel.
        /// Columns are dynamic based on the channel's type configuration.
        /// </summary>
        /// <param name="channel">The content channel.</param>
        /// <param name="gridAttributes">The attribute columns to include.</param>
        /// <param name="itemTags">The tag HTML lookup for items, or null if tagging is disabled.</param>
        /// <returns>A configured GridBuilder for ContentChannelItem.</returns>
        private GridBuilder<ContentChannelItem> GetGridBuilder( ContentChannel channel, List<AttributeCache> gridAttributes, Dictionary<Guid, string> itemTags )
        {
            var builder = new GridBuilder<ContentChannelItem>()
                .WithBlock( this )
                .AddTextField( "guid", a => a.Guid.ToString() )
                .AddTextField( "title", a => a.Title );

            // Add attribute columns.
            builder.AddAttributeFields( gridAttributes );

            // Add date columns based on channel type configuration.
            if ( channel.ContentChannelType.DateRangeType != ContentChannelDateType.NoDates )
            {
                builder.AddDateTimeField( "startDateTime", a => a.StartDateTime );
                builder.AddField( "isScheduled", a => a.StartDateTime > RockDateTime.Now );

                if ( channel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange )
                {
                    builder.AddDateTimeField( "expireDateTime", a => a.ExpireDateTime );
                }
            }

            // Add priority if not disabled.
            if ( !channel.ContentChannelType.DisablePriority )
            {
                builder.AddField( "priority", a => a.Priority );
            }

            // Add status if channel requires approval.
            if ( channel.RequiresApproval )
            {
                builder.AddField( "status", a => a.Status );
            }

            // Add event occurrences indicator.
            builder.AddField( "occurrences", a => a.EventItemOccurrences.Any() );

            // Add created by person name.
            builder.AddPersonField( "createdByPerson", a => a.CreatedByPersonAlias?.Person );

            // Add security disabled flag for the SecurityColumn.
            builder.AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );

            // Add tags if tagging is enabled for this channel.
            if ( channel.IsTaggingEnabled && itemTags != null )
            {
                builder.AddTextField( "tags", a =>
                {
                    itemTags.TryGetValue( a.Guid, out var tags );
                    return tags;
                } );
            }

            return builder;
        }

        /// <summary>
        /// Gets the content channel items for the specified channel, applying
        /// person preference filters, attribute filters, and VIEW authorization.
        /// </summary>
        /// <param name="channel">The content channel.</param>
        /// <param name="gridAttributes">The grid attributes used for attribute filtering.</param>
        /// <param name="hasActiveFilters">Set to true if any user-applied filters are active after value conversion.</param>
        /// <param name="isReorderEnabled">Set to true if items are in manual order and reordering is available.</param>
        /// <returns>The filtered and authorized list of items.</returns>
        private List<ContentChannelItem> GetFilteredItems( ContentChannel channel, List<AttributeCache> gridAttributes, out bool hasActiveFilters, out bool isReorderEnabled )
        {
            hasActiveFilters = false;
            isReorderEnabled = false;

            var contentChannelItemService = new ContentChannelItemService( RockContext );
            var queryable = contentChannelItemService.Queryable()
                .Include( i => i.EventItemOccurrences )
                .Include( i => i.CreatedByPersonAlias.Person )
                .Where( i => i.ContentChannelId == channel.Id );

            // Read grid filter settings for this channel from person preferences.
            AllGridFilterSettings.TryGetValue( channel.Guid.ToString(), out var filters );

            if ( filters != null )
            {
                // Apply date range filter.
                var lowerDate = filters.DateRangeLower.AsDateTime();
                var upperDate = filters.DateRangeUpper.AsDateTime();

                if ( lowerDate.HasValue )
                {
                    hasActiveFilters = true;
                    if ( channel.ContentChannelType.DateRangeType == ContentChannelDateType.SingleDate )
                    {
                        queryable = queryable.Where( i => i.StartDateTime >= lowerDate.Value );
                    }
                    else
                    {
                        queryable = queryable.Where( i => i.ExpireDateTime.HasValue && i.ExpireDateTime.Value >= lowerDate.Value );
                    }
                }

                if ( upperDate.HasValue )
                {
                    hasActiveFilters = true;
                    var upperDateEnd = upperDate.Value.Date.AddDays( 1 );
                    queryable = queryable.Where( i => i.StartDateTime <= upperDateEnd );
                }

                // Apply status filter.
                var status = filters.Status.ConvertToEnumOrNull<ContentChannelItemStatus>();
                if ( status.HasValue )
                {
                    hasActiveFilters = true;
                    queryable = queryable.Where( i => i.Status == status );
                }

                // Apply title filter.
                if ( filters.Title.IsNotNullOrWhiteSpace() )
                {
                    hasActiveFilters = true;
                    queryable = queryable.Where( i => i.Title.Contains( filters.Title ) );
                }

                // Apply created by filter.
                // The PersonPicker stores the person's primary alias GUID.
                var createdByAliasGuid = filters.CreatedBy?.Value.AsGuidOrNull();
                if ( createdByAliasGuid.HasValue )
                {
                    var personAlias = new PersonAliasService( RockContext ).Get( createdByAliasGuid.Value );
                    if ( personAlias != null )
                    {
                        hasActiveFilters = true;
                        queryable = queryable.Where( i => i.CreatedByPersonAlias.PersonId == personAlias.PersonId );
                    }
                }
            }

            /*
                4/10/26 - MSE

                Attribute filters are applied at the IQueryable level so
                filtering happens in SQL. Each filter's ComparisonValue
                (comparison type + value) comes from the Obsidian
                RockAttributeFilter component in SimpleFilter mode.

                Because Obsidian components emit values in a public format
                (JSON objects, GUIDs, etc.), we convert each value to its
                internal database representation via GetPrivateValue before
                passing it to GetAttributeExpression. The standard filter
                component may also omit a ComparisonType for certain field
                types, so we fall back to the field type's own default.

                Reason: Public-to-private value conversion for SQL-level
                attribute filtering.
            */
            if ( gridAttributes.Any() )
            {
                var attributeFilterValues = filters?.AttributeFilterValues ?? new Dictionary<string, ComparisonValue>();

                foreach ( var attribute in gridAttributes )
                {
                    if ( !attributeFilterValues.TryGetValue( attribute.Key, out var filterEntry ) )
                    {
                        continue;
                    }

                    var rawValue = filterEntry.Value?.Trim();
                    if ( rawValue == "null" )
                    {
                        rawValue = string.Empty;
                    }

                    var filterValue = rawValue.IsNotNullOrWhiteSpace()
                        ? PublicAttributeHelper.GetPrivateValue( attribute, rawValue )
                        : rawValue;

                    // Skip entries with no value unless the comparison type is
                    // IsBlank or IsNotBlank, which are valid without a value.
                    var isBlankComparison = filterEntry.ComparisonType.HasValue
                        && ( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( filterEntry.ComparisonType.Value );

                    if ( !isBlankComparison && filterValue.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    var entityField = EntityHelper.GetEntityFieldForAttribute( attribute, false );
                    if ( entityField == null )
                    {
                        continue;
                    }

                    // Determine the comparison type. If the client did not
                    // provide one, fall back to the field type's default.
                    var comparisonType = filterEntry.ComparisonType;
                    if ( !comparisonType.HasValue && filterValue.IsNotNullOrWhiteSpace() )
                    {
                        var supportedTypes = entityField.FieldType.Field.FilterComparisonType;
                        comparisonType = supportedTypes.HasFlag( ComparisonType.Contains )
                            ? ComparisonType.Contains
                            : ComparisonType.EqualTo;
                    }

                    var filterArgs = new List<string>();
                    if ( comparisonType.HasValue )
                    {
                        filterArgs.Add( comparisonType.ConvertToInt().ToString() );
                    }

                    filterArgs.Add( filterValue );

                    var parameterExpression = contentChannelItemService.ParameterExpression;
                    var attributeExpression = ExpressionHelper.GetAttributeExpression( contentChannelItemService, parameterExpression, entityField, filterArgs );

                    if ( attributeExpression is NoAttributeFilterExpression )
                    {
                        continue;
                    }

                    hasActiveFilters = true;
                    queryable = queryable.Where( parameterExpression, attributeExpression );
                }
            }

            // Materialize items and apply VIEW authorization.
            var items = queryable.ToList();
            var authorizedItems = new List<ContentChannelItem>();
            var hasUnauthorizedItems = false;

            foreach ( var item in items )
            {
                if ( item.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    authorizedItems.Add( item );
                }
                else
                {
                    hasUnauthorizedItems = true;
                }
            }

            // Load attributes for grid display.
            if ( gridAttributes.Any() )
            {
                var gridAttributeIds = gridAttributes.Select( a => a.Id ).ToList();
                Helper.LoadFilteredAttributes( typeof( ContentChannelItem ), authorizedItems.Cast<IHasAttributes>().ToList(), RockContext, a => gridAttributeIds.Contains( a.Id ) );
            }

            // Sort: manual order when unfiltered and all items are visible,
            // otherwise by start date descending.
            isReorderEnabled = channel.ItemsManuallyOrdered && !hasActiveFilters && !hasUnauthorizedItems;

            if ( isReorderEnabled )
            {
                return authorizedItems.OrderBy( i => i.Order ).ToList();
            }
            else
            {
                return authorizedItems.OrderByDescending( p => p.StartDateTime ).ToList();
            }
        }

        /// <summary>
        /// Gets the tag HTML for items in a channel that has tagging enabled.
        /// Checks VIEW authorization on each tag.
        /// </summary>
        /// <param name="items">The content channel items.</param>
        /// <returns>A dictionary of item GUIDs to tag HTML strings.</returns>
        private Dictionary<Guid, string> GetItemTags( List<ContentChannelItem> items )
        {
            var itemTags = items.ToDictionary( i => i.Guid, v => "" );
            var entityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid() ).Id;
            var testedTags = new Dictionary<int, string>();

            foreach ( var taggedItem in new TaggedItemService( RockContext ).Queryable()
                .Include( i => i.Tag )
                .AsNoTracking()
                .Where( i =>
                    i.EntityTypeId == entityTypeId &&
                    itemTags.Keys.Contains( i.EntityGuid ) )
                .OrderBy( i => i.Tag.Name ) )
            {
                if ( !testedTags.ContainsKey( taggedItem.TagId ) )
                {
                    testedTags.Add( taggedItem.TagId, taggedItem.Tag.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) ? taggedItem.Tag.Name : string.Empty );
                }

                if ( testedTags[taggedItem.TagId].IsNotNullOrWhiteSpace() )
                {
                    itemTags[taggedItem.EntityGuid] += string.Format( "<span class='tag'>{0}</span>", testedTags[taggedItem.TagId].SanitizeHtml( strict: false ) );
                }
            }

            return itemTags;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the updated channel list, called when category or pending toggle changes.
        /// </summary>
        /// <param name="categoryValue">The selected category ID value, or null for all.</param>
        /// <param name="showPendingOnly">If true, only return channels with pending items.</param>
        /// <returns>The updated list of channel bags.</returns>
        [BlockAction]
        public BlockActionResult GetChannels( string categoryValue, bool showPendingOnly )
        {
            // categoryValue is a category GUID string.
            int? categoryId = null;
            var categoryGuid = categoryValue.AsGuidOrNull();
            if ( categoryGuid.HasValue )
            {
                categoryId = CategoryCache.Get( categoryGuid.Value )?.Id;
            }

            var channels = GetChannelBags( categoryId );

            if ( showPendingOnly )
            {
                channels = channels.Where( c => c.PendingCount > 0 ).ToList();
            }

            return ActionOk( channels );
        }

        /// <summary>
        /// Gets the grid data for the selected content channel.
        /// Filter values are read from person preferences.
        /// </summary>
        /// <param name="request">The request containing the channel Guid.</param>
        /// <returns>The grid data response including definition, data, and metadata.</returns>
        [BlockAction]
        public BlockActionResult GetGridData( ContentChannelNavigationGetGridDataRequestBag request )
        {
            if ( request == null || !request.ChannelGuid.HasValue )
            {
                return ActionBadRequest( "A channel must be selected." );
            }

            var channel = new ContentChannelService( RockContext ).Queryable()
                .Include( c => c.ContentChannelType )
                .FirstOrDefault( c => c.Guid == request.ChannelGuid.Value );

            if ( channel == null )
            {
                return ActionNotFound( "The selected content channel was not found." );
            }

            if ( !channel.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionUnauthorized( "You are not authorized to view this content channel." );
            }

            // Get grid attributes and filtered items.
            var gridAttributes = GetGridAttributes( channel );

            bool hasActiveFilters;
            bool isReorderEnabled;
            var items = GetFilteredItems( channel, gridAttributes, out hasActiveFilters, out isReorderEnabled );

            // Build tags lookup if tagging is enabled.
            var itemTags = channel.IsTaggingEnabled ? GetItemTags( items ) : null;

            // Build grid.
            var builder = GetGridBuilder( channel, gridAttributes, itemTags );
            var gridData = builder.Build( items );
            var gridDefinition = builder.BuildDefinition();

            // Determine edit authorization.
            var canEdit = channel.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            // Build add item URL with channel context.
            var addItemUrl = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
            {
                ["ContentItemId"] = "0",
                ["ContentChannelId"] = channel.IdKey
            } );

            // Build attribute filter definitions for the grid settings modal.
            var attributeFilters = gridAttributes
                .Select( a => PublicAttributeHelper.GetPublicAttributeForEdit( a ) )
                .ToList();

            return ActionOk( new ContentChannelNavigationGetGridDataResponseBag
            {
                GridData = gridData,
                GridDefinition = gridDefinition,
                ChannelName = channel.Name,
                IsReorderEnabled = isReorderEnabled,
                CanEdit = canEdit,
                AddItemUrl = addItemUrl,
                AttributeFilters = attributeFilters,
                IsIncludeTime = channel.ContentChannelType.IncludeTime,
                HasScheduledItems = items.Any( i => i.StartDateTime > RockDateTime.Now ),
                HasEventOccurrences = items.Any( i => i.EventItemOccurrences.Any() ),
                HasActiveFilters = hasActiveFilters
            } );
        }

        /// <summary>
        /// Deletes a content channel item.
        /// </summary>
        /// <param name="guid">The Guid of the item to delete.</param>
        /// <returns>The result of the delete operation.</returns>
        [BlockAction]
        public BlockActionResult DeleteItem( Guid guid )
        {
            var contentItemService = new ContentChannelItemService( RockContext );
            var contentItem = contentItemService.Get( guid );

            if ( contentItem == null )
            {
                return ActionNotFound( "The content channel item was not found." );
            }

            if ( !contentItem.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionUnauthorized( "You are not authorized to delete this item." );
            }

            string errorMessage;
            if ( !contentItemService.CanDelete( contentItem, out errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            /*
                3/30/26 - MSE

                The ContentChannelItem SaveHook handles cascade deletion of
                associated records (ChildItems, ParentItems, and Slugs) when
                the entity state is Deleted. The WebForms version manually
                deleted these associations, but that is redundant with the
                SaveHook behavior.

                Reason: Rely on framework SaveHook for cascade cleanup.
            */
            contentItemService.Delete( contentItem );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Reorders content channel items.
        /// </summary>
        /// <param name="channelGuid">The Guid of the content channel.</param>
        /// <param name="key">The identifier of the item being moved.</param>
        /// <param name="beforeKey">The identifier of the item it should be placed before, or null for the end.</param>
        /// <returns>The result of the reorder operation.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( Guid channelGuid, string key, string beforeKey )
        {
            var channel = new ContentChannelService( RockContext ).Get( channelGuid );

            if ( channel == null || !channel.ItemsManuallyOrdered )
            {
                return ActionBadRequest( "Reordering is not available for this channel." );
            }

            if ( !channel.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionUnauthorized( "You are not authorized to edit this content channel." );
            }

            var items = new ContentChannelItemService( RockContext ).Queryable()
                .Where( i => i.ContentChannelId == channel.Id )
                .OrderBy( i => i.Order )
                .ToList();

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Creates an entity set for the subset of selected rows in the grid.
        /// </summary>
        /// <param name="entitySet">The entity set data from the grid.</param>
        /// <returns>An action result that contains the identifier of the entity set.</returns>
        [BlockAction]
        public BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        #endregion Block Actions

        #region Support Classes

        /// <summary>
        /// The grid filter settings for a single content channel, stored as
        /// part of the PersonPreferenceKey.FilterGridSettings JSON object.
        /// </summary>
        private class GridFilterSettings
        {
            public string DateRangeLower { get; set; }

            public string DateRangeUpper { get; set; }

            public string Status { get; set; }

            public string Title { get; set; }

            public ListItemBag CreatedBy { get; set; }

            public Dictionary<string, ComparisonValue> AttributeFilterValues { get; set; }
        }

        #endregion Support Classes
    }
}
