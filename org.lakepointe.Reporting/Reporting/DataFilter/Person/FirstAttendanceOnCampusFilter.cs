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
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace org.lakepointe.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people by their first attendance on a campus in a set of groups." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person First Attendance On Campus Filter" )]
    public class FirstAttendanceOnCampusFilter : DataFilterComponent
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
            return "First Attendance On Campus";
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
    var result = 'Campus';
    var campusPicker = $('.js-campus-picker', $content);
    var checkedCampus = $('.js-campus-picker', $content).find(':checked').closest('label');
    if (checkedCampus.length) {
        var campusCommaList = checkedCampus.map(function() { return $(this).text() }).get().join(',');
        result = 'Campus: ' + campusCommaList;
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
            string result = "First Attendance On Campus";

            string[] options = selection.Split( '|' );
            if ( options.Length >= 3 )
            {
                var campusName = CampusCache.Get( options[0].AsGuid() );
                var groupNames = new GroupService( new RockContext() ).GetByGuids( options[1].Split( ',' ).AsGuidList() ).Select( g => g.Name ).ToList();
                var dateRangeText = SlidingDateRangePicker.FormatDelimitedValues( options[2].Replace( ',', '|' ) );

                result = string.Format(
                    "First Attendance on the {0} campus in any of {1}. Date Range: {2}", campusName,
                    groupNames.AsDelimited( ", " ), dateRangeText );
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
            var campusPicker = new CampusPicker();
            campusPicker.Label = "Campus";
            campusPicker.ID = filterControl.ID + "_0";
            campusPicker.CssClass = "js-campus-picker campus-picker";
            campusPicker.Campuses = CampusCache.All();
            filterControl.Controls.Add( campusPicker );

            var groupPicker = new GroupPicker();
            groupPicker.AllowMultiSelect = true;
            groupPicker.AddCssClass( "js-group-picker" );
            groupPicker.ID = filterControl.ID + "_1";
            groupPicker.Label = "Groups";
            filterControl.Controls.Add( groupPicker );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            return new Control[3] { campusPicker, groupPicker, slidingDateRangePicker };
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
            var attendanceCampus = controls[0] as CampusPicker;
            var groupPicker = controls[1] as GroupPicker;
            var slidingDateRangePicker = controls[2] as SlidingDateRangePicker;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            attendanceCampus.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            groupPicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-12" );
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
            var campusId = ( controls[0] as CampusPicker ).SelectedCampusId;
            var gPicker = controls[1] as GroupPicker;
            var slidingDateRangePicker = controls[2] as SlidingDateRangePicker;

            if ( gPicker != null && campusId.HasValue && slidingDateRangePicker != null )
            {
                var selectedCampus = CampusCache.Get( campusId.Value ).Guid.ToString();
                var groupGuids = new GroupService( new RockContext() ).GetByIds( gPicker.ItemIds.AsIntegerList() ).Select( a => a.Guid ).ToList();
                var dateRangeCommaDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( '|', ',' );
                return string.Format( "{0}|{1}|{2}", selectedCampus, groupGuids.AsDelimited( "," ), dateRangeCommaDelimitedValues );
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
            var campusPicker = controls[0] as CampusPicker;
            var gPicker = controls[1] as GroupPicker;
            var slidingDateRangePicker = controls[2] as SlidingDateRangePicker;

            string[] options = selection.Split( '|' );
            if (options.Length >= 3)
            {
                campusPicker.SelectedCampusId = CampusCache.Get( options[0].AsGuid() ).Id;

                var groupGuids = options[1].Split( ',' ).AsGuidList();
                var groups = new GroupService( new RockContext() ).GetByGuids( groupGuids );
                gPicker.SetValues( groups );

                var dateRangeCommaDelimitedValues = options[2];
                string slidingDelimitedValues = dateRangeCommaDelimitedValues.Replace( ',', '|' );
                slidingDateRangePicker.DelimitedValues = slidingDelimitedValues;
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
            if ( options.Length < 3 )
            {
                return null;
            }

            var campusId = CampusCache.Get( options[0].AsGuid() ).Id;

            var rockContext = serviceInstance.Context as RockContext;
            var groupIds = new GroupService( rockContext ).GetByGuids( options[1].Split( ',' ).AsGuidList() ).Select( g => g.Id );

            var slidingDelimitedValues = options[2].Replace( ',', '|' );
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );

            var attendanceQry = new AttendanceService( rockContext ).Queryable( "Occurrence,Group" )
                .Where( a =>
                    a.DidAttend.HasValue &&
                    a.DidAttend.Value &&
                    a.CampusId.HasValue &&
                    a.CampusId.Value == campusId &&
                    groupIds.Contains( a.Occurrence.Group.Id )
                    );

            var qry = new PersonService( rockContext ).Queryable()
                  .Select( p => attendanceQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).OrderBy( xx => xx.StartDateTime ).FirstOrDefault() );

            if (dateRange.Start.HasValue)
            {
                var startDate = dateRange.Start.Value;
                qry = qry.Where( a => a.Occurrence.OccurrenceDate >= startDate );
            }

            if (dateRange.End.HasValue)
            {
                var endDate = dateRange.End.Value;
                qry = qry.Where( a => a.Occurrence.OccurrenceDate <= endDate );
            }

            var pIds = qry.Select( a => a.PersonAlias.PersonId );

            var people = new PersonService( rockContext ).Queryable()
                .Where( p => pIds.Contains( p.Id ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( people, parameterExpression, "p" );
        }

        #endregion
    }
}
