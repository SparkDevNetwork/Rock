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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

#if REVIEW_NET5_0_OR_GREATER
using Microsoft.EntityFrameworkCore.ChangeTracking;
#endif

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches a benevolence request change workflow
    /// </summary>
    public class BenevolenceRequestChangeTransaction : ITransaction 
    {
        private EntityState State;

        private Guid? BenevolenceRequestGuid;
        private int BenevolenceTypeId;

        private int? RequestByPersonId;
        private int? CaseWorkerPersonAliasId;
        private int? PreviousCaseWorkerPersonAliasId;

        private int RequestStatusValueId;
        private int PreviousRequestStatusValueId;

        /// <summary>
        /// Initializes a new instance of the <see cref="BenevolenceRequestChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The db entity entry.</param>
#if REVIEW_NET5_0_OR_GREATER
        public BenevolenceRequestChangeTransaction( EntityEntry<BenevolenceRequest> entry )
#else
        public BenevolenceRequestChangeTransaction( DbEntityEntry<BenevolenceRequest> entry )
#endif
        {
            // If entity was a benevolence request, save the values
            var benevolenceRequest = entry.Entity;
            if ( benevolenceRequest != null )
            {
                State = entry.State;

                // If this isn't a deleted benevolence request, get the benevolence request guid
                if ( State != EntityState.Deleted )
                {
                    BenevolenceRequestGuid = benevolenceRequest.Guid;
                    BenevolenceTypeId = benevolenceRequest.BenevolenceTypeId;

                    RequestByPersonId = benevolenceRequest?.RequestedByPersonAliasId;
                    CaseWorkerPersonAliasId = benevolenceRequest?.CaseWorkerPersonAliasId;
                    RequestStatusValueId = benevolenceRequest.RequestStatusValueId.ToIntSafe();

                    if ( State == EntityState.Modified )
                    {
                        var dCaseWorkerPersonAliasIdProperty = entry.Property( "CaseWorkerPersonAliasId" );
                        if ( dCaseWorkerPersonAliasIdProperty != null )
                        {
                            PreviousCaseWorkerPersonAliasId = dCaseWorkerPersonAliasIdProperty.OriginalValue as int?;
                        }

                        var dRequestStatusValueIdProperty = entry.Property( "RequestStatusValueId" );
                        if ( dRequestStatusValueIdProperty != null )
                        {
                            PreviousRequestStatusValueId = dRequestStatusValueIdProperty.OriginalValue.ToIntSafe();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Execute method to check for any workflows to launch.
        /// </summary>
        public void Execute()
        {
            if ( !( BenevolenceRequestGuid.HasValue && RequestByPersonId.HasValue ) )
            {
                return;
            }

            // Get all the workflows from cache
            var cachedWorkflows = BenevolenceWorkflowService.GetCachedTriggers();

            if ( cachedWorkflows == null || !cachedWorkflows.Any() )
            {
                return;
            }

            var workflows = cachedWorkflows
                .Where( w =>
                    w.TriggerType != BenevolenceWorkflowTriggerType.Manual & w.BenevolenceTypeId == BenevolenceTypeId
                )
                .ToList();

            if ( workflows.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Loop through benevolenceWorkflows and launch appropriate workflow
                    foreach ( var benevolenceWorkflow in workflows )
                    {
                        switch ( benevolenceWorkflow.TriggerType )
                        {
                            case BenevolenceWorkflowTriggerType.RequestStarted:
                                {
                                    if ( State == EntityState.Added )
                                    {
                                        LaunchWorkflow( rockContext, benevolenceWorkflow, "Request Started" );
                                    }
                                    break;
                                }

                            case BenevolenceWorkflowTriggerType.CaseworkerAssigned:
                                {
                                    if ( CaseWorkerPersonAliasId.HasValue &&
                                        !CaseWorkerPersonAliasId.Equals( PreviousCaseWorkerPersonAliasId ) )
                                    {
                                        LaunchWorkflow( rockContext, benevolenceWorkflow, "Case Worker Assigned" );
                                    }
                                    break;
                                }
                            case BenevolenceWorkflowTriggerType.StatusChanged:
                                {
                                    if ( State == EntityState.Modified && QualifiersMatch( benevolenceWorkflow, PreviousRequestStatusValueId, RequestStatusValueId ) )
                                    {
                                        LaunchWorkflow( rockContext, benevolenceWorkflow, "Status Changed" );
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private bool QualifiersMatch( BenevolenceWorkflow workflow, ConnectionState prevState, ConnectionState state )
        {
            if ( prevState == state )
            {
                return false;
            }

            var qualifierParts = ( workflow.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                matches = qualifierParts[1].AsInteger() == prevState.ConvertToInt();
            }

            if ( matches && qualifierParts.Length > 2 && !string.IsNullOrWhiteSpace( qualifierParts[2] ) )
            {
                matches = qualifierParts[2].AsInteger() == state.ConvertToInt();
            }

            return matches;
        }

        private bool QualifiersMatch( BenevolenceWorkflow workflow, int prevStatusId, int statusId )
        {
            if ( prevStatusId == statusId )
            {
                return false;
            }

            var qualifierParts = ( workflow.QualifierValue ?? "" ).Split( new char[] { '|' } );

            bool matches = true;

            if ( matches && qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                var qualifierStatusId = qualifierParts[1].AsIntegerOrNull();
                if ( qualifierStatusId.HasValue )
                {
                    matches = qualifierStatusId != 0 && qualifierStatusId == prevStatusId;
                }
                else
                {
                    matches = false;
                }
            }

            if ( matches && qualifierParts.Length > 2 && !string.IsNullOrWhiteSpace( qualifierParts[2] ) )
            {
                var qualifierStatusId = qualifierParts[2].AsIntegerOrNull();
                if ( qualifierStatusId.HasValue )
                {
                    matches = qualifierStatusId != 0 && qualifierStatusId == statusId;
                }
                else
                {
                    matches = false;
                }
            }

            return matches;
        }

        private void LaunchWorkflow( RockContext rockContext, BenevolenceWorkflow benevolenceWorkflow, string name )
        {
            var workflowType = WorkflowTypeCache.Get( benevolenceWorkflow.WorkflowTypeId );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                BenevolenceRequest benevolenceRequest = null;
                if ( BenevolenceRequestGuid.HasValue )
                {
                    benevolenceRequest = new BenevolenceRequestService( rockContext ).Get( BenevolenceRequestGuid.Value );

                    var workflow = Rock.Model.Workflow.Activate( workflowType, name );

                    List<string> workflowErrors;
                    new WorkflowService( rockContext ).Process( workflow, benevolenceRequest, out workflowErrors );
                }
            }
        }
    }
}