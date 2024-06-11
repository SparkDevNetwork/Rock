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
using System.Linq.Expressions;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Utility;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataSelect.Group
{
    /// <summary>
    /// The Data Select responsible for including completed learning course details.
    /// </summary>
    [Description( "Select the Person's completed Learning Course" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Completed Learning Course" )]
    [Rock.SystemGuid.EntityTypeGuid( "63b0c1e0-2800-4459-9415-8f27b5710414" )]
    public class HasCompletedCourseSelect : DataSelectComponent
    {
        #region Properties

        /// <inheritdoc/>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <inheritdoc/>
        public override string Section
        {
            get
            {
                return "LMS";
            }
        }

        /// <inheritdoc/>
        public override string ColumnPropertyName
        {
            get
            {
                return "HasCompletedCourse";
            }
        }

        /// <inheritdoc/>
        public override Type ColumnFieldType
        {
            get { return typeof( CourseCompletionData ); }
        }

        /// <inheritdoc/>
        public override string ColumnHeaderText
        {
            get
            {
                return "Has Completed Course";
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            var result = new CallbackField();

            result.OnFormatDataValue += ( sender, e ) =>
            {
                var courseData = e.DataValue as CourseCompletionData;

                if ( courseData == null )
                {
                    e.FormattedValue = string.Empty;
                }
                else
                {
                    switch ( courseData.LearningCompletionStatus )
                    {
                        case LearningCompletionStatus.Pass:
                            e.FormattedValue = $"{courseData.SemesterCompleted} ({courseData.GradeText}: {courseData.GradePercent}%)";
                            break;
                        case LearningCompletionStatus.Fail:
                            e.FormattedValue = $"{courseData.SemesterCompleted} ({courseData.GradeText}: {courseData.GradePercent}%)";
                            break;
                        case LearningCompletionStatus.Incomplete:
                            e.FormattedValue = "Pending Completion";
                            break;
                    }
                }
            };

            return result;
        }

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "Has Completed Course";
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            // Define Control: Output Format DropDown List
            var ddlCourse = new RockDropDownList();
            ddlCourse.ID = parentControl.GetChildControlInstanceName( _CtlCourse );
            ddlCourse.Label = "Completed Course";
            ddlCourse.Help = "The Learning Course that should have been completed during the specified time frame.";

            var courses = new LearningCourseService( new RockContext() ).Queryable().AsNoTracking()
               .Where( c => c.IsActive )
               .Select( c => new
               {
                   c.Id,
                   c.Name
               } )
               .ToList()
               .OrderBy( a => a.Name );
            ddlCourse.Items.Clear();
            ddlCourse.Items.Insert( 0, new ListItem() );
            foreach ( var course in courses )
            {
                var courseName = course?.Name ?? string.Empty;
                ddlCourse.Items.Add( new ListItem( courseName, course.Id.ToString() ) );
            }

            parentControl.Controls.Add( ddlCourse );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = parentControl.GetChildControlInstanceName( _CtlSlidingDateRange );
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            parentControl.Controls.Add( slidingDateRangePicker );

            var cblStatuses = new RockCheckBoxList();
            cblStatuses.Label = "with Course Status (optional)";
            cblStatuses.ID = parentControl.GetChildControlInstanceName( _CtlStatuses );
            cblStatuses.CssClass = "js-statuses";
            parentControl.Controls.Add( cblStatuses );

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

            return new System.Web.UI.Control[] { ddlCourse, slidingDateRangePicker, cblStatuses };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            var ddlProgram = controls.GetByName<DropDownList>( _CtlCourse );
            var slidingDateRangePicker = controls.GetByName<SlidingDateRangePicker>( _CtlSlidingDateRange );
            var cblStatuses = controls.GetByName<CheckBoxList>( _CtlStatuses );

            var settings = new HasCompletedCourseSelectSettings
            {
                LearningCourseId = ddlProgram.SelectedValue.AsIntegerOrNull(),
                SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues
            };

            var delimitedSelections = string.Empty;

            foreach ( ListItem item in cblStatuses.Items )
            {
                if ( item.Selected && Enum.TryParse<LearningCompletionStatus>( item.Value, out var enumValue ) )
                {
                    // Prefix a semi-colon if this is not the first record.
                    delimitedSelections += delimitedSelections.Length == 0 ?
                        enumValue.ToString() :
                        $";{enumValue}";
                }
            }

            settings.DelimitedLearningCompletionStatuses = delimitedSelections;
            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            var ddlProgram = controls.GetByName<RockDropDownList>( _CtlCourse );
            var slidingDateRangePicker = controls.GetByName<SlidingDateRangePicker>( _CtlSlidingDateRange );
            var cblStatuses = controls.GetByName<CheckBoxList>( _CtlStatuses );

            var settings = new HasCompletedCourseSelectSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            ddlProgram.SelectedValue = settings.LearningCourseId.ToString();
            slidingDateRangePicker.DelimitedValues = settings.SlidingDateRangeDelimitedValues;

            foreach ( ListItem item in cblStatuses.Items )
            {
                var isSelected = false;

                if ( settings.LearningCompletionStatuses != null && Enum.TryParse<LearningCompletionStatus>( item.Value, out var enumValue ) )
                {
                    isSelected = settings.LearningCompletionStatuses.Contains( enumValue );
                }

                item.Selected = isSelected;
            }
        }

        /// <inheritdoc/>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var settings = new HasCompletedCourseSelectSettings( selection );
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( settings.SlidingDateRangeDelimitedValues );

            var courseQuery = settings.LearningCourseId.HasValue ?
                new LearningParticipantService( context ).Queryable()
                    .AsNoTracking()
                    .Include( c => c.LearningClass )
                    .Include( c => c.LearningClass.LearningSemester )
                    .Include( c => c.LearningGradingSystemScale )
                    .Where( c => c.LearningClass.LearningCourseId == settings.LearningCourseId ) :
                new List<LearningParticipant>().AsQueryable();

            if ( dateRange.Start.HasValue || dateRange.End.HasValue )
            {
                courseQuery = courseQuery.Where( c => c.LearningCompletionDateTime.HasValue );

                if ( dateRange.Start.HasValue )
                {
                    var startDate = dateRange.Start.Value;
                    courseQuery = courseQuery.Where( c => c.LearningCompletionDateTime >= startDate );
                }

                if ( dateRange.End.HasValue )
                {
                    var endDate = dateRange.End.Value;
                    courseQuery = courseQuery.Where( c => c.LearningCompletionDateTime <= endDate );
                }
            }

            // Filter to the selected statuses or to all available options (if nothing is selected).
            var filterToStatuses = settings.LearningCompletionStatuses.Any() ?
                settings.LearningCompletionStatuses :
                CompletionStatusesWithCompletedDateValue;

            courseQuery = courseQuery.Where( c => filterToStatuses.Contains( c.LearningCompletionStatus ) );

            var personQuery = new PersonService( context ).Queryable()
                .AsNoTracking()
                .Select( p => courseQuery
                    .Where( c => c.PersonId == p.Id )
                    .Select( c => new CourseCompletionData
                    {
                        SemesterCompleted = c.LearningClass.LearningSemester.Name,
                        LearningCompletionStatus = c.LearningCompletionStatus,
                        GradePercent = c.LearningGradePercent * 100,
                        GradeText = c.LearningGradingSystemScale.Name
                    } )
                    .FirstOrDefault()
                );

            var selectExpression = SelectExpressionExtractor.Extract( personQuery, entityIdProperty, "p" );

            return selectExpression;
        }

        #endregion

        private const string _CtlCourse = "ddlCourse";
        private const string _CtlSlidingDateRange = "slidingDateRangePicker";
        private const string _CtlStatuses = "cblStatuses";

        private class CourseCompletionData
        {
            public string SemesterCompleted { get; set; }
            public LearningCompletionStatus LearningCompletionStatus { get; set; }
            public decimal GradePercent { get; set; }
            public string GradeText { get; set; }
        }

        /// <summary>
        /// A list of <see cref="LearningCompletionStatus"/> enums that the user can filter against.
        /// </summary>
        /// <remarks>
        /// Because the LearningCompletionDateTime filter only includes statuses that would set a value for LearningCompletionDateTime.
        /// Choosing all (available) has the same effect has choosing none. The results will be filtered to only those that are available.
        /// </remarks>
        private readonly List<LearningCompletionStatus> CompletionStatusesWithCompletedDateValue = new List<LearningCompletionStatus> { LearningCompletionStatus.Pass, LearningCompletionStatus.Fail };

        private class HasCompletedCourseSelectSettings : SettingsStringBase
        {
            /// <summary>
            /// Gets or sets the learning program identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning program identifiers.
            /// </value>
            public int? LearningCourseId { get; set; }

            /// <summary>
            /// Gets or sets the completion statuses to filter to.
            /// </summary>
            /// <value>
            /// The <see cref="LearningCompletionStatus" /> to filter to.
            /// </value>
            public List<LearningCompletionStatus> LearningCompletionStatuses
            {
                get
                {
                    var statuses = new List<LearningCompletionStatus>();
                    if ( DelimitedLearningCompletionStatuses.IsNotNullOrWhiteSpace() )
                    {
                        var statusValues = DelimitedLearningCompletionStatuses.SplitDelimitedValues( ";" );
                        foreach ( var statusValue in statusValues )
                        {
                            if ( Enum.TryParse<LearningCompletionStatus>( statusValue, out var status ) )
                            {
                                statuses.Add( status );
                            }
                        }
                    }

                    return statuses;
                }
            }

            /// <summary>
            /// Gets or sets a string of delimited values for filtering the <see cref="LearningCompletionStatus"/>.
            /// </summary>
            /// <value>
            /// The semi-colon delimited list of status values.
            /// </value>
            public string DelimitedLearningCompletionStatuses { get; set; }

            /// <summary>
            /// Gets or sets the sliding date range.
            /// </summary>
            /// <value>
            /// The sliding date range.
            /// </value>
            public string SlidingDateRangeDelimitedValues { get; set; }

            public HasCompletedCourseSelectSettings()
            {

            }

            public HasCompletedCourseSelectSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>
                {
                    LearningCourseId.ToStringSafe(),
                    SlidingDateRangeDelimitedValues.Replace( "|", ";" ).ToStringSafe(),
                    DelimitedLearningCompletionStatuses
                };

                return settings;
            }

            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                LearningCourseId = DataComponentSettingsHelper.GetParameterOrDefault( parameters, 0, string.Empty ).AsIntegerOrNull();
                SlidingDateRangeDelimitedValues = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 1 ).Replace( ";", "|" );
                DelimitedLearningCompletionStatuses = DataComponentSettingsHelper.GetParameterOrDefault( parameters, 2, string.Empty );
            }
        }
    }
}
