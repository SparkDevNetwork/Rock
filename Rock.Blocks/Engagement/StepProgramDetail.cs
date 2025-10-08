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
using System.Data;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Enums.Engagement;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StepProgramDetail;
using Rock.ViewModels.Blocks.Engagement.Steps;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web;
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
    [IconCssClass( "ti ti-question-mark" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

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

    [BooleanField
        ( "Enable List View Display Options",
          Key = AttributeKey.EnableListViewDisplayOptions,
          Description = "Allows selecting a display mode of Grid or Cards.",
          DefaultValue = "false",
          Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "7260278e-efb7-4b98-a862-15bf0a40ba2e" )]
    // Was [Rock.SystemGuid.BlockTypeGuid( "e2f965d1-7419-4062-9568-08613bb696e3" )]
    [Rock.SystemGuid.BlockTypeGuid( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34" )]
    public class StepProgramDetail : RockEntityDetailBlockType<StepProgram, StepProgramBag>, IBreadCrumbBlock
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
            public const string SlidingDateRange = "SlidingDateRange";
            public const string KpiLava = "KpiLava";
            public const string EnableListViewDisplayOptions = "EnableListViewDisplayOptions";
        }

        private static class DefaultValue
        {
            /// <summary>
            /// The kpi lava
            /// </summary>
            public const string KpiLava =
@"{[kpis style:'card' iconbackground:'true']}
  [[ kpi icon:'ti ti-user' value:'{{IndividualsCompleting | Format:'N0'}}' label:'Individuals Completing Program' color:'blue-700']][[ endkpi ]]
  [[ kpi icon:'ti ti-calendar' value:'{{AvgDaysToComplete | Format:'N0'}}' label:'Average Days to Complete Program' color:'gray-700']][[ endkpi ]]
  [[ kpi icon:'ti ti-stairs' value:'{{StepsStarted | Format:'N0'}}' label:'Steps Started' color:'orange-700']][[ endkpi ]]
  [[ kpi icon:'ti ti-circle-check' value:'{{StepsCompleted | Format:'N0'}}' label:'Steps Completed' color:'green-700']][[ endkpi ]]
{[endkpis]}";
        }

        #endregion Keys

        private int currentColorIndex = 0;
        private readonly string[] defaultColors = { "#ea5545", "#f46a9b", "#ef9b20", "#edbf33", "#ede15b", "#bdcf32", "#87bc45", "#27aeef", "#b33dc6" };

        #region Methods

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.StepProgramId );
            var pageParameters = new Dictionary<string, string>();

            var name = new StepProgramService( RockContext )
                .GetSelect( key, mf => mf.Name );

            if ( name != null )
            {
                pageParameters.Add( PageParameterKey.StepProgramId, key );
            }

            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( name ?? "New Step Program", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
            };
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<StepProgram>();

            return box;
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
                },

                AreViewDisplayOptionsVisible = GetAttributeValue( AttributeKey.EnableListViewDisplayOptions ).AsBoolean(),
                IsReOrderColumnVisible = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
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
        private bool ValidateStepProgram( StepProgram stepProgram, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {StepProgram.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( StepProgram.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( StepProgram.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        #region Get Entity Bag Methods

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
                CanAdministrate = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ),
                CompletionFlow = entity.Id > 0 ? ( CompletionFlow? ) entity.CompletionFlow : null,
                IsDeletable = !entity.IsSystem,
                StatusFilterOptions = GetStepStatuses( entity.Id )
                    .Select( s => new ListItemBag
                    {
                        Value = s.Guid.ToString(),
                        Text = s.Name,
                        Category = "Statuses"
                    } )
                    .ToList()
            };
        }

        /// <inheritdoc/>
        protected override StepProgramBag GetEntityBagForView( StepProgram entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            var defaultDateRange = GetAttributeValue( AttributeKey.SlidingDateRange );

            bag.DefaultDateRange = GetSlidingDateRangeBag( defaultDateRange );
            bag.StepFlowPageUrl = RequestContext.ResolveRockUrl( $"~/steps/program/{entity.Id}/flow" );

            var kpi = GetKpi( defaultDateRange, entity.Id, out string errorMessage );
            if ( kpi.IsNullOrWhiteSpace() )
            {
                bag.ErrorMessage = errorMessage;
                return bag;
            }

            bag.Kpi = kpi;

            bag.StepFlowConfigurationBag = GetStepFlowConfigBag( entity );

            bag.StepTypes = entity.StepTypes.ToListItemBagList();

            // Query used to help determine what measure filters should be displayed
            var programStepsQuery = new StepService( RockContext ).Queryable()
                .Where( s => s.StepType.StepProgramId == entity.Id )
                .Select( s => new
                {
                    s.StepType.ImpactWeight,
                    s.StepType.OrganizationalObjectiveValueId,
                    s.StepType.EngagementType
                } );

            bag.HasImpactAdjustedStepTypes = programStepsQuery.Any( x => x.ImpactWeight.HasValue );
            bag.HasOrganizationObjectiveSteps = programStepsQuery.Any( x => x.OrganizationalObjectiveValueId != null );
            bag.HasEngagementTypeSteps = programStepsQuery.Any( x => x.EngagementType.HasValue && x.EngagementType != EngagementType.None );

            return bag;
        }

        /// <inheritdoc/>
        protected override StepProgramBag GetEntityBagForEdit( StepProgram entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            bag.StepProgramAttributes = GetStepTypeAttributes( entity.Id.ToString() ).ConvertAll( e => PublicAttributeHelper.GetPublicEditableAttribute( e ) );

            bag.Statuses = GetStepStatuses( entity.Id ).Select( s => new StepStatusBag()
            {
                Guid = s.Guid,
                Id = s.Id,
                IsActive = s.IsActive,
                IsCompleteStatus = s.IsCompleteStatus,
                Name = s.Name,
                StatusColor = s.StatusColor,
                IsSystem = s.IsSystem
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

            bag.StatusOptions = new StepStatusService( RockContext ).Queryable().Where( s => s.StepProgramId == entity.Id ).AsEnumerable().ToListItemBagList();

            return bag;
        }

        #endregion Get Entity Bag Methods

        /// <summary>
        /// Gets the StepType's attributes.
        /// </summary>
        /// <param name="stepProgramId">The Step Program identifier qualifier value.</param>
        /// <returns></returns>
        private List<Model.Attribute> GetStepTypeAttributes( string stepProgramId )
        {
            return new AttributeService( RockContext ).GetByEntityTypeId( new StepType().TypeId, true ).AsQueryable()
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
            var text = new StepWorkflowTriggerService( RockContext ).GetTriggerSettingsDescription( condition, qualifierSettings );
            var value = condition.ToStringSafe();

            return new ListItemBag() { Text = text, Value = value };
        }

        /// <summary>
        /// Gets all the available step statuses.
        /// </summary>
        /// <returns></returns>
        private List<StepStatus> GetStepStatuses( int stepProgramId )
        {
            return new StepStatusService( RockContext )
                .Queryable()
                .Where( s => s.StepProgramId == stepProgramId )
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Name )
                .ToList();
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

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( StepProgram entity, ValidPropertiesBox<StepProgramBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Category ),
                () => entity.CategoryId = box.Bag.Category.GetEntityId<Category>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.DefaultListView ),
                () => entity.DefaultListView = ( StepProgram.ViewMode ) box.Bag.DefaultListView );

            box.IfValidProperty( nameof( box.Bag.Statuses ),
                () => SaveStatuses( box.Bag, entity, RockContext ) );

            box.IfValidProperty( nameof( box.Bag.WorkflowTriggers ),
                () => SaveWorkflowTriggers( box.Bag, entity, RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CompletionFlow ),
                () => entity.CompletionFlow = box.Bag.CompletionFlow.Value );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override StepProgram GetInitialEntity()
        {
            return GetInitialEntity<StepProgram, StepProgramService>( RockContext, PageParameterKey.StepProgramId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out StepProgram entity, out BlockActionResult error )
        {
            var entityService = new StepProgramService( RockContext );
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
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> viewStateAttributes )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( RockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                RockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, RockContext );
            }
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

        #region Step Program View Helper Methods

        #region KPI Methods

        /// <summary>
        /// Gets the kpi HTML.
        /// </summary>
        private string GetKpi( string delimitedDateRange, int? stepProgramId, out string errorMessage )
        {
            var template = GetAttributeValue( AttributeKey.KpiLava );

            if ( !stepProgramId.HasValue )
            {
                errorMessage = "The specified Step Program was not found.";
                return null;
            }

            var stepProgram = StepProgramCache.Get( stepProgramId.Value );
            var startedQuery = GetStartedStepQuery( delimitedDateRange, stepProgramId.Value );
            var completedStepQuery = GetCompletedStepQuery( delimitedDateRange, stepProgramId.Value );
            var completedPrograms = GetCompletedProgramQuery( delimitedDateRange, stepProgramId.Value ).ToList();

            var individualsCompleting = completedPrograms.Count;
            var stepsStarted = startedQuery.Count();
            var stepsCompleted = completedStepQuery.Count();

            var daysToCompleteList = completedPrograms
                .Where( sps => sps.CompletedDateTime.HasValue && sps.StartedDateTime.HasValue )
                .Select( sps => ( sps.CompletedDateTime.Value - sps.StartedDateTime.Value ).Days );

            var avgDaysToComplete = daysToCompleteList.Any() ? ( int ) daysToCompleteList.Average() : 0;

            errorMessage = string.Empty;
            return template.ResolveMergeFields( new Dictionary<string, object>
            {
                { "IndividualsCompleting", individualsCompleting },
                { "AvgDaysToComplete", avgDaysToComplete },
                { "StepsStarted", stepsStarted },
                { "StepsCompleted", stepsCompleted },
                { "StepProgram", stepProgram }
            } );
        }

        /// <summary>
        /// Gets the active step type ids.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> GetActiveStepTypeIds( int stepProgramId )
        {
            var stepProgramCache = StepProgramCache.Get( stepProgramId );

            if ( stepProgramCache == null )
            {
                return new List<int>();
            }

            var stepTypeIds = stepProgramCache.StepTypes.Where( st => st.IsActive ).Select( st => st.Id );
            return stepTypeIds;
        }

        /// <summary>
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetStartedStepQuery( string delimitedDateRange, int stepProgramId )
        {
            var stepTypeIds = GetActiveStepTypeIds( stepProgramId );

            if ( stepTypeIds == null )
            {
                return null;
            }

            var stepService = new StepService( RockContext );

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
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetCompletedStepQuery( string delimitedDateRange, int stepProgramId )
        {
            var stepTypeIds = GetActiveStepTypeIds( stepProgramId );

            if ( stepTypeIds == null )
            {
                return null;
            }

            var stepService = new StepService( RockContext );

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    stepTypeIds.Contains( x.StepTypeId ) &&
                    x.CompletedDateKey != null );

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
        private IQueryable<StepProgramService.PersonStepProgramViewModel> GetCompletedProgramQuery( string delimitedDateRange, int stepProgramId )
        {
            var service = new StepProgramService( RockContext );
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

        #endregion KPI Methods

        #region Chart Methods

        /// <summary>
        /// Filters the provided steps by the given status filter and returns the matching query.
        /// </summary>
        /// <param name="selectedStatusFilter">The status filter value (e.g. "All", "AllComplete", "AllIncomplete", or a status GUID).</param>
        /// <param name="stepsQuery">The queryable collection of steps to filter.</param>
        /// <param name="isCompletionOnly">Outputs whether the filter represents only completed statuses.</param>
        /// <returns>A filtered IQueryable of steps based on the selected status filter.</returns>
        private IQueryable<Step> GetStepsFilteredByStatus( string selectedStatusFilter, IQueryable<Step> stepsQuery, out bool isCompletionOnly )
        {
            switch ( selectedStatusFilter )
            {
                case "All":
                    isCompletionOnly = false;
                    return stepsQuery;
                case "AllComplete":
                    isCompletionOnly = true;
                    return stepsQuery.Where( s => s.StepStatus.IsCompleteStatus );
                case "AllIncomplete":
                    isCompletionOnly = false;
                    return stepsQuery.Where( s => !s.StepStatus.IsCompleteStatus );
            }

            if ( Guid.TryParse( selectedStatusFilter, out Guid statusGuid ) )
            {
                isCompletionOnly = new StepStatusService( RockContext ).Get( statusGuid ).IsCompleteStatus;
                return stepsQuery.Where( s => s.StepStatus.Guid == statusGuid );
            }

            // If we reach here, no valid filter was provided, return all steps.
            isCompletionOnly = false;
            return stepsQuery;
        }

        /// <summary>
        /// Retrieves the campus from the current request context, if available.
        /// </summary>
        /// <returns>The selected campus as a CampusCache object, or null if none is found.</returns>
        private CampusCache GetSelectedCampusFromContext()
        {
            var campusContext = RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                return CampusCache.Get( campusContext.Id );
            }

            return null;
        }

        /// <summary>
        /// Builds chart data showing step program completions within the given date range and view type.
        /// </summary>
        /// <param name="stepProgram">The step program to report completions for.</param>
        /// <param name="timeUnitHelper">Helper value for grouping data by time intervals.</param>
        /// <param name="startDate">The start date of the reporting range.</param>
        /// <param name="endDate">The end date of the reporting range.</param>
        /// <param name="selectedProgramView">The type of chart view to generate (Trends, Totals, or Campuses).</param>
        /// <returns>A ChartDataBag containing the requested completion data, or null if no view matches.</returns>
        private ChartDataBag GetChartForStepProgramCompletions( StepProgramCache stepProgram, int timeUnitHelper, DateTime startDate, DateTime endDate, StepProgramView selectedProgramView )
        {
            var qry = new StepProgramCompletionService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( spc => spc.Campus )
                .Select( spc => new StepProgramCompletionProjection
                {
                    DateKey = spc.EndDateKey,
                    StepProgramId = spc.StepProgramId,
                    CampusId = spc.CampusId,
                    CampusIsActive = spc.Campus.IsActive,
                    CampusGuid = spc.Campus.Guid,
                    CampusName = spc.Campus.Name,
                    CampusOrder = spc.Campus.Order
                } )
                .Where( spc => spc.StepProgramId == stepProgram.Id
                    && spc.DateKey.HasValue );


            var startDateKey = startDate.ToDateKey();
            var endDateKey = endDate.ToDateKey();

            qry = qry.Where( x => x.DateKey >= startDateKey && x.DateKey <= endDateKey );

            var selectedCampus = GetSelectedCampusFromContext();

            if ( selectedCampus != null && selectedProgramView != StepProgramView.Campuses )
            {
                qry = qry.Where( x => x.CampusId == selectedCampus.Id );
            }

            switch ( selectedProgramView )
            {
                case StepProgramView.Trends:
                    return GetStepProgramCompletionTrends( timeUnitHelper, qry, startDate, endDate );
                case StepProgramView.Totals:
                    return GetStepProgramCompletionTotals( qry );
                case StepProgramView.Campuses:
                    return GetStepProgramCompletionCampuses( qry, selectedCampus?.Name );
            }

            return null;
        }

        /// <summary>
        /// Builds chart data for steps in a program within the given date range, filtered by status and view type.
        /// </summary>
        /// <param name="stepProgram">The step program to report on.</param>
        /// <param name="timeUnitHelper">Helper value for grouping data by time intervals.</param>
        /// <param name="startDate">The start date of the reporting range.</param>
        /// <param name="endDate">The end date of the reporting range.</param>
        /// <param name="selectedMeasure">The measure to use for charting (e.g., counts or impact).</param>
        /// <param name="statusFilter">A filter for step status (All, Complete, Incomplete, or status GUID).</param>
        /// <param name="selectedProgramView">The type of chart view to generate (Trends, Totals, or Campuses).</param>
        /// <returns>A ChartDataBag containing the requested step data, or null if no view matches.</returns>
        private ChartDataBag GetChartForSteps( StepProgramCache stepProgram, int timeUnitHelper, DateTime startDate, DateTime endDate, StepChartMeasure selectedMeasure, string statusFilter, StepProgramView selectedProgramView )
        {
            var stepService = new StepService( RockContext );
            var query = stepService.Queryable()
                .AsNoTracking()
                .Include( x => x.StepType )
                .Include( x => x.Campus )
                .Where( x =>
                    x.StepType.StepProgramId == stepProgram.Id
                    && x.StepType.IsActive );

            var selectedCampus = GetSelectedCampusFromContext();

            if ( selectedCampus != null && selectedProgramView != StepProgramView.Campuses )
            {
                query = query.Where( x => x.CampusId == selectedCampus.Id );
            }

            query = GetStepsFilteredByStatus( statusFilter, query, out bool isCompletionOnly );

            var stepProjectionQry = query.Select( s => new StepProjection
            {
                DateKey = isCompletionOnly ? s.CompletedDateKey : s.StartDateKey,
                CampusId = s.CampusId,
                CampusGuid = s.Campus.Guid,
                CampusIsActive = s.Campus.IsActive,
                CampusName = s.Campus.Name,
                CampusOrder = s.Campus.Order,
                AvgCampusAttendance = s.Campus.AverageWeekendAttendance,
                EngagementType = s.StepType.EngagementType,
                OrganizationObjective = s.StepType.OrganizationalObjectiveValue,
                StepTypeId = s.StepTypeId,
                StepTypeName = s.StepType.Name,
                StepTypeOrder = s.StepType.Order,
                ImpactWeight = s.StepType.ImpactWeight,
                HighlightColor = s.StepType.HighlightColor
            } )
                .Where( s => s.DateKey.HasValue );

            var startDateKey = startDate.ToDateKey();
            var endDateKey = endDate.ToDateKey();

            stepProjectionQry = stepProjectionQry.Where( x => x.DateKey >= startDateKey && x.DateKey <= endDateKey );

            switch ( selectedProgramView )
            {
                case StepProgramView.Trends:
                    return GetStepTrendsByMeasure( selectedMeasure, stepProjectionQry, timeUnitHelper, startDate, endDate );
                case StepProgramView.Totals:
                    return GetStepTotalsByMeasure( selectedMeasure, stepProjectionQry );
                case StepProgramView.Campuses:
                    return GetStepCampusesByMeasure( selectedMeasure, stepProjectionQry, selectedCampus );
            }

            return null;
        }

        /// <summary>
        /// Generates a list of dates between the start and end dates based on the specified time unit.
        /// </summary>
        /// <param name="timeUnitHelper">The time unit for stepping through dates (1 = daily, 100 = monthly, 10000 = yearly).</param>
        /// <param name="startDate">The starting date of the range.</param>
        /// <param name="endDate">The ending date of the range.</param>
        /// <returns>A list of DateTime values within the specified range.</returns>
        private List<DateTime> GetAllDateTimesWithinFilter( int timeUnitHelper, DateTime startDate, DateTime endDate )
        {
            var allDateTimes = new List<DateTime>();
            var cursor = startDate;

            while ( cursor <= endDate )
            {
                allDateTimes.Add( cursor );

                switch ( timeUnitHelper )
                {
                    case 1:
                        cursor = cursor.AddDays( 1 );
                        break;

                    case 100:
                        cursor = cursor.AddMonths( 1 );
                        break;

                    case 10000:
                        cursor = cursor.AddYears( 1 );
                        break;

                    default:
                        cursor = cursor.AddDays( 1 );
                        break;
                }
            }

            return allDateTimes;
        }

        #region Trends Chart Methods

        /// <summary>
        /// Builds trend chart data for steps based on the selected measure within a date range.
        /// </summary>
        /// <param name="selectedMeasure">The measure to chart (e.g., Steps, Impact, Totals, Objectives, or EngagementType).</param>
        /// <param name="qry">The step projection query to aggregate.</param>
        /// <param name="timeUnitHelper">The time unit for grouping data (1 = daily, 100 = monthly, 10000 = yearly).</param>
        /// <param name="startDate">The start date of the reporting range.</param>
        /// <param name="endDate">The end date of the reporting range.</param>
        /// <returns>A ChartDataBag with series data for the selected measure, or null if no match.</returns>
        private ChartDataBag GetStepTrendsByMeasure( StepChartMeasure selectedMeasure, IQueryable<StepProjection> qry, int timeUnitHelper, DateTime startDate, DateTime endDate )
        {
            qry = qry.Select( s => new StepProjection
            {
                DateKey = s.DateKey.HasValue ? s.DateKey / timeUnitHelper : null,
                CampusId = s.CampusId,
                CampusGuid = s.CampusGuid,
                CampusIsActive = s.CampusIsActive,
                CampusName = s.CampusName,
                CampusOrder = s.CampusOrder,
                AvgCampusAttendance = s.AvgCampusAttendance,
                EngagementType = s.EngagementType,
                OrganizationObjective = s.OrganizationObjective,
                StepTypeId = s.StepTypeId,
                StepTypeName = s.StepTypeName,
                StepTypeOrder = s.StepTypeOrder,
                ImpactWeight = s.ImpactWeight,
                HighlightColor = s.HighlightColor
            } );

            var allDates = GetAllDateTimesWithinFilter( timeUnitHelper, startDate, endDate );
            var chartData = new ChartDataBag();

            switch ( selectedMeasure )
            {
                case StepChartMeasure.Steps:
                    var stepsData = qry.GroupBy( s => new { DateKey = s.DateKey.Value, s.StepTypeId, s.StepTypeName, s.StepTypeOrder, s.HighlightColor } )
                        .Select( g => new
                        {
                            g.Key.DateKey,
                            g.Key.StepTypeId,
                            g.Key.StepTypeName,
                            g.Key.StepTypeOrder,
                            g.Key.HighlightColor,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    var stepsLookup = stepsData.ToDictionary( d => (d.DateKey, d.StepTypeId), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        DateLabels = allDates,
                        Series = stepsData.Select( d => new
                        {
                            d.StepTypeId,
                            d.StepTypeName,
                            d.HighlightColor,
                            d.StepTypeOrder
                        } )
                            .DistinctBy( d => d.StepTypeId )
                            .OrderBy( d => d.StepTypeOrder )
                            .ThenBy( d => d.StepTypeName )
                            .Select( stepType => new SeriesBag
                            {
                                Label = stepType.StepTypeName,
                                Data = allDates.Select( date => stepsLookup.TryGetValue( (date.ToDateKey() / timeUnitHelper, stepType.StepTypeId), out var count ) ? count : 0 ).ToList(),
                                Color = stepType.HighlightColor.IsNullOrWhiteSpace() ? null : stepType.HighlightColor
                            } )
                            .ToList()
                    };

                    return chartData;

                case StepChartMeasure.ImpactAdjustedSteps:
                    var impactStepsData = qry.Where( s => s.ImpactWeight.HasValue )
                        .GroupBy( s => new { DateKey = s.DateKey.Value, s.StepTypeId, s.StepTypeName, s.StepTypeOrder, ImpactWeight = s.ImpactWeight.Value, s.HighlightColor } )
                        .Select( g => new
                        {
                            g.Key.DateKey,
                            g.Key.StepTypeId,
                            g.Key.StepTypeName,
                            g.Key.StepTypeOrder,
                            g.Key.HighlightColor,
                            Count = ( double ) g.Count() * g.Key.ImpactWeight
                        } )
                        .ToList();

                    var impactStepsLookup = impactStepsData.ToDictionary( d => (d.DateKey, d.StepTypeId), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        DateLabels = allDates,
                        Series = impactStepsData.Select( d => new
                        {
                            d.StepTypeId,
                            d.StepTypeName,
                            d.HighlightColor,
                            d.StepTypeOrder
                        } )
                            .DistinctBy( d => d.StepTypeId )
                            .OrderBy( d => d.StepTypeOrder )
                            .ThenBy( d => d.StepTypeName )
                            .Select( stepType => new SeriesBag
                            {
                                Label = stepType.StepTypeName,
                                Data = allDates.Select( date => impactStepsLookup.TryGetValue( (date.ToDateKey() / timeUnitHelper, stepType.StepTypeId), out var count ) ? count : 0 ).ToList(),
                                Color = stepType.HighlightColor
                            } )
                            .ToList()
                    };

                    return chartData;

                case StepChartMeasure.TotalSteps:
                    var totalStepsData = qry.GroupBy( s => s.DateKey.Value )
                        .Select( g => new
                        {
                            DateKey = g.Key,
                            Count = ( double ) g.Count()
                        } )
                        .ToDictionary( g => g.DateKey, g => g.Count );

                    chartData = new ChartDataBag
                    {
                        DateLabels = allDates,
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Steps",
                                Data = allDates.Select( date => totalStepsData.TryGetValue( date.ToDateKey() / timeUnitHelper, out var count ) ? count : 0 ).ToList()
                            }
                        }
                    };

                    return chartData;

                case StepChartMeasure.TotalStepAdjustedImpact:
                    // Step 1: per-type, per-date weighted counts
                    var impactStepsPartialData = qry.Where( s => s.ImpactWeight.HasValue )
                        .GroupBy( s => new { DateKey = s.DateKey.Value, s.ImpactWeight } )
                        .Select( g => new
                        {
                            g.Key.DateKey,
                            WeightedCount = g.Count() * g.Key.ImpactWeight.Value
                        } )
                        .ToList();

                    // Step 2: per-date totals by summing the per-type counts
                    var totalImpactSteps = impactStepsPartialData
                        .GroupBy( d => d.DateKey )
                        .Select( g => new
                        {
                            DateKey = g.Key,
                            Total = ( double ) g.Sum( x => x.WeightedCount )
                        } )
                        .ToList();

                    chartData = new ChartDataBag
                    {
                        DateLabels = allDates,
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Total Impact Steps",
                                Data = allDates.Select(date => totalImpactSteps.FirstOrDefault( t => t.DateKey == date.ToDateKey() / timeUnitHelper )?.Total ?? 0).ToList(),
                            }
                        }
                    };

                    return chartData;

                case StepChartMeasure.OrganizationObjective:
                    var orgObjectiveStepsData = qry.Where( s => s.OrganizationObjective != null && s.OrganizationObjective.Value != null )
                        .GroupBy( s => new { DateKey = s.DateKey.Value, OrganizationObjectiveValue = s.OrganizationObjective.Value } )
                        .Select( g => new
                        {
                            g.Key.DateKey,
                            g.Key.OrganizationObjectiveValue,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    var orgObjectiveStepsLookup = orgObjectiveStepsData.ToDictionary( d => (d.DateKey, d.OrganizationObjectiveValue), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        DateLabels = allDates,
                        Series = orgObjectiveStepsData.Select( d => d.OrganizationObjectiveValue )
                            .Distinct()
                            .Select( organizationObjective => new SeriesBag
                            {
                                Label = organizationObjective,
                                Data = allDates.Select( date => orgObjectiveStepsLookup.TryGetValue( (date.ToDateKey() / timeUnitHelper, organizationObjective), out var count ) ? count : 0 ).ToList()
                            } )
                            .ToList()
                    };

                    return chartData;

                case StepChartMeasure.EngagementType:
                    var engagementStepsData = qry.Where( s => s.EngagementType.HasValue && s.EngagementType.Value != EngagementType.None )
                        .GroupBy( s => new { DateKey = s.DateKey.Value, EngagementType = s.EngagementType.Value.ToString() } )
                        .Select( g => new
                        {
                            g.Key.DateKey,
                            g.Key.EngagementType,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    var engagementStepsLookup = engagementStepsData.ToDictionary( d => (d.DateKey, d.EngagementType), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        DateLabels = allDates,
                        Series = engagementStepsData.Select( d => d.EngagementType )
                            .Distinct()
                            .Select( engagementType => new SeriesBag
                            {
                                Label = engagementType,
                                Data = allDates.Select( date => engagementStepsLookup.TryGetValue( (date.ToDateKey() / timeUnitHelper, engagementType), out var count ) ? count : 0 ).ToList()
                            } )
                            .ToList()
                    };

                    return chartData;

            }

            return null;
        }

        /// <summary>
        /// Builds trend chart data showing step program completions over the given date range.
        /// </summary>
        /// <param name="timeUnitHelper">The time unit for grouping data (1 = daily, 100 = monthly, 10000 = yearly).</param>
        /// <param name="qry">The step program completion query to aggregate.</param>
        /// <param name="startDate">The start date of the reporting range.</param>
        /// <param name="endDate">The end date of the reporting range.</param>
        /// <returns>A ChartDataBag with time-based completion data.</returns>
        private ChartDataBag GetStepProgramCompletionTrends( int timeUnitHelper, IQueryable<StepProgramCompletionProjection> qry, DateTime startDate, DateTime endDate )
        {
            var stepProgramCompletionData = qry.GroupBy( spc => spc.DateKey.Value / timeUnitHelper )
                .Select( g => new
                {
                    DateKey = g.Key,
                    Count = ( double ) g.Count()
                } )
            .ToList();

            var allDates = GetAllDateTimesWithinFilter( timeUnitHelper, startDate, endDate );

            var stepProgramCompletionsLookup = stepProgramCompletionData.ToDictionary( d => d.DateKey, d => d.Count );

            var chartData = new ChartDataBag
            {
                DateLabels = allDates,
                Series = new List<SeriesBag>
                {
                    new SeriesBag
                    {
                        Label = "Individual Program Completions",
                        Data = allDates.Select( date => stepProgramCompletionsLookup.TryGetValue( date.ToDateKey() / timeUnitHelper, out var count ) ? count : 0 ).ToList()
                    }
                }
            };

            return chartData;
        }

        #endregion Trends Chart Methods

        #region Totals Chart Methods

        /// <summary>
        /// Builds total chart data for steps based on the selected measure.
        /// </summary>
        /// <param name="selectedMeasure">The measure to chart (e.g., Steps, Impact, Totals, Objectives, or EngagementType).</param>
        /// <param name="qry">The step projection query to aggregate.</param>
        /// <returns>A ChartDataBag with aggregated totals for the selected measure.</returns>
        private ChartDataBag GetStepTotalsByMeasure( StepChartMeasure selectedMeasure, IQueryable<StepProjection> qry )
        {
            var stringLabels = new List<string>();
            var chartData = new ChartDataBag();

            switch ( selectedMeasure )
            {
                case StepChartMeasure.Steps:
                    var stepsData = qry.GroupBy( s => new { s.StepTypeId, s.StepTypeName, s.StepTypeOrder } )
                        .Select( g => new
                        {
                            g.Key.StepTypeId,
                            g.Key.StepTypeName,
                            g.Key.StepTypeOrder,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    var orderedSteps = stepsData
                        .DistinctBy( d => d.StepTypeId )
                        .OrderBy( d => d.StepTypeOrder )
                        .ThenBy( d => d.StepTypeName )
                        .ToList();

                    stringLabels = orderedSteps
                        .Select( d => d.StepTypeName )
                        .ToList();

                    var stepsLookup = stepsData.ToDictionary( d => d.StepTypeId, d => d.Count );

                    chartData = new ChartDataBag
                    {
                        StringLabels = stringLabels,
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Steps",
                                Data = orderedSteps.Select( d => stepsLookup.TryGetValue( d.StepTypeId, out var count) ? count : 0 ).ToList()
                            }
                        }
                    };

                    return chartData;

                case StepChartMeasure.ImpactAdjustedSteps:
                    var impactStepsData = qry.Where( s => s.ImpactWeight.HasValue )
                        .GroupBy( s => new { s.StepTypeId, s.StepTypeName, s.StepTypeOrder, ImpactWeight = s.ImpactWeight.Value, s.HighlightColor } )
                        .Select( g => new
                        {
                            g.Key.StepTypeId,
                            g.Key.StepTypeName,
                            g.Key.StepTypeOrder,
                            g.Key.HighlightColor,
                            Count = ( double ) g.Count() * g.Key.ImpactWeight
                        } )
                        .ToList();

                    var orderedImpactSteps = impactStepsData
                        .DistinctBy( d => d.StepTypeId )
                        .OrderBy( d => d.StepTypeOrder )
                        .ThenBy( d => d.StepTypeName )
                        .ToList();

                    stringLabels = orderedImpactSteps
                        .Select( d => d.StepTypeName )
                        .ToList();

                    var impactStepsLookup = impactStepsData.ToDictionary( d => d.StepTypeId, d => d.Count );

                    chartData = new ChartDataBag
                    {
                        StringLabels = stringLabels,
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Impact-Adjusted Steps",
                                Data = orderedImpactSteps.Select( d => impactStepsLookup.TryGetValue( d.StepTypeId, out var count) ? count : 0 ).ToList()
                            }
                        }
                    };

                    return chartData;

                case StepChartMeasure.TotalSteps:
                    var totalStepsCount = qry.Count();

                    chartData = new ChartDataBag
                    {
                        StringLabels = new List<string> { "Total Steps" },
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Total Steps",
                                Data = new List<double> { totalStepsCount }
                            }
                        }
                    };

                    return chartData;

                case StepChartMeasure.TotalStepAdjustedImpact:
                    var totalStepAdjustedImpact = qry
                        .Where( s => s.ImpactWeight.HasValue )
                        .Sum( s => s.ImpactWeight );

                    chartData = new ChartDataBag
                    {
                        StringLabels = new List<string> { "Step-Adjusted Impact" },
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Step-Adjusted Impact",
                                Data = new List<double> { totalStepAdjustedImpact ?? 0 }
                            }
                        }
                    };

                    return chartData;

                case StepChartMeasure.OrganizationObjective:
                    var orgObjectiveStepsData = qry.Where( s => s.OrganizationObjective != null && s.OrganizationObjective.Value != null )
                        .GroupBy( s => new { OrganizationObjectiveValue = s.OrganizationObjective.Value } )
                        .Select( g => new
                        {
                            g.Key.OrganizationObjectiveValue,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    stringLabels = orgObjectiveStepsData
                        .Select( d => d.OrganizationObjectiveValue )
                        .Distinct()
                        .OrderBy( n => n )
                        .ToList();

                    var orgObjectiveStepsLookup = orgObjectiveStepsData.ToDictionary( d => d.OrganizationObjectiveValue, d => d.Count );

                    chartData = new ChartDataBag
                    {
                        StringLabels = stringLabels,
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Organization Objective Steps",
                                Data = stringLabels.Select( name => orgObjectiveStepsLookup.TryGetValue(name, out var count) ? count : 0).ToList()
                            }
                        }
                    };

                    return chartData;

                case StepChartMeasure.EngagementType:
                    var engagementStepsData = qry.Where( s => s.EngagementType.HasValue && s.EngagementType.Value != EngagementType.None )
                        .GroupBy( s => new { EngagementType = s.EngagementType.Value.ToString() } )
                        .Select( g => new
                        {
                            g.Key.EngagementType,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    stringLabels = engagementStepsData
                        .Select( d => d.EngagementType )
                        .Distinct()
                        .OrderBy( n => n )
                        .ToList();

                    var engagementStepsLookup = engagementStepsData.ToDictionary( d => d.EngagementType, d => d.Count );

                    chartData = new ChartDataBag
                    {
                        StringLabels = stringLabels,
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Engagement Type Steps",
                                Data = stringLabels.Select( name => engagementStepsLookup.TryGetValue(name, out var count) ? count : 0).ToList()
                            }
                        }
                    };

                    return chartData;
            }
            return null;
        }

        /// <summary>
        /// Builds total chart data for step program completions.
        /// </summary>
        /// <param name="qry">The step program completion query to aggregate.</param>
        /// <returns>A ChartDataBag containing the total number of completions.</returns>
        private ChartDataBag GetStepProgramCompletionTotals( IQueryable<StepProgramCompletionProjection> qry )
        {
            var stepProgramCompletionCount = qry.Count();

            var chartData = new ChartDataBag
            {
                StringLabels = new List<string> { "Individual Program Completions" },
                Series = new List<SeriesBag>
                {
                    new SeriesBag
                    {
                        Label = "Individual Program Completions",
                        Data = new List<double> { stepProgramCompletionCount }
                    }
                }
            };

            return chartData;
        }

        #endregion Totals Chart Methods

        #region Campus Chart Methods

        /// <summary>
        /// Builds campus-level chart data for steps based on the selected measure.
        /// </summary>
        /// <param name="selectedMeasure">The measure to chart (e.g., Steps, Impact, Totals, Objectives, EngagementType, or Attendance ratios).</param>
        /// <param name="qry">The step projection query to aggregate.</param>
        /// <param name="selectedCampus">The campus to emphasize in the chart, or null for all campuses.</param>
        /// <returns>A ChartDataBag with campus-based series data for the selected measure.</returns>
        private ChartDataBag GetStepCampusesByMeasure( StepChartMeasure selectedMeasure, IQueryable<StepProjection> qry, CampusCache selectedCampus )
        {
            var campusLabels = new List<ListItemBag>();
            var chartData = new ChartDataBag();
            bool isCampusSelected = selectedCampus != null;
            var selectedCampusGuidString = isCampusSelected ? selectedCampus.Guid.ToString() : string.Empty;

            // Filter out null campus.
            qry = qry.Where( s => s.CampusId.HasValue && s.CampusIsActive == true );

            switch ( selectedMeasure )
            {
                case StepChartMeasure.Steps:
                    var stepsData = qry.GroupBy( s => new { s.CampusGuid, s.CampusName, s.CampusOrder, s.StepTypeId, s.StepTypeName, s.StepTypeOrder, s.HighlightColor } )
                        .Select( g => new
                        {
                            g.Key.CampusGuid,
                            g.Key.CampusName,
                            g.Key.CampusOrder,
                            g.Key.StepTypeId,
                            g.Key.StepTypeName,
                            g.Key.StepTypeOrder,
                            g.Key.HighlightColor,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    campusLabels = stepsData
                        .DistinctBy( d => d.CampusGuid )
                        .OrderBy( d => d.CampusOrder )
                        .ThenBy( d => d.CampusName )
                        .Select( d => new ListItemBag
                        {
                            Text = d.CampusName,
                            Value = d.CampusGuid.ToString()
                        } )
                        .ToList();

                    var stepsLookup = stepsData.ToDictionary( d => (d.CampusGuid.ToString(), d.StepTypeId), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        CampusLabels = campusLabels,
                        Series = stepsData.Select( d => new
                        {
                            d.StepTypeId,
                            d.StepTypeName,
                            d.StepTypeOrder,
                            d.HighlightColor
                        } )
                            .DistinctBy( d => d.StepTypeId )
                            .OrderBy( d => d.StepTypeOrder )
                            .ThenBy( d => d.StepTypeName )
                            .Select( stepType => new SeriesBag
                            {
                                Label = stepType.StepTypeName,
                                Data = campusLabels.Select( campusBag => stepsLookup.TryGetValue( (campusBag.Value, stepType.StepTypeId), out var count ) ? count : 0 ).ToList(),
                                Color = stepType.HighlightColor,
                                Opacity = campusLabels.Select( campusBag => !isCampusSelected || campusBag.Value == selectedCampusGuidString ? 1 : 0.25 ).ToList()
                            } )
                            .ToList()
                    };

                    return chartData;

                case StepChartMeasure.ImpactAdjustedSteps:
                    var impactStepsData = qry.Where( s => s.ImpactWeight.HasValue )
                        .GroupBy( s => new { s.CampusGuid, s.CampusName, s.CampusOrder, s.StepTypeId, s.StepTypeName, s.StepTypeOrder, ImpactWeight = s.ImpactWeight.Value, s.HighlightColor } )
                        .Select( g => new
                        {
                            g.Key.CampusGuid,
                            g.Key.CampusName,
                            g.Key.CampusOrder,
                            g.Key.StepTypeId,
                            g.Key.StepTypeName,
                            g.Key.StepTypeOrder,
                            g.Key.HighlightColor,
                            Count = ( double ) g.Count() * g.Key.ImpactWeight
                        } )
                        .ToList();

                    campusLabels = impactStepsData
                        .DistinctBy( d => d.CampusGuid )
                        .OrderBy( d => d.CampusOrder )
                        .ThenBy( d => d.CampusName )
                        .Select( d => new ListItemBag
                        {
                            Text = d.CampusName,
                            Value = d.CampusGuid.ToString()
                        } )
                        .ToList();

                    var impactStepsLookup = impactStepsData.ToDictionary( d => (d.CampusGuid.ToString(), d.StepTypeId), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        CampusLabels = campusLabels,
                        Series = impactStepsData.Select( d => new
                        {
                            d.StepTypeId,
                            d.StepTypeName,
                            d.StepTypeOrder,
                            d.HighlightColor
                        } )
                            .DistinctBy( d => d.StepTypeId )
                            .OrderBy( d => d.StepTypeOrder )
                            .ThenBy( d => d.StepTypeOrder )
                            .Select( stepType => new SeriesBag
                            {
                                Label = stepType.StepTypeName,
                                Data = campusLabels.Select( campusBag => impactStepsLookup.TryGetValue( (campusBag.Value, stepType.StepTypeId), out var count ) ? count : 0 ).ToList(),
                                Color = stepType.HighlightColor,
                                Opacity = campusLabels.Select( campusBag => !isCampusSelected || campusBag.Value == selectedCampusGuidString ? 1 : 0.25 ).ToList()
                            } )
                            .ToList()
                    };

                    return chartData;

                case StepChartMeasure.TotalSteps:
                    var totalStepsData = qry.GroupBy( s => new { s.CampusGuid, s.CampusName, s.CampusOrder } )
                        .Select( g => new
                        {
                            g.Key.CampusGuid,
                            g.Key.CampusName,
                            g.Key.CampusOrder,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    campusLabels = totalStepsData
                        .DistinctBy( d => d.CampusGuid )
                        .OrderBy( d => d.CampusOrder )
                        .ThenBy( d => d.CampusName )
                        .Select( d => new ListItemBag
                        {
                            Text = d.CampusName,
                            Value = d.CampusGuid.ToString()
                        } )
                        .ToList();

                    var totalStepsLookup = totalStepsData.ToDictionary( d => d.CampusGuid.ToString(), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        CampusLabels = campusLabels,
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Total Steps",
                                Data = campusLabels.Select( campusBag => totalStepsLookup.TryGetValue( campusBag.Value , out var count ) ? count : 0 ).ToList(),
                                Opacity = campusLabels.Select( campusBag => !isCampusSelected || campusBag.Value == selectedCampusGuidString ? 1 : 0.25 ).ToList()
                            }
                        }
                    };

                    return chartData;

                case StepChartMeasure.TotalStepAdjustedImpact:
                    var impactStepsPartialData = qry.Where( s => s.ImpactWeight.HasValue )
                        .GroupBy( s => new { s.CampusGuid, s.CampusName, s.CampusOrder, s.ImpactWeight } )
                        .Select( g => new
                        {
                            g.Key.CampusGuid,
                            g.Key.CampusName,
                            g.Key.CampusOrder,
                            WeightedCount = g.Count() * g.Key.ImpactWeight.Value
                        } )
                        .ToList();

                    var totalImpactSteps = impactStepsPartialData
                        .GroupBy( d => new { d.CampusGuid, d.CampusName, d.CampusOrder } )
                        .Select( g => new
                        {
                            g.Key.CampusGuid,
                            g.Key.CampusName,
                            g.Key.CampusOrder,
                            Total = ( double ) g.Sum( x => x.WeightedCount )
                        } )
                        .ToList();

                    campusLabels = totalImpactSteps
                        .DistinctBy( d => d.CampusGuid )
                        .OrderBy( d => d.CampusOrder )
                        .ThenBy( d => d.CampusName )
                        .Select( d => new ListItemBag
                        {
                            Text = d.CampusName,
                            Value = d.CampusGuid.ToString()
                        } )
                        .ToList();

                    chartData = new ChartDataBag
                    {
                        CampusLabels = campusLabels,
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Total Impact Steps",
                                Data = campusLabels.Select( campus => totalImpactSteps.FirstOrDefault( t => t.CampusGuid.ToString() == campus.Value )?.Total ?? 0 ).ToList(),
                                Opacity = campusLabels.Select( campusBag => !isCampusSelected || campusBag.Value == selectedCampusGuidString ? 1 : 0.25 ).ToList()
                            }
                        }
                    };

                    return chartData;

                case StepChartMeasure.OrganizationObjective:
                    var orgObjectiveStepsData = qry.Where( s => s.OrganizationObjective != null && s.OrganizationObjective.Value != null )
                        .GroupBy( s => new { s.CampusGuid, s.CampusName, s.CampusOrder, OrganizationObjectiveValue = s.OrganizationObjective.Value } )
                        .Select( g => new
                        {
                            g.Key.CampusGuid,
                            g.Key.CampusName,
                            g.Key.CampusOrder,
                            g.Key.OrganizationObjectiveValue,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    campusLabels = orgObjectiveStepsData
                        .DistinctBy( d => d.CampusGuid )
                        .OrderBy( d => d.CampusOrder )
                        .ThenBy( d => d.CampusName )
                        .Select( d => new ListItemBag
                        {
                            Text = d.CampusName,
                            Value = d.CampusGuid.ToString()
                        } )
                        .ToList();

                    var orgObjectiveStepsLookup = orgObjectiveStepsData.ToDictionary( d => (d.CampusGuid.ToString(), d.OrganizationObjectiveValue), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        CampusLabels = campusLabels,
                        Series = orgObjectiveStepsData.Select( d => d.OrganizationObjectiveValue )
                            .Distinct()
                            .Select( organizationObjective => new SeriesBag
                            {
                                Label = organizationObjective,
                                Data = campusLabels.Select( campusBag => orgObjectiveStepsLookup.TryGetValue( (campusBag.Value, organizationObjective), out var count ) ? count : 0 ).ToList(),
                                Opacity = campusLabels.Select( campusBag => !isCampusSelected || campusBag.Value == selectedCampusGuidString ? 1 : 0.25 ).ToList()
                            } )
                            .ToList()
                    };

                    return chartData;

                case StepChartMeasure.EngagementType:
                    var engagementStepsData = qry.Where( s => s.EngagementType.HasValue && s.EngagementType.Value != EngagementType.None )
                        .GroupBy( s => new { s.CampusGuid, s.CampusName, s.CampusOrder, EngagementType = s.EngagementType.Value.ToString() } )
                        .Select( g => new
                        {
                            g.Key.CampusGuid,
                            g.Key.CampusName,
                            g.Key.CampusOrder,
                            g.Key.EngagementType,
                            Count = ( double ) g.Count()
                        } )
                        .ToList();

                    campusLabels = engagementStepsData
                        .DistinctBy( d => d.CampusGuid )
                        .OrderBy( d => d.CampusOrder )
                        .ThenBy( d => d.CampusName )
                        .Select( d => new ListItemBag
                        {
                            Text = d.CampusName,
                            Value = d.CampusGuid.ToString()
                        } )
                        .ToList();

                    var engagementStepsLookup = engagementStepsData.ToDictionary( d => (d.CampusGuid.ToString(), d.EngagementType), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        CampusLabels = campusLabels,
                        Series = engagementStepsData.Select( d => d.EngagementType )
                            .Distinct()
                            .Select( engagementType => new SeriesBag
                            {
                                Label = engagementType,
                                Data = campusLabels.Select( campusBag => engagementStepsLookup.TryGetValue( (campusBag.Value, engagementType), out var count ) ? count : 0 ).ToList(),
                                Opacity = campusLabels.Select( campusBag => !isCampusSelected || campusBag.Value == selectedCampusGuidString ? 1 : 0.25 ).ToList()
                            } )
                            .ToList()
                    };

                    return chartData;

                case StepChartMeasure.AvgTotalStepsPerWeekendAttendee:
                    var avgTotalStepPerWeekendAttendeesData = qry.Where( s => s.AvgCampusAttendance.HasValue )
                    .GroupBy( s => new { s.CampusGuid, s.CampusName, s.CampusOrder, AvgCampusAttendance = s.AvgCampusAttendance.Value } )
                    .Select( g => new
                    {
                        g.Key.CampusGuid,
                        g.Key.CampusName,
                        g.Key.CampusOrder,
                        Count = Math.Round( ( double ) g.Count() / g.Key.AvgCampusAttendance, 0 )
                    } )
                    .ToList();

                    campusLabels = avgTotalStepPerWeekendAttendeesData
                        .DistinctBy( d => d.CampusGuid )
                        .OrderBy( d => d.CampusOrder )
                        .ThenBy( d => d.CampusName )
                        .Select( d => new ListItemBag
                        {
                            Text = d.CampusName,
                            Value = d.CampusGuid.ToString()
                        } ).ToList();

                    var avgStepPerAttendeeLookup = avgTotalStepPerWeekendAttendeesData.ToDictionary( d => d.CampusGuid.ToString(), d => d.Count );

                    chartData = new ChartDataBag
                    {
                        CampusLabels = campusLabels,
                        Series = new List<SeriesBag>
                        {
                            new SeriesBag
                            {
                                Label = "Average Total Steps Per Weekend Attendee",
                                Data = campusLabels.Select( campusBag => avgStepPerAttendeeLookup.TryGetValue( campusBag.Value, out var count) ? count : 0 ).ToList(),
                                Opacity = campusLabels.Select( campusBag => !isCampusSelected || campusBag.Value == selectedCampusGuidString ? 1 : 0.25 ).ToList()
                            }
                        }
                    };

                    return chartData;
            }

            return null;
        }

        /// <summary>
        /// Builds campus-level chart data for step program completions.
        /// </summary>
        /// <param name="qry">The step program completion query to aggregate.</param>
        /// <param name="selectedCampusName">The campus name to emphasize in the chart, or null/empty for all campuses.</param>
        /// <returns>A ChartDataBag with campus-based completion data.</returns>
        private ChartDataBag GetStepProgramCompletionCampuses( IQueryable<StepProgramCompletionProjection> qry, string selectedCampusName )
        {
            bool isCampusSelected = selectedCampusName.IsNotNullOrWhiteSpace();

            // Filter out null campus
            qry = qry.Where( s => s.CampusId.HasValue && s.CampusIsActive == true );

            var stepProgramCompletionData = qry.GroupBy( spc => new { spc.CampusGuid, spc.CampusName, spc.CampusOrder } )
                .Select( g => new
                {
                    g.Key.CampusGuid,
                    g.Key.CampusName,
                    g.Key.CampusOrder,
                    Count = ( double ) g.Count()
                } )
            .ToList();

            var campusLabels = stepProgramCompletionData
                .DistinctBy( d => d.CampusGuid )
                .OrderBy( d => d.CampusOrder )
                .ThenBy( d => d.CampusName )
                .Select( d => new ListItemBag
                {
                    Text = d.CampusName,
                    Value = d.CampusGuid.ToString()
                } )
                .ToList();

            var stepProgramCompletionsLookup = stepProgramCompletionData.ToDictionary( d => d.CampusName, d => d.Count );

            var chartData = new ChartDataBag
            {
                CampusLabels = campusLabels,
                Series = new List<SeriesBag>
                {
                    new SeriesBag
                    {
                        Label = "Individual Program Completions",
                        Data = campusLabels.Select( campusBag => stepProgramCompletionsLookup.TryGetValue(campusBag.Text, out var count) ? count : 0).ToList(),
                        Opacity = campusLabels.Select( campusBag => !isCampusSelected || campusBag.Text == selectedCampusName ? 1 : 0.25 ).ToList()
                    }
                }
            };

            return chartData;
        }

        #endregion Campus Chart Methods

        #endregion Chart Methods

        #region Step Flow Methods

        /// <summary>
        /// Builds the Sankey diagram configuration for a step program, including legend HTML and colors.
        /// </summary>
        /// <param name="stepProgram">The step program whose steps are used to generate the configuration.</param>
        /// <returns>A SankeyDiagramSettingsBag containing the flow legend and settings.</returns>
        private SankeyDiagramSettingsBag GetStepFlowConfigBag( StepProgram stepProgram )
        {
            var lavaNodes = new List<Object>();
            int order = 0;

            foreach ( StepType step in stepProgram.StepTypes )
            {
                lavaNodes.Add( new
                {
                    Key = ++order,
                    StepName = step.Name,
                    Color = step.HighlightColor.IsNotNullOrWhiteSpace() ? step.HighlightColor : GetNextDefaultColor()
                } );
            }

            // The default value
            string lavaTemplate = "<div class=\"flow-legend\">\n" +
            "{% for stepItem in Steps %}\n" +
            "    <div class=\"flow-key\">\n" +
            "        <span class=\"color\" style=\"background-color:{{stepItem.Color}};\"></span>\n" +
            "        <span class=\"step-text\">{{stepItem.StepName}}</span>\n" +
            "    </div>\n" +
            "{% endfor %}\n" +
            "</div>";
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "Steps", lavaNodes );
            var legendHtml = lavaTemplate.ResolveMergeFields( mergeFields );

            return new SankeyDiagramSettingsBag
            {
                LegendHtml = legendHtml
            };
        }

        /// <summary>
        /// Builds an HTML tooltip string describing the flow between two step types.
        /// </summary>
        /// <param name="source">The source step type.</param>
        /// <param name="target">The target step type.</param>
        /// <param name="units">The number of steps taken between source and target.</param>
        /// <param name="days">The average number of days between steps, or null if unknown.</param>
        /// <returns>An HTML-formatted tooltip string.</returns>
        private string BuildTooltip( StepTypeCache source, StepTypeCache target, int units, int? days )
        {
            string sourceName = source?.Name ?? "Unknown Source";
            string targetName = target?.Name ?? "Unknown Target";
            string dayString = days.HasValue ? days.Value.ToString() : "Unknown";

            return $"<p><strong>{sourceName} > {targetName}</strong></p>" +
                $"Steps Taken: {units}<br/>" +
                $"Avg Days Between Steps: {dayString}";
        }

        /// <summary>
        /// Retrieves the next default color in sequence, cycling back when the list ends.
        /// </summary>
        /// <returns>A hex color string from the default color list.</returns>
        private string GetNextDefaultColor()

        {
            if ( currentColorIndex >= defaultColors.Length )
            {
                currentColorIndex = 0;
            }

            return defaultColors[currentColorIndex++];
        }

        /// <summary>
        /// Get the parameters dictionary for sending in to the DB query
        /// </summary>
        /// <param name="maxLevels">The maximum number of levels for any one person to complete</param>
        /// <param name="dateRangeStartDate">Filter dataset to only include steps that took place after this date.</param>
        /// <param name="dateRangeEndDate">Filter dataset to only include steps that took place before this date.</param>
        /// <returns></returns>
        private Dictionary<string, object> GetStepFlowParameters( int maxLevels, SlidingDateRangeBag date, List<int> startingStepTypeIds )
        {
            var parameters = new Dictionary<string, object>();

            // Generate a date range from the SlidingDateRangePicker's value
            var testRange = new SlidingDateRangePicker
            {
                SlidingDateRangeMode = ( SlidingDateRangePicker.SlidingDateRangeType ) ( int ) date.RangeType,
                TimeUnit = ( SlidingDateRangePicker.TimeUnitType ) ( int ) ( date.TimeUnit ?? 0 ),
                NumberOfTimeUnits = date.TimeValue ?? 1,
                DateRangeModeStart = date.LowerDate?.DateTime,
                DateRangeModeEnd = date.UpperDate?.DateTime
            };

            var dateRange = testRange.SelectedDateRange;

            parameters.Add( "StartingStepTypeIds", startingStepTypeIds.ConvertToEntityIdListParameter( "StartingStepTypeIds" ) );

            if ( dateRange.Start != null )
            {
                parameters.Add( "DateRangeStartDate", dateRange.Start );
            }
            else
            {
                parameters.Add( "DateRangeStartDate", DBNull.Value );
            }

            if ( dateRange.End != null )
            {
                parameters.Add( "DateRangeEndDate", dateRange.End );
            }
            else
            {
                parameters.Add( "DateRangeEndDate", DBNull.Value );
            }

            if ( maxLevels > 0 )
            {
                parameters.Add( "MaxLevels", maxLevels );
            }

            var campusContext = RequestContext.GetContextEntity<Campus>();
            if ( campusContext != null )
            {
                parameters.Add( "CampusId", campusContext.Id );
            }
            else
            {
                parameters.Add( "CampusId", DBNull.Value );
            }

            parameters.Add( "StepProgramId", StepProgramCache.Get( PageParameter( PageParameterKey.StepProgramId ), !PageCache.Layout.Site.DisablePredictableIds ).Id );
            parameters.Add( "DataViewId", DBNull.Value );

            return parameters;
        }

        #endregion Step Flow Methods

        #endregion Step Program View Helper Methods

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Changes the ordered position of a single step status.
        /// </summary>
        /// <param name="key">The identifier of the step status that will be moved.</param>
        /// <param name="beforeKey">The identifier of the step status it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderStepStatus( string key, string beforeKey )
        {
            var stepProgram = StepProgramCache.Get( PageParameter( PageParameterKey.StepProgramId ), !PageCache.Layout.Site.DisablePredictableIds );
            if ( stepProgram == null )
            {
                return ActionBadRequest( "Step program not found." );
            }

            var items = GetStepStatuses( stepProgram.Id );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();
            return ActionOk();
        }

        /// <summary>
        /// Changes the ordered position of a single step type attribute.
        /// </summary>
        /// <param name="key">The identifier of the step type attribute that will be moved.</param>
        /// <param name="beforeKey">The identifier of the step type attribute it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderStepTypeAttribute( string key, string beforeKey )
        {
            var stepProgram = StepProgramCache.Get( PageParameter( PageParameterKey.StepProgramId ), !PageCache.Layout.Site.DisablePredictableIds );
            if ( stepProgram == null )
            {
                return ActionBadRequest( "Step program not found." );
            }

            var items = GetStepTypeAttributes( stepProgram.Id.ToString() );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();
            return ActionOk();
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var box = new DetailBlockBox<StepProgramBag, StepProgramDetailOptionsBag>
            {
                Entity = GetEntityBagForEdit( entity )
            };

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<StepProgramBag>
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
        public BlockActionResult Save( ValidPropertiesBox<StepProgramBag> box )
        {
            var entityService = new StepProgramService( RockContext );

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
            if ( !ValidateStepProgram( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            SaveAttributes( new StepType().TypeId, "StepProgramId", entity.Id.ToString(), box.Bag.StepProgramAttributes );

            entity = entityService.Get( entity.Id );

            if ( entity == null )
            {
                return ActionBadRequest( "This record is no longer valid, please reload your data." );
            }

            var currentPerson = GetCurrentPerson();

            if ( !entity.IsAuthorized( Authorization.VIEW, currentPerson ) )
            {
                entity.AllowPerson( Authorization.VIEW, currentPerson, RockContext );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                entity.AllowPerson( Authorization.EDIT, currentPerson, RockContext );
            }

            if ( !entity.IsAuthorized( Authorization.MANAGE_STEPS, currentPerson ) )
            {
                entity.AllowPerson( Authorization.MANAGE_STEPS, currentPerson, RockContext );
            }

            if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) )
            {
                entity.AllowPerson( Authorization.ADMINISTRATE, currentPerson, RockContext );
            }

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.StepProgramId] = entity.IdKey
                } ) );
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<StepProgramBag>
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
            var entityService = new StepProgramService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionBadRequest( "You are not authorized to delete this item." );
            }

            if ( entity.IsSystem )
            {
                return ActionBadRequest( "You cannot delete a system Step Program." );
            }

            string errorMessage = null;
            RockContext.WrapTransaction( () =>
            {
                var stepTypes = entity.StepTypes.ToList();
                var stepTypeService = new StepTypeService( RockContext );

                foreach ( var stepType in stepTypes )
                {
                    if ( !stepTypeService.CanDelete( stepType, out errorMessage ) )
                    {
                        return;
                    }

                    stepTypeService.Delete( stepType );
                }

                RockContext.SaveChanges();

                if ( !entityService.CanDelete( entity, out errorMessage ) )
                {
                    return;
                }

                entityService.Delete( entity );
                RockContext.SaveChanges();
            } );

            return string.IsNullOrWhiteSpace( errorMessage ) ? ActionOk( this.GetParentPageUrl() ) : ActionBadRequest( errorMessage );
        }

        /// <summary>
        /// Refresh the chart using the current filter settings.
        /// </summary>
        /// <param name="dateRange"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetChartData( DateTimeOffset startDateTime, DateTimeOffset endDateTime, StepProgramView selectedProgramView, StepChartMeasure selectedMeasure, string selectedStatusFilter )
        {
            var stepProgram = StepProgramCache.Get( PageParameter( PageParameterKey.StepProgramId ), !PageCache.Layout.Site.DisablePredictableIds );

            if ( stepProgram == null )
            {
                return ActionBadRequest( "Could not find the specified Step Program" );
            }

            DateTime startDate = startDateTime.ToOrganizationDateTime();
            DateTime endDate = endDateTime.ToOrganizationDateTime();
            double totalDays = ( endDate - startDate ).TotalDays;

            int timeUnitHelper;
            string timeUnit;
            ChartDataBag chartDataBag;

            // More than 3 years
            if ( totalDays > 365 * 3 )
            {
                // Group by Year
                timeUnitHelper = 10000;
                timeUnit = "year";
            }
            // More than 3 months
            else if ( totalDays > 90 )
            {
                // Group by Month
                timeUnitHelper = 100;
                timeUnit = "month";
            }
            else
            {
                // Group by Day
                timeUnitHelper = 1;
                timeUnit = "day";
            }

            if ( selectedMeasure == StepChartMeasure.ProgramCompletions )
            {
                chartDataBag = GetChartForStepProgramCompletions( stepProgram, timeUnitHelper, startDate, endDate, selectedProgramView );
            }
            else
            {
                chartDataBag = GetChartForSteps( stepProgram, timeUnitHelper, startDate, endDate, selectedMeasure, selectedStatusFilter, selectedProgramView );
            }

            if ( chartDataBag == null )
            {
                return ActionBadRequest( "An error occurred while fetching chart data" );
            }

            chartDataBag.TimeUnit = timeUnit;

            return ActionOk( chartDataBag );
        }

        [BlockAction]
        public BlockActionResult GetKPIData( string dateRange )
        {
            var stepProgram = StepProgramCache.Get( PageParameter( PageParameterKey.StepProgramId ), !PageCache.Layout.Site.DisablePredictableIds );

            if ( !stepProgram.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to view this Step Program." );
            }

            var kpi = GetKpi( dateRange, stepProgram.Id, out string errorMessage );
            if ( kpi.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( kpi );
        }

        [BlockAction]
        public BlockActionResult GetStepFlowData( SlidingDateRangeBag dateRange, int maxLevels, List<Guid> startingStepTypes )
        {
            List<StepTypeCache> stepTypes = StepProgramCache.Get( PageParameter( PageParameterKey.StepProgramId ), !PageCache.Layout.Site.DisablePredictableIds ).StepTypes;
            var nodeResults = new List<SankeyDiagramNodeBag>();
            List<int> startingStepTypeIds = new List<int>();
            int order = 0;

            foreach ( StepTypeCache stepType in stepTypes )
            {
                nodeResults.Add( new SankeyDiagramNodeBag
                {
                    Id = stepType.Id,
                    Order = ++order,
                    Name = stepType.Name,
                    Color = stepType.HighlightColor.IsNotNullOrWhiteSpace() ? stepType.HighlightColor : GetNextDefaultColor()
                } );

                if ( startingStepTypes.Contains( stepType.Guid ) )
                {
                    startingStepTypeIds.Add( stepType.Id );
                }
            }

            var parameters = GetStepFlowParameters( maxLevels, dateRange, startingStepTypeIds );
            var flowEdgeData = new DbService( new RockContext() ).GetDataTableFromSqlCommand( "spSteps_StepFlow", System.Data.CommandType.StoredProcedure, parameters );
            var flowEdgeResults = new List<SankeyDiagramEdgeBag>();

            foreach ( DataRow flowEdgeRow in flowEdgeData.Rows )
            {
                int level = flowEdgeRow["Level"].ToIntSafe();
                int units = flowEdgeRow["StepCount"].ToIntSafe();
                int sourceId = flowEdgeRow["SourceStepTypeId"].ToIntSafe();
                int targetId = flowEdgeRow["TargetStepTypeId"].ToIntSafe();

                var source = stepTypes.FirstOrDefault( stepType => stepType.Id == sourceId );
                var target = stepTypes.FirstOrDefault( stepType => stepType.Id == targetId );

                flowEdgeResults.Add( new SankeyDiagramEdgeBag
                {
                    TargetId = targetId,
                    SourceId = sourceId,
                    Level = level,
                    Units = units,
                    Tooltip = level > 1 ? BuildTooltip( source, target, units, flowEdgeRow["AvgNumberOfDaysBetweenSteps"].ToIntSafe() ) : ""
                } );
            }

            return ActionOk( new StepFlowGetDataBag
            {
                Edges = flowEdgeResults,
                Nodes = nodeResults
            } );
        }

        #endregion

        private class StepProgramCompletionProjection
        {
            public int? DateKey { get; set; }

            public int StepProgramId { get; set; }

            public int? CampusId { get; set; }

            public Guid CampusGuid { get; set; }

            public bool? CampusIsActive { get; set; }

            public int CampusOrder { get; set; }

            public string CampusName { get; set; }
        }

        private class StepProjection
        {
            public int? DateKey { get; set; }

            public int? CampusId { get; set; }

            public Guid CampusGuid { get; set; }

            public bool? CampusIsActive { get; set; }

            public int CampusOrder { get; set; }

            public string CampusName { get; set; }

            public int? AvgCampusAttendance { get; set; }

            public EngagementType? EngagementType { get; set; }

            public DefinedValue OrganizationObjective { get; set; }

            public int StepTypeId { get; set; }

            public string StepTypeName { get; set; }

            public int StepTypeOrder { get; set; }

            public int? ImpactWeight { get; set; }

            public string HighlightColor { get; set; }
        }
    }
}