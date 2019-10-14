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
    /// Creates a new connection request.
    /// </summary>
    [ActionCategory( "Connections" )]
    [Description( "Transfers a connection request to a different opportunity type." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Connection Request Transfer" )]

    [WorkflowAttribute( "Connection Request Attribute", "The attribute that contains the connection request.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.ConnectionRequestFieldType" } )]
    [WorkflowAttribute( "Connection Opportunity Attribute", "The attribute that contains the type of the new connection opportunity.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.ConnectionOpportunityFieldType" } )]
    [WorkflowTextOrAttribute("Transfer Note", "Transfer Note Attribute", "The note to include with the transfer activity.", false, "", "", 2 )]
    public class TransferConnectionRequest : ActionComponent
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
            var connectionRequestService = new ConnectionRequestService( rockContext );
            request = connectionRequestService.Get( connectionRequestGuid );
            if ( request == null )
            {
                errorMessages.Add( "Invalid Connection Request Attribute or Value!" );
                return false;
            }

            // Get the opportunity
            ConnectionOpportunity opportunity = null;
            Guid opportunityTypeGuid = action.GetWorklowAttributeValue( GetAttributeValue( action, "ConnectionOpportunityAttribute" ).AsGuid() ).AsGuid();
            opportunity = new ConnectionOpportunityService( rockContext ).Get( opportunityTypeGuid );
            if ( opportunity == null )
            {
                errorMessages.Add( "Invalid Connection Opportunity Attribute or Value!" );
                return false;
            }

            // Get the transfer note
            string note = GetAttributeValue( action, "TransferNote", true );

            if ( request != null && opportunity != null )
            {
                request.ConnectionOpportunityId = opportunity.Id;

                var guid = Rock.SystemGuid.ConnectionActivityType.TRANSFERRED.AsGuid();
                var transferredActivityId = new ConnectionActivityTypeService( rockContext )
                    .Queryable()
                    .Where( t => t.Guid == guid )
                    .Select( t => t.Id )
                    .FirstOrDefault();

                ConnectionRequestActivity connectionRequestActivity = new ConnectionRequestActivity();
                connectionRequestActivity.ConnectionRequestId = request.Id;
                connectionRequestActivity.ConnectionOpportunityId = opportunity.Id;
                connectionRequestActivity.ConnectionActivityTypeId = transferredActivityId;
                connectionRequestActivity.Note = note;
                new ConnectionRequestActivityService( rockContext ).Add( connectionRequestActivity );

                rockContext.SaveChanges();
            }

            return true;
        }
    }
}