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
using System.Text;
using System.Web;
using Quartz;
using Rock.Data;
using Rock.Financial;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Charges financial transactions that have a FutureProcessingDateTime
    /// </summary>
    [DisallowConcurrentExecution]
    public class ChargeFutureTransactions : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ChargeFutureTransactions()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            var transactionService = new FinancialTransactionService( rockContext );
            var futureTransactions = transactionService.GetFutureTransactions().Where( ft => ft.FutureProcessingDateTime <= RockDateTime.Now ).ToList();
            var errors = new List<string>();
            var successCount = 0;

            foreach ( var futureTransaction in futureTransactions )
            {
                var automatedPaymentProcessor = new AutomatedPaymentProcessor( futureTransaction, rockContext );
                automatedPaymentProcessor.ProcessCharge( out var errorMessage );

                if ( !string.IsNullOrEmpty( errorMessage ) )
                {
                    errors.Add( errorMessage );
                }
                else
                {
                    successCount++;
                }
            }

            context.Result = string.Format( "{0} future transactions charged", successCount );

            if ( errors.Any() )
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine( string.Format( "{0} Errors: ", errors.Count() ) );
                errors.ForEach( e => sb.AppendLine( e ) );

                var errorMessage = sb.ToString();
                context.Result += errorMessage;

                var exception = new Exception( errorMessage );
                var context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );

                throw exception;
            }
        }
    }
}
