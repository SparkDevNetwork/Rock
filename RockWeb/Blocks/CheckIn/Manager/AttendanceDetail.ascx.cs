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
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "Attendance Detail" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block to show details of a person's attendance" )]
    public partial class AttendanceDetail : RockBlock
    {
        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string AttendanceGuid = "Attendance";
            public const string AttendanceId = "AttendanceId";
        }

        #endregion PageParameterKeys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                ShowDetail( GetAttendanceGuid() );
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        #endregion

        #region Methods

        private Guid? _attendanceGuid;

        /// <summary>
        /// Gets the attendance unique identifier based on what was passed into the page parameters
        /// </summary>
        private Guid? GetAttendanceGuid()
        {
            if ( _attendanceGuid.HasValue )
            {
                return _attendanceGuid.Value;
            }

            Guid? attendanceGuid = PageParameter( PageParameterKey.AttendanceGuid ).AsGuidOrNull();
            if ( attendanceGuid.HasValue )
            {
                return attendanceGuid.Value;
            }

            int? attendanceId = PageParameter( PageParameterKey.AttendanceId ).AsIntegerOrNull();
            if ( attendanceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    _attendanceGuid = new AttendanceService( rockContext ).GetGuid( attendanceId.Value );
                }
            }

            return _attendanceGuid;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="attendanceGuid">The attendance unique identifier.</param>
        private void ShowDetail( Guid? attendanceGuid )
        {
            if ( !attendanceGuid.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            // fetch the attendance record and include all the details that we'll be displaying
            var attendance = attendanceService.Queryable()
                .Where( a => a.Guid == attendanceGuid.Value && a.PersonAliasId.HasValue )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.Occurrence.Group )
                .Include( a => a.Occurrence.Schedule )
                .Include( a => a.Occurrence.Location )
                .Include( a => a.AttendanceCode )
                .Include( a => a.CheckedOutByPersonAlias.Person )
                .Include( a => a.PresentByPersonAlias.Person )
                .AsNoTracking()
                .FirstOrDefault();

            if ( attendance == null )
            {
                return;
            }

            ShowAttendanceDetails( attendance );
        }

        /// <summary>
        /// Sets the checkin person label (Name, PhoneNumber, etc)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="rockLiteral">The rock literal.</param>
        private static void SetCheckinPersonLabel( RockContext rockContext, int? personAliasId, RockLiteral rockLiteral )
        {
            if ( !personAliasId.HasValue )
            {
                rockLiteral.Visible = false;
                return;
            }

            var personAliasService = new PersonAliasService( rockContext );
            var person = personAliasService.GetSelect( personAliasId.Value, s => s.Person );

            if ( person == null )
            {
                rockLiteral.Visible = false;
                return;
            }

            rockLiteral.Visible = true;

            var checkedInByPersonName = person.FullName;
            var checkedInByPersonPhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            rockLiteral.Text = string.Format( "{0} {1}", checkedInByPersonName, checkedInByPersonPhone );
        }

        /// <summary>
        /// Shows the attendance details.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        private void ShowAttendanceDetails( Attendance attendance )
        {
            var occurrence = attendance.Occurrence;

            var groupType = GroupTypeCache.Get( occurrence.Group.GroupTypeId );
            var rockContext = new RockContext();
            var groupPath = new GroupTypeService( rockContext ).GetAllCheckinAreaPaths().FirstOrDefault( a => a.GroupTypeId == occurrence.Group.GroupTypeId );

            lGroupName.Text = string.Format( "{0} > {1}", groupPath, occurrence.Group );
            if ( occurrence.Location != null )
            {
                lLocationName.Text = occurrence.Location.Name;
            }

            if ( attendance.AttendanceCode != null )
            {
                lTag.Text = attendance.AttendanceCode.Code;
            }

            if ( occurrence.Schedule != null )
            {
                lScheduleName.Text = occurrence.Schedule.Name;
            }

            SetCheckinPersonLabel( rockContext, attendance.CheckedInByPersonAliasId, lCheckinByPerson );
            lCheckinTime.Text = attendance.StartDateTime.ToString();

            if ( attendance.PresentDateTime.HasValue )
            {
                lPresentTime.Visible = true;
                lPresentTime.Text = attendance.PresentDateTime.ToString();
                SetCheckinPersonLabel( rockContext, attendance.PresentByPersonAliasId, lPresentByPerson );
            }
            else
            {
                lPresentTime.Visible = false;
                pnlPresentDetails.Visible = false;
            }

            if ( attendance.EndDateTime.HasValue )
            {
                lCheckedOutTime.Visible = true;
                lCheckedOutTime.Text = attendance.EndDateTime.ToString();
                SetCheckinPersonLabel( rockContext, attendance.CheckedOutByPersonAliasId, lCheckedOutByPerson );
            }
            else
            {
                lCheckedOutTime.Visible = false;
                pnlCheckedOutDetails.Visible = false;
            }
        }

        #endregion Methods
    }
}