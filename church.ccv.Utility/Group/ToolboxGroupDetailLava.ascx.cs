// <copyright>
// Copyright by the Spark Development Network
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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Newtonsoft.Json;
using Rock.Rest.Controllers;
using static Rock.Model.PersonService;

namespace church.ccv.Utility.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Toolbox Group Detail Lava" )]
    [Category( "CCV > Groups" )]
    [Description( "Base class for all group detail blocks." )]
    [LinkedPage( "Person Detail Page", "Page to link to for more information on a group member.", false, "", "", 0 )]
    [LinkedPage( "Roster Page", "The page to link to to view the roster.", false, "", "", 2 )]
    [LinkedPage( "Communication Page", "The communication page to use for sending emails to the group members.", false, "", "", 4 )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupDetail.lava' %}", "", 8 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 10 )]
    public abstract class ToolboxGroupDetailLava : Rock.Web.UI.RockBlock
    {
        public abstract UpdatePanel MainPanel { get; }
        public abstract PlaceHolder AttributesPlaceholder { get; }
        public abstract Literal MainViewContent { get; }
        public abstract Literal DebugContent { get; }

        public abstract Panel GroupEdit { get; }
        public abstract Panel GroupView { get; }

        public abstract Panel Schedule { get; }
        public abstract DayOfWeekPicker DayOfWeekPicker { get; }
        public abstract TimePicker MeetingTime { get; }

        public abstract Literal GroupName { get; }
        
        // Stores a list of group attribute keys that should not be editable by the coach.
        // by default, it's an empty list.
        public List<string> ExcludedAttribKeys { get; private set; }

        protected int _groupId { get; set; }

        #region Models
        public class GroupMemberWithFamily : Rock.Lava.ILiquidizable
        {
            public GroupMember Member { get; set; }
            public string SpouseName { get; set; }
            public List<string> ChildrenInfo { get; set; }
            
            public GroupMemberWithFamily( GroupMember groupMember )
            {
                Member = groupMember;
                ChildrenInfo = new List<string>( );
            }
            
            // Liquid Methods
            [JsonIgnore]
            [Rock.Data.LavaIgnore]
            public List<string> AvailableKeys
            {
                get
                {
                    var availableKeys = new List<string> { "Member", "SpouseName", "ChildrenInfo" };
                    availableKeys.AddRange( Member.AvailableKeys );
                    
                    return availableKeys;
                }
            }
            
            public object ToLiquid()
            {
                return this;
            }

            [JsonIgnore]
            [Rock.Data.LavaIgnore]
            public object this[object key]
            {
               get
                {
                   switch( key.ToStringSafe() )
                   {
                       case "Member": return Member;
                       case "SpouseName": return SpouseName;
                       case "ChildrenInfo": return ChildrenInfo;
                   }

                    return null;
                }
            }
            
            public bool ContainsKey( object key )
            {
                var additionalKeys = new List<string> { "Member", "SpouseName", "ChildrenInfo" };
                if ( additionalKeys.Contains( key.ToStringSafe() ) )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            //
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editing group.
        /// used for public / protected properties
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is editing group; otherwise, <c>false</c>.
        /// </value>
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

        #endregion

        #region Base Control Methods

        protected abstract void HandlePageAction( string action, string parameters );
        protected abstract void FinalizePresentView( Dictionary<string, object> mergeFields, bool enableDebug );

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ExcludedAttribKeys = new List<string>( );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( MainPanel );

            // get the group id
            if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupId" ) ) )
            {
                _groupId = Convert.ToInt32( PageParameter( "GroupId" ) );
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( IsEditingGroup == true )
            {
                Group group = new GroupService( new RockContext() ).Get( _groupId );
                group.LoadAttributes();

                AttributesPlaceholder.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( group, AttributesPlaceholder, false, BlockValidationGroup, ExcludedAttribKeys );
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
            
            // add a navigate event to cature when someone presses the back button
            var sm = ScriptManager.GetCurrent( Page );
            sm.EnableSecureHistoryState = false;
            sm.Navigate += sm_Navigate;
        }

        /// <summary>
        /// Handles the Navigate event of the sm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        public void sm_Navigate( object sender, HistoryEventArgs e )
        {
            // show the view mode
            GroupEdit.Visible = false;
            GroupView.Visible = true;
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
        }

        /// <summary>
        /// Handles the Click event of the btnSaveGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveGroup_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );

            Group group = groupService.Get( _groupId );

            if ( group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                //group.Name = GroupName.Text;
                //group.Description = GroupDesc.Text;
                //group.IsActive = IsActive.Checked;

                if ( Schedule.Visible )
                {
                    if ( group.Schedule == null )
                    {
                        group.Schedule = new Schedule();
                        group.Schedule.iCalendarContent = null;
                    }

                    group.Schedule.WeeklyDayOfWeek = DayOfWeekPicker.SelectedDayOfWeek;
                    group.Schedule.WeeklyTimeOfDay = MeetingTime.SelectedTime;
                }

                // set attributes
                group.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( AttributesPlaceholder, group );
                
                rockContext.SaveChanges();
                group.SaveAttributeValues( rockContext );
            }

            this.IsEditingGroup = false;

            // reload the group info
            GroupEdit.Visible = false;
            GroupView.Visible = true;
            DisplayViewGroup();
        }

        /// <summary>
        /// Handles the Click event of the lbCancelGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelGroup_Click( object sender, EventArgs e )
        {
            GroupEdit.Visible = false;
            GroupView.Visible = true;
            this.IsEditingGroup = false;

            var sm = ScriptManager.GetCurrent( Page );
            sm.AddHistoryPoint( "Action", "ViewGroup" );
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Route the request to the correct panel
        /// </summary>
        private void RouteAction()
        {
            if ( Request.Form["__EVENTARGUMENT"] != null )
            {
                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];

                    //int argument = 0;
                    //int.TryParse( parameters, out argument );

                    HandlePageAction( action, parameters );
                }
            }
            else
            {
                GroupEdit.Visible = false;
                GroupView.Visible = true;
                DisplayViewGroup();
            }
        }

        /// Displays the view group  using a lava template
        protected void DisplayViewGroup( )
        {
            if ( _groupId > 0 )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();

                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );
                GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                
                var qry = groupService
                    .Queryable( "GroupLocations,Members,Members.Person,Members.Person.PhoneNumbers,GroupType" )
                    .Where( g => g.Id == _groupId );

                if ( !enableDebug )
                {
                    qry = qry.AsNoTracking();
                }

                var group = qry.FirstOrDefault();
                mergeFields.Add( "Group", group );

                // get IDs required for finding children of the group members
                Guid adultGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT );
                int adultRoleId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Roles.First( a => a.Guid == adultGuid ).Id;
                int familyGroupTypeId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

                // order group members by name
                var groupMembers = groupMemberService.Queryable( ).Where( gm => gm.GroupId == group.Id ).OrderBy( m => m.Person.LastName ).ThenBy( m => m.Person.FirstName );

                // now build a list of all members with their spouse / children
                List<GroupMemberWithFamily> groupMembersWithFamily = new List<GroupMemberWithFamily>( );
                foreach( GroupMember member in groupMembers )
                {
                    GroupMemberWithFamily memberWithFamily = new GroupMemberWithFamily( member );
                    
                    // spouse?
                    Person spouse = member.Person.GetSpouse( );
                    if( spouse != null )
                    {
                        memberWithFamily.SpouseName = spouse.FullName;
                    }

                    // kids?
                    // get all families where we're the adult 
                    var families = groupMemberService.Queryable().Where( gm => gm.PersonId == member.PersonId &&  // any group member that's us
                                                                               gm.GroupRoleId == adultRoleId &&  // where the groupmember's role is adult
                                                                               gm.Group.GroupTypeId == familyGroupTypeId ) //and the type is family
                                                                    .Select( g => g.Group ); 

                    // take a list of families, but only the kids within the family
                    var kidsInFamilies = families.Select( f => new { Kids = f.Members.Where( m => m.GroupRoleId != adultRoleId ) } ).ToList( );

                    // now for each family, add each kid within the family
                    foreach( var family in kidsInFamilies )
                    {
                        foreach( var kid in family.Kids )
                        {
                            memberWithFamily.ChildrenInfo.Add( kid.Person.FullName + ", Age: " + kid.Person.Age );
                        }
                    }

                    groupMembersWithFamily.Add( memberWithFamily );
                }

                // add lists of group members by active, inactive, and pending to make it easier / faster to organize them in lava
                List<GroupMemberWithFamily> activeGroupMembers = groupMembersWithFamily.Where( gm => gm.Member.GroupMemberStatus == GroupMemberStatus.Active ).ToList( );
                List<GroupMemberWithFamily> inactiveGroupMembers = groupMembersWithFamily.Where( gm => gm.Member.GroupMemberStatus == GroupMemberStatus.Inactive ).ToList( );
                List<GroupMemberWithFamily> pendingGroupMembers = groupMembersWithFamily.Where( gm => gm.Member.GroupMemberStatus == GroupMemberStatus.Pending ).ToList( );

                
                mergeFields.Add( "ActiveGroupMembers", activeGroupMembers );
                mergeFields.Add( "InactiveGroupMembers", inactiveGroupMembers );
                mergeFields.Add( "PendingGroupMembers", pendingGroupMembers );


                // add linked pages
                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( "PersonDetailPage", LinkedPageRoute( "PersonDetailPage" ) );
                linkedPages.Add( "RosterPage", LinkedPageUrl( "RosterPage", null ) );
                linkedPages.Add( "CommunicationPage", LinkedPageUrl( "CommunicationPage", null ) );
                mergeFields.Add( "LinkedPages", linkedPages );

                // add collection of allowed security actions
                Dictionary<string, object> securityActions = new Dictionary<string, object>();
                securityActions.Add( "View", group != null && group.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                securityActions.Add( "Edit", group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) );
                securityActions.Add( "Administrate", group != null && group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) );
                mergeFields.Add( "AllowedActions", securityActions );

                Dictionary<string, object> currentPageProperties = new Dictionary<string, object>();
                currentPageProperties.Add( "Id", RockPage.PageId );
                currentPageProperties.Add( "Path", Request.Path );
                mergeFields.AddOrIgnore( "CurrentPage", currentPageProperties );

                // let the derived class add in anything it needs, and render the actual content
                FinalizePresentView( mergeFields, enableDebug );
            }
            else
            {
                MainViewContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }
        }
        
        /// <summary>
        /// Displays the edit panel, letting a coach edit the attributes of their group.
        /// Pass an optional list of Group Attribute Keys to exclude if there are attributes of the group the coach
        /// should not be allowed to edit.
        /// </summary>
        protected void DisplayEditGroup( )
        {
            GroupEdit.Visible = true;
            GroupView.Visible = false;
            
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
                    GroupName.Text = "<h1 class=\"margin-t-sm\">" + group.Name + "</h1>";
                    //GroupDesc.Text = group.Description;
                    //IsActive.Checked = group.IsActive;

                    if ( ( group.GroupType.AllowedScheduleTypes & ScheduleType.Weekly ) == ScheduleType.Weekly )
                    {
                        Schedule.Visible = group.Schedule == null || group.Schedule.ScheduleType == ScheduleType.Weekly;
                        if ( group.Schedule != null )
                        {
                            DayOfWeekPicker.SelectedDayOfWeek = group.Schedule.WeeklyDayOfWeek;
                            MeetingTime.SelectedTime = group.Schedule.WeeklyTimeOfDay;
                        }
                        else
                        {
                            DayOfWeekPicker.SelectedDayOfWeek = null;
                            MeetingTime.SelectedTime = null;
                        }
                    }
                    else
                    {
                        Schedule.Visible = false;
                    }
                    
                    group.LoadAttributes();
                    AttributesPlaceholder.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( group, AttributesPlaceholder, true, BlockValidationGroup, ExcludedAttribKeys );
                }
                else
                {
                    MainViewContent.Text = "<div class='alert alert-warning'>You do not have permission to edit this group.</div>";
                }
            }
            else
            {
                MainViewContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }

            var sm = ScriptManager.GetCurrent( Page );
            sm.AddHistoryPoint( "Action", "EditGroup" );
        }
        
        /// <summary>
        /// Sends the communication.
        /// </summary>
        protected void SendCommunication( List<int?> primaryAliasIds )
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

                // Get the primary aliases
                foreach ( int personAlias in primaryAliasIds )
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

        protected bool AddGroupMember( int personId, int groupId, int groupRoleId, GroupMemberStatus memberStatus )
        {
            var rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                            
            // check to see if the person is already a member of this group
            var existingGroupMember = groupMemberService.GetByGroupIdAndPersonId( groupId, personId ).SingleOrDefault( );
            if ( existingGroupMember == null )
            {
                GroupMember groupMember = new GroupMember { Id = 0 };
                groupMember.GroupId = groupId;
                groupMember.PersonId = personId;

                // set their role
                GroupTypeRole role = new GroupTypeRoleService( rockContext ).Get( groupRoleId );
                groupMember.GroupRoleId = role.Id;

                // set their status.
                groupMember.GroupMemberStatus = memberStatus;
                    
                // using WrapTransaction because there are two Saves
                rockContext.WrapTransaction( () =>
                {
                    groupMemberService.Add( groupMember );

                    rockContext.SaveChanges();
                    groupMember.SaveAttributeValues( rockContext );
                } );

                Group group = new GroupService( rockContext ).Get( groupMember.GroupId );
                if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                {
                    Rock.Security.Role.Flush( group.Id );
                }
                
                return true;
            }

            return false;
        }

        #endregion
    }
}