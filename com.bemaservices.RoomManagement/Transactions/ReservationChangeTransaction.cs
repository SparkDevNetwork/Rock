// <copyright>
// Copyright by BEMA Software Services
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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using com.bemaservices.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;

namespace com.bemaservices.RoomManagement.Transactions
{
    /// <summary>
    /// Launches a reservation change workflow
    /// </summary>
    public class ReservationChangeTransaction : ITransaction
    {
        private EntityState State;
        private Guid? ReservationGuid;
        private int ReservationTypeId;
        private ReservationApprovalState ApprovalState;
        private ReservationApprovalState PreviousApprovalState;
        private DateTime PreviousModifiedDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public ReservationChangeTransaction( DbEntityEntry entry )
        {
            // If entity was a reservation, save the values
            var reservation = entry.Entity as Reservation;
            if ( reservation != null )
            {
                State = entry.State;
                ApprovalState = reservation.ApprovalState;
                ReservationTypeId = reservation.ReservationTypeId;

                // If this isn't a new reservation, get the previous state and role values
                if ( State != EntityState.Added )
                {
                    var dbStatusProperty = entry.Property( "ApprovalState" );
                    if ( dbStatusProperty != null )
                    {
                        PreviousApprovalState = ( ReservationApprovalState ) dbStatusProperty.OriginalValue;
                    }

                    var dbModifiedDateTimeProperty = entry.Property( "ModifiedDateTime" );
                    if ( dbModifiedDateTimeProperty != null )
                    {
                        PreviousModifiedDateTime = ( DateTime ) dbModifiedDateTimeProperty.OriginalValue;
                    }
                }

                // If this isn't a deleted reservation, get the reservation guid
                if ( State != EntityState.Deleted )
                {
                    ReservationGuid = reservation.Guid;
                }
            }
        }

        /// <summary>
        /// Execute method to check for any workflows to launch.
        /// </summary>
        public void Execute()
        {
            // Get all the workflows from cache
            var cachedWorkflowTriggers = ReservationWorkflowTriggerService.GetCachedTriggers();

            // If any workflows exist
            if ( cachedWorkflowTriggers != null && cachedWorkflowTriggers.Any() )
            {
                // Get the workflows associated to the reservation
                var reservationWorkflowTriggers = cachedWorkflowTriggers.Where( t => t.ReservationTypeId == ReservationTypeId ).ToList();

                if ( reservationWorkflowTriggers.Any() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        // Loop through reservationWorkflows and lauch appropriate workflow
                        foreach ( var reservationWorkflowTrigger in reservationWorkflowTriggers )
                        {
                            switch ( reservationWorkflowTrigger.TriggerType )
                            {
                                case ReservationWorkflowTriggerType.ReservationCreated:
                                    {
                                        if ( State == EntityState.Added && QualifiersMatch( rockContext, reservationWorkflowTrigger, ApprovalState, ApprovalState ) )
                                        {
                                            LaunchWorkflow( rockContext, reservationWorkflowTrigger, "Reservation Created" );
                                        }
                                        break;
                                    }
                                case ReservationWorkflowTriggerType.ReservationUpdated:
                                    {
                                        if ( State == EntityState.Modified )
                                        {
                                            LaunchWorkflow( rockContext, reservationWorkflowTrigger, "Reservation Updated" );
                                        }
                                        break;
                                    }
                                case ReservationWorkflowTriggerType.StateChanged:
                                    {
                                        if ( State == EntityState.Modified && PreviousApprovalState != ApprovalState && QualifiersMatch( rockContext, reservationWorkflowTrigger, PreviousApprovalState, ApprovalState ) )
                                        {
                                            LaunchWorkflow( rockContext, reservationWorkflowTrigger, "State Changed" );
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
        }

        private bool QualifiersMatch( RockContext rockContext, ReservationWorkflowTrigger reservationWorkflowTrigger, ReservationApprovalState prevState, ReservationApprovalState state )
        {
            var qualifierParts = ( reservationWorkflowTrigger.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var qualifierRole = qualifierParts[1].ConvertToEnumOrNull<ReservationApprovalState>();
                if ( qualifierRole.HasValue )
                {
                    matches = qualifierRole != 0 && qualifierRole == state;
                }
                else
                {
                    matches = false;
                }
            }

            if ( matches && qualifierParts.Length > 3 && !string.IsNullOrWhiteSpace( qualifierParts[3] ) )
            {
                var qualifierRole = qualifierParts[3].ConvertToEnumOrNull<ReservationApprovalState>();
                if ( qualifierRole.HasValue )
                {
                    matches = qualifierRole != 0 && qualifierRole == prevState;
                }
                else
                {
                    matches = false;
                }
            }

            return matches;
        }

        private void LaunchWorkflow( RockContext rockContext, ReservationWorkflowTrigger reservationWorkflowTrigger, string name )
        {
            var workflowType = Rock.Web.Cache.WorkflowTypeCache.Get( reservationWorkflowTrigger.WorkflowTypeId.Value );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                Reservation reservation = null;
                if ( ReservationGuid.HasValue )
                {
                    reservation = new ReservationService( rockContext ).Get( ReservationGuid.Value );

                    var workflow = Rock.Model.Workflow.Activate( workflowType, name );

                    if ( PreviousModifiedDateTime != null )
                    {
                        workflow.SetAttributeValue( "PreviousModifiedDateTime", PreviousModifiedDateTime );
                        workflow.SaveAttributeValue( "PreviousModifiedDateTime", rockContext );
                    }

                    List<string> workflowErrors;
                    new WorkflowService( rockContext ).Process( workflow, reservation, out workflowErrors );
                    if ( workflow.Id != 0 )
                    {
                        ReservationWorkflow reservationWorkflow = new ReservationWorkflow();
                        reservationWorkflow.ReservationId = reservation.Id;
                        reservationWorkflow.WorkflowId = workflow.Id;
                        reservationWorkflow.ReservationWorkflowTriggerId = reservationWorkflowTrigger.Id;
                        reservationWorkflow.TriggerType = reservationWorkflowTrigger.TriggerType;
                        reservationWorkflow.TriggerQualifier = reservationWorkflowTrigger.QualifierValue;
                        new ReservationWorkflowService( rockContext ).Add( reservationWorkflow );
                        rockContext.SaveChanges();
                    }
                }
            }
        }
    }
}