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

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "Live Metrics" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block used to view current check-in counts and locations." )]

    #region Block Attributes

    [CustomRadioListField(
        "Navigation Mode",
        Description = "Navigation and attendance counts can be grouped and displayed either by 'Group Type > Group Type (etc) > Group > Location' or by 'location > location (etc).'  Select the navigation hierarchy that is most appropriate for your organization.",
        ListSource = "T^Group Type,L^Location,",
        IsRequired = true,
        DefaultValue = "T",
        Order = 0,
        Key = AttributeKey.Mode )]
    [GroupTypeField(
        "Check-in Type",
        Description = "The Check-in Area to display.  This value can also be overridden through the URL query string key (e.g. when navigated to from the Check-in Type selection block).",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.GroupTypeTemplate,
        GroupTypePurposeValueGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE )]
    [LinkedPage(
        "Person Page",
        Description = "The page used to display a selected person's details.",
        Order = 2,
        Key = AttributeKey.PersonPage )]
    [LinkedPage(
        "Area Select Page",
        Description = "The page to redirect user to if area has not be configured or selected.",
        Order = 3,
        Key = AttributeKey.AreaSelectPage )]

    #endregion Block Attributes
    public partial class LiveMetrics : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Mode = "Mode";
            public const string GroupTypeTemplate = "GroupTypeTemplate";
            public const string PersonPage = "PersonPage";
            public const string AreaSelectPage = "AreaSelectPage";
        }

        #endregion Attribute Keys

        #region Fields

        private string _configuredMode = "L";

        #endregion

        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string CurrentCampusId = "CurrentCampusId";
            public const string CurrentScheduleId = "CurrentScheduleId";
            public const string CurrentNavPath = "CurrentNavPath";
            public const string NavData = "NavData";
        }

        #endregion ViewState Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys for page parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string NavPath = "NavPath";
            public const string GroupType = "GroupType";
            public const string Location = "Location";
            public const string Group = "Group";
            public const string Area = "Area";
        }

        #endregion Page Parameter Keys

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

            CurrentCampusId = ViewState[ViewStateKey.CurrentCampusId] as string;
            CurrentScheduleId = ViewState[ViewStateKey.CurrentScheduleId] as string;
            CurrentNavPath = ViewState[ViewStateKey.CurrentNavPath] as string;
            NavData = ViewState[ViewStateKey.NavData] as NavigationData;
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

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            InitializeChartScripts();

            upnlContent.OnPostBack += upnlContent_OnPostBack;

            rptNavItems.ItemDataBound += rptNavItems_ItemDataBound;
            rptNavItems.ItemCommand += RptNavItems_ItemCommand;
            rptPeople.ItemDataBound += rptPeople_ItemDataBound;

            _configuredMode = GetAttributeValue( AttributeKey.Mode );
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
                campus = CampusCache.Get( campusContext.Id );
            }
            else
            {
                campus = GetDefaultCampus();
            }

            if ( campus == null )
            {
                nbWarning.Text = "Check-in Manager requires that a valid campus exists.";
                nbWarning.Visible = true;
                pnlContent.Visible = false;
                return;
            }

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
                string requestedNavPath = PageParameter( PageParameterKey.NavPath );
                if ( !string.IsNullOrWhiteSpace( requestedNavPath ) )
                {
                    CurrentNavPath = requestedNavPath;
                }
                else
                {
                    int? groupTypeId = PageParameter( PageParameterKey.GroupType ).AsIntegerOrNull();
                    int? locationId = PageParameter( PageParameterKey.Location ).AsIntegerOrNull();
                    int? groupId = PageParameter( PageParameterKey.Group ).AsIntegerOrNull();
                    if ( groupTypeId.HasValue || locationId.HasValue || groupId.HasValue )
                    {
                        CurrentNavPath = BuildCurrentPath( groupTypeId, locationId, groupId );
                    }
                }

                if ( string.IsNullOrWhiteSpace( CurrentNavPath ) )
                {
                    CurrentNavPath = GetUserPreference( "CurrentNavPath" );
                }

                BuildNavigationControls();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page PostBack.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.NavData] = NavData;
            ViewState[ViewStateKey.CurrentNavPath] = CurrentNavPath;
            ViewState[ViewStateKey.CurrentScheduleId] = CurrentScheduleId;
            ViewState[ViewStateKey.CurrentCampusId] = CurrentCampusId;

            return base.SaveViewState();
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
            if ( navItem == null )
            {
                return;
            }

            var li = e.Item.FindControl( "liNavItem" ) as HtmlGenericControl;
            if ( li != null )
            {
                li.Attributes["onClick"] = upnlContent.GetPostBackEventReference(
                    string.Format(
                        "{0}|{1}{2}",
                        CurrentNavPath,
                        navItem.TypeKey,
                        navItem.Id ) );
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
            var personResult = e.Item.DataItem as PersonResult;
            if ( personResult == null && personResult.Person == null )
            {
                return;
            }

            var person = personResult.Person;

            var li = e.Item.FindControl( "liNavItem" ) as HtmlGenericControl;
            if ( li != null )
            {
                li.Attributes["onClick"] = upnlContent.GetPostBackEventReference( "P" + person.Guid.ToString() );
            }

            var img = e.Item.FindControl( "imgPerson" ) as Literal;
            if ( img != null )
            {
                img.Text = Rock.Model.Person.GetPersonPhotoImageTag( person, 50, 50, className: "avatar avatar-lg mr-3" );
            }

            string desktopStatus = string.Empty;
            string statusClass = string.Empty;
            string mobileIcon = @"<i class=""fa fa-{0}""></i>";

            if ( NavData.AllowCheckout && personResult.CheckedOutDateTime.HasValue )
            {
                statusClass = "danger";
                mobileIcon = string.Format( mobileIcon, "minus" );
                desktopStatus = "Checked-out";
            }
            else if ( personResult.PresentDateTime.HasValue )
            {
                statusClass = "success";
                mobileIcon = string.Format( mobileIcon, "check" );
                if ( NavData.EnablePresence )
                {
                    desktopStatus = "Present";
                }
                else
                {
                    desktopStatus = "Checked-in";
                }
            }
            else
            {
                statusClass = "warning";
                mobileIcon = "&nbsp;";
                desktopStatus = "Checked-in";
            }

            // mobile only
            var lMobileStatus = e.Item.FindControl( "lMobileStatus" ) as Literal;
            if ( lMobileStatus != null )
            {
                lMobileStatus.Text = string.Format( @"<span class=""badge badge-circle badge-{0} d-sm-none"">{1}</span>", statusClass, mobileIcon );
            }

            if ( person.Age.HasValue )
            {
                var lAge = e.Item.FindControl( "lAge" ) as Literal;
                if ( lAge != null )
                {
                    lAge.Text = string.Format(
                        "{0}<span class='small text-muted'>(Age: {1})</span>",
                        string.IsNullOrWhiteSpace( personResult.ScheduleGroupNames ) ? "<br/>" : " ",
                        person.Age );
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
                    NavigateToLinkedPage( AttributeKey.PersonPage, parms );
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
                        .GetAllDescendentIds( campus.LocationId.Value )
                        .ToList()
                        .ForEach( l => validLocationids.Add( l ) );
                }

                Guid? groupTypeTemplateGuid = GetSelectedAreaGuid();

                if ( !groupTypeTemplateGuid.HasValue )
                {
                    if ( string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Area ) ) )
                    {
                        // If could not determine area and did not come from are select, redirect to area select page
                        NavigateToLinkedPage( AttributeKey.AreaSelectPage );
                    }

                    nbWarning.Text = "Please select a valid Check-in type in the block settings.";
                    nbWarning.Visible = true;
                    pnlContent.Visible = false;
                    return null;
                }

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

                var allowCheckedOut = parentGroupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT ).AsBoolean( true );
                var enablePresence = parentGroupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean();
                NavData.AllowCheckout = allowCheckedOut;
                NavData.EnablePresence = enablePresence;

                pnlCheckedInCount.Visible = enablePresence;
                pnlTotalCount.Visible = enablePresence;
                lGroupTypeName.Text = parentGroupType.Name ?? string.Empty;

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

                        /* 
                            2/5/2021 MDP 

                            This used to only add the group to the GroupTypes.ChildGroups if it didn't have
                            a parent group, or the parent group was one of the groups we are displaying.
                            However, this was preventing groups from being added if they were organized (using GroupViewer)
                            under some non-checkin group. I couldn't figure out why that restriction was in place.
                         */

                        NavData.GroupTypes.Where( t => t.Id == group.GroupTypeId ).ToList()
                                .ForEach( t => t.ChildGroupIds.Add( group.Id ) );
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

                // Remove any GroupType without groups or child group types
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
                var currentCampusDateTime = GetCampusTime();

                groupIds = NavData.Groups.Select( g => g.Id ).ToList();

                List<int> checkinAreaGroupTypeIds = new List<int>();

                if ( groupTypeTemplateGuid.HasValue )
                {
                    var checkinAreaGroupTypeId = GroupTypeCache.GetId( groupTypeTemplateGuid.Value );
                    if ( checkinAreaGroupTypeId != null )
                    {
                        checkinAreaGroupTypeIds = new GroupTypeService( new RockContext() ).GetCheckinAreaDescendants( checkinAreaGroupTypeId.Value ).Select( a => a.Id ).ToList();
                    }
                }

                var schedules = new List<Schedule>();

                // Get all Attendance records for the current day and location, limited by the selected groups
                // within the selected check-in area.
                var attendanceQry = new AttendanceService( rockContext ).Queryable()
                    .Where( a =>
                        a.Occurrence.ScheduleId.HasValue &&
                        a.Occurrence.GroupId.HasValue &&
                        a.Occurrence.LocationId.HasValue &&
                        a.StartDateTime >= dayStart &&
                        a.StartDateTime <= currentCampusDateTime &&
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value &&
                        a.PersonAliasId.HasValue &&
                        checkinAreaGroupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) &&
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

                var attendanceList = attendanceQry
                    .AsNoTracking()
                    .AsEnumerable()
                    .Select( s => new
                    {
                        s.PersonAliasId,
                        PersonAliasPersonId = s.PersonAlias.PersonId,
                        s.EndDateTime,
                        s.StartDateTime,
                        s.PresentDateTime,
                        s.IsCurrentlyCheckedIn,
                        Occurrence = new
                        {
                            s.Occurrence.ScheduleId,
                            s.Occurrence.GroupId,
                            s.Occurrence.LocationId
                        }
                    }
                    )
                    .ToList();

                foreach ( var groupLoc in attendanceList
                        .Where( a =>
                            a.PersonAliasId.HasValue &&
                            a.IsCurrentlyCheckedIn &&
                            schedules.Any( b => b.Id == a.Occurrence.ScheduleId.Value ) )
                        .GroupBy( a => new
                        {
                            GroupId = a.Occurrence.GroupId.Value,
                            LocationId = a.Occurrence.LocationId.Value
                        } ) )
                {
                    List<int> pendingPeople = new List<int>();
                    List<int> presentPeople = new List<int>();

                    var groupAttendanceList = groupLoc.Select( gl => gl ).Where( a => a.PersonAliasId.HasValue ).ToList();
                    foreach ( var attendance in groupAttendanceList )
                    {
                        var personId = attendance.PersonAliasPersonId;

                        if ( attendance.PresentDateTime.HasValue )
                        {
                            // if the attendance has PresentDateTime, they are checked in (which is called present when Presence is Enabled)
                            presentPeople.Add( personId );
                        }
                        else
                        {
                            // if the attendance doesn't have PresentDateTime (which can only happen if Presence is Enabled)

                            if ( NavData.EnablePresence )
                            {
                                // they have "checked-in" but haven't been marked present
                                pendingPeople.Add( personId );
                            }
                            else
                            {
                                // Shouldn't happen, but if EnablePresence is false, AND PresentDatetime is null, consider them checked-in (not pending)
                                presentPeople.Add( personId );
                            }
                        }
                    }

                    AddPersonCountByGroup( groupLoc.Key.GroupId, pendingPeople, presentPeople );
                    AddPersonCountByLocation( groupLoc.Key.LocationId, pendingPeople, presentPeople );
                }

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
                            ( !a.EndDateTime.HasValue || ( a.EndDateTime.HasValue && a.EndDateTime > chartTime ) ) &&
                            a.PersonAliasId.HasValue &&
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
                            PersonIds = g.Select( a => a.PersonAliasPersonId ).Distinct().ToList()
                        } ) )
                    {
                        AddGroupCount( chartTime, groupLocSched.GroupId, groupLocSched.PersonIds, current );
                        AddLocationCount( chartTime, groupLocSched.LocationId, groupLocSched.PersonIds, current );
                    }
                }
            }

            return NavData;
        }

        /// <summary>
        /// Gets the selected area unique identifier.
        /// </summary>
        /// <returns></returns>
        private Guid? GetSelectedAreaGuid()
        {
            var groupTypeTemplateGuid = PageParameter( PageParameterKey.Area ).AsGuidOrNull();

            if ( !groupTypeTemplateGuid.HasValue )
            {
                // Next check if there is an Area cookie (this is usually what would happen)
                groupTypeTemplateGuid = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CheckinAreaGuid;
            }

            if ( !groupTypeTemplateGuid.HasValue )
            {
                groupTypeTemplateGuid = this.GetAttributeValue( AttributeKey.GroupTypeTemplate ).AsGuidOrNull();
            }

            if ( !groupTypeTemplateGuid.HasValue )
            {
                // Check to see if a specific group was specified
                int? groupId = PageParameter( PageParameterKey.Group ).AsIntegerOrNull();
                if ( groupId.HasValue )
                {
                    var groupTypeId = new GroupService( new RockContext() ).GetSelect( groupId.Value, s => s.GroupTypeId );
                    var groupType = GroupTypeCache.Get( groupTypeId );
                    if ( groupType != null )
                    {
                        var parentGroupType = groupType.GetCheckInConfigurationType();
                        if ( parentGroupType != null )
                        {
                            groupTypeTemplateGuid = parentGroupType.Guid;
                        }
                    }
                }
            }

            return groupTypeTemplateGuid;
        }

        private void AddPersonCountByLocation( int locationId, List<int> pendingPeople, List<int> presentPeople )
        {
            var navLocation = NavData.Locations.FirstOrDefault( g => g.Id == locationId );
            if ( navLocation != null )
            {
                navLocation.PresentPeople = navLocation.PresentPeople.Union( presentPeople ).ToList();
                navLocation.CheckedInPeople = navLocation.CheckedInPeople.Union( pendingPeople ).ToList();

                if ( navLocation.ParentId.HasValue )
                {
                    AddPersonCountByLocation( navLocation.ParentId.Value, pendingPeople, presentPeople );
                }
            }
        }

        private void AddPersonCountByGroup( int groupId, List<int> pendingPeople, List<int> presentPeople )
        {
            var navGroup = NavData.Groups.FirstOrDefault( g => g.Id == groupId );
            if ( navGroup != null )
            {
                navGroup.PresentPeople = navGroup.PresentPeople.Union( presentPeople ).ToList();
                navGroup.CheckedInPeople = navGroup.CheckedInPeople.Union( pendingPeople ).ToList();

                AddCountByGroupType( navGroup.GroupTypeId, pendingPeople, presentPeople );
            }
        }

        private void AddCountByGroupType( int groupTypeId, List<int> pendingPeople, List<int> presentPeople )
        {
            var navGroupType = NavData.GroupTypes.FirstOrDefault( g => g.Id == groupTypeId );
            if ( navGroupType != null )
            {
                navGroupType.PresentPeople = navGroupType.PresentPeople.Union( presentPeople ).ToList();
                navGroupType.CheckedInPeople = navGroupType.CheckedInPeople.Union( pendingPeople ).ToList();

                if ( navGroupType.ParentId.HasValue )
                {
                    AddCountByGroupType( navGroupType.ParentId.Value, pendingPeople, presentPeople );
                }
            }
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
            if ( NavData == null )
            {
                return;
            }

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

            var chartData = new Dictionary<DateTime, int>();

            TimeSpan baseSpan = new TimeSpan( new DateTime( 1970, 1, 1 ).Ticks );
            foreach ( var kv in chartCounts.OrderBy( c => c.Key ) )
            {
                DateTime offsetTime = kv.Key.Subtract( baseSpan );
                chartData.Add( offsetTime, kv.Value.Count() );
            }

            // Format label values as 9AM or 9:15, and only show label value if minutes are 00, 15, 30, and 45
            hfChartLabel.Value = JsonConvert.SerializeObject( chartData.Keys.Select( a => ( a.Minute == 0 ) ? a.ToString( @"htt" ) :
                ( a.Minute == 15 || a.Minute == 30 || a.Minute == 45 ) ? a.ToString( @"h\:mm" ) : string.Empty ).ToList() );
            hfChartData.Value = JsonConvert.SerializeObject( chartData.Values.ToList() );
            pnlChart.Attributes["onClick"] = upnlContent.GetPostBackEventReference( "R" );

            pnlNavHeading.Visible = item != null;
            int checkedInPeople = navItems.SelectMany( a => a.CheckedInPeople ).Distinct().Count();
            int presentPeople = navItems.SelectMany( a => a.PresentPeople ).Distinct().Count();

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
                    hfThreshold.Value = locationItem.SoftThreshold.HasValue ? locationItem.SoftThreshold.Value.ToString() : string.Empty;
                    lThreshold.Text = locationItem.SoftThreshold.HasValue ? locationItem.SoftThreshold.Value.ToString( "N0" ) : "none";
                    nbThreshold.MaximumValue = locationItem.FirmThreshold.HasValue ? locationItem.FirmThreshold.Value.ToString() : string.Empty;
                    lbUpdateThreshold.Attributes["data-key"] = locationItem.Id.ToString();

                    var rockContext = new RockContext();

                    List<int> checkinAreaGroupTypeIds = new List<int>();

                    Guid? groupTypeTemplateGuid = GetSelectedAreaGuid();

                    if ( groupTypeTemplateGuid.HasValue )
                    {
                        var checkinAreaGroupTypeId = GroupTypeCache.GetId( groupTypeTemplateGuid.Value );
                        if ( checkinAreaGroupTypeId != null )
                        {
                            checkinAreaGroupTypeIds = new GroupTypeService( new RockContext() ).GetCheckinAreaDescendants( checkinAreaGroupTypeId.Value ).Select( a => a.Id ).ToList();
                        }
                    }

                    // Get all Attendance records for the current day and location, limited to groups within the selected check-in area
                    var dayStart = RockDateTime.Today.AddDays( -1 );
                    var currentCampusDateTime = GetCampusTime();

                    var attendees = new AttendanceService( rockContext )
                        .Queryable()
                        .Include( s => s.Occurrence.Group )
                        .Include( s => s.PersonAlias.Person )
                        .Include( s => s.Occurrence.Schedule )
                        .Include( s => s.AttendanceCode )
                        .AsNoTracking()
                        .Where( a =>
                            a.PersonAliasId.HasValue &&
                            a.StartDateTime > dayStart &&
                            a.StartDateTime < currentCampusDateTime &&
                            a.Occurrence.LocationId.HasValue &&
                            a.Occurrence.LocationId == locationItem.Id &&
                            a.Occurrence.GroupId.HasValue &&
                            checkinAreaGroupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) &&
                            a.DidAttend.HasValue &&
                            a.DidAttend.Value &&
                            a.Occurrence.ScheduleId.HasValue )
                        .AsEnumerable()
                        .Where( a => a.IsCurrentlyCheckedIn )
                        .ToList();

                    int? scheduleId = CurrentScheduleId.AsIntegerOrNull();
                    var attendeesByPersonId = attendees.GroupBy( a => a.PersonAlias.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );

                    var people = new List<PersonResult>();
                    foreach ( var personId in attendees
                        .OrderBy( a => a.PersonAlias.Person.NickName )
                        .ThenBy( a => a.PersonAlias.Person.LastName )
                        .Select( a => a.PersonAlias.PersonId )
                        .Distinct() )
                    {
                        var matchingAttendees = attendeesByPersonId.GetValueOrNull( personId ) ?? new List<Attendance>();

                        if ( !scheduleId.HasValue || matchingAttendees.Any( a => a.Occurrence.ScheduleId == scheduleId.Value ) )
                        {
                            people.Add( new PersonResult( matchingAttendees ) );
                        }
                    }

                    checkedInPeople += item.CheckedInPeople.Count;
                    presentPeople += item.PresentPeople.Count;

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

            lCheckedInPeopleCount.Text = checkedInPeople.ToString();
            lPresentPeopleCount.Text = presentPeople.ToString();
            lTotalPeopleCount.Text = ( checkedInPeople + presentPeople ).ToString();

            rptNavItems.Visible = navItems.Any();
            rptNavItems.DataSource = navItems
                .OrderBy( i => i.TypeKey )
                .ThenBy( i => i.Order )
                .ThenBy( i => i.Name );
            rptNavItems.DataBind();

        }

        /// <summary>
        /// Add scripts for Chart.js components
        /// </summary>
        private void InitializeChartScripts()
        {
            // NOTE: moment.js must be loaded before Chart.js
            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );
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

            public bool EnablePresence { get; set; }

            public bool AllowCheckout { get; set; }

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
        [System.Diagnostics.DebuggerDisplay( "{Name}" )]
        public abstract class NavigationItem
        {
            public int? ParentId { get; set; }

            public int Id { get; set; }

            public string Name { get; set; }

            public int Order { get; set; }

            public List<int> CurrentPersonIds { get; set; }

            public int CurrentCount
            {
                get
                {
                    return CurrentPersonIds.Count();
                }
            }


            /// <summary>
            /// Gets or sets the checked in (not Present) people count.
            /// </summary>
            /// <value>
            /// The checked in people count.
            /// </value>
            public List<int> CheckedInPeople { get; set; }

            /// <summary>
            /// Gets or sets the Present people
            /// </summary>
            /// <value>
            /// The Present people.
            /// </value>
            public List<int> PresentPeople { get; set; }

            public Dictionary<DateTime, List<int>> RecentPersonIds { get; set; }

            public virtual string TypeKey
            {
                get
                {
                    return string.Empty;
                }
            }
        }

        [Serializable]
        public class NavigationLocation : NavigationItem
        {
            public override string TypeKey
            {
                get
                {
                    return "L";
                }
            }

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
                PresentPeople = new List<int>();
                CheckedInPeople = new List<int>();
            }
        }

        [Serializable]
        public class NavigationGroupType : NavigationItem
        {
            public override string TypeKey
            {
                get
                {
                    return "T";
                }
            }

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
                PresentPeople = new List<int>();
                CheckedInPeople = new List<int>();
            }
        }

        [Serializable]
        public class NavigationGroup : NavigationItem
        {
            public override string TypeKey
            {
                get
                {
                    return "G";
                }
            }

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
                PresentPeople = new List<int>();
                CheckedInPeople = new List<int>();
            }
        }

        public class PersonResult
        {
            public Rock.Model.Person Person { get; private set; }

            public string Name
            {
                get
                {
                    if ( Person != null )
                    {
                        return Person.FullName;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public string ScheduleGroupNames { get; set; }

            public DateTime? PresentDateTime { get; set; }

            public DateTime? CheckedOutDateTime { get; set; }

            public PersonResult()
            {
            }

            public PersonResult( List<Attendance> attendances )
            {
                if ( !attendances.Any() )
                {
                    return;
                }

                Person = attendances.First().PersonAlias.Person;

                ScheduleGroupNames = attendances
                    .Select( a => string.Format(
                        "<span class='d-block small text-muted'>{0}{1}{2}</span>",
                        a.Occurrence.Group.Name,
                        a.Occurrence.Schedule != null ? " - " + a.Occurrence.Schedule.Name : string.Empty,
                        a.AttendanceCode != null ? " - " + a.AttendanceCode.Code : string.Empty ) )
                    .Distinct()
                    .ToList()
                    .AsDelimited( "\r\n" );

                var lastAttendance = attendances.OrderByDescending( a => a.StartDateTime ).First();
                PresentDateTime = lastAttendance.PresentDateTime;
                CheckedOutDateTime = lastAttendance.EndDateTime;
            }
        }

        #endregion
    }
}