﻿// <copyright>
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
using System.Web.UI.WebControls.Expressions;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{

    /// <summary>
    /// The Data View Filter  responsible for determining if a <see cref="Person"/> has completed a <see cref="LearningClassActivity"/>.
    /// </summary>
    [Description( "Filters for People who've completed a specified learning class activity within the selected timeframe." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Has Completed Class Activity Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "fcb26028-6b54-4c2b-a47c-3b6f49a472c6" )]
    public class HasCompletedClassActivityFilter : DataFilterComponent
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
            // Broken
            return @"
function() {
    let result = 'Has Completed the \'' + $('.js-activity', $content).find(':selected').text()  + '\' Activity';

    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val();
    if (dateRangeText) {
        result +=  ' in the Date Range: \'' + dateRangeText + '\'';
    }

    var points = $('.js-points', $content).val();
    var comparisonType = $('.js-comparison-type', $content).find(':selected').text();
    if (points != '' && points >= 0 && comparisonType) {
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
            if ( selectionConfig != null && selectionConfig.LearningClassActivityGuid.HasValue )
            {
                var activity = new LearningClassActivityService( new RockContext() ).Get( selectionConfig.LearningClassActivityGuid.Value );
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
                var pointsString = comparisonType != null && selectionConfig.Points.HasValue
                    ? $"with Points {comparisonType.ToString().SplitCase()} {selectionConfig.Points}"
                    : string.Empty;

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
                Required = true,
                AutoPostBack = true
            };

            ddlCourse.AddCssClass( "js-course" );
            ddlCourse.SelectedIndexChanged += Course_SelectedIndexChanged;
            filterControl.Controls.Add( ddlCourse );
            HasCompletedCourseFilter.SetCourseItems( ddlProgram, ddlCourse );

            // Create the activity drop down.
            var ddlActivity = new RockDropDownList
            {
                ID = filterControl.GetChildControlInstanceName( "ddlActivity" ),
                Label = "Activity",
                Required = true
            };

            ddlActivity.AddCssClass( "js-activity" );
            filterControl.Controls.Add( ddlActivity );
            SetActivityItems( ddlCourse, ddlActivity );

            // Create the points input.
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
               ComparisonType.NotEqualTo |
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
                var ddlActivity = controls[2] as RockDropDownList;
                var ddlPointsComparisonType = controls[3] as DropDownList;
                var nbPoints = controls[4] as NumberBox;
                var slidingDateRangePicker = controls[6] as SlidingDateRangePicker;

                selectionConfig.LearningClassActivityGuid = ddlActivity.SelectedValueAsGuid();
                selectionConfig.SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues;
                selectionConfig.PointsComparisonType = ddlPointsComparisonType.SelectedValue;
                selectionConfig.Points = nbPoints.IntegerValue;
            }

            return selectionConfig.ToJson();
        }

        /// <inheritdoc/>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection ) ?? new SelectionConfig();

            var ddlProgram = controls[0] as RockDropDownList;
            var ddlCourse = controls[1] as RockDropDownList;
            var ddlActivity = controls[2] as RockDropDownList;
            var ddlPointsComparisonType = controls[3] as DropDownList;
            var nbPoints = controls[4] as NumberBox;
            var slidingDateRangePicker = controls[6] as SlidingDateRangePicker;

            var upstreamGuids = new LearningClassActivityService( new RockContext() )
                .Queryable()
                .Where( la => la.Guid == selectionConfig.LearningClassActivityGuid )
                .Select( la => new
                {
                    CourseGuid = la.LearningClass.LearningCourse.Guid,
                    ProgramGuid = la.LearningClass.LearningCourse.LearningProgram.Guid
                } )
                .FirstOrDefault();

            ddlProgram.SetValue( upstreamGuids?.ProgramGuid );

            HasCompletedCourseFilter.SetCourseItems( ddlProgram, ddlCourse );
            ddlCourse.SetValue( upstreamGuids?.CourseGuid );

            SetActivityItems( ddlCourse, ddlActivity );
            ddlActivity.SetValue( selectionConfig.LearningClassActivityGuid );
            ddlPointsComparisonType.SelectedValue = selectionConfig.PointsComparisonType;

            nbPoints.IntegerValue = selectionConfig.Points;
            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangeDelimitedValues;
        }

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection ) ?? new SelectionConfig();
            var selectedActivityGuid = selectionConfig.LearningClassActivityGuid;
            var comparisonType = selectionConfig
                .PointsComparisonType
                .ConvertToEnumOrNull<ComparisonType>();
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
            var rockContext = serviceInstance.Context as RockContext;

            var completionQuery = new LearningClassActivityCompletionService( rockContext ).Queryable();

            if ( selectedActivityGuid.HasValue )
            {
                completionQuery = completionQuery
                    .Where( c => c.LearningClassActivity.Guid == selectedActivityGuid.Value
                        && c.CompletedDateTime.HasValue );
            }
            else
            {
                // Make sure we get no results if not properly configured.
                completionQuery = completionQuery.Where( _ => false );
            }

            if ( dateRange.Start.HasValue )
            {
                var startDate = dateRange.Start.Value;
                completionQuery = completionQuery.Where( c => c.CompletedDateTime >= startDate );
            }

            if ( dateRange.End.HasValue )
            {
                var endDate = dateRange.End.Value;
                completionQuery = completionQuery.Where( c => c.CompletedDateTime <= endDate );
            }

            // Build the custom expression for comparing the value if we have
            // both a comparison type and a value.
            if ( selectionConfig.Points.HasValue && comparisonType.HasValue )
            {
                var valueExpression = Expression.Constant( selectionConfig.Points.Value );
                var completionParameterExpression = Expression.Parameter( typeof( LearningClassActivityCompletion ), "lac" );
                var propertyExpression = Expression.Property( completionParameterExpression, nameof( LearningClassActivityCompletion.PointsEarned ) );
                var comparisonExpression = ComparisonHelper.ValueComparisonExpression( comparisonType.Value, propertyExpression, valueExpression );

                completionQuery = completionQuery.Where( completionParameterExpression, comparisonExpression );
            }

            // Create the query that will be used for extracting the Person.
            var personQuery = new PersonService( rockContext ).Queryable().Where( p => completionQuery.Any( c => c.Student.PersonId == p.Id ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personQuery, parameterExpression, "p" );
        }

        #endregion

        #region Private members

        private void Course_SelectedIndexChanged( object sender, EventArgs e )
        {
            var filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();
            var ddlCourse = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-course" ) );
            var ddlActivity = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-activity" ) );

            SetActivityItems( ddlCourse, ddlActivity );
        }

        private void Program_SelectedIndexChanged( object sender, EventArgs e )
        {
            var filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();
            var ddlProgram = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-program" ) );
            var ddlCourse = filterField.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-course" ) );

            HasCompletedCourseFilter.SetCourseItems( ddlProgram, ddlCourse );
        }

        private void SetActivityItems( DropDownList ddlCourse, DropDownList ddlActivity )
        {
            ddlActivity.Items.Clear();
            ddlActivity.Items.Insert( 0, new ListItem() );

            var selectedCourseGuid = ddlCourse.SelectedValueAsGuid();

            // If there's no course selected hide the activity dropdown and wait for a selection.
            if ( selectedCourseGuid.HasValue )
            {
                var activities = new LearningClassActivityService( new RockContext() )
                    .Queryable()
                    .Where( la => la.LearningClass.LearningCourse.Guid == selectedCourseGuid )
                    .OrderBy( la => la.Order )
                    .Select( la => new
                    {
                        la.Guid,
                        la.LearningActivity.Name
                    } );

                foreach ( var activity in activities )
                {
                    ddlActivity.Items.Add( new ListItem( activity.Name, activity.Guid.ToString() ) );
                }
            }
        }

        #endregion

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Gets or sets the learning activity unique identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning unique activity identifier.
            /// </value>
            public Guid? LearningClassActivityGuid { get; set; }

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