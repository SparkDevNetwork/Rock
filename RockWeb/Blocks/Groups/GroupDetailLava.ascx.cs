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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;
using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Group Detail Lava" )]
    [Category( "Groups" )]
    [Description( "Presents the details of a group using Lava" )]

    [LinkedPage( "Person Detail Page", "Page to link to for more information on a group member.", false, "", "", 0 )]
    [LinkedPage( "Group Member Add Page", "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.", false, "", "", 1 )]
    [LinkedPage( "Roster Page", "The page to link to to view the roster.", true, "", "", 2 )]
    [LinkedPage( "Attendance Page", "The page to link to to manage the group's attendance.", true, "", "", 3 )]
    [LinkedPage( "Communication Page", "The communication page to use for sending emails to the group members.", true, "", "", 4 )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupDetail.lava' %}", "", 5 )]
    [BooleanField("Enable Location Edit", "Enables changing locations when editing a group.", false, "", 6)]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 7 )]
    [CodeEditorField("Edit Group Pre-HTML", "HTML to display before the edit group panel.", CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, "", "HTML Wrappers", 8)]
    [CodeEditorField( "Edit Group Post-HTML", "HTML to display after the edit group panel.", CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, "", "HTML Wrappers", 9 )]
    [CodeEditorField( "Edit Group Member Pre-HTML", "HTML to display before the edit group member panel.", CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, "", "HTML Wrappers", 10 )]
    [CodeEditorField( "Edit Group Member Post-HTML", "HTML to display after the edit group member panel.", CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, "", "HTML Wrappers", 11 )]
    public partial class GroupDetailLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        int _groupId = 0;
        private const string MEMBER_LOCATION_TAB_TITLE = "Member Location";
        private const string OTHER_LOCATION_TAB_TITLE = "Other Location";

        private readonly List<string> _tabs = new List<string> { MEMBER_LOCATION_TAB_TITLE, OTHER_LOCATION_TAB_TITLE };
        #endregion

        #region Properties

        // used for public / protected properties
        public bool IsEditingGroup
        {
            get
            {
                return ViewState["IsEditingGroup"] as bool? ?? false;
            }
            set
            {
                ViewState["IsEditingGroup"] = value;
            }
        }

        public bool IsEditingGroupMember
        {
            get
            {
                return ViewState["IsEditingGroupMember"] as bool? ?? false;
            }
            set
            {
                ViewState["IsEditingGroupMember"] = value;
            }
        }

        public int CurrentGroupMemberId
        {
            get
            {
                int groupMemberId = 0;
                int.TryParse( ViewState["CurrentGroupMemberId"].ToString(), out groupMemberId);
                return groupMemberId;
            }
            set
            {
                ViewState["CurrentGroupMemberId"] = value;
            }
        }

        private string LocationTypeTab
        {
            get
            {
                object currentProperty = ViewState["LocationTypeTab"];
                return currentProperty != null ? currentProperty.ToString() : MEMBER_LOCATION_TAB_TITLE;
            }

            set
            {
                ViewState["LocationTypeTab"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            // get the group id
            if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupId" ) ) )
            {
                _groupId = Convert.ToInt32( PageParameter( "GroupId" ) );
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( IsEditingGroup == true )
            {
                Group group = new GroupService( new RockContext() ).Get( _groupId );
                group.LoadAttributes();

                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( group, phAttributes, false, BlockValidationGroup );
            }

            if ( IsEditingGroupMember == true )
            {
                RockContext rockContext = new RockContext();
                GroupMemberService groupMemberService = new GroupMemberService( rockContext );

                var groupMember = groupMemberService.Get( this.CurrentGroupMemberId );

                if ( groupMember == null )
                {
                    groupMember = new GroupMember { Id = 0 };
                    groupMember.GroupId = _groupId;
                    groupMember.Group = new GroupService( rockContext ).Get( groupMember.GroupId );
                    groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                }

                // set attributes
                groupMember.LoadAttributes();
                phGroupMemberAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( groupMember, phGroupMemberAttributes, true, "", true );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            
            RouteAction();

            if ( !IsPostBack )
            {
                BlockSetup();
            }

            // add a navigate event to cature when someone presses the back button
            var sm = ScriptManager.GetCurrent( Page );
            sm.EnableSecureHistoryState = false;
            sm.Navigate += sm_Navigate;

        }

        void sm_Navigate( object sender, HistoryEventArgs e )
        {
            // show the view mode
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = true;
            pnlEditGroupMember.Visible = false;
            DisplayViewGroup();
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
            RouteAction();
            BlockSetup();
        }

        protected void btnSaveGroup_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );

            Group group = groupService.Get( _groupId );

            if ( group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                group.Name = tbName.Text;
                group.Description = tbDescription.Text;
                group.IsActive = cbIsActive.Checked;

                if ( pnlSchedule.Visible )
                {
                    if ( group.Schedule == null )
                    {
                        group.Schedule = new Schedule();
                        group.Schedule.iCalendarContent = null;
                    }
                    group.Schedule.WeeklyDayOfWeek = dowWeekly.SelectedDayOfWeek;
                    group.Schedule.WeeklyTimeOfDay = timeWeekly.SelectedTime;
                }

                // set attributes
                group.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, group );

                // configure locations
                if ( GetAttributeValue( "EnableLocationEdit" ).AsBoolean() )
                {
                    // get selected location
                    Location location = null;
                    int? memberPersonAliasId = null;

                    if ( LocationTypeTab.Equals( MEMBER_LOCATION_TAB_TITLE ) )
                    {
                        if ( ddlMember.SelectedValue != null )
                        {
                            var ids = ddlMember.SelectedValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                            if ( ids.Length == 2 )
                            {
                                var dbLocation = new LocationService( rockContext ).Get( int.Parse( ids[0] ) );
                                if ( dbLocation != null )
                                {
                                    location = dbLocation;
                                }

                                memberPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( int.Parse( ids[1] ) );
                            }
                        }
                    }
                    else
                    {
                        if ( locpGroupLocation.Location != null )
                        {
                            location = new LocationService( rockContext ).Get( locpGroupLocation.Location.Id );
                        }
                    }

                    if ( location != null )
                    {
                        GroupLocation groupLocation = group.GroupLocations.FirstOrDefault();

                        if ( groupLocation == null )
                        {
                            groupLocation = new GroupLocation();
                            group.GroupLocations.Add( groupLocation );
                        }

                        groupLocation.GroupMemberPersonAliasId = memberPersonAliasId;
                        groupLocation.Location = location;
                        groupLocation.LocationId = groupLocation.Location.Id;
                        groupLocation.GroupLocationTypeValueId = ddlLocationType.SelectedValueAsId();
                    }
                }

                //rockContext.WrapTransaction( () =>
                //{
                    rockContext.SaveChanges();
                    group.SaveAttributeValues( rockContext );
                //} );
            }

            this.IsEditingGroup = false;

            // reload the group info
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = true;
            DisplayViewGroup();
        }

        protected void lbCancelGroup_Click( object sender, EventArgs e )
        {
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = true;
            this.IsEditingGroup = false;

            var sm = ScriptManager.GetCurrent( Page );
            sm.AddHistoryPoint( "Action", "ViewGroup" );
        }

        protected void btnSaveGroupMember_Click( object sender, EventArgs e )
        {                        
            var rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            GroupTypeRole role = new GroupTypeRoleService( rockContext ).Get( ddlGroupRole.SelectedValueAsInt() ?? 0 );

            var groupMember = groupMemberService.Get( this.CurrentGroupMemberId );

            if ( this.CurrentGroupMemberId == 0 )
            {
                groupMember = new GroupMember { Id = 0 };
                groupMember.GroupId = _groupId;

                // check to see if the person is alread a member of the gorup/role
                var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                    _groupId, ppGroupMemberPerson.SelectedValue ?? 0, ddlGroupRole.SelectedValueAsId() ?? 0 );

                if ( existingGroupMember != null )
                {
                    // if so, don't add and show error message
                    var person = new PersonService( rockContext ).Get( (int)ppGroupMemberPerson.PersonId );

                    nbGroupMemberErrorMessage.Title = "Person Already In Group";
                    nbGroupMemberErrorMessage.Text = string.Format(
                        "{0} already belongs to the {1} role for this {2}, and cannot be added again with the same role.",
                        person.FullName,
                        ddlGroupRole.SelectedItem.Text,
                        role.GroupType.GroupTerm,
                        RockPage.PageId,
                        existingGroupMember.Id );
                    return;
                }
            }

            groupMember.PersonId = ppGroupMemberPerson.PersonId.Value;
            groupMember.GroupRoleId = role.Id;
            groupMember.GroupMemberStatus = rblStatus.SelectedValueAsEnum<GroupMemberStatus>();

            groupMember.LoadAttributes();

            Rock.Attribute.Helper.GetEditValues( phAttributes, groupMember );

            // using WrapTransaction because there are two Saves
            rockContext.WrapTransaction( () =>
            {
                if ( groupMember.Id.Equals( 0 ) )
                {
                    groupMemberService.Add( groupMember );
                }

                rockContext.SaveChanges();
                groupMember.SaveAttributeValues( rockContext );
            } );

            Group group = new GroupService( rockContext ).Get( groupMember.GroupId );
            if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
            {
                Rock.Security.Role.Flush( group.Id );
                Rock.Security.Authorization.Flush();
            }

            pnlEditGroupMember.Visible = false;
            pnlGroupView.Visible = true;
            DisplayViewGroup();
            this.IsEditingGroupMember = false;
        }

        protected void btnCancelGroupMember_Click( object sender, EventArgs e )
        {
            pnlEditGroupMember.Visible = false;
            pnlGroupView.Visible = true;
            this.IsEditingGroupMember = false;

            var sm = ScriptManager.GetCurrent( Page );
            sm.AddHistoryPoint( "Action", "ViewGroup" );
        }

        protected void lbLocationType_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                LocationTypeTab = lb.Text;

                rptLocationTypes.DataSource = _tabs;
                rptLocationTypes.DataBind();
            }

            ShowSelectedPane();
        }

        #endregion

        #region Methods

        //
        // Block Methods

        // route the request to the correct panel
        private void RouteAction()
        {
            int groupMemberId = 0;
            var sm = ScriptManager.GetCurrent( Page );

            if ( Request.Form["__EVENTARGUMENT"] != null )
            {

                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];

                    int argument = 0;
                    int.TryParse( parameters, out argument );

                    switch ( action )
                    {
                        case "EditGroup":
                            pnlGroupEdit.Visible = true;
                            pnlGroupView.Visible = false;
                            pnlEditGroupMember.Visible = false;
                            DisplayEditGroup();
                            sm.AddHistoryPoint("Action", "EditGroup");
                            break;

                        case "AddGroupMember":
                            AddGroupMember();
                            break;

                        case "EditGroupMember":
                            groupMemberId = int.Parse( parameters );
                            DisplayEditGroupMember( groupMemberId );
                            sm.AddHistoryPoint( "Action", "EditMember" );
                            break;

                        case "DeleteGroupMember":
                            groupMemberId = int.Parse( parameters );
                            DeleteGroupMember( groupMemberId );
                            DisplayViewGroup();
                            break;

                        case "SendCommunication":
                            SendCommunication();
                            break;
                    }
                }
            }
            else
            {
                pnlGroupEdit.Visible = false;
                pnlGroupView.Visible = true;
                pnlEditGroupMember.Visible = false;
                DisplayViewGroup();
            }
        }

        // setup block
        private void BlockSetup()
        {
            lGroupEditPreHtml.Text = GetAttributeValue("EditGroupPre-HTML");
            lGroupEditPostHtml.Text = GetAttributeValue( "EditGroupPost-HTML" );

            lGroupMemberEditPreHtml.Text = GetAttributeValue( "EditGroupMemberPre-HTML" );
            lGroupMemberEditPostHtml.Text = GetAttributeValue( "EditGroupMemberPost-HTML" );
        }

        //
        // Group Methods

        // displays the group info using a lava template
        private void DisplayViewGroup()
        {

            if ( _groupId > 0 )
            {
                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );

                bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();

                var qry = groupService
                    .Queryable( "GroupLocations,Members,Members.Person,Members.Person.PhoneNumbers,GroupType" )
                    .Where( g => g.Id == _groupId );

                if ( !enableDebug )
                {
                    qry = qry.AsNoTracking();
                }
                var group = qry.FirstOrDefault();

                // order group members by name
                group.Members = group.Members.OrderBy( m => m.Person.LastName ).ThenBy( m => m.Person.FirstName ).ToList();

                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Group", group );

                // add linked pages
                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( "PersonDetailPage", LinkedPageUrl( "PersonDetailPage", null ) );
                linkedPages.Add( "RosterPage", LinkedPageUrl( "RosterPage", null ) );
                linkedPages.Add( "AttendancePage", LinkedPageUrl( "AttendancePage", null ) );
                linkedPages.Add( "CommunicationPage", LinkedPageUrl( "CommunicationPage", null ) );
                mergeFields.Add( "LinkedPages", linkedPages );

                // add collection of allowed security actions
                Dictionary<string, object> securityActions = new Dictionary<string, object>();
                securityActions.Add( "View", group != null && group.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                securityActions.Add( "Edit", group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) );
                securityActions.Add( "Administrate", group != null && group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) );
                mergeFields.Add( "AllowedActions", securityActions );

                mergeFields.Add( "CurrentPerson", CurrentPerson );
                
                var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
                globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

                Dictionary<string, object> currentPageProperties = new Dictionary<string, object>();
                currentPageProperties.Add( "Id", RockPage.PageId );
                currentPageProperties.Add( "Path", Request.Path );
                mergeFields.Add( "CurrentPage", currentPageProperties );

                string template = GetAttributeValue( "LavaTemplate" );

                // show debug info
                if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
                {
                    string postbackCommands = @"<h5>Available Postback Commands</h5>
                                                <ul>
                                                    <li><strong>EditGroup:</strong> Shows a panel for modifing group info. Expects a group id. <code>{{ Group.Id | Postback:'EditGroup' }}</code></li>
                                                    <li><strong>AddGroupMember:</strong> Shows a panel for adding group info. Does not require input. <code>{{ '' | Postback:'AddGroupMember' }}</code></li>
                                                    <li><strong>EditGroupMember:</strong> Shows a panel for modifing group info. Expects a group member id. <code>{{ member.Id | Postback:'EditGroupMember' }}</code></li>
                                                    <li><strong>DeleteGroupMember:</strong> Deletes a group member. Expects a group member id. <code>{{ member.Id | Postback:'DeleteGroupMember' }}</code></li>
                                                    <li><strong>SendCommunication:</strong> Sends a communication to all group members on behalf of the Current User. This will redirect them to the communication page where they can author their email. <code>{{ '' | Postback:'SendCommunication' }}</code></li>
                                                </ul>";

                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo( null, "", postbackCommands );
                }

                lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );

            }
            else
            {
                lContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }

        }

        // displays the group edit panel
        private void DisplayEditGroup()
        {
            this.IsEditingGroup = true;
            
            if ( _groupId != -1 )
            {
                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );

                var qry = groupService
                        .Queryable( "GroupLocations,GroupType,Schedule" )
                        .Where( g => g.Id == _groupId );

                var group = qry.FirstOrDefault();

                if ( group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    tbName.Text = group.Name;
                    tbDescription.Text = group.Description;
                    cbIsActive.Checked = group.IsActive;

                    if ( ( group.GroupType.AllowedScheduleTypes & ScheduleType.Weekly ) == ScheduleType.Weekly )
                    {
                        pnlSchedule.Visible = group.Schedule == null || group.Schedule.ScheduleType == ScheduleType.Weekly;
                        if ( group.Schedule != null )
                        {
                            dowWeekly.SelectedDayOfWeek = group.Schedule.WeeklyDayOfWeek;
                            timeWeekly.SelectedTime = group.Schedule.WeeklyTimeOfDay;
                        }
                        else
                        {
                            dowWeekly.SelectedDayOfWeek = null;
                            timeWeekly.SelectedTime = null;
                        }
                    }
                    else
                    {
                        pnlSchedule.Visible = false;
                    }

                    group.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( group, phAttributes, true, BlockValidationGroup );

                    // enable editing location
                    pnlGroupEditLocations.Visible = GetAttributeValue( "EnableLocationEdit" ).AsBoolean();
                    if ( GetAttributeValue( "EnableLocationEdit" ).AsBoolean() )
                    {
                        ConfigureGroupLocationControls( group );

                        // set location tabs
                        rptLocationTypes.DataSource = _tabs;
                        rptLocationTypes.DataBind();
                    }
                }
                else
                {
                    lContent.Text = "<div class='alert alert-warning'>You do not have permission to edit this group.</div>";
                }
            }
            else
            {
                lContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }
        }

        // logic to setup the groups location entry panel
        private void ConfigureGroupLocationControls( Group group )
        {
            var rockContext = new RockContext();
            ddlMember.Items.Clear();

            var groupType = GroupTypeCache.Read( group.GroupTypeId );
            if ( groupType != null )
            {
                // only allow editing groups with single locations
                if ( !groupType.AllowMultipleLocations )
                {
                    GroupLocationPickerMode groupTypeModes = groupType.LocationSelectionMode;
                    if ( groupTypeModes != GroupLocationPickerMode.None )
                    {
                        // Set the location picker modes allowed based on the group type's allowed modes
                        LocationPickerMode modes = LocationPickerMode.None;
                        if ( (groupTypeModes & GroupLocationPickerMode.Named) == GroupLocationPickerMode.Named )
                        {
                            modes = modes | LocationPickerMode.Named;
                        }

                        if ( (groupTypeModes & GroupLocationPickerMode.Address) == GroupLocationPickerMode.Address )
                        {
                            modes = modes | LocationPickerMode.Address;
                        }

                        if ( (groupTypeModes & GroupLocationPickerMode.Point) == GroupLocationPickerMode.Point )
                        {
                            modes = modes | LocationPickerMode.Point;
                        }

                        if ( (groupTypeModes & GroupLocationPickerMode.Polygon) == GroupLocationPickerMode.Polygon )
                        {
                            modes = modes | LocationPickerMode.Polygon;
                        }

                        bool displayMemberTab = (groupTypeModes & GroupLocationPickerMode.GroupMember) == GroupLocationPickerMode.GroupMember;
                        bool displayOtherTab = modes != LocationPickerMode.None;

                        ulNav.Visible = displayOtherTab && displayMemberTab;
                        pnlMemberSelect.Visible = displayMemberTab;
                        pnlLocationSelect.Visible = displayOtherTab && !displayMemberTab;

                        if ( displayMemberTab )
                        {
                            var personService = new PersonService( rockContext );
                            Guid previousLocationType = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid();

                            foreach ( GroupMember member in new GroupMemberService( rockContext ).GetByGroupId( group.Id ) )
                            {
                                foreach ( Group family in personService.GetFamilies( member.PersonId ) )
                                {
                                    foreach ( GroupLocation familyGroupLocation in family.GroupLocations
                                        .Where( l => l.IsMappedLocation && !l.GroupLocationTypeValue.Guid.Equals( previousLocationType ) ) )
                                    {
                                        ListItem li = new ListItem(
                                            string.Format( "{0} {1} ({2})", member.Person.FullName, familyGroupLocation.GroupLocationTypeValue.Value, familyGroupLocation.Location.ToString() ),
                                            string.Format( "{0}|{1}", familyGroupLocation.Location.Id, member.PersonId ) );

                                        ddlMember.Items.Add( li );
                                    }
                                }
                            }
                        }

                        if ( displayOtherTab )
                        {
                            locpGroupLocation.AllowedPickerModes = modes;
                        }

                        ddlLocationType.DataSource = groupType.LocationTypeValues.ToList();
                        ddlLocationType.DataBind();

                        LocationTypeTab = (displayMemberTab && ddlMember.Items.Count > 0) ? MEMBER_LOCATION_TAB_TITLE : OTHER_LOCATION_TAB_TITLE;

                        var groupLocation = group.GroupLocations.FirstOrDefault();
                        if ( groupLocation != null && groupLocation.Location != null )
                        {
                            if ( displayOtherTab )
                            {
                                locpGroupLocation.CurrentPickerMode = locpGroupLocation.GetBestPickerModeForLocation( groupLocation.Location );

                                locpGroupLocation.MapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();

                                if ( groupLocation.Location != null )
                                {
                                    locpGroupLocation.Location = new LocationService( rockContext ).Get( groupLocation.Location.Id );
                                }
                            }

                            if ( displayMemberTab && ddlMember.Items.Count > 0 && groupLocation.GroupMemberPersonAliasId.HasValue )
                            {
                                LocationTypeTab = MEMBER_LOCATION_TAB_TITLE;
                                int? personId = new PersonAliasService( rockContext ).GetPersonId( groupLocation.GroupMemberPersonAliasId.Value );
                                if ( personId.HasValue )
                                {
                                    ddlMember.SetValue( string.Format( "{0}|{1}", groupLocation.LocationId, personId.Value ) );
                                }
                            }
                            else if ( displayOtherTab )
                            {
                                LocationTypeTab = OTHER_LOCATION_TAB_TITLE;
                            }

                            ddlLocationType.SetValue( groupLocation.GroupLocationTypeValueId );
                        }
                        else
                        {
                            LocationTypeTab = ( displayMemberTab && ddlMember.Items.Count > 0 ) ? MEMBER_LOCATION_TAB_TITLE : OTHER_LOCATION_TAB_TITLE;
                        }


                        rptLocationTypes.DataSource = _tabs;
                        rptLocationTypes.DataBind();

                        ShowSelectedPane();
                    }
                }
                else
                {
                    lContent.Text = "<div class='alert alert-warning'>This editor only allows editing groups with a single location.</div>";
                }
            }
            
        }

        // helper method to toggle the group location tabs
        protected string GetLocationTabClass( object property )
        {
            if ( property.ToString() == LocationTypeTab )
            {
                return "active";
            }

            return string.Empty;
        }

        private void ShowSelectedPane()
        {
            if ( LocationTypeTab.Equals( MEMBER_LOCATION_TAB_TITLE ) )
            {
                pnlMemberSelect.Visible = true;
                pnlLocationSelect.Visible = false;
            }
            else if ( LocationTypeTab.Equals( OTHER_LOCATION_TAB_TITLE ) )
            {
                pnlMemberSelect.Visible = false;
                pnlLocationSelect.Visible = true;
            }
        }

        //
        // Group Member Methods

        private void AddGroupMember()
        {
            this.IsEditingGroupMember = true;
            var personAddPage = GetAttributeValue( "GroupMemberAddPage" );

            if ( personAddPage == null || personAddPage == string.Empty )
            {
                DisplayEditGroupMember( 0 );
            }
            else
            {
                // redirect to the add page provided
                var group = new GroupService( new RockContext() ).Get( _groupId );
                if ( group != null )
                {
                    var queryParams = new Dictionary<string, string>();
                    queryParams.Add( "GroupId", group.Id.ToString() );
                    queryParams.Add( "GroupName", group.Name );
                    NavigateToLinkedPage( "GroupMemberAddPage", queryParams );
                }
            }
        }
        
        private void DisplayEditGroupMember( int groupMemberId )
        {
            // persist the group member id for use in partial postbacks
            this.CurrentGroupMemberId = groupMemberId;
            
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = false;
            pnlEditGroupMember.Visible = true;

            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            var groupMember = groupMemberService.Get( groupMemberId );

            if ( groupMember == null )
            {
                groupMember = new GroupMember { Id = 0 };
                groupMember.GroupId = _groupId;
                groupMember.Group = new GroupService( rockContext ).Get( groupMember.GroupId );
                groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
            }

            // load dropdowns
            LoadGroupMemberDropDowns( _groupId );

            // set values
            ppGroupMemberPerson.SetValue( groupMember.Person );
            ddlGroupRole.SetValue( groupMember.GroupRoleId );
            rblStatus.SetValue( (int)groupMember.GroupMemberStatus );

            // set attributes
            groupMember.LoadAttributes();
            phGroupMemberAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( groupMember, phGroupMemberAttributes, true, "", true );

            this.IsEditingGroupMember = true;
        }

        private void LoadGroupMemberDropDowns( int groupId )
        {
            Group group = new GroupService( new RockContext() ).Get( groupId );
            if ( group != null )
            {
                ddlGroupRole.DataSource = group.GroupType.Roles.OrderBy( a => a.Order ).ToList();
                ddlGroupRole.DataBind();
            }

            rblStatus.BindToEnum<GroupMemberStatus>();
        }

        private void DeleteGroupMember( int groupMemberId )
        {
            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            var groupMember = groupMemberService.Get( groupMemberId );
            if ( groupMember != null )
            {
                groupMemberService.Delete( groupMember );
            }

            rockContext.SaveChanges();
        }

        private void SendCommunication()
        {
            // create communication
            if ( this.CurrentPerson != null && _groupId != -1 && !string.IsNullOrWhiteSpace( GetAttributeValue( "CommunicationPage" ) ) )
            {
                var rockContext = new RockContext();
                var service = new Rock.Model.CommunicationService( rockContext );
                var communication = new Rock.Model.Communication();
                communication.IsBulkCommunication = false;
                communication.Status = Rock.Model.CommunicationStatus.Transient;
            
                communication.SenderPersonAliasId = this.CurrentPersonAliasId;

                service.Add( communication );

                var personAliasIds = new GroupMemberService( rockContext ).Queryable().Where( m => m.GroupId == _groupId )
                                    .ToList()
                                    .Select( m => m.Person.PrimaryAliasId )
                                    .ToList();

                // Get the primary aliases
                foreach ( int personAlias in personAliasIds )
                {
                    var recipient = new Rock.Model.CommunicationRecipient();
                    recipient.PersonAliasId = personAlias;
                    communication.Recipients.Add( recipient );
                }

                rockContext.SaveChanges();

                Dictionary<string, string> queryParameters = new Dictionary<string, string>();
                queryParameters.Add( "CommunicationId", communication.Id.ToString() );

                NavigateToLinkedPage( "CommunicationPage", queryParameters );
            }
        }

        #endregion 
    }
}