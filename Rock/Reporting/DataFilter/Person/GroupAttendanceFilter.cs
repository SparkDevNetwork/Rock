// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether they have attended the selected group(s) a specified number of times" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Group Attendance Filter" )]
    public class GroupAttendanceFilter : DataFilterComponent
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
            get { return "Rock.Model.Person"; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Group Attendance"; }
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
            return "Attendance in Groups";
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
            return "'Attended ' + " +
                "'\\'' + $('.js-group-picker', $content).find('.selected-names').text() + '\\' ' + " +
                " ($('.js-child-groups', $content).is(':checked') ? '( or child groups) ' : '') + " +
                "$('.js-filter-compare', $content).find(':selected').text() + ' ' + " +
                "$('.js-attended-count', $content).val() + ' times. Date Range: ' + " +
                "$('.js-slidingdaterange-text-value', $content).val()";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string s = "Attendance";

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                var groupsList = new GroupService( new RockContext() ).GetByGuids( options[0].Split( ',' ).AsGuidList() )
                    .Select( a => a.Name ).ToList().AsDelimited( ", ", " or " );

                ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
                bool includeChildGroups = options.Length > 4 ? options[4].AsBoolean() : false;

                string dateRangeText = SlidingDateRangePicker.FormatDelimitedValues( options[3].Replace( ',', '|' ) );

                s = string.Format(
                    "Attended '{0}'{4} {1} {2} times. Date Range: {3}",
                    groupsList != null ? groupsList : "?",
                    comparisonType.ConvertToString(),
                    options[2],
                    dateRangeText,
                    includeChildGroups ? " (or child groups) " : string.Empty );
            }

            return s;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var pGroupPicker = new GroupPicker();
            pGroupPicker.AllowMultiSelect = true;
            pGroupPicker.ID = filterControl.ID + "_pGroupPicker";
            pGroupPicker.AddCssClass( "js-group-picker" );
            filterControl.Controls.Add( pGroupPicker );

            var cbChildGroups = new RockCheckBox();
            cbChildGroups.ID = filterControl.ID + "_cbChildGroups";
            cbChildGroups.AddCssClass( "js-child-groups" );
            cbChildGroups.Text = "Include Child Groups";
            filterControl.Controls.Add( cbChildGroups );

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

            var controls = new Control[5] { pGroupPicker, cbChildGroups, ddlIntegerCompare, tbAttendedCount, slidingDateRangePicker };

            // set the default values in case this is a newly added filter
            SetSelection(
                entityType,
                controls,
                string.Format( "{0}|{1}|4|Last,4,Week,,|false", string.Empty, ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ) );

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
            var pGroupPicker = controls[0] as GroupPicker;
            var cbChildGroups = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            pGroupPicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbChildGroups.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 3
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
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
            var pGroupPicker = controls[0] as GroupPicker;
            var cbChildGroups = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;

            // convert the date range from pipe-delimited to comma since we use pipe delimited for the selection values
            var dateRangeCommaDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( '|', ',' );
            var groupGuids = new GroupService( new RockContext() ).GetByIds( pGroupPicker.ItemIds.AsIntegerList() ).Select( a => a.Guid ).ToList();
            return string.Format( "{0}|{1}|{2}|{3}|{4}", groupGuids.AsDelimited( "," ), ddlIntegerCompare.SelectedValue, tbAttendedCount.Text, dateRangeCommaDelimitedValues, cbChildGroups.Checked.ToTrueFalse() );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var pGroupPicker = controls[0] as GroupPicker;
            var cbChildGroups = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                var groupGuids = options[0].Split( ',' ).AsGuidList();
                var groups = new GroupService( new RockContext() ).GetByGuids( groupGuids );
                pGroupPicker.SetValues( groups );
                ddlIntegerCompare.SelectedValue = options[1];
                tbAttendedCount.Text = options[2];

                // convert from comma-delimited to pipe since we store it as comma delimited so that we can use pipe delimited for the selection values
                var dateRangeCommaDelimitedValues = options[3];
                string slidingDelimitedValues = dateRangeCommaDelimitedValues.Replace( ',', '|' );
                slidingDateRangePicker.DelimitedValues = slidingDelimitedValues;

                if ( options.Length >= 5 )
                {
                    cbChildGroups.Checked = options[4].AsBooleanOrNull() ?? false;
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

            var groupGuidList = options[0].Split( ',' ).AsGuidList();
            ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
            int? attended = options[2].AsIntegerOrNull();
            string slidingDelimitedValues = options[3].Replace( ',', '|' );
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );

            bool includeChildGroups = options.Length >= 5 ? options[4].AsBooleanOrNull() ?? false : false;

            var groupService = new GroupService( new RockContext() );

            var groups = groupService.GetByGuids( groupGuidList );
            List<int> groupIds = new List<int>();
            foreach ( var group in groups )
            {
                groupIds.Add( group.Id );

                if ( includeChildGroups )
                {
                    var childGroups = groupService.GetAllDescendents( group.Id );
                    if ( childGroups.Any() )
                    {
                        groupIds.AddRange( childGroups.Select( a => a.Id ) );

                        // get rid of any duplicates
                        groupIds = groupIds.Distinct().ToList();
                    }
                }
            }

            var rockContext = serviceInstance.Context as RockContext;
            var attendanceQry = new AttendanceService( rockContext ).Queryable().Where( a => a.DidAttend.HasValue && a.DidAttend.Value );

            if ( dateRange.Start.HasValue )
            {
                var startDate = dateRange.Start.Value;
                attendanceQry = attendanceQry.Where( a => a.StartDateTime >= startDate );
            }

            if ( dateRange.End.HasValue )
            {
                var endDate = dateRange.End.Value;
                attendanceQry = attendanceQry.Where( a => a.StartDateTime < endDate );
            }

            if ( groupIds.Count == 1 )
            {
                int groupId = groupIds[0];
                attendanceQry = attendanceQry.Where( a => a.GroupId.HasValue && a.GroupId.Value == groupId );
            }
            else if ( groupIds.Count > 1 )
            {
                attendanceQry = attendanceQry.Where( a => a.GroupId.HasValue && groupIds.Contains( a.GroupId.Value ) );
            }
            else
            {
                // no groups selected, so return nothing
                return Expression.Constant( false );
            }

            var qry = new PersonService( rockContext ).Queryable()
                  .Where( p => attendanceQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() == attended );

            BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" ) as BinaryExpression;
            BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );

            return result;
        }

        #endregion
    }
}