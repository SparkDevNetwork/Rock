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
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.TagList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of tags.
    /// </summary>

    [DisplayName( "Tag List" )]
    [Category( "Core" )]
    [Description( "Block for viewing a list of tags." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [BooleanField( "Show Qualifier Columns",
        Description = "Should the 'Qualifier Column' and 'Qualifier Value' fields be displayed in the grid?",
        DefaultValue = "false",
        Order = 0,
        Key = AttributeKey.ShowQualifierColumns )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the tag details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "9a396390-842f-4408-aefd-fb4793f9ef7e" )]
    [Rock.SystemGuid.BlockTypeGuid( "0acf764f-5f60-4985-9d10-029cb042da0d" )]
    [CustomizedGrid]
    public class TagList : RockEntityListBlockType<Tag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ShowQualifierColumns = "ShowQualifierColumns";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterCategory = "filter-category";
            public const string FilterEntityName = "filter-entity-name";
            public const string FilterScope = "filter-scope";
            public const string FilterOwner = "filter-owner";
        }

        private static class ScopeKey
        {
            public const string Personal = "Personal";
            public const string Organization = "Organization";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the category the results should belong to.
        /// </summary>
        /// <value>
        /// The filter category.
        /// </value>
        protected string FilterCategory => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCategory ).FromJsonOrNull<ListItemBag>()?.Value;

        /// <summary>
        /// Gets associated the entity type of the results.
        /// </summary>
        /// <value>
        /// The entity type of the tags.
        /// </value>
        protected string FilterEntityName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterEntityName ).FromJsonOrNull<ListItemBag>()?.Value;

        /// <summary>
        /// Gets scope the results should belong to, i.e personal or organizational
        /// </summary>
        /// <value>
        /// The tag scope.
        /// </value>
        protected List<string> FilterScopes => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterScope )
            .FromJsonOrNull<List<string>>() ?? new List<string>();

        /// <summary>
        /// Gets the owner the results should belong to.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        protected string FilterOwner => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterOwner ).FromJsonOrNull<ListItemBag>()?.Value;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<TagListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = true;
            box.IsDeleteEnabled = GetCanConfigure();
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private TagListOptionsBag GetBoxOptions()
        {
            var canConfigure = GetCanConfigure();

            var options = new TagListOptionsBag()
            {
                IsReorderColumnVisible = canConfigure,
                IsQualifierColumnsVisible = GetAttributeValue( AttributeKey.ShowQualifierColumns ).AsBoolean(),
                CurrentPersonAlias = GetCurrentPerson().PrimaryAlias?.ToListItemBag(),
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetCanConfigure()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "TagId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Tag> GetListQueryable( RockContext rockContext )
        {
            var queryable = new TagService( new RockContext() ).Queryable()
                .Include( a => a.EntityType )
                .Include( a => a.OwnerPersonAlias );

            var categoryGuid = FilterCategory.AsGuidOrNull();
            if ( categoryGuid.HasValue )
            {
                queryable = queryable.Where( t => t.Category.Guid == categoryGuid.Value );
            }

            var entityTypeGuid = FilterEntityName.AsGuidOrNull();
            if ( entityTypeGuid.HasValue )
            {
                queryable = queryable.Where( t => t.EntityType.Guid == entityTypeGuid.Value );
            }

            string personFlag = GetPersonFlag();

            bool includeOrgs = FilterScopes.Contains( ScopeKey.Organization );

            switch ( personFlag )
            {
                // No people
                case "":
                    return includeOrgs ?
                        queryable.Where( t => !t.OwnerPersonAliasId.HasValue ) :
                        queryable;

                // All people
                case "All":
                    return includeOrgs ?
                        queryable :
                        queryable.Where( t => t.OwnerPersonAliasId.HasValue );

                // Specific Person
                default:
                    var personAliasGuid = personFlag.AsGuid();
                    return includeOrgs ?
                        queryable.Where( t => t.OwnerPersonAlias == null || t.OwnerPersonAlias.Guid == personAliasGuid ) :
                        queryable.Where( t => t.OwnerPersonAlias != null && t.OwnerPersonAlias.Guid == personAliasGuid );
            }
        }

        /// <inheritdoc/>
        protected override IQueryable<Tag> GetOrderedListQueryable( IQueryable<Tag> queryable, RockContext rockContext )
        {
            var ordered = GetCanConfigure() && !FilterScopes.Contains( ScopeKey.Personal );

            if ( ordered )
            {
                return queryable.OrderBy( t => t.Order ).ThenBy( t => t.Name );
            }
            else
            {
                return queryable.OrderBy( t => t.Name );
            }
        }

        /// <inheritdoc/>
        protected override GridBuilder<Tag> GetGridBuilder()
        {
            return new GridBuilder<Tag>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "entityType", a => a.EntityType != null ? a.EntityType.FriendlyName : "<All>" )
                .AddTextField( "entityTypeQualifierColumn", a => a.EntityTypeQualifierColumn )
                .AddTextField( "entityTypeQualifierValue", a => a.EntityTypeQualifierValue )
                .AddPersonField( "owner", a => a.OwnerPersonAlias?.Person )
                .AddField( "isActive", a => a.IsActive )
                .AddTextField( "scope", a => a.OwnerPersonAlias != null ? ScopeKey.Personal : ScopeKey.Organization )
                .AddField( "entityCount", a => a.TaggedItems.Count );
        }

        /// <summary>
        /// Flag indicating whether or not to scope results personal tags and if so where to scope them to a specific individual.
        /// </summary>
        /// <returns></returns>
        private string GetPersonFlag()
        {
            var canConfigure = GetCanConfigure();
            string personFlag = string.Empty;     // Space = None, All = All, Guid = Specific Person
            if ( FilterScopes.Contains( ScopeKey.Personal ) )
            {
                if ( canConfigure )
                {
                    personFlag = FilterOwner.AsGuidOrNull()?.ToString() ?? "All";
                }
                else
                {
                    var currentPerson = GetCurrentPerson();
                    personFlag = currentPerson != null ? currentPerson.PrimaryAlias.Guid.ToString() : string.Empty;
                }
            }

            return personFlag;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var qry = GetListQueryable( rockContext );
                qry = GetOrderedListQueryable( qry, rockContext );

                // Get the entities from the database.
                var items = GetListItems( qry, rockContext );

                if ( !items.ReorderEntity( key, beforeKey ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new TagService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Tag.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {Tag.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}
