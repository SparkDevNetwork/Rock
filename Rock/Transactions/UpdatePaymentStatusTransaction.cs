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
using System.Collections.Generic;

using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Update the status of scheduled transactions
    /// </summary>
    public class UpdatePaymentStatusTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the gateway identifier.
        /// </summary>
        /// <value>
        /// The gateway identifier.
        /// </value>
        public int GatewayId {get; set;}

        /// <summary>
        /// Gets or sets the scheduled transaction ids.
        /// </summary>
        /// <value>
        /// The scheduled transaction ids.
        /// </value>
        public List<int> ScheduledTransactionIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePaymentStatusTransaction"/> class.
        /// </summary>
        /// <param name="gatewayId">The gateway identifier.</param>
        /// <param name="scheduledTransactionIds">The scheduled transaction ids.</param>
        public UpdatePaymentStatusTransaction( int gatewayId, List<int> scheduledTransactionIds )
        {
            GatewayId = gatewayId;
            ScheduledTransactionIds = scheduledTransactionIds;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var gateway = new FinancialGatewayService( rockContext ).Get( GatewayId );
                if ( gateway != null )
                {
                    var gatewayComponent = gateway.GetGatewayComponent();
                    if ( gatewayComponent != null )
                    {
                        var scheduledTxnService = new FinancialScheduledTransactionService( rockContext );
                        
                        foreach( var txnId in ScheduledTransactionIds )
                        {
                            var scheduledTxn = scheduledTxnService.Get( txnId );
                            if ( scheduledTxn != null )
                            {
                                string statusMsgs = string.Empty;
                                gatewayComponent.GetScheduledPaymentStatus( scheduledTxn, out statusMsgs );
                                rockContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }
    }
}