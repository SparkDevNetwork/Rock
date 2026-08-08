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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

using Location = Rock.Model.Location;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PersonSkill
{
    #region Tool(s)

    [Description( "Updates a person's address." )]
    [AgentToolGuid( "D34E7821-36E0-F2BC-4496-7A82E1CE4475" )]
    [AgentPurpose( "The combination of the personIdKey and locationTypeValueIdKey parameters will determine which address gets added or updated." )]
    public AgentToolResult AddOrUpdateAddress(
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
        bool? isMailingLocation = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var locationTypeValueId = IdHasher.Instance.GetId( locationTypeValueIdKey );
        var locationTypeValue = locationTypeValueId.HasValue
            ? DefinedValueCache.Get( locationTypeValueId.Value )
            : null;

        // Check for valid location type
        if ( !locationTypeValueId.HasValue || locationTypeValue == null )
        {
            var locationTypes = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), rockContext )
                .LocationTypeValues
                .Select( dv => new KeyNameResult( dv.Id, dv.Value ) )
                .ToList();

            helper.AddError( $"Lookup requested for {nameof( locationTypeValueIdKey )}. Metadata contains valid values." );
            helper.AddMetadata( $"{nameof( locationTypeValueIdKey )}Lookup", locationTypes );
        }

        var person = helper.GetRequiredEntity<Model.Person>( personIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var location = AddOrUpdateAddress( helper,
            rockContext,
            person,
            locationTypeValue,
            street1,
            street2,
            city,
            state,
            postalCode,
            country,
            county,
            isMappedLocation,
            isMailingLocation );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( $"The {locationTypeValue.Value} address for {person.FullName} has been updated to {location.GetFullStreetAddress()}." );
    }

    #endregion

    internal static Location AddOrUpdateAddress(
        AgentToolHelper helper,
        RockContext rockContext,
        Model.Person person,
        DefinedValueCache locationTypeValue,
        string street1 = null,
        string street2 = null,
        string city = null,
        string state = null,
        string postalCode = null,
        string country = null,
        string county = null,
        bool? isMappedLocation = null,
        bool? isMailingLocation = null )
    {
        // Add/Update the new address
        var groupLocation = person.PrimaryFamily
            .GroupLocations
            .FirstOrDefault( gl => gl.GroupId == person.PrimaryFamilyId
                && gl.GroupLocationTypeValueId == locationTypeValue.Id );

        if ( groupLocation == null )
        {
            // If no address exists today we should at least have street1, city and postal code
            if ( street1.IsNullOrWhiteSpace() || city.IsNullOrWhiteSpace() || postalCode.IsNullOrWhiteSpace() )
            {
                helper.AddError( "At minimum, street1, city, and postal code must be provided when adding a new address." );
                return null;
            }

            groupLocation = rockContext.Set<GroupLocation>().Create();
            new GroupLocationService( rockContext ).Add( groupLocation );

            groupLocation.GroupId = person.PrimaryFamilyId.Value;
            groupLocation.GroupLocationTypeValueId = locationTypeValue.Id;
            groupLocation.Location = rockContext.Set<Location>().Create();
            groupLocation.Location.State = GlobalAttributesCache.Get().OrganizationState;

            // If the location type is Home then by default set it to mailing
            // location and mapped location.
            if ( locationTypeValue != null && locationTypeValue.Guid == SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )
            {
                groupLocation.IsMailingLocation = true;
                groupLocation.IsMappedLocation = true;
            }
        }
        else
        {
            // If the address already exists and the street part has changed,
            // then create a previous address record. In the future, it might
            // be nice to use some kind of fuzzy matching to determine if the
            // address has changed enough to warrant creating a previous
            // address record.
            if ( street1 != groupLocation.Location.Street1 )
            {
                CreatePreviousAddress( rockContext, person.PrimaryFamily, groupLocation.Location );
            }
        }

        if ( street1.IsNotNullOrWhiteSpace() )
        {
            // Blank out street 2 if street1 is a different value.
            if ( street1 != groupLocation.Location.Street1 )
            {
                groupLocation.Location.Street2 = null;
            }

            helper.UpdateProperty( groupLocation.Location, l => l.Street1, street1 );
        }

        helper.UpdateProperty( groupLocation.Location, l => l.Street2, street2 );
        helper.UpdateProperty( groupLocation.Location, l => l.City, city );
        helper.UpdateProperty( groupLocation.Location, l => l.State, state );
        helper.UpdateProperty( groupLocation.Location, l => l.PostalCode, postalCode );
        helper.UpdateProperty( groupLocation.Location, l => l.Country, country );
        helper.UpdateProperty( groupLocation.Location, l => l.County, county );
        helper.UpdateProperty( groupLocation, gl => gl.IsMappedLocation, isMappedLocation );
        helper.UpdateProperty( groupLocation, gl => gl.IsMailingLocation, isMailingLocation );

        // Only one location can be mapped, so if this location is being set to
        // mapped then we need to set all the others to not mapped.
        if ( isMappedLocation == true )
        {
            foreach ( var loc in person.PrimaryFamily.GroupLocations )
            {
                if ( loc.Id != groupLocation.Id && loc.IsMappedLocation )
                {
                    loc.IsMappedLocation = false;
                }
            }
        }

        return groupLocation.Location;
    }

    internal static bool RemoveAddress(
        RockContext rockContext,
        Model.Person person,
        DefinedValueCache locationTypeValue )
    {
        // Add/Update the new address
        var groupLocation = person.PrimaryFamily
            .GroupLocations
            .FirstOrDefault( gl => gl.GroupId == person.PrimaryFamilyId
                && gl.GroupLocationTypeValueId == locationTypeValue.Id );

        if ( groupLocation == null )
        {
            return false;
        }
        else
        {
            CreatePreviousAddress( rockContext, person.PrimaryFamily, groupLocation.Location );

            return true;
        }
    }

    /// <summary>
    /// Creates a previous address record to mirror the current address that
    /// is going to be updated.
    /// </summary>
    /// <param name="rockContext">The database context to use when creating the new location.</param>
    /// <param name="familyGroup">The family to associate the previous address with.</param>
    /// <param name="currentAddress">The current address that will become the previous address.</param>
    private static void CreatePreviousAddress( RockContext rockContext, Model.Group familyGroup, Location currentAddress )
    {
        var previousAddress = new GroupLocation();

        new GroupLocationService( rockContext ).Add( previousAddress );

        var previousAddressValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid(), rockContext );

        if ( previousAddressValue != null )
        {
            previousAddress.GroupLocationTypeValueId = previousAddressValue.Id;
            previousAddress.GroupId = familyGroup.Id;

            previousAddress.Location = new Location
            {
                Street1 = currentAddress.Street1,
                Street2 = currentAddress.Street2,
                City = currentAddress.City,
                State = currentAddress.State,
                PostalCode = currentAddress.PostalCode,
                Country = currentAddress.Country
            };
        }
    }
}
