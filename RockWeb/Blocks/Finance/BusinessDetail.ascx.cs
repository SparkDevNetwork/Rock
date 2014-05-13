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
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given business." )]
    public partial class BusinessDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ) );
            ddlReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ), true );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            gContactList.DataKeyNames = new string[] { "id" };
            gContactList.Actions.ShowAdd = canEdit;
            gContactList.Actions.AddClick += gContactList_AddClick;
            gContactList.GridRebind += gContactList_GridRebind;
            gContactList.IsDeleteEnabled = canEdit;

            mdAddContact.SaveClick += mdAddContact_SaveClick;
            mdAddContact.OnCancelScript = string.Format( "$('#{0}').val('');", hfModalOpen.ClientID );
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
                var rockContext = new RockContext();
                string itemId = PageParameter( "businessId" );

                // Load the Giving Group drop down
                ddlGivingGroup.Items.Clear();
                ddlGivingGroup.Items.Add( new ListItem( None.Text, None.IdValue ) );
                if ( int.Parse( itemId ) > 0 )
                {
                    var businessService = new PersonService( rockContext );
                    var business = businessService.Get( int.Parse( itemId ) );
                    ddlGivingGroup.Items.Add( new ListItem( business.FirstName, business.Id.ToString() ) );
                }

                // Load the Campus drop down
                ddlCampus.Items.Clear();
                ddlCampus.Items.Add( new ListItem( string.Empty, string.Empty ) );
                var campusService = new CampusService( new RockContext() );
                foreach ( Campus campus in campusService.Queryable() )
                {
                    ListItem li = new ListItem( campus.Name, campus.Id.ToString() );
                    ddlCampus.Items.Add( li );
                }

                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "businessId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

            if ( !string.IsNullOrWhiteSpace( hfModalOpen.Value ) )
            {
                mdAddContact.Show();
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            Rock.Data.RockTransactionScope.WrapTransaction( () =>
            {
                var personService = new PersonService( rockContext );
                var changes = new List<string>();
                var business = new Person();
                if ( int.Parse( hfBusinessId.Value ) != 0 )
                {
                    business = personService.Get( int.Parse( hfBusinessId.Value ) );
                }

                // Business Name
                History.EvaluateChange( changes, "First Name", business.FirstName, tbBusinessName.Text );
                business.FirstName = tbBusinessName.Text;

                // Phone Number
                var phoneNumberTypeIds = new List<int>();
                var homePhoneTypeId = new DefinedValueService( rockContext ).GetByGuid( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Id;

                if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
                {
                    var phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == homePhoneTypeId );
                    string oldPhoneNumber = string.Empty;
                    if ( phoneNumber == null )
                    {
                        phoneNumber = new PhoneNumber { NumberTypeValueId = homePhoneTypeId };
                        business.PhoneNumbers.Add( phoneNumber );
                    }
                    else
                    {
                        oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                    }

                    phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                    phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );
                    phoneNumber.IsMessagingEnabled = cbSms.Checked;
                    phoneNumber.IsUnlisted = cbUnlisted.Checked;
                    phoneNumberTypeIds.Add( homePhoneTypeId );

                    History.EvaluateChange(
                        changes,
                        string.Format( "{0} Phone", DefinedValueCache.GetName( homePhoneTypeId ) ),
                        oldPhoneNumber,
                        phoneNumber.NumberFormattedWithCountryCode );
                }

                // Remove any blank numbers
                var phoneNumberService = new PhoneNumberService( rockContext );
                foreach ( var phoneNumber in business.PhoneNumbers
                    .Where( n => n.NumberTypeValueId.HasValue && !phoneNumberTypeIds.Contains( n.NumberTypeValueId.Value ) )
                    .ToList() )
                {
                    History.EvaluateChange(
                        changes,
                        string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumber.NumberTypeValueId ) ),
                        phoneNumber.NumberFormatted,
                        string.Empty );

                    business.PhoneNumbers.Remove( phoneNumber );
                    phoneNumberService.Delete( phoneNumber );
                }

                // Record Type - this is always "business". it will never change.
                business.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

                // Record Status
                int? newRecordStatusId = ddlRecordStatus.SelectedValueAsInt();
                History.EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( business.RecordStatusValueId ), DefinedValueCache.GetName( newRecordStatusId ) );
                business.RecordStatusValueId = newRecordStatusId;

                // Record Status Reason
                int? newRecordStatusReasonId = null;
                if ( business.RecordStatusValueId.HasValue && business.RecordStatusValueId.Value == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id )
                {
                    newRecordStatusReasonId = ddlReason.SelectedValueAsInt();
                }

                History.EvaluateChange( changes, "Record Status Reason", DefinedValueCache.GetName( business.RecordStatusReasonValueId ), DefinedValueCache.GetName( newRecordStatusReasonId ) );
                business.RecordStatusReasonValueId = newRecordStatusReasonId;

                // Email
                business.IsEmailActive = true;
                History.EvaluateChange( changes, "Email", business.Email, tbEmail.Text );
                business.Email = tbEmail.Text.Trim();

                var newEmailPreference = rblEmailPreference.SelectedValue.ConvertToEnum<EmailPreference>();
                History.EvaluateChange( changes, "EmailPreference", business.EmailPreference, newEmailPreference );
                business.EmailPreference = newEmailPreference;

                if ( business.IsValid )
                {
                    if ( rockContext.SaveChanges() > 0 )
                    {
                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                business.Id,
                                changes );
                        }
                    }
                }

                // Group
                var familyGroupType = GroupTypeCache.GetFamilyGroupType();
                var groupService = new GroupService( rockContext );
                var businessGroup = new Group();
                if ( business.GivingGroupId != null )
                {
                    businessGroup = groupService.Get( (int)business.GivingGroupId );
                }

                businessGroup.GroupTypeId = familyGroupType.Id;
                businessGroup.Name = tbBusinessName.Text + " Business";
                businessGroup.CampusId = ddlCampus.SelectedValueAsInt();
                var knownRelationshipGroup = new Group();
                var impliedRelationshipGroup = new Group();
                if ( business.GivingGroupId == null )
                {
                    groupService.Add( businessGroup );

                    // If there isn't a Giving Group then there aren't any other groups.
                    // We also need to add the Known Relationship and Implied Relationship groups for this business.
                    var knownRelationshipGroupTypeId = new GroupTypeService( rockContext ).Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ) ).Id;
                    knownRelationshipGroup.GroupTypeId = knownRelationshipGroupTypeId;
                    knownRelationshipGroup.Name = "Known Relationship";
                    groupService.Add( knownRelationshipGroup );

                    var impliedRelationshipGroupTypeId = new GroupTypeService( rockContext ).Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_IMPLIED_RELATIONSHIPS ) ).Id;
                    impliedRelationshipGroup.GroupTypeId = impliedRelationshipGroupTypeId;
                    impliedRelationshipGroup.Name = "Implied Relationship";
                    groupService.Add( impliedRelationshipGroup );
                }

                rockContext.SaveChanges();

                // Giving Group
                int? newGivingGroupId = ddlGivingGroup.SelectedValueAsId();
                if ( business.GivingGroupId != newGivingGroupId )
                {
                    string oldGivingGroupName = business.GivingGroup != null ? business.GivingGroup.Name : string.Empty;
                    string newGivingGroupName = newGivingGroupId.HasValue ? ddlGivingGroup.Items.FindByValue( newGivingGroupId.Value.ToString() ).Text : string.Empty;
                    History.EvaluateChange( changes, "Giving Group", oldGivingGroupName, newGivingGroupName );
                }

                business.GivingGroup = businessGroup;

                // GroupMember
                var groupMemberService = new GroupMemberService( rockContext );
                int? adultRoleId = new GroupTypeRoleService( rockContext ).Queryable()
                    .Where( r =>
                        r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var groupMember = businessGroup.Members.Where( role => role.GroupRoleId == adultRoleId ).FirstOrDefault();
                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    businessGroup.Members.Add( groupMember );

                    // If we're in here, then this is a new business.
                    // Add the known relationship and implied relationship GroupMember entries.
                    var knownRelationshipGroupMember = new GroupMember();
                    knownRelationshipGroupMember.Person = business;
                    knownRelationshipGroupMember.GroupRoleId = new GroupTypeRoleService( rockContext ).Get( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ).Id;
                    knownRelationshipGroupMember.GroupId = knownRelationshipGroup.Id;
                    knownRelationshipGroup.Members.Add( knownRelationshipGroupMember );

                    var impliedRelationshipGroupMember = new GroupMember();
                    impliedRelationshipGroupMember.Person = business;
                    impliedRelationshipGroupMember.GroupRoleId = new GroupTypeRoleService( rockContext ).Get( Rock.SystemGuid.GroupRole.GROUPROLE_IMPLIED_RELATIONSHIPS_OWNER.AsGuid() ).Id;
                    impliedRelationshipGroupMember.GroupId = impliedRelationshipGroup.Id;
                    impliedRelationshipGroup.Members.Add( impliedRelationshipGroupMember );
                }

                groupMember.Person = business;
                groupMember.GroupRoleId = new GroupTypeRoleService( rockContext ).Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
                groupMember.GroupMemberStatus = GroupMemberStatus.Active;

                // GroupLocation & Location
                var groupLocationService = new GroupLocationService( rockContext );
                var groupLocation = businessGroup.GroupLocations.FirstOrDefault();
                if ( groupLocation == null )
                {
                    groupLocation = new GroupLocation();
                    businessGroup.GroupLocations.Add( groupLocation );
                }

                groupLocation.GroupLocationTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME ).Id;

                var locationService = new LocationService( rockContext );
                var location = groupLocation.Location;
                if ( location == null )
                {
                    location = new Location();
                    groupLocation.Location = location;
                }

                location.Street1 = tbStreet1.Text.Trim();
                location.Street2 = tbStreet2.Text.Trim();
                location.City = tbCity.Text.Trim();
                location.State = ddlState.SelectedValue;
                location.Zip = tbZipCode.Text.Trim();

                rockContext.SaveChanges();

                hfBusinessId.Value = business.Id.ToString();
            } );

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            if ( hfBusinessId.ValueAsInt() != 0 )
            {
                var savedBusiness = new PersonService( rockContext ).Get( hfBusinessId.ValueAsInt() );
                ShowSummary( savedBusiness );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var business = new PersonService( rockContext ).Get( int.Parse( hfBusinessId.Value ) );
            ShowEditDetails( business );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRecordStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRecordStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlReason.Visible = ddlRecordStatus.SelectedValueAsInt() == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
        }

        /// <summary>
        /// Handles the Delete event of the gContactList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gContactList_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var business = personService.Get( int.Parse( hfBusinessId.Value ) );
            var businessContactId = e.RowKeyId;
            var businessContact = personService.Get( businessContactId );

            int? businessContactRoleId = new GroupTypeRoleService( rockContext ).Queryable()
                .Where( r =>
                    r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT ) ) )
                .Select( r => r.Id )
                .FirstOrDefault();

            int? businessRoleId = new GroupTypeRoleService( rockContext ).Queryable()
                .Where( r =>
                    r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS ) ) )
                .Select( r => r.Id )
                .FirstOrDefault();

            // remove the business from the business contact's known relationship group
            var businessContactKnownRelationshipGroup = groupMemberService.Queryable()
                .Where( g =>
                    g.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) &&
                    g.PersonId == businessContact.Id )
                .Select( g => g.Group ).FirstOrDefault();
            var businessGroupMember = businessContactKnownRelationshipGroup.Members.Where( g => g.GroupRoleId == businessRoleId ).FirstOrDefault();
            businessContactKnownRelationshipGroup.Members.Remove( businessGroupMember );
            groupMemberService.Delete( businessGroupMember );

            // remove the business contact from the businesses known relationship group
            var businessKnownRelationshipGroup = groupMemberService.Queryable()
                .Where( g =>
                    g.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) &&
                    g.PersonId == business.Id )
                .Select( g => g.Group ).FirstOrDefault();
            var businessContactGroupMember = businessKnownRelationshipGroup.Members.Where( g => g.GroupRoleId == businessContactRoleId ).FirstOrDefault();
            businessKnownRelationshipGroup.Members.Remove( businessContactGroupMember );
            groupMemberService.Delete( businessContactGroupMember );

            rockContext.SaveChanges();
            BindContactListGrid( business );
        }

        /// <summary>
        /// Handles the GridRebind event of the gContactList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gContactList_GridRebind( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var business = new PersonService( rockContext ).Get( int.Parse( hfBusinessId.Value ) );
            BindContactListGrid( business );
        }

        /// <summary>
        /// Handles the AddClick event of the gContactList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gContactList_AddClick( object sender, EventArgs e )
        {
            ppContact.SetValue( null );
            hfModalOpen.Value = "Yes";
            mdAddContact.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddContact control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void mdAddContact_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var business = personService.Get( int.Parse( hfBusinessId.Value ) );
            var contactId = (int)ppContact.PersonId;
            if ( contactId > 0 )
            {
                int? businessContactRoleId = new GroupTypeRoleService( rockContext ).Queryable()
                    .Where( r =>
                        r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT ) ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                int? businessRoleId = new GroupTypeRoleService( rockContext ).Queryable()
                    .Where( r =>
                        r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS ) ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();

                // get the known relationship group of the business contact
                // add the business as a group member of that group using the group role of GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS
                var businessContact = personService.Get( contactId );
                var contactKnownRelationshipGroup = groupMemberService.Queryable()
                    .Where( g =>
                        g.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) &&
                        g.PersonId == businessContact.Id )
                    .Select( g => g.Group ).FirstOrDefault();
                var groupMember = new GroupMember();
                groupMember.PersonId = int.Parse( hfBusinessId.Value );
                groupMember.GroupRoleId = (int)businessRoleId;
                contactKnownRelationshipGroup.Members.Add( groupMember );
                rockContext.SaveChanges();

                // get the known relationship group of the business
                // add the business contact as a group member of that group using the group role of GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT
                var businessKnownRelationshipGroup = groupMemberService.Queryable()
                    .Where( g =>
                        g.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) &&
                        g.PersonId == business.Id )
                    .Select( g => g.Group ).FirstOrDefault();
                var businessGroupMember = new GroupMember();
                businessGroupMember.PersonId = businessContact.Id;
                businessGroupMember.GroupRoleId = (int)businessContactRoleId;
                businessKnownRelationshipGroup.Members.Add( businessGroupMember );
                rockContext.SaveChanges();
            }

            mdAddContact.Hide();
            hfModalOpen.Value = string.Empty;
            BindContactListGrid( business );
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            var rockContext = new RockContext();
            pnlDetails.Visible = false;

            if ( !itemKey.Equals( "businessId" ) )
            {
                return;
            }

            bool editAllowed = true;

            Person business = null;     // A business is a person

            if ( !itemKeyValue.Equals( 0 ) )
            {
                business = new PersonService( rockContext ).Get( itemKeyValue );
                editAllowed = business.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
            else
            {
                business = new Person { Id = 0 };
            }

            if ( business == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfBusinessId.Value = business.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Person.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowSummary( business );
            }
            else
            {
                if ( business.Id > 0 )
                {
                    ShowSummary( business );
                }
                else
                {
                    ShowEditDetails( business );
                }
            }

            BindContactListGrid( business );
        }

        /// <summary>
        /// Shows the summary.
        /// </summary>
        /// <param name="business">The business.</param>
        private void ShowSummary( Person business )
        {
            SetEditMode( false );
            hfBusinessId.SetValue( business.Id );
            lTitle.Text = "View Business".FormatAsHtmlTitle();

            lDetailsLeft.Text = new DescriptionList()
                .Add( "Business Name", business.FirstName )
                .Add( "Address Line 1", business.GivingGroup.GroupLocations.FirstOrDefault().Location.Street1 )
                .Add( "Address Line 2", business.GivingGroup.GroupLocations.FirstOrDefault().Location.Street2 )
                .Add( "City", business.GivingGroup.GroupLocations.FirstOrDefault().Location.City )
                .Add( "State", business.GivingGroup.GroupLocations.FirstOrDefault().Location.State )
                .Add( "Zip Code", business.GivingGroup.GroupLocations.FirstOrDefault().Location.Zip )
                .Html;

            string phoneNumber = string.Empty;
            if ( business.PhoneNumbers.Count > 0 )
            {
                phoneNumber = business.PhoneNumbers.FirstOrDefault().ToString();
            }
            lDetailsRight.Text = new DescriptionList()
                .Add( "Phone Number", phoneNumber )
                .Add( "Email Address", business.Email )
                .Add( "Giving Group", business.GivingGroup.Name )
                .Add( "Record Status", business.RecordStatusValue )
                .Add( "Record Status Reason", business.RecordStatusReasonValue )
                .Add( "Campus", business.GivingGroup.Campus )
                .Html;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="business">The business.</param>
        private void ShowEditDetails( Person business )
        {
            if ( business.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( business.FullName ).FormatAsHtmlTitle();
                tbBusinessName.Text = business.FirstName;
                tbStreet1.Text = business.GivingGroup.GroupLocations.FirstOrDefault().Location.Street1;
                tbStreet2.Text = business.GivingGroup.GroupLocations.FirstOrDefault().Location.Street2;
                tbCity.Text = business.GivingGroup.GroupLocations.FirstOrDefault().Location.City;
                ddlState.SelectedValue = business.GivingGroup.GroupLocations.FirstOrDefault().Location.State;
                tbZipCode.Text = business.GivingGroup.GroupLocations.FirstOrDefault().Location.Zip;
                if ( business.PhoneNumbers.Count > 0 )
                {
                    pnbPhone.Text = business.PhoneNumbers.FirstOrDefault().ToString();
                    cbSms.Checked = business.PhoneNumbers.FirstOrDefault().IsMessagingEnabled;
                    cbUnlisted.Checked = business.PhoneNumbers.FirstOrDefault().IsUnlisted;
                }
                else
                {
                    pnbPhone.Text = string.Empty;
                    cbSms.Checked = false;
                    cbUnlisted.Checked = false;
                }
                tbEmail.Text = business.Email;
                rblEmailPreference.SelectedValue = business.EmailPreference.ToString();
                ddlCampus.SelectedValue = business.GivingGroup.CampusId.ToString();

                var rockContext = new RockContext();
                var groupMemberService = new GroupMemberService( rockContext );
                var knownRelationshipBusinessGroupMember = groupMemberService.Queryable()
                    .Where( g =>
                        g.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS ) ) &&
                        g.PersonId == business.Id )
                    .FirstOrDefault();

                ddlGivingGroup.SelectedValue = business.Id.ToString();
                ddlRecordStatus.SelectedValue = business.RecordStatusValueId.HasValue ? business.RecordStatusValueId.Value.ToString() : string.Empty;
                ddlReason.SelectedValue = business.RecordStatusReasonValueId.HasValue ? business.RecordStatusReasonValueId.Value.ToString() : string.Empty;
                ddlReason.Visible = business.RecordStatusReasonValueId.HasValue &&
                    business.RecordStatusValueId.Value == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
            }
            else
            {
                lTitle.Text = ActionTitle.Add( "Business" ).FormatAsHtmlTitle();
            }

            SetEditMode( true );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Binds the contact list grid.
        /// </summary>
        /// <param name="business">The business.</param>
        private void BindContactListGrid( Person business )
        {
            // Load up that contact list.
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            int? businessRoleId = new GroupTypeRoleService( rockContext ).Queryable()
                .Where( r =>
                    r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS ) ) )
                .Select( r => r.Id )
                .FirstOrDefault();

            // I want a list of people that are connected to this business/person by the business contact relationship
            List<GroupMember> contactList = new List<GroupMember>();
            if ( business.Members.Count > 0 )
            {
                contactList = business.Members.Where( g => g.GroupRoleId == businessRoleId ).ToList();
                List<Person> personList = new List<Person>();
                foreach ( var contact in contactList )
                {
                    var businessContact = new GroupMemberService( rockContext ).GetInverseRelationship( contact, false, CurrentPersonAlias );
                    personList.Add( businessContact.Person );
                }

                gContactList.DataSource = personList;
                gContactList.DataBind();
            }
            else
            {
                gContactList.Visible = false;
            }
        }

        #endregion Internal Methods
    }
}