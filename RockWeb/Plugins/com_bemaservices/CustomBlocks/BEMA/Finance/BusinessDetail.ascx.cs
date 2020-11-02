﻿// <copyright>
// Copyright by BEMA Software Services
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
using Rock.Web.UI.Controls;


/*
 * BEMA Modified Core Block ( v10.3.1)
 * Version Number based off of RockVersion.RockHotFixVersion.BemaFeatureVersion
 * 
 * Additional Features:
 * - FE1) Added Ability to add envelope information
 */
namespace RockWeb.Plugins.com_bemaservices.Finance
{
    [DisplayName( "Business Detail" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Displays the details of the given business." )]

    [LinkedPage( "Person Profile Page", "The page used to view the details of a business contact", order: 0 )]
    [LinkedPage( "Communication Page", "The communication page to use for when the business email address is clicked. Leave this blank to use the default.", false, "", "", 1 )]
    /* BEMA.FE1.Start */
    [BooleanField(
        "Show the Envelope Number Fields",
        Key = BemaAttributeKey.ShowEnvelopeFields,
        DefaultValue = "False",
        Category = "BEMA Additional Features" )]
    /* BEMA.FE1.End */

    public partial class BusinessDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        /* BEMA.Start */
        #region Attribute Keys
        private static class BemaAttributeKey
        {
            public const string ShowEnvelopeFields = "ShowEnvelopeFields";
        }

        #endregion
        /* BEMA.End */


        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            dvpRecordStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ).Id;
            dvpReason.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ).Id;

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

            var personService = new PersonService( rockContext );
            Person business = null;

            if ( int.Parse( hfBusinessId.Value ) != 0 )
            {
                business = personService.Get( int.Parse( hfBusinessId.Value ) );
            }

            if ( business == null )
            {
                business = new Person();
                personService.Add( business );
                tbBusinessName.Text = tbBusinessName.Text.FixCase();
            }

            // Business Name
            business.LastName = tbBusinessName.Text;

            // Phone Number
            var businessPhoneTypeId = new DefinedValueService( rockContext ).GetByGuid( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;

            var phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == businessPhoneTypeId );

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
            }
            else
            {
                if ( phoneNumber != null )
                {
                    business.PhoneNumbers.Remove( phoneNumber );
                    new PhoneNumberService( rockContext ).Delete( phoneNumber );
                }
            }

            // Record Type - this is always "business". it will never change.
            business.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            // Record Status
            business.RecordStatusValueId = dvpRecordStatus.SelectedValueAsInt(); ;

            // Record Status Reason
            int? newRecordStatusReasonId = null;
            if ( business.RecordStatusValueId.HasValue && business.RecordStatusValueId.Value == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id )
            {
                newRecordStatusReasonId = dvpReason.SelectedValueAsInt();
            }
            business.RecordStatusReasonValueId = newRecordStatusReasonId;

            // Email
            business.IsEmailActive = true;
            business.Email = tbEmail.Text.Trim();
            business.EmailPreference = rblEmailPreference.SelectedValue.ConvertToEnum<EmailPreference>();

            if ( !business.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            /* BEMA.FE1.Start */
            if ( GetAttributeValue( BemaAttributeKey.ShowEnvelopeFields ).AsBoolean() )
            {
                business.LoadAttributes();
                var attribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER );
                if ( attribute != null )
                {
                    business.SetAttributeValue( attribute.Key, tbEnvelopeNumber.Text );
                    business.SaveAttributeValues();
                    rockContext.SaveChanges();
                }
            }
            /* BEMA.FE1.End */

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                // Add/Update Family Group
                var familyGroupType = GroupTypeCache.GetFamilyGroupType();
                int adultRoleId = familyGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var adultFamilyMember = UpdateGroupMember( business.Id, familyGroupType, business.LastName + " Business", ddlCampus.SelectedValueAsInt(), adultRoleId, rockContext );
                business.GivingGroup = adultFamilyMember.Group;

                // Add/Update Known Relationship Group Type
                var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                int knownRelationshipOwnerRoleId = knownRelationshipGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var knownRelationshipOwner = UpdateGroupMember( business.Id, knownRelationshipGroupType, "Known Relationship", null, knownRelationshipOwnerRoleId, rockContext );

                // Add/Update Implied Relationship Group Type
                var impliedRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_PEER_NETWORK.AsGuid() );
                int impliedRelationshipOwnerRoleId = impliedRelationshipGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_PEER_NETWORK_OWNER.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var impliedRelationshipOwner = UpdateGroupMember( business.Id, impliedRelationshipGroupType, "Implied Relationship", null, impliedRelationshipOwnerRoleId, rockContext );

                rockContext.SaveChanges();

                // Location
                int workLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK ).Id;

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
                    }
                }
                else
                {
                    var newLocation = new LocationService( rockContext ).Get(
                        acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
                    if ( workLocation == null )
                    {
                        workLocation = new GroupLocation();
                        groupLocationService.Add( workLocation );
                        workLocation.GroupId = adultFamilyMember.Group.Id;
                        workLocation.GroupLocationTypeValueId = workLocationTypeId;
                    }
                    workLocation.Location = newLocation;
                    workLocation.IsMailingLocation = true;
                }

                rockContext.SaveChanges();

                hfBusinessId.Value = business.Id.ToString();
            } );

            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "businessId", hfBusinessId.Value );
            NavigateToCurrentPage( queryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int? businessId = hfBusinessId.Value.AsIntegerOrNull();
            if ( businessId.HasValue && businessId > 0 )
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
            dvpReason.Visible = dvpRecordStatus.SelectedValueAsInt() == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
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
                personService.AddContactToBusiness( business.Id, contactId.Value );
                rockContext.SaveChanges();
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
                pdAuditDetails.SetEntity( business, ResolveRockUrl( "~" ) );
            }

            if ( business == null )
            {
                business = new Person { Id = 0, Guid = Guid.NewGuid() };
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
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
                SetHeadingStatusInfo( business );
                var detailsLeft = new DescriptionList();
                detailsLeft.Add( "Business Name", business.LastName );

                if ( business.GivingGroup != null )
                {
                    detailsLeft.Add( "Campus", business.GivingGroup.Campus );
                }

                if ( business.RecordStatusReasonValue != null )
                {
                    detailsLeft.Add( "Record Status Reason", business.RecordStatusReasonValue );
                }

                lDetailsLeft.Text = detailsLeft.Html;

                var detailsRight = new DescriptionList();

                // Get address
                var workLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() );
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

                var workPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                if ( workPhoneType != null )
                {
                    var phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == workPhoneType.Id );
                    if ( phoneNumber != null )
                    {
                        detailsRight.Add( "Phone Number", phoneNumber.ToString() );
                    }
                }

                var communicationLinkedPageValue = this.GetAttributeValue( "CommunicationPage" );
                Rock.Web.PageReference communicationPageReference;
                if ( communicationLinkedPageValue.IsNotNullOrWhiteSpace() )
                {
                    communicationPageReference = new Rock.Web.PageReference( communicationLinkedPageValue );
                }
                else
                {
                    communicationPageReference = null;
                }

                detailsRight.Add( "Email Address", business.GetEmailTag( ResolveRockUrl( "/" ), communicationPageReference ) );

                /* BEMA.FE1.Start */
                if ( GetAttributeValue( BemaAttributeKey.ShowEnvelopeFields ).AsBoolean() )
                {
                    business.LoadAttributes();
                    var attribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER );
                    if( attribute != null )
                    {
                        detailsRight.Add( "Evelope Number", business.GetAttributeValue( attribute.Key ) );
                    }
                }
                /* BEMA.FE1.End */

                lDetailsRight.Text = detailsRight.Html;
            }
        }

        /// <summary>
        /// Sets the heading Status information.
        /// </summary>
        /// <param name="business">The business.</param>
        private void SetHeadingStatusInfo( Person business )
        {
            if ( business.RecordStatusValue != null )
            {
                hlStatus.Text = business.RecordStatusValue.Value;
                if ( business.RecordStatusValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() )
                {
                    hlStatus.LabelType = LabelType.Warning;
                }
                else if ( business.RecordStatusValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() )
                {
                    hlStatus.LabelType = LabelType.Danger;
                }
                else
                {
                    hlStatus.LabelType = LabelType.Success;
                }
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
                var workLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() );
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
                var workPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
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

                dvpRecordStatus.SelectedValue = business.RecordStatusValueId.HasValue ? business.RecordStatusValueId.Value.ToString() : string.Empty;
                dvpReason.SelectedValue = business.RecordStatusReasonValueId.HasValue ? business.RecordStatusReasonValueId.Value.ToString() : string.Empty;
                dvpReason.Visible = business.RecordStatusReasonValueId.HasValue &&
                    business.RecordStatusValueId.Value == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;
            }
            else
            {
                lTitle.Text = ActionTitle.Add( "Business" ).FormatAsHtmlTitle();
            }

            /* BEMA.FE1.Start */
            if ( GetAttributeValue( BemaAttributeKey.ShowEnvelopeFields ).AsBoolean() )
            {
                business.LoadAttributes();
                var attribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER );
                if ( attribute != null )
                {
                    tbEnvelopeNumber.Text = business.GetAttributeValue( attribute.Key );
                }

                tbEnvelopeNumber.Visible = true;
                btnGenerateEnvelopeNumber.Visible = true;
            }
            /* BEMA.FE1.End */

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
            pnlContactList.Visible = !editable;
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
            }

            groupMember.PersonId = businessId;
            groupMember.GroupRoleId = groupRoleId;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            groupMember.Group.GroupTypeId = groupType.Id;
            groupMember.Group.Name = groupName;
            groupMember.Group.CampusId = campusId;

            if ( groupMember.Id == 0)
            {
                groupMemberService.Add( groupMember );
            }

            return groupMember;
        }

        #endregion Internal Methods

		/* BEMA.FE1.Start */
        protected void btnGenerateEnvelopeNumber_Click( object sender, EventArgs e )
        {

            var attribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() );
            var maxEnvelopeNumber = new AttributeValueService( new RockContext() ).Queryable()
                                    .Where( a => a.AttributeId == attribute.Id && a.ValueAsNumeric.HasValue )
                                    .Max( a => ( int? ) a.ValueAsNumeric );
            tbEnvelopeNumber.Text = ( ( maxEnvelopeNumber ?? 0 ) + 1 ).ToString();
        }
		/* BEMA.FE1.End */
    }
}