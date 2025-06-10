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
using Rock.Enums.Lms;
using Rock.Lms;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningClassActivityCompletionDetail;
using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Blocks.Lms.LearningClassActivityDetail;
using Rock.ViewModels.Blocks.Lms.LearningGradingSystemScaleDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning class activity completion.
    /// </summary>

    [DisplayName( "Learning Class Activity Completion Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning class activity completion." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "19474eb0-eeda-4fcb-b1ea-a35e23e6f691" )]
    [Rock.SystemGuid.BlockTypeGuid( "4569f28d-1efb-4b95-a506-0d9043c24775" )]
    public class LearningClassActivityCompletionDetail : RockEntityDetailBlockType<LearningClassActivityCompletion, LearningClassActivityCompletionBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LearningClassActivityCompletionId = "LearningClassActivityCompletionId";
            public const string LearningClassActivityId = "LearningClassActivityId";
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningClassId = "LearningClassId";
            public const string LearningParticipantId = "LearningParticipantId";
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
            var box = new DetailBlockBox<LearningClassActivityCompletionBag, LearningClassActivityCompletionDetailOptionsBag>();

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
        private LearningClassActivityCompletionDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new LearningClassActivityCompletionDetailOptionsBag();

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
        /// Validates the LearningClassActivityCompletion for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningClassActivityCompletion">The LearningClassActivityCompletion to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningClassActivityCompletion is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningClassActivityCompletion( LearningClassActivityCompletion learningClassActivityCompletion, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LearningClassActivityCompletionBag, LearningClassActivityCompletionDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LearningClassActivityCompletion.FriendlyTypeName} was not found.";
                return;
            }

            var isAuthorizedToEditGrades = entity.IsAuthorized( Authorization.EDIT_GRADES, RequestContext.CurrentPerson );
            var isViewable = isAuthorizedToEditGrades;
            box.IsEditable = isAuthorizedToEditGrades;

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningClassActivityCompletion.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningClassActivityCompletion.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LearningClassActivityCompletionBag"/> that represents the entity.</returns>
        private LearningClassActivityCompletionBag GetCommonEntityBag( LearningClassActivityCompletion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var activityComponentId = entity?.LearningClassActivity?.LearningActivity.ActivityComponentId.ToIntSafe() ?? 0;

            var componentEntityType = EntityTypeCache.Get( activityComponentId );
            var activityComponent = LearningActivityContainer.GetComponent( componentEntityType.Name );
            var componentData = entity.LearningClassActivity.LearningActivity.ActivityComponentSettingsJson.FromJsonOrNull<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();

            var activityComponentBag = new LearningActivityComponentBag
            {
                Name = activityComponent?.Name,
                ComponentUrl = activityComponent?.ComponentUrl,
                ComponentConfiguration = activityComponent.GetActivityConfiguration( entity.LearningClassActivity, componentData, PresentedFor.Facilitator, RockContext, RequestContext ),
                HighlightColor = activityComponent?.HighlightColor,
                IconCssClass = activityComponent?.IconCssClass,
                IdKey = activityComponent?.EntityType.IdKey,
                Guid = activityComponent?.EntityType.Guid.ToString()
            };

            var binaryFile = entity.BinaryFileId > 0 ?
                new BinaryFileService( RockContext ).GetNoTracking( entity.BinaryFileId.Value ) :
                null;

            // Get the student and current persons bags.
            var currentPerson = GetCurrentPerson();
            var canViewGrades = entity.IsAuthorized( Authorization.VIEW_GRADES, currentPerson );

            var participantService = new LearningParticipantService( RockContext );

            // Get the student and current user participants for the current class.
            var studentAndCurrentPersonIds = new[] { currentPerson.Id, entity.Student.PersonId };

            // Get the data necessary for the participant bags.
            var classParticipants = participantService.Queryable()
                .AsNoTracking()
                .Include( a => a.Person )
                .Include( a => a.GroupRole )
                .Where( a => a.LearningClassId == entity.LearningClassActivity.LearningClassId )
                .Where( gm => studentAndCurrentPersonIds.Contains( gm.PersonId ) )
                .Select( p => new
                {
                    p.Id,
                    p.Person.Email,
                    IsFacilitator = p.GroupRole.IsLeader,
                    p.Person.NickName,
                    p.Person.LastName,
                    p.Person.SuffixValueId,
                    RoleName = p.GroupRole.Name,
                    p.Guid,
                    p.PersonId
                } )
                .ToList();

            // Create the participant bags
            var bags = classParticipants.Select( p => new LearningActivityParticipantBag
            {
                Email = p.Email,
                Guid = p.Guid,
                IdKey = Rock.Utility.IdHasher.Instance.GetHash( p.Id ),
                IsFacilitator = p.IsFacilitator,
                Name = Rock.Model.Person.FormatFullName( p.NickName, p.LastName, p.SuffixValueId ),
                RoleName = p.RoleName
            } );

            // Get the Guid of each participant type.
            var currentPersonPartipicantGuid = classParticipants.FirstOrDefault( gm => gm.PersonId == currentPerson.Id )?.Guid;
            var studentPartipicantGuid = classParticipants.FirstOrDefault( gm => gm.PersonId == entity.Student.PersonId )?.Guid;

            // Now get the participant bags.
            var currentPersonBag = bags.FirstOrDefault( p => p.Guid == currentPersonPartipicantGuid );
            var studentBag = bags.FirstOrDefault( p => p.Guid == studentPartipicantGuid );

            ListItemBag gradedByPersonAliasBag = null;
            if ( entity.GradedByPersonAliasId.HasValue )
            {
                var gradedByPersonData = new PersonAliasService( RockContext )
                    .GetSelect( entity.GradedByPersonAliasId.Value,
                     p => new
                     {
                         p.Person.NickName,
                         p.Person.LastName,
                         p.Person.SuffixValueId,
                         p.Person.Guid
                     } );

                if ( gradedByPersonData != null )
                {
                    gradedByPersonAliasBag = new ListItemBag
                    {
                        Text = Rock.Model.Person.FormatFullName(
                            gradedByPersonData.NickName,
                            gradedByPersonData.LastName,
                            gradedByPersonData.SuffixValueId ),
                        Value = gradedByPersonData.Guid.ToStringSafe()
                    };
                }
            }

            var scales = new LearningClassService( RockContext )
                .GetClassScales( entity.LearningClassActivity.LearningClassId );

            var now = RockDateTime.Now;

            // If the current person is a facilitator include a security grant for viewing the uploaded file.
            var binaryFileSecurityGrant = string.Empty;
            if ( binaryFile != null && currentPersonBag?.IsFacilitator == true )
            {
                var binaryFileEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.BinaryFile ) ).ToIntSafe();
                binaryFileSecurityGrant = new Rock.Security.SecurityGrant()
                        .AddRule( new Rock.Security.SecurityGrantRules.EntitySecurityGrantRule( binaryFileEntityTypeId, entity.BinaryFileId.Value ) )
                        .ToToken( true )
                        .UrlEncode();
            }

            var classActivityBag = new LearningClassActivityBag
            {
                ActivityComponent = activityComponentBag,
                AssignTo = entity.LearningClassActivity.AssignTo,
                CurrentPerson = currentPersonBag,
                Description = entity.LearningClassActivity.LearningActivity.Description,
                IsStudentCommentingEnabled = entity.LearningClassActivity.IsStudentCommentingEnabled,
                Name = entity.LearningClassActivity.Name,
                Order = entity.LearningClassActivity.Order,
                Points = entity.LearningClassActivity.Points
            };

            var completionData = entity.ActivityComponentCompletionJson.FromJsonOrNull<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();

            return new LearningClassActivityCompletionBag
            {
                IdKey = entity.IdKey,
                ClassActivityBag = classActivityBag,
                CompletionValues = activityComponent.GetCompletionValues( entity, completionData, componentData, PresentedFor.Facilitator, RockContext, RequestContext ),
                BinaryFile = binaryFile?.ToListItemBag(),
                BinaryFileSecurityGrant = binaryFileSecurityGrant,
                CompletedDate = entity.CompletedDateTime,
                DueDate = entity.DueDate,
                FacilitatorComment = entity.FacilitatorComment,
                GradeText = canViewGrades ? entity.GetGradeText( scales ) : string.Empty,
                GradedByPersonAlias = gradedByPersonAliasBag,
                IsFacilitatorCompleted = entity.IsFacilitatorCompleted,
                IsGradePassing = canViewGrades ? entity.GetGrade()?.IsPassing ?? false : false,
                IsLate = entity.IsLate,
                IsStudentCompleted = entity.IsStudentCompleted,
                PointsEarned = canViewGrades ? entity.PointsEarned : null,
                RequiresScoring = entity.RequiresGrading,
                RequiresFacilitatorCompletion = entity.RequiresFacilitatorCompletion,
                Student = studentBag,
                StudentComment = entity.StudentComment,
                WasCompletedOnTime = entity.WasCompletedOnTime
            };
        }

        /// <inheritdoc/>
        protected override LearningClassActivityCompletionBag GetEntityBagForView( LearningClassActivityCompletion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        //// <inheritdoc/>
        protected override LearningClassActivityCompletionBag GetEntityBagForEdit( LearningClassActivityCompletion entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( LearningClassActivityCompletion entity, ValidPropertiesBox<LearningClassActivityCompletionBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.FacilitatorComment ),
                () => entity.FacilitatorComment = box.Bag.FacilitatorComment );

            box.IfValidProperty( nameof( box.Bag.IsFacilitatorCompleted ),
                () => entity.IsFacilitatorCompleted = box.Bag.IsFacilitatorCompleted );

            box.IfValidProperty( nameof( box.Bag.DueDate ),
                () => entity.DueDate = box.Bag.DueDate?.DateTime );

            if ( !entity.GradedByPersonAliasId.HasValue || box.Bag.PointsEarned != entity.PointsEarned )
            {
                entity.GradedByPersonAliasId = GetCurrentPerson()?.PrimaryAliasId;

                // The class activity has been graded so there's no need to check with
                // the activity component whether it requires grading.
                entity.RequiresGrading = false;
            }

            if ( !entity.CompletedDateTime.HasValue )
            {
                entity.CompletedDateTime = RockDateTime.Now;
            }

            box.IfValidProperty( nameof( box.Bag.PointsEarned ),
                () => entity.PointsEarned = box.Bag.PointsEarned );

            box.IfValidProperty( nameof( box.Bag.WasCompletedOnTime ),
                () => entity.WasCompletedOnTime = box.Bag.WasCompletedOnTime );

            box.IfValidProperty( nameof( box.Bag.CompletionValues ), () =>
            {
                var activityComponent = LearningActivityContainer.Instance.Components.Values
                    .FirstOrDefault( c => c.Value.EntityType.Id == entity.LearningClassActivity.LearningActivity.ActivityComponentId )
                    .Value;

                var componentData = entity.LearningClassActivity.LearningActivity.ActivityComponentSettingsJson.FromJsonOrNull<Dictionary<string, string>>()
                    ?? new Dictionary<string, string>();

                var completionData = activityComponent.GetCompletionData( entity, box.Bag.CompletionValues, componentData, PresentedFor.Facilitator, RockContext, RequestContext )
                    ?? new Dictionary<string, string>();

                entity.ActivityComponentCompletionJson = completionData.ToJson();
            } );

            return true;
        }

        /// <inheritdoc/>
        protected override LearningClassActivityCompletion GetInitialEntity()
        {
            var completionId = PageParameter( PageParameterKey.LearningClassActivityCompletionId );

            if ( completionId.IsNotNullOrWhiteSpace() && completionId != "0" )
            {
                return new LearningClassActivityCompletionService( RockContext )
                    .GetInclude( completionId, a => a.LearningClassActivity );
            }
            else
            {
                return GetNewEntity();
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

        /// <summary>
        /// Gets a new <see cref="LearningClassActivityCompletion"/> record using the
        /// LearningClassActivityId and LearningParticipantId page parameters.
        /// </summary>
        /// <remarks>
        /// This is non-standard because the entity may not exist yet, but we
        /// need to initialize with values for the specified <see cref="LearningClassActivity"/>
        /// and <see cref="LearningParticipant"/>.
        /// </remarks>
        /// <returns>A new <see cref="LearningClassActivityCompletion"/> for the
        /// <see cref="LearningClassActivity"/> and <see cref="LearningParticipant"/>
        /// whose identifier is provided as a PageParameter.</returns>
        private LearningClassActivityCompletion GetNewEntity()
        {
            var classActivityIdParam = PageParameter( PageParameterKey.LearningClassActivityId );
            var participantIdParam = PageParameter( PageParameterKey.LearningParticipantId );

            int.TryParse( classActivityIdParam, out var classActivityId );
            int.TryParse( participantIdParam, out var participantId );

            if ( classActivityId == 0 && classActivityIdParam.Length > 0 )
            {
                classActivityId = IdHasher.Instance.GetId( classActivityIdParam ).ToIntSafe();
            }

            if ( participantId == 0 && participantIdParam.Length > 0 )
            {
                participantId = IdHasher.Instance.GetId( participantIdParam ).ToIntSafe();
            }

            var existingActivity = new LearningClassActivityCompletionService( RockContext )
                .Queryable()
                .FirstOrDefault( a => a.LearningClassActivityId == classActivityId && a.StudentId == participantId );

            if ( existingActivity != null )
            {
                // If the record exists, but the completion id just wasn't passed
                // get the existing record.
                return existingActivity;
            }

            // The completion record doesn't yet exist
            // We'll need to get a default for the class activity and student.
            // (This can happen when a class activity is completed by the facilitator).
            var student = new LearningParticipantService( RockContext ).Get( participantId );
            var classActivity = new LearningClassActivityService( RockContext ).Get( classActivityId );

            return LearningClassActivityCompletionService.GetNew( classActivity, student );
        }

        private string ParentPageUrl()
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId ),
                [PageParameterKey.LearningClassActivityId] = PageParameter( PageParameterKey.LearningClassActivityId ),
                [PageParameterKey.LearningClassActivityCompletionId] = PageParameter( PageParameterKey.LearningClassActivityCompletionId )
            };

            return this.GetParentPageUrl( queryParams );
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out LearningClassActivityCompletion entity, out BlockActionResult error )
        {
            var entityService = new LearningClassActivityCompletionService( RockContext );
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
                entity = GetNewEntity();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LearningClassActivityCompletion.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT_GRADES, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {LearningClassActivityCompletion.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningClassActivityCompletionId ) ?? "";
            var entityDetail =
                    entityKey.IsNullOrWhiteSpace() ?
                    null :
                    new Service<LearningClassActivityCompletion>( RockContext )
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

            return ActionOk( new ValidPropertiesBox<LearningClassActivityCompletionBag>
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
        public BlockActionResult Save( ValidPropertiesBox<LearningClassActivityCompletionBag> box )
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
            if ( !ValidateLearningClassActivityCompletion( entity, out var validationMessage ) )
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
            var entityService = new LearningClassActivityCompletionService( RockContext );

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
