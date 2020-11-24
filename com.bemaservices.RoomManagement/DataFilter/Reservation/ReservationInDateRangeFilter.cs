// <copyright>
// Copyright by BEMA Software Services
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

using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.Web.UI.Controls;

using Rock;
using Rock.Data;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace com.bemaservices.RoomManagement.DataFilter.Reservation
{
    /// <summary>
    /// Filters reservations on a date range
    /// </summary>
    /// <seealso cref="Rock.Reporting.DataFilterComponent" />
    [Description( "Filter reservations on a date range" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Reservation In Date Range Filter" )]
    public class ReservationInDateRangeFilter : DataFilterComponent
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
            get { return typeof( com.bemaservices.RoomManagement.Model.Reservation ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
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
            return "Reservation in Date Range";
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
            return @"
function() {
    
    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val()
    var cblStates = $('.js-status-picker', $content);
    var statusNames = cblStates.find('.selected-names').text() 

   return 'Reservations with statuses ' + statusNames  + ' in DateRange: ' + dateRangeText; 
}
"; // TODO: add some form of 'starting in' thing
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Reservation In Date Range";
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 3 )
            {
                SlidingDateRangePicker fakeSlidingDateRangePicker = new SlidingDateRangePicker();

                if ( selectionValues.Length >= 4 )
                {
                    // convert comma delimited to pipe
                    fakeSlidingDateRangePicker.DelimitedValues = selectionValues[3].Replace( ',', '|' );
                }
                else
                {
                    // if converting from a previous version of the selection
                    var lowerValue = selectionValues[0].AsDateTime();
                    var upperValue = selectionValues[1].AsDateTime();

                    fakeSlidingDateRangePicker.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.DateRange;
                    fakeSlidingDateRangePicker.SetDateRangeModeValue( new DateRange( lowerValue, upperValue ) );
                }

                bool includeOnlyStarting = false;
                if ( selectionValues.Length >= 5 )
                {
                    includeOnlyStarting = selectionValues[4].AsBoolean();
                }

                var statesValues = selectionValues[2].Split( ',' ).ToList();
                var statesNames = statesValues.AsDelimited( "," );

                result = string.Format(
                    "Reservations{0} {1}in Date Range: {2}",
                    !string.IsNullOrWhiteSpace( statesNames ) ? " with approval states:" + statesNames : string.Empty,
                    includeOnlyStarting ? "Starting " : String.Empty,
                    SlidingDateRangePicker.FormatDelimitedValues( fakeSlidingDateRangePicker.DelimitedValues )
                    )
                    ;
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            RockCheckBoxList cblStates = new RockCheckBoxList();
            cblStates.BindToEnum<ReservationApprovalState>();
            cblStates.ID = filterControl.ID + "_cblStates";
            cblStates.AddCssClass( "js-status-picker" );
            cblStates.Label = "Reservation Approval States";
            cblStates.Help = "The approval states that will be included in the filter.";
            filterControl.Controls.Add( cblStates );

            SlidingDateRangePicker slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range of the reservations";
            slidingDateRangePicker.Required = true;
            filterControl.Controls.Add( slidingDateRangePicker );

            RockCheckBox checkbox = new RockCheckBox();
            checkbox.ID = filterControl.ID + "_checkbox";
            checkbox.AddCssClass( "js-checkbox" );
            checkbox.Label = "Include only reservations that start in the date range";
            checkbox.Required = true;
            filterControl.Controls.Add( checkbox );

            var controls = new Control[3] { cblStates, slidingDateRangePicker, checkbox };

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
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var cblStatus = controls[0] as RockCheckBoxList;

            SlidingDateRangePicker slidingDateRangePicker = controls[1] as SlidingDateRangePicker;

            CheckBox checkbox = controls[2] as CheckBox;

            // convert pipe to comma delimited
            var delimitedValues = slidingDateRangePicker.DelimitedValues.Replace( "|", "," );

            // {1} and {2} used to store the DateRange before, but now we using the SlidingDateRangePicker
            return string.Format( "{0}|{1}|{2}|{3}|{4}", string.Empty, string.Empty, cblStatus.SelectedValues.AsDelimited( "," ), delimitedValues, checkbox.Checked );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 3 )
            {
                RockCheckBoxList cblStates = new RockCheckBoxList();
                cblStates.BindToEnum<ReservationApprovalState>();
                var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;
                var checkbox = controls[2] as CheckBox;

                if ( selectionValues.Length >= 4 )
                {
                    // convert comma delimited to pipe
                    slidingDateRangePicker.DelimitedValues = selectionValues[3].Replace( ',', '|' );
                }
                else
                {
                    // if converting from a previous version of the selection
                    var lowerValue = selectionValues[0].AsDateTime();
                    var upperValue = selectionValues[1].AsDateTime();

                    slidingDateRangePicker.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.DateRange;
                    slidingDateRangePicker.SetDateRangeModeValue( new DateRange( lowerValue, upperValue ) );
                }

                if ( selectionValues.Length >= 5 )
                {
                    checkbox.Checked = selectionValues[4].AsBoolean();
                }

                var states = ( selectionValues[2] ?? "2" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                if ( states.Any() )
                {
                    cblStates.SetValues( states );
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
            var rockContext = (RockContext)serviceInstance.Context;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length < 3 )
            {
                return null;
            }

            DateRange dateRange;

            if ( selectionValues.Length >= 4 )
            {
                string slidingDelimitedValues = selectionValues[3].Replace( ',', '|' );
                dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );
            }
            else
            {
                // if converting from a previous version of the selection
                DateTime? startDate = selectionValues[0].AsDateTime();
                DateTime? endDate = selectionValues[1].AsDateTime();
                dateRange = new DateRange( startDate, endDate );

                if ( dateRange.End.HasValue )
                {
                    // the DateRange picker doesn't automatically add a full day to the end date
                    dateRange.End.Value.AddDays( 1 );
                }
            }

            bool includeOnlyStarting = false;
            if ( selectionValues.Length >= 5 )
            {
                includeOnlyStarting = selectionValues[4].AsBoolean();
            }

            var states = new List<ReservationApprovalState>();

            foreach ( string stateVal in ( selectionValues[2] ?? "2" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var state = stateVal.ConvertToEnumOrNull<ReservationApprovalState>();
                if ( state != null )
                {
                    states.Add( state.Value );
                }
            }

            var reservationQry = new ReservationService( rockContext ).Queryable();

            if ( states.Any() )
            {
                reservationQry = reservationQry.Where( xx => states.Contains( xx.ApprovalState ) );
            }

            var reservationIdList = new List<int>();
            var reservations = reservationQry.ToList();
            if ( includeOnlyStarting )
            {
                reservationIdList = reservations
                    .Select( r => new
                    {
                        Reservation = r,
                        ReservationDateTimes = r.GetReservationTimes( dateRange.Start ?? DateTime.MinValue, dateRange.End ?? DateTime.MaxValue )
                    } )
                    .Where( r => r.ReservationDateTimes.Any() )
                    .Select( r => r.Reservation.Id )
                    .ToList();

            }
            else
            {
                reservationIdList = reservations
                    .Select( r => new
                    {
                        Reservation = r,
                        ReservationDateTimes = r.GetReservationTimes( dateRange.Start.HasValue ? dateRange.Start.Value.AddMonths( -1 ) : DateTime.MinValue, dateRange.End.HasValue ? dateRange.End.Value.AddMonths( 1 ) : DateTime.MaxValue )
                    } )
                    .Where( r => r.ReservationDateTimes.Any( rdt =>
                        ( ( rdt.StartDateTime > ( dateRange.Start ?? DateTime.MinValue ) ) || ( rdt.EndDateTime > ( dateRange.Start ?? DateTime.MinValue ) ) ) &&
                        ( ( rdt.StartDateTime < ( dateRange.End ?? DateTime.MaxValue ) ) || ( rdt.EndDateTime < ( dateRange.End ?? DateTime.MaxValue ) ) ) )
                        )
                    .Select( r => r.Reservation.Id )
                    .ToList();
            }

            var qry = new ReservationService( rockContext ).Queryable()
                .Where( r => reservationIdList.Contains( r.Id ) );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<com.bemaservices.RoomManagement.Model.Reservation>( qry, parameterExpression, "r" );

            return extractedFilterExpression;
        }

        #endregion
    }
}