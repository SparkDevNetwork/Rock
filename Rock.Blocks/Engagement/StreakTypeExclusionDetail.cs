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
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StreakTypeExclusionDetail;
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
    public class StreakTypeExclusionDetail : RockDetailBlockType
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
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<StreakTypeExclusionBag, StreakTypeExclusionDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<StreakTypeExclusion>();

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
        private StreakTypeExclusionDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new StreakTypeExclusionDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the StreakTypeExclusion for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="streakTypeExclusion">The StreakTypeExclusion to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the StreakTypeExclusion is valid, <c>false</c> otherwise.</returns>
        private bool ValidateStreakTypeExclusion( StreakTypeExclusion streakTypeExclusion, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<StreakTypeExclusionBag, StreakTypeExclusionDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {StreakTypeExclusion.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( StreakTypeExclusion.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( StreakTypeExclusion.FriendlyTypeName );
                }
            }
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

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="StreakTypeExclusionBag"/> that represents the entity.</returns>
        private StreakTypeExclusionBag GetEntityBagForView( StreakTypeExclusion entity )
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
        /// <returns>A <see cref="StreakTypeExclusionBag"/> that represents the entity.</returns>
        private StreakTypeExclusionBag GetEntityBagForEdit( StreakTypeExclusion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( StreakTypeExclusion entity, DetailBlockBox<StreakTypeExclusionBag, StreakTypeExclusionDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Location ),
                () => entity.LocationId = box.Entity.Location.GetEntityId<Location>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.StreakType ),
                () => entity.StreakTypeId = GetStreakType( rockContext, entity )?.Id ?? 0 );

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
        /// <returns>The <see cref="StreakTypeExclusion"/> to be viewed or edited on the page.</returns>
        private StreakTypeExclusion GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<StreakTypeExclusion, StreakTypeExclusionService>( rockContext, PageParameterKey.StreakTypeExclusionId );
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
        private string GetSecurityGrantToken( StreakTypeExclusion entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out StreakTypeExclusion entity, out BlockActionResult error )
        {
            var entityService = new StreakTypeExclusionService( rockContext );
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
                entity = new StreakTypeExclusion();
                entity.StreakTypeId = PageParameter( PageParameterKey.StreakTypeId ).AsInteger();
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
        /// <param name="rockContext">The rock context.</param>
        /// <param name="exclusion">The exclusion.</param>
        /// <returns></returns>
        private StreakType GetStreakType( RockContext rockContext, StreakTypeExclusion exclusion )
        {
            StreakType streakType = null;
            var streakTypeService = new StreakTypeService( rockContext );

            if ( exclusion != null && exclusion.StreakType != null )
            {
                streakType = exclusion.StreakType;
            }
            else if ( exclusion != null )
            {
                streakType = streakTypeService.Get( exclusion.StreakTypeId );
            }
            else
            {
                var streakTypeId = PageParameter( PageParameterKey.StreakTypeId ).AsIntegerOrNull();

                if ( streakTypeId.HasValue && streakTypeId.Value > 0 )
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<StreakTypeExclusionBag, StreakTypeExclusionDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
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
        public BlockActionResult Save( DetailBlockBox<StreakTypeExclusionBag, StreakTypeExclusionDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new StreakTypeExclusionService( rockContext );

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
                if ( !ValidateStreakTypeExclusion( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );

                    var currentPerson = GetCurrentPerson();

                    if ( !entity.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        entity.AllowPerson( Authorization.VIEW, currentPerson, rockContext );
                    }

                    if ( !entity.IsAuthorized( Authorization.EDIT, currentPerson ) )
                    {
                        entity.AllowPerson( Authorization.EDIT, currentPerson, rockContext );
                    }

                    if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) )
                    {
                        entity.AllowPerson( Authorization.ADMINISTRATE, currentPerson, rockContext );
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
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity ) );
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
                var entityService = new StreakTypeExclusionService( rockContext );

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

                return ActionOk( this.GetParentPageUrl( new Dictionary<string, string> {
                    { PageParameterKey.StreakTypeId, entity.StreakTypeId.ToString() }
                } ) );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<StreakTypeExclusionBag, StreakTypeExclusionDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<StreakTypeExclusionBag, StreakTypeExclusionDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
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
