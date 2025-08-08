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
using Rock.Web;
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
    [IconCssClass( "ti ti-question-mark" )]
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
    public class StepTypeDetail : RockEntityDetailBlockType<StepType, StepTypeBag>, IBreadCrumbBlock
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
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.StepTypeId );
            var pageParameters = new Dictionary<string, string>();

            var name = new StepTypeService( RockContext )
                .GetSelect( key, mf => mf.Name );

            if ( name != null )
            {
                pageParameters.Add( PageParameterKey.StepTypeId, key );
            }

            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( name ?? "New Step Type", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
            };
        }
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<StepTypeBag, StepTypeDetailOptionsBag>();

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
        private StepTypeDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var stepType = GetStepType();
            var options = new StepTypeDetailOptionsBag()
            {
                StepStatuses = GetStepStatuses( stepType ).ToListItemBagList(),
                TriggerTypes = new List<ListItemBag>
                {
                    new ListItemBag() { Text = "Step Completed", Value = StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete.ToString() },
                    new ListItemBag() { Text = "Status Changed", Value = StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged.ToString() },
                    new ListItemBag() { Text = "Manual", Value = StepWorkflowTrigger.WorkflowTriggerCondition.Manual.ToString() }
                },
                StepPrograms = GetStepPrograms( stepType?.StepProgramId )
            };

            return options;
        }

        /// <summary>
        /// Gets all the available step statuses.
        /// </summary>
        /// <returns></returns>
        private List<StepStatus> GetStepStatuses( StepType stepType )
        {
            var stepProgramId = GetStepProgramId( stepType );
            return _stepStatuses ?? ( _stepStatuses = new StepStatusService( RockContext ).Queryable().Where( s => s.StepProgramId == stepProgramId ).ToList() );
        }

        /// <summary>
        /// Gets all active Step Programs that the current person has view permissions for.
        /// </summary>
        /// <returns>Step Programs in a list of list item bags</returns>
        private List<StepProgramBag> GetStepPrograms( int? currentStepProgramId )
        {
            if ( !currentStepProgramId.HasValue )
            {
                return null;
            }

            var stepProgramService = new StepProgramService( RockContext );

            var authorizedStepPrograms = stepProgramService.Queryable()
                .AsNoTracking()
                .Where( s => s.IsActive && s.Id != currentStepProgramId )
                .Select( s => new
                {
                    StepProgram = s,
                    StepStatuses = s.StepStatuses
                        .Where( ss => ss.IsActive )
                        .Select( ss => new ListItemBag {
                            Text = ss.Name,
                            Value = ss.Guid.ToString()
                        } )
                        .ToList()
                } )
                .ToList()
                .Where( sp => sp.StepProgram.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .Select( sp => new StepProgramBag
                {
                    StepProgram = new ListItemBag
                    {
                        Text = sp.StepProgram.Name,
                        Value = sp.StepProgram.Guid.ToString()
                    },
                    StepStatuses = sp.StepStatuses
                } )
                .ToList();

            return authorizedStepPrograms;
        }

        /// <summary>
        /// Validates the StepType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="stepType">The StepType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the StepType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateStepType( StepType stepType, out string errorMessage )
        {
            errorMessage = null;

            if ( !stepType.IsValid )
            {
                errorMessage = stepType.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<StepTypeBag, StepTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {StepType.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( StepType.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( StepType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
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
                IsDeletable = !entity.IsSystem
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="StepTypeBag"/> that represents the entity.</returns>
        protected override StepTypeBag GetEntityBagForView( StepType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            var defaultDateRange = GetDefaultDateRange();

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

            bag.ShowChart = GetAttributeValue( AttributeKey.ShowChart ).AsBoolean();

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
        protected override StepTypeBag GetEntityBagForEdit( StepType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            bag.PreRequisites = entity.StepTypePrerequisites.Select( p => p.PrerequisiteStepType.Guid.ToString() ).ToList();
            bag.AvailablePreRequisites = GetPrerequisiteStepsList( entity );

            // Get the step type attributes for the grid in the edit view.
            var stepTypeAttributes = GetStepTypeAttributes( entity.Id.ToString() ).ConvertAll( e => new StepAttributeBag()
            {
                Attribute = PublicAttributeHelper.GetPublicEditableAttribute( e ),
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
                WorkflowTrigger = GetTriggerType( wt.TriggerType, wt.TypeQualifier ),
                WorkflowType = wt.WorkflowType.ToListItemBag(),
                PrimaryQualifier = GetStepStatuses( entity ).Find( ss => ss.Id == new StepWorkflowTrigger.StatusChangeTriggerSettings( wt.TypeQualifier ).FromStatusId )?.Guid.ToString(),
                SecondaryQualifier = GetStepStatuses( entity ).Find( ss => ss.Id == new StepWorkflowTrigger.StatusChangeTriggerSettings( wt.TypeQualifier ).ToStatusId )?.Guid.ToString(),
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
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        protected override bool UpdateEntityFromBox( StepType entity, ValidPropertiesBox<StepTypeBag> box )
        {
            if ( box?.Bag == null || box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.HighlightColor ),
                () => entity.HighlightColor = box.Bag.HighlightColor );

            box.IfValidProperty( nameof( box.Bag.ShowCountOnBadge ),
                () => entity.ShowCountOnBadge = box.Bag.ShowCountOnBadge );

            box.IfValidProperty( nameof( box.Bag.HasEndDate ),
                () => entity.HasEndDate = box.Bag.HasEndDate );

            box.IfValidProperty( nameof( box.Bag.AllowMultiple ),
                () => entity.AllowMultiple = box.Bag.AllowMultiple );

            box.IfValidProperty( nameof( box.Bag.IsDateRequired ),
                () => entity.IsDateRequired = box.Bag.IsDateRequired );

            box.IfValidProperty( nameof( box.Bag.AllowManualEditing ),
                () => entity.AllowManualEditing = box.Bag.AllowManualEditing );

            box.IfValidProperty( nameof( box.Bag.AudienceDataView ),
                () => entity.AudienceDataViewId = box.Bag.AudienceDataView.GetEntityId<DataView>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AutoCompleteDataView ),
                () => entity.AutoCompleteDataViewId = box.Bag.AutoCompleteDataView.GetEntityId<DataView>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CardLavaTemplate ),
                () => entity.CardLavaTemplate = box.Bag.CardLavaTemplate );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <returns>The <see cref="StepType"/> to be viewed or edited on the page.</returns>
        protected override StepType GetInitialEntity()
        {
            var entity = GetInitialEntity<StepType, StepTypeService>( RockContext, PageParameterKey.StepTypeId );

            if ( entity?.Id == 0 )
            {
                entity.StepProgramId = GetStepProgramId( entity );
            }

            return entity;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var stepTypeId = PageParameter( PageParameterKey.StepTypeId );
            var stepProgramId = PageParameter( PageParameterKey.StepProgramId );
            var linkedPageQueryParams = new Dictionary<string, string>();
            var parentPageQueryParams = new Dictionary<string, string>();

            if ( !string.IsNullOrWhiteSpace( stepTypeId ) )
            {
                linkedPageQueryParams[PageParameterKey.StepTypeId] = stepTypeId;
            }

            if ( !string.IsNullOrWhiteSpace( stepProgramId ) )
            {
                parentPageQueryParams[PageParameterKey.StepProgramId] = stepProgramId;
            }

            return new Dictionary<string, string>
            {
                [AttributeKey.BulkEntryPage] = this.GetLinkedPageUrl( AttributeKey.BulkEntryPage, linkedPageQueryParams ),
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( parentPageQueryParams )
            };
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        protected override bool TryGetEntityForEditAction( string idKey, out StepType entity, out BlockActionResult error )
        {
            var entityService = new StepTypeService( RockContext );
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
                entity.StepProgramId = GetStepProgramId( entity );
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
        /// <param name="eventIdQualifierValue">The site identifier qualifier value.</param>
        /// <returns></returns>
        private List<Model.Attribute> GetStepTypeAttributes( string eventIdQualifierValue )
        {
            return new AttributeService( RockContext ).GetByEntityTypeId( new Step().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "StepTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( eventIdQualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Get the actual StepType model for deleting or editing.
        /// </summary>
        /// <returns></returns>
        private StepType GetStepType()
        {
            if ( _stepType == null )
            {
                var stepTypeId = GetStepTypeId();

                if ( stepTypeId > 0 )
                {
                    var stepTypeService = new StepTypeService( RockContext );
                    _stepType = stepTypeService.Queryable( "StepProgram, StepTypePrerequisites" ).FirstOrDefault( stat => stat.Id == stepTypeId );
                }
            }

            return _stepType;
        }

        /// <summary>
        /// Gets the step program identifier.
        /// </summary>
        /// <returns></returns>
        private int GetStepProgramId( StepType stepType )
        {
            if ( stepType != null && stepType.StepProgramId > 0 )
            {
                return stepType.StepProgramId;
            }

            var programIdParam = PageParameter( PageParameterKey.StepProgramId );
            if ( !string.IsNullOrWhiteSpace( programIdParam ) )
            {
                var programId = programIdParam.AsIntegerOrNull();
                if ( programId.HasValue )
                {
                    return programId.Value;
                }

                var programCache = StepProgramCache.Get( programIdParam, !PageCache.Layout.Site.DisablePredictableIds );
                if ( programCache != null )
                {
                    return programCache.Id;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get the current step identifier.
        /// </summary>
        /// <returns></returns>
        private int GetStepTypeId()
        {
            return StepTypeCache.Get( PageParameter( PageParameterKey.StepTypeId ), !PageCache.Layout.Site.DisablePredictableIds )?.Id ?? 0;
        }

        /// <summary>
        /// Get the selection list for Prerequisite Steps.
        /// </summary>
        private List<ListItemBag> GetPrerequisiteStepsList( StepType stepType )
        {
            var programId = GetStepProgramId( stepType );
            var stepTypeId = GetStepTypeId();
            var stepsService = new StepTypeService( RockContext );
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
            var attributesToDelete = attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) );

            foreach ( var attr in attributesToDelete )
            {
                attributeService.Delete( attr );
            }

            if ( attributesToDelete.Any() )
            {
                RockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, RockContext );
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

            var stepsStarted = startedQuery.Count();
            var stepsCompleted = completedQuery.Count();
            var individualsCompleting = completedQuery.Select( s => s.PersonAlias.PersonId ).Distinct().Count();

            var avgDaysToComplete = 0;
            var avgQuery = completedQuery
                .Where( s => s.StartDateTime != null && s.CompletedDateTime != null )
                .Select( s => SqlFunctions.DateDiff( "DAY", s.StartDateTime, s.CompletedDateTime ) )
                .Where( diff => diff.HasValue )
                .Select( diff => diff.Value );

            if ( avgQuery.Any() )
            {
                avgDaysToComplete = ( int ) avgQuery.Average();
            }

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
            var stepService = new StepService( RockContext );
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
            var stepService = new StepService( RockContext );
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
                var stepService = new StepService( RockContext );
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
            var startDate = dateRange.Start?.Date;
            var endDate = dateRange.End;

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
                .AddTextField( "idKey", a => a.Attribute.Guid.ToString() )
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<StepTypeBag>
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
        public BlockActionResult Save( ValidPropertiesBox<StepTypeBag> box )
        {
            var entityService = new StepTypeService( RockContext );
            var stepWorkflowService = new StepWorkflowService( RockContext );
            var stepWorkflowTriggerService = new StepWorkflowTriggerService( RockContext );

            if ( box?.Bag == null )
            {
                return ActionBadRequest( "Invalid data. The entity bag is null." );
            }

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
            if ( !ValidateStepType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            // Workflow Triggers: Remove deleted triggers.
            var uiWorkflows = box.Bag.Workflows.Select( l => l.Guid );
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
            foreach ( var stepTypeWorkflowTrigger in box.Bag.Workflows )
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
                    FromStatusId = GetStepStatuses( entity ).Find( ss => ss.Guid.ToString() == stepTypeWorkflowTrigger.PrimaryQualifier )?.Id,
                    ToStatusId = GetStepStatuses( entity ).Find( ss => ss.Guid.ToString() == stepTypeWorkflowTrigger.SecondaryQualifier )?.Id,
                };

                workflowTrigger.WorkflowTypeId = stepTypeWorkflowTrigger.WorkflowType.GetEntityId<WorkflowType>( RockContext ).Value;
                workflowTrigger.TriggerType = stepTypeWorkflowTrigger.WorkflowTrigger.Value.ConvertToEnum<StepWorkflowTrigger.WorkflowTriggerCondition>();
                workflowTrigger.TypeQualifier = qualifierSettings.ToSelectionString();
                workflowTrigger.WorkflowName = stepTypeWorkflowTrigger.WorkflowType.Text;
            }

            // Update Prerequisites
            var stepProgramId = GetStepProgramId( entity );
            var stepTypeId = GetStepTypeId();
            var uiPrerequisiteStepTypeGuids = box.Bag.PreRequisites.ConvertAll( x => x.AsGuid() );
            var stepTypes = entityService.Queryable().Where( x => x.StepProgramId == stepProgramId && x.IsActive ).ToList();
            var removePrerequisiteStepTypes = entity.StepTypePrerequisites.Where( x => !uiPrerequisiteStepTypeGuids.Contains( x.PrerequisiteStepType.Guid ) ).ToList();
            var prerequisiteService = new StepTypePrerequisiteService( RockContext );

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

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            /* Save Attributes */
            var stepAttributes = box.Bag.StepAttributes.ConvertAll( e => e.Attribute );
            SaveAttributes( new Step().TypeId, "StepTypeId", entity.Id.ToString(), stepAttributes );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.StepTypeId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<StepTypeBag>
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
            var entityService = new StepTypeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            if ( entity.IsSystem )
            {
                return ActionBadRequest( "You cannot delete a System Step Type." );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
            {
                {  PageParameterKey.StepProgramId, PageParameter( PageParameterKey.StepProgramId ) },
            } ) );
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

            var entity = GetInitialEntity();
            var eventIdQualifierValue = entity.Id.ToString();
            var attributes = GetStepTypeAttributes( eventIdQualifierValue );
            var isNew = entity.Id == 0;

            if ( !attributeGuid.HasValue )
            {
                editableAttribute = new PublicEditableAttributeBag
                {
                    FieldTypeGuid = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Guid
                };
                modalTitle = ActionTitle.Add( isNew ? "new Attribute for Participants in this Step Type." : $"new Attribute for Participants in {entity.Name}." );
            }
            else
            {
                var attribute = attributes.Find( a => a.Guid == attributeGuid );
                editableAttribute = PublicAttributeHelper.GetPublicEditableAttribute( attribute );
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

        /// <summary>
        /// Transfers the Step Type to a different Step Program
        /// </summary>
        /// <param name="transferBag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult TransferStepType( StepTypeTransferBag transferBag )
        {
            if ( !TryGetEntityForEditAction( transferBag.StepTypeIdKey, out var stepType, out var actionError ) )
            {
                return actionError;
            }

            if ( !transferBag.TargetStepProgramGuid.HasValue )
            {
                return ActionBadRequest( "Target Step Program is required." );
            }

            var targetStepProgram = StepProgramCache.Get( transferBag.TargetStepProgramGuid.Value );

            if ( targetStepProgram == null )
            {
                return ActionBadRequest( "Target Step Program not found." );
            }

            // Fetch all Step Statuses from the current Step Program
            // This query pulls every status (Guid, Id, Name) that belongs to
            // the current Step Program associated with the Step Type.
            // We'll need both the Guid (for matching mappings) and the Id 
            // (for updating Step records later).
            var currentStatuses = new StepStatusService( RockContext )
                .Queryable()
                .Where( ss => ss.StepProgramId == stepType.StepProgramId )
                .Select( ss => new { ss.Guid, ss.Id, ss.Name } )
                .ToList();

            // Fetch all Step Statuses from the target Step Program
            // We load the target program's statuses into a dictionary keyed
            // by Guid (for fast lookups when validating mappings). The value
            // is the database Id so we can assign it to Steps.
            var targetStatuses = new StepStatusService( RockContext )
                .Queryable()
                .Where( ss => ss.StepProgramId == targetStepProgram.Id )
                .ToDictionary( ss => ss.Guid, ss => ss.Id );

            // Validate that every current status has a valid mapping
            // We loop over each status from the current program and check:
            //   - Does it have an entry in transferBag.StepStatusMappings?
            //   - Does the mapped Guid exist in the target program's statuses?
            // If either check fails, we return a descriptive error message.
            foreach ( var status in currentStatuses )
            {
                if ( !transferBag.StepStatusMappings.TryGetValue( status.Guid.ToString(), out var mappedGuidString ) )
                {
                    return ActionBadRequest( $"A mapping for status '{status.Name}' is required." );
                }

                var mappedGuid = mappedGuidString.AsGuid();
                if ( !targetStatuses.ContainsKey( mappedGuid ) )
                {
                    return ActionBadRequest( $"The mapped status for '{status.Name}' is not valid in the target program." );
                }
            }

            var stepsToUpdate = new StepService( RockContext )
                .Queryable()
                .Where( s => s.StepTypeId == stepType.Id )
                .ToList();

            // Update each Step's StepStatusId to match the target program mapping
            foreach ( var step in stepsToUpdate )
            {
                var oldStatusGuid = currentStatuses
                    .FirstOrDefault( cs => cs.Id == step.StepStatusId )?.Guid;

                if ( oldStatusGuid.HasValue
                     && transferBag.StepStatusMappings.TryGetValue( oldStatusGuid.Value.ToString(), out var mappedGuidString ) )
                {
                    var newStatusGuid = mappedGuidString.AsGuid();
                    step.StepStatusId = targetStatuses[newStatusGuid];
                }
            }

            stepType.StepProgramId = targetStepProgram.Id;

            RockContext.SaveChanges();

            return ActionOk( targetStepProgram.IdKey );
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
