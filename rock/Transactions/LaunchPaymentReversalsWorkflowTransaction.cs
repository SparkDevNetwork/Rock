// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches a workflow for processing
    /// </summary>
    public class LaunchPaymentReversalsWorkflowTransaction : ITransaction
    {

        /// <summary>
        /// Gets or sets the workfow type unique identifier.
        /// </summary>
        /// <value>
        /// The workfow type unique identifier.
        /// </value>
        public Guid? WorkfowTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public List<int> TransactionIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchPaymentReversalsWorkflowTransaction" /> class.
        /// </summary>
        /// <param name="workfowTypeGuid">The workfow type unique identifier.</param>
        /// <param name="transactionIds">The transaction ids.</param>
        public LaunchPaymentReversalsWorkflowTransaction( Guid workfowTypeGuid, List<int> transactionIds )
        {
            WorkfowTypeGuid = workfowTypeGuid;
            TransactionIds = transactionIds;
        }
        
        /// <summary>
        /// Execute method to launch a workflow for each reversal transaction
        /// </summary>
        public void Execute()
        {
            if ( WorkfowTypeGuid != null && TransactionIds != null && TransactionIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    var workflowType = workflowTypeService.Get( WorkfowTypeGuid.Value );
                    if ( workflowType != null )
                    {
                        foreach ( var transaction in new FinancialTransactionService( rockContext )
                            .Queryable()
                            .Where( t => TransactionIds.Contains( t.Id ) )
                            .ToList() )
                        {
                            string workflowName = transaction.TransactionCode;
                            if ( transaction.AuthorizedPersonAlias != null && transaction.AuthorizedPersonAlias.Person != null )
                            {
                                workflowName = transaction.AuthorizedPersonAlias.Person.FullName;
                            }
                            else
                            {
                                workflowName = transaction.TransactionCode;
                            }

                            var workflow = Rock.Model.Workflow.Activate( workflowType, workflowName );

                            List<string> workflowErrors;
                            new Rock.Model.WorkflowService( rockContext ).Process( workflow, transaction, out workflowErrors );
                        }
                    }
                }
            }
        }
    }
}