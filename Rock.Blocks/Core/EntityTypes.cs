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
using Rock.ViewModels.Blocks.Core.EntityTypes;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of entity types.
    /// </summary>
    [DisplayName( "Entity Types" )]
    [Category( "Core" )]
    [Description( "Displays a list of entity types." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "712dd8aa-aafa-4e92-b804-d884b6848adf" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "3136cb51-8b94-4116-9c34-59ba6425a1ca" )]
    [Rock.SystemGuid.BlockTypeGuid( "8098DF5D-4B87-4FAF-BA65-E017C5A93353" )]
    [CustomizedGrid]
    public class EntityTypes : RockListBlockType<EntityTypesBag>
    {
        #region Keys

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<EntityTypesOptionsBag>();
            var builder = GetGridBuilder();

            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();

            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private EntityTypesOptionsBag GetBoxOptions()
        {
            var options = new EntityTypesOptionsBag();
            options.IsAuthorizedToEdit = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<EntityTypesBag> GetListQueryable( RockContext rockContext )
        {
            var entityTypeService = new EntityTypeService( rockContext );
            var data = entityTypeService.Queryable().Where( e => e.IsEntity ).Select( e => new EntityTypesBag
                {
                    Id = e.Id,
                    Name = e.Name,
                    FriendlyName = e.FriendlyName,
                    IsCommon = e.IsCommon,
                    IsSecured = e.IsSecured
            } );

            return data;
        }

        /// <inheritdoc/>
        protected override IQueryable<EntityTypesBag> GetOrderedListQueryable( IQueryable<EntityTypesBag> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( p => p.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<EntityTypesBag> GetGridBuilder()
        {
            return new GridBuilder<EntityTypesBag>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.Id.AsIdKey() )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "friendlyName", a => a.FriendlyName )
                .AddField( "isCommon", a => a.IsCommon )
                .AddField( "isSecurityDisabled", a => !a.IsSecured );
        }

        /// <summary>
        /// Attempt to get entity type that is being updated
        /// </summary>
        /// <param name="idKey"></param>
        /// <param name="entity"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        protected bool TryGetEntityForEditAction( string idKey, out EntityType entity, out BlockActionResult error )
        {
            var entityTypeService = new EntityTypeService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityTypeService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                entity = null;
                return false;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{EntityType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${EntityType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check each provided property is valid and if so update the entity with the new value.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="box"></param>
        /// <returns></returns>
        private bool UpdateEntityFromBox( EntityType entity, ValidPropertiesBox<EntityTypesBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.FriendlyName ),
                () => entity.FriendlyName = box.Bag.FriendlyName );

            box.IfValidProperty( nameof( box.Bag.IsCommon ),
                () => entity.IsCommon = box.Bag.IsCommon );

            box.IfValidProperty( nameof( box.Bag.IsRelatedToInteractionTrackedOnCreate ),
                () => entity.IsRelatedToInteractionTrackedOnCreate = box.Bag.IsRelatedToInteractionTrackedOnCreate );

            box.IfValidProperty( nameof( box.Bag.IndexResultTemplate ),
                () => entity.IndexResultTemplate = box.Bag.IndexResultTemplate );

            box.IfValidProperty( nameof( box.Bag.IndexDocumentUrl ),
                () => entity.IndexDocumentUrl = box.Bag.IndexDocumentUrl );

            box.IfValidProperty( nameof( box.Bag.LinkUrlLavaTemplate ),
                () => entity.LinkUrlLavaTemplate = box.Bag.LinkUrlLavaTemplate );

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the entity type representation for editing purposes.
        /// </summary>
        /// <param name="key">The unique identifier of the entity type to be edited.</param>
        /// <returns>A response that includes the editable representation of the entity type.</returns>
        [BlockAction]
        public BlockActionResult GetEditEntityType( string key )
        {
            var entityTypeService = new EntityTypeService( RockContext );
            var entity = entityTypeService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{EntityType.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to make changes to this entity type." );
            }

            var editBag = new EntityTypesBag
            {
                Id = entity.Id,
                Name = entity.Name,
                FriendlyName = entity.FriendlyName,
                IsCommon = entity.IsCommon,
                IsRelatedToInteractionTrackedOnCreate = entity.IsRelatedToInteractionTrackedOnCreate,
                IndexResultTemplate = entity.IndexResultTemplate,
                IndexDocumentUrl = entity.IndexDocumentUrl,
                LinkUrlLavaTemplate = entity.LinkUrlLavaTemplate,
            };

            return ActionOk( editBag );
        }

        [BlockAction]
        public BlockActionResult SaveEntityType( ValidPropertiesBox<EntityTypesBag> box )
        {
            var entityTypeService = new EntityTypeService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.Id.AsIdKey(), out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        #endregion

    }
}
