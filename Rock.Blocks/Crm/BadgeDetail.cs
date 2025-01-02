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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.BadgeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays the details of a particular badge.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Badge Detail" )]
    [Category( "CRM" )]
    [Description( "Displays the details of a particular badge." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "5b57bd74-416d-4fd0-a36b-c74955f4c691" )]
    [Rock.SystemGuid.BlockTypeGuid( "5bd4cd27-c1c1-4e12-8756-9c93e4edb28e" )]
    public class BadgeDetail : RockEntityDetailBlockType<Model.Badge, BadgeBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string BadgeId = "BadgeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<BadgeBag, BadgeDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private BadgeDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new BadgeDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Badge for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="badge">The Badge to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Badge is valid, <c>false</c> otherwise.</returns>
        private bool ValidateBadge( Model.Badge badge, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<BadgeBag, BadgeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Model.Badge.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Model.Badge.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Model.Badge.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="BadgeBag"/> that represents the entity.</returns>
        private BadgeBag GetCommonEntityBag( Model.Badge entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new BadgeBag
            {
                IdKey = entity.IdKey,
                BadgeComponentEntityType = entity.BadgeComponentEntityType.ToListItemBag(),
                Description = entity.Description,
                EntityType = entity.EntityType.ToListItemBag(),
                EntityTypeQualifierColumn = entity.EntityTypeQualifierColumn,
                EntityTypeQualifierValue = entity.EntityTypeQualifierValue,
                IsActive = entity.IsActive,
                Name = entity.Name
            };
        }

        /// <inheritdoc/>
        protected override BadgeBag GetEntityBagForView( Model.Badge entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, true, IsAttributeIncluded );

            return bag;
        }

        /// <inheritdoc/>
        protected override BadgeBag GetEntityBagForEdit( Model.Badge entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, true, IsAttributeIncluded );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Model.Badge entity, ValidPropertiesBox<BadgeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.BadgeComponentEntityType ),
                () => entity.BadgeComponentEntityTypeId = box.Bag.BadgeComponentEntityType.GetEntityId<EntityType>( RockContext ) ?? 0 );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.EntityType ),
                () => entity.EntityTypeId = box.Bag.EntityType.GetEntityId<EntityType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.EntityTypeQualifierColumn ),
                () => entity.EntityTypeQualifierColumn = box.Bag.EntityTypeQualifierColumn );

            box.IfValidProperty( nameof( box.Bag.EntityTypeQualifierValue ),
                () => entity.EntityTypeQualifierValue = box.Bag.EntityTypeQualifierValue );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, true, IsAttributeIncluded );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override Model.Badge GetInitialEntity( )
        {
            return GetInitialEntity<Model.Badge, BadgeService>( RockContext, PageParameterKey.BadgeId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Model.Badge entity, out BlockActionResult error )
        {
            var entityService = new BadgeService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new Model.Badge();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Model.Badge.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Model.Badge.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the attribute should be included in the block.
        /// </summary>
        /// <param name="attribute">The attribute to be checked.</param>
        /// <returns><c>true</c> if the attribute should be included, <c>false</c> otherwise.</returns>
        private bool IsAttributeIncluded( AttributeCache attribute )
        {
            // Don't include the special attributes "Order" and "Active".
            return attribute.Key != "Order" && attribute.Key != "Active";
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<BadgeBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<BadgeBag> box )
        {
            using ( var RockContext = new RockContext() )
            {
                var entityService = new BadgeService( RockContext );

                if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateBadge( entity, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                RockContext.WrapTransaction( () =>
                {
                    RockContext.SaveChanges();
                    entity.SaveAttributeValues( RockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.BadgeId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( RockContext );

                var bag = GetEntityBagForView( entity );

                return ActionOk( new ValidPropertiesBox<BadgeBag>
                {
                    Bag = bag,
                    ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
                } );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new BadgeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
