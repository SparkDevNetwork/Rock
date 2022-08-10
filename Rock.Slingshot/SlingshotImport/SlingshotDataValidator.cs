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
using System.Collections.Generic;
using Rock.Model;
using Rock.Web.Cache;

// Alias Slingshot.Core namespace to avoid conflict with Rock.Slingshot.*
using SlingshotCore = global::Slingshot.Core;

namespace Rock.Slingshot
{
    class SlingshotDataValidator
    {

        private readonly Dictionary<int, string> campusIdToNameMapper = new Dictionary<int, string>();

        internal bool ValidatePerson( SlingshotCore.Model.Person person, out string errorMessage )
        {
            if ( person.Id == 0 )
            {
                errorMessage = $"Id Missing";
                return false;
            }

            if ( person.FamilyId == 0 )
            {
                errorMessage = $"Family Id Missing";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        internal bool ValidateCampus( SlingshotCore.Model.Person person, out string errorMessage )
        {
            errorMessage = string.Empty;
            bool isCampusEmpty = person.Campus.CampusId == 0 && person.Campus.CampusName.IsNullOrWhiteSpace();
            if ( isCampusEmpty )
            {
                return true;
            }

            if ( person.Campus.CampusId == 0 || person.Campus.CampusName.IsNullOrWhiteSpace() )
            {
                errorMessage = $"Campus Data Incomplete: Please enter the value for both the Campus Id and the Campus Name ";
                return false;
            }

            // ensure campus id is unique to a campus
            string existingCampusName = campusIdToNameMapper.GetValueOrNull( person.Campus.CampusId );
            bool isCampusNameConsistent = existingCampusName == null || person.Campus.CampusName == existingCampusName;
            if ( !isCampusNameConsistent )
            {
                errorMessage = $"Campus Name Inconsistent: Campus ID: {person.Campus.CampusId} is assigned to multiple campuses.";
                return false;
            }
            if ( existingCampusName == null )
            {
                campusIdToNameMapper.Add( person.Campus.CampusId, person.Campus.CampusName );
            }
            return true;
        }

        internal bool ValidateAddress( SlingshotCore.Model.PersonAddress personAddress, out string addressInvalidErrorMessage )
        {
            addressInvalidErrorMessage = string.Empty;
            bool isAddressBlank = personAddress.Street1.IsNullOrWhiteSpace()
                            && personAddress.Street2.IsNullOrWhiteSpace()
                            && personAddress.City.IsNullOrWhiteSpace()
                            && personAddress.State.IsNullOrWhiteSpace()
                            && personAddress.PostalCode.IsNullOrWhiteSpace()
                            && personAddress.Country.IsNullOrWhiteSpace();

            if ( isAddressBlank )
            {
                return true;
            }
            // default the country to the Organization Country if not present
            if ( personAddress.Country.IsNullOrWhiteSpace() )
            {
                personAddress.Country = GlobalAttributesCache.Get().OrganizationCountry;
            }


            Location location = new Location
            {
                Street1 = personAddress.Street1,
                Street2 = personAddress.Street2,
                City = personAddress.City,
                State = personAddress.State,
                PostalCode = personAddress.PostalCode,
                Country = personAddress.Country
            };

            bool isAddressValid = LocationService.ValidateLocationAddressRequirements( location, out string errorMessage );

            if ( !isAddressValid )
            {
                addressInvalidErrorMessage = $"{errorMessage} in Address: {location}";
            }
            return isAddressValid;
        }

        internal bool ValidatePhoneNumber( SlingshotCore.Model.PersonPhone personPhone, out string errorMessage )
        {
            errorMessage = string.Empty;
            if ( string.IsNullOrEmpty( personPhone.PhoneNumber ) )
            {
                errorMessage = $"{personPhone.PhoneType} : The Phone Number is Empty";
                return false;
            }
            bool isPhoneNumberValid = !string.IsNullOrEmpty( PhoneNumber.CleanNumber( personPhone.PhoneNumber ) );
            if ( !isPhoneNumberValid )
            {
                errorMessage = $"{personPhone.PhoneType} : The phone number {personPhone.PhoneNumber} is not valid";
            }
            return isPhoneNumberValid;
        }
    }
}
