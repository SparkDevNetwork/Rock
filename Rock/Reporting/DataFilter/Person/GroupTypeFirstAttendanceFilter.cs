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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people on whether they have attended a group type for the first time" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Group Type First Attendance Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "5748F0E7-E033-4D36-819C-0AA3823A8935" )]
    public class GroupTypeFirstAttendanceFilter : DataFilterComponent
    {
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

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Obsidian/Reporting/DataFilters/Person/groupTypeFirstAttendanceFilter.obs";

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                var groupTypeGuids = options[0].Split( ',' ).AsGuidList();
                var groupTypes = groupTypeGuids
                    .Select( gt => GroupTypeCache.Get( gt )?.ToListItemBag() )
                    .Where( gt => gt != null )
                    .ToList();
                var dateRange = "";

                var lastXWeeks = options[3].AsIntegerOrNull();
                if ( lastXWeeks.HasValue )
                {
                    // selection was from when it just simply a LastXWeeks instead of Sliding Date Range
                    // Last X Weeks was treated as "LastXWeeks * 7" days, so we have to convert it to a SlidingDateRange of Days to keep consistent behavior
                    dateRange = $"Last|{lastXWeeks * 7}|Day||";
                }
                else
                {
                    // convert from comma-delimited to pipe since we store it as comma delimited so that we can use pipe delimited for the selection values
                    dateRange = options[3].Replace( ',', '|' );
                }

                data.AddOrReplace( "groupTypes", groupTypes.ToCamelCaseJson( false, true ) );
                data.AddOrReplace( "dateRange", dateRange );

                if ( options.Length >= 5 )
                {
                    var includeChildGroupTypes = options[4];

                    data.AddOrReplace( "includeChildGroupTypes", includeChildGroupTypes );
                }
            }

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var groupTypes = data.GetValueOrNull( "groupTypes" )?.FromJsonOrNull<List<ListItemBag>>() ?? new List<ListItemBag>();
            var groupTypeGuids = groupTypes.Select( a => a.Value ).ToList().AsDelimited( "," );
            var dateRange = data.GetValueOrDefault( "dateRange", "Last|7|Day||" ).Replace( '|', ',' );
            var includeChildGroupTypes = data.GetValueOrDefault( "includeChildGroupTypes", "False" );

            return $"{groupTypeGuids}|256|1|{dateRange}|{includeChildGroupTypes}";
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "First Attendance in Group Types";
        }

        /// <inheritdoc/>
        public override string GetClientFormatSelection( Type entityType )
        {
            return "'Attended ' + " +
                "'\\'' + $('.js-group-type', $content).find(':selected').text() + '\\' ' + " +
                " ($('.js-child-group-types', $content).is(':checked') ? '( or child group types) ' : '') + " +
                "$('.js-filter-compare', $content).find(':selected').text() + ' ' + " +
                "$('.js-attended-count', $content).val() + ' times. Date Range: ' + " +
                "$('.js-slidingdaterange-text-value', $content).val()";
        }

        /// <inheritdoc/>
        public override string FormatSelection( Type entityType, string selection )
        {
            string filterName = "Group Type First Attendance";

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                var groupType = string.Empty;
                string[] listOfGroupTypeIds = options[0].Split( ',' );

                foreach ( string groupTypeId in listOfGroupTypeIds )
                {
                    groupType += $" {GroupTypeCache.Get( groupTypeId.AsGuid() )},";
                }

                groupType = groupType.TrimStart( ' ' );
                groupType = groupType.TrimEnd( ',' );

                ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
                bool includeChildGroups = options.Length > 4 ? options[4].AsBoolean() : false;

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

                filterName = string.Format(
                    "Attended '{0}' Group Type {2} for the first time in the Date Range: {1}",
                    groupType ?? "?",
                    dateRangeText,
                    includeChildGroups ? " (or child group types) " : string.Empty );
            }

            return filterName;
        }

        /// <inheritdoc/>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var gtpGroupsTypes = new GroupTypesPicker();
            gtpGroupsTypes.ID = filterControl.ID + "_0";
            gtpGroupsTypes.AddCssClass( "js-group-type" );
            gtpGroupsTypes.RepeatDirection = RepeatDirection.Horizontal;
            gtpGroupsTypes.UseGuidAsValue = true;
            gtpGroupsTypes.IsSortedByName = true;
            gtpGroupsTypes.SetGroupTypes( GroupTypeCache.All() );
            filterControl.Controls.Add( gtpGroupsTypes );

            var cbChildGroupTypes = new RockCheckBox();
            cbChildGroupTypes.ID = filterControl.ID + "_cbChildGroupTypes";
            cbChildGroupTypes.AddCssClass( "js-child-group-types" );
            cbChildGroupTypes.Text = "Include Child Group Type(s)";
            filterControl.Controls.Add( cbChildGroupTypes );

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

            var controls = new Control[5] { gtpGroupsTypes, cbChildGroupTypes, ddlIntegerCompare, tbAttendedCount, slidingDateRangePicker };

            // convert pipe to comma delimited
            var defaultDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( "|", "," );
            var defaultCount = 4;

            var groupValues = string.Empty;
            foreach ( Guid groupTypeOutput in gtpGroupsTypes.SelectedGroupTypeGuids )
            {
                groupValues += $"{groupTypeOutput},";
            }

            groupValues = groupValues.TrimEnd( ',' );

            // set the default values in case this is a newly added filter
            SetSelection(
                entityType,
                controls,
                string.Format( "{0}|{1}|{2}|{3}|false", groupValues, ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString(), defaultCount, defaultDelimitedValues ) );

            return controls;
        }

        /// <inheritdoc/>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            var gtpGroupsTypes = controls[0] as GroupTypesPicker;
            var cbChildGroupTypes = controls[1] as RockCheckBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            gtpGroupsTypes.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbChildGroupTypes.RenderControl( writer );
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
        }

        /// <inheritdoc/>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var gtpGroupsTypes = controls[0] as GroupTypesPicker;
            var cbChildGroupTypes = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;

            var groupValues = string.Empty;
            foreach ( Guid groupTypeOutput in gtpGroupsTypes.SelectedGroupTypeGuids )
            {
                groupValues += $"{groupTypeOutput},";
            }

            groupValues = groupValues.TrimEnd( ',' );

            // convert the date range from pipe-delimited to comma since we use pipe delimited for the selection values
            var dateRangeCommaDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( '|', ',' );
            return string.Format( "{0}|256|1|{3}|{4}", groupValues, ddlIntegerCompare.SelectedValue, tbAttendedCount.Text, dateRangeCommaDelimitedValues, cbChildGroupTypes.Checked.ToTrueFalse() );
        }

        /// <inheritdoc/>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var gtpGroupsTypes = controls[0] as GroupTypesPicker;
            var cbChildGroupTypes = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                gtpGroupsTypes.SelectedGroupTypeGuids = options[0].Split( ',' ).AsGuidList();
                ddlIntegerCompare.SelectedValue = options[1];
                tbAttendedCount.Text = options[2];
                int? lastXWeeks = options[3].AsIntegerOrNull();

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

                if ( options.Length >= 5 )
                {
                    cbChildGroupTypes.Checked = options[4].AsBooleanOrNull() ?? false;
                }
            }
        }

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] options = selection.Split( '|' );
            if ( options.Length < 4 )
            {
                return null;
            }

            string[] listOfGroupTypeIds = options[0].Split( ',' );
            var listOfGuidTypes = new List<Guid>();
            foreach ( string groupTypeId in listOfGroupTypeIds )
            {
                listOfGuidTypes.Add( groupTypeId.AsGuid() );
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

            bool includeChildGroupTypes = options.Length >= 5 ? options[4].AsBooleanOrNull() ?? false : false;

            var groupTypeIds = new List<int?>();
            if ( listOfGuidTypes != null )
            {
                foreach ( Guid guid in listOfGuidTypes )
                {
                    groupTypeIds.Add( GroupTypeCache.GetId( guid ) );
                }
            }

            var rockContext = serviceInstance.Context as RockContext;
            var attendanceBaseQry = new AttendanceService( rockContext ).Queryable().Where( a => a.DidAttend.HasValue && a.DidAttend.Value );


            if ( groupTypeIds.Count == 1 )
            {
                int? groupTypeId = groupTypeIds[0];
                attendanceBaseQry = attendanceBaseQry.Where( a => a.Occurrence.Group.GroupTypeId == groupTypeId );
            }
            else if ( groupTypeIds.Count > 1 )
            {
                attendanceBaseQry = attendanceBaseQry.Where( a => groupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) );
            }

            var personAliasQry = new PersonAliasService( rockContext ).Queryable();
            var personQryForJoin = new PersonService( rockContext ).Queryable();

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

        #endregion
    }
}