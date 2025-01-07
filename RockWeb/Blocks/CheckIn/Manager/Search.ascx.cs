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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "Search" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block used to search current check-in." )]

    #region Block Attributes

    [LinkedPage(
        "Person Page",
        Description = "The page used to display a selected person's details.",
        Order = 0,
        Key = AttributeKey.PersonPage )]

    [BooleanField(
        "Search By Code",
        Description = "A flag indicating if security codes should also be evaluated in the search box results.",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.SearchByCode )]

    [AttributeCategoryField(
        "Check-in Roster Alert Icon Category",
        Description = "The Person Attribute category to get the Alert Icon attributes from",
        Key = AttributeKey.CheckInRosterAlertIconCategory,
        DefaultValue = Rock.SystemGuid.Category.PERSON_ATTRIBUTES_CHECK_IN_ROSTER_ALERT_ICON,
        EntityType = typeof( Rock.Model.Person ),
        AllowMultiple = false,
        Order = 2
        )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "B10EF525-6F2F-46B8-865C-B4249A297307" )]
    public partial class Search : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string PersonPage = "PersonPage";
            public const string SearchByCode = "SearchByCode";
            public const string CheckInRosterAlertIconCategory = "CheckInRosterAlertIconCategory";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        private class PageParameterKey
        {
            public const string Person = "Person";
        }

        #endregion Page Parameter Keys

        #region ViewState Keys

        /// <summary>
        /// Keys to use for ViewState.
        /// </summary>
        private class ViewStateKey
        {
            public const string CurrentCampusId = "CurrentCampusId";
        }

        #endregion ViewState Keys

        #region Properties

        /// <summary>
        /// The current campus identifier.
        /// </summary>
        public int CurrentCampusId
        {
            get
            {
                return ( ViewState[ViewStateKey.CurrentCampusId] as string ).AsInteger();
            }

            set
            {
                ViewState[ViewStateKey.CurrentCampusId] = value.ToString();
            }
        }

        #endregion

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
            if ( this.IsPostBack )
            {
                HandleCustomPostback();
            }
            else
            {
                ShowDetails();
            }

            nbWarning.Visible = false;

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPage();
        }

        /// <summary>
        /// Handles the TextChanged event of the tbSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HandleCustomPostback()
        {
            var eventArg = this.Request.Params["__EVENTARGUMENT"];
            if ( eventArg == "search" && tbSearch.Text.Length > 2 )
            {
                ShowAttendees();
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            RosterAttendee attendee = e.Row.DataItem as RosterAttendee;

            // Desktop only.
            var lPhoto = e.Row.FindControl( "lPhoto" ) as Literal;
            lPhoto.Text = attendee.GetPersonPhotoImageHtmlTag();

            // Mobile only.
            var lMobileIcon = e.Row.FindControl( "lMobileIcon" ) as Literal;
            lMobileIcon.Text = attendee.GetStatusIconHtmlTag( true );

            // Shared between desktop and mobile.
            var lName = e.Row.FindControl( "lName" ) as Literal;
            lName.Text = attendee.GetAttendeeNameHtml();

            // Desktop only.
            var lBadges = e.Row.FindControl( "lBadges" ) as Literal;
            lBadges.Text = string.Format( "<div>{0}</div>", attendee.GetBadgesHtml( _attributesForAlertIcons ) );

            // Mobile only.
            var lMobileTagAndSchedules = e.Row.FindControl( "lMobileTagAndSchedules" ) as Literal;
            lMobileTagAndSchedules.Text = attendee.GetMobileTagAndSchedulesHtml();

            // Desktop only.
            var lStatusTag = e.Row.FindControl( "lStatusTag" ) as Literal;
            lStatusTag.Text = attendee.GetStatusIconHtmlTag( false );
        }

        /// <summary>
        /// Handles the RowSelected event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            string personGuid = e.RowKeyValues[0].ToString();
            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.Person, personGuid }
            };

            if ( !NavigateToLinkedPage( AttributeKey.PersonPage, queryParams ) )
            {
                ShowWarningMessage( "The 'Person Page' Block Attribute must be defined." );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            if ( GetAttributeValue( AttributeKey.SearchByCode ).AsBoolean() )
            {
                tbSearch.Placeholder = "Search by name or tag code";
            }
            else
            {
                tbSearch.Placeholder = "Search by name";
            }
        }

        private List<AttributeCache> _attributesForAlertIcons = new List<AttributeCache>();

        /// <summary>
        /// Shows the attendees.
        /// </summary>
        private void ShowAttendees()
        {
            pnlSearchResults.Visible = true;
            using ( var rockContext = new RockContext() )
            {
                var attendees = GetAttendees( rockContext );

                var attendeesSorted = attendees.OrderByDescending( a => a.MeetsRosterStatusFilter( RosterStatusFilter.Present ) ).ThenByDescending( a => a.CheckInTime ).ThenBy( a => a.PersonGuid ).ToList();

                var checkInRosterAlertIconCategoryGuid = this.GetAttributeValue( AttributeKey.CheckInRosterAlertIconCategory )?.AsGuid();
                if ( checkInRosterAlertIconCategoryGuid.HasValue )
                {
                    var categoryId = CategoryCache.GetId( checkInRosterAlertIconCategoryGuid.Value ) ?? 0;
                    _attributesForAlertIcons = new AttributeService( rockContext ).GetByCategoryId( categoryId ).ToAttributeCacheList();
                }
                else
                {
                    _attributesForAlertIcons = new List<AttributeCache>();
                }

                gAttendees.DataSource = attendeesSorted;
                gAttendees.DataBind();
            }
        }

        /// <summary>
        /// Gets the attendees.
        /// </summary>
        private IList<RosterAttendee> GetAttendees( RockContext rockContext )
        {
            var startDateTime = RockDateTime.Today;
            CampusCache campusCache = CampusCache.Get( CurrentCampusId );
            DateTime currentDateTime;
            if ( campusCache != null )
            {
                currentDateTime = campusCache.CurrentDateTime;
            }
            else
            {
                currentDateTime = RockDateTime.Now;
            }

            // Get all Attendance records for the current day
            var attendanceQuery = new AttendanceService( rockContext )
                .Queryable()
                .Include( a => a.AttendanceCode )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.Occurrence.Schedule )
                .Include( a => a.Occurrence.Group )
                .Include( a => a.Occurrence.Location )
                .Where( a =>
                    a.StartDateTime >= startDateTime
                    && a.DidAttend == true
                    && a.StartDateTime <= currentDateTime
                    && a.PersonAliasId.HasValue
                    && a.Occurrence.GroupId.HasValue
                    && a.Occurrence.ScheduleId.HasValue
                    && a.Occurrence.LocationId.HasValue );

            attendanceQuery = attendanceQuery.Where( a => a.DidAttend == true );

            // Do the person search
            var personService = new PersonService( rockContext );
            List<int> personIds = null;

            // ignore the result of reversed (LastName, FirstName vs FirstName LastName
            bool reversed = false;

            string searchValue = tbSearch.Text.Trim();
            if ( searchValue.IsNullOrWhiteSpace() )
            {
                personIds = new List<int>();
            }
            else
            {
                // If searching by code is enabled, first search by the code
                if ( GetAttributeValue( AttributeKey.SearchByCode ).AsBoolean() )
                {
                    var dayStart = RockDateTime.Today;
                    personIds = new AttendanceService( rockContext )
                        .Queryable().Where( a =>
                            a.StartDateTime >= dayStart &&
                            a.StartDateTime <= currentDateTime &&
                            a.AttendanceCode.Code == searchValue )
                        .Select( a => a.PersonAlias.PersonId )
                        .Distinct().ToList();
                }

                if ( personIds == null || !personIds.Any() )
                {
                    // If searching by code was disabled or nobody was found with code, search by name
                    personIds = personService
                        .GetByFullName( searchValue, false, false, false, out reversed )
                        .AsNoTracking()
                        .Select( a => a.Id )
                        .ToList();
                }
            }

            IList<RosterAttendee> attendees;

            if ( personIds.Any() )
            {
                // Get *all* of today's transactions.
                // Not sure why we aren't filtering by PersonIds yet, but
                // it could be to make performance more consistent in case the PersonQuery is complex.
                var attendanceQueryList = RosterAttendeeAttendance.Select( attendanceQuery ).ToList();

                // join (in memory) matching person ids and attendances
                var peopleAttendances = personIds
                        .GroupJoin(
                            attendanceQueryList,
                            pId => pId,
                            a => a.PersonId,
                            ( p, a ) => a )
                        .SelectMany( a => a )
                        .Distinct()
                        .ToList();

                attendees = RosterAttendee.GetFromAttendanceList( peopleAttendances );
            }
            else
            {
                // no matching persons, so return empty list
                attendees = new List<RosterAttendee>();
            }

            return attendees;
        }

        /// <summary>
        /// Shows a warning message, and optionally hides the content panels.
        /// </summary>
        /// <param name="warningMessage">The warning message to show.</param>
        private void ShowWarningMessage( string warningMessage )
        {
            nbWarning.Text = warningMessage;
            nbWarning.Visible = true;
            pnlContent.Visible = false;
        }

        #endregion Internal Methods
    }
}