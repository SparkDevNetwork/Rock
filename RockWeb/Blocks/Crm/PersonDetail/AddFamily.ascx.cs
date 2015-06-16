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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Block for adding new families
    /// </summary>
    [DisplayName( "Add Family" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows for adding new families." )]

    [GroupLocationTypeField( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Location Type",
        "The type of location that address should use", false, Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, "", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status",
        "The connection status that should be set by default", false, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR, "", 1 )]
    [BooleanField( "Gender", "Require a gender for each person", "Don't require", "Should Gender be required for each person added?", false, "", 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, "Adult Marital Status", "The default marital status for adults in the family.", false, false, "", "", 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, "Child Marital Status", "The marital status to use for children in the family.", false, false,
        Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE, "", 4 )]
    [BooleanField( "Marital Status Confirmation", "Should user be asked to confirm saving an adults without a marital status?", true, "", 5 )]
    [BooleanField( "Grade", "Require a grade for each child", "Don't require", "Should Grade be required for each child added?", false, "", 6 )]
    [BooleanField("SMS", "SMS is enabled by default", "SMS is not enabled by default", "Should SMS be enabled for cell phone numbers by default", false, "", 7)]
    [AttributeCategoryField( "Attribute Categories", "The Attribute Categories to display attributes from", true, "Rock.Model.Person", false, "", "", 8 )]

    public partial class AddFamily : Rock.Web.UI.RockBlock
    {

        #region Fields

        private bool _confirmMaritalStatus = true;
        private int _childRoleId = 0;
        private List<NewFamilyAttributes> attributeControls = new List<NewFamilyAttributes>();
        private List<GroupMember> _groupMembers = null;
        private DefinedValueCache _homePhone = null;
        private DefinedValueCache _cellPhone = null;
        private bool _SMSEnabled = false;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index of the current category.
        /// </summary>
        /// <value>
        /// The index of the current category.
        /// </value>
        protected int CurrentPageIndex
        {
            get { return ViewState["CurrentPageIndex"] as int? ?? 0; }
            set { ViewState["CurrentPageIndex"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var familyMembers = new List<GroupMember>();
            List<string> jsonStrings = ViewState["FamilyMembers"] as List<string>;
            jsonStrings.ForEach( j => familyMembers.Add( GroupMember.FromJson( j ) ) );
            CreateControls( familyMembers, false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ddlMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ), true );
            var AdultMaritalStatus = DefinedValueCache.Read( GetAttributeValue( "AdultMaritalStatus" ).AsGuid() );
            if ( AdultMaritalStatus != null )
            {
                ddlMaritalStatus.SetValue( AdultMaritalStatus.Id );
            }

            var campusi = CampusCache.All();
            cpCampus.Campuses = campusi;
            cpCampus.Visible = campusi.Any();

            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            if ( familyGroupType != null )
            {
                _childRoleId = familyGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
            }

            nfmMembers.RequireGender = GetAttributeValue( "Gender" ).AsBoolean();
            nfmMembers.RequireGrade = GetAttributeValue( "Grade" ).AsBoolean();
            _SMSEnabled = GetAttributeValue("SMS").AsBoolean();

            lTitle.Text = ( "Add Family" ).FormatAsHtmlTitle();

            _homePhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
            _cellPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

            _confirmMaritalStatus = GetAttributeValue( "MaritalStatusConfirmation" ).AsBoolean();
            if ( _confirmMaritalStatus )
            {
                string script = string.Format( @"
    $('a.js-confirm-marital-status').click(function( e ){{

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
    }});
", ddlMaritalStatus.ClientID );
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

            if ( !Page.IsPostBack )
            {
                CreateControls( new List<GroupMember>(), false );
                AddFamilyMember();
            }
            else
            {
                // Update the name on secondary pages
                if ( CurrentPageIndex == 0 )
                {
                    foreach ( var familyMemberRow in nfmMembers.FamilyMemberRows )
                    {
                        string personName = string.Format( "{0} {1}", familyMemberRow.FirstName, familyMemberRow.LastName );

                        var contactInfoRow = nfciContactInfo.ContactInfoRows.FirstOrDefault( c => c.PersonGuid == familyMemberRow.PersonGuid );
                        if ( contactInfoRow != null )
                        {
                            contactInfoRow.PersonName = personName;
                        }

                        foreach ( var attributeControl in attributeControls )
                        {
                            var attributeRow = attributeControl.AttributesRows.FirstOrDefault( r => r.PersonGuid == familyMemberRow.PersonGuid );
                            if ( attributeRow != null )
                            {
                                attributeRow.PersonName = personName;
                            }
                        }
                    }
                }
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
            if ( _groupMembers == null )
            {
                _groupMembers = GetControlData();
            }

            var groupMembers = new List<string>();
            _groupMembers.ForEach( m => groupMembers.Add( m.ToJson() ) );

            ViewState["FamilyMembers"] = groupMembers;

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( _groupMembers == null )
            {
                _groupMembers = GetControlData();
            }

            var adults = _groupMembers.Where( m => m.GroupRoleId != _childRoleId ).ToList();
            ddlMaritalStatus.Visible = adults.Any();

            base.OnPreRender( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the AddFamilyMemberClick event of the nfmMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void nfmMembers_AddFamilyMemberClick( object sender, EventArgs e )
        {
            AddFamilyMember();
        }

        /// <summary>
        /// Handles the RoleUpdated event of the familyMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void familyMemberRow_RoleUpdated( object sender, EventArgs e )
        {
            NewFamilyMembersRow row = sender as NewFamilyMembersRow;
            row.ShowGrade = row.RoleId == _childRoleId;
        }

        /// <summary>
        /// Handles the DeleteClick event of the familyMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void familyMemberRow_DeleteClick( object sender, EventArgs e )
        {
            NewFamilyMembersRow row = sender as NewFamilyMembersRow;

            var contactInfoRow = nfciContactInfo.ContactInfoRows.FirstOrDefault( c => c.PersonGuid == row.PersonGuid );
            if ( contactInfoRow != null )
            {
                nfciContactInfo.ContactInfoRows.Remove( contactInfoRow );
            }

            foreach ( var attributeControl in attributeControls )
            {
                var attributeRow = attributeControl.AttributesRows.FirstOrDefault( r => r.PersonGuid == row.PersonGuid );
                if ( attributeRow != null )
                {
                    attributeControl.Controls.Remove( attributeRow );
                }
            }

            nfmMembers.Controls.Remove( row );
        }

        protected void btnPrevious_Click( object sender, EventArgs e )
        {
            if ( CurrentPageIndex > 0 )
            {
                CurrentPageIndex--;
                ShowPage( CurrentPageIndex );
            }
        }

        protected void btnNext_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                if ( CurrentPageIndex < ( attributeControls.Count + 1 ) )
                {
                    CurrentPageIndex++;
                    ShowPage( CurrentPageIndex );
                }
                else
                {
                    var familyMembers = GetControlData();
                    if ( familyMembers.Any() )
                    {
                        var rockContext = new RockContext();
                        rockContext.WrapTransaction( () =>
                        {
                            var familyGroup = GroupService.SaveNewFamily( rockContext, familyMembers, cpCampus.SelectedValueAsInt(), true );
                            if ( familyGroup != null )
                            {
                                GroupService.AddNewFamilyAddress( rockContext, familyGroup, GetAttributeValue( "LocationType" ),
                                    acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
                            }
                        } );

                        Response.Redirect( string.Format( "~/Person/{0}", familyMembers[0].Person.Id ), false );
                    }

                }
            }

        }

        #endregion

        #region Methods

        private void CreateControls( List<GroupMember> familyMembers, bool setSelection )
        {
            // Load all the attribute controls
            attributeControls.Clear();
            pnlAttributes.Controls.Clear();

            foreach ( string categoryGuid in GetAttributeValue( "AttributeCategories" ).SplitDelimitedValues( false ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( categoryGuid, out guid ) )
                {
                    var category = CategoryCache.Read( guid );
                    if ( category != null )
                    {
                        var attributeControl = new NewFamilyAttributes();
                        attributeControl.ClearRows();
                        pnlAttributes.Controls.Add( attributeControl );
                        attributeControls.Add( attributeControl );
                        attributeControl.ID = "familyAttributes_" + category.Id.ToString();
                        attributeControl.CategoryId = category.Id;

                        foreach ( var attribute in new AttributeService( new RockContext() ).GetByCategoryId( category.Id ) )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                attributeControl.AttributeList.Add( AttributeCache.Read( attribute ) );
                            }
                        }
                    }
                }
            }

            nfmMembers.ClearRows();
            nfciContactInfo.ClearRows();

            foreach ( var familyMember in familyMembers )
            {
                string familyMemberGuidString = familyMember.Person.Guid.ToString().Replace( "-", "_" );

                var familyMemberRow = new NewFamilyMembersRow();
                nfmMembers.Controls.Add( familyMemberRow );
                familyMemberRow.ID = string.Format( "row_{0}", familyMemberGuidString );
                familyMemberRow.RoleUpdated += familyMemberRow_RoleUpdated;
                familyMemberRow.DeleteClick += familyMemberRow_DeleteClick;
                familyMemberRow.PersonGuid = familyMember.Person.Guid;
                familyMemberRow.RequireGender = nfmMembers.RequireGender;
                familyMemberRow.RequireGrade = nfmMembers.RequireGrade;
                familyMemberRow.RoleId = familyMember.GroupRoleId;
                familyMemberRow.ShowGrade = familyMember.GroupRoleId == _childRoleId;

                var contactInfoRow = new NewFamilyContactInfoRow();
                nfciContactInfo.Controls.Add( contactInfoRow );
                contactInfoRow.ID = string.Format( "ci_row_{0}", familyMemberGuidString );
                contactInfoRow.PersonGuid = familyMember.Person.Guid;
                contactInfoRow.IsMessagingEnabled = _SMSEnabled;

                if ( _homePhone != null )
                {
                    var homePhoneNumber = familyMember.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == _homePhone.Id ).FirstOrDefault();
                    if ( homePhoneNumber != null )
                    {
                        contactInfoRow.HomePhoneNumber = homePhoneNumber.NumberFormatted;
                        contactInfoRow.HomePhoneCountryCode = homePhoneNumber.CountryCode;
                    }
                }

                if ( _cellPhone != null )
                {
                    var cellPhoneNumber = familyMember.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == _cellPhone.Id ).FirstOrDefault();
                    if ( cellPhoneNumber != null )
                    {
                        contactInfoRow.CellPhoneNumber = cellPhoneNumber.NumberFormatted;
                        contactInfoRow.CellPhoneCountryCode = cellPhoneNumber.CountryCode;                     
                    }
                }

                contactInfoRow.Email = familyMember.Person.Email;

                if ( setSelection )
                {
                    if ( familyMember.Person != null )
                    {
                        familyMemberRow.TitleValueId = familyMember.Person.TitleValueId;
                        familyMemberRow.FirstName = familyMember.Person.FirstName;
                        familyMemberRow.LastName = familyMember.Person.LastName;
                        familyMemberRow.SuffixValueId = familyMember.Person.SuffixValueId;
                        familyMemberRow.Gender = familyMember.Person.Gender;
                        familyMemberRow.BirthDate = familyMember.Person.BirthDate;
                        familyMemberRow.ConnectionStatusValueId = familyMember.Person.ConnectionStatusValueId;
                        familyMemberRow.GradeOffset = familyMember.Person.GradeOffset;
                    }
                }

                foreach ( var attributeControl in attributeControls )
                {
                    var attributeRow = new NewFamilyAttributesRow();
                    attributeControl.Controls.Add( attributeRow );
                    attributeRow.ID = string.Format( "{0}_{1}", attributeControl.ID, familyMemberGuidString );
                    attributeRow.AttributeList = attributeControl.AttributeList;
                    attributeRow.PersonGuid = familyMember.Person.Guid;

                    if ( setSelection )
                    {
                        attributeRow.SetEditValues( familyMember.Person );
                    }
                }
            }

            ShowPage( CurrentPageIndex );
        }

        private List<GroupMember> GetControlData()
        {
            var familyMembers = new List<GroupMember>();

            int? childMaritalStatusId = null;
            var childMaritalStatus = DefinedValueCache.Read( GetAttributeValue( "ChildMaritalStatus" ).AsGuid() );
            if ( childMaritalStatus != null )
            {
                childMaritalStatusId = childMaritalStatus.Id;
            }
            int? adultMaritalStatusId = ddlMaritalStatus.SelectedValueAsInt();

            int recordTypePersonId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            int recordStatusActiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            foreach ( NewFamilyMembersRow row in nfmMembers.FamilyMemberRows )
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

                    if ( groupMember.GroupRoleId == _childRoleId )
                    {
                        groupMember.Person.MaritalStatusValueId = childMaritalStatusId;
                    }
                    else
                    {
                        groupMember.Person.MaritalStatusValueId = adultMaritalStatusId;
                    }
                }

                groupMember.Person.TitleValueId = row.TitleValueId;
                groupMember.Person.FirstName = row.FirstName;
                groupMember.Person.NickName = groupMember.Person.FirstName;
                groupMember.Person.LastName = row.LastName;
                groupMember.Person.SuffixValueId = row.SuffixValueId;
                groupMember.Person.Gender = row.Gender;

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
                groupMember.Person.GradeOffset = row.GradeOffset;

                NewFamilyContactInfoRow contactInfoRow = nfciContactInfo.ContactInfoRows.FirstOrDefault( c => c.PersonGuid == row.PersonGuid );
                if ( contactInfoRow != null )
                {
                    string homeNumber = PhoneNumber.CleanNumber( contactInfoRow.HomePhoneNumber );
                    if ( _homePhone != null && !string.IsNullOrWhiteSpace( homeNumber ) )
                    {
                        var homePhoneNumber = new PhoneNumber();
                        homePhoneNumber.NumberTypeValueId = _homePhone.Id;
                        homePhoneNumber.Number = homeNumber;
                        homePhoneNumber.CountryCode = PhoneNumber.CleanNumber( contactInfoRow.HomePhoneCountryCode );
                        groupMember.Person.PhoneNumbers.Add( homePhoneNumber );
                    }

                    string cellNumber = PhoneNumber.CleanNumber( contactInfoRow.CellPhoneNumber );
                    if ( _cellPhone != null && !string.IsNullOrWhiteSpace( cellNumber ) )
                    {
                        var cellPhoneNumber = new PhoneNumber();
                        cellPhoneNumber.NumberTypeValueId = _cellPhone.Id;
                        cellPhoneNumber.Number = cellNumber;
                        cellPhoneNumber.CountryCode = PhoneNumber.CleanNumber( contactInfoRow.CellPhoneCountryCode );
                        cellPhoneNumber.IsMessagingEnabled = contactInfoRow.IsMessagingEnabled;
                        groupMember.Person.PhoneNumbers.Add( cellPhoneNumber );
                    }

                    groupMember.Person.Email = contactInfoRow.Email;
                }
                groupMember.Person.IsEmailActive = true;
                groupMember.Person.EmailPreference = EmailPreference.EmailAllowed;

                groupMember.Person.LoadAttributes();

                foreach ( var attributeControl in attributeControls )
                {
                    NewFamilyAttributesRow attributeRow = attributeControl.AttributesRows.FirstOrDefault( r => r.PersonGuid == row.PersonGuid );
                    if ( attributeRow != null )
                    {
                        attributeRow.GetEditValues( groupMember.Person );
                    }
                }

                familyMembers.Add( groupMember );
            }

            return familyMembers;
        }

        private void AddFamilyMember()
        {
            var rows = nfmMembers.FamilyMemberRows;
            var familyMemberGuid = Guid.NewGuid();
            string familyMemberGuidString = familyMemberGuid.ToString().Replace( "-", "_" );

            var familyMemberRow = new NewFamilyMembersRow();
            nfmMembers.Controls.Add( familyMemberRow );
            familyMemberRow.ID = string.Format( "row_{0}", familyMemberGuidString );
            familyMemberRow.RoleUpdated += familyMemberRow_RoleUpdated;
            familyMemberRow.DeleteClick += familyMemberRow_DeleteClick;
            familyMemberRow.PersonGuid = familyMemberGuid;
            familyMemberRow.Gender = Gender.Unknown;
            familyMemberRow.RequireGender = nfmMembers.RequireGender;
            familyMemberRow.RequireGrade = nfmMembers.RequireGrade;
            familyMemberRow.ValidationGroup = BlockValidationGroup;

            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            if ( familyGroupType != null && familyGroupType.DefaultGroupRoleId.HasValue )
            {
                familyMemberRow.RoleId = familyGroupType.DefaultGroupRoleId;
                familyMemberRow.ShowGrade = familyGroupType.DefaultGroupRoleId == _childRoleId;
            }
            else
            {
                familyMemberRow.ShowGrade = false;
            }

            var ConnectionStatusValue = DefinedValueCache.Read( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid() );
            if ( ConnectionStatusValue != null )
            {
                familyMemberRow.ConnectionStatusValueId = ConnectionStatusValue.Id;
            }

            if ( rows.Count > 0 )
            {
                familyMemberRow.LastName = rows[0].LastName;
            }

            var contactInfoRow = new NewFamilyContactInfoRow();
            nfciContactInfo.Controls.Add( contactInfoRow );
            contactInfoRow.ID = string.Format( "ci_row_{0}", familyMemberGuidString );
            contactInfoRow.PersonGuid = familyMemberGuid;

            foreach ( var attributeControl in attributeControls )
            {
                var attributeRow = new NewFamilyAttributesRow();
                attributeControl.Controls.Add( attributeRow );
                attributeRow.ID = string.Format( "{0}_{1}", attributeControl.ID, familyMemberGuidString );
                attributeRow.AttributeList = attributeControl.AttributeList;
                attributeRow.PersonGuid = familyMemberGuid;
            }
        }

        private void ShowPage( int index )
        {
            pnlFamilyData.Visible = ( index == 0 );
            pnlContactInfo.Visible = ( index == 1 );

            attributeControls.ForEach( c => c.Visible = false );
            if ( index > 1 && attributeControls.Count >= ( index - 1 ) )
            {
                attributeControls[index - 2].Visible = true;
            }

            if ( _confirmMaritalStatus && index == 0)
            {
                btnNext.AddCssClass( "js-confirm-marital-status" );
            }
            else
            {
                btnNext.RemoveCssClass( "js-confirm-marital-status" );
            }

            btnPrevious.Visible = index > 0;
            btnNext.Text = index < ( attributeControls.Count + 1 ) ? "Next" : "Finish";
        }

        #endregion
    }
}