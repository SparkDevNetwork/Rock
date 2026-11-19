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

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.ConnectionSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ConnectionSkill
{
    #region Tool(s)

    [Description( "Returns a list of connection requests that match the filters." )]
    [AgentUsage( "Requests can be filtered by connection type, connection opportunity, requester, connector, created date range or connected date range. Connectors are people who are assigned a request." )]
    [AgentUsage( "Each request has one of these states. Active: the request is being worked. Inactive: the request was closed without connecting. FutureFollowUp: the request is paused until a follow-up date, then a job makes it Active. Connected: the request is done." )]
    [AgentUsage( "When connectionStates is empty, only Active and FutureFollowUp requests are returned. When you rely on this default, tell the user that Inactive and Connected requests were left out." )]
    [AgentUsage( "The connected date range only matches Connected requests, so leave connectionStates empty or set it to Connected only when you use it. Older or imported connected requests may have no connected date, so a connected date range can miss them." )]
    [AgentToolGuid( "DC03271E-2C54-D5AF-4F18-9CCC69F25202" )]
    public AgentToolResult ListConnectionRequests(
        string connectionTypeIdKey = null,
        string connectionOpportunityIdKey = null,
        string requesterPersonIdKey = null,
        string connectorPersonIdKey = null,
        [Description( "The states of the requests to return. Allowed values: Active, Inactive, FutureFollowUp, Connected. When empty, Active and FutureFollowUp are returned, or only Connected when a connected date range is given." )]
        List<ConnectionState> connectionStates = null,
        DateTime? createdStartDate = null,
        DateTime? createdEndDate = null,
        DateTime? connectedStartDate = null,
        DateTime? connectedEndDate = null,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;
        var hasCreatedRange = createdStartDate.HasValue || createdEndDate.HasValue;
        var hasConnectedRange = connectedStartDate.HasValue || connectedEndDate.HasValue;

        var query = new ConnectionRequestService( AgentRequestContext.RockContext )
            .Queryable();

        /*
            11/19/26 - CLAUDE

            Requests are filtered by ConnectionState. When no states are given,
            only Active and FutureFollowUp are returned, because those are the
            requests still being worked. A FutureFollowUp request becomes Active
            again on its follow-up date, so it is open work too.

            The connected date range filters on ConnectedDateTime, but that date
            does not show the current state. The save hook sets it only when an
            existing request changes to Connected, and it is never cleared when a
            request is reopened. So a connected date range must also limit the
            results to the Connected state. With no states given, it searches
            Connected only. With any other state given, it is an error instead
            of a list that mixes in reopened requests or comes back empty.

            Reason: ConnectedDateTime does not reliably reflect the request's state.
        */
        var selectedStates = GetListConnectionRequestStates( helper, connectionStates, hasConnectedRange );

        query = query.Where( cr => selectedStates.Contains( cr.ConnectionState ) );
        query = helper.WhereOptionalIdKey( query, cr => cr.PersonAlias.PersonId, requesterPersonIdKey );
        query = helper.WhereOptionalIdKey( query, cr => cr.ConnectorPersonAlias.PersonId, connectorPersonIdKey );
        query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionTypeId, connectionTypeIdKey );
        query = helper.WhereOptionalIdKey( query, cr => cr.ConnectionOpportunityId, connectionOpportunityIdKey );
        query = helper.WhereOptionalPropertyBetween( query, cr => cr.CreatedDateTime, createdStartDate, createdEndDate );
        query = helper.WhereOptionalPropertyBetween( query, cr => cr.ConnectedDateTime, connectedStartDate, connectedEndDate );

        // The connection states only choose which requests to include, so they
        // do not count as a filter that limits the results.
        var hasAnyFilters = !string.IsNullOrWhiteSpace( connectionTypeIdKey )
            || !string.IsNullOrWhiteSpace( connectionOpportunityIdKey )
            || !string.IsNullOrWhiteSpace( requesterPersonIdKey )
            || !string.IsNullOrWhiteSpace( connectorPersonIdKey )
            || hasCreatedRange
            || hasConnectedRange;

        if ( !hasAnyFilters )
        {
            helper.AddError( "At least one of these filters must be provided to limit the results returned: connection type, connection opportunity, requester, connector, created date range or connected date range. The connection states alone are not enough." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<ConnectionRequest>( currentPerson, qry => qry
            .OrderByDescending( cr => cr.CreatedDateTime.HasValue )
            .ThenByDescending( cr => cr.CreatedDateTime )
            .ThenBy( cr => cr.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        cursorPage.Items.LoadAttributes( AgentRequestContext.RockContext );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( cr => new ConnectionRequestResult
            {
                Id = cr.Id,
                Guid = cr.Guid,
                Requester = PersonResult.NameOnly( cr.PersonAlias ),
                ConnectionState = cr.ConnectionState,
                ConnectionStatus = new KeyNameResult
                {
                    Id = cr.ConnectionStatus.Id,
                    Guid = cr.ConnectionStatus.Guid,
                    Name = cr.ConnectionStatus.Name
                },
                ConnectionOpportunity = new ConnectionOpportunityResult
                {
                    Id = cr.ConnectionOpportunity.Id,
                    Guid = cr.ConnectionOpportunity.Guid,
                    Name = cr.ConnectionOpportunity.Name,
                    ConnectionType = new ConnectionTypeResult
                    {
                        Id = cr.ConnectionOpportunity.ConnectionType.Id,
                        Guid = cr.ConnectionOpportunity.ConnectionType.Guid,
                        Name = cr.ConnectionOpportunity.ConnectionType.Name
                    }
                },
                CreatedDateTime = cr.CreatedDateTime,
                ConnectedDateTime = cr.ConnectedDateTime,
                Connector = PersonResult.NameOnly( cr.ConnectorPersonAlias ),
                AttributeValues = cr.GetGridAttributeValueResults( AgentRequestContext ).ToList(),
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( cr => new KeyNameResult
        {
            Id = cr.Id,
            Guid = cr.Guid,
            Name = cr.ToString()
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the connection states that the ListConnectionRequests tool will
    /// return. Any problem with the requested states is added to the helper
    /// as an error.
    /// </summary>
    /// <param name="helper">The helper that collects any errors.</param>
    /// <param name="connectionStates">The states the language model asked for, which may be empty.</param>
    /// <param name="hasConnectedRange"><c>true</c> if a connected date range was provided.</param>
    /// <returns>The distinct connection states to include in the results.</returns>
    private static List<ConnectionState> GetListConnectionRequestStates( AgentToolHelper helper, List<ConnectionState> connectionStates, bool hasConnectedRange )
    {
        if ( connectionStates == null || connectionStates.Count == 0 )
        {
            if ( hasConnectedRange )
            {
                return new List<ConnectionState> { ConnectionState.Connected };
            }

            return new List<ConnectionState> { ConnectionState.Active, ConnectionState.FutureFollowUp };
        }

        // The JSON enum converter accepts numbers as well as names, so an
        // unknown number would otherwise pass through as an undefined state.
        var unknownStates = connectionStates
            .Where( state => !Enum.IsDefined( typeof( ConnectionState ), state ) )
            .ToList();

        if ( unknownStates.Count > 0 )
        {
            helper.AddError( $"The value of connectionStates contains unknown states: {string.Join( ", ", unknownStates.Select( state => ( int ) state ) )}. Allowed values: Active, Inactive, FutureFollowUp, Connected." );
        }

        var selectedStates = connectionStates.Distinct().ToList();

        // A reopened request keeps its old ConnectedDateTime, so any state other
        // than Connected would let open requests match a connected date range.
        if ( hasConnectedRange && selectedStates.Any( state => state != ConnectionState.Connected ) )
        {
            helper.AddError( "The connected date range only matches requests in the Connected state. Leave connectionStates empty, or set it to Connected only, when you use a connected date range." );
        }

        return selectedStates;
    }

    #endregion
}
