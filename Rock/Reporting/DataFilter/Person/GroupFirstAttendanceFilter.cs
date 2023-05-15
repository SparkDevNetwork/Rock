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
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people on whether they have attended the selected group(s) for the first time" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Group First Attendance Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "23B928C8-8BA9-43EC-A6B9-D1797E71E59F" )]
    public class GroupFirstAttendanceFilter : DataFilterComponent
    {
        private class GroupFirstAttendanceFilterSelection
        {
            public List<Guid> GroupGuids { get; set; } = new List<Guid>();
            public string IntegerCompare { get; set; }
            public int AttendedCount { get; set; }
            public string SlidingDateRange { get; set; }
            public bool IncludeChildGroups { get; set; }
            public List<int> Schedules { get; set; } = new List<int>();
        }

        #region Properties

        /// <inheritdoc/>
        public override string AppliesToEntityType
        {
            get
            {
                return "Rock.Model.Person";
            }
        }

        /// <inheritdoc/>
        public override string Section
        {
            get
            {
                return "Attendance";
            }
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "First Attendance in Groups";
        }

        /// <inheritdoc/>
        public override string GetClientFormatSelection( Type entityType )
        {
            return "'Attended ' + " +
                "'\\'' + $('.js-group-picker', $content).find('.selected-names').text() + '\\' ' + " +
                " ($('.js-child-groups', $content).is(':checked') ? '( or child groups) ' : '') + " +
                "$('.js-filter-compare', $content).find(':selected').text() + ' ' + " +
                "$('.js-attended-count', $content).val() + ' times. Date Range: ' + " +
                "$('.js-slidingdaterange-text-value', $content).val()";
        }

        /// <inheritdoc/>
        public override string FormatSelection( Type entityType, string selection )
        {
            var selectionOutput = "Attendance";
            var groupFirstAttendanceFilterSelection = GetGroupFirstAttendanceFilterSelection( selection );

            var groupsList = string.Empty;
            var selectedSchedules = string.Empty;
            using ( var rockContext = new RockContext() )
            {
                groupsList = new GroupService( rockContext )
                    .GetByGuids( groupFirstAttendanceFilterSelection.GroupGuids )
                    .Select( a => a.Name )
                    .ToList()
                    .AsDelimited( ", ", " or " );

                selectedSchedules = new ScheduleService( rockContext )
                    .GetByIds( groupFirstAttendanceFilterSelection.Schedules )
                    .Select( x => x.Name )
                    .ToList()
                    .AsDelimited( ", ", " or " );
            }

            if ( groupsList.IsNullOrWhiteSpace() )
            {
                groupsList = "?";
            }
            else if ( groupFirstAttendanceFilterSelection.IncludeChildGroups )
            {
                groupsList += " (or child groups)";
            }

            if ( selectedSchedules.IsNotNullOrWhiteSpace() )
            {
                selectedSchedules = $"on {selectedSchedules} ";
            }

            var comparisonType = groupFirstAttendanceFilterSelection.IntegerCompare.ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );

            string dateRangeText = SlidingDateRangePicker.FormatDelimitedValues( groupFirstAttendanceFilterSelection.SlidingDateRange );

             selectionOutput = $"Attended '{groupsList}' for the first time in the Date Range: {dateRangeText}";

            return selectionOutput;
        }

        /// <inheritdoc/>
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

            var defaultGroupFirstAttendanceFilterSelection = new GroupFirstAttendanceFilterSelection
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
                defaultGroupFirstAttendanceFilterSelection.ToJson() );

            return controls;
        }

        /// <inheritdoc/>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            var pGroupPicker = controls[0] as GroupPicker;
            var cbChildGroups = controls[1] as RockCheckBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;
            var schedulePicker = controls[5] as SchedulePicker;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            pGroupPicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbChildGroups.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 3
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-7" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            slidingDateRangePicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 4
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            schedulePicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <inheritdoc/>
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

            var groupFirstAttendanceFilterSelection = new GroupFirstAttendanceFilterSelection
            {
                GroupGuids = groupGuids,
                IntegerCompare = ddlIntegerCompare.SelectedValue,
                AttendedCount = tbAttendedCount.Text.AsInteger(),
                SlidingDateRange = slidingDateRangePicker.DelimitedValues,
                IncludeChildGroups = cbChildGroups.Checked,

                // We have to eliminate zero, because the schedulePicker control adds a zero if no values are selected.
                Schedules = schedulePicker.SelectedValues.AsIntegerList().Where( x => x != 0 ).ToList(),
            };

            return groupFirstAttendanceFilterSelection.ToJson();
        }

        /// <inheritdoc/>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var groupFirstAttendanceFilterSelection = GetGroupFirstAttendanceFilterSelection( selection );

            var pGroupPicker = controls[0] as GroupPicker;
            var cbChildGroups = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;
            var schedulePicker = controls[5] as SchedulePicker;

            using ( var rockContext = new RockContext() )
            {
                var groups = new GroupService( rockContext ).GetByGuids( groupFirstAttendanceFilterSelection.GroupGuids );
                pGroupPicker.SetValues( groups );

                var schedules = new ScheduleService( rockContext ).GetByIds( groupFirstAttendanceFilterSelection.Schedules );
                schedulePicker.SetValues( schedules );
            }

            ddlIntegerCompare.SelectedValue = "256";
            tbAttendedCount.Text = "1";
            slidingDateRangePicker.DelimitedValues = groupFirstAttendanceFilterSelection.SlidingDateRange;
            cbChildGroups.Checked = groupFirstAttendanceFilterSelection.IncludeChildGroups;
        }

        private GroupFirstAttendanceFilterSelection GetGroupFirstAttendanceFilterSelection( string selection )
        {
            var groupFirstAttendanceFilterSelection = selection.FromJsonOrNull<GroupFirstAttendanceFilterSelection>();

            if ( groupFirstAttendanceFilterSelection != null )
            {
                return groupFirstAttendanceFilterSelection;
            }

            groupFirstAttendanceFilterSelection = new GroupFirstAttendanceFilterSelection();

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                groupFirstAttendanceFilterSelection.GroupGuids = options[0].Split( ',' ).AsGuidList();
                groupFirstAttendanceFilterSelection.IntegerCompare = options[1];
                groupFirstAttendanceFilterSelection.AttendedCount = options[2].AsInteger();

                // convert from comma-delimited to pipe since we store it as comma delimited so that we can use pipe delimited for the selection values
                var dateRangeCommaDelimitedValues = options[3];
                string slidingDelimitedValues = dateRangeCommaDelimitedValues.Replace( ',', '|' );
                groupFirstAttendanceFilterSelection.SlidingDateRange = slidingDelimitedValues;

                if ( options.Length >= 5 )
                {
                    groupFirstAttendanceFilterSelection.IncludeChildGroups = options[4].AsBooleanOrNull() ?? false;
                }
            }

            return groupFirstAttendanceFilterSelection;
        }

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var groupFirstAttendanceFilterSelection = GetGroupFirstAttendanceFilterSelection( selection );
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( groupFirstAttendanceFilterSelection.SlidingDateRange );

            if ( groupFirstAttendanceFilterSelection.GroupGuids == null || groupFirstAttendanceFilterSelection.GroupGuids.Count == 0 )
            {
                // no groups selected, so return nothing
                return Expression.Constant( false );
            }

            var rockContext = serviceInstance.Context as RockContext;
            var attendanceBaseQry = new AttendanceService( rockContext ).Queryable().Where( a => a.DidAttend.HasValue && a.DidAttend.Value );

            var groupIds = GetGroupIds( groupFirstAttendanceFilterSelection.GroupGuids, groupFirstAttendanceFilterSelection.IncludeChildGroups );
            if ( groupIds.Count == 1 )
            {
                // if there is exactly one groupId we can avoid a 'Contains' (Contains has a small performance impact)
                int groupId = groupIds[0];
                attendanceBaseQry = attendanceBaseQry.Where( a => a.Occurrence.GroupId.HasValue && a.Occurrence.GroupId.Value == groupId );
            }
            else if ( groupIds.Count > 1 )
            {
                attendanceBaseQry = attendanceBaseQry.Where( a => a.Occurrence.GroupId.HasValue && groupIds.Contains( a.Occurrence.GroupId.Value ) );
            }

            if ( groupFirstAttendanceFilterSelection.Schedules.Any() )
            {
                attendanceBaseQry = attendanceBaseQry.Where( a => a.Occurrence.ScheduleId.HasValue && groupFirstAttendanceFilterSelection.Schedules.Contains( a.Occurrence.ScheduleId.Value ) );
            }

            var personAliasQry = new PersonAliasService( rockContext ).Queryable();
            var personQryForJoin = new PersonService( rockContext ).Queryable();

            // Create explicitly joins to person alias and person tables so that rendered SQL has an INNER Joins vs OUTER joins on PersonAlias
            var attendanceOccurrenceQry = attendanceBaseQry
                .Join( personAliasQry, a => a.PersonAliasId, pa => pa.Id, ( a, pa ) => new
                {
                    axn = a,
                    PersonId = pa.PersonId
                } );

            var firstAttendanceDataQry = attendanceOccurrenceQry
                .GroupBy( pa => pa.PersonId )
                .Select( fa => new
                {
                    PersonId = fa.Key,
                    FirstAttendanceDate = fa.Min( a => a.axn.Occurrence.OccurrenceDate )
                } );

            if ( dateRange.Start.HasValue )
            {
                firstAttendanceDataQry = firstAttendanceDataQry.Where( fa => fa.FirstAttendanceDate >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                firstAttendanceDataQry = firstAttendanceDataQry.Where( fa => fa.FirstAttendanceDate < dateRange.End.Value );
            }

            var innerQry = firstAttendanceDataQry.Select( fa => fa.PersonId ).AsQueryable();
            var qry = new PersonService( rockContext ).Queryable().Where( p => innerQry.Any( fa => fa == p.Id ) );

            return FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "p" );
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