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
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// The Data View Filter  responsible for determining if a <see cref="Person"/> has completed a <see cref="LearningCourse"/>.
    /// </summary>
    [Description( "Filters for People who've completed a specified learning course within the selected timeframe." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Has Completed Course Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "c762905b-b783-41c0-ac90-2991b1d2e018" )]
    public class HasCompletedCourseFilter : DataFilterComponent
    {
        #region Properties

        /// <inheritdoc/>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Obsidian/Reporting/DataFilters/Person/hasCompletedCourseFilter.obs";

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var config = SelectionConfig.Parse( selection );

            var learningProgramOptions = new LearningProgramService( rockContext )
                .Queryable()
                .Where( lp => lp.IsActive && lp.IsCompletionStatusTracked )
                .OrderBy( lp => lp.Name )
                .ToList()
                .Select( lp => lp.ToListItemBag() )
                .ToList();

            var learningProgramGuid = new LearningCourseService( rockContext )
                .Get( config?.LearningCourseGuid ?? Guid.Empty )
                ?.LearningProgram.Guid;

            var learningCourseOptions = new LearningCourseService( rockContext )
                .Queryable()
                .Where( lc => lc.IsActive )
                .OrderBy( lc => lc.Order )
                .ThenBy( lc => lc.Name )
                .ToList();

            var learningCourseOptionsByProgram = new Dictionary<Guid, List<ListItemBag>>();
            // Pre-make the lists for each program, then we'll load them after
            foreach ( var learningProgram in learningProgramOptions )
            {
                learningCourseOptionsByProgram.Add( learningProgram.Value.AsGuid(), new List<ListItemBag>() );
            }

            foreach ( var learningCourse in learningCourseOptions )
            {
                var courseProgramGuid = learningCourse.LearningProgram.Guid;
                if ( learningCourseOptionsByProgram.ContainsKey( courseProgramGuid ) )
                {
                    var list = learningCourseOptionsByProgram[courseProgramGuid];
                    var listItemBag = learningCourse.ToListItemBag();
                    list.Add( listItemBag );
                }
            }

            var data = new Dictionary<string, string>
            {
                { "learningProgramOptions", learningProgramOptions.ToCamelCaseJson( false, true ) },
                { "learningProgram", learningProgramGuid?.ToString() },
                { "learningCourseOptions", learningCourseOptionsByProgram.ToCamelCaseJson( false, true ) },
                { "learningCourse", config?.LearningCourseGuid?.ToString() },
                { "courseStatus", config?.LearningCompletionStatuses.ToCamelCaseJson( false, true ) },
                { "dateRange", config?.SlidingDateRangeDelimitedValues },
                { "selection", selection },
            };

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var selectionConfig = new SelectionConfig
            {
                LearningCourseGuid = data.GetValueOrNull( "learningCourse" )?.AsGuidOrNull(),
                SlidingDateRangeDelimitedValues = data.GetValueOrNull( "dateRange" ),
                LearningCompletionStatuses = data.GetValueOrNull( "courseStatus" )?.FromJsonOrNull<List<LearningCompletionStatus>>()
            };

            //var json = selectionConfig.ToJson();

            //return data.GetValueOrDefault( "selection", "" );
            return selectionConfig.ToJson();
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "Has Completed Course";
        }


        /// <inheritdoc/>
        public override string GetClientFormatSelection( Type entityType )
        {
            return "'Has Completed the ' + " +
                "'\\'' + $('.js-course', $content).find(':selected').text() + '\\' course ' + " +
                "$('.js-statuses input:checked', $content).toArray().map(a => '\\'' + $(a).val() + '\\'').join(' or ' ) + ' ' + " +
                "$('.js-slidingdaterange-text-value', $content).val()";
        }

        /// <inheritdoc/>
        public override string FormatSelection( Type entityType, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            if ( selectionConfig != null && selectionConfig.LearningCourseGuid.HasValue )
            {
                var course = new LearningCourseService( new RockContext() ).Get( selectionConfig.LearningCourseGuid.Value );
                var dateRangeString = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
                var statuses = selectionConfig.LearningCompletionStatuses;
                var passFailText = "";

                if ( statuses != null && statuses.Any() )
                {
                    var statusesText = statuses.Select( s => $"'{s}'" ).JoinStrings( " or " );

                    passFailText = $" with a status of {statusesText}";
                }

                return dateRangeString.IsNotNullOrWhiteSpace()
                    ? $"Completed the '{course?.Name}' course{passFailText} in the date range '{dateRangeString}'"
                    : $"Completed the '{course?.Name}' course{passFailText}";
            }

            return "Completed Course";
        }

        /// <inheritdoc/>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            // Create the program drop down.
            var ddlProgram = new RockDropDownList
            {
                ID = filterControl.GetChildControlInstanceName( "ddlProgram" ),
                Label = "Program",
                Required = true,
                AutoPostBack = true
            };

            ddlProgram.AddCssClass( "js-program" );
            ddlProgram.SelectedIndexChanged += Program_SelectedIndexChanged;
            filterControl.Controls.Add( ddlProgram );
            HasCompletedProgramFilter.SetProgramItems( ddlProgram );

            // Create the course drop down.
            var ddlCourse = new RockDropDownList
            {
                ID = filterControl.GetChildControlInstanceName( "ddlCourse" ),
                Label = "Course",
                Required = true
            };

            ddlCourse.AddCssClass( "js-course" );
            filterControl.Controls.Add( ddlCourse );
            SetCourseItems( ddlProgram, ddlCourse );

            var slidingDateRangePicker = new SlidingDateRangePicker
            {
                ID = filterControl.GetChildControlInstanceName( "slidingDateRangePicker" ),
                Label = "Date Range"
            };

            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            var cblStatuses = new RockCheckBoxList
            {
                ID = filterControl.GetChildControlInstanceName( "cblStatuses" ),
                Label = "with Course Status (optional)"
            };

            cblStatuses.AddCssClass( "js-statuses" );
            filterControl.Controls.Add( cblStatuses );

            cblStatuses.Items.Clear();
            var completionStatusType = typeof( LearningCompletionStatus );

            foreach ( var enumValue in CompletionStatusesWithCompletedDateValue )
            {
                cblStatuses.Items.Add( new ListItem( Enum.GetName( completionStatusType, enumValue ), enumValue.ToString() ) );
            }

            return new Control[4] { ddlProgram, ddlCourse, slidingDateRangePicker, cblStatuses };
        }

        private void Program_SelectedIndexChanged( object sender, EventArgs e )
        {
            var filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();
            var ddlProgram = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-program" ) );
            var ddlCourse = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-course" ) );

            SetCourseItems( ddlProgram, ddlCourse );
        }

        internal static void SetCourseItems( DropDownList ddlProgram, DropDownList ddlCourse )
        {
            ddlCourse.Items.Clear();
            ddlCourse.Items.Insert( 0, new ListItem() );

            var selectedProgramGuid = ddlProgram.SelectedValueAsGuid();

            // If there's a program selected filter to courses for that program.
            if ( selectedProgramGuid.HasValue )
            {
                var courses = new LearningCourseService( new RockContext() )
                    .Queryable()
                    .Where( lc => lc.IsActive && lc.LearningProgram.Guid == selectedProgramGuid )
                    .OrderBy( lc => lc.Order )
                    .ThenBy( lc => lc.Name )
                    .Select( lc => new
                    {
                        lc.Guid,
                        lc.Name
                    } );

                foreach ( var course in courses )
                {
                    ddlCourse.Items.Add( new ListItem( course.Name, course.Guid.ToString() ) );
                }
            }
        }

        /// <inheritdoc/>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            var ddlProgram = controls[0] as RockDropDownList;
            var ddlCourse = controls[1] as RockDropDownList;
            var slidingDateRangePicker = controls[2] as SlidingDateRangePicker;
            var cblStatuses = controls[3] as CheckBoxList;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlProgram.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlCourse.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cblStatuses.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            slidingDateRangePicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <inheritdoc/>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var selectionConfig = new SelectionConfig();

            if ( controls.Count() >= 4 )
            {
                var ddlCourse = controls[1] as RockDropDownList;
                var slidingDateRangePicker = controls[2] as SlidingDateRangePicker;
                var cblStatuses = controls[3] as CheckBoxList;

                selectionConfig.LearningCourseGuid = ddlCourse.SelectedValueAsGuid();
                selectionConfig.SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues;
                selectionConfig.LearningCompletionStatuses = new List<LearningCompletionStatus>();

                foreach ( ListItem item in cblStatuses.Items )
                {
                    if ( item.Selected && Enum.TryParse<LearningCompletionStatus>( item.Value, out var enumValue ) )
                    {
                        selectionConfig.LearningCompletionStatuses.Add( enumValue );
                    }
                }
            }

            return selectionConfig.ToJson();
        }

        /// <inheritdoc/>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection ) ?? new SelectionConfig();

            var filterField = controls[0].FirstParentControlOfType<FilterField>();
            var ddlProgram = controls[0] as RockDropDownList;
            var ddlCourse = controls[1] as RockDropDownList;
            var slidingDateRangePicker = controls[2] as SlidingDateRangePicker;
            var cblStatuses = controls[3] as CheckBoxList;

            var programGuid = new LearningCourseService( new RockContext() )
                .Queryable()
                .Where( lc => lc.Guid == selectionConfig.LearningCourseGuid )
                .Select( lc => ( Guid? ) lc.LearningProgram.Guid )
                .FirstOrDefault();

            ddlProgram.SetValue( programGuid );

            SetCourseItems( ddlProgram, ddlCourse );
            ddlCourse.SetValue( selectionConfig.LearningCourseGuid );

            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangeDelimitedValues;

            foreach ( ListItem item in cblStatuses.Items )
            {
                var isSelected = false;
                if ( selectionConfig.LearningCompletionStatuses != null && Enum.TryParse<LearningCompletionStatus>( item.Value, out var enumValue ) )
                {
                    isSelected = selectionConfig.LearningCompletionStatuses.Contains( enumValue );
                }

                item.Selected = isSelected;
            }
        }

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );
            var selectedCourseGuid = selectionConfig.LearningCourseGuid;
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
            var completionStatuses = selectionConfig.LearningCompletionStatuses;
            var rockContext = serviceInstance.Context as RockContext;

            var participantQuery = GetFilterQuery( rockContext, selectedCourseGuid, dateRange, completionStatuses );

            // Create the query that will be used for extracting the Person.
            var personQuery = new PersonService( rockContext )
                .Queryable()
                .Where( p => participantQuery.Any( lp => lp.PersonId == p.Id ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personQuery, parameterExpression, "p" );
        }

        /// <summary>
        /// Gets the query with the settings applied. This method allows us to
        /// ensure we use the same logic in the Data Filter and the Data Select.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="selectedCourseGuid">The unique identifier of the course.</param>
        /// <param name="dateRange">The date range to limit results to.</param>
        /// <param name="completionStatuses">The optional completion statuses that must be met.</param>
        /// <returns>A queryable that matches the parameters.</returns>
        internal static IQueryable<LearningParticipant> GetFilterQuery( RockContext rockContext, Guid? selectedCourseGuid, DateRange dateRange, List<LearningCompletionStatus> completionStatuses )
        {
            var participantQuery = new LearningParticipantService( rockContext ).Queryable();

            if ( selectedCourseGuid.HasValue )
            {
                participantQuery = participantQuery
                    .Where( lp => lp.LearningClass.LearningCourse.Guid == selectedCourseGuid.Value
                        && lp.LearningCompletionDateTime.HasValue );
            }
            else
            {
                // Make sure we get no results if not properly configured.
                participantQuery = participantQuery.Where( _ => false );
            }

            if ( dateRange.Start.HasValue )
            {
                var startDate = dateRange.Start.Value;
                participantQuery = participantQuery
                    .Where( c => c.LearningCompletionDateTime >= startDate );
            }

            if ( dateRange.End.HasValue )
            {
                var endDate = dateRange.End.Value;
                participantQuery = participantQuery
                    .Where( c => c.LearningCompletionDateTime <= endDate );
            }

            // Filter to the selected statuses or to all available options (if nothing is selected).
            var filterToStatuses = completionStatuses != null && completionStatuses.Any()
                ? completionStatuses
                : CompletionStatusesWithCompletedDateValue;

            return participantQuery.Where( c => filterToStatuses.Contains( c.LearningCompletionStatus ) );
        }

        #endregion

        /// <summary>
        /// A list of <see cref="LearningCompletionStatus"/> enums that the user can filter against.
        /// </summary>
        /// <remarks>
        /// Because the LearningCompletionDateTime filter only includes statuses that would set a value for LearningCompletionDateTime.
        /// Choosing all (available) has the same effect has choosing none. The results will be filtered to only those that are available.
        /// </remarks>
        internal static readonly List<LearningCompletionStatus> CompletionStatusesWithCompletedDateValue = new List<LearningCompletionStatus> { LearningCompletionStatus.Pass, LearningCompletionStatus.Fail };

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Gets or sets the learning course identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning course identifiers.
            /// </value>
            public Guid? LearningCourseGuid { get; set; }

            /// <summary>
            /// Gets or sets the completion statuses to filter to.
            /// </summary>
            /// <value>
            /// The <see cref="LearningCompletionStatus" /> to filter to.
            /// </value>
            public List<LearningCompletionStatus> LearningCompletionStatuses { get; set; }

            /// <summary>
            /// Gets or sets the sliding date range.
            /// </summary>
            /// <value>
            /// The sliding date range.
            /// </value>
            public string SlidingDateRangeDelimitedValues { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                return selection.FromJsonOrNull<SelectionConfig>();
            }
        }
    }
}