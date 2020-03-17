// <copyright>
// Copyright by BEMA Information Technologies
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
using Rock.Web.Cache;
using Rock.Workflow;
namespace com.bemaservices.WorkflowExtensions.Workflow.Action
{
    /// <summary>
    /// Runs a SQL query
    /// </summary>
    [ActionCategory( "BEMA Services > Workflow Extensions" )]
    [Description( "Cancels a Scheduled Transaction" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Scheduled Transaction Cancel" )]
    [WorkflowTextOrAttribute( "Scheduled Transaction Id or Guid", "Scheduled Transaction Attribute", "The id or guid of the Scheduled Transaction. <span class='tip tip-lava'></span>", true, "", "", 1, "ScheduledTransactionIdGuid" )]

    public class CancelScheduledTransaction : ActionComponent
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

            FinancialScheduledTransactionService financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            FinancialScheduledTransaction financialScheduledTransaction = null;
            string financialScheduledTransactionIdGuidString = GetAttributeValue( action, "ScheduledTransactionIdGuid", true ).ResolveMergeFields( mergeFields ).Trim();
            var financialScheduledTransactionGuid = financialScheduledTransactionIdGuidString.AsGuidOrNull();
            if ( financialScheduledTransactionGuid.HasValue )
            {
                financialScheduledTransaction = financialScheduledTransactionService.Get( financialScheduledTransactionGuid.Value );
            }
            else
            {
                var financialScheduledTransactionId = financialScheduledTransactionIdGuidString.AsIntegerOrNull();
                if ( financialScheduledTransactionId.HasValue )
                {
                    financialScheduledTransaction = financialScheduledTransactionService.Get( financialScheduledTransactionId.Value );
                }
            }

            if ( financialScheduledTransaction == null )
            {
                errorMessages.Add( string.Format( "Scheduled Transaction could not be found for selected value ('{0}')!", financialScheduledTransactionIdGuidString ) );
                return false;
            }


            if ( financialScheduledTransaction.FinancialGateway != null )
            {
                financialScheduledTransaction.FinancialGateway.LoadAttributes( rockContext );
            }

            string errorMessage = string.Empty;
            if ( financialScheduledTransactionService.Cancel( financialScheduledTransaction, out errorMessage ) )
            {
                financialScheduledTransactionService.GetStatus( financialScheduledTransaction, out errorMessage );
                rockContext.SaveChanges();
            }
            else
            {
                errorMessages.Add( errorMessage );
                return false;
            }

            action.AddLogEntry( $"Canceled Scheduled Transaction" );
            return true;
        }
    }
}
