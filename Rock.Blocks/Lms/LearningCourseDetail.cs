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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningCourseDetail;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning course.
    /// </summary>

    [DisplayName( "Learning Course Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning course." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Activity Detail Page",
        Description = "The page that will be navigated to when clicking an activity row.",
        Key = AttributeKey.ActivityDetailPage,
        Order = 1 )]

    [LinkedPage( "Student Detail Page",
        Description = "The page that will be navigated to when clicking a student row.",
        Key = AttributeKey.StudentDetailPage,
        Order = 2 )]

    [LinkedPage( "Facilitator Detail Page",
        Description = "The page that will be navigated to when clicking a student row.",
        Key = AttributeKey.FacilitatorDetailPage,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "cb48c60a-e518-42e8-aa52-6a549a1a4152" )]
    [Rock.SystemGuid.BlockTypeGuid( "94c4cb0b-5617-4f46-b902-6e6dd4a447b8" )]
    public class LearningCourseDetail : RockDetailBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ActivityDetailPage = "ActivityDetailPage";
            public const string FacilitatorDetailPage = "FacilitatorDetailPage";
            public const string StudentDetailPage = "StudentDetailPage";
        }

        private static class PageParameterKey
        {
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningClassId = "LearningClassId";
            public const string LearningProgramId = "LearningProgramId";
        }

        private static class NavigationUrlKey
        {
            public const string ActivityDetailPage = "ActivityDetailPage";
            public const string FacilitatorDetailPage = "FacilitatorDetailPage";
            public const string ParentPage = "ParentPage";
            public const string StudentDetailPage = "StudentDetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<LearningCourseBag, LearningCourseDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                var entity = box.Entity;

                box.NavigationUrls = GetBoxNavigationUrls( entity );
                box.Options = GetBoxOptions( entity, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<LearningCourse>();

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
        private LearningCourseDetailOptionsBag GetBoxOptions( LearningCourseBag entity, RockContext rockContext )
        {
            // Get the ConfigurationMode for the parent Program.
            var courseId = IdHasher.Instance.GetId( entity.IdKey );
            var options = new LearningClassService( rockContext )
                .Queryable()
                .Include( c => c.LearningCourse )
                .Include( c => c.GroupType )
                .Where( c => c.Id == courseId )
                .OrderBy( c => c.Order )
                .Select( c => new LearningCourseDetailOptionsBag
                {
                    ConfigurationMode = c.LearningCourse.LearningProgram.ConfigurationMode,
                    TakesAttendance = c.GroupType.TakesAttendance,
                } ).FirstOrDefault();

            return options;
        }

        /// <summary>
        /// Validates the LearningCourse for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningCourse">The LearningCourse to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningCourse is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningCourse( LearningCourse learningCourse, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<LearningCourseBag, LearningCourseDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LearningCourse.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningCourse.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningCourse.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LearningCourseBag"/> that represents the entity.</returns>
        private LearningCourseBag GetCommonEntityBag( LearningCourse entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new LearningCourseBag
            {
                IdKey = entity.IdKey,
                DefaultLearningClassIdKey = entity.LearningClasses?.OrderBy( c => c.Order ).FirstOrDefault( c => c.IsActive )?.IdKey,
                AllowHistoricalAccess = entity.AllowHistoricalAccess,
                Category = entity.Category?.ToListItemBag(),
                CategoryColor = entity.Category?.HighlightColor,
                CompletionWorkflowType = entity.CompletionWorkflowType?.ToListItemBag(),
                CourseCode = entity.CourseCode,
                Credits = entity.Credits,
                Description = entity.Description,
                EnableAnnouncements = entity.EnableAnnouncements,
                ImageBinaryFile = entity.ImageBinaryFile?.ToListItemBag(),
                IsActive = entity.IsActive,
                IsPublic = entity.IsPublic,
                MaxStudents = entity.MaxStudents,
                Name = entity.Name,
                PublicName = entity.PublicName,
                Summary = entity.Summary,
                ProgramHighlightColor = entity.LearningProgram?.HighlightColor,
                ProgramIconCssClass = entity.LearningProgram?.IconCssClass,
                CourseRequirements = entity.LearningCourseRequirements?.Select( r =>
                    new LearningCourseRequirementBag
                    {
                        LearningCourseRequirementIdKey = r.IdKey,
                        LearningCourseIdKey = entity.IdKey,
                        RequiredLearningCourseIdKey = IdHasher.Instance.GetHash( r.RequiredLearningCourseId ),
                        RequiredLearningCourseName = r.RequiredLearningCourse.Name,
                        RequiredLearningCourseCode = r.RequiredLearningCourse.CourseCode,
                        RequirementType = r.RequirementType
                    } ).ToList()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="LearningCourseBag"/> that represents the entity.</returns>
        private LearningCourseBag GetEntityBagForView( LearningCourse entity )
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
        /// <returns>A <see cref="LearningCourseBag"/> that represents the entity.</returns>
        private LearningCourseBag GetEntityBagForEdit( LearningCourse entity )
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
        private bool UpdateEntityFromBox( LearningCourse entity, DetailBlockBox<LearningCourseBag, LearningCourseDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.AllowHistoricalAccess ),
                () => entity.AllowHistoricalAccess = box.Entity.AllowHistoricalAccess );

            box.IfValidProperty( nameof( box.Entity.Category ),
                () => entity.CategoryId = box.Entity.Category.GetEntityId<Category>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.CompletionWorkflowType ),
                () => entity.CompletionWorkflowTypeId = box.Entity.CompletionWorkflowType.GetEntityId<WorkflowType>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.CourseCode ),
                () => entity.CourseCode = box.Entity.CourseCode );

            box.IfValidProperty( nameof( box.Entity.Credits ),
                () => entity.Credits = box.Entity.Credits );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.EnableAnnouncements ),
                () => entity.EnableAnnouncements = box.Entity.EnableAnnouncements );

            box.IfValidProperty( nameof( box.Entity.ImageBinaryFile ),
                () => entity.ImageBinaryFileId = box.Entity.ImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.IsPublic ),
                () => entity.IsPublic = box.Entity.IsPublic );

            box.IfValidProperty( nameof( box.Entity.MaxStudents ),
                () => entity.MaxStudents = box.Entity.MaxStudents );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.PublicName ),
                () => entity.PublicName = box.Entity.PublicName );

            box.IfValidProperty( nameof( box.Entity.Summary ),
                () => entity.Summary = box.Entity.Summary );

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
        /// <returns>The <see cref="LearningCourse"/> to be viewed or edited on the page.</returns>
        private LearningCourse GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<LearningCourse, LearningCourseService>( rockContext, PageParameterKey.LearningCourseId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( LearningCourseBag entity)
        {
            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.LearningProgramId, PageParameter( PageParameterKey.LearningProgramId ) },
                { PageParameterKey.LearningCourseId, PageParameter( PageParameterKey.LearningCourseId ) },
                { PageParameterKey.LearningClassId, entity.DefaultLearningClassIdKey }
            };

            var activityParams = new Dictionary<string, string>
            {
                { PageParameterKey.LearningProgramId, PageParameter( PageParameterKey.LearningProgramId ) },
                { PageParameterKey.LearningCourseId, PageParameter( PageParameterKey.LearningCourseId ) },
                { PageParameterKey.LearningClassId, entity.DefaultLearningClassIdKey },
                { "LearningActivityId", "((Key))" }
            };

            var participantParams = new Dictionary<string, string>
            {
                { PageParameterKey.LearningProgramId, PageParameter( PageParameterKey.LearningProgramId ) },
                { PageParameterKey.LearningCourseId, PageParameter( PageParameterKey.LearningCourseId ) },
                { PageParameterKey.LearningClassId, entity.DefaultLearningClassIdKey },
                { "LearningParticipantId", "((Key))" }
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ActivityDetailPage] = this.GetLinkedPageUrl( AttributeKey.ActivityDetailPage, activityParams ),
                [NavigationUrlKey.FacilitatorDetailPage] = this.GetLinkedPageUrl( AttributeKey.FacilitatorDetailPage, participantParams ),
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams ),
                [NavigationUrlKey.StudentDetailPage] = this.GetLinkedPageUrl( AttributeKey.StudentDetailPage, participantParams )
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
        private string GetSecurityGrantToken( LearningCourse entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out LearningCourse entity, out BlockActionResult error )
        {
            var entityService = new LearningCourseService( rockContext );
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
                entity = new LearningCourse();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LearningCourse.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${LearningCourse.FriendlyTypeName}." );
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

                var box = new DetailBlockBox<LearningCourseBag, LearningCourseDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<LearningCourseBag, LearningCourseDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LearningCourseService( rockContext );

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
                if ( !ValidateLearningCourse( entity, rockContext, out var validationMessage ) )
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
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.LearningCourseId] = entity.IdKey
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
                var entityService = new LearningCourseService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<LearningCourseBag, LearningCourseDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<LearningCourseBag, LearningCourseDetailOptionsBag>
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

        /// <summary>
        /// Gets a list of active courses which the current user is authorized to view.
        /// </summary>
        /// <param name="key">The identifier of the course the list will be shown for.</param>
        /// <returns>A list of Courses</returns>
        [BlockAction]
        public BlockActionResult GetActiveCourses( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LearningCourseService( rockContext );

                var currentRequirementIds = entityService.GetQueryableByKey( key )
                    .Include( c => c.LearningCourseRequirements )
                    .SelectMany( c => c.LearningCourseRequirements.Select( r => r.RequiredLearningCourseId ) )
                    .ToList();

                var currentId = Rock.Utility.IdHasher.Instance.GetId( key );

                if ( !currentId.HasValue || currentId.Value == 0 )
                {
                    return ActionBadRequest( "Invalid key parameter" );
                }

                currentRequirementIds.Add( currentId.Value );

                // Return all courses that the current user is authorized to view.
                var allCourses = entityService.Queryable()
                    .Where( c => c.IsActive )
                    .Where( c => !currentRequirementIds.Contains( c.Id ) )
                    .ToList()
                    // Make sure the current user is authorized to view the 
                    .Where( c => c.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .ToListItemBagList();

                return ActionOk( allCourses );
            }
        }

        /// <summary>
        /// Add a requirement to a course.
        /// </summary>
        /// <param name="key">The identifier of the course the requirement will be added to.</param>
        /// <param name="guid">The Guid identifier of the required course.</param>
        /// <param name="requirementType">The type of requirement for the required course.</param>
        /// <returns>The Newly created course requirement.</returns>
        [BlockAction]
        public BlockActionResult AddCourseRequirement( string key, string guid, RequirementType requirementType )
        {
            using ( var rockContext = new RockContext() )
            {
                var courseService = new LearningCourseService( rockContext );
                var courseId = IdHasher.Instance.GetId( key );

                // Make sure the Guid of the required course is valid.
                if ( !Guid.TryParse( guid, out var requiredCourseGuid ) )
                {
                    return ActionBadRequest( $"Required {nameof( guid )} was invalid." );
                }

                // Make sure the Id of the course with a requirement is valid.
                if ( courseId == null )
                {
                    return ActionBadRequest( $"Required {nameof( key )} was invalid." );
                }

                // Make sure both courses exist.
                var relatedCourses = courseService.Queryable()
                    .Where( c => c.Id == courseId || c.Guid == requiredCourseGuid )
                    .Select( c => new
                    {
                        c.Id,
                        c.CourseCode,
                        c.Name
                    } );

                // If there aren't exactly 2 courses we've probably got an incorrect Id
                // or a change has been made.
                if ( relatedCourses.Count() != 2 )
                {
                    return ActionBadRequest( $"One or more {LearningCourse.FriendlyTypeName.Pluralize()} not found." );
                }

                // Separate out the parent and required courses.
                var parentCourse = relatedCourses.First( c => c.Id == courseId );
                var requiredCourse = relatedCourses.First( c => c.Id != courseId );

                var requirementService = new LearningCourseRequirementService( rockContext );

                var newCourseRequirement = new LearningCourseRequirement
                {
                    LearningCourseId = parentCourse.Id,
                    RequiredLearningCourseId = requiredCourse.Id,
                    RequirementType = requirementType
                };

                requirementService.Add( newCourseRequirement );

                rockContext.SaveChanges();

                return ActionOk( new LearningCourseRequirementBag
                {
                    LearningCourseRequirementIdKey = newCourseRequirement.IdKey,
                    RequiredLearningCourseIdKey = Rock.Utility.IdHasher.Instance.GetHash( requiredCourse.Id ),
                    RequiredLearningCourseCode = requiredCourse.CourseCode,
                    RequiredLearningCourseName = requiredCourse.Name,
                    LearningCourseIdKey = key,
                    RequirementType = requirementType
                } );
            }
        }

        /// <summary>
        /// Deletes the specified course requirement.
        /// </summary>
        /// <param name="key">The identifier of the course requirement to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult DeleteCourseRequirement( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LearningCourseRequirementService( rockContext );

                var entity = entityService.Get( key );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{LearningCourseRequirement.FriendlyTypeName} not found." );
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

        #endregion

        /*
            2024/04/04 - JSC

            To match mock-ups of the Course Detail block (On-Demand mode)
            we had to embed several "list blocks" in the LearningCourseDetail component.
            Because each Obsidian block requires an instance of that block type on the page
            we couldn't create re-usable blocks, but instead had to route their block actions
            through the Course Detail block. We tried to componentize these grids as much as possible.
            If at a later time - we have a method of adding blocks to a tabbed control in Obsidian
            we can look at refactoring this code.
	
            Reason: Unable to embed child blocks in an Obsidian block.
        */
        #region Block Actions for Learning Activities, Students and Facilitators grids

        /// <summary>
        /// Gets a list of activities for the current course.
        /// </summary>
        /// <returns>A list of Courses</returns>
        [BlockAction]
        public BlockActionResult GetLearningPlan()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity == null )
                {
                    return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} was not found." );
                }

                var now = DateTime.Now;

                // Return all activities for the course.
                var gridBuilder = new GridBuilder<LearningActivity>()
                    .AddTextField( "idKey", a => a.IdKey )
                    .AddTextField( "name", a => a.Name )
                    .AddField( "assignTo", a => a.AssignTo )
                    .AddField( "type", a => a.ActivityComponentId )
                    .AddField( "dates", a => a.DatesDescription )
                    .AddField( "isPastDue", a => a.DueDateCalculated == null ? false : a.DueDateCalculated >= now )
                    .AddField( "count", a => a.LearningActivityCompletions.Count() )
                    .AddField( "completedCount", a => a.LearningActivityCompletions.Count( c => c.IsStudentCompleted ) )
                    .AddField( "componentIconCssClass", a => "fa fa-list" )
                    .AddField( "componentHighlightColor", a => "#735f95" )
                    .AddField( "componentName", a => "Check-Off" )
                    .AddField( "points", a => a.Points )
                    .AddField( "isAttentionNeeded", a => a.LearningActivityCompletions.Any( c => c.IsStudentCompleted && !c.IsFacilitatorCompleted ) )
                    .AddField( "hasStudentComments", a => a.LearningActivityCompletions.Any( c => c.StudentComment.ToStringSafe().Length > 0 ) );

                var orderedItems = GetOrderedLearningPlan( rockContext ).AsNoTracking();
                return ActionOk( gridBuilder.Build( orderedItems ) );
            }
        }

        /// <summary>
        /// Changes the ordered position of a single activity.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderActivity( string key, string beforeKey )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var items = GetOrderedLearningPlan( rockContext ).ToList();

                if ( !items.ReorderEntity( key, beforeKey ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Deletes the specified activity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult DeleteActivity( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LearningActivityService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{LearningActivity.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${LearningActivity.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Gets a list of students for the current class (default of the course).
        /// </summary>
        /// <returns>A list of students</returns>
        [BlockAction]
        public BlockActionResult GetStudents(bool includeAbsences = false)
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity == null )
                {
                    return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} was not found." );
                }

                var defaultClass = entity.LearningClasses.FirstOrDefault();

                if ( defaultClass == null )
                {
                    return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} has no classes." );
                }

                // Return all students for the course's default class.
                var gridBuilder = new GridBuilder<LearningParticipant>()
                    .AddTextField( "idKey", a => a.IdKey )
                    .AddPersonField( "name", a => a.Person )
                    .AddField( "currentGradePercent", a =>
                        a.LearningGradePercent )
                    .AddField( "currentGrade", a =>
                        a.LearningGradingSystemScale?.Name )
                    .AddTextField( "currentAssignment", a =>
                        a.LearningActivities
                        .OrderBy( t => t.DueDate )
                        .FirstOrDefault( t => !t.IsStudentCompleted )?
                        .LearningActivity?.Name );

                if ( includeAbsences )
                {
                    var groupAttendance = new AttendanceService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( a =>
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value &&
                        a.Occurrence.GroupId == defaultClass.Id &&
                        a.PersonAlias != null)
                    .GroupBy( a => a.PersonAlias.PersonId )
                    .ToList();

                    gridBuilder = gridBuilder
                        .AddField( "absences", a =>
                            groupAttendance.Any() ?
                            groupAttendance.FirstOrDefault( g => g.Key == a.Person.Id )
                            .Count() : 0 )
                        .AddField( "absencesLabelStyle", a =>
                            a.LearningClass.AbsencesLabelStyle(
                                groupAttendance.Any() ?
                                groupAttendance.FirstOrDefault( g => g.Key == a.Person.Id )
                                .Count() : 0,
                                entity.LearningProgram ) );
                }

                var students = new LearningParticipantService( rockContext )
                    .GetStudents( defaultClass.Id )
                    .AsNoTracking()
                    .ToList();

                return ActionOk( gridBuilder.Build( students ) );
            }
        }

        /// <summary>
        /// Gets a list of activities for the current course.
        /// </summary>
        /// <returns>A list of Courses</returns>
        [BlockAction]
        public BlockActionResult GetFacilitators()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity == null )
                {
                    return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} was not found." );
                }

                var defaultClass = entity.LearningClasses.FirstOrDefault();

                if ( defaultClass == null )
                {
                    return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} has no classes." );
                }

                var facilitators = new LearningParticipantService( rockContext )
                    .GetFacilitators( defaultClass.Id )
                    .AsNoTracking()
                    .ToList();

                // Return all facilitators for the course's default class.
                var gridBuilder = new GridBuilder<LearningParticipant>()
                    .AddTextField( "idKey", a => a.IdKey )
                    .AddPersonField( "name", a => a.Person );

                return ActionOk( gridBuilder.Build( facilitators ) );
            }
        }

        /// <summary>
        /// Deletes the specified participant.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult DeleteParticipant( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LearningParticipantService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{LearningParticipant.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${LearningParticipant.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion

        #region Private methods

        private IQueryable<LearningActivity> GetOrderedLearningPlan( RockContext rockContext )
        {
            var allowIdParameters = !PageCache.Layout.Site.DisablePredictableIds;

            // Get the page parameter value (either IdKey or Id).
            var classParameterValue = PageParameter( PageParameterKey.LearningClassId );

            // Parse out the Id if the parameter is an IdKey or take the Id
            // If the site allows predictable Ids in parameters.
            var classId =
                classParameterValue.IsDigitsOnly() && allowIdParameters ?
                classParameterValue.ToIntSafe() :
                IdHasher.Instance.GetId( classParameterValue ).ToIntSafe();

            var contextClass = RequestContext?.GetContextEntity<LearningClass>();
            var filteredClassId = classId > 0 ? classId : contextClass?.Id ?? 0;

            if ( filteredClassId > 0 )
            {
                return new LearningActivityService( rockContext )
                    .GetClassLearningPlan( a => a.LearningClassId == filteredClassId )
                    .AsNoTracking();
            }

            // Get the page parameter value (either IdKey or Id).
            var courseParameterValue = PageParameter( PageParameterKey.LearningCourseId );

            // Parse out the Id if the parameter is an IdKey or take the Id
            // If the site allows predictable Ids in parameters.
            var courseId =
                courseParameterValue.IsDigitsOnly() && allowIdParameters ?
                courseParameterValue.ToIntSafe() :
                IdHasher.Instance.GetId( courseParameterValue ).ToIntSafe();

            var contextCourse= RequestContext?.GetContextEntity<LearningCourse>();
            int filteredCourseId = courseId > 0 ? courseId : contextCourse?.Id ?? 0;

            if ( filteredCourseId > 0 )
            {
                // Get the default class (prevents duplicates from showing in the activity list).
                var defaultClassId = new LearningClassService( rockContext ).GetCourseDefaultClass( filteredCourseId, c => c.Id );

                return new LearningActivityService( rockContext )
                    .GetClassLearningPlan( a => a.LearningClassId == defaultClassId )
                    .AsNoTracking();
            }

            return new List<LearningActivity>().AsQueryable();
        }

        #endregion
    }
}
