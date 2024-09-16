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
using Rock.ViewModels.Blocks.Lms.LearningSemesterDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning semester.
    /// </summary>
    [DisplayName( "Learning Semester Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning semester." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "78bcf0d7-b5ac-4429-8055-b436652083a7" )]
    [Rock.SystemGuid.BlockTypeGuid( "97b2e57f-3a03-490d-834f-cd3640c7ff1e" )]
    public class LearningSemesterDetail : RockEntityDetailBlockType<LearningSemester, LearningSemesterBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LearningSemesterId = "LearningSemesterId";
            public const string LearningProgramId = "LearningProgramId";
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
            var box = new DetailBlockBox<LearningSemesterBag, LearningSemesterDetailOptionsBag>();

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
        private LearningSemesterDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new LearningSemesterDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the LearningSemester for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningSemester">The LearningSemester to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningSemester is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningSemester( LearningSemester learningSemester, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LearningSemesterBag, LearningSemesterDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LearningSemester.FriendlyTypeName} was not found.";
                return;
            }

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningSemester.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningSemester.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LearningSemesterBag"/> that represents the entity.</returns>
        private LearningSemesterBag GetCommonEntityBag( LearningSemester entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new LearningSemesterBag
            {
                IdKey = entity.IdKey,
                EndDate = entity.EndDate,
                EnrollmentCloseDate = entity.EnrollmentCloseDate,
                Name = entity.Name,
                StartDate = entity.StartDate
            };
        }

        /// <inheritdoc/>
        protected override LearningSemesterBag GetEntityBagForView( LearningSemester entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        //// <inheritdoc/>
        protected override LearningSemesterBag GetEntityBagForEdit( LearningSemester entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( LearningSemester entity, ValidPropertiesBox<LearningSemesterBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.EndDate ),
                () => entity.EndDate = box.Bag.EndDate );

            box.IfValidProperty( nameof( box.Bag.EnrollmentCloseDate ),
                () => entity.EnrollmentCloseDate = box.Bag.EnrollmentCloseDate );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.StartDate ),
                () => entity.StartDate = box.Bag.StartDate );

            box.IfValidProperty( nameof( box.Bag.LearningProgramId ),
                () => entity.LearningProgramId = box.Bag.LearningProgramId );

            return true;
        }

        /// <inheritdoc/>
        protected override LearningSemester GetInitialEntity()
        {
            return GetInitialEntity<LearningSemester, LearningSemesterService>( RockContext, PageParameterKey.LearningSemesterId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out LearningSemester entity, out BlockActionResult error )
        {
            var entityService = new LearningSemesterService( RockContext );
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
                entity = new LearningSemester();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LearningSemester.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${LearningSemester.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningSemesterId ) ?? "";

            var entityName = entityKey.Length > 0 ? new Service<LearningSemester>( RockContext ).GetSelect( entityKey, p => p.Name ) : "New Semester";
            var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, pageReference.Parameters );
            var breadCrumb = new BreadCrumbLink( entityName ?? "New Semester", breadCrumbPageRef );

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

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<LearningSemesterBag>
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
        public BlockActionResult Save( ValidPropertiesBox<LearningSemesterBag> box )
        {
            var entityService = new LearningSemesterService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Set the LearningProgramId from the page parameter if the entity doesn't yet have one.
            if ( entity.LearningProgramId == 0 )
            {
                entity.LearningProgramId = RequestContext.PageParameterAsId( PageParameterKey.LearningProgramId );
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateLearningSemester( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.LearningSemesterId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<LearningSemesterBag>
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
            var entityService = new LearningSemesterService( RockContext );

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

            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
            };

            return ActionOk( this.GetParentPageUrl( queryParams ) );
        }

        #endregion
    }
}
