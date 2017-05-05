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
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Group Registration" )]
    [Category( "Groups" )]
    [Description( "Allows a person to register for a group." )]

    [GroupTypesField( "Allowed Group Types", "This setting restricts which types of groups a person can be added to, however selecting a specific group via the Group setting will override this restriction.", true, Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP, "", 0 )]
    [GroupField( "Group", "Optional group to add person to. If omitted, the group's Guid should be passed via the Query string (GroupGuid=).", false, "", "", 0 )]
    [BooleanField( "Enable Passing Group Id", "If enabled, allows the ability to pass in a group's Id (GroupId=) instead of the Guid.", true, "", 0 )]
    [CustomRadioListField("Mode", "The mode to use when displaying registration details.", "Simple^Simple,Full^Full,FullSpouse^Full With Spouse", true, "Simple", "", 1)]
    [CustomRadioListField( "Group Member Status", "The group member status to use when adding person to group (default: 'Pending'.)", "2^Pending,1^Active,0^Inactive", true, "2", "", 2 )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 3 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 4 )]
    [WorkflowTypeField( "Workflow", "An optional workflow to start when registration is created. The GroupMember will set as the workflow 'Entity' when processing is started.", false, false, "", "", 5 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 6 )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"
", "", 7 )]
    [LinkedPage("Result Page", "An optional page to redirect user to after they have been registered for the group.", false, "", "", 8)]
    [CodeEditorField( "Result Lava Template", "The lava template to use to format result message after user has been registered. Will only display if user is not redirected to a Result Page ( previous setting ).", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"
", "", 9 )]
    [CustomRadioListField( "Auto Fill Form", "If set to FALSE then the form will not load the context of the logged in user (default: 'True'.)", "true^True,false^False", true, "true", "", 10 )]
    [TextField( "Register Button Alt Text", "Alternate text to use for the Register button (default is 'Register').", false, "", "", 11 )]
    [BooleanField( "Prevent Overcapacity Registrations", "When set to true, user cannot register for groups that are at capacity or whose default GroupTypeRole are at capacity. If only one spot is available, no spouses can be registered.", true, "", 12 )]
    public partial class GroupRegistration : RockBlock
    {
        #region Fields

        RockContext _rockContext = null;
        string _mode = "Simple";
        Group _group = null;
        GroupTypeRole _defaultGroupRole = null;
        DefinedValueCache _dvcConnectionStatus = null;
        DefinedValueCache _dvcRecordStatus = null;
        DefinedValueCache _married = null;
        DefinedValueCache _homeAddressType = null;
        GroupTypeCache _familyType = null;
        GroupTypeRoleCache _adultRole = null;
        bool _autoFill = true;
        bool _isValidSettings = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is simple.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is simple; otherwise, <c>false</c>.
        /// </value>
        protected bool IsSimple
        {
            get
            {
                return _mode == "Simple";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is full.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is full; otherwise, <c>false</c>.
        /// </value>
        protected bool IsFull
        {
            get
            {
                return _mode == "Full";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is full with spouse.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is full with spouse; otherwise, <c>false</c>.
        /// </value>
        protected bool IsFullWithSpouse
        {
            get
            {
                return _mode == "FullSpouse";
            }
        }

        #endregion

        #region Control Methods

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
            base.OnLoad( e );

            if ( !CheckSettings() )
            {
                _isValidSettings = false;
                nbNotice.Visible = true;
                pnlView.Visible = false;
            }
            else
            {
                nbNotice.Visible = false;
                pnlView.Visible = true;

                if ( !Page.IsPostBack )
                {
                    ShowDetails();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnRegister control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRegister_Click( object sender, EventArgs e )
        {
            // Check _isValidSettings in case the form was showing and they clicked the visible register button.
            if ( Page.IsValid && _isValidSettings )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                Person person = null;
                Person spouse = null;
                Group family = null;
                GroupLocation homeLocation = null;
                bool isMatch = false;

                var changes = new List<string>();
                var spouseChanges = new List<string>();
                var familyChanges = new List<string>();

                // Only use current person if the name entered matches the current person's name and autofill mode is true
                if ( _autoFill )
                {
                    if ( CurrentPerson != null &&
                        tbFirstName.Text.Trim().Equals( CurrentPerson.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) &&
                        tbLastName.Text.Trim().Equals( CurrentPerson.LastName.Trim(), StringComparison.OrdinalIgnoreCase ) )
                    {
                        person = personService.Get( CurrentPerson.Id );
                    }
                }

                // Try to find person by name/email 
                if ( person == null )
                {
                    var matches = personService.GetByMatch( tbFirstName.Text.Trim(), tbLastName.Text.Trim(), tbEmail.Text.Trim() );
                    if ( matches.Count() == 1 )
                    {
                        person = matches.First();
                        isMatch = true;
                    }
                }

                // Check to see if this is a new person
                if ( person == null )
                {
                    // If so, create the person and family record for the new person
                    person = new Person();
                    person.FirstName = tbFirstName.Text.Trim();
                    person.LastName = tbLastName.Text.Trim();
                    person.Email = tbEmail.Text.Trim();
                    person.IsEmailActive = true;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    person.ConnectionStatusValueId = _dvcConnectionStatus.Id;
                    person.RecordStatusValueId = _dvcRecordStatus.Id;
                    person.Gender = Gender.Unknown;

                    family = PersonService.SaveNewPerson( person, rockContext, _group.CampusId, false );
                }
                else
                {
                    // updating current existing person
                    History.EvaluateChange( changes, "Email", person.Email, tbEmail.Text );
                    person.Email = tbEmail.Text;

                    // Get the current person's families
                    var families = person.GetFamilies( rockContext );

                    // If address can being entered, look for first family with a home location
                    if ( !IsSimple )
                    {
                        foreach ( var aFamily in families )
                        {
                            homeLocation = aFamily.GroupLocations
                                .Where( l =>
                                    l.GroupLocationTypeValueId == _homeAddressType.Id &&
                                    l.IsMappedLocation )
                                .FirstOrDefault();
                            if ( homeLocation != null )
                            {
                                family = aFamily;
                                break;
                            }
                        }
                    }

                    // If a family wasn't found with a home location, use the person's first family
                    if ( family == null )
                    {
                        family = families.FirstOrDefault();
                    }
                }

                // If using a 'Full' view, save the phone numbers and address
                if ( !IsSimple )
                {
                    if ( !isMatch || !string.IsNullOrWhiteSpace( pnHome.Number ) )
                    {
                        SetPhoneNumber( rockContext, person, pnHome, null, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), changes );
                    }
                    if ( !isMatch || !string.IsNullOrWhiteSpace( pnHome.Number ) )
                    {
                        SetPhoneNumber( rockContext, person, pnCell, cbSms, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), changes );
                    }

                    if ( !isMatch || !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                    {
                        string oldLocation = homeLocation != null ? homeLocation.Location.ToString() : string.Empty;
                        string newLocation = string.Empty;

                        var location = new LocationService( rockContext ).Get( acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
                        if ( location != null )
                        {
                            if ( homeLocation == null )
                            {
                                homeLocation = new GroupLocation();
                                homeLocation.GroupLocationTypeValueId = _homeAddressType.Id;
                                family.GroupLocations.Add( homeLocation );
                            }
                            else
                            {
                                oldLocation = homeLocation.Location.ToString();
                            }

                            homeLocation.Location = location;
                            newLocation = location.ToString();
                        }
                        else
                        {
                            if ( homeLocation != null )
                            {
                                homeLocation.Location = null;
                                family.GroupLocations.Remove( homeLocation );
                                new GroupLocationService( rockContext ).Delete( homeLocation );
                            }
                        }

                        History.EvaluateChange( familyChanges, "Home Location", oldLocation, newLocation );
                    }

                    // Check for the spouse
                    if ( IsFullWithSpouse && !string.IsNullOrWhiteSpace(tbSpouseFirstName.Text) && !string.IsNullOrWhiteSpace(tbSpouseLastName.Text) )
                    {
                        spouse = person.GetSpouse();
                        if ( spouse == null ||
                            !tbSpouseFirstName.Text.Trim().Equals( spouse.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) ||
                            !tbSpouseLastName.Text.Trim().Equals( spouse.LastName.Trim(), StringComparison.OrdinalIgnoreCase ) )
                        {
                            spouse = new Person();

                            spouse.FirstName = tbSpouseFirstName.Text.FixCase();
                            History.EvaluateChange( spouseChanges, "First Name", string.Empty, spouse.FirstName );

                            spouse.LastName = tbSpouseLastName.Text.FixCase();
                            History.EvaluateChange( spouseChanges, "Last Name", string.Empty, spouse.LastName );

                            spouse.ConnectionStatusValueId = _dvcConnectionStatus.Id;
                            spouse.RecordStatusValueId = _dvcRecordStatus.Id;
                            spouse.Gender = Gender.Unknown;

                            spouse.IsEmailActive = true;
                            spouse.EmailPreference = EmailPreference.EmailAllowed;

                            var groupMember = new GroupMember();
                            groupMember.GroupRoleId = _adultRole.Id;
                            groupMember.Person = spouse;

                            family.Members.Add( groupMember );

                            spouse.MaritalStatusValueId = _married.Id;
                            person.MaritalStatusValueId = _married.Id;
                        }

                        History.EvaluateChange( changes, "Email", person.Email, tbEmail.Text );
                        spouse.Email = tbSpouseEmail.Text;

                        SetPhoneNumber( rockContext, spouse, pnHome, null, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), spouseChanges );
                        SetPhoneNumber( rockContext, spouse, pnSpouseCell, cbSpouseSms, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), spouseChanges );
                    }
                }

                // Save the person/spouse and change history 
                rockContext.SaveChanges();
                HistoryService.SaveChanges( rockContext, typeof( Person ),
                    Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, changes );
                HistoryService.SaveChanges( rockContext, typeof( Person ),
                    Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(), person.Id, familyChanges );
                if ( spouse != null )
                {

                    HistoryService.SaveChanges( rockContext, typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), spouse.Id, spouseChanges );
                    HistoryService.SaveChanges( rockContext, typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(), spouse.Id, familyChanges );
                }

                // Check to see if a workflow should be launched for each person
                WorkflowType workflowType = null;
                Guid? workflowTypeGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
                if ( workflowTypeGuid.HasValue )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
                }

                // Save the registrations ( and launch workflows )
                var newGroupMembers = new List<GroupMember>();
                AddPersonToGroup( rockContext, person, workflowType, newGroupMembers );
                AddPersonToGroup( rockContext, spouse, workflowType, newGroupMembers );

                // Show the results
                pnlView.Visible = false;
                pnlResult.Visible = true;

                // Show lava content
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Group", _group );
                mergeFields.Add( "GroupMembers", newGroupMembers );

                bool showDebug = UserCanEdit && GetAttributeValue( "EnableDebug" ).AsBoolean();
                lResultDebug.Visible = showDebug;
                if ( showDebug )
                {
                    lResultDebug.Text = mergeFields.lavaDebugInfo( _rockContext );
                }

                string template = GetAttributeValue( "ResultLavaTemplate" );
                lResult.Text = template.ResolveMergeFields( mergeFields );

                // Will only redirect if a value is specifed
                NavigateToLinkedPage( "ResultPage" );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            _rockContext = _rockContext ?? new RockContext();

            if ( _group != null )
            {
                // Show lava content
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Group", _group );

                bool showDebug = UserCanEdit && GetAttributeValue( "EnableDebug" ).AsBoolean();
                lLavaOutputDebug.Visible = showDebug;
                if ( showDebug )
                {
                    lLavaOutputDebug.Text = mergeFields.lavaDebugInfo( _rockContext );
                }

                string template = GetAttributeValue( "LavaTemplate" );
                lLavaOverview.Text = template.ResolveMergeFields( mergeFields );

                // Set visibility based on selected mode
                if ( IsFullWithSpouse )
                {
                    pnlCol1.RemoveCssClass( "col-md-12" ).AddCssClass( "col-md-6" );
                }
                else
                {
                    pnlCol1.RemoveCssClass( "col-md-6" ).AddCssClass( "col-md-12" );
                }
                pnlCol2.Visible = IsFullWithSpouse;

                pnlHomePhone.Visible = !IsSimple;
                pnlCellPhone.Visible = !IsSimple;
                acAddress.Visible = !IsSimple;

                if ( CurrentPersonId.HasValue && _autoFill )
                {
                    var personService = new PersonService( _rockContext );
                    Person person = personService
                        .Queryable( "PhoneNumbers.NumberTypeValue" ).AsNoTracking()
                        .FirstOrDefault( p => p.Id == CurrentPersonId.Value );

                    tbFirstName.Text = CurrentPerson.FirstName;
                    tbLastName.Text = CurrentPerson.LastName;
                    tbEmail.Text = CurrentPerson.Email;

                    if ( !IsSimple )
                    {
                        Guid homePhoneType = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
                        var homePhone = person.PhoneNumbers
                            .FirstOrDefault( n => n.NumberTypeValue.Guid.Equals( homePhoneType ) );
                        if ( homePhone != null )
                        {
                            pnHome.Text = homePhone.Number;
                        }

                        Guid cellPhoneType = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
                        var cellPhone = person.PhoneNumbers
                            .FirstOrDefault( n => n.NumberTypeValue.Guid.Equals( cellPhoneType ) );
                        if ( cellPhone != null )
                        {
                            pnCell.Text = cellPhone.Number;
                            cbSms.Checked = cellPhone.IsMessagingEnabled;
                        }

                        var homeAddress = person.GetHomeLocation();
                        if ( homeAddress != null )
                        {
                            acAddress.SetValues( homeAddress );
                        }

                        if ( IsFullWithSpouse )
                        {
                            var spouse = person.GetSpouse( _rockContext );
                            if ( spouse != null )
                            {
                                tbSpouseFirstName.Text = spouse.FirstName;
                                tbSpouseLastName.Text = spouse.LastName;
                                tbSpouseEmail.Text = spouse.Email;

                                var spouseCellPhone = spouse.PhoneNumbers
                                    .FirstOrDefault( n => n.NumberTypeValue.Guid.Equals( cellPhoneType ) );
                                if ( spouseCellPhone != null )
                                {
                                    pnSpouseCell.Text = spouseCellPhone.Number;
                                    cbSpouseSms.Checked = spouseCellPhone.IsMessagingEnabled;
                                }
                            }
                        }
                    }
                }
               
                if ( GetAttributeValue( "PreventOvercapacityRegistrations" ).AsBoolean() )
                {
                    int openGroupSpots = 2;
                    int openRoleSpots = 2;

                    // If the group has a GroupCapacity, check how far we are from hitting that.
                    if ( _group.GroupCapacity.HasValue )
                    {
                        openGroupSpots = _group.GroupCapacity.Value - _group.Members.Count();
                    }

                    // When someone registers for a group on the front-end website, they automatically get added with the group's default
                    // GroupTypeRole. If that role exists and has a MaxCount, check how far we are from hitting that.
                    if ( _defaultGroupRole != null && _defaultGroupRole.MaxCount.HasValue )
                    {
                        openRoleSpots = _defaultGroupRole.MaxCount.Value - _group.Members.Where( m => m.GroupRoleId == _defaultGroupRole.Id ).Count();
                    }

                    // Between the group's GroupCapacity and DefaultGroupRole.MaxCount, grab the one we're closest to hitting, and how close we are to
                    // hitting it.
                    int openSpots = Math.Min( openGroupSpots, openRoleSpots );

                    // If there's only one spot open, disable the spouse fields and display a warning message.
                    if ( openSpots == 1 )
                    {
                        tbSpouseFirstName.Enabled = false;
                        tbSpouseLastName.Enabled = false;
                        pnSpouseCell.Enabled = false;
                        cbSpouseSms.Enabled = false;
                        tbSpouseEmail.Enabled = false;
                        nbWarning.Text = "This group is near its capacity. Only one individual can register.";
                        nbWarning.Visible = true;
                    }

                    // If no spots are open, display a message that says so.
                    if ( openSpots <= 0 )
                    {
                        nbNotice.Text = "This group is at or exceeds capacity.";
                        nbNotice.Visible = true;
                        pnlView.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the person to group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="workflowType">Type of the workflow.</param>
        /// <param name="groupMembers">The group members.</param>
        private void AddPersonToGroup( RockContext rockContext, Person person, WorkflowType workflowType, List<GroupMember> groupMembers )
        {
            if (person != null )
            {
                if ( !_group.Members
                    .Any( m => 
                        m.PersonId == person.Id &&
                        m.GroupRoleId == _defaultGroupRole.Id))
                {
                    var groupMemberService = new GroupMemberService(rockContext);
                    var groupMember = new GroupMember();
                    groupMember.PersonId = person.Id;
                    groupMember.GroupRoleId = _defaultGroupRole.Id;
                    groupMember.GroupMemberStatus = (GroupMemberStatus)GetAttributeValue("GroupMemberStatus").AsInteger();
                    groupMember.GroupId = _group.Id;
                    groupMemberService.Add( groupMember );
                    rockContext.SaveChanges();

                    if ( workflowType != null )
                    {
                        try
                        {
                            List<string> workflowErrors;
                            var workflow = Workflow.Activate( workflowType, person.FullName );
                            new WorkflowService( rockContext ).Process( workflow, groupMember, out workflowErrors );
                        }
                        catch (Exception ex)
                        {
                            ExceptionLogService.LogException( ex, this.Context );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the settings.  If false is returned, it's expected that the caller will make
        /// the nbNotice visible to inform the user of the "settings" error.
        /// </summary>
        /// <returns>true if settings are valid; false otherwise</returns>
        private bool CheckSettings()
        {
            _rockContext = _rockContext ?? new RockContext();

            _mode = GetAttributeValue( "Mode" );

            _autoFill = GetAttributeValue( "AutoFillForm" ).AsBoolean();

            tbEmail.Required = _autoFill;

            string registerButtonText = GetAttributeValue( "RegisterButtonAltText" );
            if ( string.IsNullOrWhiteSpace( registerButtonText ) )
            {
                registerButtonText = "Register";
            }
            btnRegister.Text = registerButtonText;

            var groupService = new GroupService( _rockContext );
            bool groupIsFromQryString = true;

            Guid? groupGuid = GetAttributeValue( "Group" ).AsGuidOrNull();
            if ( groupGuid.HasValue )
            {
                _group = groupService.Get( groupGuid.Value );
                groupIsFromQryString = false;
            }

            if ( _group == null )
            {
                groupGuid = PageParameter( "GroupGuid" ).AsGuidOrNull();
                if ( groupGuid.HasValue )
                {
                    _group = groupService.Get( groupGuid.Value );
                }
            }

            if ( _group == null && GetAttributeValue( "EnablePassingGroupId" ).AsBoolean( false ) )
            {
                int? groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
                if ( groupId.HasValue )
                {
                    _group = groupService.Get( groupId.Value );
                }
            }

            if ( _group == null )
            {
                nbNotice.Heading = "Unknown Group";
                nbNotice.Text = "<p>This page requires a valid group identifying parameter and there was not one provided.</p>";
                return false;
            }
            else
            {
                var groupTypeGuids = this.GetAttributeValue( "AllowedGroupTypes" ).SplitDelimitedValues().AsGuidList();

                if ( groupIsFromQryString && groupTypeGuids.Any() && !groupTypeGuids.Contains( _group.GroupType.Guid ) )
                {
                    _group = null;
                    nbNotice.Heading = "Invalid Group";
                    nbNotice.Text = "<p>The selected group is a restricted group type therefore this block cannot be used to add people to these groups (unless configured to allow).</p>";
                    return false;
                }
                else
                {
                    _defaultGroupRole = _group.GroupType.DefaultGroupRole;
                }
            }

            _dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            if ( _dvcConnectionStatus == null )
            {
                nbNotice.Heading = "Invalid Connection Status";
                nbNotice.Text = "<p>The selected Connection Status setting does not exist.</p>";
                return false;
            }

            _dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );
            if ( _dvcRecordStatus == null )
            {
                nbNotice.Heading = "Invalid Record Status";
                nbNotice.Text = "<p>The selected Record Status setting does not exist.</p>";
                return false;
            }

            _married = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
            _homeAddressType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            _familyType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            _adultRole = _familyType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );

            if ( _married == null || _homeAddressType == null || _familyType == null || _adultRole == null )
            {
                nbNotice.Heading = "Missing System Value";
                nbNotice.Text = "<p>There is a missing or invalid system value. Check the settings for Marital Status of 'Married', Location Type of 'Home', Group Type of 'Family', and Family Group Role of 'Adult'.</p>";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the phone number.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="pnbNumber">The PNB number.</param>
        /// <param name="cbSms">The cb SMS.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        /// <param name="changes">The changes.</param>
        private void SetPhoneNumber( RockContext rockContext, Person person, PhoneNumberBox pnbNumber, RockCheckBox cbSms, Guid phoneTypeGuid, List<string> changes )
        {
            var phoneType = DefinedValueCache.Read( phoneTypeGuid );
            if ( phoneType != null )
            {
                var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneType.Id );
                string oldPhoneNumber = string.Empty;
                if ( phoneNumber == null )
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = phoneType.Id };
                }
                else
                {
                    oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                }

                phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbNumber.CountryCode );
                phoneNumber.Number = PhoneNumber.CleanNumber( pnbNumber.Number );

                if ( string.IsNullOrWhiteSpace( phoneNumber.Number ) )
                {
                    if ( phoneNumber.Id > 0 )
                    {
                        new PhoneNumberService( rockContext ).Delete( phoneNumber );
                        person.PhoneNumbers.Remove( phoneNumber );
                    }
                }
                else
                {
                    if ( phoneNumber.Id <= 0)
                    {
                        person.PhoneNumbers.Add( phoneNumber );
                    }
                    if ( cbSms != null && cbSms.Checked )
                    {
                        phoneNumber.IsMessagingEnabled = true;
                        person.PhoneNumbers
                            .Where( n => n.NumberTypeValueId != phoneType.Id )
                            .ToList()
                            .ForEach( n => n.IsMessagingEnabled = false );
                    }
                }

                History.EvaluateChange( changes,
                    string.Format( "{0} Phone", phoneType.Value ),
                    oldPhoneNumber, phoneNumber.NumberFormattedWithCountryCode );
            }
        }

        #endregion
    }
}