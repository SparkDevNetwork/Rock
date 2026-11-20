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
using Rock.Security;
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
                .Select( dv => KeyNameResult.FromCache( dv ) )
                .ToList();

            helper.AddError( $"Lookup requested for {nameof( locationTypeValueIdKey )}. Metadata contains valid values." );
            helper.AddMetadata( $"{nameof( locationTypeValueIdKey )}Lookup", locationTypes );
        }

        var person = helper.GetRequiredEntity<Model.Person>( personIdKey );

        /*
            10/16/2026 - MSE

            Loading the person only checks VIEW, which everyone has on people
            by default. Changing a person's address must require EDIT on them,
            as the other agent write tools do. A person can still edit
            themselves because Person.IsAuthorized() allows that. The check is
            here and not in the shared AddOrUpdateAddress() helper because the
            Attendee skill uses that helper with its own family rules.

            Reason: Prevent changing a person without EDIT permission.
        */
        if ( person != null && !person.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            helper.AddError( "You are not authorized to edit this person." );
        }

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

        var isNewGroupLocation = groupLocation == null;

        // If no address exists today we should at least have street1, city and postal code
        if ( isNewGroupLocation && ( street1.IsNullOrWhiteSpace() || city.IsNullOrWhiteSpace() || postalCode.IsNullOrWhiteSpace() ) )
        {
            helper.AddError( "At minimum, street1, city, and postal code must be provided when adding a new address." );
            return null;
        }

        /*
            11/20/26 - CLAUDE

            Address Location rows are shared. The family editor and other core
            blocks find or create them with LocationService.Get(), so two
            families at the same address, or a family and an Address attribute,
            can point at the same row. Editing the row in place would change
            the address for every record that uses it. Instead we work out the
            final address, then point this GroupLocation at the matching row
            from LocationService.Get(), the same way EditGroup.ascx.cs does.
            Get() also verifies and geocodes any new row it creates.

            Reason: Never edit a shared address Location row in place.
        */

        // A new address starts from the organization's state, the same
        // default this tool has always used. Parts the caller did not send
        // keep the current value, matching how UpdateProperty() ignores null
        // or blank values.
        var currentLocation = isNewGroupLocation
            ? new Location { State = GlobalAttributesCache.Get().OrganizationState }
            : groupLocation.Location;

        var isStreet1Changed = street1.IsNotNullOrWhiteSpace() && street1 != currentLocation.Street1;

        var finalStreet1 = GetAddressPartValue( street1, currentLocation.Street1 );

        // Blank out street 2 if street1 is a different value and no new
        // street 2 was sent.
        var finalStreet2 = GetAddressPartValue( street2, isStreet1Changed ? null : currentLocation.Street2 );
        var finalCity = GetAddressPartValue( city, currentLocation.City );
        var finalState = GetAddressPartValue( state, currentLocation.State );
        var finalPostalCode = GetAddressPartValue( postalCode, currentLocation.PostalCode );
        var finalCountry = GetAddressPartValue( country, currentLocation.Country );

        var isCountyChanged = county.IsNotNullOrWhiteSpace() && county != currentLocation.County;

        var isAddressChanged = isNewGroupLocation
            || isCountyChanged
            || !IsSameAddressPart( finalStreet1, currentLocation.Street1 )
            || !IsSameAddressPart( finalStreet2, currentLocation.Street2 )
            || !IsSameAddressPart( finalCity, currentLocation.City )
            || !IsSameAddressPart( finalState, currentLocation.State )
            || !IsSameAddressPart( finalPostalCode, currentLocation.PostalCode )
            || !IsSameAddressPart( finalCountry, currentLocation.Country );

        Location updatedAddress = null;

        if ( isAddressChanged )
        {
            // Older addresses may have no country. Fall back to the
            // organization's country, as FamilyPreRegistration does, so the
            // country address requirements can be checked.
            finalCountry = finalCountry.IfEmpty( GlobalAttributesCache.Get().OrganizationCountry );

            var addressToValidate = new Location
            {
                Street1 = finalStreet1,
                Street2 = finalStreet2,
                City = finalCity,
                State = finalState,
                County = county.IfEmpty( currentLocation.County ),
                PostalCode = finalPostalCode,
                Country = finalCountry
            };

            if ( !LocationService.ValidateLocationAddressRequirements( addressToValidate, out var validationMessage ) )
            {
                helper.AddError( validationMessage );
                return null;
            }

            // LocationService.Get throws when a new address fails validation or
            // cannot be saved (for example, a street that is too long), so turn
            // that into a tool error instead of a failed request.
            try
            {
                updatedAddress = new LocationService( rockContext ).Get( finalStreet1, finalStreet2, finalCity, finalState, finalPostalCode, finalCountry );
            }
            catch ( System.Exception ex )
            {
                ExceptionLogService.LogException( ex );
                helper.AddError( "The address could not be saved. Check the address parts and try again." );
                return null;
            }

            if ( updatedAddress == null )
            {
                helper.AddError( "The address could not be found or created." );
                return null;
            }

            helper.UpdateProperty( updatedAddress, l => l.County, county );
        }

        if ( isNewGroupLocation )
        {
            groupLocation = rockContext.Set<GroupLocation>().Create();
            new GroupLocationService( rockContext ).Add( groupLocation );

            groupLocation.GroupId = person.PrimaryFamilyId.Value;
            groupLocation.GroupLocationTypeValueId = locationTypeValue.Id;

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
            // address record. A call that does not send street1 leaves the
            // street unchanged, so it does not create one.
            if ( isStreet1Changed )
            {
                CreatePreviousAddress( rockContext, person.PrimaryFamily, groupLocation.Location );
            }
        }

        if ( updatedAddress != null )
        {
            groupLocation.Location = updatedAddress;
        }

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
    /// Gets the value an address part should have after the update. A null or
    /// blank parameter means the caller did not send that part, so the current
    /// value is kept. This matches how <c>AgentToolHelper.UpdateProperty()</c>
    /// treats string parameters.
    /// </summary>
    /// <param name="parameter">The value the caller sent, if any.</param>
    /// <param name="currentValue">The value the address has today.</param>
    /// <returns>The value the address part should have after the update.</returns>
    private static string GetAddressPartValue( string parameter, string currentValue )
    {
        return parameter.IsNotNullOrWhiteSpace() ? parameter : currentValue;
    }

    /// <summary>
    /// Determines whether two address parts are the same. A null part and an
    /// empty part are treated as the same, as GroupDetail does when it decides
    /// whether an address changed.
    /// </summary>
    /// <param name="value">The first address part.</param>
    /// <param name="otherValue">The second address part.</param>
    /// <returns><c>true</c> if both address parts are the same.</returns>
    private static bool IsSameAddressPart( string value, string otherValue )
    {
        return ( value ?? string.Empty ) == ( otherValue ?? string.Empty );
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
