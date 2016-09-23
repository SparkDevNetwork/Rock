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

using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Runs a job now
    /// </summary>
    [Obsolete( "Use SendPaymentReceipts instead" )]
    public class SendPaymentReciepts : ITransaction
    {
        /// <summary>
        /// Gets or sets the system email unique identifier.
        /// </summary>
        /// <value>
        /// The system email unique identifier.
        /// </value>
        public Guid SystemEmailGuid { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public List<int> TransactionIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendPaymentReciepts"/> class.
        /// </summary>
        /// <param name="systemEmailGuid">The system email unique identifier.</param>
        /// <param name="transactionIds">The transaction ids.</param>
        [Obsolete( "Use SendPaymentReceipts instead" )]
        public SendPaymentReciepts( Guid systemEmailGuid, List<int> transactionIds )
        {
            SystemEmailGuid = systemEmailGuid;
            TransactionIds = transactionIds;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            var newtxn = new SendPaymentReceipts( SystemEmailGuid, TransactionIds );
            newtxn.Execute();
        }
    }
}