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
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to download any scheduled payments that were processed by the payment gateway
    /// </summary>
    [IntegerField( "Days Back", "The number of days prior to the current date to use as the start date when querying for scheduled payments that were processed.", true, 7, "", 1 )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Online Giving", "", 2 )]
    [SystemEmailField( "Receipt Email", "The system email to use to send the receipts.", false, "", "", 3 )]
    [SystemEmailField( "Failed Payment Email", "The system email to use to send a notice about a scheduled payment that failed.", false, "", "", 4 )]
    [WorkflowTypeField( "Failed Payment Workflow", "An optional workflow to start whenever a scheduled payment has failed.", false, false, "", "", 5)]
    [FinancialGatewayField( "Target Gateway", "By default payments will download from all active financial gateways. Optionally select a single gateway to download scheduled payments from.  You will need to set up additional jobs targeting other active gateways.", false, "", "", 6 )]
    [DisallowConcurrentExecution]
    public class GetScheduledPayments : IJob
    {

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GetScheduledPayments()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="Exception">
        /// One or more exceptions occurred while downloading transactions..." + Environment.NewLine + exceptionMsgs.AsDelimited( Environment.NewLine )
        /// </exception>
        public virtual void Execute( IJobExecutionContext context )
        {
            var exceptionMsgs = new List<string>();

            // get the job map
            var dataMap = context.JobDetail.JobDataMap;
            var scheduledPaymentsProcessed = 0;

            using ( var rockContext = new RockContext() )
            {
                var targetGateways = new FinancialGatewayService( rockContext )
                    .Queryable()
                    .Where( g => g.IsActive );

                var targetGatewayGuid = dataMap.GetString( "TargetGateway" ).AsGuidOrNull();
                if (targetGatewayGuid.HasValue)
                {
                    targetGateways = targetGateways.Where(g => g.Guid == targetGatewayGuid.Value);
                }

                foreach ( var financialGateway in targetGateways )
                {
                    try
                    {
                        financialGateway.LoadAttributes( rockContext );

                        var gateway = financialGateway.GetGatewayComponent();
                        if ( gateway == null )
                        {
                            continue;
                        }

                        int daysBack = dataMap.GetString( "DaysBack" ).AsIntegerOrNull() ?? 1;

                        DateTime today = RockDateTime.Today;
                        TimeSpan days = new TimeSpan( daysBack, 0, 0, 0 );
                        DateTime endDateTime = today.Add( financialGateway.GetBatchTimeOffset() );

                        // If the calculated end time has not yet occurred, use the previous day.
                        endDateTime = RockDateTime.Now.CompareTo( endDateTime ) >= 0 ? endDateTime : endDateTime.AddDays( -1 );

                        DateTime startDateTime = endDateTime.Subtract( days );

                        string errorMessage = string.Empty;
                        var payments = gateway.GetPayments( financialGateway, startDateTime, endDateTime, out errorMessage );

                        if ( string.IsNullOrWhiteSpace( errorMessage ) )
                        {
                            Guid? receiptEmail = dataMap.GetString( "ReceiptEmail" ).AsGuidOrNull();
                            Guid? failedPaymentEmail = dataMap.GetString( "FailedPaymentEmail" ).AsGuidOrNull();
                            Guid? failedPaymentWorkflowType = dataMap.GetString( "FailedPaymentWorkflow" ).AsGuidOrNull();

                            string batchNamePrefix = dataMap.GetString( "BatchNamePrefix" );
                            FinancialScheduledTransactionService.ProcessPayments( financialGateway, batchNamePrefix, payments, string.Empty, receiptEmail, failedPaymentEmail, failedPaymentWorkflowType );
                            scheduledPaymentsProcessed += payments.Count();
                        }
                        else
                        {
                            throw new Exception( errorMessage );
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, null );
                        exceptionMsgs.Add( ex.Message );
                    }
                }
            }

            if ( exceptionMsgs.Any() )
            {
                throw new Exception( "One or more exceptions occurred while downloading transactions..." + Environment.NewLine + exceptionMsgs.AsDelimited( Environment.NewLine ) );
            }

            context.Result = string.Format( "{0} payments processed", scheduledPaymentsProcessed );
        }
    }
}
