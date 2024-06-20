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
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
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
                "$('.js-status', $content).val() + ' ' + " +
                "$('.js-slidingdaterange-text-value', $content).val()";
        }

        /// <inheritdoc/>
        public override string FormatSelection( Type entityType, string selection )
        {
            string s = "Completing Course";

            var selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null && selectionConfig.LearningCourseId.HasValue )
            {
                var course = new LearningCourseService( new RockContext() ).Get( selectionConfig.LearningCourseId.Value );
                var dateRangeString = string.Empty;
                if ( selectionConfig.SlidingDateRangeDelimitedValues.IsNotNullOrWhiteSpace() )
                {
                    dateRangeString = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
                    if ( dateRangeString.IsNotNullOrWhiteSpace() )
                    {
                        dateRangeString += $" in the Date Range: {dateRangeString}";
                    }
                }

                s = string.Format(
                    "Completed the '{0}' course '{1}'",
                    course.Name,
                    dateRangeString );
            }

            return s;
        }

        /// <inheritdoc/>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddlCourse = new RockDropDownList();
            ddlCourse.ID = filterControl.GetChildControlInstanceName( "ddlCourse" );
            ddlCourse.AddCssClass( "js-course" );
            ddlCourse.Label = "Course";
            filterControl.Controls.Add( ddlCourse );

            var courseItems = new LearningCourseService( new RockContext() ).Queryable().AsNoTracking()
                .Where( p => p.IsActive )
                .Select( c => new
                {
                    c.Name,
                    c.Guid
                } )
                .ToList()
                .OrderBy( a => a.Name )
                .Select( c => new ListItem( c.Name, c.Guid.ToString() ) ).ToArray();
            ddlCourse.Items.Clear();
            ddlCourse.Items.Insert( 0, new ListItem() );
            ddlCourse.Items.AddRange( courseItems );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = filterControl.GetChildControlInstanceName( "slidingDateRangePicker" );
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            var cblStatuses = new RockCheckBoxList();
            cblStatuses.Label = "with Course Status (optional)";
            cblStatuses.ID = filterControl.GetChildControlInstanceName( "cblStatuses" );
            cblStatuses.CssClass = "js-statuses";
            filterControl.Controls.Add( cblStatuses );

            cblStatuses.Items.Clear();
            var completionStatusType = typeof( LearningCompletionStatus );

            foreach ( LearningCompletionStatus enumValue in CompletionStatusesWithCompletedDateValue )
            {
                if ( !CompletionStatusesWithCompletedDateValue.Contains( enumValue ) )
                {
                    continue;
                }

                cblStatuses.Items.Add( new ListItem( Enum.GetName( completionStatusType, enumValue ), enumValue.ToString() ) );
            }

            var controls = new Control[3] { ddlCourse, slidingDateRangePicker, cblStatuses };

            // convert pipe to comma delimited
            var defaultDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( "|", "," );

            // set the default values in case this is a newly added filter
            var selectionConfig = new SelectionConfig()
            {
                SlidingDateRangeDelimitedValues = defaultDelimitedValues
            };

            SetSelection(
                entityType,
                controls,
                selectionConfig.ToJson() );

            return controls;
        }

        /// <inheritdoc/>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            var ddlCourse = controls[0] as RockDropDownList;
            var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;
            var cblStatuses = controls[2] as CheckBoxList;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlCourse.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-8" );
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

            if ( controls.Count() >= 3 )
            {
                var ddlCourse = controls[0] as RockDropDownList;
                var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;
                var cblStatuses = controls[2] as CheckBoxList;

                selectionConfig.LearningCourseId = ddlCourse.SelectedValueAsInt();
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
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );

            var ddlCourse = controls[0] as RockDropDownList;
            var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;
            var cblStatuses = controls[2] as CheckBoxList;

            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangeDelimitedValues;
            ddlCourse.SetValue( selectionConfig.LearningCourseId );

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
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            var selectedCourseId = selectionConfig.LearningCourseId.ToIntSafe();

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
            var rockContext = serviceInstance.Context as RockContext;

            var participantQuery = selectedCourseId == 0 ?
                new List<LearningParticipant>().AsQueryable() :
                new LearningParticipantService( rockContext ).Queryable()
                .Include( c => c.LearningClass )
                .Where( c => c.LearningClass.LearningCourseId == selectedCourseId );

            if ( dateRange.Start.HasValue )
            {
                var startDate = dateRange.Start.Value;
                participantQuery = participantQuery.Where( c => c.LearningCompletionDateTime.HasValue && c.LearningCompletionDateTime >= startDate );
            }

            if ( dateRange.End.HasValue )
            {
                var endDate = dateRange.End.Value;
                participantQuery = participantQuery.Where( c => c.LearningCompletionDateTime.HasValue && c.LearningCompletionDateTime <= endDate );
            }

            // Filter to the selected statuses or to all available options (if nothing is selected).
            var filterToStatuses = selectionConfig.LearningCompletionStatuses.Any() ?
                selectionConfig.LearningCompletionStatuses :
                CompletionStatusesWithCompletedDateValue;

            participantQuery.Where( c => filterToStatuses.Contains( c.LearningCompletionStatus ) );

            // Create the query that will be used for extracting the Person.
            var personQuery = new PersonService( rockContext ).Queryable().Where( p => participantQuery.Any( c => c.PersonId == p.Id ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personQuery, parameterExpression, "p" );
        }

        #endregion

        /// <summary>
        /// A list of <see cref="LearningCompletionStatus"/> enums that the user can filter against.
        /// </summary>
        /// <remarks>
        /// Because the LearningCompletionDateTime filter only includes statuses that would set a value for LearningCompletionDateTime.
        /// Choosing all (available) has the same effect has choosing none. The results will be filtered to only those that are available.
        /// </remarks>
        private readonly List<LearningCompletionStatus> CompletionStatusesWithCompletedDateValue = new List<LearningCompletionStatus> { LearningCompletionStatus.Pass, LearningCompletionStatus.Fail };

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
            }

            /// <summary>
            /// Gets or sets the learning course identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning course identifiers.
            /// </value>
            public int? LearningCourseId { get; set; }

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