using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using CSScriptLibrary;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_lakepointe.Groups
{
    [BooleanField( "Allow Communications",
        Description = "Is the user allowed to send a communication from this block.",
        DefaultValue = "true",
        DefaultBooleanValue = true,
        IsRequired = false,
        Key = AttributeKey.AllowCommunications,
        Category = "Configuration",
        Order = 0 )]

    [BooleanField( "Allow Private Groups",
        Description = "Can the user view a private group in the group viewer?",
        DefaultBooleanValue = false,
        DefaultValue = "False",
        IsRequired = false,
        Key = AttributeKey.AllowPrivateGroups,
        Category = "Configuration",
        Order = 1 )]

    [BooleanField( "Allow Group Member Edit",
        Description = "Can a user with Manage Member rights edit a group member?",
        DefaultBooleanValue = false,
        IsRequired = false,
        Key = AttributeKey.AllowMemberEdit,
        Category = "Configuration",
        Order = 2 )]

    [LinkedPage( "Communication Page",
        Description = "The page to redirect the user to for creating a communication.",
        DefaultValue = "",
        IsRequired = false,
        Key = AttributeKey.CommunicationsPage,
        Category = "Configuration",
        Order = 3 )]

    [BooleanField( "Filter by Gender",
        Description = "Filter the group member list by gender.",
        DefaultBooleanValue = true,
        DefaultValue = "True",
        IsRequired = false,
        Key = AttributeKey.FilterByGender,
        Category = "Configuration",
        Order = 4 )]

    [BooleanField( "Filter by Group Role",
        Description = "Filter the group member by the Group Member Role.",
        DefaultBooleanValue = true,
        DefaultValue = "True",
        IsRequired = false,
        Key = AttributeKey.FilterByGroupRole,
        Category = "Configuration",
        Order = 5 )]

    [BooleanField( "Filter by Group Member Status",
        Description = "Filter the group member list by by Group Member Status.",
        DefaultBooleanValue = true,
        DefaultValue = "True",
        IsRequired = false,
        Key = AttributeKey.FilterByGroupStatus,
        Category = "Configuration",
        Order = 5 )]

    [BooleanField( "Show Common Group Member Properties",
        Description = "Show the group member's Communication Preference, scheduling properties, and Note to people with edit access",
        DefaultBooleanValue = false,
        DefaultValue = "false",
        IsRequired = false,
        Key = AttributeKey.ShowCommonProperties,
        Category = "Group Member",
        Order = 1 )]

    [BooleanField( "Show Photo",
        Description = "Should the group member's photo.",
        DefaultBooleanValue = true,
        DefaultValue = "true",
        IsRequired = false,
        Key = AttributeKey.ShowPhoto,
        Category = "Group Member",
        Order = 2 )]

    [BooleanField( "Show Home Address",
        Description = "Show the group member's home address.",
        DefaultBooleanValue = true,
        DefaultValue = "true",
        IsRequired = false,
        Key = AttributeKey.ShowHomeAddress,
        Category = "Group Member",
        Order = 3 )]

    [BooleanField( "Show Email Address",
        Description = "Show the group member's email address.",
        IsRequired = false,
        DefaultBooleanValue = true,
        DefaultValue = "true",
        Key = AttributeKey.ShowEmailAddress,
        Category = "Group Member",
        Order = 4 )]

    [DefinedValueField( "Phone Numbers to Show",
        Description = "The phone numbers to display on the group member detail.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        AllowMultiple = true,
        IncludeInactive = false,
        IsRequired = true,
        Key = AttributeKey.PhoneNumberTypes,
        Category = "Group Member",
        Order = 5 )]

    [BooleanField( "Show First Attended",
        Description = "Show the date that the group member first attended the group.",
        DefaultBooleanValue = true,
        DefaultValue = "true",
        IsRequired = false,
        Key = AttributeKey.ShowFirstAttended,
        Category = "Group Member",
        Order = 6 )]

    [BooleanField( "Show Last Attended",
        Description = "Show the date that the group member most recently attended the group.",
        DefaultBooleanValue = true,
        DefaultValue = "true",
        IsRequired = false,
        Key = AttributeKey.ShowLastAttended,
        Category = "Group Member",
        Order = 7 )]

    [BooleanField( "Show Birthdate",
        Description = "Show the group member's birthday in details block.",
        DefaultBooleanValue = false,
        DefaultValue = "false",
        IsRequired = false,
        Key = AttributeKey.ShowBirthdate,
        Category = "Group Member",
        Order = 8 )]

    [DisplayName( "Group Roster" )]
    [Category( "LPC > Groups" )]
    [Description( "Displays the members of a group. Group Leader Toolbox V2." )]
    public partial class GroupRoster : RockBlock
    {
        #region AttributeKeys
        protected static class AttributeKey
        {
            public const string AllowCommunications = "AllowCommunications";
            public const string AllowMemberEdit = "AllowMemberEdit";
            public const string AllowPrivateGroups = "AllowPrivateGroups";
            public const string CommunicationsPage = "CommunicationsPage";
            public const string FilterByGender = "FilterByGender";
            public const string FilterByGroupRole = "FilterByGroupRole";
            public const string FilterByGroupStatus = "FilterByGroupStatus";
            public const string PhoneNumberTypes = "PhoneNumberTypes";
            public const string ShowBirthdate = "ShowBirthdate";
            public const string ShowFirstAttended = "ShowFirstAttended";
            public const string ShowEmailAddress = "ShowEmailAddress";
            public const string ShowHomeAddress = "ShowHomeAddress";
            public const string ShowLastAttended = "ShowLastAttended";
            public const string ShowPhoto = "ShowPhoto";
            public const string ShowCommonProperties = "ShowCommonProperties";
        }

        private const string EditGroupMembersDefinedValueGuid = "1f54b1cc-b94c-429b-b927-1e542d8c7fe3";
        #endregion

        #region Fields
        private RockContext _rockContext = null;
        private Group _selectedGroup = null;
        private bool? _showPhoto = null;
        private bool? _showAddress = null;
        private bool? _showBirthdate = null;
        private bool? _showEmailAddress = null;
        private bool? _showFirstAttended = null;
        private bool? _showLastAttended = null;
        private bool? _showCommonProperties = null;
        private bool? _allowCommunications = null;
        private bool? _filterByGender = null;
        private bool? _filterByGroupStatus = null;
        private bool? _filterByGroupRole = null;
        private string _communicationPageRoute = null;
        private SortedList<int, DefinedValueCache> _phoneTypes = null;
        private int gmCount = 0;
        protected FeatureSet _featureSet;

        #endregion

        #region Properties
        private int GroupId
        {
            get
            {
                return PageParameter( "GroupId" ).AsInteger();
            }
        }

        private Group SelectedGroup
        {
            get
            {
                if ( _selectedGroup == null )
                {
                    LoadGroup();
                }

                return _selectedGroup;
            }
            set
            {
                _selectedGroup = value;
            }

        }

        private bool AllowCommunications
        {
            get
            {
                if ( !_allowCommunications.HasValue )
                {
                    _allowCommunications = GetAttributeValue( AttributeKey.AllowCommunications ).AsBoolean();
                }
                return _allowCommunications.Value;
            }
        }

        private bool AllowMemberEdit
        {
            get
            {
                if ( SelectedGroup == null )
                {
                    return false;
                }
                if ( ( SelectedGroup.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson )
                    || SelectedGroup.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    && GetAttributeValue( AttributeKey.AllowMemberEdit ).AsBoolean() )
                {
                    return true;
                }
                else
                {
                    return _featureSet.EditGroupMembers;
                }
            }
        }

        private string CommunicationPageRoute
        {
            get
            {
                if ( String.IsNullOrWhiteSpace( _communicationPageRoute ) )
                {
                    _communicationPageRoute = LinkedPageRoute( AttributeKey.CommunicationsPage );
                }
                return _communicationPageRoute;
            }
        }

        private bool FilterByGender
        {
            get
            {
                if ( !_filterByGender.HasValue )
                {
                    _filterByGender = GetAttributeValue( AttributeKey.FilterByGender ).AsBoolean();
                }

                return _filterByGender.Value;
            }
        }

        private bool FilterByGroupRole
        {
            get
            {
                if ( !_filterByGroupRole.HasValue )
                {
                    _filterByGroupRole = GetAttributeValue( AttributeKey.FilterByGroupRole ).AsBoolean();
                }
                return _filterByGroupRole.Value;
            }
        }

        private bool FilterByGroupStatus
        {
            get
            {
                if ( !_filterByGroupStatus.HasValue )
                {
                    _filterByGroupStatus = GetAttributeValue( AttributeKey.FilterByGroupStatus ).AsBoolean();
                }
                return _filterByGroupStatus.Value;
            }
        }

        private bool ShowPhoto
        {
            get
            {
                if ( !_showPhoto.HasValue )
                {
                    _showPhoto = GetAttributeValue( AttributeKey.ShowPhoto ).AsBoolean();
                }

                return _showPhoto.Value;
            }
        }

        private bool ShowAddress
        {
            get
            {
                if ( !_showAddress.HasValue )
                {
                    _showAddress = GetAttributeValue( AttributeKey.ShowHomeAddress ).AsBoolean();
                }

                return _showAddress.Value;
            }
        }

        private bool ShowBirthDate
        {
            get
            {
                if ( !_showBirthdate.HasValue )
                {
                    _showBirthdate = GetAttributeValue( AttributeKey.ShowBirthdate ).AsBoolean();
                }
                return _showBirthdate.HasValue;
            }
        }

        private bool ShowEmailAddress
        {
            get
            {
                if ( !_showEmailAddress.HasValue )
                {
                    _showEmailAddress = GetAttributeValue( AttributeKey.ShowEmailAddress ).AsBoolean();
                }
                return _showEmailAddress.Value;
            }
        }

        private bool ShowFirstAttended
        {
            get
            {
                if ( !_showFirstAttended.HasValue )
                {
                    _showFirstAttended = GetAttributeValue( AttributeKey.ShowFirstAttended ).AsBoolean();
                }
                return _showFirstAttended.Value;
            }
        }

        private bool ShowLastAttended
        {
            get
            {
                if ( !_showLastAttended.HasValue )
                {
                    _showLastAttended = GetAttributeValue( AttributeKey.ShowLastAttended ).AsBoolean();
                }
                return _showLastAttended.Value;
            }
        }

        private bool ShowCommonProperties
        {
            get
            {
                if ( !_showCommonProperties.HasValue )
                {
                    _showCommonProperties = GetAttributeValue( AttributeKey.ShowCommonProperties ).AsBoolean();
                }
                return _showCommonProperties.Value;
            }
        }

        private SortedList<int, DefinedValueCache> PhoneTypes
        {
            get
            {
                if ( _phoneTypes == null )
                {
                    LoadPhoneTypeList();
                }

                return _phoneTypes;
            }
        }

        public string cardBodyMinHeight
        {
            get
            {
                if ( ShowCommonProperties && SelectedGroup.GroupType.IsSchedulingEnabled && !SelectedGroup.DisableScheduling )
                {
                    return "375px";
                }
                else if ( ShowCommonProperties )
                {
                    return "300px";
                }
                else
                {
                    return "250px";
                }
            }
        }

        private List<Assignment> Assignments
        {
            get
            {
                return ( List<Assignment> ) ViewState["assignments"];
            }
            set
            {
                ViewState["assignments"] = value;
            }
        }
        #endregion

        #region Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _rockContext = new RockContext();

            gGroupPreferenceAssignments.DataKeyNames = new string[] { "AssignmentGuid" };
            gGroupPreferenceAssignments.Actions.ShowAdd = true;
            gGroupPreferenceAssignments.Actions.AddClick += gGroupPreferenceAssignments_Add;
            gGroupPreferenceAssignments.GridRebind += gGroupPreferenceAssignments_GridRebind;

            this.BlockUpdated += ServeTeamRoster_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upGroupRoster );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            var cpr = CommunicationPageRoute;
            UpdateNotification( null, null, NotificationBoxType.Default );
            _featureSet = new FeatureSet( GetFeatureSetStrings() );
            if ( !IsPostBack )
            {
                BlockSetup();
            }
        }
        #endregion

        #region Events
        private void ServeTeamRoster_BlockUpdated( object sender, EventArgs e )
        {
            BlockSetup();
        }
        protected void gfGroupMember_ApplyFilterClick( object sender, EventArgs e )
        {
            gfGroupMember.SaveUserPreference( GetGroupFilterKey( "Gender" ), "Gender", string.Join( ",", cblGender.SelectedValues ) );
            gfGroupMember.SaveUserPreference( GetGroupFilterKey( "Role" ), "Group Role", string.Join( ",", cblGroupRole.SelectedValues ) );
            gfGroupMember.SaveUserPreference( GetGroupFilterKey( "Status" ), "Group Member Status", string.Join( ",", cblStatus.SelectedValues ) );
            LoadGroupMembers();
        }
        protected void gfGroupMember_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( SelectedGroup != null )
            {
                if ( e.Key == GetGroupFilterKey( "Gender" ) )
                {
                    var genders = e.Value.SplitDelimitedValues();
                    var sb = new System.Text.StringBuilder();
                    foreach ( var gender in genders )
                    {
                        if ( gender == "1" )
                        {
                            sb.Append( "Male, " );
                        }
                        else if ( gender == "2" )
                        {
                            sb.Append( "Female, " );
                        }
                    }

                    e.Value = sb.ToString().Substring( 0, sb.ToString().Length - 2 );

                }
                else if ( e.Key == GetGroupFilterKey( "Role" ) )
                {
                    var roleIds = e.Value.SplitDelimitedValues()
                        .Select( v => v.AsIntegerOrNull() )
                        .Where( v => v != null ).ToList();

                    var roleNames = GroupTypeCache.Get( SelectedGroup.GroupTypeId ).Roles
                        .Where( r => roleIds.Contains( r.Id ) )
                        .Select( r => r.Name )
                        .ToList();
                    e.Value = string.Join( ", ", roleNames );

                }
                else if ( e.Key == GetGroupFilterKey( "Status" ) )
                {
                    var statuses = e.Value.SplitDelimitedValues()
                        .Select( s => s.AsIntegerOrNull() )
                        .Where( s => s.HasValue )
                        .Select( s => ( ( GroupMemberStatus )s ).ToString() );

                    e.Value = string.Join( ", ", statuses );
                }
            }
        }

        protected void gfGroupMember_ClearFilterClick( object sender, EventArgs e )
        {
            gfGroupMember.DeleteUserPreferences();
            cblGender.SetValues( new List<string>() );
            cblGender.SetValues( new List<string>() );
            cblStatus.SetValues( new List<string>() { ( ( int ) GroupMemberStatus.Active ).ToString() } );
            LoadGroupMembers();
        }

        protected void lbCancelMemberUpdates_Click( object sender, EventArgs e )
        {
            ClearEditPanel();
            BlockSetup();
        }

        protected void lbSaveMemberUpdates_Click( object sender, EventArgs e )
        {
            var groupMemberId = hfGroupMemberId.Value.AsInteger();
            if ( groupMemberId <= 0 )
            {
                return;
            }

            using ( var gmUpdateContext = new RockContext() )
            {
                var gmService = new GroupMemberService( gmUpdateContext );
                var gmRoleService = new GroupTypeRoleService( gmUpdateContext );
                var groupService = new GroupService( gmUpdateContext );

                var groupMember = gmService.Get( groupMemberId );
                var groupMemberStatus = rblGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>();


                if ( groupMember == null )
                {
                    return;
                }

                // Verify valid group
                var group = groupService.Get( groupMember.GroupId );
                if ( group == null )
                {
                    return;
                }

                // Check to see if a person was selected
                int? personId = groupMember.PersonId;
                int? personAliasId = groupMember.Person.PrimaryAliasId;
                if ( !personId.HasValue || !personAliasId.HasValue )
                {
                    return;
                }

                // inactivate any deselected roles for person
                var removeGroupMembers = gmService.Queryable().Where( m =>
                                                     m.GroupId == groupMember.GroupId &&
                                                     m.PersonId == groupMember.PersonId &&
                                                     m.GroupRole.IsLeader == false &&
                                                     !cblGroupMemberRole.SelectedValuesAsInt.Contains( m.GroupRoleId ) );
                foreach ( var removeGroupMember in removeGroupMembers )
                {
                    removeGroupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                }
                gmUpdateContext.SaveChanges();

                // make sure the person is in a Member role if they're not already in a Member role or a role that includes "Prospect" (Prospect, Prospective Member) or "Guest"
                var selectedMemberRoles = cblGroupMemberRole.SelectedValuesAsInt;
                var memberRole = group.GroupType.Roles.Where( r => r.Name == "Member" ).FirstOrDefault();
                if ( memberRole != null )
                {
                    var isProspect = gmService.Queryable().Where( m =>
                                                     m.GroupId == groupMember.GroupId &&
                                                     m.PersonId == groupMember.PersonId &&
                                                     ( m.GroupRole.Name.Contains( "Prospect" ) || m.GroupRole.Name.Contains( "Guest" ) ) )
                                                .Any();
                    if ( !isProspect )
                    {
                        selectedMemberRoles.Add( memberRole.Id );
                    }
                }

                // if this group does scheduling and more than one role is selected, error out and force them to select only one role.
                if ( group.GroupType.IsSchedulingEnabled && selectedMemberRoles.Count > 1 )
                {
                    nbMemberRoleError.Visible = true;
                    nbMemberRoleError.Text = "<span style='color:red;'>Groups that do group scheduling do not support multiple roles for a group member. Please select only one role for each person in the group.</span>";
                    return;
                }

                // update each selected group member role based on the specified changes
                foreach ( var groupRoleId in selectedMemberRoles )
                {
                    GroupTypeRole groupRole = gmRoleService.Get( groupRoleId );
                    if ( groupRole != null && !groupRole.IsLeader )
                    {
                        GroupMember updateGroupMember = null;
                        updateGroupMember = gmService.Queryable( false, true ).Where( m =>
                                                      m.GroupId == groupMember.GroupId &&
                                                      m.PersonId == groupMember.PersonId &&
                                                      m.GroupRoleId == groupRoleId )
                                            .FirstOrDefault();

                        if ( updateGroupMember == null ) // In this case, we're adding the person to a new role
                        {
                            updateGroupMember = new GroupMember { Id = 0 };
                            updateGroupMember.GroupId = group.Id;
                            updateGroupMember.PersonId = personId.Value;
                            updateGroupMember.GroupRoleId = groupRole.Id;
                            updateGroupMember.IsNotified = groupMember.IsNotified;
                            updateGroupMember.CommunicationPreference = groupMember.CommunicationPreference;
                            gmService.Add( updateGroupMember );
                        }

                        updateGroupMember.Note = tbNote.Text;
                        updateGroupMember.CommunicationPreference = Enum.TryParse( rblCommunicationPreference.Text, out CommunicationType result ) ? result : CommunicationType.RecipientPreference;

                        if ( groupMemberStatus == GroupMemberStatus.Pending )
                        {
                            if ( groupRole.Name == "Member" || groupRole.Name == "Prospect" || groupRole.Name == "Guest" )
                            {
                                updateGroupMember.GroupMemberStatus = GroupMemberStatus.Pending;
                            }
                            else
                            {
                                updateGroupMember.GroupMemberStatus = GroupMemberStatus.Inactive; // Don't use Pending status for roles other than Member, Prospect, Guest
                            }
                        }
                        else
                        {
                            updateGroupMember.GroupMemberStatus = groupMemberStatus;
                        }

                        if ( pnlScheduling.Visible )
                        {
                            updateGroupMember.ScheduleTemplateId = ddlGroupMemberScheduleTemplate.SelectedValue.AsIntegerOrNull();
                            updateGroupMember.ScheduleStartDate = dpScheduleStartDate.SelectedDate;
                            updateGroupMember.ScheduleReminderEmailOffsetDays = nbScheduleReminderEmailOffsetDays.Text.AsIntegerOrNull();

                            var groupMemberAssignmentService = new GroupMemberAssignmentService( gmUpdateContext );
                            var groupLocationService = new GroupLocationService( gmUpdateContext );
                            var qryGroupLocations = groupLocationService
                                .Queryable()
                                .Where( g => g.GroupId == group.Id );

                            var uiGroupMemberAssignments = Assignments.Select( r => r.AssignmentGuid );
                            foreach ( var groupMemberAssignment in groupMember.GroupMemberAssignments.Where( r => !uiGroupMemberAssignments.Contains( r.Guid ) && (
                                        !r.LocationId.HasValue
                                        || qryGroupLocations.Any( gl => gl.LocationId == r.LocationId && gl.Schedules.Any( s => s.Id == r.ScheduleId ) )
                                    ) ).ToList() )
                            {
                                groupMember.GroupMemberAssignments.Remove( groupMemberAssignment );
                                groupMemberAssignmentService.Delete( groupMemberAssignment );
                            }

                            foreach ( var entry in Assignments )
                            {
                                GroupMemberAssignment groupMemberAssignment = groupMember.GroupMemberAssignments.Where( a => a.Guid == entry.AssignmentGuid ).FirstOrDefault();
                                if ( groupMemberAssignment == null )
                                {
                                    groupMemberAssignment = new GroupMemberAssignment();
                                    groupMember.GroupMemberAssignments.Add( groupMemberAssignment );
                                }

                                groupMemberAssignment.ScheduleId = entry.ScheduleId;
                                groupMemberAssignment.LocationId = entry.LocationId;
                            }
                        }

                        gmService.Restore( updateGroupMember ); // this ensures the group member is no longer archived
                        gmUpdateContext.SaveChanges();
                    }
                }
            }

            Assignments = new List<Assignment>();
            ClearEditPanel();
            BlockSetup();
        }

        protected void rptGroupMember_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( !AllowMemberEdit )
            {
                return;
            }
            var command = e.CommandName;
            var groupMemberId = e.CommandArgument.ToString().AsIntegerOrNull();

            if ( command == "EditMember" && groupMemberId.HasValue )
            {
                EditGroupMember( groupMemberId.Value );
            }
        }

        protected void rptGroupMember_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var gmGrouping = e.Item.DataItem as IGrouping<int, GroupMember>;
            var firstGm = gmGrouping.First();
            var gmGroupingCount = gmGrouping.Count();
            gmCount += 1;

            if ( firstGm == null || firstGm.Id <= 0 )
            {
                return;
            }

            var person = firstGm.Person;

            var hfGroupMemberId = e.Item.FindControl( "hfGroupMemberId" ) as HiddenField;

            var lAddress = e.Item.FindControl( "lAddress" ) as Literal;
            var lBirthdate = e.Item.FindControl( "lBirthDate" ) as Literal;
            var lEmail = e.Item.FindControl( "lEmail" ) as Literal;
            var lFirstAttended = e.Item.FindControl( "lFirstAttended" ) as Literal;
            var lLastAttended = e.Item.FindControl( "lLastAttended" ) as Literal;
            var lCommunicationPreference = e.Item.FindControl( "lCommunicationPreference" ) as Literal;
            var lScheduleTemplate = e.Item.FindControl( "lScheduleTemplate" ) as Literal;
            var lScheduleStart = e.Item.FindControl( "lScheduleStart" ) as Literal;
            var lNote = e.Item.FindControl( "lNote" ) as Literal;
            var lPhones = e.Item.FindControl( "lPhones" ) as Literal;
            var lRole = e.Item.FindControl( "lRole" ) as Literal;
            var lRowHeader = e.Item.FindControl( "lRowHeader" ) as Literal;
            var lRowFooter = e.Item.FindControl( "lRowFooter" ) as Literal;
            var lBaptismDate = e.Item.FindControl( "lBaptismDate" ) as Literal;
            var lSchool = e.Item.FindControl( "lSchool" ) as Literal;
            var lParent1Name = e.Item.FindControl( "lParent1Name" ) as Literal;
            var lParent1Mobile = e.Item.FindControl( "lParent1Mobile" ) as Literal;
            var lParent1Email = e.Item.FindControl( "lParent1Email" ) as Literal;
            var lParent2Name = e.Item.FindControl( "lParent2Name" ) as Literal;
            var lParent2Mobile = e.Item.FindControl( "lParent2Mobile" ) as Literal;
            var lParent2Email = e.Item.FindControl( "lParent2Email" ) as Literal;
            var lParent3Name = e.Item.FindControl( "lParent3Name" ) as Literal;
            var lParent3Mobile = e.Item.FindControl( "lParent3Mobile" ) as Literal;
            var lParent3Email = e.Item.FindControl( "lParent3Email" ) as Literal;
            var lParent4Name = e.Item.FindControl( "lParent4Name" ) as Literal;
            var lParent4Mobile = e.Item.FindControl( "lParent4Mobile" ) as Literal;
            var lParent4Email = e.Item.FindControl( "lParent4Email" ) as Literal;

            var lbEditMember = e.Item.FindControl( "lbEditMember" ) as LinkButton;

            var pnlBirthdate = e.Item.FindControl( "pnlBirthDate" ) as Panel;
            var pnlAddress = e.Item.FindControl( "pnlAddress" ) as Panel;
            var pnlEmail = e.Item.FindControl( "pnlEmail" ) as Panel;
            var pnlFirstAttended = e.Item.FindControl( "pnlFirstAttended" ) as Panel;
            var pnlLastAttended = e.Item.FindControl( "pnlLastAttended" ) as Panel;
            var pnlCommunicationPreference = e.Item.FindControl( "pnlCommunicationPreference" ) as Panel;
            var pnlScheduleTemplate = e.Item.FindControl( "pnlScheduleTemplate" ) as Panel;
            var pnlScheduleStart = e.Item.FindControl( "pnlScheduleStart" ) as Panel;
            var pnlNote = e.Item.FindControl( "pnlNote" ) as Panel;
            var pnlPhones = e.Item.FindControl( "pnlPhones" ) as Panel;
            var pnlBaptismDate = e.Item.FindControl( "pnlBaptism" ) as Panel;
            var pnlSchool = e.Item.FindControl( "pnlSchool" ) as Panel;
            var pnlParent1 = e.Item.FindControl( "pnlParent1" ) as Panel;
            var pnlParent1Name = e.Item.FindControl( "pnlParent1Name" ) as Panel;
            var pnlParent1Mobile = e.Item.FindControl( "pnlParent1Mobile" ) as Panel;
            var pnlParent1Email = e.Item.FindControl( "pnlParent1Email" ) as Panel;
            var pnlParent2 = e.Item.FindControl( "pnlParent2" ) as Panel;
            var pnlParent2Name = e.Item.FindControl( "pnlParent2Name" ) as Panel;
            var pnlParent2Mobile = e.Item.FindControl( "pnlParent2Mobile" ) as Panel;
            var pnlParent2Email = e.Item.FindControl( "pnlParent2Email" ) as Panel;
            var pnlParent3 = e.Item.FindControl( "pnlParent3" ) as Panel;
            var pnlParent3Name = e.Item.FindControl( "pnlParent3Name" ) as Panel;
            var pnlParent3Mobile = e.Item.FindControl( "pnlParent3Mobile" ) as Panel;
            var pnlParent3Email = e.Item.FindControl( "pnlParent3Email" ) as Panel;
            var pnlParent4 = e.Item.FindControl( "pnlParent4" ) as Panel;
            var pnlParent4Name = e.Item.FindControl( "pnlParent4Name" ) as Panel;
            var pnlParent4Mobile = e.Item.FindControl( "pnlParent4Mobile" ) as Panel;
            var pnlParent4Email = e.Item.FindControl( "pnlParent4Email" ) as Panel;

            pnlAddress.Visible = false;
            pnlBirthdate.Visible = false;
            pnlEmail.Visible = false;
            pnlFirstAttended.Visible = false;
            pnlLastAttended.Visible = false;
            pnlPhones.Visible = false;
            pnlBaptismDate.Visible = false;
            pnlSchool.Visible = false;
            pnlParent1.Visible = false;
            pnlParent1Name.Visible = false;
            pnlParent1Mobile.Visible = false;
            pnlParent1Email.Visible = false;
            pnlParent2Name.Visible = false;
            pnlParent2Mobile.Visible = false;
            pnlParent2Email.Visible = false;
            pnlParent3Name.Visible = false;
            pnlParent3Mobile.Visible = false;
            pnlParent3Email.Visible = false;
            pnlParent4Name.Visible = false;
            pnlParent4Mobile.Visible = false;
            pnlParent4Email.Visible = false;

            var cbGroupMember = e.Item.FindControl( "cbGroupMember" ) as RockCheckBox;

            if ( gmCount % 2 == 1 )
            {
                lRowHeader.Text = "<div class=\"row\">";
            }
            else
            {
                lRowFooter.Text = "</div>";
            }
            hfGroupMemberId.Value = firstGm.Id.ToString();

            System.Text.StringBuilder sbBadges = new System.Text.StringBuilder();

            foreach ( var gm in gmGrouping )
            {
                var roleString = string.Format( " ({0})", gm.GroupRole.Name );
                if ( gm.GroupMemberStatus == GroupMemberStatus.Inactive )
                {
                    sbBadges.AppendFormat( "<span class=\"badge badge-danger\">Inactive{0}</span>", gmGroupingCount > 1 ? roleString : "" );
                }
            }

            if ( person.BirthDate.HasValue )
            {
                DateTime birthDateTest;
                if ( person.BirthMonth.Value == 2 && person.BirthDay == 29 && !DateTime.IsLeapYear( RockDateTime.Today.Year ) )
                {
                    birthDateTest = person.BirthDate.Value.AddDays( -1 );
                }
                else
                {
                    birthDateTest = person.BirthDate.Value;
                }

                var birthdaySundayDate = new DateTime( RockDateTime.Today.Year, birthDateTest.Month, birthDateTest.Day ).SundayDate();
                var thisSunday = RockDateTime.Today.SundayDate();
                var nextSunday = RockDateTime.Today.SundayDate().AddDays( 7 );

                if ( birthdaySundayDate == thisSunday || birthdaySundayDate == nextSunday )
                {
                    sbBadges.Append( "&nbsp;<i class=\"far fa-birthday-cake\"></i>" );
                }
            }

            var pnlImage = e.Item.FindControl( "pnlImage" ) as Panel;
            if ( ShowPhoto )
            {
                pnlImage.Visible = true;
                var imgMember = e.Item.FindControl( "imgMember" ) as Image;
                imgMember.ImageUrl = Person.GetPersonPhotoUrl( person.Id, maxHeight: 60 );
            }
            else
            {
                pnlImage.Visible = false;
            }

            var lName = e.Item.FindControl( "lName" ) as Literal;
            lName.Text = person.FullName;
            if ( gmGrouping.Where( m => m.GroupMemberStatus == GroupMemberStatus.Pending ).Any() )
            {
                lName.Text = $"{person.FullName}<br>(Pending)";
            }

            var lbadges = e.Item.FindControl( "lBadges" ) as Literal;
            lbadges.Text = sbBadges.ToString();

            lRole.Text = gmGrouping.Select( gm => gm.GroupRole.Name ).JoinStrings( ", " );

            lbEditMember.Visible = AllowMemberEdit;
            lbEditMember.CommandArgument = firstGm.Id.ToString();

            if ( ShowBirthDate )
            {
                if ( person.BirthDate.HasValue )
                {
                    lBirthdate.Text = person.BirthDate.ToShortDateString();
                    pnlBirthdate.Visible = true;
                }
            }

            if ( ShowEmailAddress )
            {
                if ( person.Email.IsNotNullOrWhiteSpace() )
                {
                    lEmail.Text = person.Email;
                    pnlEmail.Visible = true;
                }
            }

            if ( ShowAddress )
            {
                var address = GetFormattedHomeAddress( person );

                if ( address.IsNotNullOrWhiteSpace() )
                {
                    lAddress.Text = address;
                    pnlAddress.Visible = true;
                }
            }

            var phonesVisible = PhoneTypes.Count > 0;

            if ( phonesVisible )
            {
                var phones = GetPhoneList( person.PhoneNumbers.ToList() );
                if ( phones.IsNotNullOrWhiteSpace() )
                {
                    lPhones.Text = phones;
                    pnlPhones.Visible = true;
                }
            }

            if ( ShowFirstAttended )
            {
                var firstAttended = GetFirstAttended( firstGm );
                if ( firstAttended > DateTime.MinValue )
                {
                    lFirstAttended.Text = firstAttended.ToShortDateString();
                    pnlFirstAttended.Visible = true;
                }
            }

            if ( ShowLastAttended )
            {
                var lastAttended = GetLastAttended( firstGm );
                if ( lastAttended > DateTime.MinValue )
                {
                    lLastAttended.Text = lastAttended.ToShortDateString();
                    pnlLastAttended.Visible = true;
                }
            }

            if ( ShowCommonProperties )
            {
                string communicationPreference = firstGm.CommunicationPreference.ToString();
                if ( communicationPreference != "" )
                {
                    if ( communicationPreference == "RecipientPreference" )
                    {
                        communicationPreference = firstGm.Person.CommunicationPreference.ToString();
                    }
                    lCommunicationPreference.Text = communicationPreference;
                    pnlCommunicationPreference.Visible = true;
                }

                string note = firstGm.Note;
                if ( note != null && note != "" )
                {
                    lNote.Text = note;
                    pnlNote.Visible = true;
                }

                if ( firstGm.ScheduleTemplate != null && firstGm.ScheduleStartDate != null )
                {
                    string scheduleTemplate = firstGm.ScheduleTemplate.Name;
                    lScheduleTemplate.Text = scheduleTemplate;
                    pnlScheduleTemplate.Visible = true;

                    var scheduleStartDate = firstGm.ScheduleStartDate;
                    if ( scheduleStartDate > DateTime.MinValue )
                    {
                        lScheduleStart.Text = scheduleStartDate.ToShortDateString();
                        pnlScheduleStart.Visible = true;
                    }
                }
            }

            if ( !AllowCommunications || String.IsNullOrWhiteSpace( CommunicationPageRoute ) )
            {
                cbGroupMember.Visible = false;
            }
            else
            {
                cbGroupMember.Visible = true;
            }

            var isLeader = gmGrouping.Where( gm => gm.GroupRole.IsLeader ).Any();
            if ( _featureSet.DisplayParentInfo && !isLeader )
            {
                person.LoadAttributes();

                pnlBaptismDate.Visible = true;
                lBaptismDate.Text = person.GetAttributeValue( "Arena-15-73" ).AsDateTime().ToShortDateString();

                pnlSchool.Visible = true;
                lSchool.Text = person.GetAttributeValue( "SchoolRegistration" );

                var groupMemberService = new GroupMemberService( _rockContext ).Queryable().AsNoTracking();
                var familyIds = groupMemberService
                    .Where( gm => gm.PersonId == person.Id && gm.Group.GroupTypeId == 10 && gm.IsArchived == false )
                    .Select( gm => gm.GroupId )
                    .ToList();
                var parentIds = groupMemberService
                    .Where( gm => familyIds.Contains( gm.GroupId ) && gm.GroupRoleId == 3 )
                    .Select( gm => gm.PersonId )
                    .ToList();
                var parents = new PersonService( _rockContext ).Queryable().AsNoTracking()
                    .Where( p => parentIds.Contains( p.Id ) && p.IsDeceased == false )
                    .OrderBy( p => p.LastName )
                    .ThenBy( p => p.BirthDate )
                    .ToList();

                if ( parents.Count > 0 )
                {
                    pnlParent1.Visible = true;
                    pnlParent1Name.Visible = true;
                    lParent1Name.Text = parents[0].FullName;

                    pnlParent1Mobile.Visible = true;
                    lParent1Mobile.Text = parents[0].PhoneNumbers.Where( pn => pn.IsUnlisted == false && pn.NumberTypeValueId == 12 ).FirstOrDefault()?.NumberFormatted;

                    pnlParent1Email.Visible = true;
                    lParent1Email.Text = parents[0].Email;
                }

                if ( parents.Count > 1 )
                {
                    pnlParent2.Visible = true;
                    pnlParent2Name.Visible = true;
                    lParent2Name.Text = parents[1].FullName;

                    pnlParent2Mobile.Visible = true;
                    lParent2Mobile.Text = parents[1].PhoneNumbers.Where( pn => pn.IsUnlisted == false && pn.NumberTypeValueId == 12 ).FirstOrDefault()?.NumberFormatted;

                    pnlParent2Email.Visible = true;
                    lParent2Email.Text = parents[1].Email;
                }

                if ( parents.Count > 2 )
                {
                    pnlParent3.Visible = true;
                    pnlParent3Name.Visible = true;
                    lParent3Name.Text = parents[2].FullName;

                    pnlParent3Mobile.Visible = true;
                    lParent3Mobile.Text = parents[2].PhoneNumbers.Where( pn => pn.IsUnlisted == false && pn.NumberTypeValueId == 12 ).FirstOrDefault()?.NumberFormatted;

                    pnlParent3Email.Visible = true;
                    lParent3Email.Text = parents[2].Email;
                }

                if ( parents.Count > 3 )
                {
                    pnlParent4.Visible = true;
                    pnlParent4Name.Visible = true;
                    lParent4Name.Text = parents[3].FullName;

                    pnlParent4Mobile.Visible = true;
                    lParent4Mobile.Text = parents[3].PhoneNumbers.Where( pn => pn.IsUnlisted == false && pn.NumberTypeValueId == 12 ).FirstOrDefault()?.NumberFormatted;

                    pnlParent4Email.Visible = true;
                    lParent4Email.Text = parents[3].Email;
                }
            }
        }

        protected void lbSendParentCommunication_Click( object sender, EventArgs e )
        {
            if ( SelectedGroup != null )
            {
                if ( String.IsNullOrWhiteSpace( CommunicationPageRoute ) || GroupId <= 0 || CurrentPerson == null )
                {
                    return;
                }

                List<int> groupMemberIds = new List<int>();

                foreach ( RepeaterItem item in rptGroupMember.Items )
                {
                    var cbGroupMember = item.FindControl( "cbGroupMember" ) as RockCheckBox;
                    var hfGroupMemberId = item.FindControl( "hfGroupMemberId" ) as HiddenField;

                    if ( cbGroupMember.Checked )
                    {
                        groupMemberIds.Add( hfGroupMemberId.ValueAsInt() );
                    }

                }

                // Get parents of students in the group
                var groupMemberService = new GroupMemberService( _rockContext ).Queryable().AsNoTracking();
                var groupMemberQuery = GetGroupMemberQuery();
                var memberIds = groupMemberQuery
                    .Where( gm => gm.GroupRoleId == SelectedGroup.GroupType.DefaultGroupRoleId ) // intended to limit selection to only students, not leaders
                    .Select( gm => gm.PersonId ).ToList();
                var familyIds = groupMemberService
                    .Where( gm => memberIds.Contains( gm.PersonId ) && gm.Group.GroupTypeId == 10 && gm.IsArchived == false )
                    .Select( gm => gm.GroupId )
                    .ToList();
                var parentIds = groupMemberService
                    .Where( gm => familyIds.Contains( gm.GroupId ) && gm.GroupRoleId == 3 )
                    .Select( gm => gm.PersonId )
                    .ToList();
                var parents = new PersonService( _rockContext ).Queryable().AsNoTracking()
                    .Where( p => parentIds.Contains( p.Id ) )
                    .ToList();
                var parentAliasIds = parents.Select( p => p.PrimaryAliasId ).ToList();

                // Then add leaders in the group (not their parents)
                var leaderRoleIds = new GroupTypeRoleService( _rockContext ).Queryable().AsNoTracking()
                    .Where( gtr => gtr.GroupTypeId == SelectedGroup.GroupTypeId && gtr.IsLeader )
                    .Select( gtr => gtr.Id )
                    .ToList();
                var leaderAliasIds = groupMemberQuery
                    .Where( gm => leaderRoleIds.Contains( gm.GroupRoleId ) )
                    .Select( gm => gm.Person.PrimaryAliasId )
                    .ToList();
                parentAliasIds.AddRange( leaderAliasIds );

                using ( var commContext = new RockContext() )
                {
                    var communicationSvc = new CommunicationService( commContext );
                    var communication = new Communication();
                    communication.IsBulkCommunication = false;
                    communication.Status = CommunicationStatus.Transient;

                    communication.SenderPersonAliasId = CurrentPersonAliasId;
                    communicationSvc.Add( communication );

                    foreach ( var id in parentAliasIds )
                    {
                        var recipient = new CommunicationRecipient();
                        recipient.PersonAliasId = id.Value;
                        communication.Recipients.Add( recipient );
                    }

                    commContext.SaveChanges();

                    Dictionary<string, string> queryParams = new Dictionary<string, string>();
                    queryParams.Add( "CommunicationId", communication.Id.ToString() );
                    NavigateToLinkedPage( AttributeKey.CommunicationsPage, queryParams );
                }
            }
        }

        protected void lbSendCommunication_Click( object sender, EventArgs e )
        {
            if ( SelectedGroup != null )
            {
                if ( String.IsNullOrWhiteSpace( CommunicationPageRoute ) || GroupId <= 0 || CurrentPerson == null )
                {
                    return;
                }

                List<int> groupMemberIds = new List<int>();

                foreach ( RepeaterItem item in rptGroupMember.Items )
                {
                    var cbGroupMember = item.FindControl( "cbGroupMember" ) as RockCheckBox;
                    var hfGroupMemberId = item.FindControl( "hfGroupMemberId" ) as HiddenField;

                    if ( cbGroupMember.Checked )
                    {
                        groupMemberIds.Add( hfGroupMemberId.ValueAsInt() );
                    }

                }

                var gmQuery = GetGroupMemberQuery();

                if ( groupMemberIds.Count > 0 )
                {
                    gmQuery = gmQuery.Where( gm => groupMemberIds.Contains( gm.Id ) );
                }

                var personAliasIds = gmQuery.Select( gm => gm.Person.PrimaryAliasId ).ToList();

                using ( var commContext = new RockContext() )
                {
                    var communicationSvc = new CommunicationService( commContext );
                    var communication = new Communication();
                    communication.IsBulkCommunication = false;
                    communication.Status = CommunicationStatus.Transient;

                    communication.SenderPersonAliasId = CurrentPersonAliasId;
                    communicationSvc.Add( communication );

                    foreach ( var id in personAliasIds )
                    {
                        var recipient = new CommunicationRecipient();
                        recipient.PersonAliasId = id.Value;
                        communication.Recipients.Add( recipient );
                    }

                    commContext.SaveChanges();

                    Dictionary<string, string> queryParams = new Dictionary<string, string>();
                    queryParams.Add( "CommunicationId", communication.Id.ToString() );
                    NavigateToLinkedPage( AttributeKey.CommunicationsPage, queryParams );
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupMemberScheduleTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupMemberScheduleTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            dpScheduleStartDate.Required = ddlGroupMemberScheduleTemplate.SelectedValue.AsIntegerOrNull().HasValue;
        }

        protected void gGroupPreferenceAssignments_Add( object sender, EventArgs e )
        {
            gGroupPreferenceAssignments_ShowEdit( Guid.Empty );
        }

        protected void gGroupPreferenceAssignments_Edit( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            gGroupPreferenceAssignments_ShowEdit( rowGuid );
        }

        protected void gGroupPreferenceAssignments_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;

            var entry = Assignments.Where( a => a.AssignmentGuid == rowGuid ).FirstOrDefault();
            if ( entry != null )
            {
                Assignments.Remove( entry );
            }

            BindGroupPreferenceAssignmentsGrid();
        }

        protected void gGroupPreferenceAssignments_GridRebind( object sender, EventArgs e )
        {
            BindGroupPreferenceAssignmentsGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupScheduleAssignmentSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupScheduleAssignmentSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            PopulateGroupScheduleAssignmentLocations( GroupId, ddlGroupScheduleAssignmentSchedule.SelectedValue.AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdGroupScheduleAssignment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdGroupScheduleAssignment_SaveClick( object sender, EventArgs e )
        {
            var groupMemberAssignmentGuid = hfGroupScheduleAssignmentGuid.Value.AsGuid();
            var scheduleId = ddlGroupScheduleAssignmentSchedule.SelectedValue.AsIntegerOrNull();
            var locationId = ddlGroupScheduleAssignmentLocation.SelectedValue.AsIntegerOrNull();

            // schedule is required, but locationId can be null (which means no location specified )
            if ( !scheduleId.HasValue )
            {
                return;
            }

            var schedule = new ScheduleService( new RockContext() ).Get( scheduleId.Value );
            if ( schedule == null )
            {
                return;
            }

            var groupMemberAssignment = Assignments.Where( w => w.AssignmentGuid.Equals( groupMemberAssignmentGuid ) && !groupMemberAssignmentGuid.Equals( Guid.Empty ) ).FirstOrDefault();
            if ( groupMemberAssignment == null )
            {
                groupMemberAssignment = new Assignment();
                groupMemberAssignment.AssignmentGuid = Guid.NewGuid();
                Assignments.Add( groupMemberAssignment );
            }

            // Calculate the Next Start Date Time based on the start of the week so that schedule columns are in the correct order
            var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );
            groupMemberAssignment.ScheduleId = scheduleId.Value;
            groupMemberAssignment.FormattedScheduleName = schedule.StartTimeOfDay.ToTimeString();
            groupMemberAssignment.LocationId = locationId;
            groupMemberAssignment.LocationName = ddlGroupScheduleAssignmentLocation.SelectedItem.Text;

            BindGroupPreferenceAssignmentsGrid();
            mdGroupScheduleAssignment.Hide();
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Binds the group preference assignments grid.
        /// </summary>
        private void BindGroupPreferenceAssignmentsGrid()
        {
            gGroupPreferenceAssignments.DataSource = Assignments;
            gGroupPreferenceAssignments.DataBind();
            upGroupRoster.Update();
        }

        /// <summary>
        /// gs the statuses_ show edit.
        /// </summary>
        /// <param name="groupPreferenceAssignmentGuid">The group preference assignment status unique identifier.</param>
        protected void gGroupPreferenceAssignments_ShowEdit( Guid guid )
        {
            int? selectedScheduleId = null;
            int? selectedLocationId = null;
            var entry = Assignments.Where( a => a.AssignmentGuid == guid ).FirstOrDefault();
            if ( entry != null )
            {
                selectedScheduleId = entry.ScheduleId;
                selectedLocationId = entry.LocationId;
            }

            hfGroupScheduleAssignmentGuid.Value = guid.ToString();
            var rockContext = new RockContext();
            var groupLocationService = new GroupLocationService( rockContext );
            var scheduleList = groupLocationService
                .Queryable()
                .AsNoTracking()
                .Where( g => g.GroupId == GroupId )
                .SelectMany( g => g.Schedules )
                .Distinct()
                .ToList();

            List<Schedule> sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();

            var configuredScheduleIds = Assignments
                .Select( s => s.ScheduleId ).Distinct().ToList();

            // limit to schedules that haven't had a schedule preference set yet
            sortedScheduleList = sortedScheduleList.Where( a =>
                a.IsActive
                && a.IsPublic.HasValue
                && a.IsPublic.Value
                && ( !configuredScheduleIds.Contains( a.Id )
                || ( selectedScheduleId.HasValue
                    && a.Id == selectedScheduleId.Value ) ) )
             .ToList();

            ddlGroupScheduleAssignmentSchedule.Items.Clear();
            ddlGroupScheduleAssignmentSchedule.Items.Add( new ListItem() );
            foreach ( var schedule in sortedScheduleList )
            {
                var scheduleName = schedule.StartTimeOfDay.ToTimeString();
                var scheduleListItem = new ListItem( scheduleName, schedule.Id.ToString() );
                if ( selectedScheduleId.HasValue && selectedScheduleId.Value == schedule.Id )
                {
                    scheduleListItem.Selected = true;
                }

                ddlGroupScheduleAssignmentSchedule.Items.Add( scheduleListItem );
            }

            PopulateGroupScheduleAssignmentLocations( GroupId, selectedScheduleId );
            ddlGroupScheduleAssignmentLocation.SetValue( selectedLocationId );

            mdGroupScheduleAssignment.Show();
        }

        /// <summary>
        /// Populates the group schedule assignment locations.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        private void PopulateGroupScheduleAssignmentLocations( int groupId, int? scheduleId )
        {
            int? selectedLocationId = ddlGroupScheduleAssignmentLocation.SelectedValue.AsIntegerOrNull();
            ddlGroupScheduleAssignmentLocation.Items.Clear();
            ddlGroupScheduleAssignmentLocation.Items.Add( new ListItem( "No Location Preference", "No Location Preference" ) );
            if ( scheduleId.HasValue )
            {
                var locations = new LocationService( new RockContext() ).GetByGroupSchedule( scheduleId.Value, groupId )
                    .OrderBy( a => a.Name )
                    .Select( a => new
                    {
                        a.Id,
                        a.Name
                    } ).ToList();

                foreach ( var location in locations )
                {
                    var locationListItem = new ListItem( location.Name, location.Id.ToString() );
                    if ( selectedLocationId.HasValue && location.Id == selectedLocationId.Value )
                    {
                        locationListItem.Selected = true;
                    }

                    ddlGroupScheduleAssignmentLocation.Items.Add( locationListItem );
                }
            }
        }

        private void BlockSetup()
        {
            pnlGroupMembers.Visible = false;
            lbSendParentCommunication.Visible = false;

            if ( SelectedGroup == null || SelectedGroup.Id <= 0 )
            {
                UpdateNotification( "Unable to View Group", "The selected group is not currently accessible.", NotificationBoxType.Warning );
                return;
            }
            if ( !SelectedGroup.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                UpdateNotification( "Unable to View Group",
                    "<p>You do not have permission to view this group. If you received this in error please contact your ministry leader or coach.",
                    NotificationBoxType.Danger );

                return;
            }

            LoadFilters();
            LoadGroupMembers();
            LoadGroupMemberRoleList();
            ConfigureCommands();

            pnlGroupMembers.Visible = true;
            pnlEditGroupMember.Visible = false;
        }

        private string GetFeatureSetStrings()
        {
            List<String> result = new List<String>();

            var context = new RockContext();
            var groupMemberService = new GroupMemberService( context );

            // get features that might be enabled because the current person is a member of this group
            foreach (var gm in  groupMemberService.Queryable().AsNoTracking().Where( gm => gm.GroupId == SelectedGroup.Id && gm.PersonId == CurrentPerson.Id ) )
            {
                var role = gm.GroupRole;
                role.LoadAttributes();
                result.Add( role.AttributeValues[ "GroupLeaderToolboxFeatureSet" ]?.Value ?? String.Empty );
            }

            // get features that might be enabled because the current person is a member of a coach group
            foreach ( var gm in groupMemberService.Queryable().AsNoTracking().Where( gm => gm.GroupId == SelectedGroup.ParentGroupId && gm.PersonId == CurrentPerson.Id ) )
            {
                var role = gm.GroupRole;
                role.LoadAttributes();
                result.Add( role.AttributeValues["GroupLeaderToolboxFeatureSet"]?.Value ?? String.Empty );
            }

            // get features that might be enabled because the current person is a member of a captain group
            foreach ( var gm in groupMemberService.Queryable().AsNoTracking().Where( gm => gm.GroupId == SelectedGroup.ParentGroup.ParentGroupId && gm.PersonId == CurrentPerson.Id ) )
            {
                var role = gm.GroupRole;
                role.LoadAttributes();
                result.Add( role.AttributeValues["GroupLeaderToolboxFeatureSet"]?.Value ?? String.Empty );
            }

            return result.JoinStrings( "," );
        }

        private void ClearEditPanel()
        {
            nbRoleNotEditable.Visible = false;
            hfGroupMemberId.Value = string.Empty;
            imgEditPhoto.ImageUrl = string.Empty;
            lName.Text = string.Empty;
            lGroupMemberRole.Text = string.Empty;
            cblGroupMemberRole.SelectedValue = null;
        }

        private void ConfigureCommands()
        {
            pnlCommandsTop.Visible = false;
            lbSendParentCommunication.Visible = false;
            lbSendCommunicationTop.Visible = false;

            if ( AllowCommunications && CommunicationPageRoute.IsNotNullOrWhiteSpace() && _featureSet.GroupCommunication )
            {
                pnlCommandsTop.Visible = true;
                lbSendParentCommunication.Visible = _featureSet.DisplayParentInfo;
                lbSendCommunicationTop.Visible = !_featureSet.DisplayParentInfo; // display one or the other; if we're displaying parent info it implies these are students and leaders should not email students
            }
        }

        private void EditGroupMember( int groupMemberId )
        {
            cblGroupMemberRole.Visible = false;

            lGroupMemberRole.Visible = false;

            var groupMemberService = new GroupMemberService( _rockContext );
            var groupMember = groupMemberService.Queryable( "Person.PhoneNumbers,GroupRole" )
                .Where( m => m.Id == groupMemberId )
                .FirstOrDefault();

            if ( groupMember == null )
            {
                return;
            }

            hfGroupMemberId.Value = groupMember.Id.ToString();

            var groupMembers = groupMemberService.Queryable( "Person.PhoneNumbers,GroupRole" )
                 .Where( m => m.GroupId == groupMember.GroupId && m.PersonId == groupMember.PersonId && m.GroupMemberStatus == groupMember.GroupMemberStatus && m.IsArchived == groupMember.IsArchived )
                 .ToList();

            GroupMemberStatus groupMemberStatus = GroupMemberStatus.Inactive;
            if ( groupMembers.Any( gm => gm.GroupMemberStatus == GroupMemberStatus.Active ) )
            {
                groupMemberStatus = GroupMemberStatus.Active;
            }
            else if ( groupMembers.Any( gm => gm.GroupMemberStatus == GroupMemberStatus.Pending ) )
            {
                groupMemberStatus = GroupMemberStatus.Pending;
            }
            rblGroupMemberStatus.BindToEnum<GroupMemberStatus>();
            rblGroupMemberStatus.SetValue( ( int ) groupMemberStatus );

            if ( ShowPhoto )
            {
                pnlEditPhoto.Visible = true;
                pnlEditPhoto.CssClass = "col-md-2 col-sm-4";
                imgEditPhoto.ImageUrl = Person.GetPersonPhotoUrl( groupMember.PersonId, maxHeight: 100 );

                pnlEditTitle.CssClass = pnlEditTitle.CssClass + " col-md-10 col-sm-8";

            }
            else
            {
                pnlEditPhoto.Visible = false;
                pnlEditTitle.CssClass = pnlEditTitle.CssClass + " col-xs-12";
            }
            lName.Text = groupMember.Person.FullName;

            var coachGuid = "b54d47f1-ba15-4158-a86f-5a3a35923ee3".AsGuid();
            var leaderRoleMember = groupMembers.Where( gm => gm.GroupRole.IsLeader || gm.GroupRole.Guid.Equals( coachGuid ) ).FirstOrDefault();
            if ( leaderRoleMember != null )
            {
                nbRoleNotEditable.Visible = true;
                cblGroupMemberRole.Required = false;
            }
            else
            {
                cblGroupMemberRole.Required = true;
            }

            cblGroupMemberRole.SetValues( groupMembers.Select( gm => gm.GroupRoleId.ToString() ).ToList() );
            cblGroupMemberRole.Visible = true;

            tbNote.Text = groupMember.Note;

            rblCommunicationPreference.SetValue( ( ( int ) groupMember.CommunicationPreference ).ToString() );

            ddlGroupMemberScheduleTemplate.Items.Clear();
            ddlGroupMemberScheduleTemplate.Items.Add( new ListItem() );

            RockContext rockContext = new RockContext();
            var groupMemberScheduleTemplateList = new GroupMemberScheduleTemplateService( rockContext ).Queryable()
                .Where( a => !a.GroupTypeId.HasValue || a.GroupTypeId == groupMember.Group.TypeId )
                .OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } );

            foreach ( var groupMemberScheduleTemplate in groupMemberScheduleTemplateList )
            {
                ddlGroupMemberScheduleTemplate.Items.Add( new ListItem( groupMemberScheduleTemplate.Name, groupMemberScheduleTemplate.Id.ToString() ) );
            }

            pnlScheduling.Visible = groupMember.Group.GroupType.IsSchedulingEnabled;
            ddlGroupMemberScheduleTemplate.SetValue( groupMember.ScheduleTemplateId );
            ddlGroupMemberScheduleTemplate_SelectedIndexChanged( null, null );

            dpScheduleStartDate.SelectedDate = groupMember.ScheduleStartDate;
            nbScheduleReminderEmailOffsetDays.Text = groupMember.ScheduleReminderEmailOffsetDays.ToString();

            // Get group member assignments
            var groupLocationService = new GroupLocationService( rockContext );
            var qryGroupLocations = groupLocationService
                .Queryable()
                .Where( g => g.GroupId == groupMember.GroupId );

            var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            var groupMemberAssignmentQuery = groupMemberAssignmentService
                .Queryable()
                .AsNoTracking()
                .Where( x =>
                    x.GroupMemberId == groupMemberId
                    && (
                        !x.LocationId.HasValue
                        || qryGroupLocations.Any( gl => gl.LocationId == x.LocationId && gl.Schedules.Any( s => s.Id == x.ScheduleId ) )
                    ) );

            Assignments = groupMemberAssignmentQuery.ToList().Select( a => new Assignment( a ) ).ToList();

            BindGroupPreferenceAssignmentsGrid();

            pnlGroupMembers.Visible = false;
            pnlEditGroupMember.Visible = true;
        }

        private DateTime GetFirstAttended( GroupMember gm )
        {
            return new AttendanceService( _rockContext ).Queryable().AsNoTracking()
                .Where( a => a.Occurrence.GroupId == gm.GroupId )
                .Where( a => a.PersonAlias.PersonId == gm.PersonId )
                .Where( a => a.DidAttend == true )
                .OrderBy( a => a.StartDateTime )
                .Select( a => a.StartDateTime )
                .FirstOrDefault();

        }

        private DateTime GetLastAttended( GroupMember gm )
        {
            return new AttendanceService( _rockContext ).Queryable().AsNoTracking()
                .Where( a => a.Occurrence.GroupId == gm.GroupId )
                .Where( a => a.PersonAlias.PersonId == gm.PersonId )
                .Where( a => a.DidAttend == true )
                .OrderByDescending( a => a.StartDateTime )
                .Select( a => a.StartDateTime )
                .FirstOrDefault();
        }

        private string GetFormattedHomeAddress( Person p )
        {
            var homeAddressDVID = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid(), _rockContext );

            var address = new GroupLocationService( _rockContext ).Queryable().AsNoTracking()
                .Where( gl => gl.GroupId == p.PrimaryFamilyId )
                .Where( gl => gl.GroupLocationTypeValueId == homeAddressDVID.Id )
                .OrderByDescending( gl => gl.ModifiedDateTime )
                .Select( gl => gl.Location )
                .FirstOrDefault();

            if ( address == null )
            {
                return null;
            }

            var streetAddress = string.Format( "{0} {1}", address.Street1, address.Street2 ).Trim();

            return string.Format( "{0}<br />{1}, {2} {3}", streetAddress, address.City, address.State, address.PostalCode );
        }

        private string GetPhoneList( List<PhoneNumber> phoneList )
        {
            var phoneSB = new StringBuilder();

            foreach ( var dvc in PhoneTypes.OrderBy( d => d.Key ) )
            {
                var phone = phoneList.Where( p => !p.IsUnlisted )
                    .Where( p => p.NumberTypeValueId == dvc.Value.Id )
                    .FirstOrDefault();

                if ( phone != null && phone.Id > 0 )
                {
                    phoneSB.Append( "<div class=\"row\">" );
                    phoneSB.AppendFormat( "<div class=\"col-sm-4\"><label>{0}</label></div><div class=\"col-sm-8\">{1}</div>", dvc.Value.Value, phone.NumberFormatted );
                    phoneSB.Append( "</div>" );
                }
            }

            return phoneSB.ToString();
        }

        private void LoadPhoneTypeList()
        {
            var selectedPhoneTypeGuids = GetAttributeValue( AttributeKey.PhoneNumberTypes )
                .SplitDelimitedValues()
                .Select( p => p.AsGuidOrNull() )
                .Where( p => p.HasValue )
                .ToList();

            var phonetypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).DefinedValues
                .Where( v => v.IsActive )
                .Where( v => selectedPhoneTypeGuids.Contains( v.Guid ) )
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();

            var counter = 0;
            _phoneTypes = new SortedList<int, DefinedValueCache>();
            foreach ( var dv in phonetypes )
            {
                _phoneTypes.Add( counter, dv );
                counter++;
            }
        }

        private void LoadFilters()
        {
            if ( !FilterByGroupRole && !FilterByGender && !FilterByGroupStatus )
            {
                gfGroupMember.Visible = false;
            }
            else
            {
                gfGroupMember.Visible = true;
            }

            if ( FilterByGender )
            {
                cblGender.Visible = true;
                LoadFilterGender();
            }
            else
            {
                cblGender.Visible = false;
            }

            if ( FilterByGroupRole )
            {
                cblGroupRole.Visible = true;
                LoadFilterGroupRole();
            }
            else
            {
                cblGroupRole.Visible = false;
            }

            if ( FilterByGroupStatus )
            {
                cblStatus.Visible = true;
                LoadFilterGroupStatus();
            }
            else
            {
                cblStatus.Visible = false;
            }
        }

        private void LoadFilterGender()
        {
            var key = GetGroupFilterKey( "Gender" );
            var preference = gfGroupMember.GetUserPreference( key );

            if ( preference.IsNotNullOrWhiteSpace() )
            {
                cblGender.SetValues( preference.SplitDelimitedValues() );
            }
        }

        private void LoadFilterGroupRole()
        {
            if ( SelectedGroup != null )
            {
                var key = GetGroupFilterKey( "Role" );
                var preference = gfGroupMember.GetUserPreference( key );

                cblGroupRole.Items.Clear();

                var groupRoles = GroupTypeCache.Get( SelectedGroup.GroupTypeId ).Roles
                    .OrderBy( r => r.Name )
                    .ToList();

                cblGroupRole.DataSource = groupRoles;
                cblGroupRole.DataValueField = "Id";
                cblGroupRole.DataTextField = "Name";
                cblGroupRole.DataBind();

                if ( !preference.IsNullOrWhiteSpace() )
                {
                    cblGroupRole.SetValues( preference.SplitDelimitedValues() );
                }
            }
        }

        private void LoadFilterGroupStatus()
        {

            var key = GetGroupFilterKey( "Status" );
            var preference = gfGroupMember.GetUserPreference( key );

            cblStatus.Items.Clear();
            Dictionary<int, string> statuses = new Dictionary<int, string>();

            foreach ( var value in Enum.GetValues( typeof( GroupMemberStatus ) ) )
            {
                var name = value.ToString();
                var id = ( int ) Enum.Parse( typeof( GroupMemberStatus ), name );

                statuses.Add( id, name );
            }

            cblStatus.DataSource = statuses.OrderBy( s => s.Key ).ToList();
            cblStatus.DataValueField = "Key";
            cblStatus.DataTextField = "Value";
            cblStatus.DataBind();

            if ( preference.IsNotNullOrWhiteSpace() )
            {
                cblStatus.SetValues( preference.SplitDelimitedValues() );
            }
            else
            {
                cblStatus.SetValues( new List<string>() { ( ( int ) GroupMemberStatus.Active ).ToString() } );
            }

        }

        private string GetGroupFilterKey( string key )
        {
            return string.Format( "{0}-{1}", GroupId, key );
        }

        private void LoadGroup()
        {
            var groupQry = new GroupService( _rockContext )
                .Queryable( "Members,Members.Person,Members.GroupRole,Members.Person.PhoneNumbers,GroupType" )
                .AsNoTracking()
                .Where( g => g.IsActive )
                .Where( g => !g.IsArchived );

            if ( !GetAttributeValue( AttributeKey.AllowPrivateGroups ).AsBoolean() )
            {
                groupQry = groupQry.Where( g => g.IsPublic );
            }

            var group = groupQry.SingleOrDefault( g => g.Id == GroupId );

            if ( group != null )
            {
                _selectedGroup = group;
            }
        }

        protected class FeatureSet
        {
            public bool AddGuestButton;
            public bool SchedulerButton;
            public bool Headcount;
            public bool RequestUpdates;
            public bool DisplayParentInfo;
            public bool ForwardAttendanceNotes;
            public bool AddSendEmailToCoachButton;
            public bool GroupLeaderToolbox;
            public bool Roster;
            public bool Attendance;
            public bool LeaderNewsPortal;
            public bool Curriculum;
            public bool EditGroupMembers;
            public bool GroupCommunication;

            public FeatureSet( string features )
            {
                AddGuestButton = features.Contains( "6abaaa35-3e8d-4386-a755-4fb8c31beef8" );
                SchedulerButton = features.Contains( "2b7997c0-e98b-40eb-9f6c-d9375bac79f1" );
                Headcount = features.Contains( "a70d029b-90c0-4e00-bcd3-ea3dec2dca7b" );
                RequestUpdates = features.Contains( "8b91f1cd-dcf6-4d04-aeb7-cbcff5b48134" );
                DisplayParentInfo = features.Contains( "bb45f157-ce0e-48e1-8946-21eb3c9c7a85" );
                ForwardAttendanceNotes = features.Contains( "31dcc393-d2e2-48b0-8fd8-481bcdfe6230" );
                AddSendEmailToCoachButton = features.Contains( "59608021-6cef-4934-9547-d7c71c2a0666" );
                GroupLeaderToolbox = features.Contains( "bf778fb4-6613-4565-8a45-371cb9a7c172" );
                Roster = features.Contains( "5c3e2545-ee21-43b4-a7c9-f3cc4eca018b" );
                Attendance = features.Contains( "30ced7e4-03ca-47d0-8aa7-98f4c64e0c2d" );
                LeaderNewsPortal = features.Contains( "e93c8e1d-afe2-486c-bd10-74dfe47593f5" );
                Curriculum = features.Contains( "63e6ea73-7e54-4bf6-a060-100f028b19a1" );
                EditGroupMembers = features.Contains( "1f54b1cc-b94c-429b-b927-1e542d8c7fe3" );
                GroupCommunication = features.Contains( "4f092398-a348-4974-bc70-517da060b7cd" ) || features.Contains( "2a54ed73-6562-4c23-a8b9-34f6f83a6251" ); // ::: the second guid is temporary and can be deleted after the next time Beta is refreshed.
            }
        }

        /// <summary>
        /// Populate the editable Group Member role list
        /// Load all roles in drop down list that are not leaders.
        /// </summary>
        private void LoadGroupMemberRoleList()
        {
            if ( SelectedGroup != null )
            {
                var coachGuid = "b54d47f1-ba15-4158-a86f-5a3a35923ee3".AsGuid();
                var roles = new GroupTypeRoleService( _rockContext ).Queryable().AsNoTracking()
                    .Where( r => r.GroupTypeId == SelectedGroup.GroupTypeId )
                    .Where( r => !r.IsLeader )
                    .Where( r => r.Guid != coachGuid )
                    .OrderBy( r => r.Name )
                    .ToList();

                cblGroupMemberRole.Items.Clear();
                cblGroupMemberRole.DataSource = roles;
                cblGroupMemberRole.DataValueField = "Id";
                cblGroupMemberRole.DataTextField = "Name";
                cblGroupMemberRole.DataBind();

                // ddlGroupMemberRole.Items.Insert( 0, new ListItem( String.Empty, String.Empty ) );
            }
        }

        private void LoadGroupMembers()
        {
            if ( SelectedGroup != null )
            {
                gmCount = 0;
                var gmQuery = GetGroupMemberQuery();

                var groupMembers = gmQuery
                    .OrderBy( m => m.Person.LastName )
                    .ThenBy( m => m.Person.NickName )
                    .GroupBy( m => m.PersonId )
                    .ToList();

                rptGroupMember.DataSource = groupMembers;
                rptGroupMember.DataBind();
            }
        }

        private IEnumerable<GroupMember> GetGroupMemberQuery()
        {
            var gmQuery = SelectedGroup.Members
                .Where( m => m.Person.IsDeceased == false );

            if ( FilterByGroupStatus )
            {
                if ( cblStatus.SelectedValuesAsInt.Count > 0 )
                {
                    var selectedStatuses = cblStatus.SelectedValuesAsInt
                        .Select( s => ( GroupMemberStatus ) s )
                        .ToList();
                    gmQuery = gmQuery.Where( m => selectedStatuses.Contains( m.GroupMemberStatus ) );
                }
            }
            else
            {
                gmQuery = gmQuery.Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive );
            }

            if ( cblGroupRole.SelectedValuesAsInt.Count > 0 )
            {
                var roleIds = cblGroupRole.SelectedValuesAsInt;
                gmQuery = gmQuery.Where( m => roleIds.Contains( m.GroupRoleId ) );
            }

            if ( cblGender.SelectedValuesAsInt.Count > 0 )
            {
                List<Gender> genders = new List<Gender>();
                foreach ( var item in cblGender.SelectedValuesAsInt )
                {
                    genders.Add( ( Gender ) item );
                }

                gmQuery = gmQuery.Where( m => genders.Contains( m.Person.Gender ) );
            }

            return gmQuery;
        }

        private void UpdateNotification( string title, string message, NotificationBoxType boxType )
        {
            nbGroupMembers.Title = title;
            nbGroupMembers.Text = message;
            nbGroupMembers.NotificationBoxType = boxType;

            nbGroupMembers.Visible = message.IsNotNullOrWhiteSpace();
            nbMemberRoleError.Visible = false;
        }
        #endregion

        #region Helper Classes
        [Serializable]
        private class Assignment
        {
            public Guid AssignmentGuid { get; set; }
            public int? LocationId { get; set; }
            public string LocationName { get; set; }
            public int? ScheduleId { get; set; }
            public string FormattedScheduleName { get; set; }

            public Assignment( GroupMemberAssignment assignment )
            {
                AssignmentGuid = assignment.Guid;
                LocationId = assignment.LocationId;
                LocationName = assignment.Location?.Name ?? "No Location Preference";
                ScheduleId = assignment.ScheduleId;
                FormattedScheduleName = assignment.Schedule.StartTimeOfDay.ToTimeString();
            }

            public Assignment() { }
        }
        #endregion
    }
}