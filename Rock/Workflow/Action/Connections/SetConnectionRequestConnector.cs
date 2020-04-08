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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets the connector to a connection request.
    /// </summary>
    [ActionCategory( "Connections" )]
    [Description( "Sets the connector to a connection request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Connection Request Set Connector" )]

    [WorkflowAttribute( "Connection Request Attribute", "The attribute that contains the connection request needing a connector.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.ConnectionRequestFieldType" } )]
    [WorkflowAttribute( "Person Attribute", "The Person attribute that contains the person who is will be the connector.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [BooleanField( "Ignore If Connector Exists", "If the connection request already has a connector set, this action will not change the connector.", true, "", 2, "Ignore" )]

    public class SetConnectionRequestConnector : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var mergeFields = GetMergeFields( action );

            // Get the connection request
            ConnectionRequest connectionRequest = null;
            Guid connectionRequestGuid = action.GetWorklowAttributeValue( GetAttributeValue( action, "ConnectionRequestAttribute" ).AsGuid() ).AsGuid();
            connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestGuid );
            if ( connectionRequest == null )
            {
                errorMessages.Add( "Invalid Connection Request Attribute or Value!" );
                return false;
            }

            // Get the connector
            int? personAliasId = null;
            Guid? personAttributeGuid = GetAttributeValue( action, "PersonAttribute" ).AsGuidOrNull();
            if ( personAttributeGuid.HasValue )
            {
                Guid? personAliasGuid = action.GetWorklowAttributeValue( personAttributeGuid.Value ).AsGuidOrNull();
                if ( personAliasGuid.HasValue )
                {
                    var personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid.Value );
                    if ( personAlias != null )
                    {
                        personAliasId = personAlias.Id;
                    }
                }
                else
                {
                    errorMessages.Add( "Invalid Person Attribute or Value!" );
                    return false;
                }
            }

            // Set the connector to the connection
            if ( !connectionRequest.ConnectorPersonAliasId.HasValue || !GetAttributeValue( action, "Ignore" ).AsBoolean() )
            {
                int? oldConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;
                int? newConnectorPersonAliasId = personAliasId;

                connectionRequest.ConnectorPersonAliasId = newConnectorPersonAliasId;
                rockContext.SaveChanges();

                if ( newConnectorPersonAliasId.HasValue && !newConnectorPersonAliasId.Equals( oldConnectorPersonAliasId ) )
                {
                    var guid = Rock.SystemGuid.ConnectionActivityType.ASSIGNED.AsGuid();
                    var assignedActivityId = new ConnectionActivityTypeService( rockContext ).Queryable()
                        .Where( t => t.Guid == guid )
                        .Select( t => t.Id )
                        .FirstOrDefault();
                    if ( assignedActivityId > 0 )
                    {
                        var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                        var connectionRequestActivity = new ConnectionRequestActivity();
                        connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                        connectionRequestActivity.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                        connectionRequestActivity.ConnectionActivityTypeId = assignedActivityId;
                        connectionRequestActivity.ConnectorPersonAliasId = newConnectorPersonAliasId;
                        connectionRequestActivityService.Add( connectionRequestActivity );
                        rockContext.SaveChanges();
                    }
                }
            }

            return true;
        }
    }
}