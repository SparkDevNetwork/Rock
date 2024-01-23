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
using Rock.ViewModels.Blocks.Core.SuggestionDetail;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular following suggestion type.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Suggestion Detail" )]
    [Category( "Follow" )]
    [Description( "Block for editing the following suggestion types." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "64e05a6c-90ad-45a8-8ca2-3e4fe29cbfdb" )]
    [Rock.SystemGuid.BlockTypeGuid( "e18ab976-6665-48a5-b418-8fac8f374135" )]
    public class SuggestionDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string EventId = "EventId";
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
                var box = new SuggestionDetailBox();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();

                return box;
            }
        }

        /// <summary>
        /// Validates the FollowingSuggestionType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="followingSuggestionType">The FollowingSuggestionType to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the FollowingSuggestionType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateFollowingSuggestionType( FollowingSuggestionType followingSuggestionType, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( SuggestionDetailBox box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FollowingSuggestionType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            // Existing entity was found, prepare for view mode by default.
            if ( isViewable )
            {
                box.Entity = GetEntityBag( entity );
                box.SecurityGrantToken = GetSecurityGrantToken( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FollowingSuggestionType.FriendlyTypeName );
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="SuggestionDetailBag"/> that represents the entity.</returns>
        private SuggestionDetailBag GetEntityBag( FollowingSuggestionType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new SuggestionDetailBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                EntityNotificationFormatLava = entity.EntityNotificationFormatLava,
                EntityType = entity.EntityType.ToListItemBag(),
                EntityTypeId = entity.EntityTypeId,
                IsActive = entity.IsActive,
                Name = entity.Name,
                Order = entity.Order,
                ReasonNote = entity.ReasonNote,
                ReminderDays = entity.ReminderDays
            };

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, true, attributeFilter: IsAttributeIncluded );

            return bag;
        }

        private bool IsAttributeIncluded( AttributeCache attribute )
        {
            return attribute.Key != "Order" && attribute.Key != "Active";
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( FollowingSuggestionType entity, SuggestionDetailBox box, RockContext rockContext )
        {
            entity.Description = box.Entity.Description;
            entity.EntityNotificationFormatLava = box.Entity.EntityNotificationFormatLava;
            entity.EntityTypeId = box.Entity.EntityType.GetEntityId<EntityType>( rockContext );
            entity.IsActive = box.Entity.IsActive;
            entity.Name = box.Entity.Name;
            entity.Order = box.Entity.Order;
            entity.ReasonNote = box.Entity.ReasonNote;
            entity.ReminderDays = box.Entity.ReminderDays;

            entity.LoadAttributes( rockContext );

            entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="FollowingSuggestionType"/> to be viewed or edited on the page.</returns>
        private FollowingSuggestionType GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<FollowingSuggestionType, FollowingSuggestionTypeService>( rockContext, PageParameterKey.EventId );
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
        private string GetSecurityGrantToken( FollowingSuggestionType entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out FollowingSuggestionType entity, out BlockActionResult error )
        {
            var entityService = new FollowingSuggestionTypeService( rockContext );
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
                entity = new FollowingSuggestionType();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FollowingSuggestionType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${FollowingSuggestionType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the entity attributes and values.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="idKey">The identifier key.</param>
        [BlockAction]
        public BlockActionResult GetEntityAttributes( Guid? entityTypeGuid, string idKey )
        {
            if ( !entityTypeGuid.HasValue )
            {
                return ActionNotFound();
            }

            var entityType = EntityTypeCache.Get( entityTypeGuid.Value );

            if ( entityType == null )
            {
                return ActionNotFound();
            }

            FollowingSuggestionType entity = new FollowingSuggestionType() { Id = 0, EntityTypeId = entityType.Id, Guid = Guid.Empty };

            if ( !string.IsNullOrWhiteSpace( idKey ) )
            {
                var rockContext = new RockContext();
                var followingSuggestionTypeService = new FollowingSuggestionTypeService( rockContext );
                var existingEntity = followingSuggestionTypeService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( existingEntity.EntityTypeId == entityType.Id )
                {
                    entity = existingEntity;
                }
            }

            entity.LoadAttributes();
            var bag = GetEntityBag( entity );

            return ActionOk( bag );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( SuggestionDetailBox box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FollowingSuggestionTypeService( rockContext );

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
                if ( !ValidateFollowingSuggestionType( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetParentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.EventId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.EventId] = entity.IdKey
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
        public BlockActionResult RefreshAttributes( SuggestionDetailBox box )
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

                var refreshedBox = new SuggestionDetailBox
                {
                    Entity = GetEntityBag( entity )
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
