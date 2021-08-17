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
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Update the status of scheduled transactions
    /// </summary>
    [Obsolete( "The bus shouldn't be used for this. Use UpdatePaymentStatusTransaction Transaction instead." )]
    [RockObsolete( "1.13" )]
    public sealed class UpdatePaymentStatusFinancialScheduledTransactions : BusStartedTask<UpdatePaymentStatusFinancialScheduledTransactions.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            if ( !message.ScheduledTransactionIds.Any() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );

                foreach ( var scheduledTransactionId in message.ScheduledTransactionIds )
                {
                    var financialScheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionId );

                    if ( financialScheduledTransaction != null )
                    {
                        financialScheduledTransactionService.GetStatus( financialScheduledTransaction, out _ );

                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the scheduled transaction ids.
            /// </summary>
            /// <value>
            /// The scheduled transaction ids.
            /// </value>
            public List<int> ScheduledTransactionIds { get; set; }
        }
    }
}