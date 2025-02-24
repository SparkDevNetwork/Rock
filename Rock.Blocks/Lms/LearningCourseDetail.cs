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
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningCourseDetail;
using Rock.ViewModels.Blocks.Lms.LearningCourseRequirement;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning requiredCourse.
    /// </summary>

    [DisplayName( "Learning Course Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning requiredCourse." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

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
            // Get the ConfigurationMode for the parent Program.
            var programConfigurationMode = new LearningProgramService( RockContext )
                .GetSelect( PageParameter( PageParameterKey.LearningProgramId ),
                c => c.ConfigurationMode,
                !this.PageCache.Layout.Site.DisablePredictableIds );

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

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

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

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

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

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
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
                /*
                    12/12/2024 - JC

                    We must load the parent LearningProgram for new records.
                    When the authorization is checked the LearningProgram (the ParentAuthority)
                    might be checked for approving/denying access (see LearningCourse.IsAuthorized).

                    Reason: ParentAuthority (LearningProgram) might be checked for authorization.
                */
                var program = new LearningProgramService( RockContext ).Get(
                    PageParameter( PageParameterKey.LearningProgramId ),
                    !this.PageCache.Layout.Site.DisablePredictableIds );

                return new LearningCourse
                {
                    LearningProgram = program,
                    LearningProgramId = program?.Id ?? 0,
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
                error = ActionBadRequest( $"Not authorized to edit {LearningCourse.FriendlyTypeName}." );
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
        /// <param name="key">The identifier of the requiredCourse the list will be shown for.</param>
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
                    .ToList():
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
                // Make sure the current user is authorized to view the requiredCourse.
                .Where( c => c.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( c => c.Name )
                .ToListItemBagList();

            return ActionOk( allCourses );
        }

        /// <summary>
        /// Get the detail for a requiredCourse requirement.
        /// </summary>
        /// <param name="key">The identifier of the requiredCourse the requirement will be added to (for including the relationship).</param>
        /// <param name="guid">The Guid identifier of the required requiredCourse.</param>
        /// <param name="requirementType">The type of requirement for the required requiredCourse.</param>
        /// <returns>The <see cref="LearningCourseRequirementBag"/> containing the requiredCourse requirement details.</returns>
        [BlockAction]
        public BlockActionResult GetCourseRequirementDetail( string key, string guid, RequirementType requirementType )
        {
            var courseService = new LearningCourseService( RockContext );
            var courseId = IdHasher.Instance.GetId( key ).ToIntSafe();

            // Make sure the Guid of the required requiredCourse is valid.
            if ( !Guid.TryParse( guid, out var requiredCourseGuid ) )
            {
                return ActionBadRequest( $"Required {nameof( guid )} was invalid." );
            }

            // Make sure the required requiredCourse exists.
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
        /// Deletes the specified requiredCourse requirement.
        /// </summary>
        /// <param name="key">The identifier of the requiredCourse requirement to be deleted.</param>
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

        #region Private methods

        /// <summary>
        /// Updates the required courses for the current requiredCourse/class.
        /// </summary>
        /// <param name="bag">The bag from the block.</param>
        /// <param name="entity">The learning course entity.</param>
        /// <param name="rockContext">The rock context.</param>
        private void UpdateRequiredCourses( LearningCourseBag bag, LearningCourse entity, RockContext rockContext )
        {
            var selectedRequirements = bag.CourseRequirements.Select( cr => new LearningCourseRequirement
            {
                Id = IdHasher.Instance.GetId( cr.IdKey ) ?? 0,
                RequiredLearningCourseId = IdHasher.Instance.GetId( cr.RequiredLearningCourseIdKey ) ?? 0,
                LearningCourseId = entity.Id,
                RequirementType = cr.RequirementType
            } )
            .Where( cr => cr.RequiredLearningCourseId > 0 && cr.LearningCourseId == entity.Id )
            .ToList();

            var requirementsRemoved = entity.LearningCourseRequirements.Where( prev => !selectedRequirements.Any( cur => prev.Id == cur.Id ) );
            var newRequirements = selectedRequirements.Where( cur => !entity.LearningCourseRequirements.Any( ex => ex.Id == cur.Id ) ).ToList();

            var entityService = new LearningCourseRequirementService( rockContext );

            entityService.AddRange( newRequirements );
            entityService.DeleteRange( requirementsRemoved );
        }

        #endregion
    }
}
