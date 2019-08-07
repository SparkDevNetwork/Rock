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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets the status of a connection request.
    /// </summary>
    [ActionCategory( "Connections" )]
    [Description( "Sets the status of a connection request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Connection Request Set Status" )]

    [WorkflowAttribute( "Connection Request Attribute", "The attribute that contains the connection request.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.ConnectionRequestFieldType" } )]
    [WorkflowAttribute( "Connection Status Attribute", "The attribute that contains the connection status.", false, "", "", 1, null,
        new string[] { "Rock.Field.Types.ConnectionStatusFieldType" } )]
    [ConnectionStatusField( "Connection Status", "The connection status to use (if Connection Status Attribute is not specified).", false, "", "", 2 )]

    public class SetConnectionRequestStatus : ActionComponent
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

            // Get the connection request
            ConnectionRequest request = null;
            Guid connectionRequestGuid = action.GetWorklowAttributeValue( GetAttributeValue( action, "ConnectionRequestAttribute" ).AsGuid() ).AsGuid();
            request = new ConnectionRequestService( rockContext ).Get( connectionRequestGuid );
            if ( request == null )
            {
                errorMessages.Add( "Invalid Connection Request Attribute or Value!" );
                return false;
            }

            // Get connection status
            ConnectionStatus status = null;
            Guid? connectionStatusGuid = null;
            Guid? connectionStatusAttributeGuid = GetAttributeValue( action, "ConnectionStatusAttribute" ).AsGuidOrNull();
            if ( connectionStatusAttributeGuid.HasValue )
            {
                connectionStatusGuid = action.GetWorklowAttributeValue( connectionStatusAttributeGuid.Value ).AsGuidOrNull();
                if ( connectionStatusGuid.HasValue )
                {
                    status = request.ConnectionOpportunity.ConnectionType.ConnectionStatuses
                        .Where( s => s.Guid.Equals( connectionStatusGuid.Value ) )
                        .FirstOrDefault();
                }
            }
            if ( status == null )
            {
                connectionStatusGuid = GetAttributeValue( action, "ConnectionStatus" ).AsGuidOrNull();
                if ( connectionStatusGuid.HasValue )
                {
                    status = request.ConnectionOpportunity.ConnectionType.ConnectionStatuses
                        .Where( s => s.Guid.Equals( connectionStatusGuid.Value ) )
                        .FirstOrDefault();
                }
            }
            if ( status == null )
            {
                errorMessages.Add( "Invalid Connection Status Attribute or Value!" );
                return false;
            }

            request.ConnectionStatusId = status.Id;
            rockContext.SaveChanges();

            return true;
        }
    }
}