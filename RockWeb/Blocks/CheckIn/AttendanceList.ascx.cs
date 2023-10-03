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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Checkin
{
    /// <summary>
    /// Block for displaying the attendance list of a group with schedule on selected date.
    /// </summary>
    [DisplayName( "Attendance List" )]
    [Category( "Check-in" )]
    [Description( "Block for displaying the attendance history of a person or a group." )]
    [Rock.SystemGuid.BlockTypeGuid( "678ED4B6-D76F-4D43-B069-659E352C9BD8" )]
    public partial class AttendanceList : RockBlock, ICustomGridColumns
    {

        #region UserPreferenceKeys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKey
        {
            /// <summary>
            /// The attended
            /// </summary>
            public const string Attended = "Attended";

            /// <summary>
            /// The entered by
            /// </summary>
            public const string EnteredBy = "Entered By";
        }

        #endregion UserPreferanceKeys

        #region PageParameterKeys

        /// <summary>
        /// A defined list of page parameter keys used by this block.
        /// </summary>
        protected static class PageParameterKey
        {
            /// <summary>
            /// The schedule identifier
            /// </summary>
            public const string ScheduleId = "ScheduleId";

            /// <summary>
            /// The location identifier
            /// </summary>
            public const string LocationId = "LocationId";

            /// <summary>
            /// The group identifier
            /// </summary>
            public const string GroupId = "GroupId";

            /// <summary>
            /// The attendance date
            /// </summary>
            public const string AttendanceDate = "AttendanceDate";
        }

        #endregion PageParameterKeys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gAttendees.GridRebind += gAttendees_GridRebind;

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.ClearFilterClick += rFilter_ClearFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                bool valid = true;

                if ( PageParameter( PageParameterKey.AttendanceDate ).IsNullOrWhiteSpace() ||
                    PageParameter( PageParameterKey.ScheduleId ).IsNullOrWhiteSpace() ||
                    PageParameter( PageParameterKey.GroupId ).IsNullOrWhiteSpace() ||
                    PageParameter( PageParameterKey.LocationId ).IsNullOrWhiteSpace() )
                {
                    valid = false;
                }

                if ( valid )
                {
                    rFilter.Visible = true;
                    gAttendees.Visible = true;
                    BindFilter();
                    BindGrid();
                }
                else
                {
                    rFilter.Visible = false;
                    gAttendees.Visible = false;
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the GridRebind event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAttendees_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the user clicking the delete button in the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAttendeesDelete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var attendance = attendanceService.Get( e.RowKeyId );

            attendance.DidAttend = false;
            rockContext.SaveChanges();

            if ( attendance.Occurrence.LocationId != null )
            {
                Rock.CheckIn.KioskLocationAttendance.Remove( attendance.Occurrence.LocationId.Value );
            }

            BindGrid();

        }


        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            var rockContext = new RockContext();
            switch ( e.Key )
            {
                case "Entered By":
                    int? personId = e.Value.AsIntegerOrNull();
                    e.Value = null;
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            e.Value = person.ToString();
                        }
                    }
                    break;
                case "Attended":
                    if ( e.Value == "1" )
                    {
                        e.Value = "Did Attend";
                    }
                    else if ( e.Value == "0" )
                    {
                        e.Value = "Did Not Attend";
                    }
                    else
                    {
                        e.Value = null;
                    }

                    break;

                default:
                    e.Value = null;
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( UserPreferenceKey.EnteredBy, ppEnteredBy.SelectedValue.ToString() );
            rFilter.SetFilterPreference( UserPreferenceKey.Attended, ddlDidAttend.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteFilterPreferences();
            BindFilter();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            int? enteredById = rFilter.GetFilterPreference( UserPreferenceKey.EnteredBy ).AsIntegerOrNull();
            if ( enteredById.HasValue )
            {
                var person = new PersonService( new RockContext() ).Get( enteredById.Value );
                ppEnteredBy.SetValue( person );
            }

            string filterValue = rFilter.GetFilterPreference( UserPreferenceKey.Attended );
            ddlDidAttend.SetValue( filterValue );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            IEnumerable<Attendance> attendance = new List<Attendance>();

            var groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            var scheduleId = PageParameter( PageParameterKey.ScheduleId ).AsIntegerOrNull();
            var locationId = PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();
            var attendanceDate = PageParameter( PageParameterKey.AttendanceDate ).AsDateTime();

            if ( groupId.HasValue && scheduleId.HasValue && locationId.HasValue && attendanceDate.HasValue )
            {
                var groupLocation = new GroupLocationService( rockContext ).Get( locationId.Value );

                //
                // Check for existing attendance records.
                //
                var attendanceQry = attendanceService.Queryable()
                    .Where( a =>
                        a.Occurrence.GroupId == groupId.Value &&
                        a.Occurrence.OccurrenceDate == attendanceDate.Value &&
                        a.Occurrence.LocationId == groupLocation.LocationId &&
                        a.Occurrence.ScheduleId == scheduleId );

                // Filter by DidAttend
                int? didAttend = ddlDidAttend.SelectedValueAsInt( false );
                if ( didAttend.HasValue )
                {
                    if ( didAttend.Value == 1 )
                    {
                        attendanceQry = attendanceQry.Where( a => a.DidAttend == true );
                    }
                    else
                    {
                        attendanceQry = attendanceQry.Where( a => a.DidAttend == false );
                    }
                }

                // Filter by Entered By
                int? personId = ppEnteredBy.SelectedValue;
                if ( personId.HasValue )
                {
                    attendanceQry = attendanceQry.Where( a => a.CreatedByPersonAliasId.HasValue && a.CreatedByPersonAlias.PersonId == personId.Value );
                }

                attendance = attendanceQry
                    .OrderBy( a => a.PersonAlias.Person.LastName )
                    .ThenBy( a => a.PersonAlias.Person.FirstName );
            }

            gAttendees.EntityTypeId = EntityTypeCache.Get<Attendance>().Id;
            gAttendees.DataSource = attendance.ToList();
            gAttendees.DataBind();
        }

        #endregion
    }
}