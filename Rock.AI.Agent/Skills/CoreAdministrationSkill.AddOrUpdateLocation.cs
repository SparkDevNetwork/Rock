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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Adds a location or updates an existing one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A new address (no name) is first looked up with
    /// <see cref="FindExistingAddressLocation"/>, which matches the way
    /// <see cref="LocationService.Get(string, string, string, string, string, string, GetLocationArgs)"/>
    /// does but never saves, so an existing location with the same address is
    /// reused instead of duplicated. When nothing matches, a new row is created
    /// from the copy the lookup already verified, so the address verification
    /// service is called at most once. A match the caller cannot view is
    /// treated as no match, and a new row is created. A new named location
    /// (a building or room) is always created, because two rooms can share an
    /// address.
    /// </para>
    /// <para>
    /// Editing the address of an existing location is allowed, but a location
    /// can be shared. A family's home address and an Address attribute can
    /// point at the same row, so the edit shows up everywhere. The usage text
    /// tells the agent this.
    /// </para>
    /// <para>
    /// Whenever an address is verified, the standardize and geocode results
    /// are returned in the metadata, the same two values the Location Detail
    /// block shows. The agent cannot see a map, so this is how it learns that
    /// an address was not standardized or its map point was not updated.
    /// </para>
    /// </remarks>
    [Description( "Adds a new location or updates an existing one. A location is either an address or a named place such as a campus, building, or room." )]
    [AgentUsage( "To add an address, supply the address parts and no name. Rock checks the address against the country's address rules, verifies it, and reuses an existing location with the same address instead of creating a duplicate. To add a named location such as a building or room, supply a name, and usually a locationTypeValueIdKey and a parentLocationIdKey." )]
    [AgentUsage( "The parent location must be a named location, such as a campus or building. A plain address cannot be a parent. The name of a named location cannot be cleared." )]
    [AgentUsage( "When an address is added or changed, the result metadata holds standardizeAttemptedResult and geocodeAttemptedResult from the address verification service. If they show that the address was not standardized or the map point was not updated, tell the user." )]
    [AgentUsage( "To fill an Address, Location, or Location List attribute, add the address with this tool (or find the location with ListLocations), then set the attribute value to the Guid of the location in the result." )]
    [AgentUsage( "Supplying locationIdKey updates that location and leaves any parameter you omit unchanged." )]
    [AgentUsage( "Be careful when you edit the address of an existing location. One location can be shared by many records, such as a family's home address and an Address attribute, so an edit changes the address everywhere it is used. When only one record should change, add a new address instead and point that record at the new location. Confirm with the user before you edit the address of an existing location." )]
    [AgentToolPrerequisite( "Call ListLocations to determine the locationIdKey and parentLocationIdKey, and ListDefinedValues on the Location Type defined type to determine the locationTypeValueIdKey." )]
    [AgentToolGuid( "93797E5A-40D6-43B6-810C-AD2214CF5B16" )]
    public AgentToolResult AddOrUpdateLocation(
        string locationIdKey = null,
        [Description( "The name of a named location, such as 'Main Building' or 'Room 101'. Leave empty for a plain address." )]
        SetOrClear<string> name = null,
        string street1 = null,
        SetOrClear<string> street2 = null,
        string city = null,
        [Description( "The state or province, usually as its abbreviation, such as 'AZ'." )]
        string state = null,
        string postalCode = null,
        [Description( "The two-letter country code, such as 'US'. Defaults to the organization's country when adding a location with an address." )]
        string country = null,
        [Description( "The location type, such as Building or Room. This is a value of the Location Type defined type." )]
        SetOrClear<string> locationTypeValueIdKey = null,
        [Description( "The parent location to place this one under, such as the building that contains a room. It must be a named location." )]
        SetOrClear<string> parentLocationIdKey = null,
        bool? isActive = null,
        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var locationService = new LocationService( rockContext );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var isNew = locationIdKey.IsNullOrWhiteSpace();
        var hasName = name != null && !name.ClearValue && name.Value.IsNotNullOrWhiteSpace();
        var isNewPlainAddress = isNew && !hasName;
        var hasAddressParts = street1.IsNotNullOrWhiteSpace()
            || ( street2 != null && street2.Value.IsNotNullOrWhiteSpace() )
            || city.IsNotNullOrWhiteSpace()
            || state.IsNotNullOrWhiteSpace()
            || postalCode.IsNotNullOrWhiteSpace();

        Rock.Model.Location location;
        int? originalParentLocationId = null;
        var isAddressChanged = false;

        if ( !isNew )
        {
            location = helper.GetRequiredEntity<Rock.Model.Location>( locationIdKey );

            if ( location == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListLocations )} function to determine the available locations." );
            }

            /*
                11/19/2026 - CLAUDE

                A named location inherits its security from its parent, and the
                parent can be changed further down. Checking only after that
                change would evaluate the parent the caller picked, so a person
                who can edit one building could move another location under it
                and gain edit rights to it. Check the location as it is stored,
                before anything changes, the same way AddOrUpdateCategory does.

                Reason: Prevent gaining edit rights to a location by moving it.
            */
            originalParentLocationId = location.ParentLocationId;

            if ( !location.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return Error( "You are not authorized to edit that location." );
            }

            // The Location Detail block requires a name for a named location,
            // so an update cannot turn a named location back into an address.
            var isNameCleared = name != null && ( name.ClearValue || name.Value.IsNullOrWhiteSpace() );

            if ( location.Name.IsNotNullOrWhiteSpace() && isNameCleared )
            {
                return Error( "The name of a named location cannot be cleared." );
            }

            var originalAddress = GetAddressSignature( location );

            helper.UpdateProperty( location, l => l.Street1, street1 );
            helper.UpdateProperty( location, l => l.Street2, street2 );
            helper.UpdateProperty( location, l => l.City, city );
            helper.UpdateProperty( location, l => l.State, state );
            helper.UpdateProperty( location, l => l.PostalCode, postalCode );
            helper.UpdateProperty( location, l => l.Country, country );

            isAddressChanged = GetAddressSignature( location ) != originalAddress;
        }
        else
        {
            if ( isNewPlainAddress )
            {
                if ( !hasAddressParts )
                {
                    return Error( "Either a name or at least one address part is required when adding a location." );
                }

                // A location without a name takes its security from the Location
                // entity type, so check that before any lookup or verification runs.
                if ( !new Rock.Model.Location().IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return Error( "You are not authorized to add a location." );
                }

                country = country.IfEmpty( GlobalAttributesCache.Get().OrganizationCountry );

                /*
                    11/19/2026 - CLAUDE

                    Check the country's address rules before the lookup, so the
                    agent gets the exact message (which fields are missing) up front
                    and no lookup or verification runs for an address that could
                    never be saved. This matches the address editor, which checks
                    the same rules before it submits.

                    Reason: Report address rule failures as a clear tool error.
                */
                var addressToValidate = new Rock.Model.Location
                {
                    Street1 = street1,
                    Street2 = street2?.Value,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    Country = country
                };

                if ( !LocationService.ValidateLocationAddressRequirements( addressToValidate, out var addressErrorMessage ) )
                {
                    return Error( addressErrorMessage );
                }
            }

            /*
                11/20/2026 - CLAUDE

                The new row is created here but only added to the context after
                the address lookup below finds no match. Until then nothing is
                tracked as added, so a reused match never leaves a stray new row
                behind, and the lookup cannot match the row being built.

                Reason: Build the new row first so every input is checked before
                the lookup calls the paid address verification service.
            */

            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            location = rockContext.Set<Rock.Model.Location>().Create();

            location.IsActive = true;

            if ( hasAddressParts )
            {
                country = country.IfEmpty( GlobalAttributesCache.Get().OrganizationCountry );
                isAddressChanged = true;
            }

            helper.UpdateProperty( location, l => l.Street1, street1 );
            helper.UpdateProperty( location, l => l.Street2, street2 );
            helper.UpdateProperty( location, l => l.City, city );
            helper.UpdateProperty( location, l => l.State, state );
            helper.UpdateProperty( location, l => l.PostalCode, postalCode );
            helper.UpdateProperty( location, l => l.Country, country );
        }

        helper.UpdateProperty( location, l => l.Name, name );

        if ( name != null )
        {
            location.Name = location.Name.IsNullOrWhiteSpace() ? null : location.Name.Trim();
        }

        helper.UpdateProperty( location, l => l.IsActive, isActive );
        helper.UpdateDefinedValueProperty( location, l => l.LocationTypeValue, locationTypeValueIdKey );
        helper.UpdateNavigationProperty( location, l => l.ParentLocation, parentLocationIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListLocations )} function to determine the available parent locations." );
        }

        if ( location.ParentLocationId.HasValue && location.ParentLocationId != originalParentLocationId )
        {
            var parentLocation = location.ParentLocation ?? locationService.Get( location.ParentLocationId.Value );

            // Named location security inherits from the parent location, so the
            // caller must be able to edit a newly chosen parent, as the Location
            // Detail block requires.
            if ( parentLocation == null || !parentLocation.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return Error( "You are not authorized to add a location to the selected parent location." );
            }

            // The Location Detail block's parent picker only offers named
            // locations, so a plain address cannot be a parent.
            if ( parentLocation.Name.IsNullOrWhiteSpace() )
            {
                return Error( "The parent location must be a named location, such as a campus or building. A plain address cannot be a parent." );
            }
        }

        /*
            11/20/2026 - CLAUDE

            LocationService.Get creates a new address row with no attribute
            values, so a new plain address with no attribute values sent skips
            SetAttributeValues and its required attribute check. Otherwise a
            required Location attribute would block adding a family's address.
            Named locations, and any call that sends attribute values, are still
            checked. Any error returns here, before the address verification
            service is called, so a bad attribute key never costs a paid call.

            Reason: Match LocationService.Get and avoid a paid call on bad input.
        */
        var hasAttributeValues = attributeValues?.Any() == true;

        if ( !isNewPlainAddress || hasAttributeValues )
        {
            // Location attributes can be qualified by the location type, which is
            // set above before this runs, so LoadAttributes resolves the right set.
            helper.SetAttributeValues( location, attributeValues );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }
        }

        // The caller must be able to edit the location, which for a named one
        // resolves through its parent (or the Location entity type at the root).
        if ( !location.IsAuthorized( Authorization.EDIT, currentPerson ) )
        {
            return Error( "You are not authorized to save that location." );
        }

        Rock.Model.Location verifiedAddress = null;

        if ( isNewPlainAddress )
        {
            /*
                11/19/2026 - CLAUDE

                The lookup for an existing location is read-only. LocationService.Get
                is not used, even with CreateNewLocation off, because when it
                finds a match whose casing differs it saves the new casing onto
                that row and clears its County. That write would happen before
                this method could check VIEW on the row, and the row may belong
                to a family or sit under a parent the caller cannot see.

                A matched location is shared, so it is returned as it is. A new
                row is created below in this context, where this method knows it
                owns the row and can safely apply isActive, the type, the parent,
                and attribute values. If two requests add the same new address at
                once, the worst case is a duplicate address row, which Rock
                already tolerates.

                Reason: Never write to a shared location as a side effect of an add.
            */
            Rock.Model.Location existingLocation;

            try
            {
                existingLocation = FindExistingAddressLocation( locationService, street1, street2?.Value, city, state, postalCode, country, currentPerson, out verifiedAddress );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "An error occurred while looking up the address location." );

                return Error( "The address could not be checked. Check the address parts and try again." );
            }

            /*
                11/20/2026 - CLAUDE

                The lookup only returns a match the caller can view. A match the
                caller cannot view is treated as no match, and a new row is
                created below. Returning an error instead would confirm that the
                address exists in a record the caller cannot see, and it would
                block a legitimate add. Rock already tolerates duplicate address
                rows, so the extra row does no harm. Because the lookup skips
                hidden rows and returns the first visible one, a second add of
                the same address reuses the row the first add created instead of
                making another duplicate.

                Reason: Do not reveal or block on an address the caller cannot view.
            */
            if ( existingLocation != null )
            {
                var hasOtherValues = isActive.HasValue
                    || locationTypeValueIdKey != null
                    || parentLocationIdKey != null
                    || hasAttributeValues;

                return GetReusedLocationResult( existingLocation, rockContext, hasOtherValues );
            }
        }

        if ( isNew )
        {
            locationService.Add( location );
        }

        if ( isAddressChanged )
        {
            // The lookup already verified a copy of this address, so use its
            // results instead of calling the verification service again, the
            // same way LocationService.Get saves the copy it verified.
            if ( verifiedAddress != null )
            {
                ApplyVerifiedAddress( verifiedAddress, location );
            }

            if ( !LocationService.ValidateLocationAddressRequirements( location, out var addressErrorMessage ) )
            {
                return Error( addressErrorMessage );
            }

            if ( verifiedAddress == null )
            {
                // Standardize and geocode the new address, as the address editor's
                // verify action does.
                locationService.Verify( location, true );
            }
        }

        if ( !location.IsValid )
        {
            return Error( location.ValidationResults.Select( r => r.ErrorMessage ).ToList().AsDelimited( " " ) );
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Check-in kiosks cache their locations, so refresh them after every
        // save, as the Location Detail block does.
        Rock.CheckIn.KioskDevice.Clear();

        var result = CreateLocationDetailResult( location, rockContext );

        result.Sanitize( AgentRequestContext );

        string instructions;

        if ( isNew )
        {
            instructions = "The location has been created.";
        }
        else if ( isAddressChanged )
        {
            instructions = "The location has been updated. The new address applies everywhere this location is used.";
        }
        else
        {
            instructions = "The location has been updated.";
        }

        var toolResult = Success( result )
            .WithInstructions( instructions )
            .WithHistoryContent( new KeyNameResult( location.Id, location.Guid, location.ToString( true ) ) );

        if ( isAddressChanged )
        {
            /*
                11/20/2026 - CLAUDE

                LocationService.Verify returns without doing anything when the
                map point is locked or no verification service is active. The
                stored results would then describe the old address, so they are
                only reported when verification actually ran. Otherwise the
                agent is told why the address was not verified.

                Reason: Never report old verification results as current.
            */
            var verificationSkippedReason = GetVerificationSkippedReason( location );

            if ( verificationSkippedReason != null )
            {
                toolResult.WithInstructions( verificationSkippedReason );
            }
            else
            {
                // These are the same two values the Location Detail block reports
                // after it verifies an address.
                toolResult
                    .WithMetadata( "standardizeAttemptedResult", location.StandardizeAttemptedResult )
                    .WithMetadata( "geocodeAttemptedResult", location.GeocodeAttemptedResult )
                    .WithInstructions( "Check standardizeAttemptedResult and geocodeAttemptedResult in the metadata. If they show that the address was not standardized or the map point was not updated, tell the user." );
            }
        }

        return toolResult;
    }

    #endregion

    #region Location Support

    /// <summary>
    /// Builds the result for an add that matched an existing address location.
    /// The location is returned as it is stored, with no changes applied. The
    /// caller must already have checked that the person can view it.
    /// </summary>
    /// <param name="location">The existing location that was reused.</param>
    /// <param name="rockContext">The context used to build the result.</param>
    /// <param name="hasOtherValues"><c>true</c> when the caller supplied values that were not applied.</param>
    /// <returns>The success result with instructions for the agent.</returns>
    private AgentToolResult GetReusedLocationResult( Rock.Model.Location location, RockContext rockContext, bool hasOtherValues )
    {
        var result = CreateLocationDetailResult( location, rockContext );

        result.Sanitize( AgentRequestContext );

        var instructions = "An existing location already had this address, so it was reused instead of creating a duplicate. Other records may use this location, so do not edit it unless the change should apply everywhere.";

        if ( hasOtherValues )
        {
            instructions += $" The isActive, locationTypeValueIdKey, parentLocationIdKey, and attributeValues you supplied were not applied. If they should apply to this shared location, confirm with the user, then call {nameof( AddOrUpdateLocation )} again with this location's locationIdKey.";
        }

        return Success( result )
            .WithInstructions( instructions )
            .WithHistoryContent( new KeyNameResult( location.Id, location.Guid, location.ToString( true ) ) );
    }

    /// <summary>
    /// Finds an existing location with the same address without changing it.
    /// The match follows <see cref="LocationService.Get(string, string, string, string, string, string, GetLocationArgs)"/>:
    /// first the address as entered, then the address after it is standardized
    /// by the address verification service.
    /// </summary>
    /// <param name="locationService">The service to query.</param>
    /// <param name="street1">The first street address line.</param>
    /// <param name="street2">The second street address line.</param>
    /// <param name="city">The city.</param>
    /// <param name="state">The state.</param>
    /// <param name="postalCode">The postal code.</param>
    /// <param name="country">The country.</param>
    /// <param name="verifiedAddress">
    /// The unsaved copy of the address after verification, or <c>null</c> when
    /// the address as entered matched and no verification ran. A new row can be
    /// built from this copy so the verification service is not called twice.
    /// </param>
    /// <param name="currentPerson">The person who must be able to view a match.</param>
    /// <returns>The matching location the person can view, or <c>null</c> when none exists.</returns>
    private static Rock.Model.Location FindExistingAddressLocation( LocationService locationService, string street1, string street2, string city, string state, string postalCode, string country, Rock.Model.Person currentPerson, out Rock.Model.Location verifiedAddress )
    {
        verifiedAddress = null;

        var address = new Rock.Model.Location
        {
            Street1 = street1.ToStringSafe().Trim(),
            Street2 = street2.ToStringSafe().Trim(),
            City = city.ToStringSafe().Trim(),
            State = state.ToStringSafe().Trim(),
            PostalCode = postalCode.ToStringSafe().Trim(),
            Country = country.ToStringSafe().Trim()
        };

        var existingLocation = FindLocationWithAddress( locationService, address, currentPerson );

        if ( existingLocation != null )
        {
            return existingLocation;
        }

        // Verify standardizes this unsaved copy in memory. It saves only a
        // service log entry, in its own context, never a location.
        var standardizedAddress = new Rock.Model.Location
        {
            Street1 = address.Street1.FixCase(),
            Street2 = address.Street2.FixCase(),
            City = address.City.FixCase(),
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };

        locationService.Verify( standardizedAddress, false );

        verifiedAddress = standardizedAddress;

        return FindLocationWithAddress( locationService, standardizedAddress, currentPerson );
    }

    /// <summary>
    /// Copies the address and the verification results from a verified,
    /// unsaved copy onto the location that will be saved. These are the
    /// values that <see cref="LocationService.Verify(Rock.Model.Location, bool)"/>
    /// sets, so the saved row matches the copy that
    /// <see cref="LocationService.Get(string, string, string, string, string, string, GetLocationArgs)"/>
    /// would have saved.
    /// </summary>
    /// <param name="verifiedAddress">The verified copy of the address.</param>
    /// <param name="location">The location to update.</param>
    private static void ApplyVerifiedAddress( Rock.Model.Location verifiedAddress, Rock.Model.Location location )
    {
        location.Street1 = verifiedAddress.Street1;
        location.Street2 = verifiedAddress.Street2;
        location.City = verifiedAddress.City;
        location.State = verifiedAddress.State;
        location.County = verifiedAddress.County;
        location.PostalCode = verifiedAddress.PostalCode;
        location.Country = verifiedAddress.Country;
        location.Barcode = verifiedAddress.Barcode;
        location.GeoPoint = verifiedAddress.GeoPoint;

        location.StandardizeAttemptedDateTime = verifiedAddress.StandardizeAttemptedDateTime;
        location.StandardizeAttemptedServiceType = verifiedAddress.StandardizeAttemptedServiceType;
        location.StandardizeAttemptedResult = verifiedAddress.StandardizeAttemptedResult;
        location.StandardizedDateTime = verifiedAddress.StandardizedDateTime;

        location.GeocodeAttemptedDateTime = verifiedAddress.GeocodeAttemptedDateTime;
        location.GeocodeAttemptedServiceType = verifiedAddress.GeocodeAttemptedServiceType;
        location.GeocodeAttemptedResult = verifiedAddress.GeocodeAttemptedResult;
        location.GeocodedDateTime = verifiedAddress.GeocodedDateTime;
    }

    /// <summary>
    /// Finds the oldest location whose address parts equal those of the given
    /// location and that the person can view. A missing part matches an empty
    /// one. The comparison follows the database collation, which is not case
    /// sensitive by default, the same as the search inside
    /// <see cref="LocationService"/>.
    /// </summary>
    /// <param name="locationService">The service to query.</param>
    /// <param name="address">The address to match.</param>
    /// <param name="currentPerson">The person who must be able to view the match.</param>
    /// <returns>The matching location, or <c>null</c> when none exists.</returns>
    private static Rock.Model.Location FindLocationWithAddress( LocationService locationService, Rock.Model.Location address, Rock.Model.Person currentPerson )
    {
        var street1 = address.Street1 ?? string.Empty;
        var street2 = address.Street2 ?? string.Empty;
        var city = address.City ?? string.Empty;
        var state = address.State ?? string.Empty;
        var postalCode = address.PostalCode ?? string.Empty;
        var country = address.Country ?? string.Empty;

        // Duplicate address rows are rare, so loading every match to check
        // security in memory is cheap.
        return locationService.Queryable()
            .Where( l => ( l.Street1 ?? string.Empty ) == street1
                && ( l.Street2 ?? string.Empty ) == street2
                && ( l.City ?? string.Empty ) == city
                && ( l.State ?? string.Empty ) == state
                && ( l.PostalCode ?? string.Empty ) == postalCode
                && ( l.Country ?? string.Empty ) == country )
            .OrderBy( l => l.Id )
            .ToList()
            .FirstOrDefault( l => l.IsAuthorized( Authorization.VIEW, currentPerson ) );
    }

    /// <summary>
    /// Gets the reason <see cref="LocationService.Verify(Rock.Model.Location, bool)"/>
    /// would not have verified the location, using the same checks it makes.
    /// </summary>
    /// <param name="location">The location whose address was changed.</param>
    /// <returns>A message for the agent, or <c>null</c> when verification ran.</returns>
    private static string GetVerificationSkippedReason( Rock.Model.Location location )
    {
        if ( location.IsGeoPointLocked == true )
        {
            return "The address was not verified because this location's map point is locked, so the map point was not updated. Tell the user.";
        }

        var hasActiveService = Rock.Address.VerificationContainer.Instance.Components.Values
            .Select( c => c.Value )
            .Any( c => c != null && c.IsActive && ( c.SupportsStandardization || c.SupportsGeocoding ) );

        if ( !hasActiveService )
        {
            return "The address was not verified because no address verification service is active, so it was not standardized and has no new map point. Tell the user.";
        }

        return null;
    }

    /// <summary>
    /// Gets a single string made from the address parts of a location. Two
    /// signatures differ when any address part differs.
    /// </summary>
    /// <param name="location">The location to read.</param>
    /// <returns>The address parts joined by a separator.</returns>
    private static string GetAddressSignature( Rock.Model.Location location )
    {
        return string.Join( "|", location.Street1, location.Street2, location.City, location.State, location.PostalCode, location.Country );
    }

    #endregion
}
