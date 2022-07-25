using System.Collections.Generic;
using Rock.Model;
using Rock.Web.Cache;

// Alias Slingshot.Core namespace to avoid conflict with Rock.Slingshot.*
using SlingshotCore = global::Slingshot.Core;

namespace Rock.Slingshot
{
    class SlingshotDataValidator
    {

        private Dictionary<int, string> campusIdToNameMapper = new Dictionary<int, string>();

        internal void ValidatePerson( SlingshotCore.Model.Person person )
        {
            if ( person.Id == 0 )
            {
                throw new UploadedPersonCSVInvalidException( $"Id Missing" );
            }

            if ( person.FamilyId == 0 )
            {
                throw new UploadedPersonCSVInvalidException( $"Family Id Missing" );
            }
        }

        internal void ValidateCampus( SlingshotCore.Model.Person person )
        {
            bool isCampusEmpty = person.Campus.CampusId == 0 && person.Campus.CampusName.IsNullOrWhiteSpace();
            if ( isCampusEmpty )
            {
                return;
            }

            if ( person.Campus.CampusId == 0 || person.Campus.CampusName.IsNullOrWhiteSpace() )
            {
                throw new UploadedPersonCSVInvalidException( $"Campus Data Incomplete: Please enter the value for both the Campus Id and the Campus Name " );
            }

            // ensure campus id is unique to a campus
            string existingCampusName = campusIdToNameMapper.GetValueOrNull( person.Campus.CampusId );
            bool isCampusNameConsistent = existingCampusName == null || person.Campus.CampusName == existingCampusName;
            if ( !isCampusNameConsistent )
            {
                throw new UploadedPersonCSVInvalidException( $"Campus Name Inconsistent: Campus ID: {person.Campus.CampusId} is assigned to multiple campuses." );
            }
            if ( existingCampusName == null )
            {
                campusIdToNameMapper.Add( person.Campus.CampusId, person.Campus.CampusName );
            }
        }

        internal void ValidateAddress( SlingshotCore.Model.PersonAddress personAddress )
        {
            bool isAddressBlank = personAddress.Street1.IsNullOrWhiteSpace()
                            && personAddress.Street2.IsNullOrWhiteSpace()
                            && personAddress.City.IsNullOrWhiteSpace()
                            && personAddress.State.IsNullOrWhiteSpace()
                            && personAddress.PostalCode.IsNullOrWhiteSpace()
                            && personAddress.Country.IsNullOrWhiteSpace();

            if ( isAddressBlank )
            {
                return;
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
            if ( location.Country.IsNullOrWhiteSpace() )
            {
                location.Country = GlobalAttributesCache.Get().OrganizationCountry;
            }
            bool isAddressValid = LocationService.ValidateAddressRequirements( location, out object errorMessage );

            if ( !isAddressValid )
            {
                throw new UploadedPersonCSVInvalidException( $"{errorMessage} in Address: {location}" );
            }
        }
    }
}
