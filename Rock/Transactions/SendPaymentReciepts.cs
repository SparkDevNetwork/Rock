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
            using ( var rockContext = new RockContext() )
            {
                var service = new FinancialTransactionService( rockContext );
                foreach ( int transactionId in TransactionIds )
                {
                    var transaction = service.Get( transactionId );
                    if ( transaction != null &&
                        transaction.AuthorizedPersonAlias != null &&
                        transaction.AuthorizedPersonAlias.Person != null )
                    {
                        var person = transaction.AuthorizedPersonAlias.Person;

                        // setup merge fields
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Person", person );

                        decimal totalAmount = 0;
                        List<Dictionary<String, object>> accountAmounts = new List<Dictionary<String, object>>();
                        foreach ( var detail in transaction.TransactionDetails )
                        {
                            if ( detail.Account != null && detail.Amount > 0 )
                            {
                                var accountAmount = new Dictionary<String, object>();
                                accountAmount.Add( "AccountId", detail.Account.Id );
                                accountAmount.Add( "AccountName", detail.Account.Name );
                                accountAmount.Add( "Amount", detail.Amount );
                                accountAmounts.Add( accountAmount );

                                totalAmount += detail.Amount;
                            }
                        }

                        mergeFields.Add( "TotalAmount", totalAmount );
                        mergeFields.Add( "GaveAnonymous", "False" );
                        mergeFields.Add( "ReceiptEmail", person.Email );
                        mergeFields.Add( "ReceiptEmailed", true );
                        mergeFields.Add( "LastName", person.LastName );
                        mergeFields.Add( "FirstNames", person.NickName );
                        mergeFields.Add( "TransactionCode", transaction.TransactionCode );
                        mergeFields.Add( "Transaction", transaction );
                        mergeFields.Add( "Amounts", accountAmounts );

                        var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );

                        var recipients = new List<RecipientData>();
                        recipients.Add( new RecipientData( person.Email, mergeFields ) );

                        Email.Send( SystemEmailGuid, recipients, appRoot );
                    }
                }
            }
        }
    }
}