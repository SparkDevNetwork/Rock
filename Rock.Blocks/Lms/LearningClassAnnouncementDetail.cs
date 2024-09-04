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
using Rock.ViewModels.Blocks.Lms.LearningClassAnnouncementDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning class announcement.
    /// </summary>

    [DisplayName( "Learning Class Announcement Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning class announcement." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "08429949-4774-41F7-8840-2D8DEFFF14AB" )]
    [Rock.SystemGuid.BlockTypeGuid( "53C12A53-773E-4398-8627-DD44C1421675" )]
    public class LearningClassAnnouncementDetail : RockEntityDetailBlockType<LearningClassAnnouncement, LearningClassAnnouncementBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ActivityDetailPage = "ActivityDetailPage";
            public const string ParentPage = "ParentPage";
        }

        private static class PageParameterKey
        {
            public const string LearningClassId = "LearningClassId";
            public const string LearningClassAnnouncementId = "LearningClassAnnouncementId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningProgramId = "LearningProgramId";
            public const string AutoEdit = "autoEdit";
            public const string ReturnUrl = "returnUrl";
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
            var box = new DetailBlockBox<LearningClassAnnouncementBag, LearningClassAnnouncementDetailOptionsBag>();

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
        private LearningClassAnnouncementDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            return new LearningClassAnnouncementDetailOptionsBag();
        }

        /// <summary>
        /// Validates the LearningClassAnnouncement for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningClassAnnouncement">The LearningClassAnnouncement to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningClassAnnouncement is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningClassAnnouncement( LearningClassAnnouncement learningClassAnnouncement, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LearningClassAnnouncementBag, LearningClassAnnouncementDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningClassAnnouncement.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningClassAnnouncement.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LearningClassAnnouncementBag"/> that represents the entity.</returns>
        private LearningClassAnnouncementBag GetCommonEntityBag( LearningClassAnnouncement entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new LearningClassAnnouncementBag
            {
                CommunicationMode = entity.CommunicationMode,
                CommunicationSent = entity.CommunicationSent,
                Description = entity.Description,
                IdKey = entity.IdKey,
                PublishDateTime = entity.PublishDateTime == DateTime.MinValue ? null : ( DateTime? ) entity.PublishDateTime,
                Title = entity.Title
            };
        }

        /// <inheritdoc/>
        protected override LearningClassAnnouncementBag GetEntityBagForView( LearningClassAnnouncement entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        //// <inheritdoc/>
        protected override LearningClassAnnouncementBag GetEntityBagForEdit( LearningClassAnnouncement entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( LearningClassAnnouncement entity, ValidPropertiesBox<LearningClassAnnouncementBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.CommunicationMode ),
                () => entity.CommunicationMode = box.Bag.CommunicationMode );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            if ( box.Bag.PublishDateTime.HasValue )
            {
                box.IfValidProperty( nameof( box.Bag.PublishDateTime ),
                () => entity.PublishDateTime = box.Bag.PublishDateTime.Value );
            }

            box.IfValidProperty( nameof( box.Bag.Title ),
                () => entity.Title = box.Bag.Title );

            return true;
        }

        /// <inheritdoc/>
        protected override LearningClassAnnouncement GetInitialEntity()
        {
            // Parse out the Id if the parameter is an IdKey or take the Id
            // If the site allows predictable Ids in parameters.
            var entityId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassAnnouncementId );

            // If a zero identifier is specified then create a new entity.
            if ( entityId == 0 )
            {
                return new LearningClassAnnouncement
                {
                    Id = 0,
                    Guid = Guid.Empty
                };
            }

            var entityService = new LearningClassAnnouncementService( RockContext );

            return entityService
                .Queryable()
                .FirstOrDefault( a => a.Id == entityId );
        }

        private Dictionary<string, string> GetCurrentPageParams()
        {
            return new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId ),
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.PageParameter( PageParameterKey.ReturnUrl ) ?? this.GetParentPageUrl( GetCurrentPageParams() ),
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out LearningClassAnnouncement entity, out BlockActionResult error )
        {
            var entityService = new LearningClassAnnouncementService( RockContext );
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
                entity = new LearningClassAnnouncement();

                entity.LearningClassId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassId );

                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LearningClassAnnouncement.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${LearningClassAnnouncement.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningClassAnnouncementId ) ?? "";

                var entityName = entityKey.Length > 0 ? new Service<LearningClassAnnouncement>( rockContext ).GetSelect( entityKey, p => p.Title ) : "New Announcement";
                var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, pageReference.Parameters );
                var breadCrumb = new BreadCrumbLink( entityName ?? "New Announcement", breadCrumbPageRef );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>
                    {
                        breadCrumb
                    }
                };
            }
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

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<LearningClassAnnouncementBag>
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
        public BlockActionResult Save( ValidPropertiesBox<LearningClassAnnouncementBag> box )
        {
            var entityService = new LearningClassAnnouncementService( RockContext );

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
            if ( !ValidateLearningClassAnnouncement( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            if ( isNew )
            {
                entity.LearningClassId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassId );
            }

            RockContext.SaveChanges();

            var returnToUrl = this.PageParameter( PageParameterKey.ReturnUrl ) ?? this.GetParentPageUrl( GetCurrentPageParams() );
            return ActionContent( System.Net.HttpStatusCode.Created, returnToUrl );
        }

        #endregion
    }
}
