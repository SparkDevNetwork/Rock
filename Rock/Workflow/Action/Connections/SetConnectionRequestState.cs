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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets the state of a connection request.
    /// </summary>
    [ActionCategory( "Connections" )]
    [Description( "Sets the state of a connection request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Connection Request Set State" )]

    [WorkflowAttribute( "Connection Request Attribute", "The attribute that contains the connection request.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.ConnectionRequestFieldType" } )]

    [WorkflowAttribute( "Connection State Attribute", "The attribute that contains the connection state.", false, "", "", 1, null,
        new string[] { "Rock.Field.Types.ConnectionStateFieldType" } )]
    [ConnectionStateField("Connection State", "The connection state to use (if Connection State Attribute is not specified).", false, "", "", 2)]

    [WorkflowAttribute( "Follow Up Date Attribute", "The attribute that contains the follow-up date when state is being set to Future Follow Up.", false, "", "", 3, null,
        new string[] { "Rock.Field.Types.DateFieldType" } )]
    [DateField( "Follow Up Date", "The follow-up date when state is being set to Future Follow Up (if Follow Up Date Attribute is not specified).", false, "", "", 4 )]

    public class SetConnectionRequestState : ActionComponent
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

            // Get connection state
            ConnectionState? connectionState = null;
            Guid? connectionStateAttributeGuid = GetAttributeValue( action, "ConnectionStateAttribute" ).AsGuidOrNull();
            if ( connectionStateAttributeGuid.HasValue )
            {
                connectionState = action.GetWorklowAttributeValue( connectionStateAttributeGuid.Value ).ConvertToEnumOrNull<ConnectionState>();
            }
            if ( connectionState == null )
            {
                connectionState = GetAttributeValue( action, "ConnectionState" ).ConvertToEnumOrNull<ConnectionState>();
            }
            if ( connectionState == null )
            {
                errorMessages.Add( "Invalid Connection State Attribute or Value!" );
                return false;
            }
            request.ConnectionState = connectionState.Value;

            // Get follow up date
            if ( connectionState.Value == ConnectionState.FutureFollowUp )
            {
                DateTime? followupDate = null;
                Guid? FollowUpDateAttributeGuid = GetAttributeValue( action, "FollowUpDateAttribute" ).AsGuidOrNull();
                if ( FollowUpDateAttributeGuid.HasValue )
                {
                    followupDate = action.GetWorklowAttributeValue( FollowUpDateAttributeGuid.Value ).AsDateTime();
                }
                if ( followupDate == null )
                {
                    followupDate = GetAttributeValue( action, "FollowUpDate" ).AsDateTime();
                }
                if ( followupDate == null )
                {
                    errorMessages.Add( "Invalid Follow Up DateAttribute or Value!" );
                    return false;
                }
                request.FollowupDate = followupDate.Value;
            }

            rockContext.SaveChanges();

            return true;
        }
    }
}