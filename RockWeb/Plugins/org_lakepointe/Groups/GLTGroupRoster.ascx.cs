using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

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
        Order = 3 )]

    [DefinedValueField( "Phone Numbers to Show",
        Description = "The phone numbers to display on the group member detail.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        AllowMultiple = true,
        IncludeInactive = false,
        IsRequired = true,
        Key = AttributeKey.PhoneNumberTypes,
        Category = "Group Member",
        Order = 4 )]

    [BooleanField( "Show First Attended",
        Description = "Show the date that the group member first attended the group.",
        DefaultBooleanValue = true,
        DefaultValue = "true",
        IsRequired = false,
        Key = AttributeKey.ShowFirstAttended,
        Category = "Group Member",
        Order = 5 )]

    [BooleanField( "Show Last Attended",
        Description = "Show the date that the group member most recently attended the group.",
        DefaultBooleanValue = true,
        DefaultValue = "true",
        IsRequired = false,
        Key = AttributeKey.ShowLastAttended,
        Category = "Group Member",
        Order = 6 )]
    [BooleanField( "Show Birthdate",
        Description = "Show the group member's birthday in details block.",
        DefaultBooleanValue = false,
        DefaultValue = "false",
        IsRequired = false,
        Key = AttributeKey.ShowBirthdate,
        Category = "Group Member",
        Order = 7 )]




    [DisplayName( "Group Leader Toolbox - Roster" )]
    [Category( "LPC > Groups" )]
    [Description( "Displays the members of a group." )]
    public partial class GLTGroupRoster : RockBlock
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
        }
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
        private bool? _allowCommunications = null;
        private bool? _allowMemberEdit = null;
        private bool? _filterByGender = null;
        private bool? _filterByGroupStatus = null;
        private bool? _filterByGroupRole = null;
        private string _communicationPageRoute = null;
        private SortedList<int, DefinedValueCache> _phoneTypes = null;
        private int gmCount = 0;

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
                if ( !_allowMemberEdit.HasValue )
                {
                    if ( SelectedGroup.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson )
                        && GetAttributeValue( AttributeKey.AllowMemberEdit ).AsBoolean() )
                    {
                        _allowMemberEdit = true;
                    }
                    else
                    {
                        _allowMemberEdit = false;
                    }
                }

                return _allowMemberEdit.Value;
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
        #endregion

        #region Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _rockContext = new RockContext();
            this.BlockUpdated += GLTGroupRoster_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upGroupRoster );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            var cpr = CommunicationPageRoute;
            UpdateNotification( null, null, NotificationBoxType.Default );
            if ( !IsPostBack )
            {
                BlockSetup();
            }
        }
        #endregion

        #region Events
        private void GLTGroupRoster_BlockUpdated( object sender, EventArgs e )
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

                var selectedMemberRoles = cblGroupMemberRole.SelectedValuesAsInt;
                var memberRole = group.GroupType.Roles.Where( r => r.Name == "Member" ).FirstOrDefault();
                if ( memberRole != null )
                {
                    var isProspect = gmService.Queryable().Where( m =>
                                                     m.GroupId == groupMember.GroupId &&
                                                     m.PersonId == groupMember.PersonId &&
                                                     m.GroupRole.Name.Contains( "Prospect" ) ).Any();
                    if ( !isProspect )
                    {
                        selectedMemberRoles.Add( memberRole.Id );
                    }
                }

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

                        if ( updateGroupMember == null )
                        {
                            updateGroupMember = new GroupMember { Id = 0 };
                            updateGroupMember.GroupId = group.Id;
                            updateGroupMember.PersonId = personId.Value;
                            updateGroupMember.GroupRoleId = groupRole.Id;
                            updateGroupMember.CommunicationPreference = groupMember.CommunicationPreference;
                            updateGroupMember.IsNotified = groupMember.IsNotified;
                            gmService.Add( updateGroupMember );
                        }

                        if ( updateGroupMember == null )
                        {
                            return;
                        }

                        if ( groupMemberStatus == GroupMemberStatus.Pending )
                        {
                            if ( groupRole.Name == "Member" )
                            {
                                updateGroupMember.GroupMemberStatus = GroupMemberStatus.Pending;
                            }
                            else
                            {
                                updateGroupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                            }
                        }
                        else
                        {
                            updateGroupMember.GroupMemberStatus = groupMemberStatus;
                        }

                        gmService.Restore( updateGroupMember );
                        gmUpdateContext.SaveChanges();
                    }

                }
            }

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
            var lPhones = e.Item.FindControl( "lPhones" ) as Literal;
            var lRole = e.Item.FindControl( "lRole" ) as Literal;
            var lRowHeader = e.Item.FindControl( "lRowHeader" ) as Literal;
            var lRowFooter = e.Item.FindControl( "lRowFooter" ) as Literal;
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
            var pnlPhones = e.Item.FindControl( "pnlPhones" ) as Panel;
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
        #endregion

        #region Internal Methodds

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

            if ( AllowCommunications && CommunicationPageRoute.IsNotNullOrWhiteSpace() )
            {
                lbSendParentCommunication.Visible = _featureSet.DisplayParentInfo;
            }

            LoadFilters();
            ConfigureCommands();
            LoadGroupMembers();
            LoadGroupMemberRoleList();

            pnlGroupMembers.Visible = true;
            pnlEditGroupMember.Visible = false;
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
            pnlCommandsTop.Visible = true;


            if ( AllowCommunications && CommunicationPageRoute.IsNotNullOrWhiteSpace() )
            {
                lbSendCommunicationTop.Visible = AllowCommunications;
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

                GetGroupTypeFeatures();
            }
        }

        protected struct FeatureSet
        {
            public bool AddGuestButton;
            public bool SchedulerButton;
            public bool Headcount;
            public bool RequestUpdates;
            public bool DisplayParentInfo;
            public bool ForwardAttendanceNotes;
        }

        protected FeatureSet _featureSet = new FeatureSet();

        private void GetGroupTypeFeatures()
        {
            _featureSet.AddGuestButton = false;
            _featureSet.SchedulerButton = false;
            _featureSet.Headcount = false;
            _featureSet.RequestUpdates = false;
            _featureSet.DisplayParentInfo = false;
            _featureSet.ForwardAttendanceNotes = false;

            SelectedGroup.GroupType.LoadAttributes();
            var features = SelectedGroup.GroupType.GetAttributeValue( "GroupLeaderToolboxFeatureSet" );
            foreach ( var feature in features.Split( ',' ) )
            {
                switch ( feature )
                {
                    case "6abaaa35-3e8d-4386-a755-4fb8c31beef8":
                        _featureSet.AddGuestButton = true;
                        break;
                    case "2b7997c0-e98b-40eb-9f6c-d9375bac79f1":
                        _featureSet.SchedulerButton = true;
                        break;
                    case "a70d029b-90c0-4e00-bcd3-ea3dec2dca7b":
                        _featureSet.Headcount = true;
                        break;
                    case "8b91f1cd-dcf6-4d04-aeb7-cbcff5b48134":
                        _featureSet.RequestUpdates = true;
                        break;
                    case "bb45f157-ce0e-48e1-8946-21eb3c9c7a85":
                        _featureSet.DisplayParentInfo = true;
                        break;
                    case "31dcc393-d2e2-48b0-8fd8-481bcdfe6230":
                        _featureSet.ForwardAttendanceNotes = true;
                        break;
                }
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
        }
        #endregion
    }
}