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
using Rock.ViewModels.Blocks.Cms.AdaptiveMessageAdaptationDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

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
    public class AdaptiveMessageAdaptationDetail : RockEntityDetailBlockType<AdaptiveMessageAdaptation, AdaptiveMessageAdaptationBag>, IBreadCrumbBlock
    {
        private AdaptiveMessageAdaptation SelectedAdaptiveMessageAdaptation { get; set; }

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
            var box = new DetailBlockBox<AdaptiveMessageAdaptationBag, AdaptiveMessageAdaptationDetailOptionsBag>();

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
        private AdaptiveMessageAdaptationDetailOptionsBag GetBoxOptions( bool isEditable )
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
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AdaptiveMessageAdaptation is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAdaptiveMessageAdaptation( AdaptiveMessageAdaptation adaptiveMessageAdaptation, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AdaptiveMessageAdaptationBag, AdaptiveMessageAdaptationDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AdaptiveMessageAdaptation.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id == 0 )
            {
                var adaptiveMessageKey = RequestContext.GetPageParameter( PageParameterKey.AdaptiveMessageId );
                var adaptiveMessage = new AdaptiveMessageService( RockContext ).Get( adaptiveMessageKey );
                if ( adaptiveMessage != null )
                {
                    entity.AdaptiveMessageId = adaptiveMessage.Id;
                }
            }

            entity.LoadAttributes( RockContext );

            // New entity is being created, prepare for edit mode by default.
            if ( box.IsEditable )
            {
                box.Entity = GetEntityBagForEdit( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AdaptiveMessageAdaptation.FriendlyTypeName );
            }

            PrepareDetailBox( box, entity );
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
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Segments = entity.AdaptiveMessageAdaptationSegments.Select( a => a.PersonalizationSegment.Guid.ToString() ).ToList()
            };
        }

        /// <inheritdoc/>
        protected override AdaptiveMessageAdaptationBag GetEntityBagForView( AdaptiveMessageAdaptation entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override AdaptiveMessageAdaptationBag GetEntityBagForEdit( AdaptiveMessageAdaptation entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( AdaptiveMessageAdaptation entity, ValidPropertiesBox<AdaptiveMessageAdaptationBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.ViewSaturationCount ),
                () => entity.ViewSaturationCount = box.Bag.ViewSaturationCount );

            box.IfValidProperty( nameof( box.Bag.ViewSaturationInDays ),
                () => entity.ViewSaturationInDays = box.Bag.ViewSaturationInDays );

            box.IfValidProperty( nameof( box.Bag.StartDate ),
                () => entity.StartDate = box.Bag.StartDate );

            box.IfValidProperty( nameof( box.Bag.EndDate ),
                () => entity.EndDate = box.Bag.EndDate );

            box.IfValidProperty( nameof( box.Bag.ViewSaturationInDays ),
                () => entity.ViewSaturationInDays = box.Bag.ViewSaturationInDays );

            box.IfValidProperty( nameof( box.Bag.Segments ),
                () =>
                {
                    var segmentIds = box.Bag.Segments.AsGuidList().Select( a => PersonalizationSegmentCache.GetId( a ) ).Where( a => a.HasValue ).ToList();

                    var itemsToAdd = segmentIds.Where( a => !entity.AdaptiveMessageAdaptationSegments.Any( b => b.PersonalizationSegmentId == a ) );
                    foreach ( var segmentId in itemsToAdd )
                    {
                        entity.AdaptiveMessageAdaptationSegments.Add( new AdaptiveMessageAdaptationSegment { PersonalizationSegmentId = segmentId.Value } );
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override AdaptiveMessageAdaptation GetInitialEntity()
        {
            return GetInitialEntity<AdaptiveMessageAdaptation, AdaptiveMessageAdaptationService>( RockContext, PageParameterKey.AdaptiveMessageAdaptationId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var adaptiveMessageId = RequestContext.GetPageParameter( PageParameterKey.AdaptiveMessageId );
            if ( adaptiveMessageId.IsNullOrWhiteSpace() )
            {
                var adaptiveMessageAdaptation = GetAdaptiveMessageAdaptation();
                if ( adaptiveMessageAdaptation != null )
                {
                    return new Dictionary<string, string>
                    {
                        [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>
                        {
                            [PageParameterKey.AdaptiveMessageId] = adaptiveMessageAdaptation.AdaptiveMessageId.ToString()
                        } )
                    };
                }
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.AdaptiveMessageId] = adaptiveMessageId
                } )
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out AdaptiveMessageAdaptation entity, out BlockActionResult error )
        {
            var entityService = new AdaptiveMessageAdaptationService( RockContext );
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

        private AdaptiveMessageAdaptation GetAdaptiveMessageAdaptation()
        {
            if ( SelectedAdaptiveMessageAdaptation == null )
            {
                SelectedAdaptiveMessageAdaptation = new AdaptiveMessageAdaptationService( RockContext ).Get( RequestContext.GetPageParameter( PageParameterKey.AdaptiveMessageAdaptationId ) );
            }

            return SelectedAdaptiveMessageAdaptation;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageReference.Parameters );
            var adaptiveMessageAdaptationId = pageReference.GetPageParameter( PageParameterKey.AdaptiveMessageAdaptationId );

            if ( adaptiveMessageAdaptationId == null )
            {
                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>()
                };
            }

            var title = new AdaptiveMessageAdaptationService( RockContext ).Get( adaptiveMessageAdaptationId )?.Name ?? "New Adaptive Message Adaptation";
            var breadCrumb = new BreadCrumbLink( title, breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>
                {
                    breadCrumb
                }
            };
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

            return ActionOk( new ValidPropertiesBox<AdaptiveMessageAdaptationBag>
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
        public BlockActionResult Save( ValidPropertiesBox<AdaptiveMessageAdaptationBag> box )
        {
            var entityService = new AdaptiveMessageAdaptationService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var isNew = entity.Id == 0;
            if ( isNew )
            {
                var adaptiveMessageKey = RequestContext.GetPageParameter( PageParameterKey.AdaptiveMessageId );
                entity.AdaptiveMessageId = new AdaptiveMessageService( RockContext ).Get( adaptiveMessageKey ).Id;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateAdaptiveMessageAdaptation( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var segmentIds = box.Bag.Segments.AsGuidList().Select( a => PersonalizationSegmentCache.GetId( a ) ).Where( a => a.HasValue ).ToList();
            var itemsToDelete = entity.AdaptiveMessageAdaptationSegments.Where( a => !segmentIds.Contains( a.PersonalizationSegmentId ) ).ToList();
            entity.AdaptiveMessageAdaptationSegments.RemoveAll( itemsToDelete );
            var adaptiveMessageAdaptationSegmentService = new AdaptiveMessageAdaptationSegmentService( RockContext );
            adaptiveMessageAdaptationSegmentService.DeleteRange( itemsToDelete );
            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
                AdaptiveMessageAdaptationCache.FlushItem( entity.Id );
            } );

            if ( isNew )
            {
                var adaptiveMessage = new AdaptiveMessageService( RockContext ).Get( entity.AdaptiveMessageId );
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.AdaptiveMessageId] = adaptiveMessage.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.AdaptiveMessageId] = entity.AdaptiveMessage.IdKey
            } ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new AdaptiveMessageAdaptationService( RockContext );

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
