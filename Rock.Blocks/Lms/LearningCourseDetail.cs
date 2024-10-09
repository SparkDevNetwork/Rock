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
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningCourseDetail;
using Rock.ViewModels.Blocks.Lms.LearningCourseRequirement;
using Rock.ViewModels.Blocks.Lms.LearningParticipantDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
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
        IsRequired = false,
        Order = 1 )]

    [LinkedPage( "Participant Detail Page",
        Description = "The page that will be navigated to when clicking a student row.",
        Key = AttributeKey.ParticipantDetailPage,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage( "Content Page Detail Page",
        Description = "The page that will be navigated to when clicking a content page row.",
        Key = AttributeKey.ContentPageDetailPage,
        IsRequired = false,
        Order = 3 )]

    [LinkedPage( "Announcement Detail Page",
        Description = "The page that will be navigated to when clicking an announcement row.",
        Key = AttributeKey.AnnouncementDetailPage,
        IsRequired = false,
        Order = 4 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "cb48c60a-e518-42e8-aa52-6a549a1a4152" )]
    [Rock.SystemGuid.BlockTypeGuid( "94c4cb0b-5617-4f46-b902-6e6dd4a447b8" )]
    public class LearningCourseDetail : RockEntityDetailBlockType<LearningCourse, LearningCourseBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ActivityDetailPage = "ActivityDetailPage";
            public const string AnnouncementDetailPage = "AnnouncementDetailPage";
            public const string ContentPageDetailPage = "ContentPageDetailPage";
            public const string ParticipantDetailPage = "ParticipantDetailPage";
        }

        private static class PageParameterKey
        {
            public const string LearningActivityId = "LearningActivityId";
            public const string LearningClassId = "LearningClassId";
            public const string LearningClassContentPageId = "LearningClassContentPageId";
            public const string LearningClassAnnouncementId = "LearningClassAnnouncementId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningParticipantId = "LearningParticipantId";
            public const string LearningProgramId = "LearningProgramId";
        }

        private static class NavigationUrlKey
        {
            public const string ActivityDetailPage = "ActivityDetailPage";
            public const string AnnouncementDetailPage = "AnnouncementDetailPage";
            public const string ContentPageDetailPage = "ContentPageDetailPage";
            public const string ParentPage = "ParentPage";
            public const string ParticipantDetailPage = "ParticipantDetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<LearningCourseBag, LearningCourseDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            var entity = box.Entity;

            box.NavigationUrls = GetBoxNavigationUrls( entity );
            box.Options = GetBoxOptions( entity );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private LearningCourseDetailOptionsBag GetBoxOptions( LearningCourseBag entity )
        {
            var disablePredictableIds = PageCache.Layout?.Site?.DisablePredictableIds ?? false;
            // Get the ConfigurationMode for the parent Program.
            var programConfigurationMode = new LearningCourseService( RockContext )
                .GetSelect( entity.IdKey, c => c.LearningProgram.ConfigurationMode, !disablePredictableIds );

            return new LearningCourseDetailOptionsBag
            {
                ConfigurationMode = programConfigurationMode
            };
        }

        /// <summary>
        /// Validates the LearningCourse for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningCourse">The LearningCourse to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningCourse is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningCourse( LearningCourse learningCourse, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LearningCourseBag, LearningCourseDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LearningCourse.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningCourse.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningCourse.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
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
                DescriptionAsHtml = entity.Description.IsNotNullOrWhiteSpace() ? new StructuredContentHelper( entity.Description ).Render() : string.Empty,
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
                        IdKey = r.IdKey,
                        LearningCourseIdKey = entity.IdKey,
                        RequiredLearningCourseIdKey = IdHasher.Instance.GetHash( r.RequiredLearningCourseId ),
                        RequiredLearningCourseName = r.RequiredLearningCourse?.Name,
                        RequiredLearningCourseCode = r.RequiredLearningCourse?.CourseCode,
                        RequirementType = r.RequirementType
                    } ).ToList()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="LearningCourseBag"/> that represents the entity.</returns>
        protected override LearningCourseBag GetEntityBagForView( LearningCourse entity )
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
        protected override LearningCourseBag GetEntityBagForEdit( LearningCourse entity )
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
        /// <param name="RockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        protected override bool UpdateEntityFromBox( LearningCourse entity, ValidPropertiesBox<LearningCourseBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AllowHistoricalAccess ),
                () => entity.AllowHistoricalAccess = box.Bag.AllowHistoricalAccess );

            box.IfValidProperty( nameof( box.Bag.Category ),
                () => entity.CategoryId = box.Bag.Category.GetEntityId<Category>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CompletionWorkflowType ),
                () => entity.CompletionWorkflowTypeId = box.Bag.CompletionWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CourseCode ),
                () => entity.CourseCode = box.Bag.CourseCode );

            box.IfValidProperty( nameof( box.Bag.Credits ),
                () => entity.Credits = box.Bag.Credits );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.EnableAnnouncements ),
                () => entity.EnableAnnouncements = box.Bag.EnableAnnouncements );

            box.IfValidProperty( nameof( box.Bag.ImageBinaryFile ),
                () => entity.ImageBinaryFileId = box.Bag.ImageBinaryFile.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.IsPublic ),
                () => entity.IsPublic = box.Bag.IsPublic );

            box.IfValidProperty( nameof( box.Bag.MaxStudents ),
                () => entity.MaxStudents = box.Bag.MaxStudents );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            var publicName = box.Bag.PublicName ?? box.Bag.Name;

            box.IfValidProperty( nameof( box.Bag.PublicName ),
                () => entity.PublicName = box.Bag.PublicName );

            box.IfValidProperty( nameof( box.Bag.Summary ),
                () => entity.Summary = box.Bag.Summary );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            box.IfValidProperty( nameof( box.Bag.CourseRequirements ),
                () => UpdateRequiredCourses( box.Bag, entity, RockContext ) );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="RockContext">The rock context.</param>
        /// <returns>The <see cref="LearningCourse"/> to be viewed or edited on the page.</returns>
        protected override LearningCourse GetInitialEntity()
        {
            var entityId = RequestContext.PageParameterAsId( PageParameterKey.LearningCourseId );

            // If a zero identifier is specified then create a new entity.
            if ( entityId == 0 )
            {
                return new LearningCourse
                {
                    LearningProgramId = RequestContext.PageParameterAsId( PageParameterKey.LearningProgramId ),
                    Id = 0,
                    Guid = Guid.Empty,
                    IsActive = true
                };
            }

            return new LearningCourseService( RockContext ).GetCourseWithRequirements( entityId );
        }

        /// <summary>
        /// Gets the current page parameters for the specified class identifier and
        /// optionally include a '((Key))' placeholder with the specified <paramref name="keyPlaceholderParameterName"/>.
        /// </summary>
        /// <param name="classIdKey">The identifier of the LearningClass parameter.</param>
        /// <param name="keyPlaceholderParameterName">The optional parameter name whose value would be dynamically set by the Obsidian Grid.</param>
        /// <returns>A Dictionary of Route/Query Parameters with the specified values.</returns>
        private Dictionary<string, string> GetCurrentPageParams( string classIdKey, string keyPlaceholderParameterName = "" )
        {
            if ( !string.IsNullOrWhiteSpace( keyPlaceholderParameterName ) )
            {
                return new Dictionary<string, string>
                {
                    [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                    [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                    [PageParameterKey.LearningClassId] = classIdKey,
                    [keyPlaceholderParameterName] = "((Key))"
                };
            }

            return new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = classIdKey,
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <remarks>
        /// Each of the Navigation pages that's a part of the learningClassSecondaryLists Obsidian component should have their own ((Key)) placeholder
        /// for their respective parameter keys. This is because the Obsidian component is responsible for replacing those values.
        /// The placeholder parameter name would be the Route or QueryString parameter name that gets sent to the page.
        /// </remarks>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( LearningCourseBag entity )
        {
            var queryParams = GetCurrentPageParams( entity.DefaultLearningClassIdKey );
            var activityParams = GetCurrentPageParams( entity.DefaultLearningClassIdKey, PageParameterKey.LearningActivityId );
            var participantParams = GetCurrentPageParams( entity.DefaultLearningClassIdKey, PageParameterKey.LearningParticipantId );
            var contentPageParams = GetCurrentPageParams( entity.DefaultLearningClassIdKey, PageParameterKey.LearningClassContentPageId );
            var announcementParams = GetCurrentPageParams( entity.DefaultLearningClassIdKey, PageParameterKey.LearningClassAnnouncementId );

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ActivityDetailPage] = this.GetLinkedPageUrl( AttributeKey.ActivityDetailPage, activityParams ),
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams ),
                [NavigationUrlKey.ParticipantDetailPage] = this.GetLinkedPageUrl( AttributeKey.ParticipantDetailPage, participantParams ),
                [NavigationUrlKey.ContentPageDetailPage] = this.GetLinkedPageUrl( AttributeKey.ContentPageDetailPage, contentPageParams ),
                [NavigationUrlKey.AnnouncementDetailPage] = this.GetLinkedPageUrl( AttributeKey.AnnouncementDetailPage, announcementParams )
            };
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningCourseId ) ?? "";

            // Exclude the auto edit and return URL parameters from the page reference parameters (if any).
            var excludedParamKeys = new[] { "autoedit", "returnurl" };
            var paramsToInclude = pageReference.Parameters.Where( kv => !excludedParamKeys.Contains( kv.Key.ToLower() ) ).ToDictionary( kv => kv.Key, kv => kv.Value );

            var entityName = entityKey.Length > 0 ? new LearningCourseService( RockContext ).GetSelect( entityKey, p => p.Name ) : "New Course";
            var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, paramsToInclude );
            var breadCrumb = new BreadCrumbLink( entityName ?? "New Course", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>
                {
                    breadCrumb
                }
            };
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        protected override bool TryGetEntityForEditAction( string idKey, out LearningCourse entity, out BlockActionResult error )
        {
            var entityService = new LearningCourseService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.GetInclude( idKey, c => c.LearningCourseRequirements, !PageCache.Layout.Site.DisablePredictableIds );
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );
            var box = new ValidPropertiesBox<LearningCourseBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            };

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<LearningCourseBag> box )
        {
            var entityService = new LearningCourseService( RockContext );

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
            if ( !ValidateLearningCourse( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            if ( isNew )
            {
                // Need to ensure the program is tied to the Course when creating a new Course.
                var learningProgramId = new LearningProgramService( RockContext )
                    .GetSelect( PageParameter( PageParameterKey.LearningProgramId ), p => p.Id, !PageCache.Layout.Site.DisablePredictableIds );

                entity.LearningProgramId = learningProgramId;
            }

            RockContext.SaveChanges();

            // Ensure navigation properties will work now.
            entity = entityService.GetCourseWithRequirements( entity.Id );
            var bag = GetEntityBagForView( entity );

            if ( isNew )
            {
                var queryParams = GetCurrentPageParams( bag.DefaultLearningClassIdKey );
                queryParams.AddOrReplace( PageParameterKey.LearningCourseId, bag.IdKey );

                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( queryParams ) );
            }

            return ActionOk( new ValidPropertiesBox<LearningCourseBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Gets a list of active courses which the current user is authorized to view.
        /// </summary>
        /// <param name="key">The identifier of the course the list will be shown for.</param>
        /// <returns>A list of Courses</returns>
        [BlockAction]
        public BlockActionResult GetActiveCourses( string key )
        {
            var entityService = new LearningCourseService( RockContext );

            var hasKey = key?.Length > 0;

            var currentRequirementIds = hasKey ?
                entityService.GetQueryableByKey( key )
                    .Include( c => c.LearningCourseRequirements )
                    .SelectMany( c => c.LearningCourseRequirements.Select( r => r.RequiredLearningCourseId ) )
                    .ToList() :
                new List<int>();

            var currentId = Rock.Utility.IdHasher.Instance.GetId( key );

            if ( hasKey )
            {
                currentRequirementIds.Add( currentId.Value );
            }

            // Return all courses that the current user is authorized to view.
            var allCourses = entityService.Queryable()
                .Where( c => c.IsActive )
                .Where( c => !currentRequirementIds.Contains( c.Id ) )
                .ToList()
                // Make sure the current user is authorized to view the course.
                .Where( c => c.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToListItemBagList();

            return ActionOk( allCourses );
        }

        /// <summary>
        /// Get the detail for a course requirement.
        /// </summary>
        /// <param name="key">The identifier of the course the requirement will be added to (for including the relationship).</param>
        /// <param name="guid">The Guid identifier of the required course.</param>
        /// <param name="requirementType">The type of requirement for the required course.</param>
        /// <returns>The <see cref="LearningCourseRequirementBag"/> containing the course requirement details.</returns>
        [BlockAction]
        public BlockActionResult GetCourseRequirementDetail( string key, string guid, RequirementType requirementType )
        {
            var courseService = new LearningCourseService( RockContext );
            var courseId = IdHasher.Instance.GetId( key ).ToIntSafe();

            // Make sure the Guid of the required course is valid.
            if ( !Guid.TryParse( guid, out var requiredCourseGuid ) )
            {
                return ActionBadRequest( $"Required {nameof( guid )} was invalid." );
            }

            // Make sure the required course exists.
            var requiredCourse = courseService.Queryable()
                .Where( c => c.Guid == requiredCourseGuid )
                .Select( c => new
                {
                    c.Id,
                    c.CourseCode,
                    c.Name
                } ).FirstOrDefault();

            if ( requiredCourse == null )
            {
                return ActionBadRequest( $"The required {LearningCourse.FriendlyTypeName} was not found." );
            }

            var newCourseRequirement = new LearningCourseRequirement
            {
                LearningCourseId = courseId,
                RequiredLearningCourseId = requiredCourse.Id,
                RequirementType = requirementType
            };

            return ActionOk( new LearningCourseRequirementBag
            {
                IdKey = newCourseRequirement.IdKey,
                RequiredLearningCourseIdKey = Rock.Utility.IdHasher.Instance.GetHash( requiredCourse.Id ),
                RequiredLearningCourseCode = requiredCourse.CourseCode,
                RequiredLearningCourseName = requiredCourse.Name,
                LearningCourseIdKey = key,
                RequirementType = requirementType
            } );
        }

        /// <summary>
        /// Deletes the specified course requirement.
        /// </summary>
        /// <param name="key">The identifier of the course requirement to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult DeleteCourseRequirement( string key )
        {
            var entityService = new LearningCourseRequirementService( RockContext );

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
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
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
        #region Secondary List/Grid Block Actions

        /// <summary>
        /// Deletes the specified activity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult DeleteActivity( string key )
        {
            var entityService = new LearningActivityService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{LearningActivity.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete ${LearningActivity.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified activity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult DeleteAnnouncement( string key )
        {
            var entityService = new LearningClassAnnouncementService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{LearningClassAnnouncement.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete ${LearningClassAnnouncement.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified activity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult DeleteContentPage( string key )
        {
            var entityService = new LearningClassContentPageService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{LearningClassContentPage.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete ${LearningClassContentPage.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified participant.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult DeleteParticipant( string key )
        {
            var entityService = new LearningParticipantService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{LearningParticipant.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete ${LearningParticipant.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets a list of announcements for the current course/class.
        /// </summary>
        /// <returns>A list of class announcements</returns>
        [BlockAction]
        public BlockActionResult GetAnnouncements()
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} was not found." );
            }

            var defaultClassId = new LearningClassService( RockContext ).GetCourseDefaultClass( entity.Id, c => c.Id );

            if ( defaultClassId == 0 )
            {
                return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} has no classes." );
            }

            var announcements = new LearningClassAnnouncementService( RockContext )
                .Queryable()
                .Where( a => a.LearningClassId == defaultClassId )
                .ToList();

            // Return all announcements for the course's default class.
            var gridBuilder = new GridBuilder<LearningClassAnnouncement>()
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "title", a => a.Title )
                .AddField( "publishDateTime", a => a.PublishDateTime )
                .AddField( "communicationMode", a => a.CommunicationMode )
                .AddField( "communicationSent", a => a.CommunicationSent );

            return ActionOk( gridBuilder.Build( announcements ) );
        }

        /// <summary>
        /// Gets a list of content pages for the current course/class.
        /// </summary>
        /// <returns>A list of class content pages</returns>
        [BlockAction]
        public BlockActionResult GetContentPages()
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} was not found." );
            }

            var defaultClassId = new LearningClassService( RockContext ).GetCourseDefaultClass( entity.Id, c => c.Id );

            if ( defaultClassId == 0 )
            {
                return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} has no classes." );
            }

            var contentPages = new LearningClassContentPageService( RockContext )
                .Queryable()
                .Where( c => c.LearningClassId == defaultClassId )
                .ToList();

            // Return all announcements for the course's default class.
            var gridBuilder = new GridBuilder<LearningClassContentPage>()
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "title", a => a.Title )
                .AddField( "startDate", a => a.StartDateTime );

            return ActionOk( gridBuilder.Build( contentPages ) );
        }

        /// <summary>
        /// Gets a list of facilitators for the current course/class.
        /// </summary>
        /// <returns>A list of facilitators</returns>
        [BlockAction]
        public BlockActionResult GetFacilitators()
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} was not found." );
            }

            var defaultClass = entity.LearningClasses.FirstOrDefault();

            if ( defaultClass == null )
            {
                return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} has no classes." );
            }

            var facilitators = new LearningParticipantService( RockContext )
                .GetFacilitators( defaultClass.Id )
                .ToList();

            // Return all facilitators for the course's default class.
            var gridBuilder = new GridBuilder<LearningParticipant>()
                .AddTextField( "idKey", a => a.IdKey )
                .AddPersonField( "name", a => a.Person )
                .AddTextField( "note", a => a.Note );

            return ActionOk( gridBuilder.Build( facilitators ) );
        }

        /// <summary>
        /// Gets a list of activities for the current course/class.
        /// </summary>
        /// <returns>A list of learning activities</returns>
        [BlockAction]
        public BlockActionResult GetLearningPlan()
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} was not found." );
            }

            var components = LearningActivityContainer.Instance.Components;
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
                .AddField( "componentIconCssClass", a => components.FirstOrDefault( c => c.Value.Value.EntityType.Id == a.ActivityComponentId ).Value.Value.IconCssClass )
                .AddField( "componentHighlightColor", a => components.FirstOrDefault( c => c.Value.Value.EntityType.Id == a.ActivityComponentId ).Value.Value.HighlightColor )
                .AddField( "componentName", a => components.FirstOrDefault( c => c.Value.Value.EntityType.Id == a.ActivityComponentId ).Value.Value.Name )
                .AddField( "points", a => a.Points )
                .AddField( "isAttentionNeeded", a => a.LearningActivityCompletions.Any( c => c.NeedsAttention ) )
                .AddField( "hasStudentComments", a => a.LearningActivityCompletions.Any( c => c.HasStudentComment ) );

            var orderedItems = GetOrderedLearningPlan().AsNoTracking();
            return ActionOk( gridBuilder.Build( orderedItems ) );
        }

        /// <summary>
        /// Gets a list of students for the current class (default of the course).
        /// </summary>
        /// <returns>A list of students</returns>
        [BlockAction]
        public BlockActionResult GetStudents()
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} was not found." );
            }

            var defaultClass = entity.LearningClasses.FirstOrDefault();

            if ( defaultClass == null )
            {
                return ActionBadRequest( $"The {LearningCourse.FriendlyTypeName} has no classes." );
            }

            var defaultClassTakesAttendance = new LearningClassService( RockContext ).Queryable()
                .Include( c => c.GroupType )
                .Any( c => c.GroupType.TakesAttendance && c.Id == defaultClass.Id );

            // Return all students for the course's default class.
            var gridBuilder = new GridBuilder<LearningParticipant>()
                .AddTextField( "idKey", a => a.IdKey )
                .AddPersonField( "name", a => a.Person )
                .AddField( "currentGradePercent", a => a.LearningGradePercent )
                .AddField( "currentGrade", a => a.LearningGradingSystemScale?.Name )
                .AddTextField( "note", a => a.Note )
                .AddTextField( "role", a => a.GroupRole.Name )
                .AddTextField( "currentAssignment", a =>
                    a.LearningActivities
                    .OrderBy( t => t.DueDate )
                    .FirstOrDefault( t => !t.IsStudentCompleted )?
                    .LearningActivity?.Name );

            if ( defaultClassTakesAttendance )
            {
                var groupAttendance = new AttendanceService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a =>
                    a.DidAttend.HasValue &&
                    a.DidAttend.Value &&
                    a.Occurrence.GroupId == defaultClass.Id &&
                    a.PersonAlias != null )
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

            var students = new LearningParticipantService( RockContext )
                .GetStudents( defaultClass.Id )
                .ToList();

            return ActionOk( gridBuilder.Build( students ) );
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
            // Get the queryable and make sure it is ordered correctly.
            var items = GetOrderedLearningPlan().ToList();

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Saves the new or updated participant for the class.
        /// </summary>
        /// <param name="participantBag">The bag containing the participant info that should be added or updated.</param>
        /// <returns>The participantBag with updated data.</returns>
        [BlockAction]
        public BlockActionResult SaveParticipant( LearningParticipantBag participantBag )
        {
            var classService = new LearningClassService( RockContext );
            var classIdKey = PageParameter( PageParameterKey.LearningClassId );

            // If the course doesn't explicitly specify which Class we're looking at
            // then get the default class for the course.
            var classId = classIdKey.IsNullOrWhiteSpace() ?
                classService.GetCourseDefaultClass(
                    RequestContext.GetPageParameter( PageParameterKey.LearningCourseId ),
                    c => c.Id ) :
                classService.GetSelect( classIdKey, p => p.Id );

            var isNew = participantBag.IdKey.IsNullOrWhiteSpace();
            var disablePredictableIds = this.PageCache.Layout.Site.DisablePredictableIds;

            var learningParticipantService = new LearningParticipantService( RockContext );
            LearningParticipant entity;

            if ( isNew )
            {
                entity = GetNewLearningParticipantFromBag( participantBag, classService, classId );
                learningParticipantService.Add( entity );
            }
            else
            {
                entity = learningParticipantService.Get( participantBag.IdKey, !disablePredictableIds );
                entity.Note = participantBag.Note;
            }

            if ( participantBag.AttributeValues != null )
            {
                // Load attributes for the entity.
                entity.LoadAttributes( RockContext );

                // Set the attribute values from the bag.
                entity.SetPublicAttributeValues( participantBag.AttributeValues, RequestContext.CurrentPerson );
            }

            RockContext.SaveChanges();

            var updatedBag = GetLearningParticipantBag( entity, learningParticipantService );

            return ActionOk( updatedBag );
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the LearningParticipantBag for the specified <paramref name="entity"/>
        /// and using the specified <paramref name="participantService"/>.
        /// </summary>
        /// <param name="entity">The <see cref="LearningParticipant"/> to convert to bag.</param>
        /// <param name="participantService">The LearningParticipantService to use for getting the updated information.</param>
        /// <returns>A LearningParticipantBag with the latest values from the database.</returns>
        private LearningParticipantBag GetLearningParticipantBag( LearningParticipant entity, LearningParticipantService participantService )
        {
            var participantDetails = participantService
                .Queryable()
                .Include( p => p.Person )
                .Include( p => p.GroupRole )
                .Include( p => p.LearningGradingSystemScale )
                .Where( p => p.Id == entity.Id )
                .FirstOrDefault();

            return new LearningParticipantBag
            {
                IdKey = entity.IdKey,
                CurrentGradePercent = participantDetails.LearningGradePercent,
                CurrentGradeText = participantDetails.LearningGradingSystemScale?.Name,
                ParticipantRole = participantDetails.GroupRole.ToListItemBag(),
                PersonAlias = participantDetails.Person.PrimaryAlias.ToListItemBag(),
                IsFacilitator = participantDetails.GroupRole.IsLeader
            };
        }

        /// <summary>
        /// Gets a new LearningParticipant based on the data in the provided bag.
        /// </summary>
        /// <param name="participantBag">The bag containing the necessary information to get a LearningParticipant.</param>
        /// <param name="classService">The <see cref="LearningClassService"/> to use for getting class roles.</param>
        /// <param name="classId">The integer identifier of the <see cref="LearningClass"/> the participant belongs to.</param>
        /// <returns>A New LearningParticipant record for the specified <paramref name="classId"/>, person and role (from <paramref name="participantBag"/>.</returns>
        private LearningParticipant GetNewLearningParticipantFromBag( LearningParticipantBag participantBag, LearningClassService classService, int classId )
        {
            int? personId = null;
            if ( Guid.TryParse( participantBag.PersonAlias.Value, out var primaryAliasGuid ) )
            {
                personId = new PersonAliasService( RockContext ).GetSelect( primaryAliasGuid, pa => pa.PersonId );
            }

            var classRoles = classService.GetClassRoles( classId ).Where( r => r.IsLeader == participantBag.IsFacilitator );

            int groupRoleId = classRoles.Select( r => r.Id ).FirstOrDefault();

            return new LearningParticipant
            {
                PersonId = personId.ToIntSafe(),
                LearningClassId = classId,
                GroupId = classId,
                Note = participantBag.Note,
                GroupRoleId = groupRoleId.ToIntSafe(),
                LearningCompletionStatus = LearningCompletionStatus.Incomplete,
                LearningGradePercent = 0
            };
        }

        /// <summary>
        /// Gets the ordered learning plan for the class specified by the current PageParameter.
        /// </summary>
        /// <param name="rockContext">The RockContex tto use for getting the ordered results</param>
        /// <returns>An IQueryable of ordered <see cref="LearningActivity"/> records.</returns>
        private IQueryable<LearningActivity> GetOrderedLearningPlan()
        {
            var classId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassId );

            if ( classId > 0 )
            {
                return new LearningActivityService( RockContext )
                    .GetClassLearningPlan( classId )
                    .AsNoTracking();
            }

            // Get the page parameter value (either IdKey or Id).
            var courseId = RequestContext.PageParameterAsId( PageParameterKey.LearningCourseId );

            if ( courseId > 0 )
            {
                // Get the default class (prevents duplicates from showing in the activity list).
                var defaultClassId = new LearningClassService( RockContext ).GetCourseDefaultClass( courseId, c => c.Id );

                return new LearningActivityService( RockContext )
                    .GetClassLearningPlan( defaultClassId )
                    .AsNoTracking();
            }

            return new List<LearningActivity>().AsQueryable();
        }

        /// <summary>
        /// Updates the required courses for the current course/class.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="contentChannel">The content channel.</param>
        /// <param name="rockContext">The rock context.</param>
        private void UpdateRequiredCourses( LearningCourseBag bag, LearningCourse entity, RockContext rockContext )
        {
            var currentRequirements = bag.CourseRequirements.Select( cr => new LearningCourseRequirement
            {
                Id = IdHasher.Instance.GetId( cr.IdKey ) ?? 0,
                RequiredLearningCourseId = IdHasher.Instance.GetId( cr.RequiredLearningCourseIdKey ) ?? 0,
                LearningCourseId = IdHasher.Instance.GetId( cr.LearningCourseIdKey ) ?? 0,
                RequirementType = cr.RequirementType
            } )
            .Where( cr => cr.RequiredLearningCourseId > 0 && cr.LearningCourseId == entity.Id )
            .ToList();

            var requirementsRemoved = entity.LearningCourseRequirements.Where( prev => !currentRequirements.Any( cur => prev.Id == cur.Id ) );
            var newRequirements = currentRequirements.Where( cur => !entity.LearningCourseRequirements.Any( prev => prev.Id == cur.Id ) );

            var entityService = new LearningCourseRequirementService( rockContext );

            entityService.DeleteRange( requirementsRemoved );
            entityService.AddRange( newRequirements );
        }

        #endregion
    }
}
