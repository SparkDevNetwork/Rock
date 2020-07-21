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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Allows a person to edit their account information.
    /// </summary>
    [DisplayName( "Account Edit" )]
    [Category( "Security" )]
    [Description( "Allows a person to edit their account information." )]

    #region Block Attributes

    [BooleanField( "Show Address",
        Key = AttributeKey.ShowAddress,
        Description = "Allows hiding the address field.",
        DefaultBooleanValue = false,
        Order = 0 )]

    [BooleanField( "Address Required",
        Key = AttributeKey.AddressRequired,
        Description = "Whether the address is required.",
        DefaultBooleanValue = false,
        Order = 1 )]

    [GroupLocationTypeField( "Location Type",
        Key = AttributeKey.LocationType,
        Description = "The type of location that address should use.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        IsRequired = false,
        Order = 2 )]

    #endregion Block Attributes

    public partial class AccountEdit : RockBlock
    {
        private static class AttributeKey
        {
            public const string ShowAddress = "ShowAddress";
            public const string AddressRequired = "AddressRequired";
            public const string LocationType = "LocationType";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            dvpTitle.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ).Id;
            dvpSuffix.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ).Id;
            string smsScript = @"
    $('.js-sms-number').on('click', function () {
        if ($(this).is(':checked')) {
            $('.js-sms-number').not($(this)).prop('checked', false);
        }
    });
";
            ScriptManager.RegisterStartupScript( rContactInfo, rContactInfo.GetType(), "sms-number-" + BlockId.ToString(), smsScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && CurrentPerson != null )
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbMoved control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMoved_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
            {
                hfStreet1.Value = acAddress.Street1;
                hfStreet2.Value = acAddress.Street2;
                hfCity.Value = acAddress.City;
                hfState.Value = acAddress.State;
                hfPostalCode.Value = acAddress.PostalCode;
                hfCountry.Value = acAddress.Country;

                Location currentAddress = new Location();
                acAddress.GetValues( currentAddress );
                lPreviousAddress.Text = string.Format( "<strong>Previous Address</strong><br />{0}", currentAddress.FormattedHtmlAddress );

                acAddress.Street1 = string.Empty;
                acAddress.Street2 = string.Empty;
                acAddress.PostalCode = string.Empty;
                acAddress.City = string.Empty;

                cbIsMailingAddress.Checked = true;
                cbIsPhysicalAddress.Checked = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            rockContext.WrapTransaction( () =>
            {
                var personService = new PersonService( rockContext );

                var person = personService.Get( CurrentPersonId ?? 0 );
                if ( person != null )
                {
                    int? orphanedPhotoId = null;
                    if ( person.PhotoId != imgPhoto.BinaryFileId )
                    {
                        orphanedPhotoId = person.PhotoId;
                        person.PhotoId = imgPhoto.BinaryFileId;
                    }

                    person.TitleValueId = dvpTitle.SelectedValueAsInt(); ;
                    person.FirstName = tbFirstName.Text;
                    person.NickName = tbNickName.Text;
                    person.LastName = tbLastName.Text;
                    person.SuffixValueId = dvpSuffix.SelectedValueAsInt(); ;

                    var birthMonth = person.BirthMonth;
                    var birthDay = person.BirthDay;
                    var birthYear = person.BirthYear;

                    var birthday = bpBirthDay.SelectedDate;
                    if ( birthday.HasValue )
                    {
                        // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                        var today = RockDateTime.Today;
                        while ( birthday.Value.CompareTo( today ) > 0 )
                        {
                            birthday = birthday.Value.AddYears( -100 );
                        }

                        person.BirthMonth = birthday.Value.Month;
                        person.BirthDay = birthday.Value.Day;
                        if ( birthday.Value.Year != DateTime.MinValue.Year )
                        {
                            person.BirthYear = birthday.Value.Year;
                        }
                        else
                        {
                            person.BirthYear = null;
                        }
                    }
                    else
                    {
                        person.SetBirthDate( null );
                    }

                    person.Gender = rblGender.SelectedValue.ConvertToEnum<Gender>(); ;

                    var phoneNumberTypeIds = new List<int>();

                    bool smsSelected = false;

                    foreach ( RepeaterItem item in rContactInfo.Items )
                    {
                        HiddenField hfPhoneType = item.FindControl( "hfPhoneType" ) as HiddenField;
                        PhoneNumberBox pnbPhone = item.FindControl( "pnbPhone" ) as PhoneNumberBox;
                        CheckBox cbUnlisted = item.FindControl( "cbUnlisted" ) as CheckBox;
                        CheckBox cbSms = item.FindControl( "cbSms" ) as CheckBox;

                        if ( hfPhoneType != null &&
                            pnbPhone != null &&
                            cbSms != null &&
                            cbUnlisted != null )
                        {
                            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
                            {
                                int phoneNumberTypeId;
                                if ( int.TryParse( hfPhoneType.Value, out phoneNumberTypeId ) )
                                {
                                    var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberTypeId );
                                    if ( phoneNumber == null )
                                    {
                                        phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberTypeId };
                                        person.PhoneNumbers.Add( phoneNumber );
                                    }

                                    phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                                    phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );

                                    // Only allow one number to have SMS selected
                                    if ( smsSelected )
                                    {
                                        phoneNumber.IsMessagingEnabled = false;
                                    }
                                    else
                                    {
                                        phoneNumber.IsMessagingEnabled = cbSms.Checked;
                                        smsSelected = cbSms.Checked;
                                    }

                                    phoneNumber.IsUnlisted = cbUnlisted.Checked;
                                    phoneNumberTypeIds.Add( phoneNumberTypeId );
                                }
                            }
                        }
                    }

                    // Remove any blank numbers
                    var phoneNumberService = new PhoneNumberService( rockContext );
                    foreach ( var phoneNumber in person.PhoneNumbers
                        .Where( n => n.NumberTypeValueId.HasValue && !phoneNumberTypeIds.Contains( n.NumberTypeValueId.Value ) )
                        .ToList() )
                    {
                        person.PhoneNumbers.Remove( phoneNumber );
                        phoneNumberService.Delete( phoneNumber );
                    }

                    person.Email = tbEmail.Text.Trim();
                    person.EmailPreference = rblEmailPreference.SelectedValue.ConvertToEnum<EmailPreference>(); ;

                    if ( person.IsValid )
                    {
                        if ( rockContext.SaveChanges() > 0 )
                        {
                            if ( orphanedPhotoId.HasValue )
                            {
                                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                                var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                                if ( binaryFile != null )
                                {
                                    // marked the old images as IsTemporary so they will get cleaned up later
                                    binaryFile.IsTemporary = true;
                                    rockContext.SaveChanges();
                                }
                            }

                            // if they used the ImageEditor, and cropped it, the uncropped file is still in BinaryFile. So clean it up
                            if ( imgPhoto.CropBinaryFileId.HasValue )
                            {
                                if ( imgPhoto.CropBinaryFileId != person.PhotoId )
                                {
                                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                                    var binaryFile = binaryFileService.Get( imgPhoto.CropBinaryFileId.Value );
                                    if ( binaryFile != null && binaryFile.IsTemporary )
                                    {
                                        string errorMessage;
                                        if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                                        {
                                            binaryFileService.Delete( binaryFile );
                                            rockContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }

                        // save address
                        if ( pnlAddress.Visible )
                        {
                            Guid? familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuidOrNull();
                            if ( familyGroupTypeGuid.HasValue )
                            {
                                var familyGroup = new GroupService( rockContext ).Queryable()
                                                .Where( f => f.GroupType.Guid == familyGroupTypeGuid.Value
                                                    && f.Members.Any( m => m.PersonId == person.Id ) )
                                                .FirstOrDefault();
                                if ( familyGroup != null )
                                {
                                    Guid? addressTypeGuid = GetAttributeValue( AttributeKey.LocationType ).AsGuidOrNull();
                                    if ( addressTypeGuid.HasValue )
                                    {
                                        var groupLocationService = new GroupLocationService( rockContext );

                                        var dvHomeAddressType = DefinedValueCache.Get( addressTypeGuid.Value );
                                        var familyAddress = groupLocationService.Queryable().Where( l => l.GroupId == familyGroup.Id && l.GroupLocationTypeValueId == dvHomeAddressType.Id ).FirstOrDefault();
                                        if ( familyAddress != null && string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                                        {
                                            // delete the current address
                                            groupLocationService.Delete( familyAddress );
                                            rockContext.SaveChanges();
                                        }
                                        else
                                        {
                                            if ( !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                                            {
                                                if ( familyAddress == null )
                                                {
                                                    familyAddress = new GroupLocation();
                                                    groupLocationService.Add( familyAddress );
                                                    familyAddress.GroupLocationTypeValueId = dvHomeAddressType.Id;
                                                    familyAddress.GroupId = familyGroup.Id;
                                                    familyAddress.IsMailingLocation = true;
                                                    familyAddress.IsMappedLocation = true;
                                                }
                                                else if ( hfStreet1.Value != string.Empty ) {

                                                    // user clicked move so create a previous address
                                                    var previousAddress = new GroupLocation();
                                                    groupLocationService.Add( previousAddress );

                                                    var previousAddressValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                                                    if ( previousAddressValue  != null )
                                                    {
                                                        previousAddress.GroupLocationTypeValueId = previousAddressValue.Id;
                                                        previousAddress.GroupId = familyGroup.Id;

                                                        Location previousAddressLocation = new Location();
                                                        previousAddressLocation.Street1 = hfStreet1.Value;
                                                        previousAddressLocation.Street2 = hfStreet2.Value;
                                                        previousAddressLocation.City = hfCity.Value;
                                                        previousAddressLocation.State = hfState.Value;
                                                        previousAddressLocation.PostalCode = hfPostalCode.Value;
                                                        previousAddressLocation.Country = hfCountry.Value;

                                                        previousAddress.Location = previousAddressLocation;
                                                    }
                                                }

                                                familyAddress.IsMailingLocation = cbIsMailingAddress.Checked;
                                                familyAddress.IsMappedLocation = cbIsPhysicalAddress.Checked;

                                                var loc = new Location();
                                                acAddress.GetValues( loc );

                                                familyAddress.Location = new LocationService( rockContext ).Get(
                                                    loc.Street1, loc.Street2, loc.City, loc.State, loc.PostalCode, loc.Country, familyGroup, true );

                                                rockContext.SaveChanges();
                                            }
                                        }

                                    }
                                }
                            }
                        }

                        NavigateToParentPage();
                    }
                }
            } );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            RockContext rockContext = new RockContext();

            pnlAddress.Visible = GetAttributeValue( AttributeKey.ShowAddress ).AsBoolean();
            acAddress.Required = GetAttributeValue( AttributeKey.AddressRequired ).AsBoolean();

            var person = CurrentPerson;
            if ( person != null )
            {
                imgPhoto.BinaryFileId = person.PhotoId;
                imgPhoto.NoPictureUrl = Person.GetPersonNoPictureUrl( person, 200, 200 );
                dvpTitle.SelectedValue = person.TitleValueId.HasValue ? person.TitleValueId.Value.ToString() : string.Empty;
                tbFirstName.Text = person.FirstName;
                tbNickName.Text = person.NickName;
                tbLastName.Text = person.LastName;
                dvpSuffix.SelectedValue = person.SuffixValueId.HasValue ? person.SuffixValueId.Value.ToString() : string.Empty;
                bpBirthDay.SelectedDate = person.BirthDate;
                rblGender.SelectedValue = person.Gender.ConvertToString();
                tbEmail.Text = person.Email;
                rblEmailPreference.SelectedValue = person.EmailPreference.ConvertToString( false );

                Guid? locationTypeGuid = GetAttributeValue( AttributeKey.LocationType ).AsGuidOrNull();
                if ( locationTypeGuid.HasValue )
                {
                    var addressTypeDv = DefinedValueCache.Get( locationTypeGuid.Value );

                    // if address type is home enable the move and is mailing/physical
                    if (addressTypeDv.Guid == Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )
                    {
                        lbMoved.Visible = true;
                        cbIsMailingAddress.Visible = true;
                        cbIsPhysicalAddress.Visible = true;
                    } else
                    {
                        lbMoved.Visible = false;
                        cbIsMailingAddress.Visible = false;
                        cbIsPhysicalAddress.Visible = false;
                    }

                    lAddressTitle.Text = addressTypeDv.Value + " Address";

                    var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuidOrNull();

                    if ( familyGroupTypeGuid.HasValue )
                    {
                        var familyGroupType = GroupTypeCache.Get( familyGroupTypeGuid.Value );

                        var familyAddress = new GroupLocationService( rockContext ).Queryable()
                                            .Where( l => l.Group.GroupTypeId == familyGroupType.Id
                                                 && l.GroupLocationTypeValueId == addressTypeDv.Id
                                                 && l.Group.Members.Any( m => m.PersonId == person.Id))
                                            .FirstOrDefault();
                        if ( familyAddress != null )
                        {
                            acAddress.SetValues( familyAddress.Location );

                            cbIsMailingAddress.Checked = familyAddress.IsMailingLocation;
                            cbIsPhysicalAddress.Checked = familyAddress.IsMappedLocation;
                        }
                    }
                }

                var mobilePhoneType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );

                var phoneNumbers = new List<PhoneNumber>();
                var phoneNumberTypes = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
                if ( phoneNumberTypes.DefinedValues.Any() )
                {
                    foreach ( var phoneNumberType in phoneNumberTypes.DefinedValues )
                    {
                        var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberType.Id );
                        if ( phoneNumber == null )
                        {
                            var numberType = new DefinedValue();
                            numberType.Id = phoneNumberType.Id;
                            numberType.Value = phoneNumberType.Value;

                            phoneNumber = new PhoneNumber { NumberTypeValueId = numberType.Id, NumberTypeValue = numberType };
                            phoneNumber.IsMessagingEnabled = mobilePhoneType != null && phoneNumberType.Id == mobilePhoneType.Id;
                        }
                        else
                        {
                            // Update number format, just in case it wasn't saved correctly
                            phoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number );
                        }

                        phoneNumbers.Add( phoneNumber );
                    }

                    rContactInfo.DataSource = phoneNumbers;
                    rContactInfo.DataBind();
                }
            }
        }
    }
}