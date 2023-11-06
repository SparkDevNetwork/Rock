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

using Rock.Attribute;
using Rock.Chart;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StepTypeDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular step type.
    /// </summary>

    [DisplayName( "Step Type Detail" )]
    [Category( "Steps" )]
    [Description( "Displays the details of the given Step Type for editing." )]
    [IconCssClass( "fa fa-question" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes
    [BooleanField
        ( "Show Chart",
          Key = AttributeKey.ShowChart,
          DefaultValue = "true",
          Order = 0 )]
    [DefinedValueField
        ( Rock.SystemGuid.DefinedType.CHART_STYLES,
         "Chart Style",
         Key = AttributeKey.ChartStyle,
         DefaultValue = Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK,
         Order = 1 )]
    [SlidingDateRangeField
        ( "Default Chart Date Range",
          Key = AttributeKey.SlidingDateRange,
          DefaultValue = "Current||Year||",
          EnabledSlidingDateRangeTypes = "Last,Previous,Current,DateRange",
          Order = 2 )]
    [CategoryField(
        "Data View Categories",
        Key = AttributeKey.DataViewCategories,
        Description = "The categories from which the Audience and Autocomplete data view options can be selected. If empty, all data views will be available.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.DataView",
        EntityTypeQualifierColumn = "",
        EntityTypeQualifierValue = "",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 7 )]

    [LinkedPage(
        name: "Bulk Entry Page",
        description: "The page to use for bulk entry of steps data",
        required: false,
        order: 8,
        key: AttributeKey.BulkEntryPage )]

    [CodeEditorField(
        "Key Performance Indicator Lava",
        IsRequired = false,
        DefaultValue = DefaultValue.KpiLava,
        Key = AttributeKey.KpiLava,
        EditorMode = CodeEditorMode.Lava,
        Description = "The Lava used to render the Key Performance Indicators bar. <span class='tip tip-lava'></span>",
        Order = 9 )]
    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "458b0a6c-73d6-456a-9a94-56b5ae3f0592" )]
    [Rock.SystemGuid.BlockTypeGuid( "487ecb63-bdf3-41a1-be67-c5faab5f27c1" )]
    public class StepTypeDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string StepTypeId = "StepTypeId";
            public const string StepProgramId = "ProgramId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string ShowChart = "ShowChart";
            public const string ChartStyle = "ChartStyle";
            public const string SlidingDateRange = "SlidingDateRange";
            public const string DataViewCategories = "DataViewCategories";
            public const string BulkEntryPage = "BulkEntryPage";
            public const string KpiLava = "KpiLava";
        }

        private static class DefaultValue
        {
            /// <summary>
            /// The kpi lava
            /// </summary>
            public const string KpiLava =
@"{[kpis style:'card' iconbackground:'true' columncount:'4']}
    [[ kpi icon:'fa-user' value:'{{IndividualsCompleting | Format:'N0'}}' label:'Individuals Completing' color:'blue-700']][[ endkpi ]]
    {% if StepType.HasEndDate %}
        [[ kpi icon:'fa-calendar' value:'{{AvgDaysToComplete | Format:'N0'}}' label:'Average Days to Complete' color:'green-600']][[ endkpi ]]
        [[ kpi icon:'fa-map-marker' value:'{{StepsStarted | Format:'N0'}}' label:'Steps Started' color:'#FF385C']][[ endkpi ]]
    {% endif %}
    [[ kpi icon:'fa-check-square' value:'{{StepsCompleted | Format:'N0'}}' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}";
        }

        #endregion Keys

        /// <summary>
        /// The rock context, should be accessed using the <see cref="GetRockContext"/> method since it performs a null check before
        /// creating a new instance.
        /// </summary>
        private RockContext _rockContext;

        /// <summary>
        /// The step type, should be accessed using the <see cref="GetStepType"/> since performs a null check on <see cref="_stepType"/>
        /// before assigning a value when possible.
        /// </summary>
        private StepType _stepType;

        /// <summary>
        /// The step statuses, should be accessed using the <see cref="GetStepStatuses"/> since performs a null check on <see cref="_stepStatuses"/>
        /// before assigning a value when possible.
        /// </summary>
        private List<StepStatus> _stepStatuses;

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = GetRockContext() )
            {
                var box = new DetailBlockBox<StepTypeBag, StepTypeDetailOptionsBag>();

                var stepProgramId = PageParameter( PageParameterKey.StepProgramId ).AsInteger();
                var stepTypeId = PageParameter( PageParameterKey.StepTypeId ).AsInteger();

                if ( stepProgramId == 0 && stepTypeId == 0 )
                {
                    box.ErrorMessage = "A new Step cannot be added because there is no Step Program available in this context.";
                }
                else
                {
                    SetBoxInitialEntityState( box, rockContext );

                    box.NavigationUrls = GetBoxNavigationUrls();
                    box.Options = GetBoxOptions( box.IsEditable, rockContext );
                    box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<StepType>();
                }

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
        private StepTypeDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new StepTypeDetailOptionsBag()
            {
                StepStatuses = GetStepStatuses().ToListItemBagList(),
                TriggerTypes = new List<ListItemBag>(),
            };

            options.TriggerTypes.Add( GetTriggerType( StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged ) );
            options.TriggerTypes.Add( GetTriggerType( StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete ) );
            options.TriggerTypes.Add( GetTriggerType( StepWorkflowTrigger.WorkflowTriggerCondition.Manual ) );

            return options;
        }

        /// <summary>
        /// Gets all the available step statuses.
        /// </summary>
        /// <returns></returns>
        private List<StepStatus> GetStepStatuses()
        {
            var stepProgramId = GetStepProgramId();
            return _stepStatuses ?? ( _stepStatuses = new StepStatusService( GetRockContext() ).Queryable().Where( s => s.StepProgramId == stepProgramId ).ToList() );
        }

        /// <summary>
        /// Validates the StepType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="stepType">The StepType to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the StepType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateStepType( StepType stepType, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<StepTypeBag, StepTypeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {StepType.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( StepType.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( StepType.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="StepTypeBag"/> that represents the entity.</returns>
        private StepTypeBag GetCommonEntityBag( StepType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new StepTypeBag
            {
                IdKey = entity.IdKey,
                AllowManualEditing = entity.AllowManualEditing,
                AllowMultiple = entity.AllowMultiple,
                AudienceDataView = entity.AudienceDataView.ToListItemBag(),
                AutoCompleteDataView = entity.AutoCompleteDataView.ToListItemBag(),
                CardLavaTemplate = entity.CardLavaTemplate,
                Description = entity.Description,
                HasEndDate = entity.HasEndDate,
                HighlightColor = entity.HighlightColor,
                IsActive = entity.IsActive,
                IsDateRequired = entity.Id == 0 || entity.IsDateRequired,
                Name = entity.Name,
                ShowCountOnBadge = entity.ShowCountOnBadge,
                IconCssClass = entity.IconCssClass,
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="StepTypeBag"/> that represents the entity.</returns>
        private StepTypeBag GetEntityBagForView( StepType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            var defaultDateRange = GetDefaultDateRange();

            bag.ShowChart = GetAttributeValue( AttributeKey.ShowChart ).AsBoolean();
            bag.Kpi = GetKpi( defaultDateRange );
            bag.DefaultDateRange = GetSlidingDateRangeBag( defaultDateRange );

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
        /// Returns the Configured SlidingDateRange Attribute value
        /// </summary>
        /// <returns></returns>
        private string GetDefaultDateRange()
        {
            return GetAttributeValue( AttributeKey.SlidingDateRange );
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
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="StepTypeBag"/> that represents the entity.</returns>
        private StepTypeBag GetEntityBagForEdit( StepType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            bag.PreRequisites = entity.StepTypePrerequisites.Select( p => p.PrerequisiteStepType.Guid.ToString() ).ToList();
            bag.AvailablePreRequisites = GetPrerequisiteStepsList();

            // Get the step type attributes for the grid in the edit view.
            var stepTypeAttributes = GetStepTypeAttributes( GetRockContext(), entity.Id.ToString() ).ConvertAll( e => new StepAttributeBag()
            {
                Attribute = PublicAttributeHelper.GetPublicEditableAttributeViewModel( e ),
                FieldType = FieldTypeCache.Get( e.FieldTypeId )?.Name,
            } );
            bag.StepAttributes = stepTypeAttributes;
            bag.StepTypeAttributesGridData = GetAttributesGridBuilder().Build( stepTypeAttributes );
            bag.StepTypeAttributesGridDefinition = GetAttributesGridBuilder().BuildDefinition();

            // Get the step type workflow triggers for the grid in the edit view.
            var workflowTriggers = entity.StepWorkflowTriggers.Select( wt => new StepTypeWorkflowTriggerBag()
            {
                IdKey = wt.IdKey,
                Guid = wt.Guid,
                WorkflowTrigger = GetTriggerType( wt.TriggerType ),
                WorkflowType = wt.WorkflowType.ToListItemBag(),
                PrimaryQualifier = GetStepStatuses().Find(ss => ss.Id == new StepWorkflowTrigger.StatusChangeTriggerSettings( wt.TypeQualifier ).FromStatusId )?.Guid.ToString(),
                SecondaryQualifier = GetStepStatuses().Find(ss => ss.Id == new StepWorkflowTrigger.StatusChangeTriggerSettings( wt.TypeQualifier ).ToStatusId )?.Guid.ToString(),
            } ).ToList();
            bag.Workflows = workflowTriggers;
            bag.WorkflowTriggerGridData = GetWorkflowTriggersGridBuilder().Build( workflowTriggers );
            bag.WorkflowTriggerGridDefinition = GetWorkflowTriggersGridBuilder().BuildDefinition();

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( StepType entity, DetailBlockBox<StepTypeBag, StepTypeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IconCssClass ),
                () => entity.IconCssClass = box.Entity.IconCssClass );

            box.IfValidProperty( nameof( box.Entity.HighlightColor ),
                () => entity.HighlightColor = box.Entity.HighlightColor );

            box.IfValidProperty( nameof( box.Entity.ShowCountOnBadge ),
                () => entity.ShowCountOnBadge = box.Entity.ShowCountOnBadge );

            box.IfValidProperty( nameof( box.Entity.HasEndDate ),
                () => entity.HasEndDate = box.Entity.HasEndDate );

            box.IfValidProperty( nameof( box.Entity.AllowMultiple ),
                () => entity.AllowMultiple = box.Entity.AllowMultiple );

            box.IfValidProperty( nameof( box.Entity.IsDateRequired ),
                () => entity.IsDateRequired = box.Entity.IsDateRequired );

            box.IfValidProperty( nameof( box.Entity.AllowManualEditing ),
                () => entity.AllowManualEditing = box.Entity.AllowManualEditing );

            box.IfValidProperty( nameof( box.Entity.AudienceDataView ),
                () => entity.AudienceDataViewId = box.Entity.AudienceDataView.GetEntityId<DataView>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.AutoCompleteDataView ),
                () => entity.AutoCompleteDataViewId = box.Entity.AutoCompleteDataView.GetEntityId<DataView>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.CardLavaTemplate ),
                () => entity.CardLavaTemplate = box.Entity.CardLavaTemplate );

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
        /// <returns>The <see cref="StepType"/> to be viewed or edited on the page.</returns>
        private StepType GetInitialEntity( RockContext rockContext )
        {
            return _stepType = GetInitialEntity<StepType, StepTypeService>( rockContext, PageParameterKey.StepTypeId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var stepType = GetStepType();
            var queryParams = new Dictionary<string, string>();

            if ( stepType != null )
            {
                queryParams[PageParameterKey.StepTypeId] = stepType.Id.ToString();
            }

            return new Dictionary<string, string>
            {
                [AttributeKey.BulkEntryPage] = this.GetLinkedPageUrl( AttributeKey.BulkEntryPage, queryParams ),
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
        private string GetSecurityGrantToken( StepType entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out StepType entity, out BlockActionResult error )
        {
            var entityService = new StepTypeService( rockContext );
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
                entity = new StepType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{StepType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${StepType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the StepType's attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="eventIdQualifierValue">The site identifier qualifier value.</param>
        /// <returns></returns>
        private static List<Model.Attribute> GetStepTypeAttributes( RockContext rockContext, string eventIdQualifierValue )
        {
            return new AttributeService( rockContext ).GetByEntityTypeId( new Step().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "StepTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( eventIdQualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
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
        /// Get the actual StepType model for deleting or editing.
        /// </summary>
        /// <returns></returns>
        private StepType GetStepType()
        {
            if ( _stepType == null )
            {
                var stepTypeId = PageParameter( PageParameterKey.StepTypeId ).AsIntegerOrNull();

                if ( stepTypeId > 0 )
                {
                    var stepTypeService = new StepTypeService( GetRockContext() );
                    _stepType = stepTypeService.Queryable( "StepProgram, StepTypePrerequisites" ).FirstOrDefault( stat => stat.Id == stepTypeId.Value );
                }
            }

            return _stepType;
        }

        /// <summary>
        /// Gets the step program identifier.
        /// </summary>
        /// <returns></returns>
        private int GetStepProgramId()
        {
            var stepType = GetStepType();

            int programId = 0;

            if ( stepType != null )
            {
                programId = stepType.StepProgramId;
            }

            if ( programId == 0 )
            {
                programId = RequestContext.GetPageParameter( PageParameterKey.StepProgramId ).AsInteger();
            }

            return programId;
        }

        /// <summary>
        /// Get the current step identifier.
        /// </summary>
        /// <returns></returns>
        private int GetStepTypeId()
        {
            return RequestContext.GetPageParameter( PageParameterKey.StepTypeId ).AsInteger();
        }

        /// <summary>
        /// Get the selection list for Prerequisite Steps.
        /// </summary>
        private List<ListItemBag> GetPrerequisiteStepsList()
        {
            var programId = GetStepProgramId();
            var stepTypeId = GetStepTypeId();
            var stepsService = new StepTypeService( GetRockContext() );
            List<StepType> prerequisiteStepTypes;

            if ( stepTypeId == 0 )
            {
                prerequisiteStepTypes = stepsService.Queryable().Where( x => x.StepProgramId == programId && x.IsActive ).ToList();
            }
            else
            {
                prerequisiteStepTypes = stepsService.GetEligiblePrerequisiteStepTypes( stepTypeId ).ToList();
            }

            return prerequisiteStepTypes.ConvertAll( p => new ListItemBag() { Text = p.Name, Value = p.Guid.ToString() } );
        }

        /// <summary>
        /// Gets the type of the trigger as a <see cref="ListItemBag"/>.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        private ListItemBag GetTriggerType( StepWorkflowTrigger.WorkflowTriggerCondition condition )
        {
            switch ( condition )
            {
                case StepWorkflowTrigger.WorkflowTriggerCondition.Manual:
                    return new ListItemBag() { Text = "Manual", Value = nameof( StepWorkflowTrigger.WorkflowTriggerCondition.Manual ) };
                case StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete:
                    return new ListItemBag() { Text = "Step Completed", Value = nameof( StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete ) };
                default:
                    return new ListItemBag() { Text = "Status Changed", Value = nameof( StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged ) };
            }
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
            var stepType = GetStepType();
            var template = GetAttributeValue( AttributeKey.KpiLava );

            if ( template.IsNullOrWhiteSpace() || stepType == null )
            {
                return string.Empty;
            }

            var startedQuery = GetStartedStepQuery( delimitedDateRange );
            var completedQuery = GetCompletedStepQuery( delimitedDateRange );

            var individualsCompleting = completedQuery.Select( s => s.PersonAlias.PersonId ).Distinct().Count();
            var stepsStarted = startedQuery.Count();
            var stepsCompleted = completedQuery.Count();

            var daysToCompleteList = completedQuery
#if REVIEW_NET5_0_OR_GREATER
                .Select( s => EF.Functions.DateDiffDay( s.StartDateTime, s.CompletedDateTime ) )
#else
                .Select( s => SqlFunctions.DateDiff( "DAY", s.StartDateTime, s.CompletedDateTime ) )
#endif
                .Where( i => i.HasValue )
                .Select( i => i.Value )
                .ToList();

            var avgDaysToComplete = daysToCompleteList.Any() ? ( int ) daysToCompleteList.Average() : 0;

            return template.ResolveMergeFields( new Dictionary<string, object>
            {
                { "IndividualsCompleting", individualsCompleting },
                { "AvgDaysToComplete", avgDaysToComplete },
                { "StepsStarted", stepsStarted },
                { "StepsCompleted", stepsCompleted },
                { "StepType", stepType }
            } );
        }

        /// <summary>
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetCompletedStepQuery( string delimitedDateRange )
        {
            var dataContext = GetRockContext();
            var stepService = new StepService( dataContext );
            var stepTypeId = GetStepTypeId();

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    x.StepTypeId == stepTypeId &&
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
        /// Gets the step started query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetStartedStepQuery( string delimitedDateRange )
        {
            var dataContext = GetRockContext();
            var stepService = new StepService( dataContext );
            var stepTypeId = GetStepTypeId();

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    x.StepTypeId == stepTypeId &&
                    x.StepType.IsActive &&
                    x.StartDateKey != null );

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
            var stepTypeId = GetStepTypeId();

            if ( showActivitySummary )
            {
                // If the Step Type does not have any activity, hide the Activity Summary.
                var dataContext = GetRockContext();
                var stepService = new StepService( dataContext );
                var stepsQuery = stepService.Queryable().AsNoTracking()
                                    .Where( x => x.StepTypeId == stepTypeId );
                showActivitySummary = stepsQuery.Any();
            }

            return  showActivitySummary;
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

            if ( startDate.HasValue )
            {
                startDate = startDate.Value.Date;
            }

            // Initialize a new Chart Factory.
            var factory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>();

            if ( reportPeriod.TimeUnit == TimePeriodUnitSpecifier.Year )
            {
                factory.TimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Month;
            }
            else
            {
                factory.TimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Day;
            }

            factory.StartDateTime = startDate;
            factory.EndDateTime = endDate;
            factory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Line;

            // Determine the appropriate date grouping for the chart data points.
            Func<int, int> groupKeySelector;
            var groupByDay = factory.TimeScale == ChartJsTimeSeriesTimeScaleSpecifier.Day;

            if ( groupByDay )
            {
                // Group Steps by Start Date.
                groupKeySelector = x => x;
            }
            else
            {
                // Group Steps by Start Date rounded to beginning of the month.
                groupKeySelector = x => x / 100;
            }

            // Add data series for Steps started.
            var startedSeriesDataPoints = GetStartedStepQuery( delimitedDateRange )
                .Select( x => x.StartDateKey.Value )
                .ToList()
                .GroupBy( groupKeySelector )
                .Select( x => new ChartDatasetInfo
                {
                    DatasetName = "Started",
                    DateTime = groupByDay ? x.Key.GetDateKeyDate() : ( ( x.Key * 100 ) + 1 ).GetDateKeyDate(), // Adding +1 to get the first day of month.
                    Value = x.Count(),
                    SortKey = "1"
                } );

            // Add data series for Steps completed.
            var completedSeriesDataPoints = GetCompletedStepQuery( delimitedDateRange )
                .Select( x => x.CompletedDateKey.Value )
                .ToList()
                .GroupBy( groupKeySelector )
                .Select( x => new ChartDatasetInfo
                {
                    DatasetName = "Completed",
                    DateTime = groupByDay ? x.Key.GetDateKeyDate() : ( ( x.Key * 100 ) + 1 ).GetDateKeyDate(), // Adding +1 to get the first day of month.
                    Value = x.Count(),
                    SortKey = "2"
                } );

            var allDataPoints = startedSeriesDataPoints.Union( completedSeriesDataPoints ).OrderBy( x => x.SortKey ).ThenBy( x => x.DateTime );

            // Add Dataset for Steps Started.
            var colorStarted = new RockColor( ChartJsConstants.Colors.Blue );
            var startedDataset = this.CreateDataSet( allDataPoints, "Started", colorStarted.ToHex() );

            factory.Datasets.Add( startedDataset );

            // Add Dataset for Steps Completed.
            var colorCompleted = new RockColor( ChartJsConstants.Colors.Green );
            var completedDataset = this.CreateDataSet( allDataPoints, "Completed", colorCompleted.ToHex() );

            factory.Datasets.Add( completedDataset );

            return factory;
        }

        private ChartJsTimeSeriesDataset CreateDataSet( IOrderedEnumerable<ChartDatasetInfo> allDataPoints, string datasetName, string colorString )
        {
            var dataset = new ChartJsTimeSeriesDataset();
            dataset.Name = datasetName;
            dataset.DataPoints = allDataPoints
                                    .Where( x => x.DatasetName == datasetName )
                                    .Select( x => new ChartJsTimeSeriesDataPoint { DateTime = x.DateTime, Value = x.Value } )
                                    .Cast<IChartJsTimeSeriesDataPoint>()
                                    .ToList();
            dataset.BorderColor = colorString;

            return dataset;
        }

        /// <summary>
        /// Gets the attributes grid builder.
        /// </summary>
        /// <returns></returns>
        private GridBuilder<StepAttributeBag> GetAttributesGridBuilder()
        {
            return new GridBuilder<StepAttributeBag>()
                .AddTextField( "idKey", a => a.Attribute.Key )
                .AddTextField( "attributeName", a => a.Attribute.Name )
                .AddTextField( "fieldType", a => a.FieldType )
                .AddField( "allowSearch", a => a.Attribute.IsAllowSearch );
        }

        /// <summary>
        /// Gets the workflow triggers grid builder.
        /// </summary>
        /// <returns></returns>
        private GridBuilder<StepTypeWorkflowTriggerBag> GetWorkflowTriggersGridBuilder()
        {
            return new GridBuilder<StepTypeWorkflowTriggerBag>()
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "workflowType", a => a.WorkflowType.Text )
                .AddTextField( "workflowTrigger", a => a.WorkflowTrigger.Text );
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

                var box = new DetailBlockBox<StepTypeBag, StepTypeDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<StepTypeBag, StepTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new StepTypeService( rockContext );
                var stepWorkflowService = new StepWorkflowService( rockContext );
                var stepWorkflowTriggerService = new StepWorkflowTriggerService( rockContext );

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
                if ( !ValidateStepType( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                // Workflow Triggers: Remove deleted triggers.
                var uiWorkflows = box.Entity.Workflows.Select( l => l.Guid );
                var deletedTriggers = entity.StepWorkflowTriggers.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList();

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
                foreach ( var stepTypeWorkflowTrigger in box.Entity.Workflows )
                {
                    var workflowTrigger = entity.StepWorkflowTriggers.FirstOrDefault( a => a.Guid == stepTypeWorkflowTrigger.Guid );

                    if ( workflowTrigger == null )
                    {
                        workflowTrigger = new StepWorkflowTrigger
                        {
                            StepProgramId = entity.StepProgramId,
                            Guid = Guid.NewGuid()
                        };
                        entity.StepWorkflowTriggers.Add( workflowTrigger );
                    }

                    var qualifierSettings = new StepWorkflowTrigger.StatusChangeTriggerSettings
                    {
                        FromStatusId = GetStepStatuses().Find( ss => ss.Guid.ToString() == stepTypeWorkflowTrigger.PrimaryQualifier )?.Id,
                        ToStatusId = GetStepStatuses().Find( ss => ss.Guid.ToString() == stepTypeWorkflowTrigger.SecondaryQualifier )?.Id,
                    };

                    workflowTrigger.WorkflowTypeId = stepTypeWorkflowTrigger.WorkflowType.GetEntityId<WorkflowType>( rockContext ).Value;
                    workflowTrigger.TriggerType = stepTypeWorkflowTrigger.WorkflowTrigger.Value.ConvertToEnum<StepWorkflowTrigger.WorkflowTriggerCondition>();
                    workflowTrigger.TypeQualifier = qualifierSettings.ToSelectionString();
                    workflowTrigger.WorkflowName = stepTypeWorkflowTrigger.WorkflowType.Text;
                }

                // Update Prerequisites
                var stepProgramId = GetStepProgramId();
                var stepTypeId = GetStepTypeId();
                var uiPrerequisiteStepTypeGuids = box.Entity.PreRequisites.ConvertAll( x => x.AsGuid() );
                var stepTypes = entityService.Queryable().Where( x => x.StepProgramId == stepProgramId && x.IsActive ).ToList();
                var removePrerequisiteStepTypes = entity.StepTypePrerequisites.Where( x => !uiPrerequisiteStepTypeGuids.Contains( x.PrerequisiteStepType.Guid ) ).ToList();
                var prerequisiteService = new StepTypePrerequisiteService( rockContext );

                foreach ( var prerequisiteStepType in removePrerequisiteStepTypes )
                {
                    entity.StepTypePrerequisites.Remove( prerequisiteStepType );
                    prerequisiteService.Delete( prerequisiteStepType );
                }

                var existingPrerequisiteStepTypeIds = entity.StepTypePrerequisites.Select( x => x.PrerequisiteStepTypeId ).ToList();
                var addPrerequisiteStepTypeIds = stepTypes.Where( x => uiPrerequisiteStepTypeGuids.Contains( x.Guid )
                                                                     && !existingPrerequisiteStepTypeIds.Contains( x.Id ) )
                                                          .Select( x => x.Id )
                                                          .ToList();

                foreach ( var prerequisiteStepTypeId in addPrerequisiteStepTypeIds )
                {
                    var newPrerequisite = new StepTypePrerequisite();
                    newPrerequisite.StepTypeId = entity.Id;
                    newPrerequisite.PrerequisiteStepTypeId = prerequisiteStepTypeId;
                    entity.StepTypePrerequisites.Add( newPrerequisite );
                }

                // Validate Prerequisites.
                // This is necessary because other Step Types may have been modified after this record edit was started.
                var isValid = true;
                var sb = new StringBuilder();
                if ( stepTypeId > 0 )
                {
                    var eligibleStepTypeIdList = entityService.GetEligiblePrerequisiteStepTypes( stepTypeId ).Select( x => x.Id ).ToList();

                    foreach ( var prerequisiteId in entity.StepTypePrerequisites.Select( p => p.PrerequisiteStepTypeId ) )
                    {
                        if ( !eligibleStepTypeIdList.Contains( prerequisiteId ) )
                        {
                            var prerequisiteStepType = entityService.Get( prerequisiteId );
                            isValid = false;
                            sb.Append( "This Step Type cannot have prerequisite \"" ).Append( prerequisiteStepType.Name ).AppendLine( "\" because it is already a prerequisite of that Step Type." );
                        }
                    }
                }

                /* Save Event Attributes */
                var stepAttributes = box.Entity.StepAttributes.ConvertAll( e => e.Attribute );
                SaveAttributes( new Step().TypeId, "StepTypeId", entity.Id.ToString(), stepAttributes, rockContext );

                if ( !isValid )
                {
                    return ActionBadRequest( sb.ToString() );
                }

                if ( isNew )
                {
                    // If there are any other step types, either:
                    // Find out the maximum Order value for the steps, and set this new Step's Order value one higher than that.
                    // If there are NOT any other step Types, set Order as 0.
                    entity.Order = stepTypes.Any() ? stepTypes.Max( st => st.Order ) + 1 : 0;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.StepTypeId] = entity.IdKey
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
                var entityService = new StepTypeService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<StepTypeBag, StepTypeDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<StepTypeBag, StepTypeDetailOptionsBag>
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
        /// Gets the attribute.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetAttribute( Guid? attributeGuid )
        {
            PublicEditableAttributeBag editableAttribute;
            string modalTitle;
            var rockContext = new RockContext();

            var entity = GetInitialEntity( rockContext );
            var eventIdQualifierValue = entity.Id.ToString();
            var attributes = GetStepTypeAttributes( rockContext, eventIdQualifierValue );
            var isNew = entity.Id == 0;

            if ( !attributeGuid.HasValue )
            {
                editableAttribute = new PublicEditableAttributeBag
                {
                    FieldTypeGuid = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Guid
                };
                modalTitle = ActionTitle.Add(  isNew ? "new Attribute for Participants in this Step Type." : $"new Attribute for Participants in {entity.Name}." );
            }
            else
            {
                var attribute = attributes.Find( a => a.Guid == attributeGuid );
                editableAttribute = PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute );
                modalTitle = ActionTitle.Edit( $"Attribute for Participants in Step Type \"{entity.Name}\"." );
            }

            var reservedKeyNames = new List<string>();
            attributes.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );

            return ActionOk( new { editableAttribute, reservedKeyNames, modalTitle } );
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

            return ActionOk( new StepTypeBag() { ChartData = chartDataJson, Kpi = kpi, ShowChart = showActivitySummary } );
        }

        #endregion

        /// <summary>
        /// Stores information about a dataset to be displayed on a chart.
        /// </summary>
        private sealed class ChartDatasetInfo
        {
            public string DatasetName { get; set; }

            public DateTime DateTime { get; set; }

            public int Value { get; set; }

            public string SortKey { get; set; }
        }
    }
}
