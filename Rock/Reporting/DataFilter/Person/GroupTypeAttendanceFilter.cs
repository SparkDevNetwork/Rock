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
    [Description( "Filter people on whether they have attended a group type a specific number of times" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Group Type Attendance Filter" )]
    public class GroupTypeAttendanceFilter : DataFilterComponent
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
            return "Recent Attendance";
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
                "'\\'' + $('.js-group-type', $content).find(':selected').text() + '\\' ' + " +
                " ($('.js-child-group-types', $content).is(':checked') ? '( or child group types) ' : '') + " +
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
            string s = "Group Type Attendance";

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                var groupType = Rock.Web.Cache.GroupTypeCache.Read( options[0].AsGuid() );

                ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
                bool includeChildGroups = options.Length > 4 ? options[4].AsBoolean() : false;

                string dateRangeText;
                int? lastXWeeks = options[3].AsIntegerOrNull();
                if (lastXWeeks.HasValue)
                {
                    dateRangeText = " in last " + ( lastXWeeks.Value * 7 ).ToString() + " days";
                }
                else
                {
                    dateRangeText = SlidingDateRangePicker.FormatDelimitedValues( options[3].Replace( ',', '|' ) );
                }
                

                s = string.Format(
                    "Attended '{0}'{4} {1} {2} times. Date Range: {3}",
                    groupType != null ? groupType.Name : "?",
                    comparisonType.ConvertToString(),
                    options[2],
                    dateRangeText,
                    includeChildGroups ? " (or child group types) " : string.Empty );
            }

            return s;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var gtpGroupType = new GroupTypePicker();
            gtpGroupType.ID = filterControl.ID + "_0";
            gtpGroupType.AddCssClass( "js-group-type" );
            filterControl.Controls.Add( gtpGroupType );

            gtpGroupType.UseGuidAsValue = true;
            gtpGroupType.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().OrderBy(a => a.Name).ToList();

            var cbChildGroupTypes = new RockCheckBox();
            cbChildGroupTypes.ID = filterControl.ID + "_cbChildGroupTypes";
            cbChildGroupTypes.AddCssClass( "js-child-group-types" );
            cbChildGroupTypes.Text = "Include Child Group Types(s)";
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

            var controls = new Control[5] { gtpGroupType, cbChildGroupTypes, ddlIntegerCompare, tbAttendedCount, slidingDateRangePicker };

            // set the default values in case this is a newly added filter
            SetSelection(
                entityType,
                controls,
                string.Format( "{0}|{1}|4|Last,4,Week,,|false", gtpGroupType.Items.Count > 0 ? gtpGroupType.Items[0].Value : "0", ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ) );

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
            var gtpGroupType = controls[0] as GroupTypePicker;
            var cbChildGroupTypes = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            gtpGroupType.RenderControl( writer );
            writer.RenderEndTag();
            
            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbChildGroupTypes.RenderControl( writer );
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
            var gtpGroupType = controls[0] as GroupTypePicker;
            var cbChildGroupTypes = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;

            // convert the date range from pipe-delimited to comma since we use pipe delimited for the selection values
            var dateRangeCommaDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace('|', ',');
            return string.Format( "{0}|{1}|{2}|{3}|{4}", gtpGroupType.SelectedValue, ddlIntegerCompare.SelectedValue, tbAttendedCount.Text, dateRangeCommaDelimitedValues, cbChildGroupTypes.Checked.ToTrueFalse() );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var gtpGroupType = controls[0] as GroupTypePicker;
            var cbChildGroupTypes = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var tbAttendedCount = controls[3] as RockTextBox;
            var slidingDateRangePicker = controls[4] as SlidingDateRangePicker;

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                gtpGroupType.SelectedValue = options[0];
                ddlIntegerCompare.SelectedValue = options[1];
                tbAttendedCount.Text = options[2];
                int? lastXWeeks = options[3].AsIntegerOrNull();
                
                if (lastXWeeks.HasValue)
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

            var groupTypeService = new GroupTypeService( new RockContext() );

            var groupType = groupTypeService.Get( groupTypeGuid );
            List<int> groupTypeIds = new List<int>();
            if ( groupType != null )
            {
                groupTypeIds.Add( groupType.Id );

                if ( includeChildGroupTypes )
                {
                    var childGroupTypes = groupTypeService.GetAllAssociatedDescendents( groupType.Guid );
                    if ( childGroupTypes.Any() )
                    {
                        groupTypeIds.AddRange( childGroupTypes.Select( a => a.Id ) );

                        // get rid of any duplicates
                        groupTypeIds = groupTypeIds.Distinct().ToList();
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
                attendanceQry = attendanceQry.Where( a => a.EndDateTime < endDate );
            }

            if ( groupTypeIds.Count == 1 )
            {
                int groupTypeId = groupTypeIds[0];
                attendanceQry = attendanceQry.Where( a => a.Group.GroupTypeId == groupTypeId );
            }
            else if ( groupTypeIds.Count > 1 )
            {
                attendanceQry = attendanceQry.Where( a => groupTypeIds.Contains( a.Group.GroupTypeId ) );
            }
            else
            {
                // no group type selected, so return nothing
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