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

using Quartz;

using Rock.Model;
using Rock.Web.Cache;
using Rock.Attribute;
using Rock.Financial;
using Rock.Data;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to download any scheduled payments that were processed by the payment gateway
    /// </summary>
    [IntegerField( "Days Back", "The number of days prior to the current date to use as the start date when querying for scheduled payments that were processed.", true, 7, "", 1 )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Online Giving", "", 2 )]

    [DisallowConcurrentExecution]
    public class GetScheduledPayments : IJob
    {

        /// <summary> 
        /// Empty constructor for job initilization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GetScheduledPayments()
        {
        }

        /// <summary> 
        /// Job that updates the JobPulse setting with the current date/time.
        /// This will allow us to notify an admin if the jobs stop running.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            try
            {
                // get the job map
                JobDataMap dataMap = context.JobDetail.JobDataMap;

                Guid? financialGatewayGuid = dataMap.GetString( "PaymentGateway" ).AsGuidOrNull();
                if ( financialGatewayGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        foreach ( var financialGateway in new FinancialGatewayService( rockContext )
                            .Queryable()
                            .Where( g => g.IsActive ) )
                        {
                            financialGateway.LoadAttributes( rockContext );

                            var gateway = financialGateway.GetGatewayComponent();
                            if ( gateway != null )
                            {
                                int daysBack = dataMap.GetString( "DaysBack" ).AsIntegerOrNull() ?? 1;

                                DateTime today = RockDateTime.Today;
                                TimeSpan days = new TimeSpan( daysBack, 0, 0, 0 );
                                DateTime endDateTime = today.Add( financialGateway.GetBatchTimeOffset() );
                                endDateTime = RockDateTime.Now.CompareTo( endDateTime ) < 0 ? endDateTime.AddDays( -1 ) : today;
                                DateTime startDateTime = endDateTime.Subtract( days );

                                string errorMessage = string.Empty;
                                var payments = gateway.GetPayments( financialGateway, startDateTime, endDateTime, out errorMessage );

                                if ( string.IsNullOrWhiteSpace( errorMessage ) )
                                {
                                    string batchNamePrefix = dataMap.GetString( "BatchNamePrefix" );
                                    FinancialScheduledTransactionService.ProcessPayments( financialGateway, batchNamePrefix, payments );
                                }
                                else
                                {
                                    throw new Exception( errorMessage );
                                }
                            }
                        }
                    }
                }
            }

            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

    }
}