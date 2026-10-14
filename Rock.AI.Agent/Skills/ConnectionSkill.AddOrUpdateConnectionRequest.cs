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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ConnectionSkill
{
    #region Tool(s)

    [Description( "Adds new or updates existing connection request." )]
    [AgentToolGuid( "8ee3913a-9bca-4971-a490-90abfc1690c3" )]
    public AgentToolResult AddOrUpdateConnectionRequest(
        [Description( "Required when editing an existing connection request." )]
        string connectionRequestIdKey = null,

        [Description( "Only valid when adding new connection request." )]
        string connectionOpportunityIdKey = null,

        [Description( "Only valid and required when adding a new connection request." )]
        string personIdKey = null,

        SetOrClear<string> connectorPersonIdKey = null,
        ConnectionState? connectionState = null,
        string connectionStatusIdKey = null,
        SetOrClear<string> comments = null,
        SetOrClear<string> placementGroupIdKey = null,
        List<AttributeValueResult> attributeValues = null )
    {
        var currentPerson = AgentRequestContext.CurrentPerson;

        if ( currentPerson == null )
        {
            return Error( "You must be logged in to add or update a connection request." );
        }

        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var connectionRequestService = new ConnectionRequestService( rockContext );

        ConnectionRequest connectionRequest;

        if ( connectionRequestIdKey.IsNotNullOrWhiteSpace() )
        {
            connectionRequest = helper.GetRequiredEntity<ConnectionRequest>( connectionRequestIdKey, checkSecurity: true );

            // This handles opportunity EDIT, connector groups, the assigned
            // connector and request security the same way the connection
            // blocks do.
            if ( connectionRequest != null && !connectionRequestService.IsAuthorizedToEdit( connectionRequest, currentPerson ) )
            {
                helper.AddError( "You are not authorized to edit this connection request." );
            }

            if ( personIdKey.IsNotNullOrWhiteSpace() )
            {
                helper.AddError( $"A connection request cannot be moved to a new person, do not provide a {nameof( personIdKey )} when editing." );
            }
        }
        else
        {
            connectionRequest = rockContext.Set<ConnectionRequest>().Create();
            connectionRequestService.Add( connectionRequest );

            var connectionOpportunity = helper.GetOptionalEntity<ConnectionOpportunity>( connectionOpportunityIdKey, checkSecurity: true );

            if ( connectionOpportunity != null )
            {
                connectionRequest.ConnectionOpportunity = connectionOpportunity;
                connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
                connectionRequest.ConnectionTypeId = connectionOpportunity.ConnectionTypeId;

                if ( !IsAuthorizedToAddRequest( connectionRequest, currentPerson, rockContext ) )
                {
                    helper.AddError( "You are not authorized to add requests to this connection opportunity." );
                }
            }
            else
            {
                helper.AddError( $"You must provide either a {nameof( connectionRequestIdKey )} to update an existing connection request or a {nameof( connectionOpportunityIdKey )} to add a new connection request." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( connectionState.HasValue )
        {
            connectionRequest.ConnectionState = connectionState.Value;
        }

        // Process the connection status. If it is not specified and we are
        // adding a new request then use the default status if available.
        if ( connectionRequest.Id == 0 )
        {
            var connectionStatus = GetConnectionStatusOrDefault( helper, connectionStatusIdKey, connectionRequest.ConnectionOpportunity );

            if ( connectionStatus != null )
            {
                connectionRequest.ConnectionStatus = connectionStatus;
                connectionRequest.ConnectionStatusId = connectionStatus.Id;
            }
        }
        else
        {
            var status = helper.GetOptionalEntity<ConnectionStatus>( connectionStatusIdKey );

            if ( status != null && status.ConnectionTypeId == connectionRequest.ConnectionTypeId )
            {
                connectionRequest.ConnectionStatus = status;
                connectionRequest.ConnectionStatusId = status.Id;
            }
            else if ( status != null )
            {
                helper.AddError( $"The {nameof( connectionStatusIdKey )} is not valid." );
                helper.AddInstructions( $"Call the {nameof( LookupConnectionTypesAndOpportunities )}function to determine available statuses that are valid for this connection request." );
            }
        }

        helper.UpdateProperty( connectionRequest, cr => cr.Comments, comments );
        helper.UpdateNavigationProperty( connectionRequest, cr => cr.PersonAlias, personIdKey );
        helper.UpdateNavigationProperty( connectionRequest, cr => cr.ConnectorPersonAlias, connectorPersonIdKey );
        helper.UpdateNavigationProperty( connectionRequest, cr => cr.AssignedGroup, placementGroupIdKey, checkSecurity: true );
        helper.SetAttributeValues( connectionRequest, attributeValues );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( GetFullConnectionRequestResult( connectionRequest ) )
            .WithHistoryContent( new KeyNameResult
            {
                Id = connectionRequest.Id,
                Name = connectionRequest.ToString()
            } )
            .WithInstructions( $"The connection request has been {( connectionRequestIdKey.IsNullOrWhiteSpace() ? "created" : "updated" )}." );
    }


    #endregion

    /// <summary>
    /// Determines if the person is allowed to add a new request to the
    /// opportunity of <paramref name="connectionRequest"/>. This follows the
    /// connection request detail block, which allows anybody with EDIT on
    /// the opportunity or in any of its connector groups. A new request does
    /// not have a campus yet, so the campus of the connector group is not
    /// considered.
    /// </summary>
    /// <param name="connectionRequest">The new connection request, which must have its opportunity set.</param>
    /// <param name="person">The person that is adding the request.</param>
    /// <param name="rockContext">The context to use when checking connector groups.</param>
    /// <returns><c>true</c> if the person can add the request; otherwise <c>false</c>.</returns>
    private static bool IsAuthorizedToAddRequest( ConnectionRequest connectionRequest, Model.Person person, RockContext rockContext )
    {
        // With no connector assigned yet this checks EDIT on the opportunity.
        if ( connectionRequest.IsAuthorized( Authorization.EDIT, person ) )
        {
            return true;
        }

        var connectionOpportunityId = connectionRequest.ConnectionOpportunityId;

        return new ConnectionOpportunityConnectorGroupService( rockContext )
            .Queryable()
            .Where( cocg => cocg.ConnectionOpportunityId == connectionOpportunityId
                && cocg.ConnectorGroup.Members.Any( m => m.PersonId == person.Id && m.GroupMemberStatus == GroupMemberStatus.Active ) )
            .Any();
    }

    private static ConnectionStatus GetConnectionStatusOrDefault( AgentToolHelper helper, string statusIdKey, ConnectionOpportunity opportunity )
    {
        if ( statusIdKey.IsNotNullOrWhiteSpace() )
        {
            if ( !helper.TryGetRequiredEntity<ConnectionStatus>( statusIdKey, out var status ) )
            {
                return null;
            }

            if ( opportunity != null && status.ConnectionTypeId != opportunity.ConnectionTypeId )
            {
                helper.AddError( $"The {nameof( statusIdKey )} is not valid." );
                helper.AddInstructions( $"Call the {nameof( LookupConnectionTypesAndOpportunities )} function to determine available statuses that match the specified opportunity." );

                return null;
            }

            return status;
        }
        else if ( opportunity != null )
        {
            var status = opportunity.ConnectionType.ConnectionStatuses.FirstOrDefault();

            if ( status == null )
            {
                helper.AddError( $"You must provide a {nameof( statusIdKey )}." );
                helper.AddInstructions( $"Call the {nameof( LookupConnectionTypesAndOpportunities )} function to determine available statuses that match the specified opportunity." );

                return null;
            }

            return status;
        }

        return null;
    }
}
