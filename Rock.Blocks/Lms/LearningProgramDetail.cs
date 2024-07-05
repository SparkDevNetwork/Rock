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
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningProgramDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning program.
    /// </summary>
    [DisplayName( "Learning Program Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning program." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CategoryField(
        "Category",
        Description = "Optional category for the Program.",
        Key = AttributeKey.Category,
        AllowMultiple = false,
        EntityType = typeof( Rock.Model.LearningProgram ),
        IsRequired = false,
        Order = 1 )]

    [CustomDropdownListField(
        "Display Mode",
        Key = AttributeKey.DisplayMode,
        Description = "Select 'Summary' to show only attributes that are 'Show on Grid'. Select 'Full' to show all attributes.",
        ListSource = "Full,Summary",
        IsRequired = true,
        DefaultValue = "Summary",
        Order = 2 )]

    [BooleanField( "Show KPIs",
        Description = "Determines if the KPIs are visible.",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowKPIs )]

    [LinkedPage( "Courses Page",
        Description = "The page that will show the courses for the learning program.",
        Key = AttributeKey.CoursesPage, IsRequired = false, Order = 4 )]

    [LinkedPage( "Completion Detail Page",
        Description = "The page that will show the program completion detail.",
        Key = AttributeKey.CompletionDetailPage, IsRequired = false, Order = 5 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "41351a30-3b4f-44da-b413-49d7c997fbb5" )]
    [Rock.SystemGuid.BlockTypeGuid( "796c87e7-678f-4a38-8c04-a401a4f7ac21" )]
    public class LearningProgramDetail : RockDetailBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Category = "Category";
            public const string CompletionDetailPage = "CompletionDetailPage";
            public const string CoursesPage = "CoursesPage";
            public const string DisplayMode = "DisplayMode";
            public const string ShowKPIs = "ShowKPIs";
        }

        private static class DisplayMode
        {
            public const string Summary = "Summary";
            public const string Full = "Full";
        }

        private static class PageParameterKey
        {
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningProgramCompletionId = "LearningProgramCompletionId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string CoursesPage = "CoursesPage";
            public const string CompletionDetailPage = "CompletionDetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<LearningProgramBag, LearningProgramDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls( box.Entity.IdKey );
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<LearningProgram>();

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
        private LearningProgramDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new LearningProgramDetailOptionsBag();

            options.SystemCommunications = isEditable ? GetCommunicationTemplates( rockContext ) : new List<ListItemBag>();

            return options;
        }

        /// <summary>
        /// Gets the communication templates.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<ListItemBag> GetCommunicationTemplates( RockContext rockContext )
        {
            var communicationTemplates = new List<ListItemBag>();
            foreach ( var systemEmail in new SystemCommunicationService( rockContext )
                .Queryable().AsNoTracking()
                .OrderBy( e => e.Title )
                .Select( e => new
                {
                    e.Guid,
                    e.Title
                } ) )
            {
                communicationTemplates.Add( new ListItemBag() { Text = systemEmail.Title, Value = systemEmail.Guid.ToString() } );
            }

            return communicationTemplates;
        }

        /// <summary>
        /// Validates the LearningProgram for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningProgram">The LearningProgram to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningProgram is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningProgram( LearningProgram learningProgram, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( learningProgram.Name.IsNullOrWhiteSpace() )
            {
                errorMessage = "Name is required";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LearningProgramBag, LearningProgramDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LearningProgram.FriendlyTypeName} was not found.";
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
                    var onlyShowIsGridColumn = GetAttributeValue( AttributeKey.DisplayMode ) == DisplayMode.Summary;
                    box.Entity = GetEntityBagForView( entity, onlyShowIsGridColumn );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningProgram.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningProgram.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LearningProgramBag"/> that represents the entity.</returns>
        private LearningProgramBag GetCommonEntityBag( LearningProgram entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var showKpis = GetAttributeValue( AttributeKey.ShowKPIs ).AsBoolean();
            var kpis = showKpis ? new LearningProgramService( RockContext ).GetProgramKpis( entity.Id ) : new LearningProgramKpis();

            return new LearningProgramBag
            {
                IdKey = entity.IdKey,
                AbsencesCriticalCount = entity.AbsencesCriticalCount,
                AbsencesWarningCount = entity.AbsencesWarningCount,
                ActiveClasses = kpis.ActiveClasses,
                ActiveStudents = kpis.ActiveStudents,
                AdditionalSettingsJson = entity.AdditionalSettingsJson,
                Category = entity.Category.ToListItemBag(),
                CategoryId = entity.CategoryId,
                CompletionWorkflowType = entity.CompletionWorkflowType.ToListItemBag(),
                CompletionWorkflowTypeId = entity.CompletionWorkflowTypeId,
                Completions = kpis.Completions,
                ConfigurationMode = entity.ConfigurationMode,
                Description = entity.Description,
                HighlightColor = entity.HighlightColor,
                IconCssClass = entity.IconCssClass,
                ImageBinaryFile = entity.ImageBinaryFile.ToListItemBag(),
                ImageBinaryFileId = entity.ImageBinaryFileId,
                IsActive = entity.IsActive,
                IsCompletionStatusTracked = entity.IsCompletionStatusTracked,
                IsPublic = entity.IsPublic,
                Name = entity.Name,
                PublicName = entity.PublicName,
                ShowKpis = showKpis,
                Summary = entity.Summary,
                SystemCommunication = entity.SystemCommunication.ToListItemBag()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="LearningProgramBag"/> that represents the entity.</returns>
        private LearningProgramBag GetEntityBagForView( LearningProgram entity, bool onlyIsGridColumn = false )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            // For the summary view only include Attribute.IsGridColumn.
            if ( onlyIsGridColumn )
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson,
                    attributeFilter: a => a.IsGridColumn
                    );
            }
            else
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="LearningProgramBag"/> that represents the entity.</returns>
        private LearningProgramBag GetEntityBagForEdit( LearningProgram entity )
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
        private bool UpdateEntityFromBox( LearningProgram entity, DetailBlockBox<LearningProgramBag, LearningProgramDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.AbsencesCriticalCount ),
                () => entity.AbsencesCriticalCount = box.Entity.AbsencesCriticalCount );

            box.IfValidProperty( nameof( box.Entity.AbsencesWarningCount ),
                () => entity.AbsencesWarningCount = box.Entity.AbsencesWarningCount );

            box.IfValidProperty( nameof( box.Entity.AdditionalSettingsJson ),
                () => entity.AdditionalSettingsJson = box.Entity.AdditionalSettingsJson );

            box.IfValidProperty( nameof( box.Entity.Category ),
                () => entity.CategoryId = box.Entity.Category.GetEntityId<Category>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.CompletionWorkflowType ),
                () => entity.CompletionWorkflowTypeId = box.Entity.CompletionWorkflowType.GetEntityId<WorkflowType>( rockContext ) );

            var isMovingToOnDemandMode =
                box.Entity.ConfigurationMode != entity.ConfigurationMode &&
                box.Entity.ConfigurationMode == Enums.Lms.ConfigurationMode.OnDemandLearning;

            // We're unable to move to academic calendar mode from On-Demand due to the fact that none of the current participants will have records.
            if ( isMovingToOnDemandMode )
            {
                throw new ApplicationException( "Unable to move from Academic Calendar mode to On-Demand mode." );
            }

            box.IfValidProperty( nameof( box.Entity.ConfigurationMode ),
                () => entity.ConfigurationMode = box.Entity.ConfigurationMode );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.HighlightColor ),
                () => entity.HighlightColor = box.Entity.HighlightColor );

            box.IfValidProperty( nameof( box.Entity.IconCssClass ),
                () => entity.IconCssClass = box.Entity.IconCssClass );

            box.IfValidProperty( nameof( box.Entity.ImageBinaryFile ),
                () => entity.ImageBinaryFileId = box.Entity.ImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.IsCompletionStatusTracked ),
                () => entity.IsCompletionStatusTracked = box.Entity.IsCompletionStatusTracked );

            box.IfValidProperty( nameof( box.Entity.IsPublic ),
                () => entity.IsPublic = box.Entity.IsPublic );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.PublicName ),
                () => entity.PublicName = box.Entity.PublicName );

            box.IfValidProperty( nameof( box.Entity.Summary ),
                () => entity.Summary = box.Entity.Summary );

            box.IfValidProperty( nameof( box.Entity.SystemCommunication ),
                () => entity.SystemCommunicationId = box.Entity.SystemCommunication.GetEntityId<SystemCommunication>( rockContext ).Value );

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
        /// <returns>The <see cref="LearningProgram"/> to be viewed or edited on the page.</returns>
        private LearningProgram GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<LearningProgram, LearningProgramService>( rockContext, PageParameterKey.LearningProgramId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( string idKey )
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = idKey
            };

            var completionDetailPageParams = new Dictionary<string, string>()
            {
                [PageParameterKey.LearningProgramId] = idKey,
                [PageParameterKey.LearningProgramCompletionId] = "((Key))"
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.CoursesPage] = this.GetLinkedPageUrl( AttributeKey.CoursesPage, queryParams ),
                [NavigationUrlKey.CompletionDetailPage] = this.GetLinkedPageUrl( AttributeKey.CompletionDetailPage, completionDetailPageParams ),
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
        private string GetSecurityGrantToken( LearningProgram entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out LearningProgram entity, out BlockActionResult error )
        {
            var entityService = new LearningProgramService( rockContext );
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
                entity = new LearningProgram();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LearningProgram.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${LearningProgram.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningProgramId ) ?? "";

                var entityName = entityKey.Length > 0 ? new LearningProgramService( rockContext ).GetSelect( entityKey, p => p.Name ) : "New Program";
                var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, pageReference.Parameters );
                var breadCrumb = new BreadCrumbLink( entityName ?? "New Program", breadCrumbPageRef );

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
        /// Gets the box that will contain all the information for the entity
        /// and all the attributes.
        /// </summary>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult GetEntityBagWithAllAttributes()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var bagWithAllAttributes = GetEntityBagForView( entity );

                return ActionOk( bagWithAllAttributes );
            }
        }

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

                var box = new DetailBlockBox<LearningProgramBag, LearningProgramDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<LearningProgramBag, LearningProgramDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LearningProgramService( rockContext );

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
                if ( !ValidateLearningProgram( entity, rockContext, out var validationMessage ) )
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
                        [PageParameterKey.LearningProgramId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                var onlyShowIsGridColumn = GetAttributeValue( AttributeKey.DisplayMode ) == DisplayMode.Summary;

                return ActionOk( GetEntityBagForView( entity, onlyShowIsGridColumn ) );
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
                var entityService = new LearningProgramService( rockContext );

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
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult DeleteSemester( string key )
        {
            var entityService = new LearningSemesterService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{LearningSemester.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete ${LearningSemester.FriendlyTypeName}." );
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
        /// Gets a Grid of the <see cref="LearningProgramCompletion"/> records for the LearningProgram specified in the PageParameters.
        /// If no LearningProgramId page parameter exists an empty list is returned.
        /// </summary>
        /// <returns>The GridDataBag of LearningProgramCompletion records.</returns>
        [BlockAction]
        public BlockActionResult GetCompletions()
        {
            var entityKey = PageParameter( PageParameterKey.LearningProgramId );
            var program = new LearningProgramService( RockContext ).GetSelect( entityKey, p => new { p.ConfigurationMode, p.Id } );

            var queryable = program == null ? new List<LearningProgramCompletion>().AsQueryable() : GetCompletionListQueryable( program.Id );

            var grid = new GridBuilder<LearningProgramCompletion>()
                 .WithBlock( this )
                 .AddTextField( "idKey", a => a.IdKey )
                 .AddPersonField( "individual", a => a.PersonAlias?.Person )
                 .AddTextField( "campus", a => a.CampusId.HasValue ? CampusCache.Get( a.CampusId.Value )?.Name : null )
                 .AddDateTimeField( "startDate", a => a.StartDate )
                 .AddDateTimeField( "endDate", a => a.EndDate )
                 .AddField( "status", a => a.CompletionStatus );

            if ( program.ConfigurationMode == ConfigurationMode.AcademicCalendar )
            {
                grid.AddTextField( "semester", a => a.LearningProgram.LearningSemesters?
                    .FirstOrDefault( s =>
                        s.StartDate >= a.StartDate &&
                        s.EndDate <= a.EndDate
                    )?.Name
                );
            }

            return ActionOk( grid.Build( queryable ) );
        }

        /// <summary>
        /// Gets a Grid of the <see cref="LearningSemester"/> records for the LearningProgram specified in the PageParameters.
        /// If no LearningProgramId page parameter exists an empty list is returned.
        /// </summary>
        /// <returns>The GridDataBag of LearningSemester records.</returns>
        [BlockAction]
        public BlockActionResult GetSemesters()
        {
            var gridBuilder = new GridBuilder<LearningSemester>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddDateTimeField( "startDate", a => a.StartDate )
                .AddDateTimeField( "endDate", a => a.EndDate )
                .AddDateTimeField( "closeDate", a => a.EnrollmentCloseDate )
                .AddField( "classCount", a => a.LearningClasses.Count() );

            var semestersQueryable = GetSemesterListQueryable();
            return ActionOk( gridBuilder.Build( semestersQueryable ) );
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<LearningProgramBag, LearningProgramDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<LearningProgramBag, LearningProgramDetailOptionsBag>
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
        /// Gets the Learning Program Completion Queryable for completions grid.
        /// </summary>
        /// <returns>A Queryable of LearningProgramCompletions.</returns>
        private IQueryable<LearningProgramCompletion> GetCompletionListQueryable( int learningProgramId )
        {
            var queryable = new LearningProgramCompletionService( RockContext )
                .Queryable()
                .Include( a => a.PersonAlias )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.LearningProgram )
                .Where( a => a.LearningProgramId == learningProgramId );

            return queryable;
        }

        /// <summary>
        /// Gets the Learning Smester Queryable for semesters grid.
        /// </summary>
        /// <returns>A Queryable of LearningSemester.</returns>
        private IQueryable<LearningSemester> GetSemesterListQueryable()
        {
            var entityId = RequestContext.PageParameterAsId( PageParameterKey.LearningProgramId );

            // If a Learning Program has been specified then get the semesters for that.
            if ( entityId > 0 )
            {
                return new LearningSemesterService( RockContext )
                    .Queryable()
                    .Include( a => a.LearningClasses )
                    .Where( a => a.LearningProgramId == entityId );
            }

            return new List<LearningSemester>().AsQueryable();
        }

        #endregion
    }
}
