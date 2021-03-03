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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people on whether they have attended the campus(es) a specific number of times" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Campus Attendance Filter" )]
    public class AttendanceCampusFilter : DataFilterComponent
    {
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
            get { return "Attendance"; }
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
            return "Attendance at Campuses";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The client format script.
        /// </returns>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
    var result = 'Campuses';
    var campusesPicker = $('.js-campuses-picker', $content);
    var checkedCampuses = $('.js-campuses-picker', $content).find(':checked').closest('label');
    if (checkedCampuses.length) {
        var campusCommaList = checkedCampuses.map(function() { return $(this).text() }).get().join(',');
        result = 'Campuses: ' + campusCommaList;
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
            string result = "Attendance at Campuses";

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                var campusGuidList = options[0].Split( ',' ).AsGuidList();
                List<string> campusNames = new List<string>();
                foreach ( var campusGuid in campusGuidList )
                {
                    var campus = CampusCache.Get( campusGuid );
                    if ( campus != null )
                    {
                        campusNames.Add( campus.Name );
                    }
                }

                ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );

                string dateRangeText;
                int? lastXWeeks = options[3].AsIntegerOrNull();
                if ( lastXWeeks.HasValue )
                {
                    dateRangeText = " in last " + ( lastXWeeks.Value * 7 ).ToString() + " days";
                }
                else
                {
                    dateRangeText = SlidingDateRangePicker.FormatDelimitedValues( options[3].Replace( ',', '|' ) );
                }

                result = string.Format(
                    "Attended '{0}' {1} {2} times. Date Range: {3}",
                    campusNames.Any() ? "Campuses: " + campusNames.AsDelimited( ", " ) : "?",
                    comparisonType.ConvertToString(),
                    options[2],
                    dateRangeText );
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the filter controls.</param>
        /// <returns>
        /// The array of new controls created to implement the filter.
        /// </returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var campusesPicker = new CampusesPicker();
            campusesPicker.Label = "Campuses";
            campusesPicker.ID = filterControl.ID + "_0";
            campusesPicker.Label = string.Empty;
            campusesPicker.CssClass = "js-campuses-picker campuses-picker";
            campusesPicker.Campuses = CampusCache.All();
            filterControl.Controls.Add( campusesPicker );

            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes );
            ddlIntegerCompare.Label = "Attendance Count";
            ddlIntegerCompare.ID = filterControl.ID + "_ddlIntegerCompare";
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );

            var tbAttendedCount = new RockTextBox();
            tbAttendedCount.ID = filterControl.ID + "_2";
            tbAttendedCount.Label = "&nbsp;"; // give it whitespace label so it lines up nicely
            tbAttendedCount.AddCssClass( "js-attended-count" );
            filterControl.Controls.Add( tbAttendedCount );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            var controls = new Control[4] { campusesPicker, ddlIntegerCompare, tbAttendedCount, slidingDateRangePicker };

            // convert pipe to comma delimited
            var defaultDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( "|", "," );
            var defaultCount = 4;

            // set the default values in case this is a newly added filter
            SetSelection(
                entityType,
                controls,
                string.Format( "{0}|{1}|{2}|{3}|false", campusesPicker.Items.Count > 0 ? campusesPicker.Items[0].Value : "0", ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString(), defaultCount, defaultDelimitedValues ) );

            return controls;
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
            var attendanceCampus = controls[0] as CampusesPicker;
            var ddlIntegerCompare = controls[1] as DropDownList;
            var tbAttendedCount = controls[2] as RockTextBox;
            var slidingDateRangePicker = controls[3] as SlidingDateRangePicker;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            attendanceCampus.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlIntegerCompare.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-1" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbAttendedCount.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-7" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            slidingDateRangePicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var campusPicker = controls[0] as CampusesPicker;
            var campusIds = campusPicker.SelectedCampusIds;

            if ( !campusPicker.Visible && !campusIds.Any() )
            {
                var campuses = CampusCache.All()
        .Where( c => !c.IsActive.HasValue || c.IsActive.Value || campusPicker.IncludeInactive )
        .OrderBy( c => c.Order )
        .ToList();

                if ( campuses.Count == 1 )
                {
                    campusIds = new List<int>() { campuses.First().Id };
                }
            }

            var ddlIntegerCompare = controls[1] as DropDownList;
            var tbAttendedCount = controls[2] as RockTextBox;
            var slidingDateRangePicker = controls[3] as SlidingDateRangePicker;

            if ( campusIds != null && campusIds.Any() )
            {
                List<Guid> campusGuids = new List<Guid>();
                foreach ( var campusId in campusIds )
                {
                    var campus = CampusCache.Get( campusId );
                    if ( campus != null )
                    {
                        campusGuids.Add( campus.Guid );
                    }
                }

                var selectedCampuses = campusGuids.Select( s => s.ToString() ).ToList().AsDelimited( "," );
                // convert the date range from pipe-delimited to comma since we use pipe delimited for the selection values
                var dateRangeCommaDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( '|', ',' );
                return string.Format( "{0}|{1}|{2}|{3}", selectedCampuses, ddlIntegerCompare.SelectedValue, tbAttendedCount.Text, dateRangeCommaDelimitedValues );
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var campusesPicker = controls[0] as CampusesPicker;
            var ddlIntegerCompare = controls[1] as DropDownList;
            var tbAttendedCount = controls[2] as RockTextBox;
            var slidingDateRangePicker = controls[3] as SlidingDateRangePicker;

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                var campusGuidList = options[0].Split( ',' ).AsGuidList();
                List<int> campusIds = new List<int>();
                foreach ( var campusGuid in campusGuidList )
                {
                    var campus = CampusCache.Get( campusGuid );
                    if ( campus != null )
                    {
                        campusIds.Add( campus.Id );
                    }
                }

                campusesPicker.SelectedCampusIds = campusIds;
                ddlIntegerCompare.SelectedValue = options[1];
                tbAttendedCount.Text = options[2];
                var lastXWeeks = options[3].AsIntegerOrNull();
                if ( lastXWeeks.HasValue )
                {
                    //// selection was from when it just simply a LastXWeeks instead of Sliding Date Range
                    // Last X Weeks was treated as "LastXWeeks * 7" days, so we have to convert it to a SlidingDateRange of Days to keep consistent behavior
                    slidingDateRangePicker.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Last;
                    slidingDateRangePicker.TimeUnit = SlidingDateRangePicker.TimeUnitType.Day;
                    slidingDateRangePicker.NumberOfTimeUnits = lastXWeeks.Value * 7;
                }
                else
                {
                    // convert from comma-delimited to pipe since we store it as comma delimited so that we can use pipe delimited for the selection values
                    var dateRangeCommaDelimitedValues = options[3];
                    string slidingDelimitedValues = dateRangeCommaDelimitedValues.Replace( ',', '|' );
                    slidingDateRangePicker.DelimitedValues = slidingDelimitedValues;
                }
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
            string[] options = selection.Split( '|' );
            if ( options.Length < 4 )
            {
                return null;
            }

            Guid groupTypeGuid = options[0].AsGuid();

            var campusGuidList = options[0].Split( ',' ).AsGuidList();
            List<int> campusIds = new List<int>();
            foreach ( var campusGuid in campusGuidList )
            {
                var campus = CampusCache.Get( campusGuid );
                if ( campus != null )
                {
                    campusIds.Add( campus.Id );
                }
            }

            if ( !campusIds.Any() )
            {
                return null;
            }

            ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
            int? attended = options[2].AsIntegerOrNull();
            string slidingDelimitedValues;

            if ( options[3].AsIntegerOrNull().HasValue )
            {
                //// selection was from when it just simply a LastXWeeks instead of Sliding Date Range
                // Last X Weeks was treated as "LastXWeeks * 7" days, so we have to convert it to a SlidingDateRange of Days to keep consistent behavior
                int lastXWeeks = options[3].AsIntegerOrNull() ?? 1;
                var fakeSlidingDateRangePicker = new SlidingDateRangePicker();
                fakeSlidingDateRangePicker.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Last;
                fakeSlidingDateRangePicker.TimeUnit = SlidingDateRangePicker.TimeUnitType.Day;
                fakeSlidingDateRangePicker.NumberOfTimeUnits = lastXWeeks * 7;
                slidingDelimitedValues = fakeSlidingDateRangePicker.DelimitedValues;
            }
            else
            {
                slidingDelimitedValues = options[3].Replace( ',', '|' );
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );

            var rockContext = serviceInstance.Context as RockContext;
            var attendanceQry = new AttendanceService( rockContext ).Queryable().Where( a => a.DidAttend.HasValue && a.DidAttend.Value );
            attendanceQry = attendanceQry.Where( a => a.CampusId.HasValue && campusIds.Contains( (int)a.CampusId ) );

            if ( dateRange.Start.HasValue )
            {
                var startDate = dateRange.Start.Value;
                attendanceQry = attendanceQry.Where( a => a.Occurrence.OccurrenceDate >= startDate );
            }

            if ( dateRange.End.HasValue )
            {
                var endDate = dateRange.End.Value;
                attendanceQry = attendanceQry.Where( a => a.Occurrence.OccurrenceDate < endDate );
            }

            var qry = new PersonService( rockContext ).Queryable()
                  .Where( p => attendanceQry.Count( xx => xx.PersonAlias.PersonId == p.Id ) == attended );

            var compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" ) as BinaryExpression;
            var result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );

            return result;
        }

        #endregion
    }
}
