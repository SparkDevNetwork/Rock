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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.AttributeCategoryList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of attribute categories and allows them to be managed.
    /// </summary>
    [DisplayName( "Attribute Categories" )]
    [Category( "Core" )]
    [Description( "Allows attribute categories to be managed." )]
    [IconCssClass( "ti ti-folder" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "B23C0C3C-E868-472A-88D6-2E9E94EF1860" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "EB736280-D4CB-4153-A99E-7E49FB15B594" )]
    [Rock.SystemGuid.BlockTypeGuid( "1FC50941-A883-47A2-ABE9-13528BCC4D1B" )]
    [CustomizedGrid]
    public class AttributeCategoryList : RockEntityListBlockType<Category>
    {
        #region Keys

        private static class PersonPreferenceKey
        {
            public const string FilterEntityType = "filter-entity-type";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the entity type filter Guid from the person preferences.
        /// Returns null when no filter is selected, or the empty GUID for
        /// "None (Global Attributes)".
        /// </summary>
        private Guid? FilterEntityTypeGuid => GetBlockPersonPreferences()
            .GetValue( PersonPreferenceKey.FilterEntityType )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AttributeCategoryListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private AttributeCategoryListOptionsBag GetBoxOptions()
        {
            return new AttributeCategoryListOptionsBag();
        }

        /// <summary>
        /// Determines if the add and delete buttons should be enabled in the grid.
        /// </summary>
        /// <returns>A boolean value that indicates if the add and delete buttons should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the list of valid entity type ID strings. These are entity types
        /// returned by GetEntities() (registered entities) minus Block and ServiceJob,
        /// which are controlled through code attribute decorations.
        /// </summary>
        /// <returns>A list of valid entity type ID strings.</returns>
        private List<string> GetValidEntityTypeIds( RockContext rockContext )
        {
            var exclusions = new List<Guid>
            {
                SystemGuid.EntityType.BLOCK.AsGuid(),
                SystemGuid.EntityType.SERVICE_JOB.AsGuid()
            };

            return new EntityTypeService( rockContext ).GetEntities()
                .Where( t => !exclusions.Contains( t.Guid ) )
                .Select( t => t.Id.ToString() )
                .ToList();
        }

        /// <inheritdoc/>
        protected override IQueryable<Category> GetListQueryable( RockContext rockContext )
        {
            var attributeEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;

            var queryable = new CategoryService( rockContext ).Queryable()
                .Where( c =>
                    c.EntityTypeId == attributeEntityTypeId &&
                    c.EntityTypeQualifierColumn == "EntityTypeId" );

            // Apply the entity type filter from person preferences.
            var filterGuid = FilterEntityTypeGuid;
            if ( filterGuid.HasValue )
            {
                if ( filterGuid.Value == Guid.Empty )
                {
                    // "None (Global Attributes)" — categories with no entity type.
                    queryable = queryable.Where( c => c.EntityTypeQualifierValue == null );
                }
                else
                {
                    var entityType = EntityTypeCache.Get( filterGuid.Value );
                    if ( entityType != null )
                    {
                        var entityTypeId = entityType.Id.ToString();
                        queryable = queryable.Where( c => c.EntityTypeQualifierValue == entityTypeId );
                    }
                }
            }
            else
            {
                // When no filter is applied, only include categories for valid
                // registered entity types (excluding Block and ServiceJob).
                var validEntityTypeIds = GetValidEntityTypeIds( rockContext );

                queryable = queryable.Where( c =>
                    c.EntityTypeQualifierValue == null ||
                    validEntityTypeIds.Contains( c.EntityTypeQualifierValue ) );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<Category> GetOrderedListQueryable( IQueryable<Category> queryable, RockContext rockContext )
        {
            // When filtered by entity type, order by the explicit Order field
            // so drag-and-drop reordering is meaningful. Otherwise, order by name.
            if ( FilterEntityTypeGuid.HasValue )
            {
                return queryable
                    .OrderBy( c => c.Order )
                    .ThenBy( c => c.Name );
            }

            return queryable.OrderBy( c => c.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Category> GetGridBuilder()
        {
            return new GridBuilder<Category>()
                .WithBlock( this )
                .AddTextField( "idKey", c => c.IdKey )
                .AddTextField( "name", c => c.Name )
                .AddTextField( "entityTypeName", c => GetEntityTypeName( c.EntityTypeQualifierValue ) )
                .AddTextField( "iconCssClass", c => c.IconCssClass );
        }

        /// <summary>
        /// Gets the display name for an entity type qualifier value.
        /// Returns "None (Global Attributes)" when the qualifier value is null or empty.
        /// </summary>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value (entity type ID as string).</param>
        /// <returns>The friendly name of the entity type, or "None (Global Attributes)".</returns>
        private string GetEntityTypeName( string entityTypeQualifierValue )
        {
            if ( entityTypeQualifierValue.IsNullOrWhiteSpace() )
            {
                return "None (Global Attributes)";
            }

            var entityTypeId = entityTypeQualifierValue.AsIntegerOrNull();
            if ( entityTypeId.HasValue && entityTypeId.Value > 0 )
            {
                var entityType = EntityTypeCache.Get( entityTypeId.Value );
                if ( entityType != null )
                {
                    return entityType.FriendlyName;
                }
            }

            return "None (Global Attributes)";
        }

        /// <summary>
        /// Gets the entity bag for editing.
        /// </summary>
        /// <param name="category">The category entity to convert to a bag.</param>
        /// <returns>An <see cref="AttributeCategoryBag"/> representing the category.</returns>
        private AttributeCategoryBag GetEntityBagForEdit( Category category )
        {
            if ( category == null )
            {
                return null;
            }

            ListItemBag entityTypeBag = null;
            var entityTypeId = category.EntityTypeQualifierValue.AsIntegerOrNull();

            if ( entityTypeId.HasValue && entityTypeId.Value > 0 )
            {
                var entityType = EntityTypeCache.Get( entityTypeId.Value );
                if ( entityType != null )
                {
                    entityTypeBag = new ListItemBag
                    {
                        Value = entityType.Guid.ToString(),
                        Text = entityType.FriendlyName
                    };
                }
            }

            return new AttributeCategoryBag
            {
                IdKey = category.IdKey,
                Name = category.Name,
                Description = category.Description,
                EntityType = entityTypeBag,
                IconCssClass = category.IconCssClass,
                HighlightColor = category.HighlightColor
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the specified category for editing.
        /// </summary>
        /// <param name="key">The identifier of the category to edit. An empty key indicates a new category.</param>
        /// <returns>A block action result containing the <see cref="AttributeCategoryBag"/>.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to edit attribute categories." );
            }

            Category category;

            if ( key.IsNullOrWhiteSpace() )
            {
                // New category — return an empty bag.
                category = new Category
                {
                    Id = 0
                };
            }
            else
            {
                var entityService = new CategoryService( RockContext );
                category = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( category == null )
                {
                    return ActionBadRequest( "Category not found." );
                }
            }

            return ActionOk( GetEntityBagForEdit( category ) );
        }

        /// <summary>
        /// Saves the specified category.
        /// </summary>
        /// <param name="bag">The bag containing the category data to save.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( AttributeCategoryBag bag )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to edit attribute categories." );
            }

            var entityService = new CategoryService( RockContext );
            Category category;

            if ( bag.IdKey.IsNullOrWhiteSpace() )
            {
                category = new Category
                {
                    EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id,
                    EntityTypeQualifierColumn = "EntityTypeId"
                };

                // Set the order to the next available value.
                var orders = entityService.Queryable()
                    .Where( c =>
                        c.EntityTypeId == category.EntityTypeId &&
                        c.EntityTypeQualifierColumn == "EntityTypeId" )
                    .Select( c => c.Order )
                    .ToList();

                category.Order = orders.Any() ? orders.Max() + 1 : 0;

                entityService.Add( category );
            }
            else
            {
                category = entityService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( category == null )
                {
                    return ActionBadRequest( "Category not found." );
                }
            }

            category.Name = bag.Name;
            category.Description = bag.Description;
            category.IconCssClass = bag.IconCssClass;
            category.HighlightColor = bag.HighlightColor;

            // Set the entity type qualifier value from the selected entity type.
            // The EntityTypePicker sends a Guid; the empty GUID means "None (Global Attributes)".
            var entityTypeGuid = bag.EntityType?.Value.AsGuidOrNull();
            if ( entityTypeGuid.HasValue && entityTypeGuid.Value != Guid.Empty )
            {
                var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                category.EntityTypeQualifierValue = entityType?.Id.ToString();
            }
            else
            {
                category.EntityTypeQualifierValue = null;
            }

            if ( !category.IsValid )
            {
                return ActionBadRequest( category.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified category.
        /// </summary>
        /// <param name="key">The identifier of the category to delete.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new CategoryService( RockContext );
            var category = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( category == null )
            {
                return ActionBadRequest( "Category not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to delete attribute categories." );
            }

            if ( !entityService.CanDelete( category, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( category );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );
            var items = GetListItems( qry, RockContext );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
