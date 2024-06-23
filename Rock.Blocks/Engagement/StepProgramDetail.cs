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
using Rock.Chart;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StepProgramDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular step program.
    /// </summary>

    [DisplayName( "Step Program Detail" )]
    [Category( "Steps" )]
    [Description( "Displays the details of the given Step Program for editing." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [BooleanField
        ( "Show Chart",
          Key = AttributeKey.ShowChart,
          DefaultValue = "true",
          Order = 0 )]

    [SlidingDateRangeField
        ( "Default Chart Date Range",
          Key = AttributeKey.SlidingDateRange,
          DefaultValue = "Current||Year||",
          EnabledSlidingDateRangeTypes = "Last,Previous,Current,DateRange",
          Order = 1 )]

    [CodeEditorField(
        "Key Performance Indicator Lava",
        IsRequired = false,
        DefaultValue = DefaultValue.KpiLava,
        Key = AttributeKey.KpiLava,
        EditorMode = CodeEditorMode.Lava,
        Description = "The Lava used to render the Key Performance Indicators bar. <span class='tip tip-lava'></span>",
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "7260278e-efb7-4b98-a862-15bf0a40ba2e" )]
    [Rock.SystemGuid.BlockTypeGuid( "e2f965d1-7419-4062-9568-08613bb696e3" )]
    public class StepProgramDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string StepProgramId = "ProgramId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string ShowChart = "Show Chart";
            public const string SlidingDateRange = "SlidingDateRange";
            public const string KpiLava = "KpiLava";
        }

        private static class DefaultValue
        {
            /// <summary>
            /// The kpi lava
            /// </summary>
            public const string KpiLava =
@"{[kpis style:'card' iconbackground:'true']}
  [[ kpi icon:'fa-user' value:'{{IndividualsCompleting | Format:'N0'}}' label:'Individuals Completing Program' color:'blue-700']][[ endkpi ]]
  [[ kpi icon:'fa-calendar' value:'{{AvgDaysToComplete | Format:'N0'}}' label:'Average Days to Complete Program' color:'green-600']][[ endkpi ]]
  [[ kpi icon:'fa-map-marker' value:'{{StepsStarted | Format:'N0'}}' label:'Steps Started' color:'#FF385C']][[ endkpi ]]
  [[ kpi icon:'fa-check-square' value:'{{StepsCompleted | Format:'N0'}}' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}";
        }

        #endregion Keys

        /// <summary>
        /// The step statuses, should be accessed using the <see cref="GetStepStatuses"/> since performs a null check on <see cref="_stepStatuses"/>
        /// before assigning a value when possible.
        /// </summary>
        private List<StepStatus> _stepStatuses;

        /// <summary>
        /// The rock context, should be accessed using the <see cref="GetRockContext"/> method since it performs a null check before
        /// creating a new instance.
        /// </summary>
        private RockContext _rockContext;

        /// <summary>
        /// The step type, should be accessed using the <see cref="GetStepType"/> since performs a null check on <see cref="_stepType"/>
        /// before assigning a value when possible.
        /// </summary>
        private StepProgram _stepProgram;

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = GetRockContext() )
            {
                var box = new DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag>();

                var entity = GetInitialEntity( rockContext );

                SetBoxInitialEntityState( box, rockContext, entity );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions();
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<StepProgram>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private StepProgramDetailOptionsBag GetBoxOptions()
        {
            var options = new StepProgramDetailOptionsBag
            {
                ViewModes = typeof( StepProgram.ViewMode ).ToEnumListItemBag(),

                TriggerTypes = new List<ListItemBag>
                {
                    new ListItemBag() { Text = "Step Completed", Value = StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete.ToString() },
                    new ListItemBag() { Text = "Status Changed", Value = StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged.ToString() },
                    new ListItemBag() { Text = "Manual", Value = StepWorkflowTrigger.WorkflowTriggerCondition.Manual.ToString() }
                }
            };

            return options;
        }

        /// <summary>
        /// Validates the StepProgram for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="stepProgram">The StepProgram to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the StepProgram is valid, <c>false</c> otherwise.</returns>
        private bool ValidateStepProgram( StepProgram stepProgram, RockContext rockContext, out string errorMessage )
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
        /// <param name="entity">The rock entity.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag> box, RockContext rockContext, StepProgram entity )
        {
            if ( entity == null )
            {
                box.ErrorMessage = $"The {StepProgram.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( StepProgram.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( StepProgram.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="StepProgramBag"/> that represents the entity.</returns>
        private StepProgramBag GetCommonEntityBag( StepProgram entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new StepProgramBag
            {
                IdKey = entity.IdKey,
                Category = entity.Category.ToListItemBag(),
                Description = entity.Description,
                IconCssClass = entity.IconCssClass,
                IsActive = entity.IsActive,
                Name = entity.Name,
                DefaultListView = entity.DefaultListView.ConvertToInt(),
                CanAdministrate = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="StepProgramBag"/> that represents the entity.</returns>
        private StepProgramBag GetEntityBagForView( StepProgram entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            var defaultDateRange = GetAttributeValue( AttributeKey.SlidingDateRange );

            bag.ShowChart = ShowActivitySummary();
            bag.Kpi = GetKpi( defaultDateRange );
            bag.DefaultDateRange = GetSlidingDateRangeBag( defaultDateRange );
            bag.StepFlowPageUrl = RequestContext.ResolveRockUrl( $"~/steps/program/{GetStepProgramId()}/flow" );

            var showActivitySummary = ShowActivitySummary();

            if ( showActivitySummary )
            {
                // Get chart data and set visibility of related elements.
                var chartFactory = GetChartJsFactory( defaultDateRange );

                if ( chartFactory.HasData )
                {
                    var args = GetChartArgs();
                    // Add client script to construct the chart.
                    bag.ChartData = chartFactory.GetChartDataJson( args );
                }
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="StepProgramBag"/> that represents the entity.</returns>
        private StepProgramBag GetEntityBagForEdit( StepProgram entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            var rockContext = GetRockContext();

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            bag.StepProgramAttributes = GetStepTypeAttributes( rockContext, entity.Id.ToString() ).ConvertAll( e => PublicAttributeHelper.GetPublicEditableAttributeViewModel( e ) );

            bag.Statuses = entity.StepStatuses.Select( s => new StepStatusBag()
            {
                Guid = s.Guid,
                Id = s.Id,
                IsActive = s.IsActive,
                IsCompleteStatus = s.IsCompleteStatus,
                Name = s.Name,
                StatusColor = s.StatusColor
            } ).ToList();

            bag.WorkflowTriggers = entity.StepWorkflowTriggers
                .OrderBy( c => c.TypeName ).ThenBy( c => c.TriggerType.ConvertToString() )
                .Select( wt => new StepProgramWorkflowTriggerBag()
                {
                    IdKey = wt.IdKey,
                    Guid = wt.Guid,
                    WorkflowTrigger = GetTriggerType( wt.TriggerType, wt.TypeQualifier ),
                    WorkflowType = wt.WorkflowType.ToListItemBag(),
                    PrimaryQualifier = GetStepStatuses( entity.Id ).Find( ss => ss.Id == new StepWorkflowTrigger.StatusChangeTriggerSettings( wt.TypeQualifier ).FromStatusId )?.Guid.ToString(),
                    SecondaryQualifier = GetStepStatuses( entity.Id ).Find( ss => ss.Id == new StepWorkflowTrigger.StatusChangeTriggerSettings( wt.TypeQualifier ).ToStatusId )?.Guid.ToString(),
                } ).ToList();

            bag.StatusOptions = new StepStatusService( rockContext ).Queryable().Where( s => s.StepProgramId == entity.Id ).AsEnumerable().ToListItemBagList();

            return bag;
        }

        /// <summary>
        /// Gets the StepType's attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="stepProgramId">The Step Program identifier qualifier value.</param>
        /// <returns></returns>
        private static List<Model.Attribute> GetStepTypeAttributes( RockContext rockContext, string stepProgramId )
        {
            return new AttributeService( rockContext ).GetByEntityTypeId( new StepType().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "StepProgramId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( stepProgramId ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the type of the trigger as a <see cref="ListItemBag"/>.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="typeQualifier">The type qualifier.</param>
        /// <returns></returns>
        private ListItemBag GetTriggerType( StepWorkflowTrigger.WorkflowTriggerCondition condition, string typeQualifier )
        {
            var qualifierSettings = new StepWorkflowTrigger.StatusChangeTriggerSettings( typeQualifier );
            var text = new StepWorkflowTriggerService( GetRockContext() ).GetTriggerSettingsDescription( condition, qualifierSettings );
            var value = condition.ToStringSafe();

            return new ListItemBag() { Text = text, Value = value };
        }

        /// <summary>
        /// Gets all the available step statuses.
        /// </summary>
        /// <returns></returns>
        private List<StepStatus> GetStepStatuses( int stepProgramId )
        {
            return _stepStatuses ?? ( _stepStatuses = new StepStatusService( GetRockContext() ).Queryable().Where( s => s.StepProgramId == stepProgramId ).ToList() );
        }

        /// <summary>
        /// Gets the rock context.
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            return _rockContext ?? ( _rockContext = new RockContext() );
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( StepProgram entity, DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Category ),
                () => entity.CategoryId = box.Entity.Category.GetEntityId<Category>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IconCssClass ),
                () => entity.IconCssClass = box.Entity.IconCssClass );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.DefaultListView ),
                () => entity.DefaultListView = ( StepProgram.ViewMode ) box.Entity.DefaultListView );

            box.IfValidProperty( nameof( box.Entity.Statuses ),
                () => SaveStatuses( box.Entity, entity, rockContext ) );

            box.IfValidProperty( nameof( box.Entity.WorkflowTriggers ),
                () => SaveWorkflowTriggers( box.Entity, entity, rockContext ) );

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
        /// <returns>The <see cref="StepProgram"/> to be viewed or edited on the page.</returns>
        private StepProgram GetInitialEntity( RockContext rockContext )
        {
            return _stepProgram = GetInitialEntity<StepProgram, StepProgramService>( rockContext, PageParameterKey.StepProgramId );
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
        private string GetSecurityGrantToken( StepProgram entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out StepProgram entity, out BlockActionResult error )
        {
            var entityService = new StepProgramService( rockContext );
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
                entity = new StepProgram();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{StepProgram.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${StepProgram.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save attributes associated with this step.
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="qualifierColumn"></param>
        /// <param name="qualifierValue"></param>
        /// <param name="viewStateAttributes"></param>
        /// <param name="rockContext"></param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> viewStateAttributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        /// <summary>
        /// Gets the kpi HTML.
        /// </summary>
        private string GetKpi( string delimitedDateRange )
        {
            var stepProgram = GetStepProgram();
            var template = GetAttributeValue( AttributeKey.KpiLava );

            if ( template.IsNullOrWhiteSpace() || stepProgram == null )
            {
                return string.Empty;
            }

            var startedQuery = GetStartedStepQuery( delimitedDateRange );
            var completedStepQuery = GetCompletedStepQuery( delimitedDateRange );
            var completedPrograms = GetCompletedProgramQuery( delimitedDateRange ).ToList();

            var individualsCompleting = completedPrograms.Count;
            var stepsStarted = startedQuery.Count();
            var stepsCompleted = completedStepQuery.Count();

            var daysToCompleteList = completedPrograms
                .Where( sps => sps.CompletedDateTime.HasValue && sps.StartedDateTime.HasValue )
                .Select( sps => ( sps.CompletedDateTime.Value - sps.StartedDateTime.Value ).Days );

            var avgDaysToComplete = daysToCompleteList.Any() ? ( int ) daysToCompleteList.Average() : 0;

            return template.ResolveMergeFields( new Dictionary<string, object>
            {
                { "IndividualsCompleting", individualsCompleting },
                { "AvgDaysToComplete", avgDaysToComplete },
                { "StepsStarted", stepsStarted },
                { "StepsCompleted", stepsCompleted },
                { "StepType", stepProgram }
            } );
        }

        /// <summary>
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetStepsCompletedQuery( string delimitedDateRange )
        {
            var dataContext = GetRockContext();
            var stepService = new StepService( dataContext );
            var stepProgramId = GetStepProgramId();

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    x.StepType.StepProgramId == stepProgramId &&
                    x.StepType.IsActive &&
                    x.CompletedDateKey != null );

            var campusContext = this.RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                query = query.Where( s => s.CampusId == campusContext.Id );
            }

            // Apply date range
            var reportPeriod = new TimePeriod( delimitedDateRange );
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate != null )
            {
                var startDateKey = startDate.Value.ToDateKey();
                query = query.Where( x => x.CompletedDateKey >= startDateKey );
            }

            if ( endDate != null )
            {
                var compareDateKey = endDate.Value.ToDateKey();
                query = query.Where( x => x.CompletedDateKey <= compareDateKey );
            }

            return query;
        }

        /// <summary>
        /// Gets the active step type ids.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> GetActiveStepTypeIds()
        {
            var stepProgramId = GetStepProgramId();
            var stepProgram = StepProgramCache.Get( stepProgramId );

            if ( stepProgram == null )
            {
                return new List<int>();
            }

            var stepTypeIds = stepProgram.StepTypes.Where( st => st.IsActive ).Select( st => st.Id );
            return stepTypeIds;
        }

        /// <summary>
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetCompletedStepQuery( string delimitedDateRange )
        {
            var stepTypeIds = GetActiveStepTypeIds();

            if ( stepTypeIds == null )
            {
                return null;
            }

            var dataContext = GetRockContext();
            var stepService = new StepService( dataContext );

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    stepTypeIds.Contains( x.StepTypeId ) &&
                    x.CompletedDateKey != null );

            var campusContext =  RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                query = query.Where( s => s.CampusId == campusContext.Id );
            }

            // Apply date range
            var reportPeriod = new TimePeriod( delimitedDateRange );
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate != null )
            {
                var startDateKey = startDate.Value.ToDateKey();
                query = query.Where( x => x.CompletedDateKey >= startDateKey );
            }

            if ( endDate != null )
            {
                var compareDateKey = endDate.Value.ToDateKey();
                query = query.Where( x => x.CompletedDateKey <= compareDateKey );
            }

            return query;
        }

        /// <summary>
        /// Gets the completed step program query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<StepProgramService.PersonStepProgramViewModel> GetCompletedProgramQuery( string delimitedDateRange )
        {
            var stepProgramId = GetStepProgramId();
            var rockContext = GetRockContext();
            var service = new StepProgramService( rockContext );
            var query = service.GetPersonCompletingProgramQuery( stepProgramId );

            if ( query == null )
            {
                return null;
            }

            // Apply date range
            var reportPeriod = new TimePeriod( delimitedDateRange );
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate != null )
            {
                query = query.Where( x => x.CompletedDateTime >= startDate.Value );
            }

            if ( endDate != null )
            {
                var compareDate = endDate.Value.AddDays( 1 );
                query = query.Where( x => x.CompletedDateTime < compareDate );
            }

            var campusContext = RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                query = query.Where( s => s.CampusId == campusContext.Id );
            }

            return query;
        }

        /// <summary>
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetStartedStepQuery( string delimitedDateRange )
        {
            var stepTypeIds = GetActiveStepTypeIds();

            if ( stepTypeIds == null )
            {
                return null;
            }

            var dataContext = GetRockContext();
            var stepService = new StepService( dataContext );

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    stepTypeIds.Contains( x.StepTypeId ) &&
                    x.StartDateKey != null );

            var campusContext = RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                query = query.Where( s => s.CampusId == campusContext.Id );
            }

            // Apply date range
            var reportPeriod = new TimePeriod( delimitedDateRange );
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate != null )
            {
                var startDateKey = startDate.Value.ToDateKey();
                query = query.Where( x => x.StartDateKey >= startDateKey );
            }

            if ( endDate != null )
            {
                var compareDateKey = endDate.Value.ToDateKey();
                query = query.Where( x => x.StartDateKey <= compareDateKey );
            }

            return query;
        }

        /// <summary>
        /// Returns true if the block should display the Activity Summary chart.
        /// </summary>
        /// <returns></returns>
        private bool ShowActivitySummary()
        {
            // Set the visibility of the Activity Summary chart.
            var showActivitySummary = GetAttributeValue( AttributeKey.ShowChart ).AsBoolean( true );
            var stepTypeId = GetStepProgramId();

            if ( showActivitySummary )
            {
                // If the Step Type does not have any activity, hide the Activity Summary.
                var dataContext = GetRockContext();
                var stepService = new StepService( dataContext );
                var stepsQuery = stepService.Queryable().AsNoTracking()
                                    .Where( x => x.StepTypeId == stepTypeId );
                showActivitySummary = stepsQuery.Any();
            }

            return showActivitySummary;
        }

        /// <summary>
        /// Gets a configured factory that creates the data required for the chart.
        /// </summary>
        /// <returns></returns>
        public ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint> GetChartJsFactory( string delimitedDateRange )
        {
            var reportPeriod = new TimePeriod( delimitedDateRange );
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;
            StepProgram program = GetStepProgram();

            // Get all of the completed Steps associated with the current program, grouped by Step Type.
            var stepsCompletedQuery = GetStepsCompletedQuery( delimitedDateRange );

            if ( startDate.HasValue )
            {
                startDate = startDate.Value.Date;
                var startDateKey = startDate.ToDateKey();
                stepsCompletedQuery = stepsCompletedQuery.Where( x => x.CompletedDateKey.Value >= startDateKey );
            }

            if ( endDate != null )
            {
                var compareDateKey = endDate.Value.ToDateKey();
                stepsCompletedQuery = stepsCompletedQuery.Where( x => x.CompletedDateKey.Value <= compareDateKey );
            }

            List<StepTypeActivityDataPoint> stepTypeDataPoints;

            // Get the Data Points, scaled according to the currently selected range.
            ChartJsTimeSeriesTimeScaleSpecifier chartTimeScale;

            if ( reportPeriod.TimeUnit == TimePeriodUnitSpecifier.Year )
            {
                // Group by Month
                chartTimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Month;

                stepTypeDataPoints = stepsCompletedQuery
                    .GroupBy( x => new
                    {
                        MonthKey = x.CompletedDateKey.Value / 100,
                        DatasetName = x.StepType.Name,
                        SortKey1 = x.StepType.Order,
                        SortKey2 = x.StepTypeId
                    } )
                    .Select( x => new
                    {
                        x.Key,
                        Count = x.Count()
                    } )
                    .ToList()
                    .Select( x =>
                    {
                        // Add 1 for the first day of the month
                        var dateKey = ( x.Key.MonthKey * 100 ) + 1;

                        return new StepTypeActivityDataPoint
                        {
                            StepTypeName = x.Key.DatasetName,
                            DateTime = dateKey.GetDateKeyDate(),
                            SortKey1 = x.Key.SortKey1,
                            SortKey2 = x.Key.SortKey2,
                            CompletedCount = x.Count
                        };
                    } )
                    .OrderBy( x => x.SortKey1 )
                    .ThenBy( x => x.SortKey2 )
                    .ToList();
            }
            else
            {
                // Group by Day
                chartTimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Day;

                stepTypeDataPoints = stepsCompletedQuery
                    .GroupBy( x => new
                    {
                        DateKey = x.CompletedDateKey.Value,
                        DatasetName = x.StepType.Name,
                        SortKey1 = x.StepType.Order,
                        SortKey2 = x.StepTypeId
                    } )
                    .Select( x => new
                    {
                        x.Key,
                        Count = x.Count()
                    } )
                    .ToList()
                    .Select( x => new StepTypeActivityDataPoint
                    {
                        StepTypeName = x.Key.DatasetName,
                        DateTime = x.Key.DateKey.GetDateKeyDate(),
                        SortKey1 = x.Key.SortKey1,
                        SortKey2 = x.Key.SortKey2,
                        CompletedCount = x.Count
                    } )
                    .OrderBy( x => x.SortKey1 )
                    .ThenBy( x => x.SortKey2 )
                    .ToList();
            }

            var stepTypeDatasets = stepTypeDataPoints
                .OrderBy( x => x.SortKey1 ).ThenBy( x => x.SortKey2 )
                .Select( x => x.StepTypeName )
                .Distinct()
                .ToList();

            var factory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>();

            factory.TimeScale = chartTimeScale;
            factory.StartDateTime = startDate;
            factory.EndDateTime = endDate;
            factory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.StackedLine;

            foreach ( var stepTypeDataset in stepTypeDatasets )
            {
                var dataset = new ChartJsTimeSeriesDataset();

                // Set Line Color to Step Type Highlight Color.
                var step = program.StepTypes.FirstOrDefault( x => x.Name == stepTypeDataset );

                if ( step != null )
                {
                    dataset.BorderColor = step.HighlightColor;
                }

                dataset.Name = stepTypeDataset;

                dataset.DataPoints = stepTypeDataPoints
                                        .Where( x => x.StepTypeName == stepTypeDataset )
                                        .Select( x => new ChartJsTimeSeriesDataPoint { DateTime = x.DateTime, Value = x.CompletedCount } )
                                        .Cast<IChartJsTimeSeriesDataPoint>()
                                        .ToList();

                factory.Datasets.Add( dataset );
            }

            return factory;
        }

        /// <summary>
        /// Gets the arguments for creating the Chart.
        /// </summary>
        /// <returns></returns>
        private static ChartJsTimeSeriesDataFactory.GetJsonArgs GetChartArgs()
        {
            return new ChartJsTimeSeriesDataFactory.GetJsonArgs
            {
                DisplayLegend = false,
                LineTension = 0.4m,
                MaintainAspectRatio = false,
                SizeToFitContainerWidth = true
            };
        }

        /// <summary>
        /// Gets the step program data model displayed by this page.
        /// </summary>
        /// <returns></returns>
        private StepProgram GetStepProgram()
        {
            if ( _stepProgram == null )
            {
                var stepProgramId = GetStepProgramId();
                _stepProgram = new StepProgramService( GetRockContext() ).Queryable()
                    .Where( c => c.Id == stepProgramId )
                    .FirstOrDefault();
            }

            return _stepProgram ?? RequestContext.GetContextEntity<StepProgram>();
        }

        private int GetStepProgramId()
        {
            var key = RequestContext.GetPageParameter( PageParameterKey.StepProgramId );
            var id = !PageCache.Layout.Site.DisablePredictableIds ? key.AsIntegerOrNull() : null;

            if ( !id.HasValue )
            {
                id = Rock.Utility.IdHasher.Instance.GetId( key );
            }

            return id.GetValueOrDefault();
        }

        /// <summary>
        /// Converts the delimited SlidingDateRange attribute value to a <see cref="SlidingDateRangeBag"/> for the UI.
        /// </summary>
        /// <param name="defaultDateRange"></param>
        /// <returns></returns>
        private SlidingDateRangeBag GetSlidingDateRangeBag( string defaultDateRange )
        {
            var dateRangeBag = new SlidingDateRangeBag() { RangeType = Enums.Controls.SlidingDateRangeType.Current, TimeUnit = Enums.Controls.TimeUnitType.Year };

            if ( defaultDateRange.IsNullOrWhiteSpace() )
            {
                return dateRangeBag;
            }

            string[] splitValues = ( defaultDateRange ?? string.Empty ).Split( '|' );

            if ( splitValues.Length == 5 )
            {
                dateRangeBag.RangeType = splitValues[0].ConvertToEnum<Enums.Controls.SlidingDateRangeType>();
                dateRangeBag.TimeValue = splitValues[1].AsIntegerOrNull() ?? 1;
                dateRangeBag.TimeUnit = splitValues[2].ConvertToEnumOrNull<Enums.Controls.TimeUnitType>() ?? Enums.Controls.TimeUnitType.Year;
                dateRangeBag.LowerDate = splitValues[3].AsDateTime();
                dateRangeBag.UpperDate = splitValues[4].AsDateTime();
            }

            return dateRangeBag;
        }

        /// <summary>
        /// Saves the workflow triggers from the client.
        /// </summary>
        /// <param name="bag">The Step program bag.</param>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SaveWorkflowTriggers( StepProgramBag bag, StepProgram entity, RockContext rockContext )
        {
            var stepWorkflowService = new StepWorkflowService( rockContext );
            var stepWorkflowTriggerService = new StepWorkflowTriggerService( rockContext );
            // Workflow Triggers: Remove deleted triggers.
            // Note that we need to be careful not to remove triggers related to a specific Step Type here, because they are managed separately in the Step Type Detail block.
            var uiWorkflows = bag.WorkflowTriggers.Select( l => l.Guid );

            var deletedTriggers = entity.StepWorkflowTriggers.Where( l => l.StepTypeId == null && !uiWorkflows.Contains( l.Guid ) ).ToList();

            foreach ( var trigger in deletedTriggers )
            {
                // Remove the Step workflows associated with this trigger.
                var stepWorkflows = stepWorkflowService.Queryable().Where( w => w.StepWorkflowTriggerId == trigger.Id );

                foreach ( var requestWorkflow in stepWorkflows )
                {
                    stepWorkflowService.Delete( requestWorkflow );
                }

                // Remove the trigger.
                entity.StepWorkflowTriggers.Remove( trigger );

                stepWorkflowTriggerService.Delete( trigger );
            }

            // Workflow Triggers: Update modified triggers.
            foreach ( var stateTrigger in bag.WorkflowTriggers )
            {
                var workflowTrigger = entity.StepWorkflowTriggers.FirstOrDefault( a => a.Guid == stateTrigger.Guid );

                if ( workflowTrigger == null )
                {
                    workflowTrigger = new StepWorkflowTrigger();
                    workflowTrigger.StepProgramId = entity.Id;
                    entity.StepWorkflowTriggers.Add( workflowTrigger );
                }

                var primaryQualifier = new ListItemBag() { Value = stateTrigger.PrimaryQualifier };
                var secondaryQualifier = new ListItemBag() { Value = stateTrigger.SecondaryQualifier };

                var qualifierSettings = new StepWorkflowTrigger.StatusChangeTriggerSettings
                {
                    FromStatusId = primaryQualifier.GetEntityId<StepStatus>( rockContext ),
                    ToStatusId = secondaryQualifier.GetEntityId<StepStatus>( rockContext ),
                };

                workflowTrigger.WorkflowTypeId = stateTrigger.WorkflowType.GetEntityId<WorkflowType>( rockContext ) ?? 0;
                workflowTrigger.TriggerType = stateTrigger.WorkflowTrigger.Value.ConvertToEnum<StepWorkflowTrigger.WorkflowTriggerCondition>();
                workflowTrigger.TypeQualifier = qualifierSettings.ToSelectionString();
                workflowTrigger.WorkflowName = stateTrigger.WorkflowType.Text;
                workflowTrigger.StepTypeId = null;
            }
        }

        /// <summary>
        /// Saves the statuses from the Client.
        /// </summary>
        /// <param name="bag">The Step program bag.</param>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SaveStatuses( StepProgramBag bag, StepProgram entity, RockContext rockContext )
        {
            var stepStatusService = new StepStatusService( rockContext );
            var uiStatuses = bag.Statuses.Select( r => r.Guid );

            // Step Statuses: Remove deleted Statuses
            var deletedStatuses = entity.StepStatuses.Where( r => !uiStatuses.Contains( r.Guid ) ).ToList();

            foreach ( var stepStatus in deletedStatuses )
            {
                entity.StepStatuses.Remove( stepStatus );
                stepStatusService.Delete( stepStatus );
            }

            // Step Statuses: Update modified Statuses
            foreach ( var stepStatusState in bag.Statuses )
            {
                var stepStatus = entity.StepStatuses.FirstOrDefault( a => a.Guid == stepStatusState.Guid );

                if ( stepStatus == null )
                {
                    stepStatus = new StepStatus();
                    entity.StepStatuses.Add( stepStatus );
                }

                stepStatus.Name = stepStatusState.Name;
                stepStatus.IsActive = stepStatusState.IsActive;
                stepStatus.IsCompleteStatus = stepStatusState.IsCompleteStatus;
                stepStatus.StatusColor = stepStatusState.StatusColor;

                stepStatus.StepProgramId = entity.Id;
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new StepProgramService( rockContext );

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
                if ( !ValidateStepProgram( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                SaveAttributes( new StepType().TypeId, "StepProgramId", entity.Id.ToString(), box.Entity.StepProgramAttributes, rockContext );

                entity = entityService.Get( entity.Id );

                if ( entity == null )
                {
                    return ActionBadRequest( "This record is no longer valid, please reload your data." );
                }

                var currentPerson = GetCurrentPerson();

                if ( !entity.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    entity.AllowPerson( Authorization.VIEW, currentPerson, rockContext );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    entity.AllowPerson( Authorization.EDIT, currentPerson, rockContext );
                }

                if ( !entity.IsAuthorized( Authorization.MANAGE_STEPS, currentPerson ) )
                {
                    entity.AllowPerson( Authorization.MANAGE_STEPS, currentPerson, rockContext );
                }

                if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) )
                {
                    entity.AllowPerson( Authorization.ADMINISTRATE, currentPerson, rockContext );
                }

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.StepProgramId] = entity.IdKey
                    } ) );
                }

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
                var entityService = new StepProgramService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
                {
                    return ActionBadRequest( "You are not authorized to delete this item." );
                }

                string errorMessage = null;
                rockContext.WrapTransaction( () =>
                {
                    var stepTypes = entity.StepTypes.ToList();
                    var stepTypeService = new StepTypeService( rockContext );

                    foreach ( var stepType in stepTypes )
                    {
                        if ( !stepTypeService.CanDelete( stepType, out errorMessage ) )
                        {
                            return;
                        }

                        stepTypeService.Delete( stepType );
                    }

                    rockContext.SaveChanges();

                    if ( !entityService.CanDelete( entity, out errorMessage ) )
                    {
                        return;
                    }

                    entityService.Delete( entity );
                    rockContext.SaveChanges();
                } );

                return string.IsNullOrWhiteSpace( errorMessage ) ? ActionOk( this.GetParentPageUrl() ) : ActionBadRequest( errorMessage );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag>
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
        /// Refresh the chart using the current filter settings.
        /// </summary>
        /// <param name="dateRange"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult RefreshChart( string dateRange )
        {
            var showActivitySummary = ShowActivitySummary();
            var chartDataJson = string.Empty;

            if ( showActivitySummary )
            {
                // Get chart data and set visibility of related elements.
                var chartFactory = GetChartJsFactory( dateRange );

                if ( chartFactory.HasData )
                {
                    var args = GetChartArgs();
                    // Add client script to construct the chart.
                    chartDataJson = chartFactory.GetChartDataJson( args );
                }
            }

            var kpi = GetKpi( dateRange );

            return ActionOk( new StepProgramBag() { ChartData = chartDataJson, Kpi = kpi, ShowChart = showActivitySummary } );
        }

        #endregion

        /// <summary>
        /// A single data point in the result set of a Steps Activity query.
        /// </summary>
        private class StepTypeActivityDataPoint
        {
            /// <summary>
            /// The name of the Step Type to which this Step activity relates.
            /// </summary>
            public string StepTypeName { get; set; }

            /// <summary>
            /// The date and time represented by this data point.
            /// </summary>
            public DateTime DateTime { get; set; }

            /// <summary>
            /// The number of completions represented by this data point.
            /// </summary>
            public int CompletedCount { get; set; }

            /// <summary>
            /// A value used to sort the datapoint within the set of values for this Step Type.
            /// </summary>
            public int SortKey1 { get; set; }

            /// <summary>
            /// A value used to sort the datapoint within the set of values for this Step Type.
            /// </summary>
            public int SortKey2 { get; set; }
        }
    }
}
