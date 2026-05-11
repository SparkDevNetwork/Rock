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
using Rock.ViewModels.Blocks.Core.CategoryList;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of categories and allows them to be managed for a
    /// specific configured entity type (or any entity type when not configured).
    /// Supports hierarchy drill-down, reorder, inline add/edit modal with
    /// entity attribute editing, and server-side entity-type filtering.
    /// </summary>
    [DisplayName( "Categories" )]
    [Category( "Core" )]
    [Description( "Block for managing categories for a specific, configured entity type." )]
    [IconCssClass( "ti ti-folder-open" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [EntityTypeField( "Entity Type",
        Description = "The entity type to manage categories for.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EntityType )]

    [TextField( "Entity Qualifier Column",
        Description = "Column to evaluate to determine entities that this category applies to.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.EntityQualifierColumn )]

    [TextField( "Entity Qualifier Value",
        Description = "The value of the column that this category applies to.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.EntityQualifierValue )]

    [BooleanField( "Enable Hierarchy",
        Description = "When set allows you to drill down through the category hierarchy.",
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKey.EnableHierarchy )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "01785FCD-FB94-436A-87FB-0A0F5C70BD59" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "6BF24209-2B9C-4ABB-9414-9DC4E2EFE758" )]
    [Rock.SystemGuid.BlockTypeGuid( "620FC4A2-6587-409F-8972-22065919D9AC" )]
    [CustomizedGrid]
    public class CategoryList : RockEntityListBlockType<Category>, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EntityType = "EntityType";
            public const string EntityQualifierColumn = "EntityQualifierColumn";
            public const string EntityQualifierValue = "EntityQualifierValue";
            public const string EnableHierarchy = "EnableHierarchy";
        }

        private static class PageParameterKey
        {
            public const string Category = "CategoryId";
            public const string EntityType = "EntityTypeId";
            public const string EntityQualifierColumn = "EntityQualifierColumn";
            public const string EntityQualifierValue = "EntityQualifierValue";
        }

        private static class PersonPreferenceKey
        {
            public const string FilterEntityType = "filter-entity-type";
        }

        #endregion

        #region Fields

        /// <summary>
        /// Resolved entity-type context for the current request. Populated
        /// lazily on first access via <see cref="GetEntityContext"/>.
        /// </summary>
        private EntityContext? _context;

        /// <summary>
        /// Cache of entity-type IDs that should never be surfaced via this
        /// block (Block and ServiceJob categories are driven by code attribute
        /// decorations).
        /// </summary>
        private static readonly Lazy<List<Guid>> _excludedEntityTypeGuids = new Lazy<List<Guid>>( () => new List<Guid>
        {
            SystemGuid.EntityType.BLOCK.AsGuid(),
            SystemGuid.EntityType.SERVICE_JOB.AsGuid()
        } );

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type filter selected via the grid header picker
        /// (only applicable when the entity-type context is not fixed).
        /// Returns <c>null</c> when no filter is selected.
        /// </summary>
        private Guid? FilterEntityTypeGuid => GetBlockPersonPreferences()
            .GetValue( PersonPreferenceKey.FilterEntityType )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CategoryListOptionsBag>();
            var isAuthorized = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            if ( !isAuthorized )
            {
                box.Options = new CategoryListOptionsBag
                {
                    IsBlockVisible = false,
                    BlockErrorMessage = "You are not authorized to configure this page."
                };
                return box;
            }

            var builder = GetGridBuilder();

            box.IsAddEnabled = true;
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();
            box.SecurityGrantToken = GetSecurityGrantToken();

            return box;
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            return GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the security grant token used by entity-attribute field types
        /// that need additional per-field permissions to render edit controls.
        /// </summary>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            foreach ( var fieldType in FieldTypeCache.All() )
            {
                if ( fieldType.Field is Rock.Field.ISecurityGrantFieldType grantFieldType )
                {
                    grantFieldType.AddRulesToSecurityGrant( securityGrant, new Dictionary<string, string>() );
                }
            }

            var attributes = AttributeCache.AllForEntityType<Category>();
            securityGrant.AddRulesForAttributes( attributes );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Gets the box options required for the component to render.
        /// </summary>
        private CategoryListOptionsBag GetBoxOptions()
        {
            var context = GetEntityContext();
            var isHierarchyEnabled = GetAttributeValue( AttributeKey.EnableHierarchy ).AsBoolean();

            var isSecured = false;
            if ( context.EntityTypeId > 0 )
            {
                var entityType = EntityTypeCache.Get( context.EntityTypeId );
                if ( entityType != null )
                {
                    isSecured = entityType.IsSecured;
                }
            }

            return new CategoryListOptionsBag
            {
                IsBlockVisible = true,
                IsEntityTypeContextFixed = context.IsFixed,
                IsHierarchyEnabled = isHierarchyEnabled,
                IsSecurityColumnVisible = isSecured,
                ExcludedEntityTypeGuids = _excludedEntityTypeGuids.Value
            };
        }

        /// <summary>
        /// Resolves (once per request) and returns the active
        /// <see cref="EntityContext"/> for this block invocation.
        /// </summary>
        private EntityContext GetEntityContext()
        {
            if ( _context.HasValue )
            {
                return _context.Value;
            }

            /*
                4/16/2026 - MSE

                Entity-type scope resolution priority:

                1. Parent drill-down — when ?CategoryId is present, the
                   parent category's EntityTypeId and qualifier column/value
                   define the scope.
                2. URL parameters — ?EntityTypeId plus optional
                   ?EntityQualifierColumn and ?EntityQualifierValue.
                3. Block settings — the configured EntityType attribute and
                   its qualifier-column/value attributes.
                4. None — no scope is pinned; the grid-header entity-type
                   filter can still narrow the list at query time.

                External callers link here with varying combinations of
                these inputs, so reordering would silently change what
                categories appear for those links.
            */

            var parentCategoryId = PageParameter( PageParameterKey.Category ).AsIntegerOrNull();
            if ( parentCategoryId.HasValue )
            {
                var parentCategory = CategoryCache.Get( parentCategoryId.Value );
                if ( parentCategory != null )
                {
                    _context = new EntityContext(
                        parentCategory.EntityTypeId ?? 0,
                        parentCategory.EntityTypeQualifierColumn,
                        parentCategory.EntityTypeQualifierValue,
                        parentCategoryId );
                    return _context.Value;
                }
            }

            var urlEntityTypeId = PageParameter( PageParameterKey.EntityType ).AsIntegerOrNull();
            if ( urlEntityTypeId.HasValue )
            {
                _context = new EntityContext(
                    urlEntityTypeId.Value,
                    PageParameter( PageParameterKey.EntityQualifierColumn ) ?? string.Empty,
                    PageParameter( PageParameterKey.EntityQualifierValue ) ?? string.Empty,
                    null );
                return _context.Value;
            }

            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EntityType ), out var configuredEntityTypeGuid ) )
            {
                _context = new EntityContext(
                    EntityTypeCache.GetId( configuredEntityTypeGuid ) ?? 0,
                    GetAttributeValue( AttributeKey.EntityQualifierColumn ) ?? string.Empty,
                    GetAttributeValue( AttributeKey.EntityQualifierValue ) ?? string.Empty,
                    null );
                return _context.Value;
            }

            _context = new EntityContext( 0, null, null, null );
            return _context.Value;
        }

        /// <inheritdoc/>
        protected override IQueryable<Category> GetListQueryable( RockContext rockContext )
        {
            var context = GetEntityContext();
            var queryable = new CategoryService( rockContext ).Queryable();

            if ( context.ParentCategoryId.HasValue )
            {
                // Drill-down: show only direct children of the parent.
                var parentId = context.ParentCategoryId.Value;
                queryable = queryable.Where( c => c.ParentCategoryId == parentId );
            }
            else
            {
                queryable = queryable.Where( c => c.ParentCategoryId == null );

                if ( context.EntityTypeId > 0 )
                {
                    var entityTypeId = context.EntityTypeId;
                    queryable = queryable.Where( c => c.EntityTypeId == entityTypeId );
                }
                else
                {
                    // Apply the optional entity-type filter from the grid header.
                    var filterGuid = FilterEntityTypeGuid;
                    if ( filterGuid.HasValue )
                    {
                        var filterEntityTypeId = EntityTypeCache.GetId( filterGuid.Value );
                        if ( filterEntityTypeId.HasValue )
                        {
                            queryable = queryable.Where( c => c.EntityTypeId == filterEntityTypeId.Value );
                        }
                    }
                }

                if ( context.QualifierColumn != null && context.QualifierValue != null )
                {
                    var qualifierColumn = context.QualifierColumn;
                    var qualifierValue = context.QualifierValue;
                    queryable = queryable.Where( c =>
                        ( c.EntityTypeQualifierColumn ?? "" ) == qualifierColumn &&
                        ( c.EntityTypeQualifierValue ?? "" ) == qualifierValue );
                }
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<Category> GetOrderedListQueryable( IQueryable<Category> queryable, RockContext rockContext )
        {
            return queryable
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Category> GetGridBuilder()
        {
            return new GridBuilder<Category>()
                .WithBlock( this )
                .AddTextField( "idKey", c => c.IdKey )
                .AddField( "id", c => c.Id )
                .AddTextField( "name", c => c.Name )
                .AddTextField( "description", c => c.Description )
                .AddTextField( "iconCssClass", c => c.IconCssClass )
                .AddField( "childCount", c => c.ChildCategories.Count() )
                .AddTextField( "entityTypeName", c => c.EntityType.Name )
                .AddTextField( "entityQualifierField", c => c.EntityTypeQualifierColumn )
                .AddTextField( "entityQualifierValue", c => c.EntityTypeQualifierValue )
                .AddField( "isSystem", c => c.IsSystem );
        }

        /// <summary>
        /// Builds the edit bag for the given category, including its attributes
        /// and attribute values. A <c>null</c> entity yields <c>null</c>.
        /// </summary>
        private CategoryBag GetEntityBagForEdit( Category category )
        {
            if ( category == null )
            {
                return null;
            }

            var bag = new CategoryBag
            {
                IdKey = category.IdKey,
                Name = category.Name,
                Description = category.Description,
                IconCssClass = category.IconCssClass,
                HighlightColor = category.HighlightColor,
                ParentCategory = category.ParentCategoryId.HasValue
                    ? CategoryCache.Get( category.ParentCategoryId.Value ).ToListItemBag()
                    : null,
                EntityType = category.EntityTypeId > 0
                    ? EntityTypeCache.Get( category.EntityTypeId ).ToListItemBag()
                    : null,
                EntityTypeQualifierColumn = category.EntityTypeQualifierColumn,
                EntityTypeQualifierValue = category.EntityTypeQualifierValue
            };

            bag.LoadAttributesAndValuesForPublicEdit( category, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        /// <summary>
        /// Applies the entity-type and qualifier assignment to the category
        /// being saved, using the resolved <paramref name="context"/>.
        /// </summary>
        private void ApplyEntityTypeAssignment( Category category, CategoryBag bag, EntityContext context )
        {
            /*
                4/16/2026 - MSE

                Priority mirrors GetEntityContext(): parent drill-down
                overrides block/URL config, which overrides the modal's
                user-selected picker. The picker is only reachable when no
                other context is fixed, so falling through to it is the last
                branch by design.

                Reason: Entity-type precedence on save must match the
                precedence used when reading the list, or editing a category
                in drill-down mode could silently move it to a different
                entity type.
            */

            if ( context.ParentCategoryId.HasValue )
            {
                var parentCategory = CategoryCache.Get( context.ParentCategoryId.Value );
                category.EntityTypeId = parentCategory?.EntityTypeId ?? 0;
                category.EntityTypeQualifierColumn = parentCategory?.EntityTypeQualifierColumn;
                category.EntityTypeQualifierValue = parentCategory?.EntityTypeQualifierValue;
                return;
            }

            if ( context.EntityTypeId > 0 )
            {
                category.EntityTypeId = context.EntityTypeId;
                category.EntityTypeQualifierColumn = context.QualifierColumn;
                category.EntityTypeQualifierValue = context.QualifierValue;
                return;
            }

            // User-specified via the modal picker.
            var bagEntityTypeGuid = bag.EntityType?.Value.AsGuidOrNull();
            category.EntityTypeId = bagEntityTypeGuid.HasValue
                ? EntityTypeCache.GetId( bagEntityTypeGuid.Value ) ?? 0
                : 0;
            category.EntityTypeQualifierColumn = bag.EntityTypeQualifierColumn;
            category.EntityTypeQualifierValue = bag.EntityTypeQualifierValue;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var entityTypeName = string.Empty;
            EntityTypeCache entityType = null;

            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EntityType ), out var configuredEntityTypeGuid ) )
            {
                entityType = EntityTypeCache.Get( configuredEntityTypeGuid );
            }

            if ( entityType != null )
            {
                entityTypeName = entityType.FriendlyName;
            }

            CategoryCache parentCategory = null;
            var parentCategoryId = pageReference.GetPageParameter( PageParameterKey.Category ).AsIntegerOrNull();
            if ( parentCategoryId.HasValue )
            {
                parentCategory = CategoryCache.Get( parentCategoryId.Value );

                if ( entityType == null && parentCategory?.EntityTypeId.HasValue == true )
                {
                    entityType = EntityTypeCache.Get( parentCategory.EntityTypeId.Value );
                    if ( entityType != null )
                    {
                        entityTypeName = entityType.FriendlyName;
                    }
                }
            }

            if ( entityType == null && parentCategory == null )
            {
                return new BreadCrumbResult();
            }

            var crumbs = new List<IBreadCrumb>();

            var walker = parentCategory;
            while ( walker != null )
            {
                var parms = new Dictionary<string, string>
                {
                    { PageParameterKey.Category, walker.Id.ToString() }
                };
                crumbs.Add( new BreadCrumbLink( walker.Name, new PageReference( pageReference.PageId, 0, parms ) ) );
                walker = walker.ParentCategoryId.HasValue ? CategoryCache.Get( walker.ParentCategoryId.Value ) : null;
            }

            // Root crumb: "{EntityType} Categories" when no qualifier column is
            // configured, otherwise fall back to the page title.
            var rootTitle = GetAttributeValue( AttributeKey.EntityQualifierColumn ).IsNullOrWhiteSpace()
                ? $"{entityTypeName} Categories"
                : PageCache.PageTitle;
            crumbs.Add( new BreadCrumbLink( rootTitle, new PageReference( pageReference.PageId ) ) );

            crumbs.Reverse();

            return new BreadCrumbResult
            {
                BreadCrumbs = crumbs
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the specified category for editing. An empty key indicates a
        /// new category — the resulting bag is pre-populated with the active
        /// entity-type context (from parent drill-down or block config).
        /// </summary>
        /// <param name="key">The identifier of the category to edit.</param>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to edit categories." );
            }

            Category category;

            if ( key.IsNullOrWhiteSpace() )
            {
                var context = GetEntityContext();
                category = new Category
                {
                    Id = 0,
                    EntityTypeId = context.EntityTypeId,
                    EntityTypeQualifierColumn = context.QualifierColumn,
                    EntityTypeQualifierValue = context.QualifierValue,
                    ParentCategoryId = context.ParentCategoryId
                };
            }
            else
            {
                category = new CategoryService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );
                if ( category == null )
                {
                    return ActionBadRequest( "Category not found." );
                }
            }

            category.LoadAttributes( RockContext );

            return ActionOk( GetEntityBagForEdit( category ) );
        }

        /// <summary>
        /// Saves the specified category. Creates a new record when
        /// <see cref="EntityBagBase.IdKey"/> is empty, otherwise updates the
        /// existing one.
        /// </summary>
        /// <param name="bag">The bag containing the category data to save.</param>
        [BlockAction]
        public BlockActionResult Save( CategoryBag bag )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to edit categories." );
            }

            var context = GetEntityContext();
            var entityService = new CategoryService( RockContext );
            Category category;

            if ( bag.IdKey.IsNullOrWhiteSpace() )
            {
                category = new Category();

                var maxOrder = GetListQueryable( RockContext ).Max( c => ( int? ) c.Order );
                category.Order = ( maxOrder ?? -1 ) + 1;

                entityService.Add( category );
            }
            else
            {
                category = entityService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );
                if ( category == null )
                {
                    return ActionBadRequest( "Category not found." );
                }

                if ( category.IsSystem )
                {
                    return ActionBadRequest( "System categories cannot be modified." );
                }
            }

            ApplyEntityTypeAssignment( category, bag, context );

            category.Name = bag.Name;
            category.Description = bag.Description;
            category.IconCssClass = bag.IconCssClass;
            category.HighlightColor = bag.HighlightColor;

            category.ParentCategoryId = context.ParentCategoryId ?? bag.ParentCategory.GetEntityId<Category>( RockContext );

            category.LoadAttributes( RockContext );
            if ( bag.AttributeValues != null )
            {
                category.SetPublicAttributeValues( bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
            }

            if ( !category.IsValid )
            {
                return ActionBadRequest( category.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                category.SaveAttributeValues( RockContext );
            } );

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified category.
        /// </summary>
        /// <param name="key">The identifier of the category to delete.</param>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to delete categories." );
            }

            var entityService = new CategoryService( RockContext );
            var category = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( category == null )
            {
                return ActionBadRequest( "Category not found." );
            }

            if ( category.IsSystem )
            {
                return ActionBadRequest( "System categories cannot be deleted." );
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
        /// Changes the ordered position of a single category within the
        /// currently displayed scope.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to reorder categories." );
            }

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

        #region Nested Types

        /// <summary>
        /// The resolved entity-type scope for the current block invocation:
        /// the optional parent category (when drilling down), the entity type
        /// the listed categories belong to, and the entity-type qualifier
        /// column/value that further narrows that scope. These four values
        /// flow together through the list query, bag population, and save
        /// path, so they are resolved once per request and passed as a unit.
        /// </summary>
        private readonly struct EntityContext
        {
            public EntityContext( int entityTypeId, string qualifierColumn, string qualifierValue, int? parentCategoryId )
            {
                EntityTypeId = entityTypeId;
                QualifierColumn = qualifierColumn;
                QualifierValue = qualifierValue;
                ParentCategoryId = parentCategoryId;
            }

            /// <summary>
            /// Gets the id of the entity type that the listed categories
            /// belong to, or <c>0</c> when no entity type has been fixed by
            /// block setting, URL parameter, or parent drill-down.
            /// </summary>
            public int EntityTypeId { get; }

            /// <summary>
            /// Gets the entity-type qualifier column name (e.g.,
            /// <c>"GroupTypeId"</c>) that narrows the scope, or <c>null</c>
            /// when no qualifier applies.
            /// </summary>
            public string QualifierColumn { get; }

            /// <summary>
            /// Gets the value paired with <see cref="QualifierColumn"/>, or
            /// <c>null</c> when no qualifier applies.
            /// </summary>
            public string QualifierValue { get; }

            /// <summary>
            /// Gets the id of the parent category when the user is drilling
            /// into its children, otherwise <c>null</c>.
            /// </summary>
            public int? ParentCategoryId { get; }

            /// <summary>
            /// Gets a value indicating whether the entity-type scope is
            /// pinned — by a parent drill-down or by an in-scope entity
            /// type — in which case the grid-header entity-type filter and
            /// the modal's entity-type picker are both hidden.
            /// </summary>
            public bool IsFixed => ParentCategoryId.HasValue || EntityTypeId > 0;
        }

        #endregion
    }
}
