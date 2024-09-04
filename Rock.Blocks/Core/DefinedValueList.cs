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
using Rock.ViewModels.Blocks.Core.CategoryDetail;
using Rock.ViewModels.Blocks.Core.DefinedValueList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of defined values.
    /// </summary>
    [DisplayName( "Defined Value List" )]
    [Category( "Core" )]
    [Description( "Block for viewing values for a defined type." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [DefinedTypeField( "Defined Type",
        Description = "If a Defined Type is set, only its Defined Values will be displayed (regardless of the querystring parameters).",
        IsRequired = false,
        Key = AttributeKey.DefinedType )]

    [Rock.SystemGuid.EntityTypeGuid( "710916bd-4bc1-4d05-b088-381394351b53" )]
    [Rock.SystemGuid.BlockTypeGuid( "f431f950-f007-493e-81c8-16559fe4c0f0" )]
    [CustomizedGrid]
    public class DefinedValueList : RockEntityListBlockType<DefinedValue>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DefinedType = "DefinedType";
        }

        private static class PageParameterKey
        {
            public const string DefinedTypeId = "DefinedTypeId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Cached value of the current Defined Type, should be access via the <see cref="GetDefinedType(RockContext)"/> method.
        /// </summary>
        private DefinedType _definedType;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<DefinedValueListOptionsBag>();
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
        private DefinedValueListOptionsBag GetBoxOptions()
        {
            var definedType = GetDefinedType();
            var options = new DefinedValueListOptionsBag()
            {
                IsBlockVisible = definedType != null,
                IsCategorizedValuesEnabled = definedType?.CategorizedValuesEnabled == true,
                DefinedTypeName = definedType?.Name,
                IsSecurityOnValuesEnabled = definedType?.EnableSecurityOnValues == true,
                EntityTypeQualifierValue = definedType?.Id.ToString()
            };
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<DefinedValue> GetListQueryable( RockContext rockContext )
        {
            var definedType = GetDefinedType();
            IEnumerable<DefinedValue> definedValues = new List<DefinedValue>();

            if ( definedType != null )
            {
                definedValues = new DefinedValueService( rockContext ).Queryable()
                    .Include( dv => dv.Category )
                    .Where( dv => dv.DefinedTypeId == definedType.Id );
            }

            return definedValues.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<DefinedValue> GetOrderedListQueryable( IQueryable<DefinedValue> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( dv => dv.Order );
        }

        /// <inheritdoc/>
        protected override GridBuilder<DefinedValue> GetGridBuilder()
        {
            var definedType = GetDefinedType();

            return new GridBuilder<DefinedValue>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "value", a => a.Value )
                .AddTextField( "description", a => a.Description )
                .AddField( "isActive", a => a.IsActive )
                .AddTextField( "category", a => a.Category?.Name )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "isSecurityDisabled", _ => definedType?.EnableSecurityOnValues != true )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <inheritdoc/>
        protected override List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<DefinedValue>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                var definedType = GetDefinedType();
                return AttributeCache.GetOrderedGridAttributes( entityTypeId, "DefinedTypeId", definedType?.Id.ToString() );
            }

            return new List<AttributeCache>();
        }

        /// <summary>
        /// Gets the specified Defined Type either from the block settings or query params.
        /// </summary>
        /// <returns></returns>
        private DefinedType GetDefinedType()
        {
            if ( _definedType == null )
            {
                var definedTypeService = new DefinedTypeService( RockContext );

                // A configured defined type takes precedence over any definedTypeId param value that is passed in.
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.DefinedType ), out Guid definedTypeGuid ) )
                {
                    _definedType = definedTypeService.GetInclude( definedTypeGuid, dt => dt.Category );
                }
                else
                {
                    _definedType = definedTypeService.GetInclude( PageParameter( PageParameterKey.DefinedTypeId ), dt => dt.Category );
                }
            }

            return _definedType;
        }

        /// <summary>
        /// Gets the bag for editing the Defined Value.
        /// </summary>
        /// <param name="entity">The entity to be represented for editing purposes.</param>
        /// <returns>A <see cref="DefinedValueBag"/> that represents the entity.</returns>
        private DefinedValueBag GetEntityBagForEdit( DefinedValue entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new DefinedValueBag()
            {
                Category = entity.Category.ToListItemBag(),
                Description = entity.Description,
                IdKey = entity.IdKey,
                IsActive = entity.IsActive,
                Value = entity.Value
            };

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
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
            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );

            // Get the entities from the database.
            var items = GetListItems( qry, RockContext );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new DefinedValueService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{DefinedValue.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {DefinedValue.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the specified entity for editing.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${DefinedValue.FriendlyTypeName}." );
            }

            var entityService = new DefinedValueService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                var definedType = GetDefinedType();

                entity = new DefinedValue
                {
                    Id = 0,
                    DefinedTypeId = definedType.Id,
                    IsSystem = false
                };
            }

            entity.LoadAttributes();

            return ActionOk( GetEntityBagForEdit( entity ) );
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="bag">The bag that contains all the information required to save.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( DefinedValueBag bag )
        {
            var definedType = GetDefinedType();
            var entityService = new DefinedValueService( RockContext );
            DefinedValue entity;

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${DefinedValue.FriendlyTypeName}." );
            }

            if ( bag.IdKey.IsNullOrWhiteSpace() )
            {
                entity = new DefinedValue
                {
                    Id = 0,
                    DefinedTypeId = definedType.Id,
                    IsSystem = false
                };

                var orders = entityService.Queryable()
                    .Where( d => d.DefinedTypeId == definedType.Id )
                    .Select( d => d.Order )
                    .ToList();

                entity.Order = orders.Any() ? orders.Max() + 1 : 0;
            }
            else
            {
                entity = entityService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            if ( entity == null )
            {
                return ActionBadRequest( $"{DefinedValue.FriendlyTypeName} not found." );
            }

            entity.LoadAttributes( RockContext );

            entity.Value = bag.Value;
            entity.Description = bag.Description;
            entity.IsActive = bag.IsActive;
            entity.CategoryId = bag.Category.GetEntityId<Category>( RockContext );

            if ( bag.AttributeValues != null )
            {
                entity.SetPublicAttributeValues( bag.AttributeValues, RequestContext.CurrentPerson );
            }

            if ( !entity.IsValid )
            {
                return ActionBadRequest( entity.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() );
            }

            RockContext.WrapTransaction( () =>
            {
                if ( entity.Id.Equals( 0 ) )
                {
                    entityService.Add( entity );
                }

                RockContext.SaveChanges();

                entity.SaveAttributeValues( RockContext );
            } );

            return ActionOk();
        }

        #endregion
    }
}
