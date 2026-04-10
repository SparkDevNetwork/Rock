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
using Rock.Model;
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
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        ConnectionRequest connectionRequest;

        if ( connectionRequestIdKey.IsNotNullOrWhiteSpace() )
        {
            connectionRequest = helper.GetRequiredEntity<ConnectionRequest>( connectionRequestIdKey, checkSecurity: true );
        }
        else
        {
            connectionRequest = rockContext.Set<ConnectionRequest>().Create();
            new ConnectionRequestService( rockContext ).Add( connectionRequest );

            var connectionOpportunity = helper.GetOptionalEntity<ConnectionOpportunity>( connectionOpportunityIdKey, checkSecurity: true );

            if ( connectionOpportunity != null )
            {
                connectionRequest.ConnectionOpportunity = connectionOpportunity;
                connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
                connectionRequest.ConnectionTypeId = connectionOpportunity.ConnectionTypeId;
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
