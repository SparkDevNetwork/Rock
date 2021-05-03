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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Allows the user to edit their account on a mobile application.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Profile Details" )]
    [Category( "Mobile > Cms" )]
    [Description( "Allows the user to edit their account on a mobile application." )]
    [IconCssClass( "fa fa-user-cog" )]

    #region Block Attributes

    [DefinedValueField(
        "Connection Status",
        Key = AttributeKeys.ConnectionStatus,
        Description = "The connection status to use for new individuals (default = 'Web Prospect'.)",
        DefinedTypeGuid = "2E6540EA-63F0-40FE-BE50-F2A84735E600",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = "368DD475-242C-49C4-A42C-7278BE690CC2",
        Order = 11 )]

    [DefinedValueField(
        "Record Status",
        Key = AttributeKeys.RecordStatus,
        Description = "The record status to use for new individuals (default = 'Pending'.)",
        DefinedTypeGuid = "8522BADD-2871-45A5-81DD-C76DA07E2E7E",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = "283999EC-7346-42E3-B807-BCE9B2BABB49",
        Order = 12 )]

    [BooleanField(
        "Birthdate Show",
        Key = AttributeKeys.BirthDateShow,
        Description = "Determines whether the birthdate field will be available for input.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 0 )]

    [BooleanField(
        "BirthDate Required",
        Key = AttributeKeys.BirthDateRequired,
        Description = "Requires that a birthdate value be entered before allowing the user to register.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 1 )]

    [BooleanField(
        "Campus Show",
        Key = AttributeKeys.CampusShow,
        Description = "Determines whether the campus field will be available for input.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 2 )]

    [BooleanField(
        "Campus Required",
        Key = AttributeKeys.CampusRequired,
        Description = "Requires that a campus value be entered before allowing the user to register.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 3 )]

    [BooleanField(
        "Email Show",
        Key = AttributeKeys.EmailShow,
        Description = "Determines whether the email field will be available for input.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 4 )]

    [BooleanField(
        "Email Required",
        Key = AttributeKeys.EmailRequired,
        Description = "Requires that a email value be entered before allowing the user to register.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 5 )]

    [BooleanField(
        "Mobile Phone Show",
        Key = AttributeKeys.MobilePhoneShow,
        Description = "Determines whether the mobile phone field will be available for input.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 6 )]

    [BooleanField(
        "Mobile Phone Required",
        Key = AttributeKeys.MobilePhoneRequired,
        Description = "Requires that a mobile phone value be entered before allowing the user to register.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 7 )]

    [BooleanField(
        "Address Show",
        Key = AttributeKeys.AddressShow,
        Description = "Determines whether the address field will be available for input.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 8 )]

    [BooleanField(
        "Address Required",
        Key = AttributeKeys.AddressRequired,
        Description = "Requires that a address value be entered before allowing the user to register.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "custommobile",
        Order = 9 )]

    #endregion

    public class ProfileDetails: RockMobileBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the MobileProfileDetails block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The connection status key
            /// </summary>
            public const string ConnectionStatus = "ConnectionStatus";

            /// <summary>
            /// The record status key
            /// </summary>
            public const string RecordStatus = "RecordStatus";

            /// <summary>
            /// The birth date show key
            /// </summary>
            public const string BirthDateShow = "BirthDateShow";

            /// <summary>
            /// The birth date required key
            /// </summary>
            public const string BirthDateRequired = "BirthDateRequired";

            /// <summary>
            /// The campus show key
            /// </summary>
            public const string CampusShow = "CampusShow";

            /// <summary>
            /// The campus required key
            /// </summary>
            public const string CampusRequired = "CampusRequired";

            /// <summary>
            /// The email show key
            /// </summary>
            public const string EmailShow = "EmailShow";

            /// <summary>
            /// The email required key
            /// </summary>
            public const string EmailRequired = "EmailRequired";

            /// <summary>
            /// The mobile phone show key
            /// </summary>
            public const string MobilePhoneShow = "MobilePhoneShow";

            /// <summary>
            /// The mobile phone required key
            /// </summary>
            public const string MobilePhoneRequired = "MobilePhoneRequired";

            /// <summary>
            /// The address show key
            /// </summary>
            public const string AddressShow = "AddressShow";

            /// <summary>
            /// The address required key
            /// </summary>
            public const string AddressRequired = "AddressRequired";
        }

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.ProfileDetails";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
            };
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Updates the user's profile.
        /// </summary>
        /// <param name="profile">The new profile data.</param>
        /// <returns>A full reference to the person.</returns>
        [BlockAction]
        public object UpdateProfile( MobilePerson profile )
        {
            var user = UserLoginService.GetCurrentUser( false );

            if ( user == null )
            {
                return ActionStatusCode( System.Net.HttpStatusCode.Unauthorized );
            }

            var personId = user.PersonId.Value;
            var rockContext = new Data.RockContext();

            var personService = new PersonService( rockContext );
            var phoneNumberService = new PhoneNumberService( rockContext );
            var person = personService.Get( personId );

            person.NickName = person.NickName == person.FirstName ? profile.FirstName : person.NickName;
            person.FirstName = profile.FirstName;
            person.LastName = profile.LastName;
            person.Gender = ( Gender ) profile.Gender;

            if ( GetAttributeValue( AttributeKeys.BirthDateShow ).AsBoolean() )
            {
                person.SetBirthDate( profile.BirthDate?.Date );
            }

            if ( GetAttributeValue( AttributeKeys.CampusShow ).AsBoolean() )
            {
                person.PrimaryFamily.CampusId = profile.CampusGuid.HasValue ? CampusCache.Get( profile.CampusGuid.Value )?.Id : null;
            }

            if ( GetAttributeValue( AttributeKeys.EmailShow ).AsBoolean() )
            {
                person.Email = profile.Email;
            }

            if ( GetAttributeValue( AttributeKeys.MobilePhoneShow ).AsBoolean() )
            {
                int phoneNumberTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

                var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberTypeId );
                if ( phoneNumber == null )
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberTypeId };
                    person.PhoneNumbers.Add( phoneNumber );
                }

                // TODO: What to do with country code?
                phoneNumber.CountryCode = PhoneNumber.CleanNumber( "+1" );
                phoneNumber.Number = PhoneNumber.CleanNumber( profile.MobilePhone );

                if ( string.IsNullOrWhiteSpace( phoneNumber.Number ) )
                {
                    person.PhoneNumbers.Remove( phoneNumber );
                    phoneNumberService.Delete( phoneNumber );
                }
            }

            if ( GetAttributeValue( AttributeKeys.AddressShow ).AsBoolean() )
            {
                var addressTypeGuid = SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();

                var groupLocationService = new GroupLocationService( rockContext );

                var dvHomeAddressType = DefinedValueCache.Get( addressTypeGuid );
                var familyAddress = groupLocationService.Queryable().Where( l => l.GroupId == person.PrimaryFamily.Id && l.GroupLocationTypeValueId == dvHomeAddressType.Id ).FirstOrDefault();

                if ( familyAddress != null && string.IsNullOrWhiteSpace( profile.HomeAddress.Street1 ) )
                {
                    // delete the current address
                    groupLocationService.Delete( familyAddress );
                }
                else
                {
                    if ( !string.IsNullOrWhiteSpace( profile.HomeAddress.Street1 ) )
                    {
                        if ( familyAddress == null )
                        {
                            familyAddress = new GroupLocation();
                            groupLocationService.Add( familyAddress );
                            familyAddress.GroupLocationTypeValueId = dvHomeAddressType.Id;
                            familyAddress.GroupId = person.PrimaryFamily.Id;
                            familyAddress.IsMailingLocation = true;
                            familyAddress.IsMappedLocation = true;
                        }
                        else if ( familyAddress.Location.Street1 != profile.HomeAddress.Street1 )
                        {
                            // user clicked move so create a previous address
                            var previousAddress = new GroupLocation();
                            groupLocationService.Add( previousAddress );

                            var previousAddressValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                            if ( previousAddressValue != null )
                            {
                                previousAddress.GroupLocationTypeValueId = previousAddressValue.Id;
                                previousAddress.GroupId = person.PrimaryFamily.Id;

                                Location previousAddressLocation = new Location
                                {
                                    Street1 = familyAddress.Location.Street1,
                                    Street2 = familyAddress.Location.Street2,
                                    City = familyAddress.Location.City,
                                    State = familyAddress.Location.State,
                                    PostalCode = familyAddress.Location.PostalCode,
                                    Country = familyAddress.Location.Country
                                };

                                previousAddress.Location = previousAddressLocation;
                            }
                        }

                        // TODO: ???
                        //familyAddress.IsMailingLocation = cbIsMailingAddress.Checked;
                        //familyAddress.IsMappedLocation = cbIsPhysicalAddress.Checked;

                        familyAddress.Location = new LocationService( rockContext ).Get(
                            profile.HomeAddress.Street1,
                            "",
                            profile.HomeAddress.City,
                            profile.HomeAddress.State,
                            profile.HomeAddress.PostalCode,
                            profile.HomeAddress.Country,
                            person.PrimaryFamily,
                            true );

                        // since there can only be one mapped location, set the other locations to not mapped
                        if ( familyAddress.IsMappedLocation )
                        {
                            var groupLocations = groupLocationService.Queryable()
                                .Where( l => l.GroupId == person.PrimaryFamily.Id && l.Id != familyAddress.Id ).ToList();

                            foreach ( var groupLocation in groupLocations )
                            {
                                groupLocation.IsMappedLocation = false;
                            }
                        }

                        rockContext.SaveChanges();
                    }
                }
            }

            rockContext.SaveChanges();

            var mobilePerson = MobileHelper.GetMobilePerson( person, MobileHelper.GetCurrentApplicationSite() );
            mobilePerson.AuthToken = MobileHelper.GetAuthenticationToken( user.UserName );

            return ActionOk( mobilePerson );
        }

        #endregion
    }
}
