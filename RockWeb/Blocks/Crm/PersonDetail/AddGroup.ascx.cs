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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Block for adding new families/groups
    /// </summary>
    [DisplayName( "Add Group" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows for adding a new group and the people in the group (e.g. New Families." )]

    [GroupTypeField( "Group Type",
        Key = AttributeKey.GroupType,
        Description = "The group type to display groups for (default is Family)",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        Order = 0 )]

    [GroupField( "Parent Group", "The parent group to add the new group to (default is none)", false, "", "", 1 )]
    [BooleanField( "Show Title", "Show person title.", true, order: 2 )]
    [BooleanField( "Show Nick Name", "Show an edit box for Nick Name.", false, order: 3 )]
    [BooleanField( "Show Middle Name", "Show an edit box for Middle Name.", false, order: 4 )]
    [BooleanField( "Enable Common Last Name", "Autofills the last name field when adding a new group member with the last name of the first group member.", true, order: 5 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The connection status that should be set by default", false, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR, "", 6 )]
    [BooleanField( "Show Suffix", "Show person suffix.", true, order: 7 )]
    [BooleanField( "Gender", "Require a gender for each person", "Don't require", "Should Gender be required for each person added?", false, "", 8 )]
    [BooleanField( "Birth Date", "Require a birth date for each person", "Don't require", "Should a birth date be required for each person added?", false, "", 9 )]
    [BooleanField( "Child Birthdate", "Require a birth date for each child", "Don't require", "When Family group type, should birth date be required for each child added?", false, "", 10 )]
    [CustomDropdownListField( "Grade", "When Family group type, should Grade be required for each child added?", "True^Require a grade for each child,False^Don't require,None^Grade is not displayed", false, "", "", 11 )]
    [BooleanField( "Show Inactive Campuses", "Determines if inactive campuses should be shown.", true, order: 12 )]
    [BooleanField( "Require Campus", "Determines if a campus is required. The campus will not be displayed if there is only one available campus.", true, "", 13 )]
    [BooleanField( "Show County", "Should County be displayed when editing an address?.", false, "", 14 )]
    [BooleanField( "Marital Status Confirmation", "When Family group type, should user be asked to confirm saving an adult without a marital status?", true, "", 15 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, "Adult Marital Status", "When Family group type, the default marital status for adults in the family.", false, false, "", "", 16 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, "Child Marital Status", "When Family group type, the marital status to use for children in the family.", false, false, Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE, "", 17 )]
    [CustomDropdownListField( "Address", "Should an address be required for the family?", "REQUIRE^Require an address,HOMELESS^Require an address unless family is homeless,NOTREQUIRED^Don't require", false, "NOTREQUIRED", "", 18 )]

    [DefinedValueField(
        "Location Type",
        Key = AttributeKey.LocationType,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE,
        Description = "The type of location that address should use",
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        Order = 19 )]

    [BooleanField( "Show Cell Phone Number First", "Should the cell phone number be listed first before home phone number?", false, "", 20 )]
    [BooleanField( "Phone Number", "Require a phone number", "Don't require", "Should a phone number be required for at least one person?", false, "", 21 )]
    [BooleanField( "Adult Phone Number", "Require a phone number for each adult", "Don't require", "When Family group type, should a phone number be required for each adult added?", false, "", 22 )]
    [CustomDropdownListField( "SMS", "Should SMS be enabled for cell phone numbers by default?", "True^SMS is enabled by default,False^SMS is not enabled by default,None^SMS option is hidden", false, "", "", 23 )]
    [AttributeCategoryField( "Attribute Categories", "The Person Attribute Categories to display attributes from", true, "Rock.Model.Person", false, "", "", 24 )]
    [WorkflowTypeField( "Person Workflow(s)", "The workflow(s) to launch for every person added.", true, false, "", "", 25, "PersonWorkflows" )]
    [WorkflowTypeField( "Adult Workflow(s)", "When Family group type, the workflow(s) to launch for every adult added.", true, false, "", "", 28, "AdultWorkflows" )]
    [WorkflowTypeField( "Child Workflow(s)", "When Family group type, the workflow(s) to launch for every child added.", true, false, "", "", 27, "ChildWorkflows" )]
    [WorkflowTypeField( "Group Workflow(s)", "The workflow(s) to launch for the group (family) that is added.", true, false, "", "", 28, "GroupWorkflows" )]
    [LinkedPage( "Person Detail Page", "The Page to navigate to after the family has been added. (Note that {GroupId} and {PersonId} can be included in the route). Leave blank to go to the default page of ~/Person/{PersonId}.", false, order: 29 )]
    [BooleanField( "Enable Alternate Identifier", "If enabled, an additional step will be shown for supplying a custom alternate identifier for each person.", false, order: 30 )]
    [BooleanField( "Generate Alternate Identifier", "If enabled, a custom alternate identifier will be generated for each person.", true, order: 31 )]

    [BooleanField(
        "Detect Groups already at the Address",
        Description = "If enabled, a prompt to select an existing group will be displayed if there are existing groups that have the same address as the new group.",
        Key = AttributeKey.DetectGroupsAlreadyAtTheAddress,
        DefaultBooleanValue = true,
        Order = 32 )]

    [IntegerField(
        "Max Groups at Address to Detect",
        Key = AttributeKey.MaxGroupsAtAddressToDetect,
        DefaultIntegerValue = 10,
        Order = 33
        )]
    public partial class AddGroup : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string GroupType = "GroupType";
            public const string DetectGroupsAlreadyAtTheAddress = "DetectGroupsAlreadyAtTheAddress";
            public const string MaxGroupsAtAddressToDetect = "MaxGroupsAtAddressToDetect";
            public const string LocationType = "LocationType";
        }

        #endregion Attribute Keys

        #region Fields

        private GroupTypeCache _groupType = null;
        private bool _isFamilyGroupType = false;
        protected string _groupTypeName = string.Empty;

        private DefinedValueCache _locationType = null;
        private bool _isValidLocationType = false;

        private bool _confirmMaritalStatus = true;
        private int _childRoleId = 0;
        private int _adultRoleId = 0;
        private List<NewGroupAttributes> attributeControls = new List<NewGroupAttributes>();
        private Dictionary<string, int?> _verifiedLocations = null;
        private Dictionary<Guid, string> _alternateIds = null;
        private DefinedValueCache _homePhone = null;
        private DefinedValueCache _cellPhone = null;
        private string _smsOption = "False";
        private bool _enableAlternateIdentifier = false;
        private bool _generateAlternateIdentifier = true;
        private bool _areDatePickersValid = true;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index of the current category.
        /// </summary>
        /// <value>
        /// The index of the current category.
        /// </value>
        protected int CurrentPageIndex { get; set; }

        /// <summary>
        /// Gets or sets the group members that have been added by user
        /// </summary>
        /// <value>
        /// The group members.
        /// </value>
        protected List<GroupMember> GroupMembers { get; set; }

        /// <summary>
        /// A dictionary of a list of duplicate person ids for each Person.Guid  Gets or sets any possible duplicates for each group member
        /// </summary>
        /// <value>
        /// The duplicates.
        /// </value>
        protected Dictionary<Guid, int[]> Duplicates { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            CurrentPageIndex = ViewState["CurrentPageIndex"] as int? ?? 0;

            string json = ViewState["GroupMembers"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupMembers = new List<GroupMember>();
            }
            else
            {
                GroupMembers = JsonConvert.DeserializeObject<List<GroupMember>>( json );
            }

            json = ViewState["Duplicates"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                Duplicates = new Dictionary<Guid, int[]>();
            }
            else
            {
                Duplicates = JsonConvert.DeserializeObject<Dictionary<Guid, int[]>>( json );
            }

            _verifiedLocations = ViewState["VerifiedLocations"] as Dictionary<string, int?>;
            _alternateIds = ViewState["AlternateIds"] as Dictionary<Guid, string>;

            CreateControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Tell the browsers to not cache. This will help prevent browser using a stale person guid when the user uses the Back button
            Page.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Page.Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Page.Response.Cache.SetNoStore();

            _groupType = GroupTypeCache.Get( GetAttributeValue( AttributeKey.GroupType ).AsGuid() );
            if ( _groupType == null )
            {
                _groupType = GroupTypeCache.GetFamilyGroupType();
            }

            _groupTypeName = _groupType.Name;
            _isFamilyGroupType = _groupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            _locationType = _groupType.LocationTypeValues.FirstOrDefault( v => v.Guid.Equals( GetAttributeValue( "LocationType" ).AsGuid() ) );
            _isValidLocationType = _locationType != null;
            if ( !_isValidLocationType )
            {
                _locationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
            }

            if ( _isFamilyGroupType )
            {
                divGroupName.Visible = false;

                cpCampus.Required = GetAttributeValue( "RequireCampus" ).AsBoolean( true );
                ;

                var campusi = GetAttributeValue( "ShowInactiveCampuses" ).AsBoolean() ? CampusCache.All() : CampusCache.All( false ).ToList();
                cpCampus.Campuses = campusi;

                dvpMaritalStatus.Visible = true;
                dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
                var adultMaritalStatus = DefinedValueCache.Get( GetAttributeValue( "AdultMaritalStatus" ).AsGuid() );
                if ( adultMaritalStatus != null )
                {
                    dvpMaritalStatus.SetValue( adultMaritalStatus.Id );
                }

                _childRoleId = _groupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                _adultRoleId = _groupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
            }
            else
            {
                divGroupName.Visible = true;
                tbGroupName.Label = _groupTypeName + " Name";
                cpCampus.Visible = false;
                dvpMaritalStatus.Visible = false;
            }

            nfmMembers.ShowGrade = _isFamilyGroupType && GetAttributeValue( "Grade" ) != "None";
            nfmMembers.RequireGender = GetAttributeValue( "Gender" ).AsBoolean();
            nfmMembers.RequireBirthdate = GetAttributeValue( "BirthDate" ).AsBoolean();
            nfmMembers.RequireGrade = GetAttributeValue( "Grade" ).AsBoolean();
            nfmMembers.ShowTitle = GetAttributeValue( "ShowTitle" ).AsBoolean();
            nfmMembers.ShowMiddleName = GetAttributeValue( "ShowMiddleName" ).AsBoolean();
            nfmMembers.ShowSuffix = GetAttributeValue( "ShowSuffix" ).AsBoolean();
            _smsOption = GetAttributeValue( "SMS" );

            lTitle.Text = string.Format( "Add {0}", _groupType.Name ).FormatAsHtmlTitle();

            nfciContactInfo.ShowCellPhoneFirst = GetAttributeValue( "ShowCellPhoneNumberFirst" ).AsBoolean();
            nfciContactInfo.IsMessagingVisible = string.IsNullOrWhiteSpace( _smsOption ) || _smsOption != "None";

            acAddress.Visible = _isValidLocationType;
            acAddress.Required = _isValidLocationType && GetAttributeValue( "Address" ) == "REQUIRE";
            acAddress.ShowCounty = GetAttributeValue( "ShowCounty" ).AsBoolean();

            cbHomeless.Visible = _isValidLocationType && GetAttributeValue( "Address" ) == "HOMELESS";

            _homePhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
            _cellPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

            _enableAlternateIdentifier = GetAttributeValue( "EnableAlternateIdentifier" ).AsBooleanOrNull() ?? false;
            _generateAlternateIdentifier = GetAttributeValue( "GenerateAlternateIdentifier" ).AsBooleanOrNull() ?? true;

            _confirmMaritalStatus = _isFamilyGroupType && GetAttributeValue( "MaritalStatusConfirmation" ).AsBoolean();
            if ( _confirmMaritalStatus )
            {
                string script = string.Format(
      @"$('a.js-confirm-marital-status').click(function( e ){{
        var anyAdults = false;
        $(""input[id$='_rblRole_0']"").each(function() {{
            if ( $(this).prop('checked') ) {{
                anyAdults = true;
            }}
        }});

        if ( anyAdults ) {{
            if ( $('#{0}').val() == '' ) {{
                e.preventDefault();
                Rock.dialogs.confirm('You have not selected a marital status for the adults in this new family. Are you sure you want to continue?', function (result) {{
                    if (result) {{
                        window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                    }}
                }});
            }}
            }}
      }});",
      dvpMaritalStatus.ClientID );

                ScriptManager.RegisterStartupScript( btnNext, btnNext.GetType(), "confirm-marital-status", script, true );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbValidation.Visible = false;

            if ( !Page.IsPostBack )
            {
                GroupMembers = new List<GroupMember>();
                Duplicates = new Dictionary<Guid, int[]>();
                _verifiedLocations = new Dictionary<string, int?>();
                _alternateIds = new Dictionary<Guid, string>();
                AddGroupMember();
                CreateControls( true );
            }
            else
            {
                GetControlData();
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
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["CurrentPageIndex"] = CurrentPageIndex;
            ViewState["GroupMembers"] = JsonConvert.SerializeObject( GroupMembers, Formatting.None, jsonSetting );
            ViewState["Duplicates"] = JsonConvert.SerializeObject( Duplicates, Formatting.None, jsonSetting );
            ViewState["VerifiedLocations"] = _verifiedLocations;
            ViewState["AlternateIds"] = _alternateIds;

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( _isFamilyGroupType )
            {
                var adults = GroupMembers.Where( m => m.GroupRoleId != _childRoleId ).ToList();
                dvpMaritalStatus.Visible = adults.Any();
            }

            base.OnPreRender( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the AddGroupMemberClick event of the nfmMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void nfmMembers_AddGroupMemberClick( object sender, EventArgs e )
        {
            AddGroupMember();
            CreateControls( true );
        }

        /// <summary>
        /// Handles the RoleUpdated event of the groupMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void groupMemberRow_RoleUpdated( object sender, EventArgs e )
        {
            NewGroupMembersRow row = sender as NewGroupMembersRow;
            row.ShowGradePicker = row.RoleId == _childRoleId;
            row.ShowGradeColumn = _isFamilyGroupType && GetAttributeValue( "Grade" ) != "None";
        }

        /// <summary>
        /// Handles the DeleteClick event of the groupMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void groupMemberRow_DeleteClick( object sender, EventArgs e )
        {
            NewGroupMembersRow row = sender as NewGroupMembersRow;
            var groupMember = GroupMembers.FirstOrDefault( m => m.Person.Guid.Equals( row.PersonGuid ) );
            if ( groupMember != null )
            {
                GroupMembers.Remove( groupMember );
            }

            CreateControls( true );
        }

        /// <summary>
        /// Handles the Click event of the btnPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPrevious_Click( object sender, EventArgs e )
        {
            if ( CurrentPageIndex > 0 )
            {
                CurrentPageIndex--;
                CreateControls( true );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {

            if ( Page.IsValid )
            {
                var errorMessages = new List<string>();

                var people = GroupMembers.Where( m => m.Person != null ).Select( m => m.Person ).ToList();
                var children = GroupMembers.Where( m => m.GroupRoleId == _childRoleId ).Select( m => m.Person ).ToList();
                var adults = GroupMembers.Where( m => m.GroupRoleId == _adultRoleId ).Select( m => m.Person ).ToList();
                if ( CurrentPageIndex == 0 )
                {
                    if ( GetAttributeValue( "Gender" ).AsBoolean() && people.Any( p => p.Gender == Gender.Unknown ) )
                    {
                        errorMessages.Add( "Gender is required for all members." );
                    }

                    if ( GetAttributeValue( "BirthDate" ).AsBoolean() && people.Any( p => !p.BirthDate.HasValue ) )
                    {
                        errorMessages.Add( "A valid Birthdate is required for all members." );
                    }
                    else if ( GetAttributeValue( "ChildBirthdate" ).AsBoolean() && children.Any( p => !p.BirthDate.HasValue ) )
                    {
                        errorMessages.Add( "A valid Birth Date is required for all children." );
                    }

                    if ( GetAttributeValue( "Grade" ).AsBoolean() && children.Any( p => !p.GraduationYear.HasValue ) )
                    {
                        errorMessages.Add( "Grade is required for all children." );
                    }

                    // In GetControlData() all of the date pickers for each group member are checked (currently only birthday). If any are not a valid date (e.g. 19740110) then this value is false.
                    // The each problem picker will have the "has-error" CSS class which will outline it in read so it can be easily identified and corrected.
                    if ( !_areDatePickersValid )
                    {
                        errorMessages.Add( "Date is not in the correct format." );
                    }

                    if ( _isValidLocationType )
                    {
                        int? locationId = null;
                        string locationKey = GetLocationKey();
                        if ( !string.IsNullOrEmpty( locationKey ) )
                        {
                            if ( _verifiedLocations.ContainsKey( locationKey ) )
                            {
                                locationId = _verifiedLocations[locationKey];
                            }
                            else
                            {
                                using ( var rockContext = new RockContext() )
                                {
                                    var location = new LocationService( rockContext ).Get( acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
                                    locationId = location != null ? location.Id : ( int? ) null;
                                    _verifiedLocations.AddOrIgnore( locationKey, locationId );
                                }
                            }
                        }

                        if ( !locationId.HasValue )
                        {
                            string addressRequired = GetAttributeValue( "Address" );
                            if ( addressRequired == "REQUIRE" )
                            {
                                errorMessages.Add( "Address is required." );
                            }

                            if ( addressRequired == "HOMELESS" && !cbHomeless.Checked )
                            {
                                errorMessages.Add( "Address is required unless the family is homeless." );
                            }
                        }
                    }
                }

                if ( CurrentPageIndex == 1 )
                {
                    if ( GetAttributeValue( "PhoneNumber" ).AsBoolean() && !people.Any( p => p.PhoneNumbers.Any() ) )
                    {
                        errorMessages.Add( "At least one phone number is required." );
                    }
                    else if ( GetAttributeValue( "AdultPhoneNumber" ).AsBoolean() && adults.Any( p => !p.PhoneNumbers.Any() ) )
                    {
                        errorMessages.Add( "At least one phone number is required for all adults." );
                    }
                }

                if ( CurrentPageIndex == 2 && _enableAlternateIdentifier )
                {
                    if ( !( people.All( p => _alternateIds.ContainsKey( p.Guid ) ) && people.All( p => !string.IsNullOrEmpty( _alternateIds[p.Guid] ) ) ) )
                    {
                        errorMessages.Add( "Alternate Id is required." );
                    }

                    int alternateValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
                    var alternateIds = _alternateIds.Select( a => a.Value ).ToList();

                    // Look for duplicates in the local/current set of alternate ids
                    var localDuplicateAlternateIds = alternateIds.GroupBy( x => x )
                      .Where( g => g.Count() > 1 )
                      .Select( y => y.Key )
                      .ToList();
                    if ( localDuplicateAlternateIds.Count > 0 )
                    {
                        errorMessages.Add( string.Format( "Alternate {1} '{0}' cannot be shared by multiple people.",
                            localDuplicateAlternateIds.AsDelimited( "' and '" ),
                            "id".PluralizeIf( localDuplicateAlternateIds.Count > 1 ) ) );
                    }

                    // Look for duplicates among all other existing alternate ids
                    using ( var rockContext = new RockContext() )
                    {
                        var duplicateAlternateIds = new PersonSearchKeyService( rockContext )
                                                .Queryable()
                                                .Where( a => a.SearchTypeValueId == alternateValueId && alternateIds.Contains( a.SearchValue ) )
                                                .Select( a => a.SearchValue )
                                                .Distinct()
                                                .ToList();

                        if ( duplicateAlternateIds.Count == 1 )
                        {
                            errorMessages.Add( string.Format( "Alternate Id '{0}' is already assigned to another person.", duplicateAlternateIds[0] ) );
                        }
                        else if ( duplicateAlternateIds.Count > 0 )
                        {
                            errorMessages.Add( string.Format( "Alternate Ids '{0}' are already assigned to other people.", duplicateAlternateIds.AsDelimited( "', '" ) ) );
                        }
                    }
                }

                if ( errorMessages.Any() )
                {
                    nbValidation.Text = string.Format( "<ul><li>{0}</li></ul>", errorMessages.AsDelimited( "</li><li>" ) );
                    nbValidation.Visible = true;
                }
                else
                {
                    var pnlAttributeCount = _enableAlternateIdentifier ? 1 : 0;
                    if ( CurrentPageIndex < ( attributeControls.Count + 1 + pnlAttributeCount ) )
                    {
                        CurrentPageIndex++;
                        CreateControls( true );
                    }
                    else
                    {
                        if ( GroupMembers.Any() )
                        {
                            if ( CurrentPageIndex == ( attributeControls.Count + 1 + pnlAttributeCount ) && FindDuplicates() )
                            {
                                CurrentPageIndex++;
                                CreateControls( true );
                            }
                            else
                            {
                                var rockContext = new RockContext();

                                Guid? parentGroupGuid = GetAttributeValue( "ParentGroup" ).AsGuidOrNull();
                                int? groupId = null;
                                bool isNewGroup = true;
                                List<Guid> newGroupMemberPersonGuids = GroupMembers.Select( a => a.Person.Guid ).ToList();

                                try
                                {
                                    rockContext.WrapTransaction( () =>
                                    {
                                        int? addToExistingGroupId = hfSelectedGroupAtAddressGroupId.Value.AsIntegerOrNull();

                                        // put them in the selected group if an existing group was selected in the "Detect Groups at Address" prompt
                                        if ( addToExistingGroupId.HasValue )
                                        {
                                            foreach ( var groupMember in GroupMembers )
                                            {
                                                PersonService.AddPersonToGroup( groupMember.Person, true, addToExistingGroupId.Value, groupMember.GroupRoleId, rockContext );
                                            }

                                            groupId = addToExistingGroupId;
                                        }
                                        else
                                        {
                                            Group group;
                                            if ( _isFamilyGroupType )
                                            {
                                                group = GroupService.SaveNewFamily( rockContext, GroupMembers, cpCampus.SelectedValueAsInt(), true );
                                            }
                                            else
                                            {
                                                group = GroupService.SaveNewGroup( rockContext, _groupType.Id, parentGroupGuid, tbGroupName.Text, GroupMembers, null, true );
                                            }

                                            groupId = group.Id;
                                            string locationKey = GetLocationKey();
                                            if ( !string.IsNullOrEmpty( locationKey ) && _verifiedLocations.ContainsKey( locationKey ) )
                                            {
                                                GroupService.AddNewGroupAddress( rockContext, group, _locationType.Guid.ToString(), _verifiedLocations[locationKey] );
                                            }
                                        }

                                        if ( _enableAlternateIdentifier )
                                        {
                                            var personSearchKeyService = new PersonSearchKeyService( rockContext );
                                            int alternateValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
                                            foreach ( var groupMember in GroupMembers )
                                            {
                                                var personSearchKey = new PersonSearchKey();
                                                personSearchKey.SearchValue = _alternateIds[groupMember.Person.Guid];
                                                personSearchKey.PersonAliasId = groupMember.Person.PrimaryAliasId;
                                                personSearchKey.SearchTypeValueId = alternateValueId;
                                                personSearchKeyService.Add( personSearchKey );
                                            }

                                            rockContext.SaveChanges();
                                        }

                                    } );

                                    if ( groupId.HasValue )
                                    {
                                        LaunchWorkflows( groupId.Value, isNewGroup, newGroupMemberPersonGuids );
                                    }

                                    // If a custom PersonDetailPage is specified, navigate to that. Otherwise, just go to ~/Person/{PersonId}
                                    var queryParams = new Dictionary<string, string>();
                                    queryParams.Add( "PersonId", GroupMembers[0].Person.Id.ToString() );
                                    if ( groupId.HasValue )
                                    {
                                        queryParams.Add( "GroupId", groupId.ToString() );
                                    }

                                    var personDetailUrl = LinkedPageUrl( "PersonDetailPage", queryParams );

                                    if ( PageParameter( "ReturnUrl" ).IsNotNullOrWhiteSpace() )
                                    {
                                        string redirectUrl = Server.UrlDecode( PageParameter( "ReturnUrl" ) );

                                        string queryString = string.Empty;
                                        if ( redirectUrl.Contains( "?" ) )
                                        {
                                            queryString = redirectUrl.Split( '?' ).Last();
                                        }
                                        // this gets all the query string key value pairs and replace the existing key if any with new value.
                                        var newQueryString = HttpUtility.ParseQueryString( queryString );
                                        newQueryString.Set( "PersonId", GroupMembers[0].Person.Id.ToString() );
                                        newQueryString.Set( "GroupId", groupId.ToString() );

                                        Response.Redirect( redirectUrl.Split( '?' ).First() + "?" + newQueryString );
                                        Context.ApplicationInstance.CompleteRequest();
                                    }
                                    else if ( !string.IsNullOrWhiteSpace( personDetailUrl ) )
                                    {
                                        NavigateToLinkedPage( "PersonDetailPage", queryParams );
                                    }
                                    else
                                    {
                                        Response.Redirect( string.Format( "~/Person/{0}", GroupMembers[0].Person.Id ), false );
                                    }
                                }
                                catch ( GroupMemberValidationException vex )
                                {
                                    cvGroupMember.IsValid = false;
                                    cvGroupMember.ErrorMessage = vex.Message;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the controls.
        /// </summary>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        private void CreateControls( bool setSelection )
        {
            // Load all the attribute controls
            attributeControls.Clear();
            pnlAttributes.Controls.Clear();
            phDuplicates.Controls.Clear();

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var locationService = new LocationService( rockContext );

            foreach ( string categoryGuid in GetAttributeValue( "AttributeCategories" ).SplitDelimitedValues( false ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( categoryGuid, out guid ) )
                {
                    var category = CategoryCache.Get( guid );
                    if ( category != null )
                    {
                        var attributeControl = new NewGroupAttributes();
                        attributeControl.ClearRows();
                        pnlAttributes.Controls.Add( attributeControl );
                        attributeControls.Add( attributeControl );
                        attributeControl.ID = "groupAttributes_" + category.Id.ToString();
                        attributeControl.CategoryId = category.Id;

                        foreach ( var attribute in attributeService.GetByCategoryId( category.Id, false ) )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                attributeControl.AttributeList.Add( AttributeCache.Get( attribute ) );
                            }
                        }
                    }
                }
            }

            nfmMembers.ClearRows();
            nfciContactInfo.ClearRows();
            nfaiAdvanceInfo.ClearRows();

            var groupMemberService = new GroupMemberService( rockContext );
            int defaultRoleId = _groupType.DefaultGroupRoleId ?? _groupType.Roles.Select( r => r.Id ).FirstOrDefault();

            var location = new Location();
            acAddress.GetValues( location );

            var showTitle = this.GetAttributeValue( "ShowTitle" ).AsBoolean();
            var showMiddleName = this.GetAttributeValue( "ShowMiddleName" ).AsBoolean();
            var showSuffix = this.GetAttributeValue( "ShowSuffix" ).AsBoolean();
            var showGrade = GetAttributeValue( "Grade" ) != "None";

            foreach ( var groupMember in GroupMembers )
            {
                string groupMemberGuidString = groupMember.Person.Guid.ToString().Replace( "-", "_" );

                var groupMemberRow = new NewGroupMembersRow();
                groupMemberRow.GroupTypeId = _groupType.Id;
                nfmMembers.Controls.Add( groupMemberRow );
                groupMemberRow.ID = string.Format( "row_{0}", groupMemberGuidString );
                groupMemberRow.RoleUpdated += groupMemberRow_RoleUpdated;
                groupMemberRow.DeleteClick += groupMemberRow_DeleteClick;
                groupMemberRow.PersonGuid = groupMember.Person.Guid;
                groupMemberRow.RequireGender = nfmMembers.RequireGender;
                groupMemberRow.RequireBirthdate = nfmMembers.RequireBirthdate;
                groupMemberRow.RequireGrade = nfmMembers.RequireGrade;
                groupMemberRow.RoleId = groupMember.GroupRoleId;
                groupMemberRow.ShowTitle = showTitle;
                groupMemberRow.ShowMiddleName = showMiddleName;
                groupMemberRow.ShowSuffix = showSuffix;
                groupMemberRow.ShowGradeColumn = _isFamilyGroupType && showGrade;
                groupMemberRow.ShowGradePicker = groupMember.GroupRoleId == _childRoleId;
                groupMemberRow.ShowNickName = this.GetAttributeValue( "ShowNickName" ).AsBoolean();
                groupMemberRow.ValidationGroup = BlockValidationGroup;

                var contactInfoRow = new NewGroupContactInfoRow();
                nfciContactInfo.Controls.Add( contactInfoRow );
                contactInfoRow.ID = string.Format( "ci_row_{0}", groupMemberGuidString );
                contactInfoRow.PersonGuid = groupMember.Person.Guid;
                contactInfoRow.IsMessagingVisible = string.IsNullOrWhiteSpace( _smsOption ) || _smsOption != "None";
                contactInfoRow.IsMessagingEnabled = _smsOption.AsBoolean();
                contactInfoRow.PersonName = groupMember.Person.FullName;
                contactInfoRow.ShowCellPhoneFirst = this.GetAttributeValue( "ShowCellPhoneNumberFirst" ).AsBoolean();

                if ( _homePhone != null )
                {
                    var homePhoneNumber = groupMember.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == _homePhone.Id ).FirstOrDefault();
                    if ( homePhoneNumber != null )
                    {
                        contactInfoRow.HomePhoneNumber = PhoneNumber.FormattedNumber( homePhoneNumber.CountryCode, homePhoneNumber.Number );
                        contactInfoRow.HomePhoneCountryCode = homePhoneNumber.CountryCode;
                    }
                    else
                    {
                        contactInfoRow.HomePhoneNumber = string.Empty;
                        contactInfoRow.HomePhoneCountryCode = string.Empty;
                    }
                }

                if ( _cellPhone != null )
                {
                    var cellPhoneNumber = groupMember.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == _cellPhone.Id ).FirstOrDefault();
                    if ( cellPhoneNumber != null )
                    {
                        contactInfoRow.CellPhoneNumber = PhoneNumber.FormattedNumber( cellPhoneNumber.CountryCode, cellPhoneNumber.Number );
                        contactInfoRow.CellPhoneCountryCode = cellPhoneNumber.CountryCode;
                        contactInfoRow.IsMessagingEnabled = cellPhoneNumber.IsMessagingEnabled;
                    }
                    else
                    {
                        contactInfoRow.CellPhoneNumber = string.Empty;
                        contactInfoRow.CellPhoneCountryCode = string.Empty;
                    }
                }

                contactInfoRow.Email = groupMember.Person.Email;

                if ( _enableAlternateIdentifier )
                {
                    var advanceInfoRow = new NewGroupAdvanceInfoRow();
                    nfaiAdvanceInfo.Controls.Add( advanceInfoRow );
                    advanceInfoRow.ID = string.Format( "ai_row_{0}", groupMemberGuidString );
                    advanceInfoRow.PersonGuid = groupMember.Person.Guid;
                    advanceInfoRow.PersonName = groupMember.Person.FullName;
                    if ( _alternateIds.ContainsKey( groupMember.Person.Guid ) && !string.IsNullOrEmpty( _alternateIds[groupMember.Person.Guid] ) )
                    {
                        advanceInfoRow.AlternateId = _alternateIds[groupMember.Person.Guid];
                    }
                    else
                    {
                        if ( _generateAlternateIdentifier )
                        {
                            advanceInfoRow.AlternateId = PersonSearchKeyService.GenerateRandomAlternateId( true );
                        }
                    }
                }

                if ( setSelection )
                {
                    if ( groupMember.Person != null )
                    {
                        groupMemberRow.TitleValueId = groupMember.Person.TitleValueId;
                        groupMemberRow.FirstName = groupMember.Person.FirstName;
                        groupMemberRow.MiddleName = groupMember.Person.MiddleName;
                        groupMemberRow.NickName = groupMember.Person.NickName;
                        groupMemberRow.LastName = groupMember.Person.LastName;
                        groupMemberRow.SuffixValueId = groupMember.Person.SuffixValueId;
                        groupMemberRow.Gender = groupMember.Person.Gender;
                        groupMemberRow.BirthDate = groupMember.Person.BirthDate;
                        groupMemberRow.ConnectionStatusValueId = groupMember.Person.ConnectionStatusValueId;
                        groupMemberRow.GradeOffset = groupMember.Person.GradeOffset;
                    }
                }

                foreach ( var attributeControl in attributeControls )
                {
                    var attributeRow = new NewGroupAttributesRow();
                    attributeControl.Controls.Add( attributeRow );
                    attributeRow.ID = string.Format( "{0}_{1}", attributeControl.ID, groupMemberGuidString );
                    attributeRow.AttributeList = attributeControl.AttributeList;
                    attributeRow.PersonGuid = groupMember.Person.Guid;
                    attributeRow.PersonName = groupMember.Person.FullName;

                    if ( setSelection )
                    {
                        attributeRow.SetEditValues( groupMember.Person );
                    }
                }

                // Prompt if there are already groups at the address
                if ( this.GetAttributeValue( AttributeKey.DetectGroupsAlreadyAtTheAddress ).AsBoolean() )
                {
                    ShowGroupsAtAddress();
                }

                // show duplicate person warning
                if ( Duplicates.ContainsKey( groupMember.Person.Guid ) )
                {
                    var dupRow = new HtmlGenericControl( "div" );
                    dupRow.AddCssClass( "row row-duplicate" );
                    dupRow.ID = string.Format( "dupRow_{0}", groupMemberGuidString );
                    phDuplicates.Controls.Add( dupRow );

                    var newPersonCol = new HtmlGenericControl( "div" );
                    newPersonCol.AddCssClass( "col-md-6" );
                    newPersonCol.ID = string.Format( "newPersonCol_{0}", groupMemberGuidString );
                    dupRow.Controls.Add( newPersonCol );

                    newPersonCol.Controls.Add( PersonHtmlPanel(
                        groupMemberGuidString,
                        groupMember.Person,
                        groupMember.GroupRole,
                        location,
                        rockContext ) );

                    LinkButton lbRemoveMember = new LinkButton();
                    lbRemoveMember.ID = string.Format( "lbRemoveMember_{0}", groupMemberGuidString );
                    lbRemoveMember.AddCssClass( "btn btn-danger btn-xs" );
                    lbRemoveMember.Text = "Do Not Add Individual";
                    lbRemoveMember.Click += lbRemoveMember_Click;
                    newPersonCol.Controls.Add( lbRemoveMember );

                    var dupPersonCol = new HtmlGenericControl( "div" );
                    dupPersonCol.AddCssClass( "col-md-6" );
                    dupPersonCol.ID = string.Format( "dupPersonCol_{0}", groupMemberGuidString );
                    dupRow.Controls.Add( dupPersonCol );

                    var duplicateHeader = new HtmlGenericControl( "h4" );
                    duplicateHeader.InnerText = "Possible Duplicate Records";
                    dupPersonCol.Controls.Add( duplicateHeader );

                    foreach ( var duplicatePersonId in Duplicates[groupMember.Person.Guid] )
                    {
                        GroupTypeRole groupTypeRole = null;
                        Location duplocation = null;
                        Person duplicatePerson = null;

                        var dupGroupMember = groupMemberService.Queryable()
                            .Where( a => a.PersonId == duplicatePersonId )
                            .Where( a => a.Group.GroupTypeId == _groupType.Id )
                            .Select( s => new
                            {
                                s.GroupRole,
                                s.Person,
                                GroupLocation = s.Group.GroupLocations.Where( a => a.GroupLocationTypeValue.Guid.Equals( _locationType.Guid ) ).Select( a => a.Location ).FirstOrDefault()
                            } )
                            .AsNoTracking().FirstOrDefault();

                        if ( dupGroupMember != null )
                        {
                            groupTypeRole = dupGroupMember.GroupRole;
                            duplocation = dupGroupMember.GroupLocation;
                            duplicatePerson = dupGroupMember.Person;
                        }

                        dupPersonCol.Controls.Add( PersonHtmlPanel(
                            groupMemberGuidString,
                            duplicatePerson,
                            groupTypeRole,
                            duplocation,
                            rockContext ) );
                    }
                }
            }

            ShowPage();
        }

        /// <summary>
        /// Shows any groups have that have the same address.
        /// </summary>
        private void ShowGroupsAtAddress()
        {
            string locationKey = GetLocationKey();
            pnlAddressInUseWarning.Visible = false;
            lAlreadyInUseWarning.Text = string.Format(
                "This address already has a {0} assigned to it. Select the {0} if you would prefer to add the individuals as new {1}. You may also continue adding the new {0} if you believe this is the correct information.",
                _groupType.GroupTerm.ToLower(), _groupType.GroupMemberTerm.Pluralize().ToLower() );

            if ( !string.IsNullOrWhiteSpace( locationKey ) && _verifiedLocations.ContainsKey( locationKey ) )
            {
                int? locationId = _verifiedLocations[locationKey];
                if ( locationId.HasValue )
                {
                    RockContext rockContext = new RockContext();
                    var groupLocationService = new GroupLocationService( rockContext );

                    var groupsAtLocationList = groupLocationService.Queryable().Where( a =>
                            a.GroupLocationTypeValueId == _locationType.Id
                            && a.Group.GroupTypeId == _groupType.Id
                            && a.Group.IsActive
                            && a.LocationId == locationId )
                            .Select( a => a.Group )
                            .ToList();

                    var maxGroupsAtAddressToDetect = this.GetAttributeValue( AttributeKey.MaxGroupsAtAddressToDetect ).AsInteger();

                    groupsAtLocationList = groupsAtLocationList.Take( maxGroupsAtAddressToDetect ).ToList();

                    if ( groupsAtLocationList.Any() )
                    {
                        pnlAddressInUseWarning.Visible = CurrentPageIndex == 1;
                        var sortedGroupsList = groupsAtLocationList.Select( a => new GroupAtLocationInfo
                        {
                            Id = a.Id,
                            GroupTitle = _isFamilyGroupType ? RockUdfHelper.ufnCrm_GetFamilyTitle( rockContext, null, a.Id, null, true ) : a.Name,
                            GroupLocation = a.GroupLocations.Where( gl => gl.LocationId == locationId ).FirstOrDefault(),
                            GroupMembers = a.Members
                        } ).OrderBy( a => a.GroupMembers.AsQueryable().HeadOfHousehold().LastName ).ToList();

                        rptGroupsAtAddress.DataSource = sortedGroupsList;

                        rptGroupsAtAddress.DataBind();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRemoveMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRemoveMember_Click( object sender, EventArgs e )
        {
            Guid personGuid = ( ( LinkButton ) sender ).ID.Substring( 15 ).Replace( "_", "-" ).AsGuid();
            var groupMember = GroupMembers.Where( f => f.Person.Guid.Equals( personGuid ) ).FirstOrDefault();
            if ( groupMember != null )
            {
                GroupMembers.Remove( groupMember );
                Duplicates.Remove( personGuid );
                if ( !GroupMembers.Any() )
                {
                    AddGroupMember();
                    CurrentPageIndex = 0;
                }

                CreateControls( true );
            }
        }

        /// <summary>
        /// Persons the HTML panel.
        /// </summary>
        /// <param name="groupMemberGuidString">The group member unique identifier string.</param>
        /// <param name="person">The person.</param>
        /// <param name="groupTypeRole">The group type role.</param>
        /// <param name="location">The location.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Panel PersonHtmlPanel(
            string groupMemberGuidString,
            Person person,
            GroupTypeRole groupTypeRole,
            Location location,
            RockContext rockContext )
        {
            var personInfoHtml = new StringBuilder();

            Guid? recordTypeValueGuid = null;
            if ( person.RecordTypeValueId.HasValue )
            {
                recordTypeValueGuid = DefinedValueCache.Get( person.RecordTypeValueId.Value, rockContext ).Guid;
            }

            string personName = string.Format( "{0} <small>(New Record)</small>", person.FullName );
            if ( person.Id > 0 )
            {
                string personUrl = ResolveRockUrl( string.Format( "~/person/{0}", person.Id ) );
                personName = string.Format( "<a href='{0}' target='_blank'>{1}</a>", personUrl, person.FullName );
            }

            personInfoHtml.Append( "<div class='row margin-b-lg'>" );

            // add photo if it's not the new record
            if ( person.Id > 0 )
            {
                personInfoHtml.Append( "<div class='col-md-2'>" );
                if ( person.PhotoId.HasValue )
                {
                    personInfoHtml.AppendFormat(
                        "<img src='{0}' class='img-thumbnail'>",
                        Person.GetPersonPhotoUrl( person, 200, 200 ) );
                }

                personInfoHtml.Append( "</div>" );
            }

            personInfoHtml.Append( "<div class='col-md-10'>" );
            personInfoHtml.AppendFormat( "<h4 class='margin-t-none'>{0}</h4>", personName );

            if ( groupTypeRole != null )
            {
                personInfoHtml.Append( groupTypeRole.Name );
            }

            int? personAge = person.Age;
            if ( personAge.HasValue )
            {
                personInfoHtml.AppendFormat( " <em>({0} yrs old)</em>", personAge.Value );
            }

            var groupMembers = person.GetGroupMembers( _groupType.Id, false, rockContext );
            if ( groupMembers != null && groupMembers.Any() )
            {
                personInfoHtml.AppendFormat(
                    "<p><strong>{0} Members:</strong> {1}",
                    _groupType.Name,
                    groupMembers.Select( m => m.Person.NickName ).ToList().AsDelimited( ", " ) );
            }

            if ( location != null && location.GetFullStreetAddress().IsNotNullOrWhiteSpace() )
            {
                personInfoHtml.AppendFormat( "<p><strong>Address</strong><br/>{0}</p>", location.GetFullStreetAddress().ConvertCrLfToHtmlBr() );
            }

            // Generate the HTML for Email and PhoneNumbers
            if ( !string.IsNullOrWhiteSpace( person.Email ) || person.PhoneNumbers.Any() )
            {
                string emailAndPhoneHtml = "<p class='margin-t-sm'>";
                emailAndPhoneHtml += person.Email;
                string phoneNumberList = string.Empty;
                foreach ( var phoneNumber in person.PhoneNumbers )
                {
                    var phoneType = DefinedValueCache.Get( phoneNumber.NumberTypeValueId ?? 0, rockContext );
                    phoneNumberList += string.Format(
                        "<br>{0} <small>{1}</small>",
                        phoneNumber.IsUnlisted ? "Unlisted" : phoneNumber.NumberFormatted,
                        phoneType != null ? phoneType.Value : string.Empty );
                }

                emailAndPhoneHtml += phoneNumberList + "<p>";

                personInfoHtml.Append( emailAndPhoneHtml );
            }

            personInfoHtml.Append( "</div>" );
            personInfoHtml.Append( "</div>" );

            var dupPersonPnl = new Panel();
            dupPersonPnl.ID = string.Format( "dupPersonPnl_{0}_{1}", groupMemberGuidString, person.Id );
            dupPersonPnl.Controls.Add( new LiteralControl( personInfoHtml.ToString() ) );

            return dupPersonPnl;
        }

        /// <summary>
        /// Gets the control data.
        /// </summary>
        private void GetControlData()
        {
            GroupMembers = new List<GroupMember>();

            int? childMaritalStatusId = null;
            var childMaritalStatus = DefinedValueCache.Get( GetAttributeValue( "ChildMaritalStatus" ).AsGuid() );
            if ( childMaritalStatus != null )
            {
                childMaritalStatusId = childMaritalStatus.Id;
            }

            int? adultMaritalStatusId = dvpMaritalStatus.SelectedValueAsInt();

            int recordTypePersonId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            int recordStatusActiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            foreach ( NewGroupMembersRow row in nfmMembers.GroupMemberRows )
            {
                var groupMember = new GroupMember();
                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                groupMember.Person = new Person();
                groupMember.Person.Guid = row.PersonGuid.Value;
                groupMember.Person.RecordTypeValueId = recordTypePersonId;
                groupMember.Person.RecordStatusValueId = recordStatusActiveId;

                if ( row.RoleId.HasValue )
                {
                    groupMember.GroupRoleId = row.RoleId.Value;

                    if ( _isFamilyGroupType )
                    {
                        if ( groupMember.GroupRoleId == _childRoleId )
                        {
                            groupMember.Person.MaritalStatusValueId = childMaritalStatusId;
                        }
                        else
                        {
                            groupMember.Person.MaritalStatusValueId = adultMaritalStatusId;
                        }
                    }
                    else
                    {
                        groupMember.Person.MaritalStatusValueId = null;
                    }
                }

                groupMember.Person.TitleValueId = row.TitleValueId;
                groupMember.Person.FirstName = row.FirstName.FixCase();
                if ( this.GetAttributeValue( "ShowNickName" ).AsBoolean() && !string.IsNullOrEmpty( row.NickName ) )
                {
                    groupMember.Person.NickName = row.NickName.FixCase();
                }
                else
                {
                    groupMember.Person.NickName = groupMember.Person.FirstName;
                }
                groupMember.Person.MiddleName = row.MiddleName.FixCase();
                groupMember.Person.LastName = row.LastName.FixCase();
                groupMember.Person.SuffixValueId = row.SuffixValueId;
                groupMember.Person.Gender = row.Gender;

                _areDatePickersValid = true;
                var datePickers = row.Controls.OfType<DatePicker>();
                foreach ( DatePicker datePicker in datePickers )
                {
                    DateTime dateTime;
                    if ( datePicker.Text.IsNotNullOrWhiteSpace() && !DateTime.TryParse( datePicker.Text, out dateTime ) )
                    {
                        datePicker.AddCssClass( "has-error" );
                        datePicker.ShowErrorMessage( "Date is not in the correct format." );
                        _areDatePickersValid = false;
                    }
                    else
                    {
                        datePicker.RemoveCssClass( "has-error" );
                    }
                }

                var birthday = row.BirthDate;
                if ( birthday.HasValue )
                {
                    // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                    var today = RockDateTime.Today;
                    while ( birthday.Value.CompareTo( today ) > 0 )
                    {
                        birthday = birthday.Value.AddYears( -100 );
                    }

                    groupMember.Person.BirthMonth = birthday.Value.Month;
                    groupMember.Person.BirthDay = birthday.Value.Day;

                    if ( birthday.Value.Year != DateTime.MinValue.Year )
                    {
                        groupMember.Person.BirthYear = birthday.Value.Year;
                    }
                    else
                    {
                        groupMember.Person.BirthYear = null;
                    }
                }
                else
                {
                    groupMember.Person.SetBirthDate( null );
                }

                groupMember.Person.ConnectionStatusValueId = row.ConnectionStatusValueId;

                if ( _isFamilyGroupType )
                {
                    groupMember.Person.GradeOffset = row.GradeOffset;
                }

                var contactInfoRow = nfciContactInfo.ContactInfoRows.FirstOrDefault( c => c.PersonGuid == row.PersonGuid );
                if ( contactInfoRow != null )
                {
                    string homeNumber = PhoneNumber.CleanNumber( contactInfoRow.HomePhoneNumber );
                    if ( _homePhone != null && !string.IsNullOrWhiteSpace( homeNumber ) )
                    {
                        var homePhoneNumber = new PhoneNumber();
                        homePhoneNumber.NumberTypeValueId = _homePhone.Id;
                        homePhoneNumber.Number = homeNumber;
                        homePhoneNumber.CountryCode = PhoneNumber.CleanNumber( contactInfoRow.HomePhoneCountryCode );
                        homePhoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( homePhoneNumber.CountryCode, homeNumber );
                        groupMember.Person.PhoneNumbers.Add( homePhoneNumber );
                    }

                    string cellNumber = PhoneNumber.CleanNumber( contactInfoRow.CellPhoneNumber );
                    if ( _cellPhone != null && !string.IsNullOrWhiteSpace( cellNumber ) )
                    {
                        var cellPhoneNumber = new PhoneNumber();
                        cellPhoneNumber.NumberTypeValueId = _cellPhone.Id;
                        cellPhoneNumber.Number = cellNumber;
                        cellPhoneNumber.CountryCode = PhoneNumber.CleanNumber( contactInfoRow.CellPhoneCountryCode );
                        cellPhoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( cellPhoneNumber.CountryCode, cellNumber );
                        cellPhoneNumber.IsMessagingEnabled = contactInfoRow.IsMessagingEnabled;
                        groupMember.Person.PhoneNumbers.Add( cellPhoneNumber );
                    }

                    groupMember.Person.Email = contactInfoRow.Email;
                }

                groupMember.Person.IsEmailActive = true;
                groupMember.Person.EmailPreference = EmailPreference.EmailAllowed;

                if ( _enableAlternateIdentifier )
                {
                    var advanceInfoRow = nfaiAdvanceInfo.AdvanceInfoRows.FirstOrDefault( c => c.PersonGuid == row.PersonGuid );
                    if ( advanceInfoRow != null )
                    {
                        _alternateIds.AddOrReplace( advanceInfoRow.PersonGuid.Value, advanceInfoRow.AlternateId );
                    }
                }

                groupMember.Person.LoadAttributes();

                foreach ( var attributeControl in attributeControls )
                {
                    var attributeRow = attributeControl.AttributesRows.FirstOrDefault( r => r.PersonGuid == row.PersonGuid );
                    if ( attributeRow != null )
                    {
                        attributeRow.GetEditValues( groupMember.Person );
                    }
                }

                GroupMembers.Add( groupMember );
            }
        }

        /// <summary>
        /// Gets the location key.
        /// </summary>
        /// <returns></returns>
        private string GetLocationKey()
        {
            var location = new Location();
            acAddress.GetValues( location );
            return location.GetFullStreetAddress().Trim();
        }

        /// <summary>
        /// Adds the group member.
        /// </summary>
        private void AddGroupMember()
        {
            int defaultRoleId = _groupType.DefaultGroupRoleId ?? _groupType.Roles.Select( r => r.Id ).FirstOrDefault();
            int recordTypePersonId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            int recordStatusActiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            var connectionStatusValue = DefinedValueCache.Get( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid() );

            var person = new Person();
            person.Guid = Guid.NewGuid();
            person.RecordTypeValueId = recordTypePersonId;
            person.RecordStatusValueId = recordStatusActiveId;
            person.Gender = Gender.Unknown;
            person.ConnectionStatusValueId = ( connectionStatusValue != null ) ? connectionStatusValue.Id : ( int? ) null;

            var groupMember = new GroupMember();
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
            groupMember.GroupRoleId = defaultRoleId;
            groupMember.Person = person;

            if ( GetAttributeValue( "EnableCommonLastName" ).AsBoolean() )
            {
                if ( GroupMembers.Count > 0 )
                {
                    person.LastName = GroupMembers.FirstOrDefault().Person.LastName;
                }
            }

            GroupMembers.Add( groupMember );
        }

        /// <summary>
        /// Finds the duplicates.
        /// </summary>
        /// <returns></returns>
        public bool FindDuplicates()
        {
            Duplicates = new Dictionary<Guid, int[]>();

            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );
            var groupService = new GroupService( rockContext );
            var personService = new PersonService( rockContext );

            // Find any other group members (any group) that have same location
            var othersAtAddress = new List<int>();

            string locationKey = GetLocationKey();
            if ( !string.IsNullOrWhiteSpace( locationKey ) && _verifiedLocations.ContainsKey( locationKey ) )
            {
                int? locationId = _verifiedLocations[locationKey];
                if ( locationId.HasValue )
                {
                    var location = locationService.Get( locationId.Value );
                    if ( location != null )
                    {
                        othersAtAddress = groupService
                            .Queryable().AsNoTracking()
                            .Where( g =>
                                g.GroupTypeId == _locationType.Id &&
                                g.GroupLocations.Any( l => l.LocationId == location.Id ) )
                            .SelectMany( g => g.Members )
                            .Select( m => m.PersonId )
                            .ToList();
                    }
                }
            }

            foreach ( var person in GroupMembers
                .Where( m =>
                    m.Person != null &&
                    m.Person.FirstName != "" )
                .Select( m => m.Person ) )
            {
                bool otherCriteria = false;
                var personQry = personService
                    .Queryable().AsNoTracking()
                    .Where( p =>
                        p.FirstName == person.FirstName ||
                        p.NickName == person.FirstName );

                if ( othersAtAddress.Any() )
                {
                    personQry = personQry
                        .Where( p => othersAtAddress.Contains( p.Id ) );
                }

                if ( person.BirthDate.HasValue )
                {
                    otherCriteria = true;
                    personQry = personQry
                        .Where( p =>
                            p.BirthDate.HasValue &&
                            p.BirthDate.Value == person.BirthDate.Value );
                }

                if ( _homePhone != null )
                {
                    var homePhoneNumber = person.PhoneNumbers.Where( p => p.NumberTypeValueId == _homePhone.Id ).FirstOrDefault();
                    if ( homePhoneNumber != null )
                    {
                        otherCriteria = true;
                        personQry = personQry
                            .Where( p =>
                                p.PhoneNumbers.Any( n =>
                                    n.NumberTypeValueId == _homePhone.Id &&
                                    n.Number == homePhoneNumber.Number ) );
                    }
                }

                if ( _cellPhone != null )
                {
                    var cellPhoneNumber = person.PhoneNumbers.Where( p => p.NumberTypeValueId == _cellPhone.Id ).FirstOrDefault();
                    if ( cellPhoneNumber != null )
                    {
                        otherCriteria = true;
                        personQry = personQry
                            .Where( p =>
                                p.PhoneNumbers.Any( n =>
                                    n.NumberTypeValueId == _cellPhone.Id &&
                                    n.Number == cellPhoneNumber.Number ) );
                    }
                }

                if ( !string.IsNullOrWhiteSpace( person.Email ) )
                {
                    otherCriteria = true;
                    personQry = personQry
                        .Where( p => p.Email == person.Email );
                }

                var dups = new List<int>();
                if ( otherCriteria )
                {
                    // If a birthday, email, phone, or address was entered, find anyone with same info and same first name
                    dups = personQry.Select( a => a.Id ).ToList();
                }
                else
                {
                    // otherwise find people with same first and last name
                    dups = personQry
                        .Where( p => p.LastName == person.LastName )
                        .Select( a => a.Id ).ToList();
                }

                if ( dups.Any() )
                {
                    Duplicates.Add( person.Guid, dups.ToArray() );
                }
            }

            return Duplicates.Any();
        }

        /// <summary>
        /// Launches the workflows.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="isNewGroup">if set to <c>true</c> [is new group].</param>
        /// <param name="newGroupMemberIds">The new group member ids.</param>
        private void LaunchWorkflows( int groupId, bool isNewGroup, List<Guid> newGroupMemberPersonGuids )
        {
            RockContext rockContext = new RockContext();

            // Launch any workflows
            var workflowService = new WorkflowService( rockContext );

            var personWorkflows = GetAttributeValue( "PersonWorkflows" ).SplitDelimitedValues().AsGuidList();
            var adultWorkflows = GetAttributeValue( "AdultWorkflows" ).SplitDelimitedValues().AsGuidList();
            var childWorkflows = GetAttributeValue( "ChildWorkflows" ).SplitDelimitedValues().AsGuidList();
            var groupWorkflows = GetAttributeValue( "GroupWorkflows" ).SplitDelimitedValues().AsGuidList();

            if ( personWorkflows.Any() || adultWorkflows.Any() || childWorkflows.Any() || groupWorkflows.Any() )
            {
                var group = new GroupService( rockContext ).Get( groupId );
                if ( group != null )
                {
                    foreach ( var groupMember in group.Members.Where( a => newGroupMemberPersonGuids.Contains( a.Person.Guid ) ).ToList() )
                    {
                        foreach ( var workflowType in personWorkflows )
                        {
                            LaunchWorkflow( workflowService, workflowType, groupMember.Person.FullName, groupMember.Person );
                        }

                        if ( _isFamilyGroupType )
                        {
                            if ( groupMember.GroupRoleId == _childRoleId )
                            {
                                foreach ( var workflowType in childWorkflows )
                                {
                                    LaunchWorkflow( workflowService, workflowType, groupMember.Person.FullName, groupMember.Person );
                                }
                            }
                            else
                            {
                                foreach ( var workflowType in adultWorkflows )
                                {
                                    LaunchWorkflow( workflowService, workflowType, groupMember.Person.FullName, groupMember.Person );
                                }
                            }
                        }
                    }

                    foreach ( var workflowType in groupWorkflows )
                    {
                        if ( isNewGroup )
                        {
                            LaunchWorkflow( workflowService, workflowType, group.Name, group );
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="workflowService">The workflow service.</param>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entity">The entity.</param>
        private void LaunchWorkflow( WorkflowService workflowService, Guid workflowTypeGuid, string name, object entity )
        {
            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );
            if ( workflowType != null )
            {
                var workflow = Workflow.Activate( workflowType, name );
                List<string> workflowErrors;
                workflowService.Process( workflow, entity, out workflowErrors );
            }
        }

        /// <summary>
        /// Shows the page.
        /// </summary>
        private void ShowPage()
        {
            pnlGroupData.Visible = CurrentPageIndex == 0;
            pnlContactInfo.Visible = CurrentPageIndex == 1;

            var startAttributePageIndex = 2;
            if ( _enableAlternateIdentifier )
            {
                startAttributePageIndex += 1;
            }

            pnlAdvanceInfo.Visible = _enableAlternateIdentifier && CurrentPageIndex == 2;
            pnlAttributes.Visible = CurrentPageIndex > startAttributePageIndex - 1 && CurrentPageIndex <= attributeControls.Count + startAttributePageIndex - 1;

            bool showDuplicates = ( CurrentPageIndex > attributeControls.Count + startAttributePageIndex - 1 ) && phDuplicates.Controls.Count > 0;

            pnlDuplicateWarning.Visible = showDuplicates;

            attributeControls.ForEach( c => c.Visible = false );
            if ( CurrentPageIndex > startAttributePageIndex - 1 && attributeControls.Count >= ( CurrentPageIndex - ( startAttributePageIndex - 1 ) ) )
            {
                int index = _enableAlternateIdentifier ? CurrentPageIndex - 3 : CurrentPageIndex - 2;
                attributeControls[index].Visible = true;
            }

            if ( _confirmMaritalStatus && CurrentPageIndex == 0 )
            {
                btnNext.AddCssClass( "js-confirm-marital-status" );
            }
            else
            {
                btnNext.RemoveCssClass( "js-confirm-marital-status" );
            }

            btnPrevious.Visible = CurrentPageIndex > 0;
            btnNext.Text = CurrentPageIndex > attributeControls.Count + startAttributePageIndex - 2 ?
                ( showDuplicates ? "Continue With Add" : "Finish" ) : "Next";

            // If no panels are being show, they have cleared all the duplicates. Provide a message confirming this.
            if ( !pnlGroupData.Visible && !pnlContactInfo.Visible && !pnlAttributes.Visible && !pnlDuplicateWarning.Visible && !pnlAdvanceInfo.Visible )
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Success;
                nbMessages.Text = "No more duplicates remain. Select Finish to complete the addition of these individuals.";
            }
        }

        #endregion

        /// <summary>Handles the ItemDataBound event of the rptGroupsAtAddress control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGroupsAtAddress_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.DataItem != null )
            {
                GroupAtLocationInfo groupAtLocationInfo = e.Item.DataItem as GroupAtLocationInfo;
                var groupMembers = groupAtLocationInfo.GroupMembers;

                var lGroupTitle = e.Item.FindControl( "lGroupTitle" ) as Literal;
                lGroupTitle.Text = groupAtLocationInfo.GroupTitle;

                var lGroupLocationHtml = e.Item.FindControl( "lGroupLocationHtml" ) as Literal;
                lGroupLocationHtml.Text = groupAtLocationInfo.GroupLocation.GroupLocationTypeValue.Value + ": " + groupAtLocationInfo.GroupLocation.Location.ToString();

                var rbGroupToUse = e.Item.FindControl( "rbGroupToUse" ) as RockRadioButton;
                if ( rbGroupToUse != null )
                {
                    rbGroupToUse.Attributes["data-groupid"] = groupAtLocationInfo.Id.ToString();
                }

                var sortedGroupMembers = groupMembers.OrderBy( a => a.GroupRole.Order ).ThenBy( a => a.Person.Gender ).ThenBy( a => a.Person.NickName ).ToList();
                string groupMembersHtml = string.Empty;
                foreach ( var groupMember in sortedGroupMembers )
                {
                    string maritalStatusValue = "Unknown Martial Status";
                    if ( groupMember.Person.MaritalStatusValue != null )
                    {
                        maritalStatusValue = groupMember.Person.MaritalStatusValue.Value;
                    }

                    groupMembersHtml += string.Format( "<li>{0}: {1}, {2}, {3}", groupMember.Person.FullName, groupMember.GroupRole, maritalStatusValue, groupMember.Person.Gender.ConvertToString() );
                    if ( groupMember.Person.Age.HasValue )
                    {
                        groupMembersHtml += ", Age " + groupMember.Person.Age.ToString();
                    }

                    groupMembersHtml += "</li>";
                }

                var lGroupMembersHtml = e.Item.FindControl( "lGroupMembersHtml" ) as Literal;
                lGroupMembersHtml.Text = string.Format( "<ul>{0}</ul>", groupMembersHtml );
                if ( ( e.Item.ItemIndex - 1 ) % 3 == 0 )
                {
                    var lNewRowHtml = e.Item.FindControl( "lNewRowHtml" ) as Literal;
                    lNewRowHtml.Text = "</div><div class='row'>";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="Rock.Utility.RockDynamic" />
        public class GroupAtLocationInfo : RockDynamic
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the group location.
            /// </summary>
            /// <value>
            /// The group location.
            /// </value>
            public GroupLocation GroupLocation { get; set; }

            /// <summary>
            /// Gets or sets the group members.
            /// </summary>
            /// <value>
            /// The group members.
            /// </value>
            public ICollection<GroupMember> GroupMembers { get; set; }

            /// <summary>
            /// Gets the group title.
            /// </summary>
            /// <value>
            /// The group title.
            /// </value>
            public string GroupTitle { get; internal set; }
        }
    }
}