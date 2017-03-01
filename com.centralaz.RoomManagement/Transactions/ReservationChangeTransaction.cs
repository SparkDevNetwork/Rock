// <copyright>
// Copyright by the Central Christian Church
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
using com.centralaz.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;

namespace com.centralaz.RoomManagement.Transactions
{
    /// <summary>
    /// Launches a connection request change workflow
    /// </summary>
    public class ReservationChangeTransaction : ITransaction
    {
        private EntityState State;
        private Guid? ReservationGuid;
        private int ReservationStatusId;
        private int PreviousReservationStatusId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public ReservationChangeTransaction( DbEntityEntry entry )
        {
            // If entity was a connection request, save the values
            var reservation = entry.Entity as Reservation;
            if ( reservation != null )
            {
                State = entry.State;
                ReservationStatusId = reservation.ReservationStatusId;

                // If this isn't a new connection request, get the previous state and role values
                if ( State != EntityState.Added )
                {
                    var dbStatusProperty = entry.Property( "ReservationStatusId" );
                    if ( dbStatusProperty != null )
                    {
                        PreviousReservationStatusId = (int)dbStatusProperty.OriginalValue;
                    }
                }

                // If this isn't a deleted connection request, get the connection request guid
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
                // Get the workflows associated to the connection
                var reservationWorkflowTriggers = cachedWorkflowTriggers.ToList();

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
                                        if ( State == EntityState.Added && QualifiersMatch( rockContext, reservationWorkflowTrigger, ReservationStatusId, ReservationStatusId ) )
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
                                case ReservationWorkflowTriggerType.StatusChanged:
                                    {
                                        if ( State == EntityState.Modified && QualifiersMatch( rockContext, reservationWorkflowTrigger, PreviousReservationStatusId, ReservationStatusId ) )
                                        {
                                            LaunchWorkflow( rockContext, reservationWorkflowTrigger, "Status Changed" );
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
        }

        private bool QualifiersMatch( RockContext rockContext, ReservationWorkflowTrigger reservationWorkflowTrigger, int prevStatusId, int statusId )
        {
            var qualifierParts = ( reservationWorkflowTrigger.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var qualifierRoleId = qualifierParts[1].AsIntegerOrNull();
                if ( qualifierRoleId.HasValue )
                {
                    matches = qualifierRoleId != 0 && qualifierRoleId == statusId;
                }
                else
                {
                    matches = false;
                }
            }

            if ( matches && qualifierParts.Length > 3 && !string.IsNullOrWhiteSpace( qualifierParts[3] ) )
            {
                var qualifierRoleId = qualifierParts[3].AsIntegerOrNull();
                if ( qualifierRoleId.HasValue )
                {
                    matches = qualifierRoleId != 0 && qualifierRoleId == prevStatusId;
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
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( reservationWorkflowTrigger.WorkflowTypeId.Value );
            if ( workflowType != null )
            {
                Reservation reservation = null;
                if ( ReservationGuid.HasValue )
                {
                    reservation = new ReservationService( rockContext ).Get( ReservationGuid.Value );

                    var workflow = Rock.Model.Workflow.Activate( workflowType, name );

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