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
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.CategoryTreeView;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a tree of categories for the configured entity type. Selecting a node navigates to
    /// the configured Detail Page, or reloads the current page, with the selection and expanded nodes
    /// on the query string so sibling blocks read them as page parameters.
    /// </summary>
    [DisplayName( "Category Tree View" )]
    [Category( "Core" )]
    [Description( "Displays a tree of categories for the configured entity type." )]
    [IconCssClass( "ti ti-list-tree" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "The page to navigate to when a category or item is selected.",
        IsRequired = false,
        Key = AttributeKey.DetailPage )]

    [EntityTypeField( "Entity Type",
        Description = "Display categories associated with this type of entity.",
        Key = AttributeKey.EntityType )]

    [TextField( "Entity Type Friendly Name",
        Description = "The text to show for the entity type name. Leave blank to get it from the specified Entity Type.",
        IsRequired = false,
        Key = AttributeKey.EntityTypeFriendlyName )]

    [TextField( "Entity Type Qualifier Property",
        IsRequired = false,
        Key = AttributeKey.EntityTypeQualifierProperty )]

    [TextField( "Entity Type Qualifier Value",
        IsRequired = false,
        Key = AttributeKey.EntityTypeQualifierValue )]

    [BooleanField( "Show Unnamed Entity Items",
        Description = "Set to false to hide any entity items that have a blank name.",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowUnnamedEntityItems )]

    [TextField( "Page Parameter Key",
        Description = "The page parameter to use for determining the currently selected entity whose category is selected. If not present, the currently selected category node is used.",
        IsRequired = false,
        Key = AttributeKey.PageParameterKey )]

    [TextField( "Default Icon CSS Class",
        Description = "The icon CSS class to use for items that do not have an icon of their own.",
        IsRequired = false,
        DefaultValue = "ti ti-list-numbers",
        Key = AttributeKey.DefaultIconCssClass )]

    [CategoryField( "Root Category",
        Description = "Select the root category to use as a starting point for the tree view.",
        AllowMultiple = false,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.RootCategory )]

    [CategoryField( "Exclude Categories",
        Description = "Select any category that should be excluded from the tree view.",
        AllowMultiple = true,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.ExcludeCategories )]

    [LinkedPage( "Search Results Page",
        Description = "The page to display search results on.",
        IsRequired = false,
        Key = AttributeKey.SearchResultsPage )]

    [BooleanField( "Show Only Categories",
        Description = "Set to true to show only the categories rather than the categorized entities for the configured entity type.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowOnlyCategories )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Navigation )]
    [Rock.SystemGuid.EntityTypeGuid( "D5E0A7C3-6B41-4F28-9A1E-7C3B5D2F8E60" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "8C2F4A6D-1B9E-4C73-A5F8-2D6B9E4A1C30" )]
    [Rock.SystemGuid.BlockTypeGuid( "ADE003C7-649B-466A-872B-B8AC952E7841" )]
    public class CategoryTreeView : RockBlockType, IHasCustomActions
    {
        #region Keys

        private const string DefaultPageParameterKey = "CategoryId";

        /*
            06/11/26 - JMH

            Two attribute keys keep their long-standing WebForms spellings on purpose:
            EntitytypeQualifierValue (lowercase "type") and DefaultIconCSSClass (uppercase
            CSS). This block adopts the WebForms block type Guid, so existing placements
            already store their values under these exact keys; matching them preserves the
            saved configuration. "Correcting" the spelling would orphan those values.

            Reason: Preserve existing block settings across the WebForms-to-Obsidian chop.
        */
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string EntityType = "EntityType";
            public const string EntityTypeFriendlyName = "EntityTypeFriendlyName";
            public const string EntityTypeQualifierProperty = "EntityTypeQualifierProperty";
            public const string EntityTypeQualifierValue = "EntitytypeQualifierValue";
            public const string ShowUnnamedEntityItems = "ShowUnnamedEntityItems";
            public const string PageParameterKey = "PageParameterKey";
            public const string DefaultIconCssClass = "DefaultIconCSSClass";
            public const string RootCategory = "RootCategory";
            public const string ExcludeCategories = "ExcludeCategories";
            public const string SearchResultsPage = "SearchResultsPage";
            public const string ShowOnlyCategories = "ShowOnlyCategories";
        }

        private static class PageParameterKey
        {
            public const string CategoryId = "CategoryId";
            public const string ExpandedIds = "ExpandedIds";
            public const string ParentCategoryId = "ParentCategoryId";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string SearchResultsPage = "SearchResultsPage";
        }

        private static class PersonPreferenceKey
        {
            public const string HideInactiveItems = "hide-inactive-items";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<CategoryTreeViewBag, CategoryTreeViewOptionsBag>
            {
                Options = GetBoxOptions(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            box.Bag = GetBoxBag();

            return box;
        }

        /// <summary>
        /// Builds the block's configured settings for the client.
        /// </summary>
        /// <returns>The options bag describing how the tree should be displayed and scoped.</returns>
        private CategoryTreeViewOptionsBag GetBoxOptions()
        {
            return new CategoryTreeViewOptionsBag
            {
                BlockProperties = new CategoryTreeViewBlockAttributesBag
                {
                    EntityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull(),
                    EntityTypeFriendlyName = GetAttributeValue( AttributeKey.EntityTypeFriendlyName ),
                    RootCategoryGuid = GetAttributeValue( AttributeKey.RootCategory ).AsGuidOrNull(),
                    EntityTypeQualifierColumn = GetAttributeValue( AttributeKey.EntityTypeQualifierProperty ),
                    EntityTypeQualifierValue = GetAttributeValue( AttributeKey.EntityTypeQualifierValue ),
                    PageParameterKey = GetAttributeValue( AttributeKey.PageParameterKey ).IfEmpty( DefaultPageParameterKey ),
                    ShowOnlyCategories = GetAttributeValue( AttributeKey.ShowOnlyCategories ).AsBoolean(),
                    ShowUnnamedEntityItems = GetAttributeValue( AttributeKey.ShowUnnamedEntityItems ).AsBoolean( true ),
                    DefaultIconCssClass = GetAttributeValue( AttributeKey.DefaultIconCssClass ),
                    ExcludeCategoryGuids = GetAttributeValue( AttributeKey.ExcludeCategories )
                        .SplitDelimitedValues()
                        .Select( v => v.AsGuidOrNull() )
                        .Where( g => g.HasValue )
                        .Select( g => g.Value )
                        .ToList(),
                    PanelTitle = ResolvePanelTitle()
                }
            };
        }

        /// <summary>
        /// Resolves the panel title from the configured entity type friendly name, falling back to
        /// the entity type's own friendly name.
        /// </summary>
        /// <returns>The title for the tree panel, or null when no entity type is configured.</returns>
        private string ResolvePanelTitle()
        {
            var friendlyName = GetAttributeValue( AttributeKey.EntityTypeFriendlyName );
            if ( friendlyName.IsNotNullOrWhiteSpace() )
            {
                return friendlyName;
            }

            var entityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            return entityTypeGuid.HasValue ? EntityTypeCache.Get( entityTypeGuid.Value )?.FriendlyName : null;
        }

        /// <summary>
        /// Builds the navigation URLs for the configured linked pages.
        /// </summary>
        /// <returns>A map of navigation key to URL; an entry is blank when its page is not configured.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage ),
                [NavigationUrlKey.SearchResultsPage] = this.GetLinkedPageUrl( AttributeKey.SearchResultsPage )
            };
        }

        /// <summary>
        /// Builds the runtime data for the client: the categories to select and expand on load, the
        /// edit affordance flag, and the active-filter state.
        /// </summary>
        /// <returns>The populated runtime bag.</returns>
        private CategoryTreeViewBag GetBoxBag()
        {
            var bag = new CategoryTreeViewBag
            {
                SelectedCategoryGuids = new List<Guid>(),
                ExpandedCategoryGuids = new List<Guid>()
            };

            // The add affordances are shown only to people with EDIT on the block.
            bag.CanEdit = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            // The active/inactive filter applies only to entity types that track an active flag.
            var entityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            var modelType = entityTypeGuid.HasValue ? EntityTypeCache.Get( entityTypeGuid.Value )?.GetEntityType() : null;
            bag.IsActiveFilterVisible = modelType != null && typeof( IHasActiveFlag ).IsAssignableFrom( modelType );
            bag.HideInactiveItems = GetBlockPersonPreferences().GetValue( PersonPreferenceKey.HideInactiveItems ).AsBoolean();

            // Deep-link: resolve the selected and expanded categories from the page parameters.
            // CategoryCache.Get accepts an integer Id, an IdKey, or a Guid, so the block restores
            // selection whether the URL carries a WebForms-style integer Id, an IdKey, or a Guid.
            var allowIntegerId = !PageCache.Layout.Site.DisablePredictableIds;
            var pageParameterKey = GetAttributeValue( AttributeKey.PageParameterKey ).IfEmpty( DefaultPageParameterKey );

            // WebForms reveals the add affordances when the person holds EDIT on the selected
            // category even without block EDIT. Track the selected category (or, in entity-node
            // mode, the entity's parent category) so that elevation can be applied below.
            CategoryCache selectedCategoryForAuth = null;

            // A selected category arrives in the CategoryId parameter, which is where navigate mode
            // writes category selections.
            var selectedCategoryValue = RequestContext.GetPageParameter( PageParameterKey.CategoryId );
            if ( selectedCategoryValue.IsNotNullOrWhiteSpace() )
            {
                var selectedCategory = CategoryCache.Get( selectedCategoryValue, allowIntegerId );
                if ( selectedCategory != null )
                {
                    bag.SelectedCategoryGuids.Add( selectedCategory.Guid );
                    selectedCategoryForAuth = selectedCategory;
                }
            }

            // In entity-node mode a selected entity arrives in the configured page parameter (when it
            // differs from CategoryId). Select the entity and expand its parent category to reveal it.
            if ( !pageParameterKey.Equals( PageParameterKey.CategoryId, StringComparison.OrdinalIgnoreCase ) )
            {
                var selectedEntityValue = RequestContext.GetPageParameter( pageParameterKey );
                if ( selectedEntityValue.IsNotNullOrWhiteSpace() )
                {
                    /*
                        06/12/26 - JMH

                        AdaptiveMessageCategory is the odd one out among categorized entity types: its
                        tree node value is the AdaptiveMessage.Guid (CategoryClientService swaps it),
                        while its page parameter carries the join-row Id (AdaptiveMessageDetail resolves
                        the message through that Id). Resolve that case against the join row so the
                        AdaptiveMessage node is the one selected and its category is expanded. Every
                        other entity type is its own tree node, keyed by its own Guid.

                        Reason: The node value and the page-parameter currency identify different
                        entities for AdaptiveMessageCategory.
                    */
                    if ( IsAdaptiveMessageCategoryEntityType() )
                    {
                        var adaptiveMessageCategory = GetAdaptiveMessageCategory( selectedEntityValue, allowIntegerId );
                        if ( adaptiveMessageCategory?.AdaptiveMessage != null )
                        {
                            bag.SelectedCategoryGuids.Add( adaptiveMessageCategory.AdaptiveMessage.Guid );

                            var parentCategory = CategoryCache.Get( adaptiveMessageCategory.CategoryId );
                            if ( parentCategory != null )
                            {
                                bag.ExpandedCategoryGuids.Add( parentCategory.Guid );
                                selectedCategoryForAuth = selectedCategoryForAuth ?? parentCategory;
                            }
                        }
                    }
                    else
                    {
                        var entity = GetCategorizedEntity( selectedEntityValue, allowIntegerId );
                        if ( entity != null )
                        {
                            bag.SelectedCategoryGuids.Add( entity.Guid );

                            var parentCategoryId = ( entity as ICategorized )?.CategoryId;
                            if ( parentCategoryId.HasValue )
                            {
                                var parentCategory = CategoryCache.Get( parentCategoryId.Value );
                                if ( parentCategory != null )
                                {
                                    bag.ExpandedCategoryGuids.Add( parentCategory.Guid );
                                    selectedCategoryForAuth = selectedCategoryForAuth ?? parentCategory;
                                }
                            }
                        }
                    }
                }
            }

            // Expand the selected category's ancestor chain so a deep deep-link opens the tree far
            // enough to reveal it (WebForms walked ParentCategory to the root).
            AddAncestorCategoryGuids( selectedCategoryForAuth, bag.ExpandedCategoryGuids );

            var expandedValue = RequestContext.GetPageParameter( PageParameterKey.ExpandedIds );
            if ( expandedValue.IsNotNullOrWhiteSpace() )
            {
                foreach ( var key in expandedValue.SplitDelimitedValues() )
                {
                    var expandedCategory = CategoryCache.Get( key, allowIntegerId );

                    // Skip categories already in the set (the ancestor chain above may have added
                    // them) so the expanded set, and the ExpandedIds it is later re-emitted into,
                    // do not accumulate duplicates. Mirrors the WebForms block's Contains guard.
                    if ( expandedCategory != null && !bag.ExpandedCategoryGuids.Contains( expandedCategory.Guid ) )
                    {
                        bag.ExpandedCategoryGuids.Add( expandedCategory.Guid );
                    }
                }
            }

            if ( !bag.CanEdit && selectedCategoryForAuth != null && selectedCategoryForAuth.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                bag.CanEdit = true;
            }

            return bag;
        }

        /// <summary>
        /// Builds the navigate-mode URL for a selected (or to-be-added) category.
        /// </summary>
        /// <param name="categoryGuid">The selected category, or an empty Guid when adding.</param>
        /// <param name="parentGuid">The parent category, used when adding a child.</param>
        /// <param name="expandedGuids">The categories currently expanded in the tree.</param>
        /// <param name="error">When this returns, indicates whether the URL could not be built.</param>
        /// <returns>The Detail Page URL (or current-page URL) with the page parameters applied.</returns>
        private string GetNavigationUrl( Guid categoryGuid, Guid parentGuid, List<Guid> expandedGuids, out ErrorPouch error )
        {
            error = new ErrorPouch();
            expandedGuids = expandedGuids ?? new List<Guid>();

            /*
                06/18/26 - CLAUDE

                Navigate mode emits the entity IdKey for CategoryId and the configured Page Parameter
                Key; the consumer blocks (PrayerCardView, CategoryList, ScheduleList, AdaptiveMessageList,
                AdaptiveMessageDetail) have been updated to read them IdKey-aware. It routes by node kind
                the way WebForms did: a category node goes to the CategoryId parameter, while an entity
                node (Show Only Categories off) goes to the configured Page Parameter Key. The server
                tells the two apart by resolving the Guid as a category first, then as the configured
                entity type. ParentCategoryId is intentionally still emitted as an integer Id pending an
                IdKey-safe fallback in the Form Builder form list client (Add-child-category path), so do
                not flip it yet.

                Reason: CategoryId and the configured Page Parameter Key now emit the entity IdKey.
            */
            var pageParameterKey = GetAttributeValue( AttributeKey.PageParameterKey ).IfEmpty( DefaultPageParameterKey );
            var qryParams = new Dictionary<string, string>();

            if ( categoryGuid == Guid.Empty )
            {
                // An add action targets a new category; the Detail Page treats CategoryId=0 as "new".
                qryParams[PageParameterKey.CategoryId] = "0";
            }
            else
            {
                var selectedCategory = CategoryCache.Get( categoryGuid );
                if ( selectedCategory != null )
                {
                    qryParams[PageParameterKey.CategoryId] = selectedCategory.IdKey;
                }
                else
                {
                    var entity = GetCategorizedEntity( categoryGuid.ToString(), false );
                    if ( entity == null )
                    {
                        error = new ErrorPouch { IsError = true, Message = "The selected item could not be found." };
                        return string.Empty;
                    }

                    var pageParameterValue = entity.IdKey;

                    // An AdaptiveMessageCategory node carries the AdaptiveMessage.Guid, but its page
                    // parameter is the join-row Id. Map the message back to its join row (preferring
                    // the parent category when the caller supplies one) so the Detail Page resolves it.
                    if ( entity is Rock.Model.AdaptiveMessage && IsAdaptiveMessageCategoryEntityType() )
                    {
                        var joinId = GetAdaptiveMessageCategoryId( entity.Id, parentGuid );
                        if ( joinId.HasValue )
                        {
                            pageParameterValue = Rock.Utility.IdHasher.Instance.GetHash( joinId.Value );
                        }
                    }

                    qryParams[pageParameterKey] = pageParameterValue;
                }
            }

            // The parent and expanded nodes are always categories.
            var parentCategory = parentGuid != Guid.Empty ? CategoryCache.Get( parentGuid ) : null;

            // Adding a top-level category parents it under the configured Root Category when one is
            // set, matching the WebForms "Add Top-Level" behavior.
            if ( parentCategory == null && categoryGuid == Guid.Empty )
            {
                var rootCategoryGuid = GetAttributeValue( AttributeKey.RootCategory ).AsGuidOrNull();
                if ( rootCategoryGuid.HasValue )
                {
                    parentCategory = CategoryCache.Get( rootCategoryGuid.Value );
                }
            }

            if ( parentCategory != null )
            {
                qryParams[PageParameterKey.ParentCategoryId] = parentCategory.Id.ToString();
            }

            var expandedIds = expandedGuids
                .Select( guid => CategoryCache.Get( guid )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value.ToString() )
                .Distinct()
                .ToList();
            if ( expandedIds.Any() )
            {
                qryParams[PageParameterKey.ExpandedIds] = string.Join( ",", expandedIds );
            }

            var detailPageUrl = this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams );
            if ( detailPageUrl.IsNotNullOrWhiteSpace() )
            {
                return detailPageUrl;
            }

            return this.GetCurrentPageUrl( qryParams );
        }

        /// <summary>
        /// Resolves a categorized entity from a key (Id, IdKey, or Guid) using the block's configured entity type.
        /// </summary>
        /// <param name="entityKey">The entity's Id, IdKey, or Guid.</param>
        /// <param name="allowIntegerId">Whether a raw integer Id is accepted for the key.</param>
        /// <returns>The entity, or null when no entity type is configured or no match is found.</returns>
        private IEntity GetCategorizedEntity( string entityKey, bool allowIntegerId )
        {
            var entityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            var entityType = entityTypeGuid.HasValue ? EntityTypeCache.Get( entityTypeGuid.Value ) : null;
            var clrType = entityType?.GetEntityType();
            if ( clrType == null )
            {
                return null;
            }

            var entityGuid = entityKey.AsGuidOrNull();
            if ( entityGuid.HasValue )
            {
                /*
                    06/11/26 - JMH

                    CategoryClientService.GetChildrenItems sets an AdaptiveMessageCategory node's
                    value to its AdaptiveMessage.Guid, so a node Guid resolves to an AdaptiveMessage,
                    not the join row. Mirror that here: when the configured entity type is
                    AdaptiveMessageCategory and the Guid does not resolve to one, fall through to an
                    AdaptiveMessage lookup by the same Guid. GetNavigationUrl then maps the resolved
                    AdaptiveMessage back to its join-row Id for the page parameter.

                    Reason: Unswapped lookup returns null and produces a 400 on entity node clicks.
                */
                if ( clrType == typeof( Rock.Model.AdaptiveMessageCategory ) )
                {
                    var adaptiveMessage = Reflection.GetIEntityForEntityType( typeof( Rock.Model.AdaptiveMessage ), entityGuid.Value );
                    if ( adaptiveMessage != null )
                    {
                        return adaptiveMessage;
                    }
                }

                return Reflection.GetIEntityForEntityType( clrType, entityGuid.Value );
            }

            var entityId = allowIntegerId ? entityKey.AsIntegerOrNull() : null;
            if ( !entityId.HasValue )
            {
                entityId = Rock.Utility.IdHasher.Instance.GetId( entityKey );
            }

            if ( !entityId.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                return Reflection.GetIEntityForEntityType( entityType.Id, entityId.Value, rockContext );
            }
        }

        /// <summary>
        /// Indicates whether the configured entity type is AdaptiveMessageCategory, the one categorized
        /// type whose tree node value and page parameter identify different entities.
        /// </summary>
        /// <returns>true when the configured entity type is AdaptiveMessageCategory; otherwise false.</returns>
        private bool IsAdaptiveMessageCategoryEntityType()
        {
            var entityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            var clrType = entityTypeGuid.HasValue ? EntityTypeCache.Get( entityTypeGuid.Value )?.GetEntityType() : null;
            return clrType == typeof( Rock.Model.AdaptiveMessageCategory );
        }

        /// <summary>
        /// Resolves an AdaptiveMessageCategory join row from a key (Id or IdKey), with its AdaptiveMessage loaded.
        /// </summary>
        /// <param name="key">The join row's Id or IdKey.</param>
        /// <param name="allowIntegerId">Whether a raw integer Id is accepted for the key.</param>
        /// <returns>The join row with its AdaptiveMessage loaded, or null when no match is found.</returns>
        private Rock.Model.AdaptiveMessageCategory GetAdaptiveMessageCategory( string key, bool allowIntegerId )
        {
            var id = allowIntegerId ? key.AsIntegerOrNull() : null;
            if ( !id.HasValue )
            {
                id = Rock.Utility.IdHasher.Instance.GetId( key );
            }

            if ( !id.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                return new AdaptiveMessageCategoryService( rockContext )
                    .Queryable()
                    .Include( a => a.AdaptiveMessage )
                    .FirstOrDefault( a => a.Id == id.Value );
            }
        }

        /// <summary>
        /// Finds the AdaptiveMessageCategory join-row Id for an AdaptiveMessage, optionally scoped to a category.
        /// </summary>
        /// <param name="adaptiveMessageId">The AdaptiveMessage whose join row is needed.</param>
        /// <param name="parentGuid">The category to scope to, or an empty Guid to match the message's first join row.</param>
        /// <returns>The join-row Id, or null when the message has no join row in the given scope.</returns>
        private int? GetAdaptiveMessageCategoryId( int adaptiveMessageId, Guid parentGuid )
        {
            var parentCategoryId = parentGuid != Guid.Empty ? CategoryCache.Get( parentGuid )?.Id : null;

            using ( var rockContext = new RockContext() )
            {
                var query = new AdaptiveMessageCategoryService( rockContext ).Queryable()
                    .Where( a => a.AdaptiveMessageId == adaptiveMessageId );

                if ( parentCategoryId.HasValue )
                {
                    query = query.Where( a => a.CategoryId == parentCategoryId.Value );
                }

                return query.Select( a => ( int? ) a.Id ).FirstOrDefault();
            }
        }

        /// <summary>
        /// Adds the ancestor categories of the given category to the expanded set so a deep-link
        /// selection opens the tree far enough to reveal it.
        /// </summary>
        /// <param name="category">The category whose ancestors should be expanded; may be null.</param>
        /// <param name="expandedGuids">The expanded-category set to add the ancestors to.</param>
        private static void AddAncestorCategoryGuids( CategoryCache category, List<Guid> expandedGuids )
        {
            var visited = new HashSet<Guid>();
            var ancestor = category?.ParentCategory;

            while ( ancestor != null && visited.Add( ancestor.Guid ) )
            {
                if ( !expandedGuids.Contains( ancestor.Guid ) )
                {
                    expandedGuids.Add( ancestor.Guid );
                }

                ancestor = ancestor.ParentCategory;
            }
        }

        #endregion Methods

        #region IHasCustomActions

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canEdit || canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "ti ti-edit",
                    Tooltip = "Set Category Options",
                    ComponentFileUrl = "/Obsidian/Blocks/Core/CategoryTreeView/categoryTreeViewCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion IHasCustomActions

        #region Block Actions

        /// <summary>
        /// Saves the person's hide-inactive-items preference so the active/inactive filter persists across visits.
        /// </summary>
        /// <param name="hideInactiveItems">Whether inactive items should be hidden.</param>
        /// <returns>An OK result.</returns>
        [BlockAction]
        public BlockActionResult SetHideInactiveItems( bool hideInactiveItems )
        {
            var preferences = GetBlockPersonPreferences();
            preferences.SetValue( PersonPreferenceKey.HideInactiveItems, hideInactiveItems.ToTrueFalse() );
            preferences.Save();

            return ActionOk();
        }

        /// <summary>
        /// Gets the current Root Category and Exclude Categories, scoped to the configured entity type, for the custom settings modal.
        /// </summary>
        /// <returns>A box with the current values and the entity type used to scope the category pickers.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            var currentPerson = RequestContext.CurrentPerson;

            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) )
            {
                return ActionForbidden( $"{currentPerson?.FullName} is not authorized to edit block settings." );
            }

            var options = new CategoryTreeViewCustomSettingsOptionsBag
            {
                EntityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull()
            };

            var rootCategory = CategoryCache.Get( GetAttributeValue( AttributeKey.RootCategory ).AsGuid() );
            var excludedCategories = CategoryCache.GetMany(
                GetAttributeValue( AttributeKey.ExcludeCategories )
                    .SplitDelimitedValues()
                    .Select( s => s.AsGuid() )
                    .ToList() );

            var settings = new CategoryTreeViewCustomSettingsBag
            {
                RootCategory = new ListItemBag
                {
                    Value = rootCategory?.Guid.ToString(),
                    Text = rootCategory?.Name
                },
                ExcludedCategories = excludedCategories
                    .Select( c => new ListItemBag
                    {
                        Value = c.Guid.ToString(),
                        Text = c.Name
                    } )
                    .ToList()
            };

            return ActionOk( new CustomSettingsBox<CategoryTreeViewCustomSettingsBag, CategoryTreeViewCustomSettingsOptionsBag>
            {
                Settings = settings,
                Options = options
            } );
        }

        /// <summary>
        /// Saves the Root Category and Exclude Categories edited in the custom settings modal.
        /// </summary>
        /// <param name="box">The box holding the edited values.</param>
        /// <returns>An OK result, or forbidden when the person cannot administrate the block.</returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<CategoryTreeViewCustomSettingsBag, CategoryTreeViewCustomSettingsOptionsBag> box )
        {
            var currentPerson = RequestContext.CurrentPerson;

            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) )
            {
                return ActionForbidden( $"{currentPerson?.FullName} is not authorized to edit block settings." );
            }

            var block = new BlockService( RockContext ).Get( BlockId );
            block.LoadAttributes( RockContext );

            box.IfValidProperty(
                nameof( box.Settings.RootCategory ),
                () => block.SetAttributeValue( AttributeKey.RootCategory, box.Settings.RootCategory?.Value ) );

            box.IfValidProperty(
                nameof( box.Settings.ExcludedCategories ),
                () => block.SetAttributeValue(
                    AttributeKey.ExcludeCategories,
                    box.Settings.ExcludedCategories != null
                        ? string.Join( ",", box.Settings.ExcludedCategories.Select( c => c.Value ) )
                        : string.Empty ) );

            block.SaveAttributeValues( RockContext );

            return ActionOk();
        }

        /// <summary>
        /// Builds the navigate-mode URL for a category selection or add action.
        /// </summary>
        /// <param name="categoryGuid">The selected category, or an empty Guid when adding.</param>
        /// <param name="parentGuid">The parent category, used when adding a child.</param>
        /// <param name="expandedGuids">The categories currently expanded in the tree.</param>
        /// <returns>The navigation URL, or a bad-request result when it could not be built.</returns>
        [BlockAction]
        public BlockActionResult GetNavigationUrl( Guid categoryGuid, Guid parentGuid, List<Guid> expandedGuids )
        {
            var url = GetNavigationUrl( categoryGuid, parentGuid, expandedGuids, out var error );

            if ( url.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( error.IsError ? error.Message : "Could not determine the navigation URL for the provided category." );
            }

            return ActionOk( url );
        }

        /// <summary>
        /// Builds the URL for adding a new entity, parented to the selected node's category. Used by
        /// the entity-node "Add" affordance when the tree shows categorized entities.
        /// </summary>
        /// <param name="selectedGuid">The selected category or entity; empty to add without a parent.</param>
        /// <param name="expandedGuids">The categories currently expanded in the tree, re-emitted so the Detail Page can restore the open state.</param>
        /// <returns>The add URL, or a bad-request result when no Detail Page is configured.</returns>
        [BlockAction]
        public BlockActionResult GetAddEntityUrl( Guid selectedGuid, List<Guid> expandedGuids )
        {
            var pageParameterKey = GetAttributeValue( AttributeKey.PageParameterKey ).IfEmpty( DefaultPageParameterKey );
            var qryParams = new Dictionary<string, string> { [pageParameterKey] = "0" };

            // Add the new item under the selected category, or under a selected entity's parent category.
            int? parentCategoryId = null;
            if ( selectedGuid != Guid.Empty )
            {
                var selectedCategory = CategoryCache.Get( selectedGuid );
                parentCategoryId = selectedCategory != null
                    ? selectedCategory.Id
                    : ( GetCategorizedEntity( selectedGuid.ToString(), false ) as ICategorized )?.CategoryId;
            }

            if ( parentCategoryId.HasValue )
            {
                qryParams[PageParameterKey.ParentCategoryId] = parentCategoryId.Value.ToString();
            }

            // Carry the expanded categories through so the tree restores its open state after the add,
            // matching the category-add path and the WebForms lbAddItem behavior.
            var expandedIds = ( expandedGuids ?? new List<Guid>() )
                .Select( guid => CategoryCache.Get( guid )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value.ToString() )
                .Distinct()
                .ToList();
            if ( expandedIds.Any() )
            {
                qryParams[PageParameterKey.ExpandedIds] = string.Join( ",", expandedIds );
            }

            var url = this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams );
            if ( url.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "No Detail Page is configured for adding an item." );
            }

            return ActionOk( url );
        }

        #endregion Block Actions

        #region Helper Classes

        private class ErrorPouch
        {
            public bool IsError { get; set; } = false;

            public string Message { get; set; } = string.Empty;
        }

        #endregion Helper Classes
    }
}
