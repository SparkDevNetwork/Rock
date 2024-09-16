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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningActivityCompletionDetail;
using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Blocks.Lms.LearningActivityDetail;
using Rock.ViewModels.Blocks.Lms.LearningGradingSystemScaleDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning activity completion.
    /// </summary>

    [DisplayName( "Learning Activity Completion Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning activity completion." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "19474eb0-eeda-4fcb-b1ea-a35e23e6f691" )]
    [Rock.SystemGuid.BlockTypeGuid( "4569f28d-1efb-4b95-a506-0d9043c24775" )]
    public class LearningActivityCompletionDetail : RockEntityDetailBlockType<LearningActivityCompletion, LearningActivityCompletionBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LearningActivityCompletionId = "LearningActivityCompletionId";
            public const string LearningActivityId = "LearningActivityId";
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningClassId = "LearningClassId";
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
            var box = new DetailBlockBox<LearningActivityCompletionBag, LearningActivityCompletionDetailOptionsBag>();

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
        private LearningActivityCompletionDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new LearningActivityCompletionDetailOptionsBag();


            var scales = new LearningClassService( RockContext )
                .GetClassScales( RequestContext.PageParameterAsId( PageParameterKey.LearningClassId ) )
                .Select( s => new LearningGradingSystemScaleBag
                {
                    Description = s.Description,
                    IsPassing = s.IsPassing,
                    Name = s.Name,
                    ThresholdPercentage = s.ThresholdPercentage ?? 0
                } ).ToList();

            options.GradingScales = scales;

            return options;
        }

        /// <summary>
        /// Validates the LearningActivityCompletion for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningActivityCompletion">The LearningActivityCompletion to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningActivityCompletion is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningActivityCompletion( LearningActivityCompletion learningActivityCompletion, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LearningActivityCompletionBag, LearningActivityCompletionDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LearningActivityCompletion.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    if ( box.IsEditable )
                    {
                        box.ValidProperties = box.Entity.GetType().GetProperties().Select( p => p.Name ).ToList();
                    }
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningActivityCompletion.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningActivityCompletion.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LearningActivityCompletionBag"/> that represents the entity.</returns>
        private LearningActivityCompletionBag GetCommonEntityBag( LearningActivityCompletion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var activityComponentBag = GetLearningActivityComponentBag( entity.LearningActivity?.ActivityComponentId );

            var binaryFile = entity.BinaryFileId > 0 ?
                new BinaryFileService( RockContext ).GetNoTracking( entity.BinaryFileId.Value ) :
                null;

            // Get the student and current persons bags.
            var currentPerson = GetCurrentPerson();

            var studentAndCurrentPersonIds = new[] { currentPerson.Id, entity.Student.PersonId };

            var bags = new LearningParticipantService( RockContext )
                .GetParticipantBags( entity.LearningActivity.LearningClassId, false, studentAndCurrentPersonIds );

            var currentPersonBag = bags.FirstOrDefault( p => p.IdKey == currentPerson.IdKey );
            var studentBag = bags.FirstOrDefault( p => p.IdKey != currentPerson.IdKey );

            var scales = new LearningClassService( RockContext )
                .GetClassScales( entity.LearningActivity.LearningClassId );

            var now = RockDateTime.Now;
            var activityDescriptionAsHtml = entity.LearningActivity.Description.IsNotNullOrWhiteSpace() ?
                new StructuredContentHelper( entity.LearningActivity.Description ).Render() :
                string.Empty;

            var activityBag = new LearningActivityBag
            {
                ActivityComponent = activityComponentBag,
                ActivityComponentSettingsJson = entity.LearningActivity.ActivityComponentSettingsJson,
                AssignTo = entity.LearningActivity.AssignTo,
                CurrentPerson = currentPersonBag,
                Description = entity.LearningActivity.Description,
                DescriptionAsHtml = activityDescriptionAsHtml,
                IsStudentCommentingEnabled = entity.LearningActivity.IsStudentCommentingEnabled,
                Name = entity.LearningActivity.Name,
                Order = entity.LearningActivity.Order,
                Points = entity.LearningActivity.Points
            };

            return new LearningActivityCompletionBag
            {
                IdKey = entity.IdKey,
                ActivityBag = activityBag,
                ActivityComponentCompletionJson = entity.ActivityComponentCompletionJson,
                BinaryFile = binaryFile?.ToListItemBag(),
                CompletedDate = entity.CompletedDateTime,
                DueDate = entity.DueDate,
                FacilitatorComment = entity.FacilitatorComment,
                GradeText = entity.GetGradeText( scales ),
                IsGradePassing = entity.GetGrade().IsPassing,
                IsFacilitatorCompleted = entity.IsFacilitatorCompleted,
                IsStudentCompleted = entity.IsStudentCompleted,
                PointsEarned = entity.PointsEarned,
                Student = studentBag,
                StudentComment = entity.StudentComment,
                WasCompletedOnTime = entity.WasCompletedOnTime
            };
        }

        private LearningActivityComponentBag GetLearningActivityComponentBag( int? activityComponentId )
        {
            var componentId = activityComponentId.ToIntSafe();
            if ( componentId == 0 )
            {
                return new LearningActivityComponentBag();
            }
            else
            {
                var componentEntityType = EntityTypeCache.Get( componentId );
                var activityComponent = Rock.Lms.LearningActivityContainer.GetComponent( componentEntityType.Name );

                return new LearningActivityComponentBag
                {
                    Name = activityComponent?.Name,
                    ComponentUrl = activityComponent?.ComponentUrl,
                    HighlightColor = activityComponent?.HighlightColor,
                    IconCssClass = activityComponent?.IconCssClass,
                    IdKey = activityComponent?.EntityType.IdKey,
                    Guid = activityComponent?.EntityType.Guid.ToString()
                };
            }
        }

        /// <inheritdoc/>
        protected override LearningActivityCompletionBag GetEntityBagForView( LearningActivityCompletion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        //// <inheritdoc/>
        protected override LearningActivityCompletionBag GetEntityBagForEdit( LearningActivityCompletion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( LearningActivityCompletion entity, ValidPropertiesBox<LearningActivityCompletionBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.ActivityComponentCompletionJson ),
                () => entity.ActivityComponentCompletionJson = box.Bag.ActivityComponentCompletionJson );

            box.IfValidProperty( nameof( box.Bag.DueDate ),
                () => entity.DueDate = box.Bag.DueDate );

            box.IfValidProperty( nameof( box.Bag.FacilitatorComment ),
                () => entity.FacilitatorComment = box.Bag.FacilitatorComment );

            box.IfValidProperty( nameof( box.Bag.IsFacilitatorCompleted ),
                () => entity.IsFacilitatorCompleted = box.Bag.IsFacilitatorCompleted );

            box.IfValidProperty( nameof( box.Bag.PointsEarned ),
                () => entity.PointsEarned = box.Bag.PointsEarned );

            box.IfValidProperty( nameof( box.Bag.WasCompletedOnTime ),
                () => entity.WasCompletedOnTime = box.Bag.WasCompletedOnTime );

            return true;
        }

        /// <inheritdoc/>
        protected override LearningActivityCompletion GetInitialEntity()
        {
            var completionId = RequestContext.PageParameterAsId( PageParameterKey.LearningActivityCompletionId );
            if ( completionId > 0 )
            {
                return new LearningActivityCompletionService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( a => a.LearningActivity )
                    .FirstOrDefault( a => a.Id == completionId );
            }
            else
            {
                return new LearningActivityCompletion();
            }
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = ParentPageUrl()
            };
        }

        private string ParentPageUrl()
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId ),
                [PageParameterKey.LearningActivityId] = PageParameter( PageParameterKey.LearningActivityId ),
                [PageParameterKey.LearningActivityCompletionId] = PageParameter( PageParameterKey.LearningActivityCompletionId )
            };

            return this.GetParentPageUrl( queryParams );
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out LearningActivityCompletion entity, out BlockActionResult error )
        {
            var entityService = new LearningActivityCompletionService( RockContext );
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
                entity = new LearningActivityCompletion();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LearningActivityCompletion.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${LearningActivityCompletion.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningActivityCompletionId ) ?? "";
            var entityDetail =
                    entityKey.IsNullOrWhiteSpace() ?
                    null :
                    new Service<LearningActivityCompletion>( RockContext )
                        .GetSelect( entityKey, p => new { p.Student.Person.NickName, p.Student.Person.LastName, p.Student.Person.SuffixValueId } );

            // This page doesn't support adding records so if there's no valid key then we should return early.
            if ( entityDetail == null )
            {
                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>()
                };
            }

            var entityName = Rock.Model.Person.FormatFullName( entityDetail.NickName, entityDetail.LastName, entityDetail.SuffixValueId );
            var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, pageReference.Parameters );
            var breadCrumb = new BreadCrumbLink( entityName, breadCrumbPageRef );

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

            return ActionOk( new ValidPropertiesBox<LearningActivityCompletionBag>
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
        public BlockActionResult Save( ValidPropertiesBox<LearningActivityCompletionBag> box )
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
            if ( !ValidateLearningActivityCompletion( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, ParentPageUrl() );
            }

            return ActionOk( ParentPageUrl() );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new LearningActivityCompletionService( RockContext );

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

            return ActionOk( ParentPageUrl() );
        }

        #endregion
    }
}
