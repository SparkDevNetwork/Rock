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
using Rock.Attribute;
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

    [LinkedPage( "Person Profile Page", "The page used to view the details of a business contact" )]
    public partial class BusinessDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Base Control Methods

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

            gContactList.DataKeyNames = new string[] { "Id" };
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
                ShowDetail( PageParameter( "businessId" ).AsInteger() );
            }

            if ( !string.IsNullOrWhiteSpace( hfModalOpen.Value ) )
            {
                mdAddContact.Show();
            }
        }

        #endregion Control Methods

        #region Events

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
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            rockContext.WrapTransaction( () =>
            {
                var personService = new PersonService( rockContext );
                var changes = new List<string>();
                Person business = null;

                if ( int.Parse( hfBusinessId.Value ) != 0 )
                {
                    business = personService.Get( int.Parse( hfBusinessId.Value ) );
                }

                if ( business == null )
                {
                    business = new Person();
                    personService.Add( business );
                }

                // Business Name
                History.EvaluateChange( changes, "Last Name", business.LastName, tbBusinessName.Text );
                business.LastName = tbBusinessName.Text;

                // Phone Number
                var businessPhoneTypeId = new DefinedValueService( rockContext ).GetByGuid( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;

                string oldPhoneNumber = string.Empty;
                string newPhoneNumber = string.Empty;

                var phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == businessPhoneTypeId );
                if ( phoneNumber != null )
                {
                    oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                }

                if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
                {
                    if ( phoneNumber == null )
                    {
                        phoneNumber = new PhoneNumber { NumberTypeValueId = businessPhoneTypeId };
                        business.PhoneNumbers.Add( phoneNumber );
                    }
                    phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                    phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );
                    phoneNumber.IsMessagingEnabled = cbSms.Checked;
                    phoneNumber.IsUnlisted = cbUnlisted.Checked;

                    newPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                }
                else
                {
                    if ( phoneNumber != null )
                    {
                        business.PhoneNumbers.Remove( phoneNumber );
                        new PhoneNumberService( rockContext ).Delete( phoneNumber );
                    }
                }

                History.EvaluateChange(
                    changes,
                    string.Format( "{0} Phone", DefinedValueCache.GetName( businessPhoneTypeId ) ),
                    oldPhoneNumber,
                    newPhoneNumber );

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

                // Add/Update Family Group
                var familyGroupType = GroupTypeCache.GetFamilyGroupType();
                int adultRoleId = familyGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var adultFamilyMember = UpdateGroupMember( business.Id, familyGroupType, business.LastName + " Business", ddlCampus.SelectedValueAsInt(), adultRoleId, rockContext );
                business.GivingGroup = adultFamilyMember.Group;

                // Add/Update Known Relationship Group Type
                var knownRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                int knownRelationshipOwnerRoleId = knownRelationshipGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var knownRelationshipOwner = UpdateGroupMember( business.Id, knownRelationshipGroupType, "Known Relationship", null, knownRelationshipOwnerRoleId, rockContext );

                // Add/Update Implied Relationship Group Type
                var impliedRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_IMPLIED_RELATIONSHIPS.AsGuid() );
                int impliedRelationshipOwnerRoleId = impliedRelationshipGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_IMPLIED_RELATIONSHIPS_OWNER.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var impliedRelationshipOwner = UpdateGroupMember( business.Id, impliedRelationshipGroupType, "Implied Relationship", null, impliedRelationshipOwnerRoleId, rockContext );

                rockContext.SaveChanges();

                // Location
                int workLocationTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK ).Id;

                var groupLocationService = new GroupLocationService( rockContext );
                var workLocation = groupLocationService.Queryable( "Location" )
                    .Where( gl =>
                        gl.GroupId == adultFamilyMember.Group.Id &&
                        gl.GroupLocationTypeValueId == workLocationTypeId )
                    .FirstOrDefault();

                if ( string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                {
                    if ( workLocation != null )
                    {
                        groupLocationService.Delete( workLocation );
                        History.EvaluateChange( changes, "Address", workLocation.Location.ToString(), string.Empty );
                    }
                }
                else
                {
                    var oldValue = string.Empty;

                    var newLocation = new LocationService( rockContext ).Get(
                        acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );

                    if ( workLocation != null )
                    {
                        oldValue = workLocation.Location.ToString();
                    }
                    else
                    {
                        workLocation = new GroupLocation();
                        groupLocationService.Add( workLocation );
                        workLocation.GroupId = adultFamilyMember.Group.Id;
                        workLocation.GroupLocationTypeValueId = workLocationTypeId;
                    }
                    workLocation.Location = newLocation;

                    History.EvaluateChange( changes, "Address", oldValue, newLocation.ToString() );
                }

                rockContext.SaveChanges();

                hfBusinessId.Value = business.Id.ToString();
            } );

            ShowSummary( hfBusinessId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int? businessId = hfBusinessId.Value.AsIntegerOrNull();
            if ( businessId.HasValue )
            {
                ShowSummary( businessId.Value );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gContactList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gContactList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "PersonId", e.RowKeyId.ToString() );
            NavigateToLinkedPage( "PersonProfilePage", queryParams );
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
        /// Handles the Delete event of the gContactList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gContactList_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            int? businessId = hfBusinessId.Value.AsIntegerOrNull();
            if ( businessId.HasValue )
            {
                var businessContactId = e.RowKeyId;

                var rockContext = new RockContext();
                var groupMemberService = new GroupMemberService( rockContext );

                Guid businessContact = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT.AsGuid();
                Guid business = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS.AsGuid();
                Guid ownerGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();
                foreach ( var groupMember in groupMemberService.Queryable()
                    .Where( m =>
                        (
                            // The contact person in the business's known relationships
                            m.PersonId == businessContactId &&
                            m.GroupRole.Guid.Equals( businessContact ) &&
                            m.Group.Members.Any( o =>
                                o.PersonId == businessId &&
                                o.GroupRole.Guid.Equals( ownerGuid ) )
                        ) ||
                        (
                            // The business in the person's know relationships
                            m.PersonId == businessId &&
                            m.GroupRole.Guid.Equals( business ) &&
                            m.Group.Members.Any( o =>
                                o.PersonId == businessContactId &&
                                o.GroupRole.Guid.Equals( ownerGuid ) )
                        )
                        ) )
                {
                    groupMemberService.Delete( groupMember );
                }

                rockContext.SaveChanges();

                BindContactListGrid( new PersonService( rockContext ).Get( businessId.Value ) );
            }

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
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var business = personService.Get( int.Parse( hfBusinessId.Value ) );
            int? contactId = ppContact.PersonId;
            if ( contactId.HasValue && contactId.Value > 0 )
            {
                // Get the relationship roles to use
                var knownRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                int businessContactRoleId = knownRelationshipGroupType.Roles
                    .Where( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                int businessRoleId = knownRelationshipGroupType.Roles
                    .Where( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                int ownerRoleId = knownRelationshipGroupType.Roles
                    .Where( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();

                if ( ownerRoleId > 0 && businessContactRoleId > 0 && businessRoleId > 0 )
                {
                    // get the known relationship group of the business contact
                    // add the business as a group member of that group using the group role of GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS
                    var contactKnownRelationshipGroup = groupMemberService.Queryable()
                        .Where( g =>
                            g.GroupRoleId == ownerRoleId &&
                            g.PersonId == contactId.Value )
                        .Select( g => g.Group )
                        .FirstOrDefault();
                    if (contactKnownRelationshipGroup == null)
                    {
                        // In some cases person may not yet have a know relationship group type
                        contactKnownRelationshipGroup = new Group();
                        groupService.Add( contactKnownRelationshipGroup );
                        contactKnownRelationshipGroup.Name = "Known Relationship";
                        contactKnownRelationshipGroup.GroupTypeId = knownRelationshipGroupType.Id;

                        var ownerMember = new GroupMember();
                        ownerMember.PersonId = contactId.Value;
                        ownerMember.GroupRoleId = ownerRoleId;
                        contactKnownRelationshipGroup.Members.Add( ownerMember );
                    }
                    var groupMember = new GroupMember();
                    groupMember.PersonId = int.Parse( hfBusinessId.Value );
                    groupMember.GroupRoleId = businessRoleId;
                    contactKnownRelationshipGroup.Members.Add( groupMember );

                    // get the known relationship group of the business
                    // add the business contact as a group member of that group using the group role of GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT
                    var businessKnownRelationshipGroup = groupMemberService.Queryable()
                        .Where( g =>
                            g.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) &&
                            g.PersonId == business.Id )
                        .Select( g => g.Group ).FirstOrDefault();
                    var businessGroupMember = new GroupMember();
                    businessGroupMember.PersonId = contactId.Value;
                    businessGroupMember.GroupRoleId = businessContactRoleId;
                    businessKnownRelationshipGroup.Members.Add( businessGroupMember );

                    rockContext.SaveChanges();
                }
            }

            mdAddContact.Hide();
            hfModalOpen.Value = string.Empty;
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

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="businessId">The business identifier.</param>
        public void ShowDetail( int businessId )
        {
            var rockContext = new RockContext();

            // Load the Campus drop down
            ddlCampus.Items.Clear();
            ddlCampus.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var campus in CampusCache.All() )
            {
                ListItem li = new ListItem( campus.Name, campus.Id.ToString() );
                ddlCampus.Items.Add( li );
            }

            Person business = null;     // A business is a person

            if ( !businessId.Equals( 0 ) )
            {
                business = new PersonService( rockContext ).Get( businessId );
            }

            if ( business == null )
            {
                business = new Person { Id = 0, Guid = Guid.NewGuid() };
            }

            bool editAllowed = business.IsAuthorized( Authorization.EDIT, CurrentPerson );

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
                ShowSummary( businessId );
            }
            else
            {
                if ( business.Id > 0 )
                {
                    ShowSummary( business.Id );
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
        private void ShowSummary( int businessId )
        {
            SetEditMode( false );
            hfBusinessId.SetValue( businessId );
            lTitle.Text = "View Business".FormatAsHtmlTitle();

            var business = new PersonService( new RockContext() ).Get( businessId );
            if ( business != null )
            {
                lDetailsLeft.Text = new DescriptionList()
                    .Add( "Business Name", business.LastName )
                    .Add( "Campus", business.GivingGroup.Campus )
                    .Add( "Record Status", business.RecordStatusValue )
                    .Add( "Record Status Reason", business.RecordStatusReasonValue )
                    .Html;

                var detailsRight = new DescriptionList();

                // Get address
                var workLocationType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() );
                if ( workLocationType != null )
                {
                    if ( business.GivingGroup != null ) // Giving Group is a shortcut to Family Group for business
                    {
                        var location = business.GivingGroup.GroupLocations
                            .Where( gl => gl.GroupLocationTypeValueId == workLocationType.Id )
                            .Select( gl => gl.Location )
                            .FirstOrDefault();
                        if ( location != null )
                        {
                            detailsRight.Add( "Address", location.GetFullStreetAddress().ConvertCrLfToHtmlBr() );
                        }
                    }
                }

                var workPhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                if ( workPhoneType != null )
                {
                    var phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == workPhoneType.Id );
                    if ( phoneNumber != null )
                    {
                        detailsRight.Add( "Phone Number", phoneNumber.ToString() );
                    }
                }

                lDetailsRight.Text = detailsRight
                    .Add( "Email Address", business.Email )
                    .Html;
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="business">The business.</param>
        private void ShowEditDetails( Person business )
        {
            if ( business.Id > 0 )
            {
                var rockContext = new RockContext();

                lTitle.Text = ActionTitle.Edit( business.FullName ).FormatAsHtmlTitle();
                tbBusinessName.Text = business.LastName;

                // address
                Location location = null;
                var workLocationType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() );
                if ( business.GivingGroup != null )     // Giving group is a shortcut to the family group for business
                {
                    ddlCampus.SelectedValue = business.GivingGroup.CampusId.ToString();

                    location = business.GivingGroup.GroupLocations
                        .Where( gl => gl.GroupLocationTypeValueId == workLocationType.Id )
                        .Select( gl => gl.Location )
                        .FirstOrDefault();
                }
                acAddress.SetValues( location );

                // Phone Number
                var workPhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                PhoneNumber phoneNumber = null;
                if ( workPhoneType != null )
                {
                    phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == workPhoneType.Id );
                }
                if ( phoneNumber != null )
                {
                    pnbPhone.Text = phoneNumber.NumberFormatted;
                    cbSms.Checked = phoneNumber.IsMessagingEnabled;
                    cbUnlisted.Checked = phoneNumber.IsUnlisted;
                }
                else
                {
                    pnbPhone.Text = string.Empty;
                    cbSms.Checked = false;
                    cbUnlisted.Checked = false;
                }

                tbEmail.Text = business.Email;
                rblEmailPreference.SelectedValue = business.EmailPreference.ToString();

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
            gContactList.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Binds the contact list grid.
        /// </summary>
        /// <param name="business">The business.</param>
        private void BindContactListGrid( Person business )
        {
            var personList = new GroupMemberService( new RockContext() ).Queryable()
                .Where( g =>
                    g.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS ) ) &&
                    g.PersonId == business.Id )
                .SelectMany( g => g.Group.Members
                    .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) )
                    .Select( m => m.Person ) )
                .ToList();

            gContactList.DataSource = personList;
            gContactList.DataBind();
        }

        /// <summary>
        /// Updates the group member.
        /// </summary>
        /// <param name="businessId">The business identifier.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private GroupMember UpdateGroupMember( int businessId, GroupTypeCache groupType, string groupName, int? campusId, int groupRoleId, RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );

            GroupMember groupMember = groupMemberService.Queryable( "Group" )
                .Where( m =>
                    m.PersonId == businessId &&
                    m.GroupRoleId == groupRoleId )
                .FirstOrDefault();

            if ( groupMember == null )
            {
                groupMember = new GroupMember();
                groupMember.Group = new Group();
                groupMemberService.Add( groupMember );
            }

            groupMember.PersonId = businessId;
            groupMember.GroupRoleId = groupRoleId;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            groupMember.Group.GroupTypeId = groupType.Id;
            groupMember.Group.Name = groupName;
            groupMember.Group.CampusId = campusId;

            return groupMember;
        }

        #endregion Internal Methods

    }
}