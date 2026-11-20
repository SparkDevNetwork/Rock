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
    /// Deletes a location.
    /// </summary>
    /// <remarks>
    /// A location with child locations, or one still used by another record,
    /// cannot be deleted. See <see cref="GetLocationInUseMessage"/> for the
    /// checks this adds on top of <see cref="LocationService.CanDelete"/>.
    /// </remarks>
    [AgentGuardrail( "This permanently deletes the location. Confirm the exact location with the user before proceeding." )]
    [Description( "Deletes a location. A location that has child locations or is used by other records, such as a family address, a group, attendance, or an attribute value, cannot be deleted." )]
    [AgentToolPreamble( "Deleting the location." )]
    [AgentToolPrerequisite( "Call ListLocations to determine the locationIdKey." )]
    [AgentToolGuid( "E211A7D0-4D76-450E-90A6-B4E1449AD0F5" )]
    public AgentToolResult DeleteLocation( string locationIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var locationService = new LocationService( rockContext );

        var location = helper.GetRequiredEntity<Rock.Model.Location>( locationIdKey );

        if ( location == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListLocations )} function to determine the available locations." );
        }

        // A named location inherits its security from its parent location.
        if ( !location.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to delete that location." );
        }

        if ( !locationService.CanDelete( location, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        var inUseMessage = GetLocationInUseMessage( location, rockContext );

        if ( inUseMessage != null )
        {
            return Error( inUseMessage );
        }

        var isNamedLocation = location.IsNamedLocation;
        var description = location.ToString( true );

        locationService.Delete( location );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Check-in kiosks cache their named locations, so refresh them, as the
        // Location Detail block does after a delete.
        if ( isNamedLocation )
        {
            Rock.CheckIn.KioskDevice.Clear();
        }

        return Success( $"The '{description}' location has been deleted." );
    }

    #endregion

    #region Location Support

    /// <summary>
    /// Checks for the uses of a location that <see cref="LocationService.CanDelete"/>
    /// does not cover.
    /// </summary>
    /// <param name="location">The location about to be deleted.</param>
    /// <param name="rockContext">The context to query.</param>
    /// <returns>A message that describes the first use found, or <c>null</c> when the location is not in use.</returns>
    private static string GetLocationInUseMessage( Rock.Model.Location location, RockContext rockContext )
    {
        /*
            11/19/2026 - CLAUDE

            CanDelete does not check group locations, attendance occurrences, or
            device locations, because all three cascade on delete. Deleting a
            family's home address would silently remove it from the family, and
            deleting a room would remove its attendance history and its kiosk
            assignments. It also cannot see Address, Location, and Location List
            attribute values or attribute defaults, which store the location's
            Guid as text. The referenced entity tables are not used here because
            not every write path keeps them filled in. Refuse all of these so
            the agent never removes data the user did not name.

            Reason: Treat a location as in use when anything still points at it.
        */
        if ( new GroupLocationService( rockContext ).Queryable().Any( gl => gl.LocationId == location.Id ) )
        {
            return "This location is used by a group or family, such as a family's address or a group's meeting location, and cannot be deleted.";
        }

        if ( new AttendanceOccurrenceService( rockContext ).Queryable().Any( ao => ao.LocationId == location.Id ) )
        {
            return "This location has attendance records and cannot be deleted.";
        }

        // The DeviceLocation join table also cascades, so deleting a room would
        // silently remove it from every check-in kiosk that uses it.
        if ( new DeviceService( rockContext ).Queryable().Any( d => d.Locations.Any( l => l.Id == location.Id ) ) )
        {
            return "This location is assigned to a device, such as a check-in kiosk, and cannot be deleted.";
        }

        var locationFieldTypeIds = new[]
            {
                FieldTypeCache.GetId( SystemGuid.FieldType.ADDRESS.AsGuid() ),
                FieldTypeCache.GetId( SystemGuid.FieldType.LOCATION.AsGuid() ),
                FieldTypeCache.GetId( SystemGuid.FieldType.LOCATION_LIST.AsGuid() )
            }
            .Where( id => id.HasValue )
            .Select( id => id.Value )
            .ToList();

        var locationGuidText = location.Guid.ToString();

        var isUsedByAttributeValue = new AttributeValueService( rockContext )
            .Queryable()
            .Any( av => locationFieldTypeIds.Contains( av.Attribute.FieldTypeId )
                && av.Value.Contains( locationGuidText ) );

        if ( isUsedByAttributeValue )
        {
            return "This location is stored in an attribute value, such as an Address attribute, and cannot be deleted.";
        }

        // An entity with no stored value, or an empty one, uses the attribute's
        // default value, so a default that points here is a use too.
        var isUsedByAttributeDefault = new AttributeService( rockContext )
            .Queryable()
            .Any( a => locationFieldTypeIds.Contains( a.FieldTypeId )
                && a.DefaultValue.Contains( locationGuidText ) );

        if ( isUsedByAttributeDefault )
        {
            return "This location is the default value of an attribute, such as an Address attribute, and cannot be deleted.";
        }

        return null;
    }

    #endregion
}
