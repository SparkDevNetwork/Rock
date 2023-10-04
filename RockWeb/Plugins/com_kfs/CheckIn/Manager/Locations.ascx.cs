using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Group = Rock.Model.Group;

namespace RockWeb.Plugins.com_kfs.CheckIn.Manager
{
    /// <summary>
    /// Block used to view current check-in counts and locations
    /// </summary>
    [DisplayName( "Locations" )]
    [Category( "KFS > Check-in > Manager" )]
    [Description( "Block used to view current check-in counts and locations." )]
    [CustomRadioListField( "Navigation Mode", "Navigation and attendance counts can be grouped and displayed either by 'Group Type > Group Type (etc) > Group > Location' or by 'location > location (etc).'  Select the navigation hierarchy that is most appropriate for your organization.", "T^Group Type,L^Location,", true, "T", "", 0, "Mode" )]
    [GroupTypeField( "Check-in Type", "The Check-in Area to display.  This value can also be overridden through the URL query string key (e.g. when navigated to from the Check-in Type selection block).", false, "", "", 1, "GroupTypeTemplate", Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE )]
    [LinkedPage( "Person Page", "The page used to display a selected person's details.", order: 2 )]
    [LinkedPage( "Area Select Page", "The page to redirect user to if area has not be configured or selected.", order: 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", order: 4, defaultValue: Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK )]
    [BooleanField( "Search By Code", "A flag indicating if security codes should also be evaluated in the search box results.", order: 5 )]
    [IntegerField( "Lookback Minutes", "The number of minutes the chart will lookback.", true, 120, order: 6 )]
    [BooleanField( "Location Active", "A flag indicating if location should currently have a schedule assigned to it to be displayed.", order: 7 )]
    [BooleanField( "Close Occurrence", "If KFS Load Balance Locations is being used in the check-in workflow, this will close the occurrence instead of the location.", order: 8 )]
    [BooleanField( "Show Delete", "A flag indicating if the Delete button should be displayed.", true, "Attendee Actions", 0 )]
    [BooleanField( "Show Checkout", "A flag indicating if the Checkout button should be displayed.", true, "Attendee Actions", 1 )]
    [BooleanField( "Show Move", "A flag indicating if the Move Attendee buttons should be displayed.", true, "Attendee Actions", 2 )]
    [BooleanField( "Include Group Move", "A flag indicating if the Groups should be presented when Move Attendee modal is displayed.", false, "Attendee Actions", 3 )]
    [BooleanField( "Show Print Label", "A flag indicating if the Print Label button should be displayed.", true, "Attendee Actions", 4 )]
    [BooleanField( "Show Advanced Print Options", "A flag indicating if the Advanced Print Options should be displayed.", false, "Print Actions", 0 )]
    [CustomDropdownListField( "Print Density", "The default print density for reprint.", "6^6 dpmm (152 dpi),8^8 dpmm (203 dpi),12^12 dpmm (300 dpi),24^24 dpmm (600 dpi)", true, "8", "Print Actions", 1 )]
    [TextField( "Label Width", "The default width of label for reprint.", true, "4", "Print Actions", 2 )]
    [TextField( "Label Height", "The default height of label for reprint.", true, "2", "Print Actions", 3 )]
    [BooleanField( "Show ZPL Print Button", "A flag indicating if the ZPL Print Button should be displayed.", false, "Print Actions", 4 )]
    public partial class Locations : Rock.Web.UI.RockBlock
    {
        #region Fields

        private string _configuredMode = "L";
        private bool _showAdvancedPrintOptions = false;
        private bool _showZplPrintButton = false;
        private bool _useKFSCloseAttribute = false;
        private int _occurrenceClosedAttributeId = 0;

        #endregion

        #region Properties

        public string CurrentCampusId { get; set; }
        public string CurrentScheduleId { get; set; }
        public string CurrentNavPath { get; set; }

        public NavigationData NavData { get; set; }

        public static bool ShowDelete { get; set; }
        public static bool ShowCheckout { get; set; }
        public static bool ShowMove { get; set; }
        public static bool ShowPrintLabel { get; set; }
        public List<int> ActiveScheduleIds { get; set; }

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
            ActiveScheduleIds = ViewState["ActiveScheduleIds"] as List<int>;
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

            ShowDelete = GetAttributeValue( "ShowDelete" ).AsBoolean();
            ShowCheckout = GetAttributeValue( "ShowCheckout" ).AsBoolean();
            ShowMove = GetAttributeValue( "ShowMove" ).AsBoolean();
            ShowPrintLabel = GetAttributeValue( "ShowPrintLabel" ).AsBoolean();
            _showAdvancedPrintOptions = GetAttributeValue( "ShowAdvancedPrintOptions" ).AsBoolean();
            _showZplPrintButton = GetAttributeValue( "ShowZPLPrintButton" ).AsBoolean();

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

            var occurrenceClosedAttribute = AttributeCache.Get( "B271037B-01AD-4270-B688-63DE29022915".AsGuid() );
            if ( GetAttributeValue( "CloseOccurrence" ).AsBoolean() && occurrenceClosedAttribute.Id > 0 )
            {
                _useKFSCloseAttribute = true;
                _occurrenceClosedAttributeId = occurrenceClosedAttribute.Id;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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
                        CurrentNavPath = GetUserPreference( "CurrentNavPath" );
                    }

                    SetChartOptions();
                    BuildNavigationControls();
                    SetPrintControls();
                }
            }
            else
            {
                nbWarning.Text = "Check-in Manager requires that a valid campus exists.";
                nbWarning.Visible = true;
                pnlContent.Visible = false;
            }
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
            ViewState["ActiveScheduleIds"] = ActiveScheduleIds;

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
                        ScheduleGroupNames = "",
                        ScheduleGroupIds = ""
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
            SetPrintControls();
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
                        tgl.Attributes["data-key"] = loc.Id.ToString();

                        int? groupId = null;
                        var pathParts = CurrentNavPath.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                        if ( pathParts.Length >= 3 && pathParts[2].StartsWith( "G" ) )
                        {
                            groupId = pathParts[2].Substring( 1 ).AsIntegerOrNull();
                            tgl.Attributes["data-group"] = groupId.ToStringSafe();
                        }

                        if ( _useKFSCloseAttribute && groupId.HasValue )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                var activeScheduleIds = new List<int>();
                                foreach ( var schedule in new ScheduleService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( s => s.CheckInStartOffsetMinutes.HasValue ) )
                                {
                                    if ( schedule.WasScheduleOrCheckInActive( GetCampusTime() ) )
                                    {
                                        activeScheduleIds.Add( schedule.Id );
                                    }
                                }

                                var occurrences = new AttendanceOccurrenceService( rockContext )
                                                        .Queryable()
                                                        .Where( o =>
                                                            o.OccurrenceDate == RockDateTime.Today &&
                                                            o.GroupId == groupId &&
                                                            o.LocationId == loc.Id &&
                                                            ( o.ScheduleId.HasValue && activeScheduleIds.Contains( o.ScheduleId.Value ) ) )
                                                        .ToList();

                                if ( occurrences.Any() )
                                {
                                    var occurrence = occurrences.FirstOrDefault();
                                    occurrence.LoadAttributes();
                                    tgl.Checked = !( occurrence.GetAttributeValue( "com.kfs.OccurrenceClosed" ).AsBoolean( false ) );
                                }
                                else
                                {
                                    tgl.Checked = true;
                                }
                            }
                        }
                        else
                        {
                            tgl.Checked = loc.IsActive;
                        }
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
                    img.Text = Rock.Model.Person.GetPersonPhotoImageTag( person.Id, person.PhotoId, person.Age.AsIntegerOrNull(), person.Gender, null, 50, 50 );
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
                        lAge.Text = string.Format( "{0}<small>(Age: {1})</small>",
                            string.IsNullOrWhiteSpace( person.ScheduleGroupNames ) ? "<br/>" : " ",
                            person.Age );
                    }
                }

                var lbPrintLabel = e.Item.FindControl( "lbPrintLabel" ) as LinkButton;
                if ( lbPrintLabel != null )
                {
                    lbPrintLabel.CommandArgument = string.Format( "{0}^{1}", person.Id, person.ScheduleGroupIds );
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
        /// Handles the Click event of the lbMoveAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMoveAll_Click( object sender, EventArgs e )
        {
            var lb = sender as LinkButton;
            if ( lb != null )
            {
                int? id = lb.Attributes["data-key"].AsIntegerOrNull();
                string people = lb.Attributes["data-people"];
                if ( id.HasValue && !string.IsNullOrWhiteSpace( people ) )
                {
                    ShowDialog( "MoveLocation", people, id.ToString(), true );
                }
            }

            BuildNavigationControls();
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
                int? groupId = tgl.Attributes["data-group"].AsIntegerOrNull();

                if ( _useKFSCloseAttribute && groupId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var occurrences = new AttendanceOccurrenceService( rockContext )
                                                .Queryable()
                                                .Where( o =>
                                                    o.OccurrenceDate == RockDateTime.Today &&
                                                    o.GroupId == groupId &&
                                                    o.LocationId == id &&
                                                    ( o.ScheduleId.HasValue && ActiveScheduleIds.Contains( o.ScheduleId.Value ) ) )
                                                .ToList();

                        if ( ActiveScheduleIds.Count > occurrences.Count )
                        {
                            var occurrenceScheduleIds = occurrences.Select( o => o.ScheduleId.Value ).ToList();
                            foreach ( var scheduleId in ActiveScheduleIds )
                            {
                                if ( !occurrenceScheduleIds.Contains( scheduleId ) )
                                {
                                    // create occurrence
                                    var occurrenceService = new AttendanceOccurrenceService( rockContext );
                                    var occurrence = new AttendanceOccurrence
                                    {
                                        ScheduleId = scheduleId,
                                        GroupId = groupId,
                                        LocationId = id,
                                        OccurrenceDate = RockDateTime.Today,
                                        Guid = new Guid()
                                    };
                                    occurrenceService.Add( occurrence );
                                    rockContext.SaveChanges();

                                    // set attribute
                                    occurrence.LoadAttributes();
                                    {
                                        occurrence.SetAttributeValue( "com.kfs.OccurrenceClosed", ( !tgl.Checked ).ToString() );
                                        occurrence.SaveAttributeValue( "com.kfs.OccurrenceClosed" );
                                        rockContext.SaveChanges();
                                        Rock.CheckIn.KioskDevice.Clear();
                                    }
                                }
                            }
                        }

                        if ( occurrences.Any() )
                        {
                            foreach ( var occurrence in occurrences )
                            {
                                occurrence.LoadAttributes();
                                {
                                    occurrence.SetAttributeValue( "com.kfs.OccurrenceClosed", ( !tgl.Checked ).ToString() );
                                    occurrence.SaveAttributeValue( "com.kfs.OccurrenceClosed" );
                                    rockContext.SaveChanges();
                                    Rock.CheckIn.KioskDevice.Clear();
                                }
                            }
                        }
                    }
                    NavData.Locations.Where( l => l.Id == id.Value ).ToList().ForEach( l => l.IsActive = tgl.Checked );
                }
                else if ( id.HasValue )
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

            if ( e.CommandName == "Checkout" )
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
                            var dayStart = RockDateTime.Today;
                            var now = GetCampusTime();

                            using ( var rockContext = new RockContext() )
                            {
                                var attendanceService = new AttendanceService( rockContext );
                                foreach ( var attendance in attendanceService
                                    .Queryable()
                                    .Where( a =>
                                        a.StartDateTime > dayStart &&
                                        a.StartDateTime < now &&
                                        !a.EndDateTime.HasValue &&
                                        a.Occurrence.LocationId.HasValue &&
                                        a.Occurrence.LocationId.Value == itemId.Value &&
                                        a.PersonAlias != null &&
                                        a.PersonAlias.PersonId == personId &&
                                        a.DidAttend.HasValue &&
                                        a.DidAttend.Value &&
                                        a.Occurrence.ScheduleId.HasValue &&
                                        ActiveScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) ) )
                                {
                                    attendance.EndDateTime = now;
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

            if ( e.CommandName == "Move" )
            {
                var personId = e.CommandArgument.ToString();
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
                            ShowDialog( "MoveLocation", personId, itemId.Value.ToString(), true );
                        }
                    }
                }
            }

            if ( e.CommandName == "PrintLabel" )
            {
                litLabel.Text = string.Empty;
                var commandArgs = e.CommandArgument.ToString().Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                var personId = commandArgs[0];
                var groupIds = commandArgs[1];

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
                            ddlLabelToPrint.Items.Clear();
                            ddlLabelToPrint.Items.Add( "" );
                            using ( var rockContext = new RockContext() )
                            {
                                foreach ( var groupId in groupIds.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                                {
                                    var group = new GroupService( rockContext ).Get( groupId.AsInteger() );
                                    var groupType = group.GroupType;
                                    groupType.LoadAttributes();
                                    var labels = new List<KioskLabel>();

                                    foreach ( var attribute in groupType.Attributes.OrderBy( a => a.Value.Order ) )
                                    {
                                        if ( attribute.Value.FieldType.Guid == Rock.SystemGuid.FieldType.LABEL.AsGuid() &&
                                            attribute.Value.QualifierValues.ContainsKey( "binaryFileType" ) &&
                                            attribute.Value.QualifierValues["binaryFileType"].Value.Equals( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL, StringComparison.OrdinalIgnoreCase ) )
                                        {
                                            Guid? binaryFileGuid = groupType.GetAttributeValue( attribute.Key ).AsGuidOrNull();
                                            if ( binaryFileGuid != null )
                                            {
                                                var labelCache = KioskLabel.Get( binaryFileGuid.Value );
                                                labelCache.Order = attribute.Value.Order;
                                                if ( labelCache != null )
                                                {
                                                    labels.Add( labelCache );
                                                }
                                            }
                                        }
                                    }

                                    if ( labels.Any() )
                                    {
                                        foreach ( var label in labels )
                                        {
                                            var file = new BinaryFileService( rockContext ).Get( label.Guid );

                                            var listItem = new ListItem( file.FileName, string.Format( "{0}^{1}", label.Guid, groupId ) );
                                            listItem.Attributes.Add( "OptionGroup", group.Name );
                                            ddlLabelToPrint.Items.Add( listItem );
                                        }
                                    }
                                }
                            }

                            ShowDialog( "PrintLabel", personId, itemId.Value.ToString(), true, groupIds );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgMoveLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgMoveLocation_SaveClick( object sender, EventArgs e )
        {
            var newLocationId = lpNewLocation.Location.Id;
            lpNewLocation.Location = null;
            int? newGroupId = null;
            if ( GetAttributeValue( "IncludeGroupMove" ).AsBoolean() )
            {
                newGroupId = rdlNewGroup.SelectedValueAsInt();
            }

            var personIds = hfPersonId.Value;
            var locationId = hfLocationId.Value.AsInteger();

            if ( !string.IsNullOrWhiteSpace( personIds ) && locationId > 0 && newLocationId > 0 )
            {
                var dayStart = RockDateTime.Today;
                var now = GetCampusTime();

                using ( var rockContext = new RockContext() )
                {
                    foreach ( var personId in personIds.Split( ',' ).AsIntegerList() )
                    {
                        var movedAttendance = new List<Attendance>();

                        var attendanceService = new AttendanceService( rockContext );
                        foreach ( var attendance in attendanceService
                            .Queryable()
                            .Where( a =>
                                a.StartDateTime > dayStart &&
                                a.StartDateTime < now &&
                                !a.EndDateTime.HasValue &&
                                a.Occurrence.LocationId.HasValue &&
                                a.Occurrence.LocationId.Value == locationId &&
                                a.PersonAlias != null &&
                                a.PersonAlias.PersonId == personId &&
                                a.DidAttend.HasValue &&
                                a.DidAttend.Value &&
                                a.Occurrence.ScheduleId.HasValue &&
                                ActiveScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) ) )
                        {
                            var newAttendance = new Attendance();
                            var groupId = newGroupId ?? attendance.Occurrence.GroupId;
                            newAttendance = attendanceService.AddOrUpdate( attendance.PersonAliasId, now, groupId, newLocationId, attendance.Occurrence.ScheduleId, attendance.CampusId, attendance.DeviceId, attendance.SearchTypeValueId, attendance.SearchValue, attendance.SearchResultGroupId, attendance.AttendanceCodeId );
                            newAttendance.EndDateTime = null;
                            movedAttendance.Add( newAttendance );
                            attendance.EndDateTime = now;
                            Rock.CheckIn.KioskLocationAttendance.AddAttendance( newAttendance );
                        }

                        attendanceService.AddRange( movedAttendance );

                        rockContext.SaveChanges();

                        Rock.CheckIn.KioskLocationAttendance.Remove( locationId );
                    }
                }

                int? campusId = CurrentCampusId.AsIntegerOrNull();
                if ( campusId.HasValue )
                {
                    int? scheduleId = CurrentScheduleId.AsIntegerOrNull();
                    NavData = GetNavigationData( CampusCache.Get( campusId.Value ), scheduleId );
                }
                BuildNavigationControls();
            }
            HideDialog();
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
            var options = new ChartOptions();
            options.series = new SeriesOptions( false, true, false );
            options.xaxis = new AxisOptions { mode = AxisMode.time };
            options.grid = new GridOptions { hoverable = true, clickable = false };

            options.SetChartStyle( GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );

            options.xaxis.timeformat = "%I:%M";

            hfChartOptions.Value = JsonConvert.SerializeObject( options, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore } );
        }

        private void RegisterStartupScript()
        {
            var options = new ChartOptions();
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

    $('.js-threshold-btn-edit').click(function(e){{
        var $parentDiv = $(this).closest('div.js-threshold');
        $parentDiv.find('.js-threshold-nb').val($parentDiv.find('.js-threshold-hf').val());
        $parentDiv.find('.js-threshold-view').hide();
        $parentDiv.find('.js-threshold-edit').show();
    }});

    $('a.js-threshold-edit').click(function(e){{
        var $parentDiv = $(this).closest('div.js-threshold');
        $parentDiv.find('.js-threshold-edit').hide();
        $parentDiv.find('.js-threshold-view').show();
        return true;
    }});

    $('.js-threshold').click(function(e){{
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
            if ( ActiveScheduleIds == null )
            {
                ActiveScheduleIds = new List<int>();
            }
            using ( var rockContext = new RockContext() )
            {
                var occurrences = new List<AttendanceOccurrence>();
                var schedules = new List<Schedule>();

                if ( scheduleId.HasValue )
                {
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

                var chartTimes = GetChartTimes( campus );
                var minDate = RockDateTime.Today.AddDays( -1 );
                foreach ( DateTime chartTime in chartTimes )
                {
                    // Get the active schedules
                    foreach ( var schedule in schedules )
                    {
                        if ( schedule.WasScheduleOrCheckInActive( chartTime ) )
                        {
                            ActiveScheduleIds.Add( schedule.Id );

                            var currentAttendance = new AttendanceService( rockContext )
                                .Queryable()
                                .Where( a =>
                                    a.StartDateTime > minDate &&
                                    !a.EndDateTime.HasValue &&
                                    a.Occurrence.LocationId.HasValue &&
                                    a.PersonAlias != null &&
                                    a.DidAttend.HasValue &&
                                    a.DidAttend.Value &&
                                    a.Occurrence.ScheduleId.HasValue &&
                                    a.Occurrence.ScheduleId == schedule.Id )
                                .ToList();

                            if ( currentAttendance.Any() )
                            {
                                foreach ( var attendance in currentAttendance )
                                {
                                    if ( attendance.Occurrence.LocationId.HasValue && !occurrences.Any( o => o.Id == attendance.OccurrenceId ) )
                                    {
                                        occurrences.Add( attendance.Occurrence );
                                    }
                                }
                            }
                        }
                    }
                }

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

                // Get location Ids which are currently active
                var validLocationIdsFiltered = new List<int>();
                if ( GetAttributeValue( "LocationActive" ).AsBoolean() )
                {
                    validLocationIdsFiltered = new GroupLocationService( rockContext )
                        .GetActiveByLocations( validLocationids )
                        .Where( l => l.Schedules.Any( s => s.IsActive && ActiveScheduleIds.Contains( s.Id ) ) )
                        .Select( l => l.LocationId )
                        .ToList();
                }
                else
                {
                    validLocationIdsFiltered = validLocationids;
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
                            .Where( l => validLocationIdsFiltered.Contains( l.LocationId ) )
                            .Select( l => l.LocationId )
                            .ToList();

                        // look for locations in use by this group currently and add to the childLocationIds
                        foreach ( var occurrence in occurrences )
                        {
                            if ( occurrence.GroupId == group.Id && !childLocationIds.Contains( occurrence.LocationId.Value ) && validLocationids.Contains( occurrence.LocationId.Value ) )
                            {
                                childLocationIds.Add( occurrence.LocationId.Value );
                            }
                        }

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
                    }

                    var attendanceList = attendanceQry.ToList();

                    foreach ( DateTime chartTime in chartTimes )
                    {
                        bool current = chartTime.Equals( chartTimes.Max() );

                        ActiveScheduleIds.Clear();

                        // Get the active schedules
                        foreach ( var schedule in schedules )
                        {
                            if ( schedule.WasScheduleOrCheckInActive( chartTime ) )
                            {
                                ActiveScheduleIds.Add( schedule.Id );
                            }
                        }

                        foreach ( var groupLocSched in attendanceList
                            .Where( a =>
                                a.StartDateTime < chartTime &&
                                a.PersonAlias != null &&
                                ActiveScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) )
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
            var min = GetAttributeValue( "LookbackMinutes" ).AsInteger();
            var time = endTime.AddMinutes( -min );

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

        private void SetPrintControls()
        {
            if ( ShowPrintLabel )
            {
                ddlLabelToPrint.Required = _showZplPrintButton;
                rcwPrint.Visible = _showZplPrintButton;
                wpAdvancedPrintOptions.Visible = _showAdvancedPrintOptions;

                ddlPrintDensity.SelectedValue = GetAttributeValue( "PrintDensity" );
                nbLabelWidth.Text = GetAttributeValue( "LabelWidth" );
                nbLabelHeight.Text = GetAttributeValue( "LabelHeight" );
            }
        }

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

                SetUserPreference( "CurrentNavPath", CurrentNavPath );

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

                            if ( itemId != null )
                            {
                                var location = new LocationService( new RockContext() ).Get( ( int ) itemId );
                                if ( location != null )
                                {
                                    lpNewLocation.Location = location;
                                }
                            }

                            if ( GetAttributeValue( "IncludeGroupMove" ).AsBoolean() )
                            {
                                string groupTypeKey = pathParts[1];
                                int? groupTypeId = groupTypeKey.Length > 1 ? groupTypeKey.Substring( 1 ).AsIntegerOrNull() : null;

                                string groupItemKey = pathParts[numParts - 2];
                                string groupItemType = groupItemKey.Left( 1 );
                                int? currentGroupId = groupItemKey.Length > 1 ? groupItemKey.Substring( 1 ).AsIntegerOrNull() : null;

                                if ( groupTypeId.HasValue && currentGroupId.HasValue )
                                {
                                    rdlNewGroup.Visible = true;

                                    rdlNewGroup.SelectedValue = null;
                                    rdlNewGroup.Items.Clear();

                                    rdlNewGroup.Items.Add( new ListItem() );

                                    var rockContext = new RockContext();
                                    var groupService = new Rock.Model.GroupService( rockContext );
                                    var groups = groupService.Queryable().Where( r => r.GroupTypeId == groupTypeId.Value ).OrderBy( a => a.Name ).ToList();

                                    foreach ( var r in groups )
                                    {
                                        var groupListItem = new ListItem( r.Name, r.Id.ToString().ToUpper() );
                                        groupListItem.Selected = r.Id == currentGroupId;
                                        rdlNewGroup.Items.Add( groupListItem );
                                    }
                                }
                            }

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

                        if ( pathParts.Length >= 3 && pathParts[2].StartsWith( "G" ) )
                        {
                            tglHeadingRoom.Attributes["data-group"] = pathParts[2].Substring( 1 );
                        }

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

                        if ( ShowMove && attendees.Any() )
                        {
                            var attendeeIds = new List<int>();
                            var attendeeList = attendees.ToList();
                            foreach ( var attendee in attendeeList )
                            {
                                attendeeIds.Add( attendee.PersonAlias.PersonId );
                            }

                            lbMoveAll.Visible = true;
                            lbMoveAll.Attributes["data-key"] = locationItem.Id.ToString();
                            lbMoveAll.Attributes["data-people"] = string.Join( ",", attendeeIds );
                        }
                        else
                        {
                            lbMoveAll.Visible = false;
                        }
                    }
                    else
                    {
                        tglHeadingRoom.Visible = false;
                        lbMoveAll.Visible = false;
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

        #region Modal

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, string personId, string locationId, bool setValues = false, string groupIds = "" )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            hfPersonId.Value = personId;
            hfLocationId.Value = locationId;
            hfGroupIds.Value = groupIds;
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "MOVELOCATION":
                    dlgMoveLocation.Show();
                    break;
                case "PRINTLABEL":
                    dlgPrintLabel.Show();
                    ViewState["LabelJSON"] = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "MOVELOCATION":
                    dlgMoveLocation.Hide();
                    break;
                case "PRINTLABEL":
                    dlgPrintLabel.Hide();
                    ViewState["LabelJSON"] = string.Empty;
                    break;
            }

            hfActiveDialog.Value = string.Empty;
            hfPersonId.Value = string.Empty;
            hfLocationId.Value = string.Empty;
            hfGroupIds.Value = string.Empty;
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
            public string ScheduleGroupIds { get; set; }
            public string Age { get; set; }
            public bool ShowCancel { get; set; }
            public bool ShowCheckout { get; set; }
            public bool ShowMove { get; set; }
            public bool ShowPrintLabel { get; set; }

            public PersonResult()
            {
                ShowCancel = false;
                ShowCheckout = false;
                ShowMove = false;
                ShowPrintLabel = false;
            }

            public PersonResult( List<Attendance> attendances )
            {
                ShowCancel = Locations.ShowDelete;
                ShowCheckout = Locations.ShowCheckout;
                ShowMove = Locations.ShowMove;
                ShowPrintLabel = Locations.ShowPrintLabel;
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
                        .Select( a => string.Format( "<br/><small>{0}{1}{2}</small>",
                                a.Occurrence.Group.Name,
                                a.Occurrence.Schedule != null ? " - " + a.Occurrence.Schedule.Name : "",
                                a.AttendanceCode != null ? " - " + a.AttendanceCode.Code : "" ) )
                        .Distinct()
                        .ToList()
                        .AsDelimited( "\r\n" );

                    ScheduleGroupIds = attendances
                        .Select( a => a.Occurrence.Group.Id )
                        .Distinct()
                        .ToList()
                        .AsDelimited( "," );
                }
            }
        }

        #endregion

        #region Print

        protected void btnViewLabel_Click( object sender, EventArgs e )
        {
            var labelJson = string.Empty;
            var ddlValues = ddlLabelToPrint.SelectedValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
            if ( ddlValues.Any() )
            {
                var labelGuid = ddlValues[0].AsGuid();
                var groupId = ddlValues[1].AsInteger();
                var personId = hfPersonId.Value.AsInteger();

                if ( personId > 0 && groupId > 0 && labelGuid != Guid.Empty )
                {
                    var dayStart = RockDateTime.Today;
                    var now = GetCampusTime();
                    var locationId = hfLocationId.Value.AsInteger();

                    var schedule = new CheckInSchedule();
                    var location = new CheckInLocation();
                    var group = new CheckInGroup();
                    var groupType = new CheckInGroupType();
                    var person = new CheckInPerson();
                    var people = new List<CheckInPerson>();

                    using ( var rockContext = new RockContext() )
                    {
                        var rockPerson = new Rock.Model.Person();
                        var code = string.Empty;
                        var schedules = new List<Schedule>();

                        var attendanceService = new AttendanceService( rockContext );
                        var firstTime = !attendanceService
                                .Queryable()
                                .AsNoTracking()
                                .Where( a =>
                                    a.PersonAlias != null &&
                                    a.PersonAlias.PersonId == personId &&
                                    a.StartDateTime < dayStart
                                    )
                                .Any();

                        foreach ( var attendance in attendanceService
                                        .Queryable()
                                        .AsNoTracking()
                                        .Where( a =>
                                            a.StartDateTime > dayStart &&
                                            a.StartDateTime < now &&
                                            !a.EndDateTime.HasValue &&
                                            a.Occurrence.LocationId.HasValue &&
                                            a.Occurrence.LocationId.Value == locationId &&
                                            a.PersonAlias != null &&
                                            a.PersonAlias.PersonId == personId &&
                                            a.DidAttend.HasValue &&
                                            a.DidAttend.Value &&
                                            a.Occurrence.ScheduleId.HasValue &&
                                            ActiveScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) )
                                        .OrderBy( t => t.Occurrence.Schedule.WeeklyTimeOfDay )
                                        .ThenBy( t => t.Occurrence.Schedule.Name ) )
                        {
                            rockPerson = attendance.PersonAlias.Person;
                            code = attendance.AttendanceCode.ToString();
                            if ( !schedules.Contains( attendance.Occurrence.Schedule ) )
                            {
                                schedules.Add( attendance.Occurrence.Schedule );
                                var test = attendance.Occurrence.Location;
                            }
                        }

                        schedule.Schedule = schedules.OrderBy( t => t.WeeklyTimeOfDay ).ThenBy( t => t.Name ).FirstOrDefault();

                        var loc = new LocationService( rockContext )
                            .Queryable()
                            .AsNoTracking()
                            .Where( l => l.Id == locationId )
                            .FirstOrDefault();

                        foreach ( var s in schedules )
                        {
                            var sch = new CheckInSchedule();
                            sch.Schedule = s;
                            location.Location = loc;
                            location.Schedules.Add( sch );
                            location.SelectedForSchedule.Add( s.Id );
                        }

                        var gr = new GroupService( rockContext )
                            .Queryable()
                            .AsNoTracking()
                            .Where( g => g.Id == groupId )
                            .FirstOrDefault();

                        group.Group = gr;
                        group.Locations.Add( location );
                        group.SelectedForSchedule.Add( schedule.Schedule.Id );

                        groupType.GroupType = GroupTypeCache.Get( gr.GroupTypeId );
                        groupType.Groups.Add( group );
                        groupType.SelectedForSchedule.Add( schedule.Schedule.Id );

                        person.Person = rockPerson;
                        person.SecurityCode = code;
                        person.GroupTypes.Add( groupType );
                        person.FirstTime = firstTime;
                        people.Add( person );
                    }

                    var dpmm = ddlPrintDensity.SelectedValue;
                    var width = nbLabelWidth.Text;
                    var height = nbLabelHeight.Text;
                    var index = nbShowLabel.Text;
                    var zpl = string.Empty;

                    var kioskLabel = KioskLabel.Get( labelGuid );
                    if ( kioskLabel != null )
                    {
                        var mergeObjects = new Dictionary<string, object>();
                        var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        foreach ( var keyValue in commonMergeFields )
                        {
                            mergeObjects.Add( keyValue.Key, keyValue.Value );
                        }

                        mergeObjects.Add( "Location", location );
                        mergeObjects.Add( "Group", group );
                        mergeObjects.Add( "Person", person );
                        mergeObjects.Add( "People", people );
                        mergeObjects.Add( "GroupType", groupType );

                        var checkinLabel = new CheckInLabel( kioskLabel, mergeObjects, personId );
                        var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );
                        checkinLabel.LabelFile = urlRoot + checkinLabel.LabelFile;
                        checkinLabel.FileGuid = kioskLabel.Guid;
                        checkinLabel.PrintTo = PrintTo.Kiosk;
                        //
                        // tbd if null for these messes anything up
                        //
                        //checkinLabel.PrinterDeviceId = null;
                        //checkinLabel.PrinterAddress = null;
                        //
                        //
                        var checkinLabelList = new List<CheckInLabel>();
                        checkinLabelList.Add( checkinLabel );
                        labelJson = checkinLabelList.ToJson();

                        string printContent = kioskLabel.FileContent;
                        foreach ( var mergeField in checkinLabel.MergeFields )
                        {
                            if ( !string.IsNullOrWhiteSpace( mergeField.Value ) )
                            {
                                printContent = Regex.Replace( printContent, string.Format( @"(?<=\^FD){0}(?=\^FS)", mergeField.Key ), ZebraFormatString( mergeField.Value ) );
                            }
                            else
                            {
                                // Remove the box preceding merge field
                                printContent = Regex.Replace( printContent, string.Format( @"\^FO.*\^FS\s*(?=\^FT.*\^FD{0}\^FS)", mergeField.Key ), string.Empty );
                                // Remove the merge field
                                printContent = Regex.Replace( printContent, string.Format( @"\^FD{0}\^FS", mergeField.Key ), "^FD^FS" );
                            }
                        }
                        zpl = printContent;
                    }

                    byte[] zplBytes = Encoding.UTF8.GetBytes( zpl );

                    var labelaryUrl = string.Format( "http://api.labelary.com/v1/printers/{0}dpmm/labels/{1}x{2}/{3}", dpmm, width, height, ( index != string.Empty ? index + "/" : string.Empty ) );
                    var request = ( HttpWebRequest ) WebRequest.Create( labelaryUrl );
                    request.Method = "POST";
                    request.Accept = "application/pdf"; // omit this line to get PNG images back
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = zplBytes.Length;

                    var requestStream = request.GetRequestStream();
                    requestStream.Write( zplBytes, 0, zplBytes.Length );
                    requestStream.Close();

                    try
                    {
                        var response = ( HttpWebResponse ) request.GetResponse();
                        var responseStream = response.GetResponseStream();
                        var memoryStream = new MemoryStream();

                        byte[] buffer = new byte[16 * 1024];
                        int read;
                        while ( ( read = responseStream.Read( buffer, 0, buffer.Length ) ) > 0 )
                        {
                            memoryStream.Write( buffer, 0, read );
                        }

                        var labelPath = string.Format( "~/Cache/{0}_{1}_{2}_{3}.pdf", person.Person.FullName.RemoveSpaces(), personId, locationId, labelGuid );

                        byte[] bytes = memoryStream.ToArray();
                        System.IO.Directory.CreateDirectory( Server.MapPath( "~/Cache" ) );
                        System.IO.File.WriteAllBytes( Server.MapPath( labelPath ), bytes );
                        var embed = "<div class=\"pdfObject\" style=\"width:100%;height:350px;\"><object toolbar=\"true\" id=\"pdfDocument\" width=\"100%\" height=\"100%\" data=\"{0}\" type=\"application/pdf\"><p>It appears you don't have a PDF plugin for this browser. <a href=\"{0}\">click here to download the PDF file.</a></p></object></div>";
                        litLabel.Text = string.Format( embed, ResolveRockUrlIncludeRoot( labelPath ) );
                        memoryStream.Close();

                        responseStream.Close();

                        if ( hfIsEdge.Value == "true" )
                        {
                            pnlEdgeAlert.Visible = true;
                        }
                    }
                    catch ( WebException ex )
                    {
                        Console.WriteLine( "Error: {0}", ex.Status );
                    }
                }
            }
            else
            {
                litLabel.Text = string.Empty;
            }

            ViewState["LabelJSON"] = labelJson;
        }

        protected void btnPrintLabel_Click( object sender, EventArgs e )
        {
            var jsonObject = ViewState["LabelJSON"] as string;
            AddLabelScript( jsonObject );
        }

        /// <summary>
        /// Adds the label script.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

        // setup deviceready event to wait for cordova
	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/)) {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}

	    // label data
        var labelData = {0};

		function onDeviceReady() {{
            try {{			
                printLabels();
            }} 
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
		}}
		
		function alertDismissed() {{
		    // do something
		}}
		
		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData), 
            	function(result) {{ 
			        console.log('Tag printed');
			    }},
			    function(error) {{   
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    navigator.notification.alert(
                        'An error occurred while printing the labels.' + error[0],  // message
                        alertDismissed,         // callback
                        'Error',            // title
                        'Ok'                  // buttonName
                    );
			    }}
            );
	    }}
", jsonObject );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
        }

        private string ZebraFormatString( string input, bool isJson = false )
        {
            if ( isJson )
            {
                return input.Replace( "é", @"\\82" );  // fix acute e
            }
            else
            {
                return input.Replace( "é", @"\82" );  // fix acute e
            }
        }

        #endregion
    }
}
