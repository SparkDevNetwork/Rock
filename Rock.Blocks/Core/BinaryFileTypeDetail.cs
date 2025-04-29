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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.BinaryFileTypeDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular binary file type.
    /// </summary>
    [DisplayName( "Binary File Type Detail" )]
    [Category( "Core" )]
    [Description( "Displays all details of a binary file type." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b2c1f7f4-4810-4b34-9fb6-9e6d6debe4c9" )]
    [Rock.SystemGuid.BlockTypeGuid( "dabf690b-be17-4821-a13e-44c7c8d587cd" )]
    public class BinaryFileTypeDetail : RockEntityDetailBlockType<BinaryFileType, BinaryFileTypeBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string BinaryFileTypeId = "BinaryFileTypeId";
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
            var box = new DetailBlockBox<BinaryFileTypeBag, BinaryFileTypeDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private BinaryFileTypeDetailOptionsBag GetBoxOptions()
        {
            var options = new BinaryFileTypeDetailOptionsBag();
            return options;
        }

        /// <summary>
        /// Validates the BinaryFileType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="binaryFileType">The BinaryFileType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the BinaryFileType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateBinaryFileType( BinaryFileType binaryFileType, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<BinaryFileTypeBag, BinaryFileTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {BinaryFileType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( BinaryFileType.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( BinaryFileType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="BinaryFileTypeBag"/> that represents the entity.</returns>
        private BinaryFileTypeBag GetCommonEntityBag( BinaryFileType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var cacheability = entity.CacheControlHeaderSettings.FromJsonOrNull<RockCacheability>();

            return new BinaryFileTypeBag
            {
                IdKey = entity.IdKey,
                AllowAnonymous = entity.AllowAnonymous,
                CacheControlHeaderSettings = cacheability.ToCacheabilityBag(),
                CacheToServerFileSystem = entity.CacheToServerFileSystem,
                Description = entity.Description,
                IconCssClass = entity.IconCssClass,
                IsSystem = entity.IsSystem,
                MaxFileSizeBytes = entity.MaxFileSizeBytes,
                MaxHeight = entity.MaxHeight,
                MaxWidth = entity.MaxWidth,
                Name = entity.Name,
                PreferredRequired = entity.PreferredRequired,
                RequiresViewSecurity = entity.RequiresViewSecurity,
                StorageEntityType = entity.StorageEntityType.ToListItemBag()
            };
        }

        /// <inheritdoc/>
        protected override BinaryFileTypeBag GetEntityBagForView( BinaryFileType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        /// <inheritdoc/>
        protected override BinaryFileTypeBag GetEntityBagForEdit( BinaryFileType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false );
            bag.BinaryFileTypeAttributes = GetAttributes( entity ).ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );

            if ( entity.IsSystem )
            {
                bag.RestrictedEdit = true;
                bag.EditModeMessage = EditModeMessage.System( BinaryFileType.FriendlyTypeName );
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( BinaryFileType entity, ValidPropertiesBox<BinaryFileTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AllowAnonymous ),
                () => entity.AllowAnonymous = box.Bag.AllowAnonymous );

            box.IfValidProperty( nameof( box.Bag.CacheControlHeaderSettings ),
                () => entity.CacheControlHeaderSettings = box.Bag.CacheControlHeaderSettings.ToCacheability()?.ToJson() );

            box.IfValidProperty( nameof( box.Bag.CacheToServerFileSystem ),
                () => entity.CacheToServerFileSystem = box.Bag.CacheToServerFileSystem );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.MaxFileSizeBytes ),
                () => entity.MaxFileSizeBytes = box.Bag.MaxFileSizeBytes );

            box.IfValidProperty( nameof( box.Bag.MaxHeight ),
                () => entity.MaxHeight = box.Bag.MaxHeight );

            box.IfValidProperty( nameof( box.Bag.MaxWidth ),
                () => entity.MaxWidth = box.Bag.MaxWidth );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.PreferredRequired ),
                () => entity.PreferredRequired = box.Bag.PreferredRequired );

            box.IfValidProperty( nameof( box.Bag.RequiresViewSecurity ),
                () => entity.RequiresViewSecurity = box.Bag.RequiresViewSecurity );

            box.IfValidProperty( nameof( box.Bag.StorageEntityType ),
                () => entity.StorageEntityTypeId = box.Bag.StorageEntityType.GetEntityId<EntityType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override BinaryFileType GetInitialEntity()
        {
            return GetInitialEntity<BinaryFileType, BinaryFileTypeService>( RockContext, PageParameterKey.BinaryFileTypeId );
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
        protected override bool TryGetEntityForEditAction( string idKey,  out BinaryFileType entity, out BlockActionResult error )
        {
            var entityService = new BinaryFileTypeService( RockContext );
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
                entity = new BinaryFileType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{BinaryFileType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${BinaryFileType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save attributes associated with this BinaryFileType.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="qualifierColumn">The qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="viewStateAttributes">The view state attributes.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> viewStateAttributes )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( RockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                RockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, RockContext );
            }
        }

        /// <summary>
        /// Gets the attributes associated with this BinaryFileType.
        /// </summary>
        /// <param name="entity">The BinaryFileType.</param>
        /// <returns></returns>
        private List<Rock.Model.Attribute> GetAttributes( BinaryFileType entity )
        {
            string qualifierValue = entity.Id.ToString();
            var attributeService = new AttributeService( RockContext );
            var qryBinaryFileAttributes = attributeService.GetByEntityTypeId( new BinaryFile().TypeId, true ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "BinaryFileTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            return qryBinaryFileAttributes.ToList();
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

            return ActionOk( new ValidPropertiesBox<BinaryFileTypeBag>
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
        public BlockActionResult Save( ValidPropertiesBox<BinaryFileTypeBag> box )
        {
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
            if ( !ValidateBinaryFileType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );

                SaveAttributes( new BinaryFile().TypeId, "BinaryFileTypeId", entity.Id.ToString(), box.Bag.BinaryFileTypeAttributes );
            } );

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new BinaryFileTypeService( RockContext );

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
