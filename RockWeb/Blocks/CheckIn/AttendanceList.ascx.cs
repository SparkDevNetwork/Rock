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
    [Category( "Checkin" )]
    [Description( "Block for displaying the attendance history of a person or a group." )]

    public partial class AttendanceList : RockBlock, ICustomGridColumns
    {
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

                if ( valid )
                {
                    var attendanceDate =  PageParameter( "AttendanceDate" ).AsDateTime();
                    rFilter.Visible = true;
                    gAttendees.Visible = true;
                    BindFilter();
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
        /// Handles the GridRebind event of the gHistory control.
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
            rFilter.SaveUserPreference( "Entered By", ppEnteredBy.SelectedValue.ToString() );
            rFilter.SaveUserPreference( "Attended", ddlDidAttend.SelectedValue );

            BindGrid();
        }

        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            BindFilter();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var qryAttendance = attendanceService.Queryable();

            int? didAttend = ddlDidAttend.SelectedValueAsInt( false );
            if ( didAttend.HasValue )
            {
                if ( didAttend.Value == 1 )
                {
                    qryAttendance = qryAttendance.Where( a => a.DidAttend == true );
                }
                else
                {
                    qryAttendance = qryAttendance.Where( a => a.DidAttend == false );
                }
            }

            var qry = qryAttendance
                .Select( a => new
                {
                    LocationId = a.Occurrence.LocationId,
                    LocationName = a.Occurrence.Location != null ? a.Occurrence.Location.Name : string.Empty,
                    CampusId = a.CampusId,
                    CampusName = a.Campus != null ? a.Campus.Name : string.Empty,
                    ScheduleName = a.Occurrence.Schedule != null ? a.Occurrence.Schedule.Name : string.Empty,
                    Person = a.PersonAlias.Person,
                    GroupName = a.Occurrence.Group != null ? a.Occurrence.Group.Name : string.Empty,
                    GroupTypeId = a.Occurrence.Group != null ? a.Occurrence.Group.GroupTypeId : ( int? ) null,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime,
                    DidAttend = a.DidAttend
                } );

            SortProperty sortProperty = gAttendees.SortProperty;
            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( p => p.StartDateTime );
            }

            gAttendees.EntityTypeId = EntityTypeCache.Get<Attendance>().Id;
            gAttendees.DataSource = qry.ToList();
            gAttendees.DataBind();
        }

        #endregion
    }
}
