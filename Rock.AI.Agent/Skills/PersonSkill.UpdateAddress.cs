using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

using AngleSharp.Dom;

using DocumentFormat.OpenXml.Spreadsheet;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Utility;
using Rock.Web.Cache;

using Location = Rock.Model.Location;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)

        [Description( "Updates a person's address." )]
        //[AgentUsage( "The phoneTypeValueIdKey must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the appropriate IdKey." )]
        [AgentToolGuid( "D34E7821-36E0-F2BC-4496-7A82E1CE4475" )]
        public IAgentToolResult UpdateAddress(
            string personIdKey,
            string locationTypeValueIdKey,
            string street1 = null,
            string street2 = null,
            string city = null,
            string state = null,
            string postalCode = null,
            string country = null,
            string county = null,
            bool? isMappedLocation = null,
            bool? isMailingLocation = null
        )
        {
            var locationTypeValueId = IdHasher.Instance.GetId( locationTypeValueIdKey );
            var locationTypeValue = DefinedValueCache.Get( locationTypeValueId ?? 0 );

            // Check for valid location type
            if ( !locationTypeValueId.HasValue || locationTypeValue == null )
            {
                var locationTypes = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() )?
                    .LocationTypeValues
                    .Select( dv => new KeyNameResult { Id = dv.Id, Name = dv.Value } )
                    .ToList();

                return Error( "Lookups Required" )
                    .WithContent( locationTypes )
                    .WithHistoryContent( locationTypes )
                    .WithInstructions( "Use the following location types to determine the proper IdKey for the tool." );
            }

            
            using var rockContext = RockApp.Current.CreateRockContext();

            // Load the person to ensure they exist
            var personService = new PersonService( rockContext );
            var person = personService.Get( IdHasher.Instance.GetId( personIdKey ) ?? 0 );

            if ( person == null )
            {
                return Error( "No person could be found with the provided personIdKey." );
            }

            // Add/Update the new address
            var personPhoneService = new PhoneNumberService( rockContext );
            var groupLocation = new GroupLocationService( rockContext )
                .Queryable()
                .Where( gl => gl.GroupId == person.PrimaryFamilyId
                    && gl.GroupLocationTypeValueId == locationTypeValueId.Value )
                .FirstOrDefault();

            if ( groupLocation == null )
            {
                // If no address exists today we should at least have street1, city and postal code
                if ( street1.IsNullOrWhiteSpace() || city.IsNullOrWhiteSpace() || postalCode.IsNullOrWhiteSpace() )
                {
                    return Error( "At minimum, street1, city, and postal code must be provided when adding a new address." );
                }

                // If adding and no state was provided we'll set the state to the global default
                if ( state.IsNullOrWhiteSpace() )
                {
                    var globalAttributesCache = GlobalAttributesCache.Get();
                    state = globalAttributesCache.OrganizationState;
                }

                groupLocation = new GroupLocation
                {
                    GroupId = person.PrimaryFamilyId.Value,
                    GroupLocationTypeValueId = locationTypeValueId
                };

                // If the group location is Home by default set it to mailing location and mapped location.
                if ( locationTypeValue != null && locationTypeValue.Guid == SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )
                {
                    groupLocation.IsMailingLocation = true;
                    groupLocation.IsMappedLocation = true;
                }

                var location = new Location();
                groupLocation.Location = location;
            }

            // If this is a Home address and different from the current address then set the current address to a previous address.
            if ( locationTypeValue.Guid == SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid()
                    && street1 != groupLocation.Location.Street1 )
            {
                groupLocation.Location.LocationTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() ).Id;
                groupLocation.IsMappedLocation = false;
                groupLocation.IsMailingLocation = false;

                groupLocation.Location = new Location();
                groupLocation.IsMailingLocation = true;
                groupLocation.IsMappedLocation = true;
            }

            // Set values
            if ( street1 != null )
            {
                // Blank out street 2 if street1 is a different value
                if (street1 != groupLocation.Location.Street1 )
                {
                    groupLocation.Location.Street2 = null;
                }

                groupLocation.Location.Street1 = street1;
            }

            if ( street2 != null )
            {
                groupLocation.Location.Street2 = street2;
            }

            if ( city != null )
            {
                groupLocation.Location.City = city;
            }

            if ( state != null )
            {
                groupLocation.Location.State = state;
            }

            if ( postalCode != null )
            {
                groupLocation.Location.PostalCode = postalCode;
            }

            if ( country != null )
            {
                groupLocation.Location.Country = country;
            }

            if ( county != null )
            {
                groupLocation.Location.County = county;
            }

            if ( isMappedLocation.HasValue )
            {
                groupLocation.IsMappedLocation = isMappedLocation.Value;
            }

            if ( isMailingLocation.HasValue )
            {
                groupLocation.IsMailingLocation = isMailingLocation.Value;
            }

            rockContext.SaveChanges();

            return Success( $"The {locationTypeValue.Value} address for {person.FullName} has been updated to {groupLocation.Location.GetFullStreetAddress()}." );
        }

        #endregion
    }
}
