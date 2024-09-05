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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "Locations (Obsolete)" )]
    [RockObsolete( "1.12" )]
    [Category( "Check-in > Manager" )]
    [Description( "Obsolete. Use Roster, LiveMetrics, and RoomSettings Blocks instead" )]

    [CustomRadioListField( "Navigation Mode", "Navigation and attendance counts can be grouped and displayed either by 'Group Type > Group Type (etc) > Group > Location' or by 'location > location (etc).'  Select the navigation hierarchy that is most appropriate for your organization.", "T^Group Type,L^Location,", true, "T", "", 0, "Mode" )]
    [GroupTypeField( "Check-in Type", "The Check-in Area to display.  This value can also be overridden through the URL query string key (e.g. when navigated to from the Check-in Type selection block).", false, "", "", 1, "GroupTypeTemplate", Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE )]
    [LinkedPage( "Person Page", "The page used to display a selected person's details.", order: 2 )]
    [LinkedPage( "Area Select Page", "The page to redirect user to if area has not be configured or selected.", order: 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", order: 4, defaultValue: Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK )]
    [BooleanField( "Search By Code", "A flag indicating if security codes should also be evaluated in the search box results.", order: 5 )]
    [Rock.SystemGuid.BlockTypeGuid( "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC" )]
    public partial class Locations : Rock.Web.UI.RockBlock
    {
        #region Fields

        private string _configuredMode = "L";

        #endregion

        #region Properties

        public string CurrentCampusId { get; set; }
        public string CurrentScheduleId { get; set; }
        public string CurrentNavPath { get; set; }

        public NavigationData NavData { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            CurrentCampusId = ViewState["CurrentCampusId"] as string;
            CurrentScheduleId = ViewState["CurrentScheduleId"] as string;
            CurrentNavPath = ViewState["CurrentNavPath"] as string;
            NavData = ViewState["NavData"] as NavigationData;
        }

        public override List<Rock.Web.UI.BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            return base.GetBreadCrumbs( pageReference );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/flot/jquery.flot.js" );
            RockPage.AddScriptLink( "~/Scripts/flot/jquery.flot.time.js" );
            RockPage.AddScriptLink( "~/Scripts/flot/jquery.flot.resize.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            upnlContent.OnPostBack += upnlContent_OnPostBack;

            rptNavItems.ItemDataBound += rptNavItems_ItemDataBound;
            rptNavItems.ItemCommand += RptNavItems_ItemCommand;
            rptPeople.ItemDataBound += rptPeople_ItemDataBound;

            _configuredMode = GetAttributeValue( "Mode" );

            Page.Form.DefaultButton = lbSearch.UniqueID;
            Page.Form.DefaultFocus = tbSearch.UniqueID;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbWarning.Visible = false;

            var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
            var campusContext = RockPage.GetCurrentContext( campusEntityType ) as Campus;
            CampusCache campus = null;
            if ( campusContext != null )
            {
                campus = CampusCache.Get( campusContext );
            }
            else
            {
                campus = GetDefaultCampus();
            }

            if ( campus != null )
            {

                var scheduleEntityType = EntityTypeCache.Get( "Rock.Model.Schedule" );
                var scheduleContext = RockPage.GetCurrentContext( scheduleEntityType ) as Schedule;
                string scheduleId = string.Empty;
                if ( scheduleContext != null )
                {
                    scheduleId = scheduleContext.Id.ToString();
                }

                if ( campus.Id.ToString() != CurrentCampusId || scheduleId != CurrentScheduleId || NavData == null )
                {
                    CurrentCampusId = campus.Id.ToString();
                    NavData = GetNavigationData( campus, scheduleId.AsIntegerOrNull() );
                    CurrentScheduleId = scheduleId;

                    if ( Page.IsPostBack )
                    {
                        CurrentNavPath = string.Empty;
                        BuildNavigationControls();
                    }
                }

                if ( !Page.IsPostBack )
                {
                    // Check for navigation path
                    string requestedNavPath = PageParameter( "NavPath" );
                    if ( !string.IsNullOrWhiteSpace( requestedNavPath ) )
                    {
                        CurrentNavPath = requestedNavPath;
                    }
                    else
                    {
                        int? groupTypeId = PageParameter( "GroupType" ).AsIntegerOrNull();
                        int? locationId = PageParameter( "Location" ).AsIntegerOrNull();
                        int? groupId = PageParameter( "Group" ).AsIntegerOrNull();
                        if ( groupTypeId.HasValue || locationId.HasValue || groupId.HasValue )
                        {
                            CurrentNavPath = BuildCurrentPath( groupTypeId, locationId, groupId );
                        }
                    }
                    if ( string.IsNullOrWhiteSpace( CurrentNavPath ) )
                    {
                        var globalPreferences = GetGlobalPersonPreferences();
                        CurrentNavPath = globalPreferences.GetValue( "checkin-manager-current-nav-path" );
                    }

                    SetChartOptions();
                    BuildNavigationControls();
                }
            }
            else
            {
                nbWarning.Text = "Check-in Manager requires that a valid campus exists.";
                nbWarning.Visible = true;
                pnlContent.Visible = false;
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["NavData"] = NavData;
            ViewState["CurrentNavPath"] = CurrentNavPath;
            ViewState["CurrentScheduleId"] = CurrentScheduleId;
            ViewState["CurrentCampusId"] = CurrentCampusId;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get all the schedules that allow checkin
                var schedules = new ScheduleService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( s => s.CheckInStartOffsetMinutes.HasValue && s.IsActive )
                    .ToList();

                // Get a lit of the schedule ids
                var scheduleIds = schedules.Select( s => s.Id ).ToList();

                // Get a list of all the groups that we're concerned about
                var groupIds = NavData.Groups.Select( g => g.Id ).ToList();

                // Get a list of all the people that are currently checked in
                var minDate = RockDateTime.Today.AddDays( -1 );
                var attendanceService = new AttendanceService( rockContext );
                var currentAttendeeIds = attendanceService
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.Occurrence.ScheduleId.HasValue &&
                        a.Occurrence.GroupId.HasValue &&
                        a.Occurrence.LocationId.HasValue &&
                        a.PersonAlias != null &&
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value &&
                        a.StartDateTime > minDate &&
                        !a.EndDateTime.HasValue &&
                        scheduleIds.Contains( a.Occurrence.ScheduleId.Value ) &&
                        groupIds.Contains( a.Occurrence.GroupId.Value ) )
                    .ToList()
                    .Where( a => a.IsCurrentlyCheckedIn )
                    .Select( a => a.PersonAlias.PersonId )
                    .Distinct();

                // Create a qry to get the last checkin date (used in next statement's join)
                var attendanceQry = attendanceService
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.Occurrence.ScheduleId.HasValue &&
                        a.Occurrence.GroupId.HasValue &&
                        a.Occurrence.LocationId.HasValue &&
                        a.PersonAliasId.HasValue &&
                        a.DidAttend.Value &&
                        scheduleIds.Contains( a.Occurrence.ScheduleId.Value ) &&
                        groupIds.Contains( a.Occurrence.GroupId.Value ) )
                    .GroupBy( a => new
                    {
                        PersonId = a.PersonAlias.PersonId
                    } )
                    .Select( g => new PersonResult
                    {
                        Id = g.Key.PersonId,
                        Guid = Guid.Empty,
                        Name = "",
                        PhotoId = null,
                        LastCheckin = g.Max( a => a.StartDateTime ),
                        CheckedInNow = false,
                        ScheduleGroupNames = ""
                    } );

                // Do the person search
                var personService = new PersonService( rockContext );
                List<Rock.Model.Person> people = null;
                bool reversed = false;

                string searchValue = tbSearch.Text.Trim();
                if ( searchValue.IsNullOrWhiteSpace() )
                {
                    people = new List<Rock.Model.Person>();
                }
                else
                {
                    // If searching by code is enabled, first search by the code
                    if ( GetAttributeValue( "SearchByCode" ).AsBoolean() )
                    {
                        var dayStart = RockDateTime.Today;
                        var now = GetCampusTime();
                        var personIds = new AttendanceService( rockContext )
                            .Queryable().Where( a =>
                                a.StartDateTime >= dayStart &&
                                a.StartDateTime <= now &&
                                a.AttendanceCode.Code == searchValue )
                            .Select( a => a.PersonAlias.PersonId )
                            .Distinct();
                        people = personService.Queryable()
                            .Where( p => personIds.Contains( p.Id ) )
                            .ToList();
                    }

                    if ( people == null || !people.Any() )
                    {
                        // If searching by code was disabled or nobody was found with code, search by name
                        people = personService
                            .GetByFullName( searchValue, false, false, false, out reversed )
                            .ToList();
                    }
                }

                var results = people
                    .GroupJoin(
                        attendanceQry,
                        p => p.Id,
                        a => a.Id,
                        ( p, a ) => a
                            .Select( c =>
                                new PersonResult
                                {
                                    Id = p.Id,
                                    Guid = p.Guid,
                                    Name = ( reversed ?
                                        p.LastName + ", " + p.NickName :
                                        p.NickName + " " + p.LastName ),
                                    PhotoId = p.PhotoId,
                                    Age = p.Age.ToString() ?? "",
                                    LastCheckin = c.LastCheckin,
                                    CheckedInNow = currentAttendeeIds.Contains( p.Id ),
                                    ScheduleGroupNames = ""
                                } )
                            .DefaultIfEmpty(
                                new PersonResult
                                {
                                    Id = p.Id,
                                    Guid = p.Guid,
                                    Name = ( reversed ?
                                        p.LastName + ", " + p.NickName :
                                        p.NickName + " " + p.LastName ),
                                    PhotoId = p.PhotoId,
                                    Age = p.Age.ToString() ?? "",
                                    LastCheckin = null,
                                    CheckedInNow = false,
                                    ScheduleGroupNames = ""
                                } ) )
                    .SelectMany( a => a )
                    .Distinct()
                    .OrderByDescending( a => a.CheckedInNow )
                    .ThenByDescending( a => a.LastCheckin )
                    .ThenBy( a => a.Name )
                    .ToList();

                pnlNavHeading.Attributes["onClick"] = upnlContent.GetPostBackEventReference( CurrentNavPath );
                lNavHeading.Text = "Back";

                rptNavItems.Visible = false;

                rptPeople.Visible = true;
                rptPeople.DataSource = results;
                rptPeople.DataBind();

            }

            RegisterStartupScript();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetChartOptions();
            BuildNavigationControls();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptNavItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptNavItems_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var navItem = e.Item.DataItem as NavigationItem;
            if ( navItem != null )
            {
                var li = e.Item.FindControl( "liNavItem" ) as HtmlGenericControl;
                if ( li != null )
                {
                    li.Attributes["onClick"] = upnlContent.GetPostBackEventReference( string.Format( "{0}|{1}{2}",
                        CurrentNavPath, navItem.TypeKey, navItem.Id ) );
                }

                var loc = navItem as NavigationLocation;

                var lbl = e.Item.FindControl( "lblCurrentCount" ) as Label;
                if ( lbl != null )
                {
                    lbl.Text = navItem.CurrentCount.ToString( "N0" );
                    if ( loc != null && loc.SoftThreshold.HasValue )
                    {
                        lbl.Text = string.Format( "{0:N0}/{1:N0}", navItem.CurrentCount, loc.SoftThreshold.Value );
                        if ( loc.CurrentCount >= loc.SoftThreshold )
                        {
                            lbl.AddCssClass( "badge-danger" );
                        }
                        else
                        {
                            if ( navItem.CurrentCount > 0 )
                            {
                                lbl.AddCssClass( "badge-success" );
                            }
                        }
                    }
                    else
                    {
                        if ( navItem.CurrentCount > 0 )
                        {
                            lbl.AddCssClass( "badge-success" );
                        }
                    }
                }

                var tgl = e.Item.FindControl( "tglRoom" ) as Toggle;
                if ( tgl != null )
                {
                    if ( loc != null )
                    {
                        tgl.Visible = loc.HasGroups;
                        tgl.Checked = loc.IsActive;
                        tgl.Attributes["data-key"] = loc.Id.ToString();
                    }
                    else
                    {
                        tgl.Visible = false;
                    }
                }

            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the RptNavItems control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        private void RptNavItems_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "RefreshThreshold" )
            {
                int? id = e.CommandArgument.ToString().AsIntegerOrNull();
                var nb = e.Item.FindControl( "nbThreshold" ) as NumberBox;
                if ( id.HasValue && nb != null )
                {
                    int? threshold = nb.Text.AsIntegerOrNull();

                    using ( var rockContext = new RockContext() )
                    {
                        var location = new LocationService( rockContext ).Get( id.Value );
                        if ( location != null && location.SoftRoomThreshold != threshold )
                        {
                            location.SoftRoomThreshold = threshold;
                            rockContext.SaveChanges();
                            Rock.CheckIn.KioskDevice.Clear();
                        }
                    }

                    NavData.Locations.Where( l => l.Id == id.Value ).ToList().ForEach( l => l.SoftThreshold = threshold );
                }
            }

            BuildNavigationControls();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPeople control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPeople_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var person = e.Item.DataItem as PersonResult;
            if ( person != null )
            {
                var li = e.Item.FindControl( "liNavItem" ) as HtmlGenericControl;
                if ( li != null )
                {
                    li.Attributes["onClick"] = upnlContent.GetPostBackEventReference( "P" + person.Guid.ToString() );
                }

                var img = e.Item.FindControl( "imgPerson" ) as Literal;
                if ( img != null )
                {
                    img.Text = Rock.Model.Person.GetPersonPhotoImageTag( person.Id, person.PhotoId, person.Age.AsIntegerOrNull(), person.Gender, null, 50, 50, person.Name, "avatar avatar-lg mr-3" );
                }

                var lStatus = e.Item.FindControl( "lStatus" ) as Literal;
                if ( lStatus != null )
                {
                    if ( person.LastCheckin.HasValue )
                    {
                        if ( person.CheckedInNow )
                        {
                            lStatus.Text = "<span class='badge badge-success'>Checked In</span><br/>";
                        }
                        else
                        {
                            lStatus.Text = person.LastCheckin.Value.ToShortDateString() + "<br/>";
                        }
                    }
                    else
                    {
                        lStatus.Text = string.Empty;
                    }
                }

                if ( person.Age != string.Empty )
                {
                    var lAge = e.Item.FindControl( "lAge" ) as Literal;
                    if ( lAge != null )
                    {
                        lAge.Text = string.Format( "{0}<span class='small text-muted'>(Age: {1})</span>",
                            string.IsNullOrWhiteSpace( person.ScheduleGroupNames ) ? "<br/>" : " ",
                            person.Age );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the OnPostBack event of the upnlContent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PostBackEventArgs"/> instance containing the event data.</param>
        protected void upnlContent_OnPostBack( object sender, PostBackEventArgs e )
        {
            tbSearch.Text = string.Empty;

            if ( e.EventArgument == "R" )
            {
                int? campusId = CurrentCampusId.AsIntegerOrNull();
                if ( campusId.HasValue )
                {
                    int? scheduleId = CurrentScheduleId.AsIntegerOrNull();
                    NavData = GetNavigationData( CampusCache.Get( campusId.Value ), scheduleId );
                }
                BuildNavigationControls();
            }
            else
            {
                if ( e.EventArgument.StartsWith( "P" ) )
                {
                    var parms = new Dictionary<string, string>();
                    parms.Add( "Person", e.EventArgument.Substring( 1 ) );
                    NavigateToLinkedPage( "PersonPage", parms );
                }
                else
                {
                    CurrentNavPath = e.EventArgument;
                    BuildNavigationControls();
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglRoom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglRoom_CheckedChanged( object sender, EventArgs e )
        {
            var tgl = sender as Toggle;
            if ( tgl != null )
            {
                int? id = tgl.Attributes["data-key"].AsIntegerOrNull();
                if ( id.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var location = new LocationService( rockContext ).Get( id.Value );
                        if ( location != null && location.IsActive != tgl.Checked )
                        {
                            location.IsActive = tgl.Checked;
                            rockContext.SaveChanges();
                            Rock.CheckIn.KioskDevice.Clear();
                        }
                    }
                    NavData.Locations.Where( l => l.Id == id.Value ).ToList().ForEach( l => l.IsActive = tgl.Checked );
                }
            }

            BuildNavigationControls();
        }


        protected void lbUpdateThreshold_Click( object sender, EventArgs e )
        {
            int? id = lbUpdateThreshold.Attributes["data-key"].AsIntegerOrNull();
            if ( id.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    int? softThreshold = nbThreshold.Text.AsIntegerOrNull();
                    var location = new LocationService( rockContext ).Get( id.Value );
                    if ( location != null && location.SoftRoomThreshold != softThreshold )
                    {
                        location.SoftRoomThreshold = nbThreshold.Text.AsIntegerOrNull();
                        rockContext.SaveChanges();
                        Rock.CheckIn.KioskDevice.Clear();

                        NavData.Locations.Where( l => l.Id == id.Value ).ToList().ForEach( l => l.SoftThreshold = softThreshold );
                    }
                }
            }

            BuildNavigationControls();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptPeople control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptPeople_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Delete" )
            {
                int personId = e.CommandArgument.ToString().AsInteger();

                if ( string.IsNullOrWhiteSpace( CurrentNavPath ) || !CurrentNavPath.StartsWith( _configuredMode ) )
                {
                    CurrentNavPath = _configuredMode;
                }

                var pathParts = CurrentNavPath.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                int partIndex = pathParts.Length - 1;

                if ( partIndex >= 0 )
                {
                    string itemKey = pathParts[partIndex];
                    if ( itemKey.Length > 0 )
                    {
                        string itemType = itemKey.Left( 1 );
                        int? itemId = itemKey.Length > 1 ? itemKey.Substring( 1 ).AsIntegerOrNull() : null;

                        if ( itemType == "L" && itemId.HasValue )
                        {
                            // Only get attendance for last couple days. We'll then check each one based on timezone
                            // to see if it's active.
                            var minDate = RockDateTime.Today.AddDays( -1 );

                            using ( var rockContext = new RockContext() )
                            {
                                var attendanceService = new AttendanceService( rockContext );
                                foreach ( var attendance in attendanceService
                                    .Queryable()
                                    .Where( a =>
                                        a.StartDateTime > minDate &&
                                        !a.EndDateTime.HasValue &&
                                        a.Occurrence.LocationId.HasValue &&
                                        a.Occurrence.LocationId.Value == itemId.Value &&
                                        a.PersonAlias != null &&
                                        a.PersonAlias.PersonId == personId &&
                                        a.DidAttend.HasValue &&
                                        a.DidAttend.Value &&
                                        a.Occurrence.ScheduleId.HasValue )
                                    .ToList()
                                    .Where( a => a.IsCurrentlyCheckedIn ) )
                                {
                                    attendanceService.Delete( attendance );
                                }

                                rockContext.SaveChanges();

                                Rock.CheckIn.KioskLocationAttendance.Remove( itemId.Value );
                            }

                            int? campusId = CurrentCampusId.AsIntegerOrNull();
                            if ( campusId.HasValue )
                            {
                                int? scheduleId = CurrentScheduleId.AsIntegerOrNull();
                                NavData = GetNavigationData( CampusCache.Get( campusId.Value ), scheduleId );
                            }
                            BuildNavigationControls();
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        private CampusCache GetDefaultCampus()
        {
            return CampusCache.All().FirstOrDefault();
        }

        private string BuildCurrentPath( int? groupTypeId, int? locationId, int? groupId )
        {
            NavigationGroupType groupType = groupTypeId.HasValue ? NavData.GroupTypes.Where( t => t.Id == groupTypeId.Value ).FirstOrDefault() : null;
            NavigationLocation location = locationId.HasValue ? NavData.Locations.Where( l => l.Id == locationId.Value ).FirstOrDefault() : null;
            NavigationGroup group = groupId.HasValue ? NavData.Groups.Where( g => g.Id == groupId.Value ).FirstOrDefault() : null;

            var pathParts = new List<string>();

            if ( _configuredMode == "T" )
            {
                if ( group != null && location != null )
                {
                    pathParts.Add( "L" + location.Id.ToString() );
                    pathParts.Add( "G" + group.Id.ToString() );

                    groupType = NavData.GroupTypes.Where( t => t.Id == group.GroupTypeId ).FirstOrDefault();
                }

                while ( groupType != null )
                {
                    pathParts.Add( "T" + groupType.Id.ToString() );
                    if ( groupType.ParentId.HasValue )
                    {
                        groupType = NavData.GroupTypes.Where( t => t.Id == groupType.ParentId.Value ).FirstOrDefault();
                    }
                    else
                    {
                        groupType = null;
                    }
                }
            }
            else
            {
                while ( location != null )
                {
                    pathParts.Add( "L" + location.Id.ToString() );
                    if ( location.ParentId.HasValue )
                    {
                        location = NavData.Locations.Where( l => l.Id == location.ParentId.Value ).FirstOrDefault();
                    }
                    else
                    {
                        location = null;
                    }
                }
            }

            if ( pathParts.Any() )
            {
                pathParts.Reverse();
                return pathParts.AsDelimited( "|" );
            }

            return string.Empty;
        }

        private void SetChartOptions()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var options = new ChartOptions();
#pragma warning restore CS0618 // Type or member is obsolete
            options.series = new SeriesOptions( false, true, false );
            options.xaxis = new AxisOptions { mode = AxisMode.time };
            options.grid = new GridOptions { hoverable = true, clickable = false };

            options.SetChartStyle( GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );

            options.xaxis.timeformat = "%I:%M";

            hfChartOptions.Value = JsonConvert.SerializeObject( options, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore } );
        }

        private void RegisterStartupScript()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var options = new ChartOptions();
#pragma warning restore CS0618 // Type or member is obsolete
            options.series = new SeriesOptions( false, true, false );
            options.yaxis = new AxisOptions { min = 0, minTickSize = 1 };
            options.xaxis = new AxisOptions { mode = AxisMode.time };
            options.grid = new GridOptions { hoverable = true, clickable = false };
            options.SetChartStyle( GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );
            options.xaxis.timeformat = "%I:%M";

            string script = string.Format( @"
    var data = eval($('#{1}').val());
    var options = {2};
    $.plot( $('#{0}'), data, options );

    $('.js-threshold-btn-edit').on('click', function(e){{
        var $parentDiv = $(this).closest('div.js-threshold');
        $parentDiv.find('.js-threshold-nb').val($parentDiv.find('.js-threshold-hf').val());
        $parentDiv.find('.js-threshold-view').hide();
        $parentDiv.find('.js-threshold-edit').show();
    }});

    $('a.js-threshold-edit').on('click', function(e){{
        var $parentDiv = $(this).closest('div.js-threshold');
        $parentDiv.find('.js-threshold-edit').hide();
        $parentDiv.find('.js-threshold-view').show();
        return true;
    }});

    $('.js-threshold').on('click', function(e){{
        e.stopPropagation();
    }});
",
                pnlChart.ClientID, hfChartData.ClientID,
                JsonConvert.SerializeObject(
                    options,
                    Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    } ) );

            ScriptManager.RegisterStartupScript( pnlChart, pnlChart.GetType(), "chart", script, true );
        }

        #region Get Navigation Data

        private NavigationData GetNavigationData( CampusCache campus, int? scheduleId )
        {
            using ( var rockContext = new RockContext() )
            {
                var validLocationids = new List<int>();
                if ( campus.LocationId.HasValue )
                {
                    // Get all the child locations
                    validLocationids.Add( campus.LocationId.Value );
                    new LocationService( rockContext )
                        .GetAllDescendents( campus.LocationId.Value )
                        .Select( l => l.Id )
                        .ToList()
                        .ForEach( l => validLocationids.Add( l ) );
                }

                var groupTypeTemplateGuid = PageParameter( "Area" ).AsGuidOrNull();
                if ( !groupTypeTemplateGuid.HasValue )
                {
                    groupTypeTemplateGuid = this.GetAttributeValue( "GroupTypeTemplate" ).AsGuidOrNull();
                }

                if ( !groupTypeTemplateGuid.HasValue )
                {
                    // Check to see if a specific group was specified
                    Guid? guid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
                    int? groupId = PageParameter( "Group" ).AsIntegerOrNull();
                    if ( groupId.HasValue && guid.HasValue )
                    {
                        var group = new GroupService( rockContext ).Get( groupId.Value );
                        if ( group != null && group.GroupType != null )
                        {
                            var groupType = GetParentPurposeGroupType( group.GroupType, guid.Value );
                            if ( groupType != null )
                            {
                                groupTypeTemplateGuid = groupType.Guid;
                            }
                        }
                    }
                }

                if ( groupTypeTemplateGuid.HasValue )
                {
                    pnlContent.Visible = true;

                    NavData = new NavigationData();

                    var chartTimes = GetChartTimes( campus );

                    // Get the group types
                    var parentGroupType = GroupTypeCache.Get( groupTypeTemplateGuid.Value );
                    if ( parentGroupType != null )
                    {
                        foreach ( var childGroupType in parentGroupType.ChildGroupTypes )
                        {
                            AddGroupType( null, childGroupType, chartTimes );
                        }
                    }

                    lGroupTypeName.Text = parentGroupType.Name ?? "";

                    // Get the groups
                    var groupTypeIds = NavData.GroupTypes.Select( t => t.Id ).ToList();

                    var groups = new GroupService( rockContext )
                        .Queryable( "GroupLocations" ).AsNoTracking()
                        .Where( g =>
                            groupTypeIds.Contains( g.GroupTypeId ) &&
                            g.IsActive )
                        .ToList();
                    var groupIds = groups.Select( g => g.Id ).ToList();

                    foreach ( var group in groups )
                    {
                        var childGroupIds = groups
                            .Where( g =>
                                g.ParentGroupId.HasValue &&
                                g.ParentGroupId.Value == group.Id )
                            .Select( g => g.Id )
                            .ToList();

                        var childLocationIds = group.GroupLocations
                            .Where( l => validLocationids.Contains( l.LocationId ) )
                            .Select( l => l.LocationId )
                            .ToList();

                        if ( childLocationIds.Any() || childGroupIds.Any() )
                        {
                            var navGroup = new NavigationGroup( group, chartTimes );
                            navGroup.ChildLocationIds = childLocationIds;
                            navGroup.ChildGroupIds = childGroupIds;
                            NavData.Groups.Add( navGroup );


                            if ( !group.ParentGroupId.HasValue || groupIds.Contains( group.ParentGroupId.Value ) )
                            {
                                NavData.GroupTypes.Where( t => t.Id == group.GroupTypeId ).ToList()
                                    .ForEach( t => t.ChildGroupIds.Add( group.Id ) );
                            }
                        }
                    }

                    // Remove any groups without child locations
                    var emptyGroupIds = NavData.Groups
                        .Where( g => !g.ChildGroupIds.Any() && !g.ChildLocationIds.Any() )
                        .Select( g => g.Id )
                        .ToList();
                    while ( emptyGroupIds.Any() )
                    {
                        NavData.Groups = NavData.Groups.Where( g => !emptyGroupIds.Contains( g.Id ) ).ToList();
                        NavData.Groups.ForEach( g =>
                            g.ChildGroupIds = g.ChildGroupIds.Where( c => !emptyGroupIds.Contains( c ) ).ToList() );
                        emptyGroupIds = NavData.Groups
                            .Where( g => !g.ChildGroupIds.Any() && !g.ChildLocationIds.Any() )
                            .Select( g => g.Id )
                            .ToList();
                    }

                    // Remove any grouptype without groups or child group types
                    var emptyGroupTypeIds = NavData.GroupTypes
                        .Where( t => !t.ChildGroupIds.Any() && !t.ChildGroupTypeIds.Any() )
                        .Select( t => t.Id )
                        .ToList();
                    while ( emptyGroupTypeIds.Any() )
                    {
                        NavData.GroupTypes = NavData.GroupTypes.Where( t => !emptyGroupTypeIds.Contains( t.Id ) ).ToList();
                        NavData.GroupTypes.ForEach( t =>
                            t.ChildGroupTypeIds = t.ChildGroupTypeIds.Where( c => !emptyGroupTypeIds.Contains( c ) ).ToList() );
                        emptyGroupTypeIds = NavData.GroupTypes
                            .Where( t => !t.ChildGroupIds.Any() && !t.ChildGroupTypeIds.Any() )
                            .Select( t => t.Id )
                            .ToList();
                    }

                    // If no group types left, display error message.
                    if ( NavData.GroupTypes.Count == 0 )
                    {
                        nbWarning.Text = "The selected check-in type does not have any valid groups or locations.";
                        nbWarning.Visible = true;
                        pnlContent.Visible = false;

                        return null;
                    }

                    // Get the locations
                    var locationIds = NavData.Groups.SelectMany( g => g.ChildLocationIds ).Distinct().ToList();
                    foreach ( var location in new LocationService( rockContext )
                        .Queryable( "ParentLocation" ).AsNoTracking()
                        .Where( l => locationIds.Contains( l.Id ) ) )
                    {
                        var navLocation = AddLocation( location, chartTimes );
                        navLocation.HasGroups = true;
                    }

                    // Get the attendance counts
                    var dayStart = RockDateTime.Today;
                    var now = GetCampusTime();

                    groupIds = NavData.Groups.Select( g => g.Id ).ToList();

                    var schedules = new List<Schedule>();

                    var attendanceQry = new AttendanceService( rockContext ).Queryable()
                        .Where( a =>
                            a.Occurrence.ScheduleId.HasValue &&
                            a.Occurrence.GroupId.HasValue &&
                            a.Occurrence.LocationId.HasValue &&
                            a.StartDateTime > dayStart &&
                            a.StartDateTime < now &&
                            !a.EndDateTime.HasValue &&
                            a.DidAttend.HasValue &&
                            a.DidAttend.Value &&
                            groupIds.Contains( a.Occurrence.GroupId.Value ) &&
                            locationIds.Contains( a.Occurrence.LocationId.Value ) );

                    if ( scheduleId.HasValue )
                    {
                        attendanceQry = attendanceQry.Where( a => a.Occurrence.ScheduleId == scheduleId.Value );

                        var schedule = new ScheduleService( rockContext ).Get( scheduleId.Value );
                        if ( schedule != null )
                        {
                            schedules.Add( schedule );
                        }
                    }
                    else
                    {
                        schedules = new ScheduleService( rockContext ).Queryable().AsNoTracking()
                            .Where( s => s.IsActive && s.CheckInStartOffsetMinutes.HasValue )
                            .ToList();
                    }

                    var attendanceList = attendanceQry.ToList();

                    foreach ( DateTime chartTime in chartTimes )
                    {
                        // Get the active schedules
                        var activeSchedules = new List<int>();
                        foreach ( var schedule in schedules )
                        {
                            if ( schedule.WasScheduleOrCheckInActive( chartTime ) )
                            {
                                activeSchedules.Add( schedule.Id );
                            }
                        }

                        bool current = chartTime.Equals( chartTimes.Max() );

                        foreach ( var groupLocSched in attendanceList
                            .Where( a =>
                                a.StartDateTime < chartTime &&
                                a.PersonAlias != null &&
                                activeSchedules.Contains( a.Occurrence.ScheduleId.Value ) )
                            .GroupBy( a => new
                            {
                                ScheduleId = a.Occurrence.ScheduleId.Value,
                                GroupId = a.Occurrence.GroupId.Value,
                                LocationId = a.Occurrence.LocationId.Value
                            } )
                            .Select( g => new
                            {
                                ScheduleId = g.Key.ScheduleId,
                                GroupId = g.Key.GroupId,
                                LocationId = g.Key.LocationId,
                                PersonIds = g.Select( a => a.PersonAlias.PersonId ).Distinct().ToList()
                            } ) )
                        {
                            AddGroupCount( chartTime, groupLocSched.GroupId, groupLocSched.PersonIds, current );
                            AddLocationCount( chartTime, groupLocSched.LocationId, groupLocSched.PersonIds, current );
                        }
                    }
                    return NavData;
                }
                else
                {
                    if ( string.IsNullOrWhiteSpace( PageParameter( "Area" ) ) )
                    {
                        // If could not determine area and did not come from are select, redirect to area select page
                        NavigateToLinkedPage( "AreaSelectPage" );
                    }

                    nbWarning.Text = "Please select a valid Check-in type in the block settings.";
                    nbWarning.Visible = true;
                    pnlContent.Visible = false;
                }
            }

            return null;
        }

        public GroupType GetParentPurposeGroupType( GroupType groupType, Guid purposeGuid )
        {
            if ( groupType != null &&
                groupType.GroupTypePurposeValue != null &&
                groupType.GroupTypePurposeValue.Guid.Equals( purposeGuid ) )
            {
                return groupType;
            }

            foreach ( var parentGroupType in groupType.ParentGroupTypes )
            {
                // skip if parent group type and current group type are the same (a situation that should not be possible) to prevent stack overflow
                if ( groupType.Id == parentGroupType.Id )
                {
                    continue;
                }

                var testGroupType = GetParentPurposeGroupType( parentGroupType, purposeGuid );
                if ( testGroupType != null )
                {
                    return testGroupType;
                }
            }

            return null;
        }

        private DateTime GetCampusTime()
        {
            int? campusId = CurrentCampusId.AsIntegerOrNull();
            if ( !campusId.HasValue )
            {
                return RockDateTime.Now;
            }

            var cacheCampus = CampusCache.Get( campusId.Value );
            return cacheCampus != null ? cacheCampus.CurrentDateTime : RockDateTime.Now;
        }

        private List<DateTime> GetChartTimes( CampusCache campus )
        {
            // Get the current minute
            var rockNow = campus != null ? campus.CurrentDateTime : RockDateTime.Now;
            var now = new DateTime( rockNow.Year, rockNow.Month, rockNow.Day, rockNow.Hour, rockNow.Minute, 0 );

            // Find the end mark
            var endTime = now.AddMinutes( 1 );
            while ( endTime.Minute % 5 != 0 )
            {
                endTime = endTime.AddMinutes( 1 );
            }

            // Get the start time
            var time = endTime.AddHours( -2 );

            // Get 5 min increments
            var times = new List<DateTime>();
            while ( time <= endTime )
            {
                times.Add( time );
                time = time.AddMinutes( 5 );
            }

            return times;
        }

        private void AddGroupType( int? parentGroupTypeId, GroupTypeCache groupType, List<DateTime> chartTimes )
        {
            if ( groupType != null && !NavData.GroupTypes.Exists( g => g.Id == groupType.Id ) )
            {
                var navGroupType = new NavigationGroupType( groupType, chartTimes );
                navGroupType.ParentId = parentGroupTypeId;

                NavData.GroupTypes.Add( navGroupType );

                foreach ( var childGroupType in groupType.ChildGroupTypes )
                {
                    AddGroupType( groupType.Id, childGroupType, chartTimes );
                    navGroupType.ChildGroupTypeIds.Add( childGroupType.Id );
                }
            }
        }

        private NavigationLocation AddLocation( Location location, List<DateTime> chartTimes )
        {
            if ( location != null )
            {
                var navLocation = NavData.Locations.FirstOrDefault( l => l.Id == location.Id );
                if ( navLocation == null )
                {
                    navLocation = new NavigationLocation( location, chartTimes );
                    NavData.Locations.Add( navLocation );
                }

                if ( location.ParentLocationId.HasValue )
                {
                    navLocation.ParentId = location.ParentLocationId;

                    var parentLocation = NavData.Locations.FirstOrDefault( l => l.Id == location.ParentLocationId );
                    if ( parentLocation == null )
                    {
                        parentLocation = AddLocation( location.ParentLocation, chartTimes );
                    }
                    if ( parentLocation != null )
                    {
                        parentLocation.ChildLocationIds.Add( navLocation.Id );
                    }
                }

                return navLocation;
            }

            return null;
        }

        private void AddGroupCount( DateTime time, int groupId, List<int> personIds, bool current )
        {
            var navGroup = NavData.Groups.FirstOrDefault( g => g.Id == groupId );
            if ( navGroup != null )
            {
                if ( !navGroup.RecentPersonIds.ContainsKey( time ) )
                {
                    navGroup.RecentPersonIds.Add( time, new List<int>() );
                }
                navGroup.RecentPersonIds[time] = navGroup.RecentPersonIds[time].Union( personIds ).ToList();

                if ( current )
                {
                    navGroup.CurrentPersonIds = navGroup.CurrentPersonIds.Union( personIds ).ToList();
                }

                AddGroupTypeCount( time, navGroup.GroupTypeId, personIds, current );
            }
        }

        private void AddGroupTypeCount( DateTime time, int groupTypeId, List<int> personIds, bool current )
        {
            var navGroupType = NavData.GroupTypes.FirstOrDefault( g => g.Id == groupTypeId );
            if ( navGroupType != null )
            {
                if ( !navGroupType.RecentPersonIds.ContainsKey( time ) )
                {
                    navGroupType.RecentPersonIds.Add( time, new List<int>() );
                }
                navGroupType.RecentPersonIds[time] = navGroupType.RecentPersonIds[time].Union( personIds ).ToList();

                if ( current )
                {
                    navGroupType.CurrentPersonIds = navGroupType.CurrentPersonIds.Union( personIds ).ToList();
                }

                if ( navGroupType.ParentId.HasValue )
                {
                    AddGroupTypeCount( time, navGroupType.ParentId.Value, personIds, current );
                }
            }
        }

        private void AddLocationCount( DateTime time, int locationId, List<int> personIds, bool current )
        {
            var navLocation = NavData.Locations.FirstOrDefault( g => g.Id == locationId );
            if ( navLocation != null )
            {
                if ( !navLocation.RecentPersonIds.ContainsKey( time ) )
                {
                    navLocation.RecentPersonIds.Add( time, new List<int>() );
                }
                navLocation.RecentPersonIds[time] = navLocation.RecentPersonIds[time].Union( personIds ).ToList();

                if ( current )
                {
                    navLocation.CurrentPersonIds = navLocation.CurrentPersonIds.Union( personIds ).ToList();
                }

                if ( navLocation.ParentId.HasValue )
                {
                    AddLocationCount( time, navLocation.ParentId.Value, personIds, current );
                }
            }
        }

        #endregion

        #region Rebuild Controls

        private void BuildNavigationControls()
        {
            if ( NavData != null )
            {
                if ( string.IsNullOrWhiteSpace( CurrentNavPath ) || !CurrentNavPath.StartsWith( _configuredMode ) )
                {
                    CurrentNavPath = _configuredMode;
                }

                var pathParts = CurrentNavPath.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                int numParts = pathParts.Length;

                NavigationItem item = NavData.GetItem( pathParts[numParts - 1] );
                while ( item == null && numParts > 1 )
                {
                    numParts--;
                    item = NavData.GetItem( pathParts[numParts - 1] );
                }
                CurrentNavPath = pathParts.Take( numParts ).ToList().AsDelimited( "|" );

                string itemKey = pathParts[numParts - 1];
                string itemType = itemKey.Left( 1 );
                int? itemId = itemKey.Length > 1 ? itemKey.Substring( 1 ).AsIntegerOrNull() : null;

                var globalPreferences = GetGlobalPersonPreferences();
                globalPreferences.SetValue( "checkin-manager-current-nav-path", CurrentNavPath );
                globalPreferences.Save();

                var navItems = new List<NavigationItem>();

                switch ( itemType )
                {
                    case "L":   // Location
                        {
                            // Add child locations
                            NavData.Locations
                                .Where( l => l.ParentId == itemId )
                                .ToList()
                                .ForEach( l => navItems.Add( l ) );

                            break;
                        }

                    case "T":   // Group Type
                        {
                            // Add child group types
                            NavData.GroupTypes
                                .Where( t => t.ParentId.Equals( itemId ) )
                                .ToList().ForEach( t => navItems.Add( t ) );

                            // Add child groups
                            var groupIds = NavData.GroupTypes
                                .Where( t => t.Id == itemId )
                                .SelectMany( t => t.ChildGroupIds )
                                .Distinct()
                                .ToList();
                            NavData.Groups
                                .Where( g => groupIds.Contains( g.Id ) )
                                .ToList()
                                .ForEach( g => navItems.Add( g ) );

                            break;
                        }

                    case "G":   // Group
                        {
                            // Add child groups
                            var groupIds = NavData.Groups
                                .Where( g => g.Id == itemId )
                                .SelectMany( g => g.ChildGroupIds )
                                .Distinct()
                                .ToList();
                            NavData.Groups
                                .Where( g => groupIds.Contains( g.Id ) )
                                .ToList()
                                .ForEach( g => navItems.Add( g ) );

                            // Add child locations
                            var locations = NavData.Groups
                                .Where( g => g.Id == itemId )
                                .SelectMany( g => g.ChildLocationIds )
                                .Distinct()
                                .ToList();
                            NavData.Locations
                                .Where( l => locations.Contains( l.Id ) )
                                .ToList()
                                .ForEach( l => navItems.Add( l ) );

                            break;
                        }
                }

                // Get chart data
                Dictionary<DateTime, List<int>> chartCounts = null;
                if ( item != null )
                {
                    chartCounts = item.RecentPersonIds;
                }
                else
                {
                    chartCounts = new Dictionary<DateTime, List<int>>();
                    foreach ( var navItem in navItems )
                    {
                        foreach ( var kv in navItem.RecentPersonIds )
                        {
                            if ( !chartCounts.ContainsKey( kv.Key ) )
                            {
                                chartCounts.Add( kv.Key, new List<int>() );
                            }
                            chartCounts[kv.Key] = chartCounts[kv.Key].Union( kv.Value ).ToList();
                        }
                    }
                }

                var chartData = new List<string>();

                TimeSpan baseSpan = new TimeSpan( new DateTime( 1970, 1, 1 ).Ticks );
                foreach ( var kv in chartCounts.OrderBy( c => c.Key ) )
                {
                    DateTime offsetTime = kv.Key.Subtract( baseSpan );
                    long ticks = ( long ) ( offsetTime.Ticks / 10000 );
                    chartData.Add( string.Format( "[{0}, {1}]", ticks, kv.Value.Count() ) );
                }
                hfChartData.Value = string.Format( "[ [ {0} ] ]", chartData.AsDelimited( ", " ) );
                pnlChart.Attributes["onClick"] = upnlContent.GetPostBackEventReference( "R" );

                pnlNavHeading.Visible = item != null;
                if ( item != null )
                {
                    if ( numParts > 0 )
                    {
                        pnlNavHeading.Attributes["onClick"] = upnlContent.GetPostBackEventReference(
                            pathParts.ToList().Take( numParts - 1 ).ToList().AsDelimited( "|" ) );
                    }
                    lNavHeading.Text = item.Name;

                    var locationItem = item as NavigationLocation;
                    if ( locationItem != null && locationItem.HasGroups )
                    {
                        tglHeadingRoom.Visible = true;
                        tglHeadingRoom.Checked = locationItem.IsActive;
                        tglHeadingRoom.Attributes["data-key"] = locationItem.Id.ToString();

                        pnlThreshold.Visible = locationItem.SoftThreshold.HasValue || locationItem.FirmThreshold.HasValue;
                        hfThreshold.Value = locationItem.SoftThreshold.HasValue ? locationItem.SoftThreshold.Value.ToString() : "";
                        lThreshold.Text = locationItem.SoftThreshold.HasValue ? locationItem.SoftThreshold.Value.ToString( "N0" ) : "none";
                        nbThreshold.MaximumValue = locationItem.FirmThreshold.HasValue ? locationItem.FirmThreshold.Value.ToString() : "";
                        lbUpdateThreshold.Attributes["data-key"] = locationItem.Id.ToString();

                        var rockContext = new RockContext();

                        var dayStart = RockDateTime.Today.AddDays( -1 );
                        var attendees = new AttendanceService( rockContext )
                            .Queryable( "Occurrence.Group,PersonAlias.Person,Occurrence.Schedule,AttendanceCode" )
                            .AsNoTracking()
                            .Where( a =>
                                a.StartDateTime > dayStart &&
                                !a.EndDateTime.HasValue &&
                                a.Occurrence.LocationId.HasValue &&
                                a.Occurrence.LocationId == locationItem.Id &&
                                a.DidAttend.HasValue &&
                                a.DidAttend.Value &&
                                a.Occurrence.ScheduleId.HasValue )
                            .ToList()
                            .Where( a => a.IsCurrentlyCheckedIn )
                            .ToList();

                        int? scheduleId = CurrentScheduleId.AsIntegerOrNull();

                        var people = new List<PersonResult>();
                        foreach ( var personId in attendees
                            .OrderBy( a => a.PersonAlias.Person.NickName )
                            .ThenBy( a => a.PersonAlias.Person.LastName )
                            .Select( a => a.PersonAlias.PersonId )
                            .Distinct() )
                        {
                            var matchingAttendees = attendees
                                .Where( a => a.PersonAlias.PersonId == personId )
                                .ToList();

                            if ( !scheduleId.HasValue || matchingAttendees.Any( a => a.Occurrence.ScheduleId == scheduleId.Value ) )
                            {
                                people.Add( new PersonResult( matchingAttendees ) );
                            }
                        }

                        rptPeople.Visible = true;
                        rptPeople.DataSource = people;
                        rptPeople.DataBind();
                    }
                    else
                    {
                        tglHeadingRoom.Visible = false;
                        pnlThreshold.Visible = false;
                        rptPeople.Visible = false;
                    }
                }
                else
                {
                    rptPeople.Visible = false;
                }

                rptNavItems.Visible = navItems.Any();
                rptNavItems.DataSource = navItems
                    .OrderBy( i => i.TypeKey )
                    .ThenBy( i => i.Order )
                    .ThenBy( i => i.Name );
                rptNavItems.DataBind();

                RegisterStartupScript();
            }
        }

        #endregion

        #endregion

        #region Helper Navigation Classes

        [Serializable]
        public class NavigationData
        {
            public List<NavigationLocation> Locations { get; set; }
            public List<NavigationGroupType> GroupTypes { get; set; }
            public List<NavigationGroup> Groups { get; set; }

            public NavigationData()
            {
                Locations = new List<NavigationLocation>();
                GroupTypes = new List<NavigationGroupType>();
                Groups = new List<NavigationGroup>();
            }

            public NavigationItem GetItem( string itemKey )
            {
                NavigationItem item = null;

                if ( itemKey.Length > 1 )
                {
                    string itemType = itemKey.Left( 1 );

                    int? itemId = itemKey.Length > 1 ? itemKey.Substring( 1 ).AsIntegerOrNull() : null;
                    if ( itemId.HasValue )
                    {
                        switch ( itemType )
                        {
                            case "L":
                                {
                                    item = Locations.Where( l => l.Id == itemId ).FirstOrDefault();
                                    break;
                                }

                            case "T":
                                {
                                    item = GroupTypes.Where( l => l.Id == itemId ).FirstOrDefault();
                                    break;
                                }

                            case "G":
                                {
                                    item = Groups.Where( l => l.Id == itemId ).FirstOrDefault();
                                    break;
                                }
                        }
                    }
                }

                return item;
            }
        }

        [Serializable]
        public abstract class NavigationItem
        {
            public int? ParentId { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public int Order { get; set; }
            public List<int> CurrentPersonIds { get; set; }
            public int CurrentCount { get { return CurrentPersonIds.Count(); } }
            public Dictionary<DateTime, List<int>> RecentPersonIds { get; set; }
            public virtual string TypeKey { get { return ""; } }
        }

        [Serializable]
        public class NavigationLocation : NavigationItem
        {
            public override string TypeKey { get { return "L"; } }
            public bool IsActive { get; set; }
            public int? SoftThreshold { get; set; }
            public int? FirmThreshold { get; set; }
            public List<int> ChildLocationIds { get; set; }
            public bool HasGroups { get; set; }

            public NavigationLocation( Location location, List<DateTime> chartTimes )
            {
                Id = location.Id;
                Name = location.Name;
                CurrentPersonIds = new List<int>();
                RecentPersonIds = new Dictionary<DateTime, List<int>>();
                chartTimes.ForEach( t => RecentPersonIds.Add( t, new List<int>() ) );
                IsActive = location.IsActive;
                SoftThreshold = location.SoftRoomThreshold;
                FirmThreshold = location.FirmRoomThreshold;
                ChildLocationIds = new List<int>();
            }
        }

        [Serializable]
        public class NavigationGroupType : NavigationItem
        {
            public override string TypeKey { get { return "T"; } }
            public List<int> ChildGroupTypeIds { get; set; }
            public List<int> ChildGroupIds { get; set; }

            public NavigationGroupType( GroupTypeCache groupType, List<DateTime> chartTimes )
            {
                Id = groupType.Id;
                Name = groupType.Name;
                Order = groupType.Order;
                CurrentPersonIds = new List<int>();
                RecentPersonIds = new Dictionary<DateTime, List<int>>();
                chartTimes.ForEach( t => RecentPersonIds.Add( t, new List<int>() ) );
                ChildGroupTypeIds = new List<int>();
                ChildGroupIds = new List<int>();
            }
        }

        [Serializable]
        public class NavigationGroup : NavigationItem
        {
            public override string TypeKey { get { return "G"; } }
            public int GroupTypeId { get; set; }
            public List<int> ChildLocationIds { get; set; }
            public List<int> ChildGroupIds { get; set; }

            public NavigationGroup( Group group, List<DateTime> chartTimes )
            {
                Id = group.Id;
                Name = group.Name;
                Order = group.Order;
                CurrentPersonIds = new List<int>();
                RecentPersonIds = new Dictionary<DateTime, List<int>>();
                chartTimes.ForEach( t => RecentPersonIds.Add( t, new List<int>() ) );
                GroupTypeId = group.GroupTypeId;
                ChildLocationIds = new List<int>();
                ChildGroupIds = new List<int>();
            }
        }

        public class PersonResult
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public string Name { get; set; }
            public Gender Gender { get; set; }
            public int? PhotoId { get; set; }
            public DateTime? LastCheckin { get; set; }
            public bool CheckedInNow { get; set; }
            public string ScheduleGroupNames { get; set; }
            public string Age { get; set; }
            public bool ShowCancel { get; set; }

            public PersonResult()
            {
                ShowCancel = false;
            }

            public PersonResult( List<Attendance> attendances )
            {
                ShowCancel = true;
                if ( attendances.Any() )
                {
                    var person = attendances.First().PersonAlias.Person;
                    Id = person.Id;
                    Guid = person.Guid;
                    Name = person.FullName;
                    Gender = person.Gender;
                    Age = person.Age.ToString() ?? "";
                    PhotoId = person.PhotoId;

                    ScheduleGroupNames = attendances
                        .Select( a => string.Format( "<span class='d-block small text-muted'>{0}{1}{2}</span>",
                                a.Occurrence.Group.Name,
                                a.Occurrence.Schedule != null ? " - " + a.Occurrence.Schedule.Name : "",
                                a.AttendanceCode != null ? " - " + a.AttendanceCode.Code : "" ) )
                        .Distinct()
                        .ToList()
                        .AsDelimited( "\r\n" );
                }
            }
        }

        #endregion


    }
}