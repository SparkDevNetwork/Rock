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
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single location in full detail.
    /// </summary>
    /// <remarks>
    /// This is where a caller gets the address parts, map coordinates, child
    /// count, and attribute values. The list tool carries only enough to choose
    /// a row.
    /// </remarks>
    [Description( "Gets a single location in full detail, including its unique identifier, address parts, and attribute values." )]
    [AgentPurpose( "Retrieves one location, including the unique identifier that an Address, Location, or Location List attribute stores." )]
    [AgentToolPrerequisite( "Call ListLocations to determine the locationIdKey." )]
    [AgentToolGuid( "98333433-96B7-4F62-9D76-DC086B787C3D" )]
    public AgentToolResult GetLocation( string locationIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var location = helper.GetRequiredEntity<Rock.Model.Location>( locationIdKey );

        if ( location == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListLocations )} function to determine the available locations." );
        }

        var result = CreateLocationDetailResult( location, AgentRequestContext.RockContext );

        // Required, not optional. GetAttributeValueResults filters only on
        // IsPublic; the per-attribute VIEW authorization happens here.
        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this location." );
        }

        return Success( result )
            .WithHistoryContent( new KeyNameResult( location.Id, location.Guid, location.ToString( true ) ) );
    }

    #endregion

    #region Location Support

    /// <summary>
    /// Builds the detail result for a location. GetLocation and
    /// AddOrUpdateLocation return the same shape, so they share this.
    /// </summary>
    /// <param name="location">The location to describe.</param>
    /// <param name="rockContext">The context used to load attributes and count children.</param>
    /// <returns>The detail result. The caller is responsible for sanitizing it.</returns>
    private LocationDetailResult CreateLocationDetailResult( Rock.Model.Location location, RockContext rockContext )
    {
        location.LoadAttributes( rockContext );

        var childLocationCount = new LocationService( rockContext )
            .Queryable()
            .Count( l => l.ParentLocationId == location.Id );

        return new LocationDetailResult
        {
            Id = location.Id,
            Guid = location.Guid,
            Name = location.Name,
            Street1 = location.Street1,
            Street2 = location.Street2,
            City = location.City,
            State = location.State,
            County = location.County,
            PostalCode = location.PostalCode,
            Country = location.Country,
            FormattedAddress = location.GetFullStreetAddress(),
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            IsActive = location.IsActive,
            LocationType = GetLocationTypeResult( location.LocationTypeValueId ),
            ParentLocation = GetParentLocationResult( location.ParentLocationId ),
            ChildLocationCount = childLocationCount,
            AttributeValues = location.GetAttributeValueResults( AgentRequestContext ).ToList()
        };
    }

    #endregion
}
