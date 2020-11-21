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
using Newtonsoft.Json;
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
        private class GroupAttendanceFilterSelection
        {
            public List<Guid> GroupGuids { get; set; } = new List<Guid>();
            public string IntegerCompare { get; set; }
            public int AttendedCount { get; set; }
            public string SlidingDateRange { get; set; }
            public bool IncludeChildGroups { get; set; }
            public List<int> Schedules { get; set; } = new List<int>();
        }

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
            var selectionOutput = "Attendance";
            var groupAttendanceFilterSelection = GetGroupAttendanceFilterSelection( selection );

            var groupsList = "";
            var selectedSchedules = "";
            using ( var rockContext = new RockContext() )
            {
                groupsList = new GroupService( rockContext )
                    .GetByGuids( groupAttendanceFilterSelection.GroupGuids )
                    .Select( a => a.Name )
                    .ToList()
                    .AsDelimited( ", ", " or " );

                selectedSchedules = new ScheduleService( rockContext )
                    .GetByIds( groupAttendanceFilterSelection.Schedules )
                    .Select(x => x.Name)
                    .ToList()
                    .AsDelimited( ", ", " or " );
            }

            if ( groupsList.IsNullOrWhiteSpace() )
            {
                groupsList = "?";
            } else if ( groupAttendanceFilterSelection.IncludeChildGroups )
            {
                groupsList += " (or child groups)";
            }

            if ( selectedSchedules.IsNotNullOrWhiteSpace() )
            {
                selectedSchedules = $"on {selectedSchedules} ";
            }

            var comparisonType = groupAttendanceFilterSelection.IntegerCompare.ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );

            string dateRangeText = SlidingDateRangePicker.FormatDelimitedValues( groupAttendanceFilterSelection.SlidingDateRange );

            selectionOutput = $"Attended '{groupsList}' {selectedSchedules}{comparisonType.ConvertToString()} {groupAttendanceFilterSelection.AttendedCount} times. Date Range: {dateRangeText}";

            return selectionOutput;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var pGroupPicker = new GroupPicker();
            pGroupPicker.AllowMultiSelect = true;
            pGroupPicker.ID = $"{filterControl.ID}_{nameof( pGroupPicker )}";
            pGroupPicker.AddCssClass( "js-group-picker" );
            filterControl.Controls.Add( pGroupPicker );

            var cbChildGroups = new RockCheckBox();
            cbChildGroups.ID = $"{filterControl.ID}_{nameof( cbChildGroups )}";
            cbChildGroups.AddCssClass( "js-child-groups" );
            cbChildGroups.Text = "Include Child Groups";
            filterControl.Controls.Add( cbChildGroups );

            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes );
            ddlIntegerCompare.Label = "Attendance Count";
            ddlIntegerCompare.ID = $"{filterControl.ID}_{nameof( ddlIntegerCompare )}";
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );

            var tbAttendedCount = new RockTextBox();
            tbAttendedCount.ID = $"{filterControl.ID}_{nameof( tbAttendedCount )}";
            tbAttendedCount.Label = "&nbsp;"; // give it whitespace label so it lines up nicely
            tbAttendedCount.AddCssClass( "js-attended-count" );
            filterControl.Controls.Add( tbAttendedCount );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = $"{filterControl.ID}_{nameof( slidingDateRangePicker )}";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            var schedulePicker = new SchedulePicker();
            schedulePicker.Label = "Schedules";
            schedulePicker.ID = $"{filterControl.ID}_{nameof( schedulePicker )}";
            schedulePicker.AddCssClass( "js-schedule-picker" );
            schedulePicker.AllowMultiSelect = true;
            filterControl.Controls.Add( schedulePicker );

            var controls = new Control[6] { pGroupPicker, cbChildGroups, ddlIntegerCompare, tbAttendedCount, slidingDateRangePicker, schedulePicker };

            var defaultGroupAttendanceFilterSelection = new GroupAttendanceFilterSelection
            {
                IntegerCompare = ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString(),
                AttendedCount = 4,
                SlidingDateRange = slidingDateRangePicker.DelimitedValues,
                IncludeChildGroups = false,
            };

            // set the default values in case this is a newly added filter
            SetSelection(
                entityType,
                controls,
                defaultGroupAttendanceFilterSelection.ToJson() );

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
            var schedulePicker = controls[5] as SchedulePicker;

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

            // Row 4
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            schedulePicker.RenderControl( writer );
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
            var schedulePicker = controls[5] as SchedulePicker;

            // convert the date range from pipe-delimited to comma since we use pipe delimited for the selection values
            var dateRangeCommaDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( '|', ',' );
            var groupGuids = new GroupService( new RockContext() ).GetByIds( pGroupPicker.ItemIds.AsIntegerList() ).Select( a => a.Guid ).ToList();

            var groupAttendanceFilterSelection = new GroupAttendanceFilterSelection
            {
                GroupGuids = groupGuids,
                IntegerCompare = ddlIntegerCompare.SelectedValue,
                AttendedCount = tbAttendedCount.Text.AsInteger(),
                SlidingDateRange = slidingDateRangePicker.DelimitedValues,
                IncludeChildGroups = cbChildGroups.Checked,
                // We have to eliminate zero, because the schedulePicker control adds a zero if no values are selected.
                Schedules = schedulePicker.SelectedValues.AsIntegerList().Where(x => x != 0).ToList(),
            };

            return groupAttendanceFilterSelection.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var groupAttendanceFilterSelection = GetGroupAttendanceFilterSelection( selection );

            var pGroupPicker = controls[0] as GroupPicker;
            var cbChildGroups = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;
            var schedulePicker = controls[5] as SchedulePicker;

            using ( var rockContext = new RockContext() )
            {
                var groups = new GroupService( rockContext ).GetByGuids( groupAttendanceFilterSelection.GroupGuids );
                pGroupPicker.SetValues( groups );

                var schedules = new ScheduleService( rockContext ).GetByIds( groupAttendanceFilterSelection.Schedules );
                schedulePicker.SetValues( schedules );
            }

            ddlIntegerCompare.SelectedValue = groupAttendanceFilterSelection.IntegerCompare;
            tbAttendedCount.Text = groupAttendanceFilterSelection.AttendedCount.ToString();
            slidingDateRangePicker.DelimitedValues = groupAttendanceFilterSelection.SlidingDateRange;
            cbChildGroups.Checked = groupAttendanceFilterSelection.IncludeChildGroups;
        }

        private GroupAttendanceFilterSelection GetGroupAttendanceFilterSelection( string selection )
        {
            var groupAttendanceFilterSelection = selection.FromJsonOrNull<GroupAttendanceFilterSelection>();

            if ( groupAttendanceFilterSelection != null )
            {
                return groupAttendanceFilterSelection;
            }

            groupAttendanceFilterSelection = new GroupAttendanceFilterSelection();

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                groupAttendanceFilterSelection.GroupGuids = options[0].Split( ',' ).AsGuidList();
                groupAttendanceFilterSelection.IntegerCompare = options[1];
                groupAttendanceFilterSelection.AttendedCount = options[2].AsInteger();

                // convert from comma-delimited to pipe since we store it as comma delimited so that we can use pipe delimited for the selection values
                var dateRangeCommaDelimitedValues = options[3];
                string slidingDelimitedValues = dateRangeCommaDelimitedValues.Replace( ',', '|' );
                groupAttendanceFilterSelection.SlidingDateRange = slidingDelimitedValues;

                if ( options.Length >= 5 )
                {
                    groupAttendanceFilterSelection.IncludeChildGroups = options[4].AsBooleanOrNull() ?? false;
                }
            }

            return groupAttendanceFilterSelection;
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
            var groupAttendanceFilterSelection = GetGroupAttendanceFilterSelection( selection );

            if ( groupAttendanceFilterSelection.GroupGuids == null || groupAttendanceFilterSelection.GroupGuids.Count == 0 )
            {
                // no groups selected, so return nothing
                return Expression.Constant( false );
            }

            var rockContext = serviceInstance.Context as RockContext;
            var attendanceQry = new AttendanceService( rockContext ).Queryable().Where( a => a.DidAttend.HasValue && a.DidAttend.Value );

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( groupAttendanceFilterSelection.SlidingDateRange );
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

            var groupIds = GetGroupIds( groupAttendanceFilterSelection.GroupGuids, groupAttendanceFilterSelection.IncludeChildGroups );
            if ( groupIds.Count == 1 )
            {
                // if there is exactly one groupId we can avoid a 'Contains' (Contains has a small performance impact)
                int groupId = groupIds[0];
                attendanceQry = attendanceQry.Where( a => a.Occurrence.GroupId.HasValue && a.Occurrence.GroupId.Value == groupId );
            }
            else if ( groupIds.Count > 1 )
            {
                attendanceQry = attendanceQry.Where( a => a.Occurrence.GroupId.HasValue && groupIds.Contains( a.Occurrence.GroupId.Value ) );
            }

            if ( groupAttendanceFilterSelection.Schedules.Any() )
            {
                attendanceQry = attendanceQry.Where( a => a.Occurrence.ScheduleId.HasValue && groupAttendanceFilterSelection.Schedules.Contains( a.Occurrence.ScheduleId.Value ) );
            }

            var qry = new PersonService( rockContext )
                .Queryable()
                .Where( p => attendanceQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() == groupAttendanceFilterSelection.AttendedCount );

            var compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" ) as BinaryExpression;
            var comparisonType = groupAttendanceFilterSelection.IntegerCompare.ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
            var result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );

            return result;
        }

        private List<int> GetGroupIds( List<Guid> groupGuids, bool includeChildGroups )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );

                var groups = groupService.GetByGuids( groupGuids );
                var groupIds = new List<int>( groups.Count() );
                foreach ( var group in groups )
                {
                    groupIds.Add( group.Id );

                    if ( includeChildGroups )
                    {
                        var childGroupIds = groupService.GetAllDescendentGroupIds( group.Id, false );
                        if ( childGroupIds.Any() )
                        {
                            groupIds.AddRange( childGroupIds );
                        }
                    }
                }

                return groupIds.Distinct().ToList();
            }
        }

        #endregion
    }
}