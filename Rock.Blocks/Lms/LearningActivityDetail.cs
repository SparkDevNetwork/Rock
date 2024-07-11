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
using Rock.Lms;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Blocks.Lms.LearningActivityDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning activity.
    /// </summary>

    [DisplayName( "Learning Activity Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning activity." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "fe13bfef-6266-4667-b51f-01af8e6c5b89" )]
    [Rock.SystemGuid.BlockTypeGuid( "4b18bf0d-d91b-4934-ac2d-a7188b15b893" )]
    public class LearningActivityDetail : RockEntityDetailBlockType<LearningActivity, LearningActivityBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LearningActivityId = "LearningActivityId";
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningClassId = "LearningClassId";
            public const string CloneId = "CloneId";
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
            var box = new DetailBlockBox<LearningActivityBag, LearningActivityDetailOptionsBag>();

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
        private LearningActivityDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new LearningActivityDetailOptionsBag();

            var components = LearningActivityContainer.Instance.Components;

            // Get the list of Activity Components (for rendering the actual Obsidian component.
            options.ActivityTypes = components.Select( component => new LearningActivityComponentBag
            {
                Name = component.Value.Value.Name,
                ComponentUrl = component.Value.Value.ComponentUrl,
                HighlightColor = component.Value.Value.HighlightColor,
                IconCssClass = component.Value.Value.IconCssClass,
                IdKey = component.Value.Value.EntityType.IdKey,
                Guid = component.Value.Value.EntityType.Guid.ToString()
            } ).ToList();

            // Get a list of Activity Types for the user to select from.
            options.ActivityTypeListItems = options.ActivityTypes.Select( a => new ListItemBag
            {
                Value = a.Guid,
                Text = a.Name
            } ).ToList();

            options.HasCompletions = ActivityHasCompletions();

            return options;
        }

        private bool ActivityHasCompletions()
        {
            var activityId = RequestContext.PageParameterAsId( PageParameterKey.LearningActivityId );
            return new LearningActivityCompletionService( RockContext ).Queryable().Any( c => c.LearningActivityId == activityId );
        }

        /// <summary>
        /// Validates the LearningActivity for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningActivity">The LearningActivity to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningActivity is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningActivity( LearningActivity learningActivity, out string errorMessage )
        {
            errorMessage = null;

            if ( learningActivity.Name.IsNullOrWhiteSpace() )
            {
                errorMessage = "Name is a required field";
            }
            else if ( learningActivity.ActivityComponentId == 0 )
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
        private void SetBoxInitialEntityState( DetailBlockBox<LearningActivityBag, LearningActivityDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LearningActivity.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningActivity.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningActivity.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LearningActivityBag"/> that represents the entity.</returns>
        private LearningActivityBag GetCommonEntityBag( LearningActivity entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var completionStatistics = new LearningActivityService( RockContext ).GetCompletionStatistics( entity );

            // Get the current persons info.
            var currentPerson = GetCurrentPerson();
            var isClassFacilitator = new LearningParticipantService( RockContext ).GetFacilitatorId( currentPerson.Id, entity.LearningClassId ) > 0;
            var currentPersonBag = new LearningActivityParticipantBag
            {
                Name = currentPerson.FullName,
                IdKey = currentPerson.IdKey,
                IsFacilitator = isClassFacilitator
            };

            var activityComponentBag = new LearningActivityComponentBag();
            if ( entity.ActivityComponentId > 0 )
            {
                var componentEntityType = EntityTypeCache.Get( entity.ActivityComponentId );
                var activityComponent = LearningActivityContainer.GetComponent( componentEntityType.Name );

                activityComponentBag = new LearningActivityComponentBag
                {
                    ComponentUrl = activityComponent?.ComponentUrl,
                    Guid = activityComponent?.EntityType.Guid.ToString(),
                    HighlightColor = activityComponent?.HighlightColor,
                    IconCssClass = activityComponent?.IconCssClass,
                    IdKey = activityComponent?.EntityType.IdKey,
                    Name = activityComponent?.Name,
                };
            }

            return new LearningActivityBag
            {
                IdKey = entity.IdKey,
                ActivityComponent = activityComponentBag,
                ActivityComponentSettingsJson = entity.ActivityComponentSettingsJson,
                AssignTo = entity.AssignTo,
                AvailableDateCalculationMethod = entity.AvailableDateCalculationMethod,
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
                Description = entity.Description,
                DescriptionAsHtml = entity.Description.IsNotNullOrWhiteSpace() ? new StructuredContentHelper( entity.Description ).Render() : string.Empty,
                DueDateCalculationMethod = entity.DueDateCalculationMethod,
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

        /// <inheritdoc/>
        protected override LearningActivityBag GetEntityBagForView( LearningActivity entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        //// <inheritdoc/>
        protected override LearningActivityBag GetEntityBagForEdit( LearningActivity entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( LearningActivity entity, ValidPropertiesBox<LearningActivityBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            // Don't allow edits to these properties once an activity has been completed.
            // Doing so could cause unexpected behavior because configuration is done in JSON.
            if ( !ActivityHasCompletions() )
            {
                box.IfValidProperty( nameof( box.Bag.ActivityComponent ),
                () =>
                {
                    var componentEntityTypeId = Rock.Utility.IdHasher.Instance.GetId( box.Bag?.ActivityComponent?.IdKey );
                    if ( componentEntityTypeId.HasValue )
                        entity.ActivityComponentId = componentEntityTypeId.Value;
                } );

                box.IfValidProperty( nameof( box.Bag.ActivityComponentSettingsJson ),
                    () => entity.ActivityComponentSettingsJson = box.Bag.ActivityComponentSettingsJson );

                box.IfValidProperty( nameof( box.Bag.AssignTo ),
                    () => entity.AssignTo = box.Bag.AssignTo );

                box.IfValidProperty( nameof( box.Bag.AvailableDateCalculationMethod ),
                    () => entity.AvailableDateCalculationMethod = box.Bag.AvailableDateCalculationMethod );

                box.IfValidProperty( nameof( box.Bag.AvailableDateDefault ),
                    () => entity.AvailableDateDefault = box.Bag.AvailableDateDefault );

                box.IfValidProperty( nameof( box.Bag.AvailableDateOffset ),
                    () => entity.AvailableDateOffset = box.Bag.AvailableDateOffset );

                box.IfValidProperty( nameof( box.Bag.DueDateCalculationMethod ),
                    () => entity.DueDateCalculationMethod = box.Bag.DueDateCalculationMethod );

                box.IfValidProperty( nameof( box.Bag.DueDateDefault ),
                    () => entity.DueDateDefault = box.Bag.DueDateDefault );

                box.IfValidProperty( nameof( box.Bag.DueDateOffset ),
                    () => entity.DueDateOffset = box.Bag.DueDateOffset );

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

                        entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                    } );

            }

            return true;
        }

        /// <inheritdoc/>
        protected override LearningActivity GetInitialEntity()
        {
            var entityId = RequestContext.PageParameterAsId( PageParameterKey.LearningActivityId );

            // If a zero identifier is specified then create a new entity.
            if ( entityId == 0 )
            {
                return new LearningActivity
                {
                    Id = 0,
                    Guid = Guid.Empty
                };
            }

            var entityService = new LearningActivityService( RockContext );

            return entityService.Queryable().AsNoTracking().Include( a => a.CompletionWorkflowType ).FirstOrDefault( a => a.Id == entityId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out LearningActivity entity, out BlockActionResult error )
        {
            var entityService = new LearningActivityService( RockContext );
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
                entity = new LearningActivity();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LearningActivity.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${LearningActivity.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningActivityId ) ?? "";

                var entityName = entityKey.Length > 0 ? new Service<LearningActivity>( rockContext ).GetSelect( entityKey, p => p.Name ) : "New Activity";
                var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, pageReference.Parameters );
                var breadCrumb = new BreadCrumbLink( entityName ?? "New Activity", breadCrumbPageRef );

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

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<LearningActivityBag>
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
        public BlockActionResult Save( ValidPropertiesBox<LearningActivityBag> box )
        {
            var entityService = new LearningActivityService( RockContext );

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
            if ( !ValidateLearningActivity( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;
            if ( isNew )
            {
                entity.LearningClassId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassId );
            }

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.LearningActivityId] = entity.IdKey
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

            return ActionOk( new ValidPropertiesBox<LearningActivityBag>
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
            var entityService = new LearningActivityService( RockContext );

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

            var copiedEntity = new LearningActivityService( new RockContext() ).Copy( key );

            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId ),
                [PageParameterKey.LearningActivityId] = copiedEntity.IdKey
            };

            return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( queryParams ) );
        }


        #endregion
    }
}
