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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard.Flot;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Locations" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block used to view current check-in counts and locations." )]

    [CustomRadioListField("Mode", "Mode to use for navigation", "L:Location,T:Check-in Area", true, "L", order: 0 )]
    [GroupTypeField( "Check-in Type", key: "GroupTypeTemplate", groupTypePurposeValueGuid: Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE, order: 1 )]
    [LinkedPage( "Person Page", "Page used to display person details", order: 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", order: 3 )]
    public partial class Locations : Rock.Web.UI.RockBlock
    {
        #region Fields


        private string _configuredMode = "L";
    
        #endregion

        #region Properties

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

            CurrentNavPath = ViewState["CurrentNavPath"] as string;
            NavData = ViewState["NavData"] as NavigationData;
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
            rptPeople.ItemDataBound += rptPeople_ItemDataBound;

            _configuredMode = GetAttributeValue( "Mode" );

            Page.Form.DefaultButton = lbSearch.UniqueID;
            Page.Form.DefaultFocus = tbSearch.UniqueID;

            RegisterStartupScript();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbGroupTypeWarning.Visible = false;

            if (NavData == null)
            {
                NavData = GetNavigationData();
            }

            if ( !Page.IsPostBack )
            {
                // Check for requested location/group
                int? locationId = PageParameter( "Location" ).AsIntegerOrNull();
                int? groupId = PageParameter( "Group" ).AsIntegerOrNull();
                if (locationId.HasValue && groupId.HasValue)
                {
                    CurrentNavPath = BuildCurrentPath( locationId.Value, groupId.Value );
                }

                if ( string.IsNullOrWhiteSpace( CurrentNavPath ) )
                {
                    CurrentNavPath = GetUserPreference( "CurrentNavPath" );
                }

                SetChartOptions();
                BuildNavigationControls();
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

            return base.SaveViewState();
        }
        #endregion

        #region Events

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            // Get all the schedules that allow checkin
            var schedules = new ScheduleService( rockContext ).Queryable()
                    .Where( s => s.CheckInStartOffsetMinutes.HasValue )
                    .ToList();

            // Get a lit of the schedule ids
            var scheduleIds = schedules.Select(s => s.Id).ToList();

            // Get a list of the schedule id that are currently active for checkin
            var activeScheduleIds = new List<int>();
            foreach (var schedule in schedules)
            {
                if (  schedule.IsScheduleOrCheckInActive )
                {
                    activeScheduleIds.Add( schedule.Id );
                }
            }

            // Get a list of all the groups that we're concerned about 
            var groupIds = NavData.Groups.Select( g => g.Id ).ToList();

            // Get a list of all the people that are currently checked in
            var today = RockDateTime.Today;
            var attendanceService = new AttendanceService(rockContext);
            var currentAttendeeIds = attendanceService.Queryable()
                .Where( a =>
                    a.ScheduleId.HasValue &&
                    a.GroupId.HasValue &&
                    a.LocationId.HasValue &&
                    a.PersonId.HasValue &&
                    a.DidAttend &&
                    a.StartDateTime > today &&
                    activeScheduleIds.Contains( a.ScheduleId.Value ) &&
                    groupIds.Contains( a.GroupId.Value ) )
                .Select( a =>
                    a.PersonId.Value )
                .Distinct();

            // Create a qry to get the last checkin date (used in next statement's join)
            var attendanceQry = attendanceService.Queryable()
                .Where( a =>
                    a.ScheduleId.HasValue &&
                    a.GroupId.HasValue &&
                    a.LocationId.HasValue &&
                    a.PersonId.HasValue &&
                    a.DidAttend &&
                    scheduleIds.Contains( a.ScheduleId.Value) && 
                    groupIds.Contains( a.GroupId.Value ) )
                .GroupBy( a => new
                {
                    PersonId = a.PersonId
                } )
                .Select( g => new PersonResult 
                {
                    Id = g.Key.PersonId.Value,
                    Guid = Guid.Empty,
                    Name = "",
                    PhotoId = null,
                    LastCheckin = g.Max( a => a.StartDateTime ),
                    CheckedInNow = false,
                    GroupName = ""
                } );

            // Do the person search
            bool reversed = false;
            var results = new PersonService( rockContext ).GetByFullName(
                tbSearch.Text, false, false, false, out reversed )
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
                                LastCheckin = c.LastCheckin,
                                CheckedInNow = currentAttendeeIds.Contains( p.Id ),
                                GroupName = ""
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
                                LastCheckin = null,
                                CheckedInNow = false,
                                GroupName = ""
                            } ) )
                .SelectMany( a => a )
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
        void rptNavItems_ItemDataBound( object sender, RepeaterItemEventArgs e )
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

                var tgl = e.Item.FindControl( "tglRoom" ) as Toggle;
                if (tgl != null)
                {   
                    var loc = navItem as NavigationLocation;
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
        /// Handles the ItemDataBound event of the rptPeople control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        void rptPeople_ItemDataBound( object sender, RepeaterItemEventArgs e )
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
                if (img != null)
                {
                    img.Text = Person.GetPhotoImageTag( person.PhotoId, person.Gender, 50, 50 );
                }

                var lStatus = e.Item.FindControl( "lStatus" ) as Literal;
                if ( lStatus != null )
                {
                    if ( person.LastCheckin.HasValue )
                    {
                        if (person.CheckedInNow)
                        {
                            lStatus.Text = "<span class='badge badge-success'>Checked In</span>";
                        }
                        else
                        {
                            lStatus.Text = person.LastCheckin.Value.ToShortDateString();
                        }
                    }
                    else
                    {
                        lStatus.Text = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the OnPostBack event of the upnlContent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PostBackEventArgs"/> instance containing the event data.</param>
        void upnlContent_OnPostBack( object sender, PostBackEventArgs e )
        {
            tbSearch.Text = string.Empty;

            if ( e.EventArgument.StartsWith( "P" ) )
            {
                var parms = new Dictionary<string, string>();
                parms.Add("Person", e.EventArgument.Substring(1));
                NavigateToLinkedPage( "PersonPage", parms );
            }
            else
            {
                CurrentNavPath = e.EventArgument;
                BuildNavigationControls();
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
            int? id = tgl.Attributes["data-key"].AsIntegerOrNull();
            if (id.HasValue)
            {
                var rockContext = new RockContext();
                var location = new LocationService( rockContext ).Get( id.Value );
                if (location != null)
                {
                    location.IsActive = tgl.Checked;
                    rockContext.SaveChanges();
                }

                NavData.Locations.Where( l => l.Id == id.Value ).ToList().ForEach( l => l.IsActive = tgl.Checked );
            }

            BuildNavigationControls();
        }

        #endregion

        #region Methods

        private string BuildCurrentPath(int locationId, int groupId)
        {
            var location = NavData.Locations.Where( l => l.Id == locationId ).FirstOrDefault();
            var group = NavData.Groups.Where( g => g.Id == groupId ).FirstOrDefault();

            if ( location != null && group != null )
            {
                var pathParts = new List<string>();

                if ( _configuredMode == "T" )
                {
                    pathParts.Add( "L" + location.Id.ToString() );
                    pathParts.Add( "G" + group.Id.ToString() );
                    var groupType = NavData.GroupTypes.Where( t => t.Id == group.GroupTypeId ).FirstOrDefault();
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

                if (pathParts.Any())
                {
                    pathParts.Reverse();
                    return pathParts.AsDelimited( "|" );
                }
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
", 
                pnlChart.ClientID, hfChartData.ClientID, 
                JsonConvert.SerializeObject( 
                    options, 
                    Formatting.Indented, 
                    new JsonSerializerSettings() { 
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore, 
                        NullValueHandling = NullValueHandling.Ignore 
                    } ) );

            ScriptManager.RegisterStartupScript( pnlChart, pnlChart.GetType(), "chart", script, true );
        }

        #region Get Navigation Data

        private NavigationData GetNavigationData()
        {
            var groupTypeTemplateGuid = this.GetAttributeValue( "GroupTypeTemplate" ).AsGuidOrNull();
            if ( groupTypeTemplateGuid.HasValue )
            {
                NavData = new NavigationData();

                var rockContext = new RockContext();

                var chartTimes = GetChartTimes();

                // Get the group types
                var parentGroupType = GroupTypeCache.Read( groupTypeTemplateGuid.Value );
                if ( parentGroupType != null )
                {
                    foreach ( var childGroupType in parentGroupType.ChildGroupTypes )
                    {
                        AddGroupType( childGroupType, chartTimes );
                    }
                }

                // Get the groups
                var groupTypeIds = NavData.GroupTypes.Select( t => t.Id ).ToList();
                foreach ( var group in new GroupService( rockContext ).Queryable()
                    .Where( g =>
                        groupTypeIds.Contains( g.GroupTypeId ) &&
                        g.IsActive ) )
                {
                    if ( group.GroupLocations.Any() )
                    {
                        var navGroup = new NavigationGroup( group, chartTimes );
                        navGroup.ChildLocationIds = group.GroupLocations.Select( g => g.LocationId ).ToList();
                        NavData.Groups.Add( navGroup );
                        NavData.GroupTypes.Where( g => g.Id == group.GroupTypeId ).ToList()
                            .ForEach( g => g.ChildGroupIds.Add( group.Id ) );
                    }
                }

                // Remove any grouptype trees without locations
                var emptyGroupTypeIds = NavData.GroupTypes
                    .Where( g => !g.ChildGroupIds.Any() && !g.ChildGroupTypeIds.Any() )
                    .Select( g => g.Id )
                    .ToList();

                while ( emptyGroupTypeIds.Any() )
                {
                    NavData.GroupTypes = NavData.GroupTypes.Where( g => !emptyGroupTypeIds.Contains( g.Id ) ).ToList();

                    NavData.GroupTypes.ForEach( g =>
                        g.ChildGroupTypeIds = g.ChildGroupTypeIds.Where( c => !emptyGroupTypeIds.Contains( c ) ).ToList() );

                    emptyGroupTypeIds = NavData.GroupTypes
                        .Where( g => !g.ChildGroupIds.Any() && !g.ChildGroupTypeIds.Any() )
                        .Select( g => g.Id )
                        .ToList();
                }

                // Get the locations
                var locationIds = NavData.Groups.SelectMany( g => g.ChildLocationIds ).Distinct().ToList();
                foreach ( var location in new LocationService( rockContext ).Queryable( "ParentLocation" )
                    .Where( l => locationIds.Contains( l.Id ) ) )
                {
                    var navLocation = AddLocation( location, chartTimes );
                    navLocation.HasGroups = true;
                }

                // Get the attendance counts
                var dayStart = RockDateTime.Today;
                var now = RockDateTime.Now;
                var groupIds = NavData.Groups.Select( g => g.Id ).ToList();

                var attendances = new AttendanceService( rockContext ).Queryable()
                    .Where( a =>
                        a.ScheduleId.HasValue &&
                        a.GroupId.HasValue &&
                        a.LocationId.HasValue &&
                        a.StartDateTime > dayStart &&
                        a.StartDateTime < now &&
                        a.DidAttend &&
                        groupIds.Contains( a.GroupId.Value ) )
                    .ToList();

                var schedules = new ScheduleService( rockContext ).Queryable()
                        .Where( s => s.CheckInStartOffsetMinutes.HasValue ) 
                        .ToList();

                foreach ( DateTime chartTime in chartTimes )
                {
                    // Get the active schedules
                    var timeOffset = new DateTimeOffset(chartTime);
                    var activeSchedules = new List<int>();
                    foreach ( var schedule in schedules )
                    {
                        if ( schedule.WasScheduleOrCheckInActive(timeOffset) )
                        {
                            activeSchedules.Add( schedule.Id );
                        }
                    }

                    bool current = chartTime.Equals( chartTimes.Max() );

                    foreach ( var itemCount in attendances
                        .Where( a => 
                            a.StartDateTime < chartTime &&
                            activeSchedules.Contains( a.ScheduleId.Value ) )
                        .GroupBy( a => new
                        {
                            ScheduleId = a.ScheduleId.Value,
                            GroupId = a.GroupId.Value,
                            LocationId = a.LocationId.Value
                        } )
                        .Select( g => new
                        {
                            ScheduleId = g.Key.ScheduleId,
                            GroupId = g.Key.GroupId,
                            LocationId = g.Key.LocationId,
                            Count = g.Count()
                        } ) )
                    {
                        AddGroupCount( chartTime, itemCount.GroupId, itemCount.Count, current );
                        AddLocationCount( chartTime, itemCount.LocationId, itemCount.Count, current );
                    }
                }
                return NavData;
            }
            else
            {
                nbGroupTypeWarning.Visible = true;
            }

            return null;

        }

        private List<DateTime> GetChartTimes()
        {
            // Get the current minute
            var now = DateTime.Now;
            now = new DateTime( now.Year, now.Month, now.Day, now.Hour, now.Minute, 0 );

            // Find the previous 5 min mark
            var prevTime = now.AddMinutes( -1 );
            while ( prevTime.Minute % 5 != 0 )
            {
                prevTime = prevTime.AddMinutes( -1 );
            }

            // Get the start time
            var time = prevTime.AddHours( -2 );

            // Get 5 min increments
            var times = new List<DateTime>();
            while ( time <= prevTime )
            {
                times.Add( time );
                time = time.AddMinutes( 5 );
            }
            times.Add( now );

            return times;
        }

        private void AddGroupType( GroupTypeCache groupType, List<DateTime> chartTimes )
        {
            if ( groupType != null && !NavData.GroupTypes.Exists( g => g.Id == groupType.Id ) )
            {
                var navGroupType = new NavigationGroupType( groupType, chartTimes );
                NavData.GroupTypes.Add( navGroupType );

                foreach ( var childGroupType in groupType.ChildGroupTypes )
                {
                    AddGroupType( childGroupType, chartTimes );
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

        private void AddGroupCount( DateTime time, int groupId, int count, bool current )
        {
            var navGroup = NavData.Groups.FirstOrDefault( g => g.Id == groupId );
            if ( navGroup != null )
            {
                if (!navGroup.RecentCounts.ContainsKey(time))
                {
                    navGroup.RecentCounts.Add( time, 0 );
                }
                navGroup.RecentCounts[time] += count;

                if ( current )
                {
                    navGroup.CurrentCount += count;
                }

                AddGroupTypeCount( time, navGroup.GroupTypeId, count, current );
            }
        }

        private void AddGroupTypeCount( DateTime time, int groupTypeId, int count, bool current )
        {
            var navGroupType = NavData.GroupTypes.FirstOrDefault( g => g.Id == groupTypeId );
            if ( navGroupType != null )
            {
                if ( !navGroupType.RecentCounts.ContainsKey( time ) )
                {
                    navGroupType.RecentCounts.Add( time, 0 );
                }
                navGroupType.RecentCounts[time] += count;

                if ( current )
                {
                    navGroupType.CurrentCount += count;
                }

                if ( navGroupType.ParentId.HasValue )
                {
                    AddGroupTypeCount( time, navGroupType.ParentId.Value, count, current );
                }
            }
        }

        private void AddLocationCount( DateTime time, int locationId, int count, bool current )
        {
            var navLocation = NavData.Locations.FirstOrDefault( g => g.Id == locationId );
            if ( navLocation != null )
            {
                if ( !navLocation.RecentCounts.ContainsKey( time ) )
                {
                    navLocation.RecentCounts.Add( time, 0 );
                }
                navLocation.RecentCounts[time] += count;

                if (current)
                {
                    navLocation.CurrentCount += count;
                }

                if ( navLocation.ParentId.HasValue )
                {
                    AddLocationCount( time, navLocation.ParentId.Value, count, current );
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

                SetUserPreference( "CurrentNavPath", CurrentNavPath );

                var pathParts = CurrentNavPath.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                int numParts = pathParts.Length - 1;

                string itemKey = pathParts[numParts];
                string itemType = itemKey.Left( 1 );
                int? itemId = itemKey.Length > 1 ? itemKey.Substring( 1 ).AsIntegerOrNull() : null;
                NavigationItem item = NavData.GetItem( pathParts[numParts] );

                var navItems = new List<NavigationItem>();

                switch ( itemType )
                {
                    case "L":   // Location
                        {
                            NavData.Locations
                                .Where( l => l.ParentId == itemId )
                                .ToList().ForEach( l => navItems.Add( l ) );

                            break;
                        }

                    case "T":   // Group Type
                        {
                            NavData.GroupTypes
                                .Where( t => t.ParentId.Equals(itemId) )
                                .ToList().ForEach( t => navItems.Add( t ) );

                            NavData.Groups
                                .Where( g => g.GroupTypeId == itemId )
                                .ToList().ForEach( g => navItems.Add( g ) );

                            break;
                        }

                    case "G":   // Group
                        {
                            var locations = new List<int>();
                            NavData.Groups
                                .Where( g => g.Id == itemId )
                                .ToList()
                                .ForEach( g =>
                                    g.ChildLocationIds
                                        .ForEach( l => locations.Add( l ) ) );

                            NavData.Locations
                                .Where( l => locations.Contains( l.Id ) )
                                .ToList().ForEach( l => navItems.Add( l ) );

                            break;
                        }

                }

                // Get chart data
                Dictionary<DateTime, int> chartCounts = null;
                if (item != null)
                {
                    chartCounts = item.RecentCounts;
                }
                else
                {
                    chartCounts = new Dictionary<DateTime, int>();
                    foreach(var navItem in navItems)
                    {
                        foreach(var kv in navItem.RecentCounts)
                        {
                            if ( !chartCounts.ContainsKey( kv.Key ) )
                            {
                                chartCounts.Add( kv.Key, 0 );
                            }
                            chartCounts[kv.Key] += kv.Value;
                        }
                    }
                }

                var chartData = new List<string>();
                TimeSpan baseSpan = new TimeSpan( DateTime.Parse( "1/1/1970" ).Ticks );
                foreach ( var kv in chartCounts.OrderBy( c => c.Key ) )
                {
                    DateTime offsetTime = kv.Key.Subtract( baseSpan );
                    long ticks = (long)( offsetTime.Ticks / 10000 );
                    chartData.Add( string.Format( "[{0}, {1}]", ticks, kv.Value ) );
                }
                hfChartData.Value = string.Format( "[ [ {0} ] ]", chartData.AsDelimited( ", " ) );

                pnlNavHeading.Visible = item != null;
                if ( item != null )
                {
                    pnlNavHeading.Attributes["onClick"] = upnlContent.GetPostBackEventReference(
                        pathParts.ToList().Take( numParts ).ToList().AsDelimited( "|" ) );
                    lNavHeading.Text = item.Name;

                    var locationItem = item as NavigationLocation;
                    if (locationItem != null && locationItem.HasGroups)
                    {
                        tglHeadingRoom.Visible = true;
                        tglHeadingRoom.Checked = locationItem.IsActive;
                        tglHeadingRoom.Attributes["data-key"] = locationItem.Id.ToString();

                        var rockContext = new RockContext();
                        var activeSchedules = new List<int>();
                        foreach ( var schedule in new ScheduleService( rockContext ).Queryable()
                            .Where( s => s.CheckInStartOffsetMinutes.HasValue ) )
                        {
                            if ( schedule.IsScheduleOrCheckInActive )
                            {
                                activeSchedules.Add( schedule.Id );
                            }
                        }

                        var dayStart = RockDateTime.Today;
                        var now = RockDateTime.Now;
                        var attendees = new AttendanceService( rockContext ).Queryable( "Group" )
                            .Where( a =>
                                a.ScheduleId.HasValue &&
                                a.GroupId.HasValue &&
                                a.LocationId.HasValue &&
                                a.LocationId == locationItem.Id &&
                                a.StartDateTime > dayStart &&
                                a.StartDateTime < now &&
                                a.DidAttend &&
                                activeSchedules.Contains( a.ScheduleId.Value ) )
                            .Distinct()
                            .OrderBy( a => a.Person.NickName )
                            .ThenBy( a => a.Person.LastName )
                            .ToList();

                        var results = attendees
                            .Select( a => new PersonResult
                            {
                                Id = a.Person.Id,
                                Guid = a.Person.Guid,
                                Name = a.Person.FullName,
                                Gender = a.Person.Gender,
                                PhotoId = a.Person.PhotoId,
                                GroupName = a.Group.Name
                            } );

                        rptPeople.Visible = true;
                        rptPeople.DataSource = results;
                        rptPeople.DataBind();
                    }
                    else
                    {
                        tglHeadingRoom.Visible = false;
                        rptPeople.Visible = false;
                    }
                }

                rptNavItems.Visible = navItems.Any();
                rptNavItems.DataSource = navItems
                    .OrderBy( i => i.TypeKey )
                    .ThenBy( i => i.Order )
                    .ThenBy( i => i.Name );
                rptNavItems.DataBind();
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

            public NavigationItem GetItem (string itemKey)
            {
                NavigationItem item = null;

                if ( itemKey.Length > 1 )
                {
                    string itemType = itemKey.Left( 1 );
                    
                    int? itemId = itemKey.Length > 1 ? itemKey.Substring( 1 ).AsIntegerOrNull() : null;
                    if (itemId.HasValue)
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
            public int CurrentCount { get; set; }
            public Dictionary<DateTime, int> RecentCounts { get; set; }
            public virtual string TypeKey { get { return ""; } }
        }

        [Serializable]
        public class NavigationLocation : NavigationItem
        {
            public override string TypeKey { get { return "L"; } }
            public bool IsActive { get; set; }
            public List<int> ChildLocationIds { get; set; }
            public bool HasGroups { get; set; }
            public NavigationLocation( Location location, List<DateTime> chartTimes )
            {
                Id = location.Id;
                Name = location.Name;
                RecentCounts = new Dictionary<DateTime, int>();
                chartTimes.ForEach( t => RecentCounts.Add( t, 0 ) );
                IsActive = location.IsActive;
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
                RecentCounts = new Dictionary<DateTime, int>();
                chartTimes.ForEach( t => RecentCounts.Add( t, 0 ) );
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
            public NavigationGroup( Group group, List<DateTime> chartTimes )
            {
                Id = group.Id;
                Name = group.Name;
                Order = group.Order;
                RecentCounts = new Dictionary<DateTime, int>();
                chartTimes.ForEach( t => RecentCounts.Add( t, 0 ) );
                GroupTypeId = group.GroupTypeId;
                ChildLocationIds = new List<int>();
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
            public string GroupName { get; set; }
        }

        #endregion
}
}