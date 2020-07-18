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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// A DataView Filter that selects people based on their participation in one or more Steps.
    /// </summary>
    [Description( "Filter people on whether they have participated in one or more Steps" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Has Taken Step" )]
    public class StepsTakenFilter : DataFilterComponent
    {
        #region Settings

        /// <summary>
        /// Settings for the Data Filter component.
        /// </summary>
        public class FilterSettings : SettingsStringBase
        {
            /// <summary>
            /// The step program unique identifier
            /// </summary>
            public Guid? StepProgramGuid;

            /// <summary>
            /// The step type guids
            /// </summary>
            public List<Guid> StepTypeGuids = new List<Guid>();

            /// <summary>
            /// The step status guids
            /// </summary>
            public List<Guid> StepStatusGuids = new List<Guid>();

            /// <summary>
            /// The started in period
            /// </summary>
            public TimePeriod StartedInPeriod = new TimePeriod();

            /// <summary>
            /// The completed in period
            /// </summary>
            public TimePeriod CompletedInPeriod = new TimePeriod();

            /// <summary>
            /// The step campus guids
            /// </summary>
            public List<Guid> StepCampusGuids = new List<Guid>();

            /// <summary>
            /// Initializes a new instance of the <see cref="FilterSettings"/> class.
            /// </summary>
            public FilterSettings()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FilterSettings"/> class.
            /// </summary>
            /// <param name="settingsString">The settings string.</param>
            public FilterSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            /// <summary>
            /// Indicates if the current settings are valid.
            /// </summary>
            /// <value>
            /// True if the settings are valid.
            /// </value>
            public override bool IsValid
            {
                get { return StepProgramGuid.HasValue; }
            }

            /// <summary>
            /// Set the property values parsed from a settings string.
            /// </summary>
            /// <param name="version">The version number of the parameter set.</param>
            /// <param name="parameters">An ordered collection of strings representing the parameter values.</param>
            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                StepProgramGuid = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 0 ).AsGuidOrNull();

                StepTypeGuids = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 1 ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsGuid() ).ToList();

                StepStatusGuids = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 2 ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsGuid() ).ToList();

                var periodStartSettings = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 3 );

                StartedInPeriod = new TimePeriod( periodStartSettings, "," );

                var periodCompletedSettings = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 4 );

                CompletedInPeriod = new TimePeriod( periodCompletedSettings, "," );

                StepCampusGuids = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 5 ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsGuid() ).ToList();
            }

            /// <summary>
            /// Gets an ordered set of property values that can be used to construct the
            /// settings string.
            /// </summary>
            /// <returns>
            /// An ordered collection of strings representing the parameter values.
            /// </returns>
            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( StepProgramGuid.ToStringSafe() );
                settings.Add( StepTypeGuids.AsDelimited( "," ) );
                settings.Add( StepStatusGuids.AsDelimited( "," ) );
                settings.Add( StartedInPeriod.ToDelimitedString( "," ) );
                settings.Add( CompletedInPeriod.ToDelimitedString( "," ) );
                settings.Add( StepCampusGuids.AsDelimited( "," ) );

                return settings;
            }
        }

        #endregion

        #region Private Declarations

        private SingleEntityPicker<StepProgram> _stepProgramPicker = null;
        private RockCheckBoxList _cblStepType = null;
        private RockCheckBoxList _cblStepStatus = null;
        private RockCheckBoxList _cblStepCampus = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Steps Taken";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
    var result = 'Steps taken';

    var stepProgramName = $('.js-step-program-picker', $content).find(':selected').text();
    if (stepProgramName) {
        result += ' in Program: ' + stepProgramName;

        var checkedStepTypes = $('.js-rockcheckboxlist .js-step-type', $content).find(':checked').closest('label');
        if (checkedStepTypes.length > 0) {
            var stepTypeDelimitedList = checkedStepTypes.map(function() { return $(this).text() }).get().join(',');
            result += ', in Step: ' + stepTypeDelimitedList;
        }

        var checkedStatuses = $('.js-rockcheckboxlist .js-step-status', $content).find(':checked').closest('label');
        if (checkedStatuses.length > 0) {
            var statusesDelimitedList = checkedStatuses.map(function() { return $(this).text() }).get().join(',');
            result += ', with Status: ' + statusesDelimitedList;
        }

        var dateStartedText = $('.js-date-started .js-slidingdaterange-text-value', $content).val();
        if (dateStartedText) {
            result += ', with Date Started: ' + dateStartedText
        }

        var dateCompletedText = $('.js-date-completed .js-slidingdaterange-text-value', $content).val();
        if (dateCompletedText) {
            result += ', with Date Completed: ' + dateCompletedText
        }

        var checkedCampuses = $('.js-rockcheckboxlist .js-step-campus', $content).find(':checked').closest('label');
        if (checkedCampuses.length > 0) {
            var campusesDelimitedList = checkedCampuses.map(function() { return $(this).text() }).get().join(',');
            result += ', at Campus: ' + campusesDelimitedList;
        }
    }

    return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Steps taken";

            var settings = new FilterSettings( selection );

            if ( !settings.IsValid )
            {
                return result;
            }

            var dataContext = new RockContext();

            // Step Program
            var stepProgram = this.GetStepProgram( dataContext, settings.StepProgramGuid );

            if ( stepProgram == null )
            {
                return result;
            }

            // Step Types
            List<StepType> stepTypes;

            if ( settings.StepTypeGuids != null )
            {
                var stepTypeService = new StepTypeService( dataContext );

                stepTypes = stepTypeService.Queryable().Where( a => settings.StepTypeGuids.Contains( a.Guid ) ).ToList();
            }
            else
            {
                stepTypes = new List<StepType>();
            }

            // Step Statuses
            List<StepStatus> stepStatuses;

            if ( settings.StepStatusGuids != null )
            {
                var stepStatusService = new StepStatusService( dataContext );

                stepStatuses = stepStatusService.Queryable().Where( a => settings.StepStatusGuids.Contains( a.Guid ) ).ToList();
            }
            else
            {
                stepStatuses = new List<StepStatus>();
            }

            // Step Campuses
            List<CampusCache> stepCampuses;

            if ( settings.StepCampusGuids != null )
            {
                stepCampuses = CampusCache.All().Where( a => settings.StepCampusGuids.Contains( a.Guid ) ).ToList();
            }
            else
            {
                stepCampuses = new List<CampusCache>();
            }

            result += string.Format( " in Program: {0}", stepProgram.Name );

            if ( stepTypes.Any() )
            {
                result += string.Format( ", in Step: {0}", stepTypes.Select( a => a.Name ).ToList().AsDelimited( "," ) );
            }

            if ( stepStatuses.Any() )
            {
                result += string.Format( ", with Status: {0}", stepStatuses.Select( a => a.Name ).ToList().AsDelimited( "," ) );
            }

            // Start Date
            if ( settings.StartedInPeriod != null
                 && settings.StartedInPeriod.Range != TimePeriodRangeSpecifier.All )
            {
                result += string.Format( ", with Date Started: {0}", settings.StartedInPeriod.GetDescription() );
            }

            // Completion Date
            if ( settings.CompletedInPeriod != null
                 && settings.CompletedInPeriod.Range != TimePeriodRangeSpecifier.All )
            {
                result += string.Format( ", with Date Completed: {0}", settings.CompletedInPeriod.GetDescription() );
            }

            if ( stepCampuses.Any() )
            {
                result += string.Format( ", at Campus: {0}", stepCampuses.Select( a => a.Name ).ToList().AsDelimited( "," ) );
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            int? selectedStepProgramId = null;

            if ( _stepProgramPicker != null )
            {
                selectedStepProgramId = _stepProgramPicker.SelectedId;
            }

            var dataContext = new RockContext();

            // Step Program selection
            _stepProgramPicker = new SingleEntityPicker<StepProgram>();
            _stepProgramPicker.ID = filterControl.ID + "_StepProgramPicker";
            _stepProgramPicker.AddCssClass( "js-step-program-picker" );
            _stepProgramPicker.Label = "Step Program";
            _stepProgramPicker.Help = "The Program in which the Step was undertaken.";
            _stepProgramPicker.Required = true;

            _stepProgramPicker.SelectedIndexChanged += StepProgramPicker_SelectedIndexChanged;
            _stepProgramPicker.AutoPostBack = true;

            var programService = new StepProgramService( dataContext );

            var availablePrograms = programService.Queryable()
                .Where( x => x.IsActive )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            _stepProgramPicker.InitializeListItems( availablePrograms, x => x.Name, allowEmptySelection: true );

            _stepProgramPicker.SelectedId = selectedStepProgramId;

            filterControl.Controls.Add( _stepProgramPicker );

            // Step Type selection
            _cblStepType = new RockCheckBoxList();
            _cblStepType.ID = filterControl.ID + "_cblStepType";
            _cblStepType.AddCssClass( "js-step-type" );
            _cblStepType.Label = "Steps";
            _cblStepType.Help = "If selected, specifies the required Steps that have been undertaken.";

            filterControl.Controls.Add( _cblStepType );

            // Step Status selection
            _cblStepStatus = new RockCheckBoxList();
            _cblStepStatus.ID = filterControl.ID + "_cblStepStatus";
            _cblStepStatus.AddCssClass( "js-step-status" );
            _cblStepStatus.Label = "Statuses";
            _cblStepStatus.Help = "If selected, specifies the required Statuses of the Steps.";

            filterControl.Controls.Add( _cblStepStatus );

            // Date Started
            var drpStarted = new SlidingDateRangePicker();
            drpStarted.ID = filterControl.ID + "_drpDateStarted";
            drpStarted.AddCssClass( "js-date-started" );
            drpStarted.Label = "Date Started";
            drpStarted.Help = "The date range within which the Step was started";

            filterControl.Controls.Add( drpStarted );

            // Date Completed
            var drpCompleted = new SlidingDateRangePicker();
            drpCompleted.ID = filterControl.ID + "_drpDateCompleted";
            drpCompleted.AddCssClass( "js-date-completed" );
            drpCompleted.Label = "Date Completed";
            drpCompleted.Help = "The date range within which the Step was completed";

            filterControl.Controls.Add( drpCompleted );

            // Step Campus selection
            _cblStepCampus = new RockCheckBoxList();
            _cblStepCampus.ID = filterControl.ID + "_cblStepCampus";
            _cblStepCampus.AddCssClass( "js-step-campus" );
            _cblStepCampus.Label = "Campuses";
            _cblStepCampus.Help = "Select the campuses that the steps were completed at. Not selecting a value will select all campuses.";

            filterControl.Controls.Add( _cblStepCampus );

            // Populate lists
            PopulateStepProgramRelatedSelectionLists( selectedStepProgramId );
            PopulateStepCampuses();

            return new Control[] { _stepProgramPicker, _cblStepType, _cblStepStatus, drpStarted, drpCompleted, _cblStepCampus };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the StepProgramPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void StepProgramPicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            int stepProgramId = _stepProgramPicker.SelectedValueAsId() ?? 0;

            PopulateStepProgramRelatedSelectionLists( stepProgramId );
        }

        /// <summary>
        /// Populates the step campuses.
        /// </summary>
        private void PopulateStepCampuses()
        {
            _cblStepCampus.DataSource = CampusCache.All();
            _cblStepCampus.DataTextField = "Name";
            _cblStepCampus.DataValueField = "Guid";
            _cblStepCampus.DataBind();
        }

        /// <summary>
        /// Populates the selection lists for Step Type and Step Status.
        /// </summary>
        /// <param name="stepProgramId">The Step Program identifier.</param>
        private void PopulateStepProgramRelatedSelectionLists( int? stepProgramId )
        {
            var dataContext = new RockContext();

            var programService = new StepProgramService( dataContext );

            StepProgram stepProgram = null;

            if ( stepProgramId != null )
            {
                stepProgram = programService.Get( stepProgramId.Value );
            }

            if ( stepProgram != null )
            {
                // Step Type list
                _cblStepType.Items.Clear();

                var stepTypeService = new StepTypeService( dataContext );

                var stepTypes = stepTypeService.Queryable().Where( x => x.StepProgramId == stepProgramId );

                foreach ( var item in stepTypes )
                {
                    _cblStepType.Items.Add( new ListItem( item.Name, item.Guid.ToString() ) );
                }

                _cblStepType.Visible = _cblStepType.Items.Count > 0;

                // Step Status list
                _cblStepStatus.Items.Clear();

                var stepStatusService = new StepStatusService( dataContext );

                var stepStatuses = stepStatusService.Queryable().Where( x => x.StepProgramId == stepProgramId );

                foreach ( var item in stepStatuses )
                {
                    _cblStepStatus.Items.Add( new ListItem( item.Name, item.Guid.ToString() ) );
                }

                _cblStepStatus.Visible = _cblStepStatus.Items.Count > 0;
            }
            else
            {
                _cblStepType.Visible = false;
                _cblStepStatus.Visible = false;
            }
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        private StepProgram GetStepProgram( RockContext dataContext, int stepProgramId )
        {
            var programService = new StepProgramService( dataContext );

            var stepProgram = programService.Get( stepProgramId );

            return stepProgram;
        }

        private StepProgram GetStepProgram( RockContext dataContext, Guid? stepProgramGuid )
        {
            if ( stepProgramGuid == null )
            {
                return null;
            }

            var programService = new StepProgramService( dataContext );

            var stepProgram = programService.Get( stepProgramGuid.Value );

            return stepProgram;
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var stepProgramPicker = ( controls[0] as SingleEntityPicker<StepProgram> );
            var cblStepTypes = ( controls[1] as RockCheckBoxList );
            var cblStepStatuses = ( controls[2] as RockCheckBoxList );
            var sdpDateStarted = ( controls[3] as SlidingDateRangePicker );
            var sdpDateCompleted = ( controls[4] as SlidingDateRangePicker );
            var cblStepCampuses = ( controls[5] as RockCheckBoxList );

            var settings = new FilterSettings();

            int stepProgramId = stepProgramPicker.SelectedValueAsId() ?? 0;

            var dataContext = new RockContext();

            var stepProgram = GetStepProgram( dataContext, stepProgramId );

            if ( stepProgram != null )
            {
                settings.StepProgramGuid = stepProgram.Guid;
            }

            settings.StepTypeGuids = cblStepTypes.SelectedValues.AsGuidList();

            settings.StepStatusGuids = cblStepStatuses.SelectedValues.AsGuidList();

            settings.StartedInPeriod.FromDelimitedString( sdpDateStarted.DelimitedValues );

            settings.CompletedInPeriod.FromDelimitedString( sdpDateCompleted.DelimitedValues );

            settings.StepCampusGuids = cblStepCampuses.SelectedValues.AsGuidList();

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var stepProgramPicker = ( controls[0] as SingleEntityPicker<StepProgram> );
            var cblStepTypes = ( controls[1] as RockCheckBoxList );
            var cblStepStatuses = ( controls[2] as RockCheckBoxList );
            var sdpDateStarted = ( controls[3] as SlidingDateRangePicker );
            var sdpDateCompleted = ( controls[4] as SlidingDateRangePicker );
            var cblStepCampuses = ( controls[5] as RockCheckBoxList );

            var settings = new FilterSettings( selection );

            // Step Program
            var dataContext = new RockContext();

            StepProgram stepProgram = null;

            if ( settings.StepProgramGuid.HasValue )
            {
                stepProgram = new StepProgramService( dataContext ).Get( settings.StepProgramGuid.Value );
            }

            int? stepProgramId = null;

            if ( stepProgram != null )
            {
                stepProgramId = stepProgram.Id;
            }

            stepProgramPicker.SetValue( stepProgramId );

            PopulateStepProgramRelatedSelectionLists( stepProgramId );

            foreach ( var item in cblStepTypes.Items.OfType<ListItem>() )
            {
                item.Selected = settings.StepTypeGuids.Contains( item.Value.AsGuid() );
            }

            foreach ( var item in cblStepStatuses.Items.OfType<ListItem>() )
            {
                item.Selected = settings.StepStatusGuids.Contains( item.Value.AsGuid() );
            }

            sdpDateStarted.DelimitedValues = settings.StartedInPeriod.ToDelimitedString();

            sdpDateCompleted.DelimitedValues = settings.CompletedInPeriod.ToDelimitedString();

            PopulateStepCampuses();

            foreach ( var item in cblStepCampuses.Items.OfType<ListItem>() )
            {
                item.Selected = settings.StepCampusGuids.Contains( item.Value.AsGuid() );
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var settings = new FilterSettings( selection );

            if ( !settings.IsValid )
            {
                return null;
            }

            var dataContext = ( RockContext ) serviceInstance.Context;

            int stepProgramId = 0;

            var stepProgram = GetStepProgram( dataContext, settings.StepProgramGuid );

            if ( stepProgram != null )
            {
                stepProgramId = stepProgram.Id;
            }

            var stepService = new StepService( dataContext );

            // Filter by Step Program
            var stepQuery = stepService.Queryable().Where( x => x.StepType.StepProgramId == stepProgramId );

            // Filter by Step Types
            if ( settings.StepTypeGuids.Count() > 0 )
            {
                var stepTypeService = new StepTypeService( dataContext );

                var stepTypeIds = stepTypeService.Queryable()
                                    .Where( a => settings.StepTypeGuids.Contains( a.Guid ) )
                                    .Select( a => a.Id ).ToList();

                stepQuery = stepQuery.Where( x => stepTypeIds.Contains( x.StepTypeId ) );
            }

            // Filter by Step Status
            if ( settings.StepStatusGuids.Count() > 0 )
            {
                var stepStatusService = new StepStatusService( dataContext );

                var stepStatusIds = stepStatusService.Queryable()
                                        .Where( a => settings.StepStatusGuids.Contains( a.Guid ) )
                                        .Select( a => a.Id ).ToList();

                stepQuery = stepQuery.Where( x => x.StepStatusId.HasValue && stepStatusIds.Contains( x.StepStatusId.Value ) );
            }

            // Filter by Date Started
            if ( settings.StartedInPeriod != null )
            {
                var startDateRange = settings.StartedInPeriod.GetDateRange( TimePeriodDateRangeBoundarySpecifier.Exclusive );

                if ( startDateRange.Start != null )
                {
                    stepQuery = stepQuery.Where( x => x.StartDateTime > startDateRange.Start.Value );
                }
                if ( startDateRange.End != null )
                {
                    stepQuery = stepQuery.Where( x => x.StartDateTime < startDateRange.End.Value );
                }
            }

            // Filter by Date Completed
            if ( settings.CompletedInPeriod != null )
            {
                var completedDateRange = settings.CompletedInPeriod.GetDateRange( TimePeriodDateRangeBoundarySpecifier.Exclusive );

                if ( completedDateRange.Start != null )
                {
                    stepQuery = stepQuery.Where( x => x.CompletedDateTime > completedDateRange.Start.Value );
                }
                if ( completedDateRange.End != null )
                {
                    stepQuery = stepQuery.Where( x => x.CompletedDateTime < completedDateRange.End.Value );
                }
            }

            // Filter by Step Campus
            if ( settings.StepCampusGuids.Count() > 0 )
            {
                var campusService = new CampusService( dataContext );

                var stepCampusIds = campusService.Queryable()
                                        .Where( a => settings.StepCampusGuids.Contains( a.Guid ) )
                                        .Select( a => a.Id ).ToList();

                stepQuery = stepQuery.Where( x => x.CampusId.HasValue && stepCampusIds.Contains( x.CampusId.Value ) );
            }

            // Create Person Query.
            var personService = new PersonService( ( RockContext ) serviceInstance.Context );

            var qry = personService.Queryable()
                        .Where( p => stepQuery.Any( x => x.PersonAlias.PersonId == p.Id ) );

            var extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}