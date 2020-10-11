﻿// <copyright>
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
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Group Detail Lava" )]
    [Category( "Groups" )]
    [Description( "Presents the details of a group using Lava" )]

    #region Block Attributes

    [LinkedPage( "Person Detail Page",
        Key = AttributeKey.PersonDetailPage,
        Description = "Page to link to for more information on a group member.",
        IsRequired = false,
        Order = 0 )]

    [LinkedPage( "Group Member Add Page",
        Key = AttributeKey.GroupMemberAddPage,
        Description = "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage( "Roster Page",
        Key = AttributeKey.RosterPage,
        Description = "The page to link to to view the roster.",
        IsRequired = true,
        Order = 2 )]

    [LinkedPage( "Attendance Page",
        Key = AttributeKey.AttendancePage,
        Description = "The page to link to to manage the group's attendance.",
        IsRequired = true,
        Order = 3 )]

    [LinkedPage( "Communication Page",
        Key = AttributeKey.CommunicationPage,
        Description = "The communication page to use for sending emails to the group members.",
        IsRequired = true,
        Order = 4 )]

    [LinkedPage( "Alternate Communication Page",
        Key = AttributeKey.AlternateCommunicationPage,
        Description = "The communication page to use for sending an alternate communication to the group members.",
        IsRequired = false,
        Order = 5 )]

    [BooleanField( "Hide the 'Active' Group checkbox",
        Key = AttributeKey.HideActiveGroupCheckbox,
        Description = "Set this to true to hide the checkbox for 'Active' for the group.",
        DefaultBooleanValue = false,
        Order = 6 )]

    [BooleanField("Hide the 'Public' Group checkbox",
        Key = AttributeKey.HidePublicGroupCheckbox,
        Description = "Set this to true to hide the checkbox for 'Public' for the group.",
        DefaultBooleanValue = true,
        Order = 7 )]

    [BooleanField( "Hide Inactive Group Member Status",
        Description = "Set this to true to hide the radiobox for the 'Inactive' group member status.",
        DefaultBooleanValue = false,
        Order = 8 )]

    [BooleanField( "Hide Group Member Role",
        Key = AttributeKey.HideGroupMemberRole,
        Description = "Set this to true to hide the drop down list for the 'Role' when editing a group member. If set to 'true' then the default group role will be used when adding a new member.",
        DefaultBooleanValue = false,
        Order = 9 )]

    [BooleanField( "Hide Group Description Edit",
        Key = AttributeKey.HideGroupDescriptionEdit,
        Description = "Set this to true to hide the edit box for group 'Description'.",
        DefaultBooleanValue = false,
        Order = 10 )]

    [BooleanField( "Enable Group Capacity Edit",
        Key = AttributeKey.EnableGroupCapacityEdit,
        Description = "Enables changing Group Capacity when editing a group. Note: The group type must have a 'Group Capacity Rule'.",
        DefaultBooleanValue = false,
        Order = 11 )]

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The lava template to use to format the group details.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = "{% include '~~/Assets/Lava/GroupDetail.lava' %}",
        Order = 12 )]

    [BooleanField( "Enable Location Edit",
        Key = AttributeKey.EnableLocationEdit,
        Description = "Enables changing locations when editing a group.",
        DefaultBooleanValue = false,
        Order = 13 )]

    [BooleanField( "Allow Group Member Delete",
        Key = AttributeKey.AllowGroupMemberDelete,
        Description = "Should deleting of group members be allowed?",
        DefaultBooleanValue = true,
        Order = 14 )]

    [CodeEditorField( "Edit Group Pre-HTML",
        Key = AttributeKey.EditGroupPreHTML,
        Description = "HTML to display before the edit group panel.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "",
        Category = "HTML Wrappers",
        Order = 15 )]

    [CodeEditorField( "Edit Group Post-HTML",
        Key = AttributeKey.EditGroupPostHTML,
        Description = "HTML to display after the edit group panel.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "",
        Category = "HTML Wrappers",
        Order = 16 )]

    [CodeEditorField( "Edit Group Member Pre-HTML",
        Key = AttributeKey.EditGroupMemberPreHTML,
        Description = "HTML to display before the edit group member panel.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "",
        Category = "HTML Wrappers",
        Order = 17 )]

    [CodeEditorField( "Edit Group Member Post-HTML",
        Key = AttributeKey.EditGroupMemberPostHTML,
        Description = "HTML to display after the edit group member panel.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "",
        Category = "HTML Wrappers",
        Order = 18 )]

    [BooleanField( "Enable Communication Preference",
        Description = "Determines if the currently logged in individual should be allowed to set their communication preference for the group.",
        DefaultBooleanValue = false,
        Order = 19 )]

    #endregion Block Attributes

    public partial class GroupDetailLava : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupName = "GroupName";
            public const string CommunicationId = "CommunicationId";
        }

        private static class AttributeKey
        {
            public const string PersonDetailPage = "PersonDetailPage";
            public const string GroupMemberAddPage = "GroupMemberAddPage";
            public const string RosterPage = "RosterPage";
            public const string AttendancePage = "AttendancePage";
            public const string CommunicationPage = "CommunicationPage";
            public const string AlternateCommunicationPage = "AlternateCommunicationPage";
            public const string HideActiveGroupCheckbox = "HideActiveGroupCheckbox";
            public const string HidePublicGroupCheckbox = "HidePublicGroupCheckbox";
            public const string HideInactiveGroupMemberStatus = "HideInactiveGroupMemberStatus";
            public const string HideGroupMemberRole = "HideGroupMemberRole";
            public const string HideGroupDescriptionEdit = "HideGroupDescriptionEdit";
            public const string EnableGroupCapacityEdit = "EnableGroupCapacityEdit";
            public const string LavaTemplate = "LavaTemplate";
            public const string EnableLocationEdit = "EnableLocationEdit";
            public const string AllowGroupMemberDelete = "AllowGroupMemberDelete";
            public const string EditGroupPreHTML = "EditGroupPre-HTML";
            public const string EditGroupPostHTML = "EditGroupPost-HTML";
            public const string EditGroupMemberPreHTML = "EditGroupMemberPre-HTML";
            public const string EditGroupMemberPostHTML = "EditGroupMemberPost-HTML";
            public const string EnableCommunicationPreference = "EnableCommunicationPreference";
        }

        #endregion Attribute Keys

        #region Fields

        // used for private variables
        private int _groupId = 0;
        private const string MEMBER_LOCATION_TAB_TITLE = "Member Location";
        private const string OTHER_LOCATION_TAB_TITLE = "Other Location";

        private readonly List<string> _tabs = new List<string> { MEMBER_LOCATION_TAB_TITLE, OTHER_LOCATION_TAB_TITLE };

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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editing group member.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is editing group member; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets the current group member identifier.
        /// </summary>
        /// <value>
        /// The current group member identifier.
        /// </value>
        public int CurrentGroupMemberId
        {
            get
            {
                return ViewState["CurrentGroupMemberId"] as int? ?? 0;
            }

            set
            {
                ViewState["CurrentGroupMemberId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the location type tab.
        /// </summary>
        /// <value>
        /// The location type tab.
        /// </value>
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

        /// <summary>
        /// Gets a value indicating whether enable communication preference has been set.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the enable communication preference has been set; otherwise, <c>false</c>.
        /// </value>
        private bool EnableCommunicationPreference
        {
            get { return this.GetAttributeValue( AttributeKey.EnableCommunicationPreference ).AsBooleanOrNull() ?? false; }
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

            // get the group id
            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.GroupId ) ) )
            {
                _groupId = Convert.ToInt32( PageParameter( PageParameterKey.GroupId ) );
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
                Rock.Attribute.Helper.AddEditControls( groupMember, phGroupMemberAttributes, true, string.Empty, true );
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
                BindCommunicationPreference();
            }

            // add a navigate event to capture when someone presses the back button
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
                group.Name = tbName.Text;
                group.Description = tbDescription.Text;
                group.IsActive = cbIsActive.Checked;
                group.IsPublic = cbIsPublic.Checked;

                // Don't save inactive properties if the group is active.
                group.InactiveReasonValueId = cbIsActive.Checked ? null : ddlInactiveReason.SelectedValueAsInt();
                group.InactiveReasonNote = cbIsActive.Checked ? null : tbInactiveNote.Text;

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

                if ( nbGroupCapacity.Visible )
                {
                    group.GroupCapacity = nbGroupCapacity.Text.AsIntegerOrNull();
                }

                // set attributes
                group.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, group );

                // configure locations
                if ( GetAttributeValue( AttributeKey.EnableLocationEdit ).AsBoolean() )
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
                    else
                    {
                        var groupLocation = group.GroupLocations.FirstOrDefault();

                        if ( groupLocation != null )
                        {
                            rockContext.GroupLocations.Remove( groupLocation );
                        }
                    }
                }

                rockContext.WrapTransaction( () => {

                    rockContext.SaveChanges();

                    group.SaveAttributeValues( rockContext );

                    if ( group.IsActive == false && cbInactivateChildGroups.Checked )
                    {
                        var allActiveChildGroupsId = groupService.GetAllDescendentGroupIds( group.Id, false );
                        var allActiveChildGroups = groupService.GetByIds( allActiveChildGroupsId );
                        foreach ( var childGroup in allActiveChildGroups )
                        {
                            if ( childGroup.IsActive )
                            {
                                childGroup.IsActive = false;
                                childGroup.InactiveReasonValueId = ddlInactiveReason.SelectedValueAsInt();
                                childGroup.InactiveReasonNote = "Parent Deactivated";
                                if ( tbInactiveNote.Text.IsNotNullOrWhiteSpace() )
                                {
                                    childGroup.InactiveReasonNote += ": " + tbInactiveNote.Text;
                                }
                            }
                        }

                        rockContext.SaveChanges();
                    }
                } );

            }

            this.IsEditingGroup = false;

            // reload the group info
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = true;
            DisplayViewGroup();
        }

        /// <summary>
        /// Handles the Click event of the lbCancelGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelGroup_Click( object sender, EventArgs e )
        {
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = true;
            this.IsEditingGroup = false;

            var sm = ScriptManager.GetCurrent( Page );
            sm.AddHistoryPoint( "Action", "ViewGroup" );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveGroupMember_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            GroupTypeRole role = new GroupTypeRoleService( rockContext ).Get( ddlGroupRole.SelectedValueAsInt() ?? 0 );

            var groupMember = groupMemberService.Get( this.CurrentGroupMemberId );
            if ( groupMember == null )
            {
                groupMember = new GroupMember { Id = 0 };
                groupMember.GroupId = _groupId;

                // check to see if the person is already a member of the group/role
                var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                    _groupId, ppGroupMemberPerson.SelectedValue ?? 0, ddlGroupRole.SelectedValueAsId() ?? 0 );

                if ( existingGroupMember != null )
                {
                    // if so, don't add and show error message
                    var person = new PersonService( rockContext ).Get( ( int ) ppGroupMemberPerson.PersonId );

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

            // set their status.  If HideInactiveGroupMemberStatus is True, and they are already Inactive, keep their status as Inactive;
            bool hideGroupMemberInactiveStatus = this.GetAttributeValue( AttributeKey.HideInactiveGroupMemberStatus ).AsBooleanOrNull() ?? false;
            var selectedStatus = rblStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>();
            if ( !selectedStatus.HasValue )
            {
                if ( hideGroupMemberInactiveStatus )
                {
                    selectedStatus = GroupMemberStatus.Inactive;
                }
                else
                {
                    selectedStatus = GroupMemberStatus.Active;
                }
            }

            groupMember.GroupMemberStatus = selectedStatus.Value;
            groupMember.CommunicationPreference = rblCommunicationPreference.SelectedValueAsEnum<CommunicationType>();

            groupMember.LoadAttributes();

            Rock.Attribute.Helper.GetEditValues( phAttributes, groupMember );

            if ( !Page.IsValid )
            {
                return;
            }

            // if the groupMember IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of GroupMember didn't pass.
            // So, make sure a message is displayed in the validation summary
            cvEditGroupMember.IsValid = groupMember.IsValidGroupMember( rockContext );

            if ( !cvEditGroupMember.IsValid )
            {
                cvEditGroupMember.ErrorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return;
            }

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

            pnlEditGroupMember.Visible = false;
            pnlGroupView.Visible = true;
            DisplayViewGroup();
            this.IsEditingGroupMember = false;

            if ( groupMember.PersonId == CurrentPersonId )
            {
                tglCommunicationPreference.Checked = groupMember.CommunicationPreference == CommunicationType.SMS;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelGroupMember_Click( object sender, EventArgs e )
        {
            pnlEditGroupMember.Visible = false;
            pnlGroupView.Visible = true;
            this.IsEditingGroupMember = false;

            var sm = ScriptManager.GetCurrent( Page );
            sm.AddHistoryPoint( "Action", "ViewGroup" );
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirmDelete_Click( object sender, EventArgs e )
        {
            mdConfirmDelete.Hide();

            if ( GetAttributeValue( AttributeKey.AllowGroupMemberDelete ).AsBoolean() )
            {
                RockContext rockContext = new RockContext();
                GroupMemberService groupMemberService = new GroupMemberService( rockContext );

                var groupMember = groupMemberService.Get( this.CurrentGroupMemberId );
                if ( groupMember != null )
                {
                    groupMemberService.Delete( groupMember );
                }

                rockContext.SaveChanges();
            }

            DisplayViewGroup();
        }

        /// <summary>
        /// Handles the Click event of the lbLocationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the CheckedChanged event of the tglCommunicationPreference control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglCommunicationPreference_CheckedChanged( object sender, EventArgs e )
        {
            if ( CurrentPersonId == null || !EnableCommunicationPreference || _groupId.Equals( 0 ) )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var groupMember = groupMemberService.GetByGroupIdAndPersonId( _groupId, CurrentPersonId.Value ).FirstOrDefault();

                if ( groupMember == null )
                {
                    return;
                }

                groupMember.CommunicationPreference = tglCommunicationPreference.Checked ? CommunicationType.SMS : CommunicationType.Email;

                rockContext.SaveChanges();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the communication preference.
        /// </summary>
        private void BindCommunicationPreference()
        {
            if ( CurrentPersonId == null || !EnableCommunicationPreference || _groupId.Equals( 0 ) )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var groupMember = groupMemberService.GetByGroupIdAndPersonId( _groupId, CurrentPersonId.Value ).FirstOrDefault();

                if ( groupMember == null )
                {
                    return;
                }

                tglCommunicationPreference.Checked = groupMember.CommunicationPreference == CommunicationType.SMS;
            }
        }

        /// <summary>
        /// Route the request to the correct panel
        /// </summary>
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
                            sm.AddHistoryPoint( "Action", "EditGroup" );
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
                            if ( GetAttributeValue( AttributeKey.AllowGroupMemberDelete ).AsBoolean() )
                            {
                                groupMemberId = int.Parse( parameters );
                                DisplayDeleteGroupMember( groupMemberId );
                                sm.AddHistoryPoint( "Action", "DeleteMember" );
                            }
                            break;

                        case "SendCommunication":
                            SendCommunication();
                            break;

                        case "SendAlternateCommunication":
                            SendAlternateCommunication();
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

        /// <summary>
        /// setup block
        /// </summary>
        private void BlockSetup()
        {
            lGroupEditPreHtml.Text = GetAttributeValue( AttributeKey.EditGroupPreHTML );
            lGroupEditPostHtml.Text = GetAttributeValue( AttributeKey.EditGroupPostHTML );

            lGroupMemberEditPreHtml.Text = GetAttributeValue( AttributeKey.EditGroupMemberPreHTML );
            lGroupMemberEditPostHtml.Text = GetAttributeValue( AttributeKey.EditGroupMemberPostHTML );

            bool hideActiveGroupCheckbox = this.GetAttributeValue( AttributeKey.HideActiveGroupCheckbox ).AsBooleanOrNull() ?? false;
            if ( hideActiveGroupCheckbox )
            {
                cbIsActive.Visible = false;
            }

            bool hidePublicGroupCheckbox = this.GetAttributeValue( AttributeKey.HidePublicGroupCheckbox ).AsBooleanOrNull() ?? true;
            if ( hidePublicGroupCheckbox )
            {
                cbIsPublic.Visible = false;
            }

            bool hideDescriptionEdit = this.GetAttributeValue( AttributeKey.HideGroupDescriptionEdit ).AsBooleanOrNull() ?? false;
            if ( hideDescriptionEdit )
            {
                tbDescription.Visible = false;
            }
        }

        ////
        //// Group Methods

        /// <summary>
        /// Displays the view group  using a lava template
        /// </summary>
        private void DisplayViewGroup()
        {
            if ( _groupId > 0 )
            {
                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );

                var qry = groupService
                    .Queryable( "GroupLocations,Members,Members.Person,Members.Person.PhoneNumbers,GroupType" )
                    .Where( g => g.Id == _groupId );

                qry = qry.AsNoTracking();

                var group = qry.FirstOrDefault();

                // order group members by name
                if ( group != null )
                {
                    group.Members = group.Members.OrderBy( m => m.Person.LastName ).ThenBy( m => m.Person.FirstName ).ToList();
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "Group", group );

                // add linked pages
                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( AttributeKey.PersonDetailPage, LinkedPageRoute( AttributeKey.PersonDetailPage ) );
                linkedPages.Add( AttributeKey.RosterPage, LinkedPageRoute( AttributeKey.RosterPage ) );
                linkedPages.Add( AttributeKey.AttendancePage, LinkedPageRoute( AttributeKey.AttendancePage ) );
                linkedPages.Add( AttributeKey.CommunicationPage, LinkedPageRoute( AttributeKey.CommunicationPage ) );
                linkedPages.Add( AttributeKey.AlternateCommunicationPage, LinkedPageRoute( AttributeKey.AlternateCommunicationPage ) );
                mergeFields.Add( "LinkedPages", linkedPages );

                // add collection of allowed security actions
                Dictionary<string, object> securityActions = new Dictionary<string, object>();
                securityActions.Add( "View", group != null && group.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                securityActions.Add( "ManageMembers", group != null && group.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson ) );
                securityActions.Add( "Edit", group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) );
                securityActions.Add( "Administrate", group != null && group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) );
                mergeFields.Add( "AllowedActions", securityActions );

                Dictionary<string, object> currentPageProperties = new Dictionary<string, object>();
                currentPageProperties.Add( "Id", RockPage.PageId );
                currentPageProperties.Add( "Path", Request.Path );
                mergeFields.Add( "CurrentPage", currentPageProperties );

                string template = GetAttributeValue( AttributeKey.LavaTemplate );

                lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );

                dCommunicationsPreference.Visible = EnableCommunicationPreference && group.Members.Any( gm => gm.PersonId == CurrentPersonId );
            }
            else
            {
                dCommunicationsPreference.Visible = false;
                lContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }
        }

        /// <summary>
        /// Displays the edit group panel.
        /// </summary>
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
                    cbIsPublic.Checked = group.IsPublic;

                    if ( group.GroupType != null && group.GroupType.EnableInactiveReason )
                    {
                        ddlInactiveReason.Visible = true;
                        ddlInactiveReason.Items.Clear();
                        ddlInactiveReason.Items.Add( new ListItem() );
                        ddlInactiveReason.Required = group.GroupType.RequiresInactiveReason;

                        foreach ( var reason in new GroupTypeService( rockContext ).GetInactiveReasonsForGroupType( group.GroupTypeId ).Select( r => new { Text = r.Value, Value = r.Id } ) )
                        {
                            ddlInactiveReason.Items.Add( new ListItem( reason.Text, reason.Value.ToString() ) );
                        }

                        ddlInactiveReason.SelectedValue = group.InactiveReasonValueId.ToString();

                        tbInactiveNote.Visible = true;
                        tbInactiveNote.Text = group.InactiveReasonNote;
                    }

                    // The inactivate child groups checkbox should only be visible if there are children to inactivate. js on the page will consume this.
                    hfHasChildGroups.Value = groupService.GetAllDescendentGroupIds( group.Id, false ).Any() ? "true" : "false";

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

                    nbGroupCapacity.Text = group.GroupCapacity.ToString();
                    bool enableGroupCapacityEdit = this.GetAttributeValue( AttributeKey.EnableGroupCapacityEdit ).AsBooleanOrNull() ?? false;
                    if ( enableGroupCapacityEdit )
                    {
                        nbGroupCapacity.Visible = group.GroupType.GroupCapacityRule != GroupCapacityRule.None;
                    }

                    group.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( group, phAttributes, true, BlockValidationGroup );

                    // enable editing location
                    pnlGroupEditLocations.Visible = GetAttributeValue( AttributeKey.EnableLocationEdit ).AsBoolean();
                    if ( GetAttributeValue( AttributeKey.EnableLocationEdit ).AsBoolean() )
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

        /// <summary>
        /// logic to setup the groups location entry panel
        /// </summary>
        /// <param name="group">The group.</param>
        private void ConfigureGroupLocationControls( Group group )
        {
            var rockContext = new RockContext();
            ddlMember.Items.Clear();

            var groupType = GroupTypeCache.Get( group.GroupTypeId );
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
                        if ( ( groupTypeModes & GroupLocationPickerMode.Named ) == GroupLocationPickerMode.Named )
                        {
                            modes = modes | LocationPickerMode.Named;
                        }

                        if ( ( groupTypeModes & GroupLocationPickerMode.Address ) == GroupLocationPickerMode.Address )
                        {
                            modes = modes | LocationPickerMode.Address;
                        }

                        if ( ( groupTypeModes & GroupLocationPickerMode.Point ) == GroupLocationPickerMode.Point )
                        {
                            modes = modes | LocationPickerMode.Point;
                        }

                        if ( ( groupTypeModes & GroupLocationPickerMode.Polygon ) == GroupLocationPickerMode.Polygon )
                        {
                            modes = modes | LocationPickerMode.Polygon;
                        }

                        bool displayMemberTab = ( groupTypeModes & GroupLocationPickerMode.GroupMember ) == GroupLocationPickerMode.GroupMember;
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

                            // Add an empty selection to the list.
                            if ( ddlMember.Items.Count > 0 )
                            {
                                ddlMember.Items.Insert( 0, new ListItem( string.Empty ) );
                            }
                        }

                        if ( displayOtherTab )
                        {
                            locpGroupLocation.AllowedPickerModes = modes;
                        }

                        ddlLocationType.DataSource = groupType.LocationTypeValues.ToList();
                        ddlLocationType.DataBind();

                        LocationTypeTab = ( displayMemberTab && ddlMember.Items.Count > 0 ) ? MEMBER_LOCATION_TAB_TITLE : OTHER_LOCATION_TAB_TITLE;

                        var groupLocation = group.GroupLocations.FirstOrDefault();
                        if ( groupLocation != null && groupLocation.Location != null )
                        {
                            if ( displayOtherTab )
                            {
                                locpGroupLocation.SetBestPickerModeForLocation( groupLocation.Location );

                                //locpGroupLocation.MapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();

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

        /// <summary>
        /// helper method to toggle the group location tabs
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected string GetLocationTabClass( object property )
        {
            if ( property.ToString() == LocationTypeTab )
            {
                return "active";
            }

            return string.Empty;
        }

        /// <summary>
        /// Shows the selected pane.
        /// </summary>
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

        ////
        //// Group Member Methods

        /// <summary>
        /// Adds the group member.
        /// </summary>
        private void AddGroupMember()
        {
            this.IsEditingGroupMember = true;
            var personAddPage = GetAttributeValue( AttributeKey.GroupMemberAddPage );

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
                    queryParams.Add( PageParameterKey.GroupId, group.Id.ToString() );
                    queryParams.Add( PageParameterKey.GroupName, group.Name );
                    NavigateToLinkedPage( AttributeKey.GroupMemberAddPage, queryParams );
                }
            }
        }

        /// <summary>
        /// Displays the edit group member.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
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
                ppGroupMemberPerson.Enabled = true;
            }
            else
            {
                ppGroupMemberPerson.Enabled = false;
            }

            // load dropdowns
            LoadGroupMemberDropDowns( _groupId );

            // set values
            ppGroupMemberPerson.SetValue( groupMember.Person );
            ddlGroupRole.SetValue( groupMember.GroupRoleId );
            rblStatus.SetValue( ( int ) groupMember.GroupMemberStatus );
            rblCommunicationPreference.SetValue( groupMember.CommunicationPreference == CommunicationType.SMS ? "2" : "1" );

            bool hideGroupMemberInactiveStatus = this.GetAttributeValue( AttributeKey.HideInactiveGroupMemberStatus ).AsBooleanOrNull() ?? false;
            if ( hideGroupMemberInactiveStatus )
            {
                var inactiveItem = rblStatus.Items.FindByValue( ( ( int ) GroupMemberStatus.Inactive ).ToString() );
                if ( inactiveItem != null )
                {
                    rblStatus.Items.Remove( inactiveItem );
                }
            }
            bool hideGroupMemberRole = this.GetAttributeValue( AttributeKey.HideGroupMemberRole ).AsBooleanOrNull() ?? false;
            if ( hideGroupMemberRole )
            {
                pnlGroupMemberRole.Visible = false;
                pnlGroupMemberAttributes.AddCssClass( "col-md-12" ).RemoveCssClass( "col-md-6" );
            }
            else
            {
                pnlGroupMemberRole.Visible = true;
                pnlGroupMemberAttributes.AddCssClass( "col-md-6" ).RemoveCssClass( "col-md-12" );
            }

            // set attributes
            groupMember.LoadAttributes();
            phGroupMemberAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( groupMember, phGroupMemberAttributes, true, string.Empty, true );

            this.IsEditingGroupMember = true;
        }

        /// <summary>
        /// Displays the delete group member.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        private void DisplayDeleteGroupMember( int groupMemberId )
        {
            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            var groupMember = groupMemberService.Get( groupMemberId );
            if ( groupMember != null )
            {
                // persist the group member id for use in partial postbacks
                this.CurrentGroupMemberId = groupMember.Id;

                lConfirmDeleteMsg.Text = string.Format( "Are you sure you want to delete (remove) {0} from {1}?", groupMember.Person.FullName, groupMember.Group.Name );

                mdConfirmDelete.Show();
                //mdConfirmDelete.Header.Visible = false;
            }
        }

        /// <summary>
        /// Loads the group member drop downs.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
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

        /// <summary>
        /// Sends the communication.
        /// </summary>
        private void SendCommunication()
        {
            // create communication
            if ( this.CurrentPerson != null && _groupId != -1 && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.CommunicationPage ) ) )
            {
                var rockContext = new RockContext();
                var service = new Rock.Model.CommunicationService( rockContext );
                var communication = new Rock.Model.Communication();
                communication.IsBulkCommunication = false;
                communication.Status = Rock.Model.CommunicationStatus.Transient;

                communication.SenderPersonAliasId = this.CurrentPersonAliasId;

                service.Add( communication );

                var personAliasIds = new GroupMemberService( rockContext ).Queryable()
                                    .Where( m => m.GroupId == _groupId && m.GroupMemberStatus != GroupMemberStatus.Inactive )
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
                queryParameters.Add( PageParameterKey.CommunicationId, communication.Id.ToString() );

                NavigateToLinkedPage( AttributeKey.CommunicationPage, queryParameters );
            }
        }

        /// <summary>
        /// Sends the communication.
        /// </summary>
        private void SendAlternateCommunication()
        {
            // create communication
            if ( this.CurrentPerson != null && _groupId != -1 && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.CommunicationPage ) ) )
            {
                var rockContext = new RockContext();
                var service = new Rock.Model.CommunicationService( rockContext );
                var communication = new Rock.Model.Communication();
                communication.IsBulkCommunication = false;
                communication.Status = Rock.Model.CommunicationStatus.Transient;

                communication.SenderPersonAliasId = this.CurrentPersonAliasId;

                service.Add( communication );

                var personAliasIds = new GroupMemberService( rockContext ).Queryable()
                                    .Where( m => m.GroupId == _groupId && m.GroupMemberStatus != GroupMemberStatus.Inactive )
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
                queryParameters.Add( PageParameterKey.CommunicationId, communication.Id.ToString() );

                NavigateToLinkedPage( AttributeKey.AlternateCommunicationPage, queryParameters );
            }
        }

        #endregion
    }
}