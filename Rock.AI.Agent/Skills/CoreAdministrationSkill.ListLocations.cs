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
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the locations that match the filters. A location is either an
    /// address or a named place such as a building or room.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Location table holds every address in Rock, including every family's
    /// home address, so at least one filter is required to keep a single call
    /// from returning a very large list. The filter is not a security control.
    /// Short prefixes can still cover most of the table. Access is controlled
    /// by Rock's location security, which CursorPaginator checks on every row.
    /// An address location has no parent, so it uses the security of the
    /// Location entity type, the same as everywhere else in Rock.
    /// </para>
    /// <para>
    /// Location derives from Model&lt;T&gt; and is secured per row, and there is
    /// no cache that holds address locations, so this uses CursorPaginator for
    /// the same reason ListSystemCommunications does.
    /// </para>
    /// <para>
    /// The list carries the Guid and the formatted address, which is more than
    /// the identity-only rule for this skill usually allows. The Guid is what an
    /// Address, Location, or Location List attribute stores, and an address
    /// location has no name, so the address is the only way to tell rows apart.
    /// </para>
    /// </remarks>
    [Description( "Lists the locations that match the filters. A location is either an address or a named place such as a campus, building, or room." )]
    [AgentPurpose( "Finds an existing location, such as the location to store in an Address, Location, or Location List attribute." )]
    [AgentUsage( "At least one filter other than includeInactive is required. Address and named locations are both returned. An Address, Location, or Location List attribute stores the Guid of a location, so use the Guid from the result when setting one of those attributes." )]
    [AgentToolGuid( "FB10F6D2-D6FE-401B-BD6B-F34AC4E51E88" )]
    public AgentToolResult ListLocations(
        [Description( "Part of the location name, such as 'Main Building' or 'Room 101'." )]
        string partialName = null,
        [Description( "Part of the first street address line, such as '123 Main'." )]
        string street1 = null,
        [Description( "The city, or the start of the city name." )]
        string city = null,
        [Description( "The postal code, or the start of it. '85383' also matches '85383-1234'." )]
        string postalCode = null,
        [Description( "The location type, such as Building or Room. This is a value of the Location Type defined type." )]
        string locationTypeValueIdKey = null,
        [Description( "Limits the results to the direct children of this location." )]
        string parentLocationIdKey = null,
        bool includeInactive = false,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var hasAnyFilters = partialName.IsNotNullOrWhiteSpace()
            || street1.IsNotNullOrWhiteSpace()
            || city.IsNotNullOrWhiteSpace()
            || postalCode.IsNotNullOrWhiteSpace()
            || locationTypeValueIdKey.IsNotNullOrWhiteSpace()
            || parentLocationIdKey.IsNotNullOrWhiteSpace();

        if ( !hasAnyFilters )
        {
            return Error( "At least one filter parameter must be provided to limit the results returned." )
                .WithInstructions( "Filter by partialName, street1, city, postalCode, locationTypeValueIdKey, or parentLocationIdKey." );
        }

        var query = new LocationService( AgentRequestContext.RockContext ).Queryable();

        if ( !includeInactive )
        {
            query = query.Where( l => l.IsActive );
        }

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( l => l.Name.Contains( partialName ) );
        }

        if ( street1.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( l => l.Street1.Contains( street1 ) );
        }

        if ( city.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( l => l.City.StartsWith( city ) );
        }

        if ( postalCode.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( l => l.PostalCode.StartsWith( postalCode ) );
        }

        query = helper.WhereOptionalIdKey( query, l => l.LocationTypeValueId, locationTypeValueIdKey );
        query = helper.WhereOptionalIdKey( query, l => l.ParentLocationId, parentLocationIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // The ThenBy on Id is required, not stylistic. Address locations have no
        // name and can share a street, so without a unique tiebreaker two rows
        // produce the same cursor and the seek predicate skips or repeats one.
        var paginator = new CursorPaginator<Rock.Model.Location>( currentPerson, qry => qry
            .OrderBy( l => l.Name )
            .ThenBy( l => l.Street1 )
            .ThenBy( l => l.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( l => new LocationResult
            {
                Id = l.Id,
                Guid = l.Guid,
                Name = l.Name,
                FormattedAddress = l.GetFullStreetAddress(),
                LocationType = GetLocationTypeResult( l.LocationTypeValueId ),
                ParentLocation = GetParentLocationResult( l.ParentLocationId ),
                IsActive = l.IsActive
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items
            .Select( l => new KeyNameResult { Id = l.Id, Guid = l.Guid, Name = l.ToString( true ) } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion

    #region Location Support

    /// <summary>
    /// Gets the reference to a location's type from the defined value cache.
    /// </summary>
    /// <param name="locationTypeValueId">The identifier of the location type defined value.</param>
    /// <returns>The reference, or <c>null</c> when no type is set.</returns>
    private static KeyNameResult GetLocationTypeResult( int? locationTypeValueId )
    {
        return locationTypeValueId.HasValue
            ? KeyNameResult.FromCache( DefinedValueCache.Get( locationTypeValueId.Value ) )
            : null;
    }

    /// <summary>
    /// Gets the reference to a location's parent from the named location cache.
    /// Location.ParentAuthority reads the parent the same way, and the cache
    /// avoids loading one parent per row from the database.
    /// </summary>
    /// <param name="parentLocationId">The identifier of the parent location.</param>
    /// <returns>The reference, or <c>null</c> when there is no parent.</returns>
    private static KeyNameResult GetParentLocationResult( int? parentLocationId )
    {
        return parentLocationId.HasValue
            ? KeyNameResult.FromCache( NamedLocationCache.Get( parentLocationId.Value ) )
            : null;
    }

    #endregion
}
