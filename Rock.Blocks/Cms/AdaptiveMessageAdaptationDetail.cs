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
using System.IO;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.AdaptiveMessageAdaptationDetail;
using Rock.ViewModels.Blocks.Cms.LayoutDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular adaptive message adaptation.
    /// </summary>

    [DisplayName( "Adaptive Message Adaptation Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular adaptive message adaptation." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "005292c8-6af7-4250-b29f-759047243baf" )]
    [Rock.SystemGuid.BlockTypeGuid( "113c4223-19b9-46f2-aae8-ac646bc5a3c7" )]
    public class AdaptiveMessageAdaptationDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string AdaptiveMessageAdaptationId = "AdaptiveMessageAdaptationId";
            public const string AdaptiveMessageId = "AdaptiveMessageId";
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
                var box = new DetailBlockBox<AdaptiveMessageAdaptationBag, AdaptiveMessageAdaptationDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<AdaptiveMessageAdaptation>();

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
        private AdaptiveMessageAdaptationDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new AdaptiveMessageAdaptationDetailOptionsBag();
            options.SegmentOptions = new List<ViewModels.Utility.ListItemBag>();

            if ( isEditable )
            {
                var segments = PersonalizationSegmentCache.All();
                foreach ( var item in segments.OrderBy( a => a.Name ) )
                {
                    options.SegmentOptions.Add( new ViewModels.Utility.ListItemBag() { Text = item.Name, Value = item.Guid.ToString() } );
                }
            }

            return options;
        }

        /// <summary>
        /// Validates the AdaptiveMessageAdaptation for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="adaptiveMessageAdaptation">The AdaptiveMessageAdaptation to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AdaptiveMessageAdaptation is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAdaptiveMessageAdaptation( AdaptiveMessageAdaptation adaptiveMessageAdaptation, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<AdaptiveMessageAdaptationBag, AdaptiveMessageAdaptationDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AdaptiveMessageAdaptation.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id == 0 )
            {
                var adaptiveMessageKey = RequestContext.GetPageParameter( PageParameterKey.AdaptiveMessageId );
                var adaptiveMessage = new AdaptiveMessageService( rockContext ).Get( adaptiveMessageKey );
                if ( adaptiveMessage != null )
                {
                    entity.AdaptiveMessageId = adaptiveMessage.Id;
                }
            }

            entity.LoadAttributes( rockContext );

            // New entity is being created, prepare for edit mode by default.
            if ( box.IsEditable )
            {
                box.Entity = GetEntityBagForEdit( entity );
                box.SecurityGrantToken = GetSecurityGrantToken( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AdaptiveMessageAdaptation.FriendlyTypeName );
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AdaptiveMessageAdaptationBag"/> that represents the entity.</returns>
        private AdaptiveMessageAdaptationBag GetCommonEntityBag( AdaptiveMessageAdaptation entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new AdaptiveMessageAdaptationBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IsActive = entity.IsActive,
                Name = entity.Name,
                Order = entity.Order,
                ViewSaturationCount = entity.ViewSaturationCount,
                ViewSaturationInDays = entity.ViewSaturationInDays,
                Segments = entity.AdaptiveMessageAdaptationSegments.Select( a => a.PersonalizationSegment.Guid.ToString() ).ToList()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="AdaptiveMessageAdaptationBag"/> that represents the entity.</returns>
        private AdaptiveMessageAdaptationBag GetEntityBagForView( AdaptiveMessageAdaptation entity )
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
        /// <returns>A <see cref="AdaptiveMessageAdaptationBag"/> that represents the entity.</returns>
        private AdaptiveMessageAdaptationBag GetEntityBagForEdit( AdaptiveMessageAdaptation entity )
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
        private bool UpdateEntityFromBox( AdaptiveMessageAdaptation entity, DetailBlockBox<AdaptiveMessageAdaptationBag, AdaptiveMessageAdaptationDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.ViewSaturationCount ),
                () => entity.ViewSaturationCount = box.Entity.ViewSaturationCount );

            box.IfValidProperty( nameof( box.Entity.ViewSaturationInDays ),
                () => entity.ViewSaturationInDays = box.Entity.ViewSaturationInDays );

            box.IfValidProperty( nameof( box.Entity.Segments ),
                () =>
                {
                    var segmentIds = box.Entity.Segments.AsGuidList().Select( a => PersonalizationSegmentCache.GetId( a ) ).Where( a => a.HasValue ).ToList();

                    var itemsToAdd = segmentIds.Where( a => !entity.AdaptiveMessageAdaptationSegments.Any( b => b.PersonalizationSegmentId == a ) );
                    foreach ( var segmentId in itemsToAdd )
                    {
                        entity.AdaptiveMessageAdaptationSegments.Add( new AdaptiveMessageAdaptationSegment { PersonalizationSegmentId = segmentId.Value } );
                    }
                } );

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
        /// <returns>The <see cref="AdaptiveMessageAdaptation"/> to be viewed or edited on the page.</returns>
        private AdaptiveMessageAdaptation GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<AdaptiveMessageAdaptation, AdaptiveMessageAdaptationService>( rockContext, PageParameterKey.AdaptiveMessageAdaptationId );
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
        private string GetSecurityGrantToken( AdaptiveMessageAdaptation entity )
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
        public bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out AdaptiveMessageAdaptation entity, out BlockActionResult error )
        {
            var entityService = new AdaptiveMessageAdaptationService( rockContext );
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
                entity = new AdaptiveMessageAdaptation();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AdaptiveMessageAdaptation.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AdaptiveMessageAdaptation.FriendlyTypeName}." );
                return false;
            }

            return true;
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

                var box = new DetailBlockBox<AdaptiveMessageAdaptationBag, AdaptiveMessageAdaptationDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<AdaptiveMessageAdaptationBag, AdaptiveMessageAdaptationDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new AdaptiveMessageAdaptationService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var isNew = entity.Id == 0;
                if ( isNew )
                {
                    var adaptiveMessageKey = RequestContext.GetPageParameter( PageParameterKey.AdaptiveMessageId );
                    entity.AdaptiveMessageId = new AdaptiveMessageService( rockContext ).Get( adaptiveMessageKey ).Id;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateAdaptiveMessageAdaptation( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var segmentIds = box.Entity.Segments.AsGuidList().Select( a => PersonalizationSegmentCache.GetId( a ) ).Where( a => a.HasValue ).ToList();
                var itemsToDelete = entity.AdaptiveMessageAdaptationSegments.Where( a => !segmentIds.Contains( a.PersonalizationSegmentId ) ).ToList();
                entity.AdaptiveMessageAdaptationSegments.RemoveAll( itemsToDelete );
                var adaptiveMessageAdaptationSegmentService = new AdaptiveMessageAdaptationSegmentService( rockContext );
                adaptiveMessageAdaptationSegmentService.DeleteRange( itemsToDelete );
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    var adaptiveMessage = new AdaptiveMessageService( rockContext ).Get( entity.AdaptiveMessageId );
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetParentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.AdaptiveMessageId] = adaptiveMessage.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.AdaptiveMessageId] = entity.AdaptiveMessage.IdKey
                } ) );
                ;
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
                var entityService = new AdaptiveMessageAdaptationService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<AdaptiveMessageAdaptationBag, AdaptiveMessageAdaptationDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<AdaptiveMessageAdaptationBag, AdaptiveMessageAdaptationDetailOptionsBag>
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
