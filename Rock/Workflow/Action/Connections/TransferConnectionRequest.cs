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
using System.Data.Entity;
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

    [WorkflowAttribute( "Connection Request Attribute",
        Description = "The attribute that contains the connection request.",
        IsRequired = true,
        DefaultValue = "",
        Category = "",
        Order = 0,
        Key = AttributeKey.ConnectionRequestAttribute,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.ConnectionRequestFieldType" } )]

    [WorkflowAttribute( "Connection Opportunity Attribute",
        Description = "The attribute that contains the type of the new connection opportunity.",
        IsRequired = true,
        DefaultValue = "",
        Category = "",
        Order = 1,
        Key = AttributeKey.ConnectionOpportunityAttribute,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.ConnectionOpportunityFieldType" } )]

    [WorkflowTextOrAttribute("Transfer Note",
        "Transfer Note Attribute",
        Description = "The note to include with the transfer activity.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 2,
        Key = AttributeKey.TransferNote )]

    [EnumField( "Connection State",
        Description = "The connection State to set the connection request.",
        EnumSourceType = typeof( ConnectionState ),
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.ConnectionState )]

    [IntegerField( "Connection Status Id",
        Description = "The connection status id that request apply to",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.ConnectionStatusId )]

    [Rock.SystemGuid.EntityTypeGuid( "308B46FD-6D87-471B-AA98-AAE1894B0D49")]
    public class TransferConnectionRequest : ActionComponent
    {
        #region Workflow Attributes

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The connection state
            /// </summary>
            public const string ConnectionState = "ConnectionState";

            /// <summary>
            /// The connection status identifier
            /// </summary>
            public const string ConnectionStatusId = "ConnectionStatusId";

            /// <summary>
            /// The connection opportunity attribute key
            /// </summary>
            public const string ConnectionOpportunityAttribute = "ConnectionOpportunityAttribute";

            /// <summary>
            /// The connection request attribute key
            /// </summary>
            public const string ConnectionRequestAttribute = "ConnectionRequestAttribute";

            /// <summary>
            /// The transfer note attribute key.
            /// </summary>
            public const string TransferNote = "TransferNote|TransferNoteAttribute";
        }

        #endregion Workflow Attributes

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
            Guid connectionRequestGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, AttributeKey.ConnectionRequestAttribute ).AsGuid() ).AsGuid();
            var connectionRequestService = new ConnectionRequestService( rockContext );
            request = connectionRequestService.Get( connectionRequestGuid );
            if ( request == null )
            {
                errorMessages.Add( "Invalid Connection Request Attribute or Value!" );
                return false;
            }

            // Get the opportunity
            ConnectionOpportunity opportunity = null;
            Guid opportunityTypeGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, AttributeKey.ConnectionOpportunityAttribute ).AsGuid() ).AsGuid();
            opportunity = new ConnectionOpportunityService( rockContext )
                .Queryable( "ConnectionType.ConnectionStatuses" )
                .AsNoTracking()
                .FirstOrDefault(opp => opp.Guid == opportunityTypeGuid );
            if ( opportunity == null )
            {
                errorMessages.Add( "Invalid Connection Opportunity Attribute or Value!" );
                return false;
            }

            var status = opportunity.ConnectionType.ConnectionStatuses.Where( s => s.IsDefault ).FirstOrDefault();
            if ( status == null )
            {
                errorMessages.Add( "Connection Type is not in valid state. Connection Type must be having at least one default status." );
                return false;
            }

            // Get the transfer note
            string note = GetAttributeValue( action, AttributeKey.TransferNote, true );

            if ( request != null && opportunity != null )
            {
                request.ConnectionOpportunityId = opportunity.Id;
                var connectionStatusId = GetAttributeValue( action, AttributeKey.ConnectionStatusId ).AsIntegerOrNull();
                if ( connectionStatusId.HasValue )
                {
                    if ( opportunity.ConnectionType.ConnectionStatuses.Any( s => s.Id == connectionStatusId.Value ) )
                    {
                        request.ConnectionStatusId = connectionStatusId.Value;
                    }
                    else
                    {
                        errorMessages.Add( "The provided Connection Status Id was not valid for the Connection Type the request is being transferred to, therefore the default Status was used instead." );
                        request.ConnectionStatusId = status.Id;
                    }
                }
                else if ( request.ConnectionTypeId != opportunity.ConnectionTypeId )
                {
                    request.ConnectionStatusId = status.Id;
                }

                var connectionState = this.GetAttributeValue( action, AttributeKey.ConnectionState ).ConvertToEnumOrNull<ConnectionState>();
                if ( connectionState.HasValue )
                {
                    request.ConnectionState = connectionState.Value;
                }

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
                connectionRequestActivity.ConnectorPersonAliasId = request.ConnectorPersonAliasId;
                new ConnectionRequestActivityService( rockContext ).Add( connectionRequestActivity );

                rockContext.SaveChanges();
            }

            return true;
        }
    }
}