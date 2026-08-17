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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Mobile.Connection;

/// <summary>
/// Answers whether the current person may add or edit a
/// <see cref="ConnectionRequest"/>, using the same rules as the web
/// Connections Hub. Shared by the mobile Connection blocks so a person is
/// never offered an action they cannot complete, and is never denied one
/// the web would have granted them.
/// </summary>
internal static class ConnectionRequestAuthorization
{
    #region Add Gate

    /// <summary>
    /// Determines whether the current person may add a request to a single
    /// connection opportunity. Ports the Hub's
    /// <c>CanEditConnectionRequests( type, opportunity )</c>.
    /// </summary>
    /// <param name="rockContext">The Rock context to use for the connector group fallback query.</param>
    /// <param name="connectionOpportunity">The opportunity being added to. Its <see cref="ConnectionOpportunity.ConnectionType"/> must be loaded.</param>
    /// <param name="currentPerson">The current person.</param>
    /// <returns><c>true</c> if the person may add a request to the opportunity.</returns>
    internal static bool CanAddRequest( RockContext rockContext, ConnectionOpportunity connectionOpportunity, Person currentPerson )
    {
        if ( connectionOpportunity == null )
        {
            return false;
        }

        return FilterToAddAuthorized( rockContext, new List<ConnectionOpportunity> { connectionOpportunity }, currentPerson ).Count > 0;
    }

    /// <summary>
    /// Filters a set of connection opportunities down to the ones the
    /// current person may add a request to. The connector group fallback is
    /// a single query over the whole set rather than one query per
    /// opportunity, so this is the method to prefer when evaluating more
    /// than one opportunity.
    /// </summary>
    /// <param name="rockContext">The Rock context to use for the connector group fallback query.</param>
    /// <param name="connectionOpportunities">
    /// The opportunities to evaluate. Each one's <see cref="ConnectionOpportunity.ConnectionType"/>
    /// navigation property must be loaded, or resolvable from the supplied context.
    /// </param>
    /// <param name="currentPerson">The current person.</param>
    /// <returns>The opportunities the person may add a request to, in the supplied order.</returns>
    internal static List<ConnectionOpportunity> FilterToAddAuthorized( RockContext rockContext, List<ConnectionOpportunity> connectionOpportunities, Person currentPerson )
    {
        var authorizedOpportunities = new List<ConnectionOpportunity>();

        // No null guard on currentPerson: the Hub evaluates the authorization
        // check for an anonymous individual too, so a rule granting EDIT to All
        // Users still resolves. Only the connector fallback below is
        // person-specific, and it returns nothing when there is no person.
        if ( connectionOpportunities == null || connectionOpportunities.Count == 0 )
        {
            return authorizedOpportunities;
        }

        // Authorization is served out of the in-memory security cache, so
        // this pass costs nothing. Only the opportunities it cannot resolve
        // reach the connector query below.
        var unresolvedOpportunities = new List<ConnectionOpportunity>();

        foreach ( var opportunity in connectionOpportunities )
        {
            if ( opportunity?.ConnectionType == null )
            {
                continue;
            }

            if ( GetAuthorizationTarget( opportunity.ConnectionType, opportunity ).IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                authorizedOpportunities.Add( opportunity );
            }
            else
            {
                unresolvedOpportunities.Add( opportunity );
            }
        }

        if ( unresolvedOpportunities.Count == 0 )
        {
            return authorizedOpportunities;
        }

        var connectorOpportunityIds = GetConnectorGroupsByOpportunity( rockContext, unresolvedOpportunities.Select( o => o.Id ).ToList(), currentPerson ).Keys;

        authorizedOpportunities.AddRange( unresolvedOpportunities.Where( o => connectorOpportunityIds.Contains( o.Id ) ) );

        // Restore the caller's ordering, which the two-pass split above lost.
        var authorizedOpportunityIds = new HashSet<int>( authorizedOpportunities.Select( o => o.Id ) );

        return connectionOpportunities
            .Where( o => o != null && authorizedOpportunityIds.Contains( o.Id ) )
            .ToList();
    }

    /// <summary>
    /// Determines whether the current person may add a request anywhere,
    /// that is, to at least one active opportunity under at least one
    /// active connection type.
    /// </summary>
    /// <param name="rockContext">The Rock context to use for database queries.</param>
    /// <param name="currentPerson">The current person.</param>
    /// <returns><c>true</c> if the person may add a request somewhere.</returns>
    internal static bool CanAddRequestAnywhere( RockContext rockContext, Person currentPerson )
    {
        return GetAddAuthorizedConnectionTypeIds( rockContext, currentPerson ).Count > 0;
    }

    /// <summary>
    /// Gets the identifiers of the active connection types that have at
    /// least one active opportunity the current person may add a request
    /// to.
    /// </summary>
    /// <remarks>
    /// The Hub's type-level overload requires the person to be authorized on
    /// <c>every</c> opportunity in the type, because there it gates actions
    /// that operate across the whole type. The mobile Add button instead
    /// opens a wizard where the person picks a type and then an opportunity,
    /// so the question here is whether <c>any</c> opportunity is reachable.
    /// Using the Hub's All() semantics would hide an Add button that would
    /// have worked.
    /// </remarks>
    /// <param name="rockContext">The Rock context to use for database queries.</param>
    /// <param name="currentPerson">The current person.</param>
    /// <returns>The set of authorized <see cref="ConnectionType"/> identifiers, empty when there are none.</returns>
    internal static HashSet<int> GetAddAuthorizedConnectionTypeIds( RockContext rockContext, Person currentPerson )
    {
        var authorizedConnectionTypeIds = new HashSet<int>();

        var opportunities = new ConnectionOpportunityService( rockContext )
            .Queryable()
            .AsNoTracking()
            .Include( o => o.ConnectionType )
            .Where( o => o.IsActive && o.ConnectionType.IsActive )
            .ToList();

        foreach ( var opportunity in FilterToAddAuthorized( rockContext, opportunities, currentPerson ) )
        {
            authorizedConnectionTypeIds.Add( opportunity.ConnectionTypeId );
        }

        return authorizedConnectionTypeIds;
    }

    #endregion

    #region Edit Gate

    /// <summary>
    /// Determines whether the current person may edit an existing request.
    /// Ports the Hub's <c>CanEditSpecifiedConnectionRequests</c>: an
    /// EnableRequestSecurity-aware EDIT check, then a fallback that grants
    /// the assigned connector, and then active connector group membership
    /// subject to the campus rule.
    /// </summary>
    /// <remarks>
    /// The connector fallback deliberately applies in both branches, matching
    /// the Hub, where the fallback sits outside the request-security if/else.
    /// Nesting it inside the non-secured branch would deny a connector on a
    /// request-secured type that the web would let through.
    /// </remarks>
    /// <param name="rockContext">The Rock context to use for the connector group fallback query.</param>
    /// <param name="connectionRequest">
    /// The request being authorized. Its <see cref="ConnectionRequest.ConnectionOpportunity"/>,
    /// that opportunity's <see cref="ConnectionOpportunity.ConnectionType"/>, and
    /// <see cref="ConnectionRequest.ConnectorPersonAlias"/> navigation properties must be loaded.
    /// </param>
    /// <param name="currentPerson">The current person.</param>
    /// <returns><c>true</c> if the person may edit the request.</returns>
    internal static bool CanEditRequest( RockContext rockContext, ConnectionRequest connectionRequest, Person currentPerson )
    {
        if ( connectionRequest == null )
        {
            return false;
        }

        var connectionOpportunity = connectionRequest.ConnectionOpportunity;

        if ( connectionOpportunity == null )
        {
            return false;
        }

        var enableRequestSecurity = connectionOpportunity.ConnectionType?.EnableRequestSecurity == true;

        var canEdit = enableRequestSecurity
            ? connectionRequest.IsAuthorized( Authorization.EDIT, currentPerson )
            : connectionOpportunity.IsAuthorized( Authorization.EDIT, currentPerson );

        if ( canEdit )
        {
            return true;
        }

        // The assigned connector may always work their own request. Everything
        // from here on is person-specific, so an anonymous individual can only
        // ever have been granted by the authorization check above.
        if ( currentPerson == null )
        {
            return false;
        }

        if ( connectionRequest.ConnectorPersonAlias != null
            && connectionRequest.ConnectorPersonAlias.PersonId == currentPerson.Id )
        {
            return true;
        }

        var connectorGroupsByOpportunity = GetConnectorGroupsByOpportunity( rockContext, new List<int> { connectionRequest.ConnectionOpportunityId }, currentPerson );

        if ( !connectorGroupsByOpportunity.TryGetValue( connectionRequest.ConnectionOpportunityId, out var connectorGroups ) )
        {
            return false;
        }

        // A campus-specific connector group only grants on requests for its
        // own campus. A single-campus install, a group with no campus, and a
        // request with no campus (which includes new unsaved requests) are
        // all treated as unrestricted.
        var activeCampusCount = CampusCache.All().Count( c => c.IsActive ?? true );

        return activeCampusCount == 1
            || connectorGroups.Any( g => !g.CampusId.HasValue )
            || !connectionRequest.CampusId.HasValue
            || connectorGroups.Any( g => g.CampusId == connectionRequest.CampusId.Value );
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the appropriate authorization target for the specified connection
    /// opportunity based on whether request-level security is enabled on the
    /// connection type. Mirrors the Hub's <c>GetAuthorizationTarget</c>.
    /// </summary>
    /// <param name="connectionType">The connection type to check for request security configuration.</param>
    /// <param name="connectionOpportunity">The connection opportunity to get the authorization target for.</param>
    /// <returns>
    /// A <see cref="ConnectionRequest"/> if <see cref="ConnectionType.EnableRequestSecurity"/>
    /// is enabled; otherwise the <see cref="ConnectionOpportunity"/> itself.
    /// </returns>
    private static ISecured GetAuthorizationTarget( ConnectionType connectionType, ConnectionOpportunity connectionOpportunity )
    {
        if ( connectionType.EnableRequestSecurity )
        {
            return new ConnectionRequest
            {
                ConnectionTypeId = connectionType.Id,
                ConnectionOpportunityId = connectionOpportunity.Id,
                ConnectionOpportunity = connectionOpportunity
            };
        }

        return connectionOpportunity;
    }

    /// <summary>
    /// Gets, per opportunity, the active connector groups on which the
    /// current person is an active member. Inactive and archived connector
    /// groups, and inactive and archived group members, are excluded so they
    /// grant nothing. This is one query over the whole set rather than one
    /// query per opportunity.
    /// </summary>
    /// <param name="rockContext">The Rock context to use for the query.</param>
    /// <param name="opportunityIds">The opportunities to evaluate.</param>
    /// <param name="currentPerson">The current person.</param>
    /// <returns>
    /// A dictionary keyed on Connection Opportunity Id. Only opportunities the
    /// person connects for are present, so key presence alone answers the
    /// non-campus-aware form of the question.
    /// </returns>
    private static Dictionary<int, List<ConnectionOpportunityConnectorGroup>> GetConnectorGroupsByOpportunity( RockContext rockContext, List<int> opportunityIds, Person currentPerson )
    {
        if ( currentPerson == null || opportunityIds.Count == 0 )
        {
            return new Dictionary<int, List<ConnectionOpportunityConnectorGroup>>();
        }

        var currentPersonId = currentPerson.Id;

        return new ConnectionOpportunityConnectorGroupService( rockContext )
            .Queryable()
            .AsNoTracking()
            .Where( cg => opportunityIds.Contains( cg.ConnectionOpportunityId )
                && cg.ConnectorGroup != null
                && cg.ConnectorGroup.IsActive
                && !cg.ConnectorGroup.IsArchived
                && cg.ConnectorGroup.Members.Any( m => m.PersonId == currentPersonId
                    && m.GroupMemberStatus == GroupMemberStatus.Active
                    && !m.IsArchived ) )
            .ToList()
            .GroupBy( cg => cg.ConnectionOpportunityId )
            .ToDictionary( g => g.Key, g => g.ToList() );
    }

    #endregion
}
