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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{

    /// <summary>
    /// The Data View Filter  responsible for determining if a <see cref="Person"/> has completed a <see cref="LearningCourse"/>.
    /// </summary>
    [Description( "Filters for People who've completed a specified learning activity within the selected timeframe." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Has Completed Activity Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "fcb26028-6b54-4c2b-a47c-3b6f49a472c6" )]
    public class HasCompletedActivityFilter : DataFilterComponent
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
            return "Has Completed Activity";
        }


        /// <inheritdoc/>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
    let result = 'Has Completed the ''' + $('.js-activity', $content).find(':selected').text()  + ''' Activity';

    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val();
    if(dateRangeText) {
        result +=  ' in the Date Range: ''' + dateRangeText + '''';
    }

    var points = $('.js-points', $content).val();
    var comparisonType = $('.js-comparison-type', $content).find(':selected').text();
    if (points >= 0 && comparisonType) {
        result += ' with Points ' + comparisonType + ' ' + points;
    }

    return result;
}
";
        }

        /// <inheritdoc/>
        public override string FormatSelection( Type entityType, string selection )
        {
            string s = "Has Completed Activity";

            var selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null && selectionConfig.LearningActivityId.HasValue )
            {
                var activity = new LearningActivityService( new RockContext() ).Get( selectionConfig.LearningActivityId.Value );
                var dateRangeString = string.Empty;
                if ( selectionConfig.SlidingDateRangeDelimitedValues.IsNotNullOrWhiteSpace() )
                {
                    var formattedDateValue = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
                    if ( formattedDateValue.IsNotNullOrWhiteSpace() )
                    {
                        dateRangeString += $" in the Date Range: '{formattedDateValue}'";
                    }
                }

                var comparisonType = selectionConfig.PointsComparisonType.ConvertToEnumOrNull<ComparisonType>();
                var pointsString = comparisonType != null && selectionConfig.Points.HasValue ?
                    $"with Points {comparisonType} {selectionConfig.Points}" :
                    string.Empty;

                s = string.Format(
                    "Has Completed the '{0}' Activity {1} {2}",
                    activity.Name,
                    dateRangeString,
                    pointsString );
            }

            return s;
        }

        /// <inheritdoc/>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddlProgram = new RockDropDownList();
            ddlProgram.ID = filterControl.GetChildControlInstanceName( "ddlProgram" );
            ddlProgram.AddCssClass( "js-program" );
            ddlProgram.Label = "Program";
            ddlProgram.DataTextField = "Name";
            ddlProgram.DataValueField = "Id";
            ddlProgram.DataSource = new LearningProgramService( new RockContext() ).Queryable().AsNoTracking()
                .Where( p => p.IsActive )
                .Select( p => new
                {
                    p.Id,
                    p.Name
                } )
                .ToList()
                .OrderBy( a => a.Name );
            ddlProgram.DataBind();
            ddlProgram.AutoPostBack = true;
            ddlProgram.SelectedIndexChanged += ProgramSelection_Changed;
            filterControl.Controls.Add( ddlProgram );

            var ddlCourse = new RockDropDownList();
            ddlCourse.ID = filterControl.GetChildControlInstanceName( "ddlCourse" );
            ddlCourse.AddCssClass( "js-course" );
            ddlCourse.Label = "Course";
            ddlCourse.AutoPostBack = true;
            ddlCourse.SelectedIndexChanged += CourseSelection_Changed;
            filterControl.Controls.Add( ddlCourse );

            var ddlActivity = new RockDropDownList();
            ddlActivity.ID = filterControl.GetChildControlInstanceName( "ddlActivity" );
            ddlActivity.AddCssClass( "js-activity" );
            ddlActivity.Label = "Activity";
            filterControl.Controls.Add( ddlActivity );
            SetCourseItems( filterControl );
            SetActivityItems( filterControl );

            var lblPoints = new Label();
            lblPoints.Text = " Points ";
            filterControl.Controls.Add( lblPoints );

            var nbPoints = new NumberBox();
            nbPoints.ID = filterControl.GetChildControlInstanceName( "points" );
            nbPoints.AddCssClass( "js-points" );
            nbPoints.FieldName = "Points";
            filterControl.Controls.Add( nbPoints );

            var ComparisonTypes =
               ComparisonType.EqualTo |
               ComparisonType.LessThan |
               ComparisonType.LessThanOrEqualTo |
               ComparisonType.GreaterThan |
               ComparisonType.GreaterThanOrEqualTo;
            var ddlPointsComparisonType = ComparisonHelper.ComparisonControl( ComparisonTypes );
            ddlPointsComparisonType.ID = filterControl.GetChildControlInstanceName( "pointsComparisonType" );
            ddlPointsComparisonType.AddCssClass( "js-comparison-type" );
            filterControl.Controls.Add( ddlPointsComparisonType );


            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = filterControl.GetChildControlInstanceName( "slidingDateRangePicker" );
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            var controls = new Control[7] { ddlProgram, ddlCourse, ddlActivity, ddlPointsComparisonType, nbPoints, lblPoints, slidingDateRangePicker };

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
            var ddlProgram = controls[0] as RockDropDownList;
            var ddlCourse = controls[1] as RockDropDownList;
            var ddlActivity = controls[2] as RockDropDownList;
            var ddlPointsComparisonType = controls[3] as DropDownList;
            var nbPoints = controls[4] as NumberBox;
            var lblPoints = controls[5] as Label;
            var slidingDateRangePicker = controls[6] as SlidingDateRangePicker;

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
            ddlActivity.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlPointsComparisonType.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-1" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            nbPoints.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-8 mt-2" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            lblPoints.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 3
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

            if ( controls.Count() >= 6 )
            {
                var ddlProgram = controls[0] as RockDropDownList;
                var ddlCourse = controls[1] as RockDropDownList;
                var ddlActivity = controls[2] as RockDropDownList;
                var ddlPointsComparisonType = controls[3] as DropDownList;
                var nbPoints = controls[4] as NumberBox;
                var slidingDateRangePicker = controls[6] as SlidingDateRangePicker;

                selectionConfig.LearningProgramId = ddlProgram.SelectedValueAsInt();
                selectionConfig.LearningCourseId = ddlCourse.SelectedValueAsInt();
                selectionConfig.LearningActivityId = ddlActivity.SelectedValueAsInt();
                selectionConfig.SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues;
                selectionConfig.PointsComparisonType = ddlPointsComparisonType.SelectedValue;
                selectionConfig.Points = nbPoints.IntegerValue;
            }

            return selectionConfig.ToJson();
        }

        /// <inheritdoc/>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );

            var ddlProgram = controls[0] as RockDropDownList;
            var ddlCourse = controls[1] as RockDropDownList;
            var ddlActivity = controls[2] as RockDropDownList;
            var ddlPointsComparisonType = controls[3] as DropDownList;
            var nbPoints = controls[4] as NumberBox;
            var slidingDateRangePicker = controls[6] as SlidingDateRangePicker;

            ddlProgram.SetValue( selectionConfig.LearningProgramId );
            ddlCourse.SetValue( selectionConfig.LearningCourseId );

            // If there's a course selection ensure the activity list is populated
            // before trying to set the dropdown value to the selection.
            var selectedCourseId = ddlCourse.SelectedValueAsInt().ToIntSafe();
            if ( selectedCourseId > 0 )
            {
                SetActivityItems( ddlActivity, selectedCourseId );
            }

            ddlActivity.SetValue( selectionConfig.LearningActivityId );
            ddlPointsComparisonType.SelectedValue = selectionConfig.PointsComparisonType;

            if ( selectionConfig.Points.HasValue )
            {
                nbPoints.IntegerValue = selectionConfig.Points;
            }

            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangeDelimitedValues;
        }

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );

            var comparisonType = selectionConfig
                .PointsComparisonType
                .ConvertToEnumOrNull<ComparisonType>();

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
            var activityId = selectionConfig.LearningActivityId.ToIntSafe();
            var rockContext = serviceInstance.Context as RockContext;

            var completionQuery = new LearningActivityCompletionService( rockContext ).Queryable()
                .Include( c => c.Student )
                .Where( c => c.LearningActivityId == activityId );

            if ( dateRange.Start.HasValue )
            {
                var startDate = dateRange.Start.Value;
                completionQuery = completionQuery.Where( c => c.CompletedDateTime.HasValue && c.CompletedDateTime >= startDate );
            }

            if ( dateRange.End.HasValue )
            {
                var endDate = dateRange.End.Value;
                completionQuery = completionQuery.Where( c => c.CompletedDateTime.HasValue && c.CompletedDateTime <= endDate );
            }

            if ( selectionConfig.Points.HasValue && selectionConfig.Points.Value > 0 && comparisonType != null )
            {
                switch ( comparisonType )
                {
                    case ComparisonType.EqualTo:
                        completionQuery = completionQuery.Where( c => c.PointsEarned == selectionConfig.Points );
                        break;
                    case ComparisonType.LessThan:
                        completionQuery = completionQuery.Where( c => c.PointsEarned < selectionConfig.Points );
                        break;
                    case ComparisonType.LessThanOrEqualTo:
                        completionQuery = completionQuery.Where( c => c.PointsEarned <= selectionConfig.Points );
                        break;
                    case ComparisonType.GreaterThan:
                        completionQuery = completionQuery.Where( c => c.PointsEarned > selectionConfig.Points );
                        break;
                    case ComparisonType.GreaterThanOrEqualTo:
                        completionQuery = completionQuery.Where( c => c.PointsEarned >= selectionConfig.Points );
                        break;
                }
            }

            // Create the query that will be used for extracting the Person.
            var personQuery = new PersonService( rockContext ).Queryable().Where( p => completionQuery.Any( c => c.Student.PersonId == p.Id ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personQuery, parameterExpression, "p" );
        }

        #endregion

        #region Private members

        private void CourseSelection_Changed( object sender, EventArgs e )
        {
            FilterField filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();

            SetActivityItems( filterField );
        }

        private void ProgramSelection_Changed( object sender, EventArgs e )
        {
            FilterField filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();

            SetCourseItems( filterField );
        }

        private void SetActivityItems( FilterField filterField )
        {
            DropDownList ddlCourse = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-course" ) );
            DropDownList ddlActivity = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-activity" ) );

            SetActivityItems( ddlActivity, ddlCourse.SelectedValue.ToIntSafe() );
        }

        private void SetActivityItems( DropDownList ddlActivity, int courseId )
        {
            ddlActivity.Items.Clear();
            ddlActivity.Items.Insert( 0, new ListItem() );

            // If there's no course selected hide the activity dropdown and wait for a selection.
            if ( courseId > 0 )
            {
                var activityItems = new LearningActivityService( new RockContext() ).Queryable()
                .AsNoTracking()
                .Include( a => a.LearningClass )
                .Where( a => a.LearningClass.LearningCourseId == courseId )
                .Select( a => new { a.Name, a.Id } )
                .ToList()
                .OrderBy( a => a.Name )
                .Select( a => new ListItem( a.Name, a.Id.ToString() ) )
                .ToArray();

                ddlActivity.Items.AddRange( activityItems );
            }

            ddlActivity.Visible = ddlActivity.Items.Count > 1;
        }

        private void SetCourseItems( FilterField filterField )
        {
            DropDownList ddlProgram = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-program" ) );
            DropDownList ddlCourse = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-course" ) );

            ddlCourse.Items.Clear();
            ddlCourse.Items.Insert( 0, new ListItem() );

            var selectedProgramId = ddlProgram.SelectedValue.ToIntSafe();

            // If there's a program selected filter to courses for that program.
            if ( selectedProgramId > 0 )
            {
                var courseItems = new LearningCourseService( new RockContext() ).Queryable()
                .AsNoTracking()
                .Include( c => c.LearningProgram )
                .Where( c => c.IsActive && c.LearningProgramId == selectedProgramId )
                .Select( c => new { c.Name, c.Id } )
                .ToList()
                .OrderBy( a => a.Name )
                .Select( c => new ListItem( c.Name, c.Id.ToString() ) )
                .ToArray();

                ddlCourse.Items.AddRange( courseItems );
            }

            ddlCourse.Visible = ddlCourse.Items.Count > 1;
        }

        #endregion

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
            /// Gets or sets the learning program identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning program identifiers.
            /// </value>
            public int? LearningProgramId { get; set; }

            /// <summary>
            /// Gets or sets the learning course identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning course identifiers.
            /// </value>
            public int? LearningCourseId { get; set; }

            /// <summary>
            /// Gets or sets the learning activity identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning activity identifiers.
            /// </value>
            public int? LearningActivityId { get; set; }

            /// <summary>
            /// Gets or sets the points comparison type.
            /// </summary>
            /// <value>
            /// The points comparison type
            /// </value>
            public string PointsComparisonType { get; set; }

            /// <summary>
            /// Gets or sets the activity points to filter for.
            /// </summary>
            /// <value>
            /// The activity points.
            /// </value>
            public int? Points { get; set; }

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