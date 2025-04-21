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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StreakTypeExclusionDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular streak type exclusion.
    /// </summary>

    [DisplayName( "Streak Type Exclusion Detail" )]
    [Category( "Streaks" )]
    [Description( "Displays the details of the given Exclusion for editing." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "0667f91d-e7fc-44e6-a969-eebbf99802b2" )]
    [Rock.SystemGuid.BlockTypeGuid( "d8b2132d-8725-47ff-84cd-c86c163abe4d" )]
    public class StreakTypeExclusionDetail : RockEntityDetailBlockType<StreakTypeExclusion, StreakTypeExclusionBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string StreakTypeId = "StreakTypeId";
            public const string StreakTypeExclusionId = "StreakTypeExclusionId";
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
            var box = new DetailBlockBox<StreakTypeExclusionBag, StreakTypeExclusionDetailOptionsBag>();
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
        private StreakTypeExclusionDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new StreakTypeExclusionDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the StreakTypeExclusion for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="streakTypeExclusion">The StreakTypeExclusion to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the StreakTypeExclusion is valid, <c>false</c> otherwise.</returns>
        private bool ValidateStreakTypeExclusion( StreakTypeExclusion streakTypeExclusion, out string errorMessage )
        {
            errorMessage = null;

            if ( streakTypeExclusion.StreakTypeId == 0 )
            {
                errorMessage = "Streak Type is required.";
                return false;
            }

            if ( !streakTypeExclusion.IsValid )
            {
                errorMessage = string.Join( "</br>", streakTypeExclusion.ValidationResults.Select( v => v.ErrorMessage ) );
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<StreakTypeExclusionBag, StreakTypeExclusionDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {StreakTypeExclusion.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( StreakTypeExclusion.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( StreakTypeExclusion.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="StreakTypeExclusionBag"/> that represents the entity.</returns>
        private StreakTypeExclusionBag GetCommonEntityBag( StreakTypeExclusion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new StreakTypeExclusionBag
            {
                IdKey = entity.IdKey,
                Location = new ViewModels.Utility.ListItemBag() { Text = entity.Location?.Name, Value = entity.Location?.Guid.ToString() },
                StreakType = entity.StreakType.ToListItemBag()
            };
        }

        // <inheritdoc/>
        protected override StreakTypeExclusionBag GetEntityBagForView( StreakTypeExclusion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        // <inheritdoc/>
        protected override StreakTypeExclusionBag GetEntityBagForEdit( StreakTypeExclusion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        // <inheritdoc/>
        protected override bool UpdateEntityFromBox( StreakTypeExclusion entity, ValidPropertiesBox<StreakTypeExclusionBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Location ),
                () => entity.LocationId = box.Bag.Location.GetEntityId<Location>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.StreakType ),
                () => entity.StreakTypeId = GetStreakType( entity )?.Id ?? 0 );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        // <inheritdoc/>
        protected override StreakTypeExclusion GetInitialEntity()
        {
            var entity = GetInitialEntity<StreakTypeExclusion, StreakTypeExclusionService>( RockContext, PageParameterKey.StreakTypeExclusionId );
            if ( entity.Id == 0 )
            {
                var streakType = StreakTypeCache.Get( PageParameter( PageParameterKey.StreakTypeId ), true );
                if ( streakType != null )
                {
                    entity.StreakTypeId = streakType.Id;
                }
            }

            return entity;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var streakType = StreakTypeCache.Get( PageParameter( PageParameterKey.StreakTypeId ), true );
            var queryParams = new Dictionary<string, string>();

            if ( streakType != null )
            {
                queryParams.Add( PageParameterKey.StreakTypeId, streakType.Id.ToString() );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams )
            };
        }

        // <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out StreakTypeExclusion entity, out BlockActionResult error )
        {
            var entityService = new StreakTypeExclusionService( RockContext );
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
                entity = GetInitialEntity();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{StreakTypeExclusion.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${StreakTypeExclusion.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the actual enrollment model for deleting or editing
        /// </summary>
        /// <param name="exclusion">The exclusion.</param>
        /// <returns></returns>
        private StreakType GetStreakType( StreakTypeExclusion exclusion )
        {
            StreakType streakType = null;
            var streakTypeService = new StreakTypeService( RockContext );

            if ( exclusion?.StreakType != null )
            {
                streakType = exclusion.StreakType;
            }
            else if ( exclusion != null && exclusion.StreakTypeId > 0 )
            {
                streakType = streakTypeService.Get( exclusion.StreakTypeId );
            }
            else
            {
                var streakTypeIdParam = PageParameter( PageParameterKey.StreakTypeId );
                var streakTypeId = Rock.Utility.IdHasher.Instance.GetId( streakTypeIdParam ) ?? streakTypeIdParam.AsIntegerOrNull();

                if ( streakTypeId > 0 )
                {
                    streakType = streakTypeService.Get( streakTypeId.Value );
                }
            }

            return streakType;
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

            return ActionOk( new ValidPropertiesBox<StreakTypeExclusionBag>
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
        public BlockActionResult Save( ValidPropertiesBox<StreakTypeExclusionBag> box )
        {
            var entityService = new StreakTypeExclusionService( RockContext );

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
            if ( !ValidateStreakTypeExclusion( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );

                var currentPerson = GetCurrentPerson();

                if ( !entity.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    entity.AllowPerson( Authorization.VIEW, currentPerson, RockContext );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    entity.AllowPerson( Authorization.EDIT, currentPerson, RockContext );
                }

                if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) )
                {
                    entity.AllowPerson( Authorization.ADMINISTRATE, currentPerson, RockContext );
                }

            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.StreakTypeExclusionId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<StreakTypeExclusionBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new StreakTypeExclusionService( RockContext );

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

            return ActionOk( this.GetParentPageUrl( new Dictionary<string, string> {
                    { PageParameterKey.StreakTypeId, entity.StreakTypeId.ToString() }
            } ) );
        }

        #endregion
    }
}
