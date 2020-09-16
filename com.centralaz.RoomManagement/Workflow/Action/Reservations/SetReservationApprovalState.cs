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
using com.centralaz.RoomManagement.Model;
using com.centralaz.RoomManagement.Attribute;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.centralaz.RoomManagement.Workflow.Actions.Reservations
{
    /// <summary>
    /// Sets the approval state of a reservation.
    /// </summary>
    [ActionCategory( "Reservation" )]
    [Description( "Sets the state of a reservation." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reservation Set State" )]

    [WorkflowAttribute( "Reservation Attribute", "The attribute that contains the reservation.", true, "", "", 0, null,
        new string[] { "com.centralaz.RoomManagement.Field.Types.ReservationFieldType" } )]

    [WorkflowAttribute( "Approval State Attribute", "The attribute that contains the reservation approval state.", false, "", "", 1, null,
        new string[] { "com.centralaz.RoomManagement.Field.Types.ReservationApprovalStateFieldType" } )]
    [ReservationApprovalStateField( "Approval State", "The connection state to use (if Connection State Attribute is not specified).", false, "", "", 2 )]
    public class SetReservationApprovalState : ActionComponent
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

            // Get the reservation
            Reservation reservation = null;
            Guid reservationGuid = action.GetWorklowAttributeValue( GetAttributeValue( action, "ReservationAttribute" ).AsGuid() ).AsGuid();
            reservation = new ReservationService( rockContext ).Get( reservationGuid );
            if ( reservation == null )
            {
                errorMessages.Add( "Invalid Reservation Attribute or Value!" );
                return false;
            }

            // Get reservation approval state
            ReservationApprovalState? approvalState = null;
            Guid? approvalStateAttributeGuid = GetAttributeValue( action, "ApprovalStateAttribute" ).AsGuidOrNull();
            if ( approvalStateAttributeGuid.HasValue )
            {
                approvalState = action.GetWorklowAttributeValue( approvalStateAttributeGuid.Value ).ConvertToEnumOrNull<ReservationApprovalState>();
            }

            if ( approvalState == null )
            {
                approvalState = GetAttributeValue( action, "ApprovalState" ).ConvertToEnumOrNull<ReservationApprovalState>();
            }

            if ( approvalState == null )
            {
                errorMessages.Add( "Invalid Approval State Attribute or Value!" );
                return false;
            }
            var oldValue = reservation.ApprovalState;

            reservation.ApprovalState = approvalState.Value;

            if ( oldValue != reservation.ApprovalState )
            {
                var changes = new History.HistoryChangeList();
                History.EvaluateChange(
                    changes,
                    "Approval State",
                    oldValue.ToString(),
                    reservation.ApprovalState.ToString() );

                if ( changes.Any() )
                {
                    changes.Add( new History.HistoryChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, string.Format( "Updated by the '{0}' workflow", action.ActionTypeCache.ActivityType.WorkflowType.Name ) ) );
                    HistoryService.SaveChanges( rockContext, typeof( Reservation ), com.centralaz.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(), reservation.Id, changes, false );
                }

                rockContext.SaveChanges();

                if ( action.Activity != null && action.Activity.Workflow != null )
                {
                    var workflowType = action.Activity.Workflow.WorkflowTypeCache;
                    if ( workflowType != null && workflowType.LoggingLevel == WorkflowLoggingLevel.Action )
                    {
                        action.AddLogEntry( string.Format( "Updated approval state for {0} to {1}.", reservation.Name, reservation.ApprovalState.ToString() ) );
                    }
                }
            }
            return true;
        }
    }
}