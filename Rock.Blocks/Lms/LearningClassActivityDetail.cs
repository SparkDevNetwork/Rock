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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Blocks.Lms.LearningClassActivityDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning activity.
    /// </summary>

    [DisplayName( "Learning Class Activity Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning class activity." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "fe13bfef-6266-4667-b51f-01af8e6c5b89" )]
    [Rock.SystemGuid.BlockTypeGuid( "4b18bf0d-d91b-4934-ac2d-a7188b15b893" )]
    public class LearningClassActivityDetail : RockEntityDetailBlockType<LearningClassActivity, LearningClassActivityBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LearningClassActivityId = "LearningClassActivityId";
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningClassId = "LearningClassId";
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
            var box = new DetailBlockBox<LearningClassActivityBag, LearningClassActivityDetailOptionsBag>();

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
        private LearningClassActivityDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new LearningClassActivityDetailOptionsBag();

            var components = LearningActivityContainer.Instance.Components;

            // Get the list of Activity Components (for rendering the actual Obsidian component.
            options.ActivityTypes = components.Select( component => new LearningActivityComponentBag
            {
                Name = component.Value.Value.Name,
                ComponentUrl = component.Value.Value.ComponentUrl,
                ComponentConfiguration = component.Value.Value.GetActivityConfiguration( null, new Dictionary<string, string>(), PresentedFor.Configuration, RockContext, RequestContext ),
                HighlightColor = component.Value.Value.HighlightColor,
                IconCssClass = component.Value.Value.IconCssClass,
                IdKey = component.Value.Value.EntityType.IdKey,
                Guid = component.Value.Value.EntityType.Guid.ToString()
            } )
                .OrderBy( a => a.Name )
                .ToList();

            // Get a list of Activity Types for the user to select from.
            options.ActivityTypeListItems = options.ActivityTypes.Select( a => new ListItemBag
            {
                Value = a.Guid,
                Text = a.Name
            } ).ToList();

            options.HasCompletions = ActivityHasCompletions();

            var configurationMode = new LearningProgramService( RockContext ).GetSelect( PageParameter( PageParameterKey.LearningProgramId ), p => p.ConfigurationMode );
            var activityService = new LearningClassActivityService( RockContext );

            options.AvailabilityCriteriaOptions = activityService.GetAvailabilityCriteria( configurationMode );
            options.DueDateCriteriaOptions = activityService.GetDueDateCriteria( configurationMode );

            return options;
        }

        private bool ActivityHasCompletions()
        {
            var activityId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassActivityId );
            return new LearningClassActivityCompletionService( RockContext ).Queryable().Any( c => c.LearningClassActivityId == activityId );
        }

        /// <summary>
        /// Validates the LearningClassActivity for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningClassActivity">The LearningClassActivity to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningClassActivity is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningClassActivity( LearningClassActivity learningClassActivity, out string errorMessage )
        {
            errorMessage = null;

            if ( learningClassActivity.Name.IsNullOrWhiteSpace() )
            {
                errorMessage = "Name is a required field";
            }
            else if ( learningClassActivity.LearningActivity.ActivityComponentId == 0 )
            {
                errorMessage = "Activity Type is a required field";
            }

            return errorMessage == null;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LearningClassActivityBag, LearningClassActivityDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LearningClassActivity.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningClassActivity.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningClassActivity.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LearningClassActivityBag"/> that represents the entity.</returns>
        private LearningClassActivityBag GetCommonEntityBag( LearningClassActivity entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var learningClassActivityService = new LearningClassActivityService( RockContext );
            var completionStatistics = learningClassActivityService.GetCompletionStatistics( entity );

            // Get the current persons info.
            var currentPerson = GetCurrentPerson();
            var facilitatorId = new LearningParticipantService( RockContext )
                .GetFacilitatorId( currentPerson.Id, entity.LearningClassId );

            var isClassFacilitator = facilitatorId.HasValue && facilitatorId.Value > 0;
            var currentPersonBag = new LearningActivityParticipantBag
            {
                Name = currentPerson.FullName,
                IdKey = currentPerson.IdKey,
                IsFacilitator = isClassFacilitator
            };

            var classId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassId );
            var isFirstClassActivity = !learningClassActivityService.Queryable().Any( a => a.LearningClassId == classId );
            var isNew = entity.Id == 0;

            // If this is an existing record use it's availability criteria
            // If new - use "Always Available" for the first activity in a class
            // and "After Previous Completed" for all subsequent activities.
            var availabilityCriteria =
                !isNew ?
                entity.AvailabilityCriteria :
                isFirstClassActivity ? AvailabilityCriteria.AlwaysAvailable :
                AvailabilityCriteria.AfterPreviousCompleted;

            return new LearningClassActivityBag
            {
                IdKey = entity.IdKey,
                AssignTo = entity.AssignTo,
                AvailabilityCriteria = availabilityCriteria,
                AvailableDateCalculated = entity.AvailableDateCalculated,
                AvailableDateDefault = entity.AvailableDateDefault,
                AvailableDateDescription = entity.AvailableDateDescription,
                AvailableDateOffset = entity.AvailableDateOffset,
                AverageGrade = completionStatistics.AverageGrade?.Name,
                AverageGradePercent = completionStatistics.AverageGradePercent,
                AverageGradeIsPassing = completionStatistics.AverageGrade?.IsPassing ?? false,
                CompleteCount = completionStatistics.Complete,
                CompletionWorkflowType = entity.CompletionWorkflowType.ToListItemBag(),
                CurrentPerson = currentPersonBag,
                Description = entity.LearningActivity.Description,
                DueDateCriteria = entity.DueDateCriteria,
                DueDateCalculated = entity.DueDateCalculated,
                DueDateDefault = entity.DueDateDefault,
                DueDateDescription = entity.DueDateDescription,
                DueDateOffset = entity.DueDateOffset,
                IncompleteCount = completionStatistics.Incomplete,
                IsPastDue = entity.IsPastDue,
                IsStudentCommentingEnabled = entity.IsStudentCommentingEnabled,
                Name = entity.Name,
                Order = entity.Order,
                PercentComplete = completionStatistics.PercentComplete,
                Points = entity.Points,
                SendNotificationCommunication = entity.SendNotificationCommunication
            };
        }

        private LearningClassActivity GetDefaultEntity()
        {
            /*
                12/12/2024 - JC

                We must load the parent LearningClass for new records.
                When the authorization is checked the LearningClass (the ParentAuthority)
                will be responsible for approving/denying access (see LearningActvity.IsAuthorized).

                Reason: ParentAuthority (LearningClass) will be checked for authorization.
            */
            var learningClass = new LearningClassService( RockContext ).Get(
                PageParameter( PageParameterKey.LearningClassId ),
                !this.PageCache.Layout.Site.DisablePredictableIds );

            return new LearningClassActivity
            {
                Id = 0,
                Guid = Guid.Empty,
                LearningActivity = new LearningActivity(),
                LearningClass = learningClass,
                LearningClassId = learningClass.Id,
                AvailabilityCriteria = Enums.Lms.AvailabilityCriteria.AfterPreviousCompleted,
                DueDateCriteria = Enums.Lms.DueDateCriteria.NoDate
            };
        }

        /// <inheritdoc/>
        protected override LearningClassActivityBag GetEntityBagForView( LearningClassActivity entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        //// <inheritdoc/>
        protected override LearningClassActivityBag GetEntityBagForEdit( LearningClassActivity entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.LearningActivity.ActivityComponentId > 0 )
            {
                var componentEntityType = EntityTypeCache.Get( entity.LearningActivity.ActivityComponentId );
                var activityComponent = LearningActivityContainer.GetComponent( componentEntityType.Name );
                var componentData = entity.LearningActivity.ActivityComponentSettingsJson.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();

                bag.ActivityComponent = new LearningActivityComponentBag
                {
                    ComponentUrl = activityComponent.ComponentUrl,
                    ComponentConfiguration = activityComponent.GetActivityConfiguration( entity, componentData, PresentedFor.Configuration, RockContext, RequestContext ),
                    Guid = activityComponent.EntityType.Guid.ToString(),
                    HighlightColor = activityComponent.HighlightColor,
                    IconCssClass = activityComponent.IconCssClass,
                    IdKey = activityComponent.EntityType.IdKey,
                    Name = activityComponent.Name,
                };

                bag.ComponentSettings = activityComponent.GetComponentSettings( entity, componentData, RockContext, RequestContext );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( LearningClassActivity entity, ValidPropertiesBox<LearningClassActivityBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.LearningActivity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.LearningActivity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.ActivityComponent ),
                () =>
                {
                    var componentEntityTypeId = Rock.Utility.IdHasher.Instance.GetId( box.Bag?.ActivityComponent?.IdKey );
                    if ( componentEntityTypeId.HasValue )
                        entity.LearningActivity.ActivityComponentId = componentEntityTypeId.Value;
                } );

            box.IfValidProperty( nameof( box.Bag.AssignTo ),
                () => entity.AssignTo = box.Bag.AssignTo );

            box.IfValidProperty( nameof( box.Bag.AvailabilityCriteria ),
                () => entity.AvailabilityCriteria = box.Bag.AvailabilityCriteria );

            box.IfValidProperty( nameof( box.Bag.AvailableDateDefault ),
                () => entity.AvailableDateDefault = box.Bag.AvailableDateDefault );

            box.IfValidProperty( nameof( box.Bag.AvailableDateOffset ),
                () => entity.AvailableDateOffset = box.Bag.AvailableDateOffset );

            var canUpdateDueDates = entity.Id == 0 || box.Bag.DueDateChangeType.HasValue;

            if ( canUpdateDueDates )
            {
                box.IfValidProperty( nameof( box.Bag.DueDateCriteria ),
                () => entity.DueDateCriteria = box.Bag.DueDateCriteria );

                box.IfValidProperty( nameof( box.Bag.DueDateDefault ),
                    () => entity.DueDateDefault = box.Bag.DueDateDefault );

                box.IfValidProperty( nameof( box.Bag.DueDateOffset ),
                    () => entity.DueDateOffset = box.Bag.DueDateOffset );
            }

            box.IfValidProperty( nameof( box.Bag.IsStudentCommentingEnabled ),
                () => entity.IsStudentCommentingEnabled = box.Bag.IsStudentCommentingEnabled );

            box.IfValidProperty( nameof( box.Bag.Order ),
                () => entity.Order = box.Bag.Order );

            box.IfValidProperty( nameof( box.Bag.Points ),
                () => entity.Points = box.Bag.Points );

            box.IfValidProperty( nameof( box.Bag.SendNotificationCommunication ),
                () => entity.SendNotificationCommunication = box.Bag.SendNotificationCommunication );

            box.IfValidProperty( nameof( box.Bag.CompletionWorkflowType ),
                () => entity.CompletionWorkflowTypeId = box.Bag.CompletionWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.TaskBinaryFile ),
                () => entity.TaskBinaryFileId = box.Bag.TaskBinaryFile.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            // Update this last in case the component needs to access any of the
            // updated entity property values.
            box.IfValidProperty( nameof( box.Bag.ComponentSettings ), () =>
            {
                var entityType = EntityTypeCache.Get( entity.LearningActivity.ActivityComponentId );
                var component = LearningActivityContainer.GetComponent( entityType.Name );
                var componentData = component.GetComponentSettings( entity, box.Bag.ComponentSettings, RockContext, RequestContext )
                    ?? new Dictionary<string, string>();

                entity.LearningActivity.ActivityComponentSettingsJson = componentData.ToJson();
            } );

            return true;
        }

        /// <inheritdoc/>
        protected override LearningClassActivity GetInitialEntity()
        {
            var entityId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassActivityId );

            // If a zero identifier is specified then create a new entity.
            if ( entityId == 0 )
            {
                return GetDefaultEntity();
            }

            var entityService = new LearningClassActivityService( RockContext );

            return entityService.Queryable()
                .AsNoTracking()
                .Include( a => a.LearningClass )
                .Include( a => a.CompletionWorkflowType )
                .FirstOrDefault( a => a.Id == entityId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId )
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams )
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out LearningClassActivity entity, out BlockActionResult error )
        {
            var entityService = new LearningClassActivityService( RockContext );
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
                entity = GetDefaultEntity();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LearningClassActivity.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {LearningClassActivity.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningClassActivityId ) ?? "";

            // Exclude the auto edit and return URL parameters from the page reference parameters (if any).
            var excludedParamKeys = new[] { PageParameterKey.AutoEdit.ToLower(), PageParameterKey.ReturnUrl.ToLower() };
            var paramsToInclude = pageReference.Parameters
                .Where( kv => !excludedParamKeys.Contains( kv.Key.ToLower() ) )
                .ToDictionary( kv => kv.Key, kv => kv.Value );

            var entityName = entityKey.Length > 0
                ? new LearningClassActivityService( RockContext ).GetSelect( entityKey, p => p.LearningActivity.Name )
                : "New Activity";
            var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, paramsToInclude );
            var breadCrumb = new BreadCrumbLink( entityName ?? "New Activity", breadCrumbPageRef );

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

            return ActionOk( new ValidPropertiesBox<LearningClassActivityBag>
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
        public BlockActionResult Save( ValidPropertiesBox<LearningClassActivityBag> box )
        {
            var entityService = new LearningClassActivityService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Get the previous Available & DueDates in case we need it for
            // updating existing LearningClassActivityCompletion records.
            var previousAvailableDate = entity.AvailableDateCalculated;
            var previousDueDate = entity.DueDateCalculated;

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            var newAvailableDate = entity.AvailableDateCalculated;
            var newDueDate = entity.DueDateCalculated;

            // Check to see if we should also update LearningClassActivityCompletion records.
            // If the available dates changes
            // or if the due date changed and we have a due date change type.
            var updateAvailableDates = previousAvailableDate != newAvailableDate;
            var updateDueDates = previousDueDate != newDueDate && box.Bag.DueDateChangeType.HasValue;

            // Ensure everything is valid before saving.
            if ( !ValidateLearningClassActivity( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;
            if ( isNew )
            {
                entity.LearningClassId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassId );
            }
            else if ( updateAvailableDates || updateDueDates )
            {
                // If there is a change to the available or due date for an
                // existing LearningClassActivity we need to update any those completions
                // to use the new date.
                var allCompletions = new LearningClassActivityCompletionService( RockContext )
                    .Queryable()
                    .Where( c => c.LearningClassActivityId == entity.Id );

                foreach ( var c in allCompletions )
                {
                    if ( updateAvailableDates )
                    {
                        c.AvailableDateTime = newAvailableDate;
                    }

                    if ( updateDueDates )
                    {
                        // Update the DueDate if we're updating all records
                        // or if this record matches the previous value -
                        // per the user provided DueDateChangeType.
                        if ( box.Bag.DueDateChangeType == DueDateChangeType.UpdateAll || c.DueDate.Value.Date == previousDueDate.Value.Date )
                        {
                            c.DueDate = newDueDate;
                        }
                    }
                }
            }

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.LearningClassActivityId] = entity.IdKey
                } ) );
            }

            var returnPageUrl = PageParameter( PageParameterKey.ReturnUrl ) ?? string.Empty;
            if ( returnPageUrl.Length > 0 )
            {
                return ActionOk( returnPageUrl );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<LearningClassActivityBag>
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
            var entityService = new LearningClassActivityService( RockContext );

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

        /// <summary>
        /// Copy the Activity to create as a new Activity
        /// </summary>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return ActionNotFound();
            }

            var copiedEntity = new LearningClassActivityService( new RockContext() ).Copy( key );

            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId ),
                [PageParameterKey.LearningClassActivityId] = copiedEntity.IdKey
            };

            return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( queryParams ) );
        }

        #endregion
    }
}
