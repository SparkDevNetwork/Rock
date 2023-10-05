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
    public class BinaryFileTypeDetail : RockDetailBlockType
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
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<BinaryFileTypeBag, BinaryFileTypeDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<BinaryFileType>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private BinaryFileTypeDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new BinaryFileTypeDetailOptionsBag();
            options.PreferredColorDepthOptions = typeof( ColorDepth ).ToEnumListItemBag();
            options.PreferredFormatOptions = typeof( Format ).ToEnumListItemBag();
            options.PreferredResolutionOptions = typeof( Resolution ).ToEnumListItemBag();
            return options;
        }

        /// <summary>
        /// Validates the BinaryFileType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="binaryFileType">The BinaryFileType to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the BinaryFileType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateBinaryFileType( BinaryFileType binaryFileType, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<BinaryFileTypeBag, BinaryFileTypeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {BinaryFileType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
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
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( BinaryFileType.FriendlyTypeName );
                }
            }
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
                CacheControlHeaderSettings = ToCacheabilityBag( cacheability ),
                CacheToServerFileSystem = entity.CacheToServerFileSystem,
                Description = entity.Description,
                IconCssClass = entity.IconCssClass,
                IsSystem = entity.IsSystem,
                MaxFileSizeBytes = entity.MaxFileSizeBytes,
                MaxHeight = entity.MaxHeight,
                MaxWidth = entity.MaxWidth,
                Name = entity.Name,
                PreferredColorDepth = entity.PreferredColorDepth,
                PreferredFormat = entity.PreferredFormat,
                PreferredRequired = entity.PreferredRequired,
                PreferredResolution = entity.PreferredResolution,
                RequiresViewSecurity = entity.RequiresViewSecurity,
                StorageEntityType = entity.StorageEntityType.ToListItemBag()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="BinaryFileTypeBag"/> that represents the entity.</returns>
        private BinaryFileTypeBag GetEntityBagForView( BinaryFileType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="BinaryFileTypeBag"/> that represents the entity.</returns>
        private BinaryFileTypeBag GetEntityBagForEdit( BinaryFileType entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );
            bag.BinaryFileTypeAttributes = GetAttributes( entity, rockContext ).ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttributeViewModel( a ) );

            if ( entity.IsSystem )
            {
                bag.RestrictedEdit = true;
                bag.EditModeMessage = EditModeMessage.System( BinaryFileType.FriendlyTypeName );
            }

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( BinaryFileType entity, DetailBlockBox<BinaryFileTypeBag, BinaryFileTypeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.CacheControlHeaderSettings ),
                () => entity.CacheControlHeaderSettings = ToCacheability( box.Entity.CacheControlHeaderSettings ).ToJson() );

            box.IfValidProperty( nameof( box.Entity.CacheToServerFileSystem ),
                () => entity.CacheToServerFileSystem = box.Entity.CacheToServerFileSystem );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IconCssClass ),
                () => entity.IconCssClass = box.Entity.IconCssClass );

            box.IfValidProperty( nameof( box.Entity.MaxFileSizeBytes ),
                () => entity.MaxFileSizeBytes = box.Entity.MaxFileSizeBytes );

            box.IfValidProperty( nameof( box.Entity.MaxHeight ),
                () => entity.MaxHeight = box.Entity.MaxHeight );

            box.IfValidProperty( nameof( box.Entity.MaxWidth ),
                () => entity.MaxWidth = box.Entity.MaxWidth );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.PreferredColorDepth ),
                () => entity.PreferredColorDepth = box.Entity.PreferredColorDepth );

            box.IfValidProperty( nameof( box.Entity.PreferredFormat ),
                () => entity.PreferredFormat = box.Entity.PreferredFormat );

            box.IfValidProperty( nameof( box.Entity.PreferredRequired ),
                () => entity.PreferredRequired = box.Entity.PreferredRequired );

            box.IfValidProperty( nameof( box.Entity.PreferredResolution ),
                () => entity.PreferredResolution = box.Entity.PreferredResolution );

            box.IfValidProperty( nameof( box.Entity.RequiresViewSecurity ),
                () => entity.RequiresViewSecurity = box.Entity.RequiresViewSecurity );

            box.IfValidProperty( nameof( box.Entity.StorageEntityType ),
                () => entity.StorageEntityTypeId = box.Entity.StorageEntityType.GetEntityId<EntityType>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="BinaryFileType"/> to be viewed or edited on the page.</returns>
        private BinaryFileType GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<BinaryFileType, BinaryFileTypeService>( rockContext, PageParameterKey.BinaryFileTypeId );
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
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( BinaryFileType entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out BinaryFileType entity, out BlockActionResult error )
        {
            var entityService = new BinaryFileTypeService( rockContext );
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
        /// Converts the <see cref="RockCacheability"/> to a <see cref="RockCacheabilityBag"/>.
        /// </summary>
        /// <param name="cacheability">The cacheability.</param>
        /// <returns></returns>
        private RockCacheabilityBag ToCacheabilityBag( RockCacheability cacheability )
        {
            var bag = new RockCacheabilityBag()
            {
                MaxAge = new TimeIntervalBag
                {
                    Unit = TimeIntervalUnit.Minutes,
                    Value = 0
                },
                RockCacheabilityType = RockCacheablityType.Public.ConvertToInt(),
                SharedMaxAge = new TimeIntervalBag
                {
                    Unit = TimeIntervalUnit.Minutes,
                    Value = 0
                }
            };

            if ( cacheability == null )
            {
                return bag;
            }

            if ( cacheability.MaxAge != null )
            {
                bag.MaxAge = new TimeIntervalBag
                {
                    Unit = cacheability.MaxAge.Unit,
                    Value = cacheability.MaxAge.Value
                };
            }

            if ( cacheability.RockCacheablityType == RockCacheablityType.Private )
            {
                bag.RockCacheabilityType = cacheability.RockCacheablityType.ConvertToInt();
            }
            else
            {
                bag.RockCacheabilityType = cacheability.RockCacheablityType.ConvertToInt();

                if ( cacheability.SharedMaxAge != null )
                {
                    bag.SharedMaxAge = new TimeIntervalBag
                    {
                        Unit = cacheability.SharedMaxAge.Unit,
                        Value = cacheability.SharedMaxAge.Value
                    };
                }
            }

            return bag;
        }

        /// <summary>
        /// Converts the <see cref="RockCacheabilityBag"/> to a <see cref="RockCacheability"/>.
        /// </summary>
        /// <param name="cacheability">The cacheability.</param>
        /// <returns></returns>
        private static RockCacheability ToCacheability( RockCacheabilityBag cacheControlHeaderSettings )
        {
            var cacheability = new RockCacheability()
            {
                MaxAge = new TimeInterval
                {
                    Unit = cacheControlHeaderSettings.MaxAge.Unit,
                    Value = cacheControlHeaderSettings.MaxAge.Value,
                },
                RockCacheablityType = ( RockCacheablityType ) cacheControlHeaderSettings.RockCacheabilityType,
            };

            if ( cacheability.RockCacheablityType == RockCacheablityType.Public )
            {
                cacheability.SharedMaxAge = new TimeInterval
                {
                    Unit = cacheControlHeaderSettings.SharedMaxAge.Unit,
                    Value = cacheControlHeaderSettings.SharedMaxAge.Value,
                };
            }

            return cacheability;
        }

        /// <summary>
        /// Save attributes associated with this BinaryFileType.
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="qualifierColumn"></param>
        /// <param name="qualifierValue"></param>
        /// <param name="viewStateAttributes"></param>
        /// <param name="rockContext"></param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> viewStateAttributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        private List<Rock.Model.Attribute> GetAttributes( BinaryFileType entity, RockContext rockContext )
        {
            string qualifierValue = entity.Id.ToString();
            var attributeService = new AttributeService( rockContext );
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<BinaryFileTypeBag, BinaryFileTypeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<BinaryFileTypeBag, BinaryFileTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateBinaryFileType( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                SaveAttributes( new BinaryFile().TypeId, "BinaryFileTypeId", entity.Id.ToString(), box.Entity.BinaryFileTypeAttributes, rockContext );

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                return ActionOk( this.GetParentPageUrl() );
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
            using ( var rockContext = new RockContext() )
            {
                var entityService = new BinaryFileTypeService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<BinaryFileTypeBag, BinaryFileTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<BinaryFileTypeBag, BinaryFileTypeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
