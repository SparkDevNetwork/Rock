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
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Runs a job now
    /// </summary>
    public class SendPaymentReceipts : ITransaction
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
        /// Initializes a new instance of the <see cref="SendPaymentReceipts"/> class.
        /// </summary>
        /// <param name="systemEmailGuid">The system email unique identifier.</param>
        /// <param name="transactionIds">The transaction ids.</param>
        public SendPaymentReceipts( Guid systemEmailGuid, List<int> transactionIds )
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

                        var transactionDetailEntityList = transaction.TransactionDetails.Where( a => a.EntityTypeId.HasValue && a.EntityId.HasValue ).ToList();
                        var transactionEntityList = new List<IEntity>();
                        foreach ( var transactionDetailEntity in transactionDetailEntityList)
                        {
                            var transactionEntityType = EntityTypeCache.Read( transactionDetailEntity.EntityTypeId.Value );
                            if ( transactionEntityType != null )
                            {
                                var dbContext = Reflection.GetDbContextForEntityType( transactionEntityType.GetEntityType() );
                                IService serviceInstance = Reflection.GetServiceForEntityType( transactionEntityType.GetEntityType(), dbContext );
                                if ( serviceInstance != null )
                                {
                                    System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                                    var transactionEntity = getMethod.Invoke( serviceInstance, new object[] { transactionDetailEntity.EntityId.Value } ) as Rock.Data.IEntity;
                                    transactionEntityList.Add( transactionEntity );
                                }
                            }
                        }

                        if ( transactionEntityList.Any())
                        {
                            mergeFields.Add( "TransactionEntityList", transactionEntityList );
                            mergeFields.Add( "TransactionEntity", transactionEntityList.First() );
                        }

                        var emailMessage = new RockEmailMessage( SystemEmailGuid );
                        emailMessage.AddRecipient( new RecipientData( person.Email, mergeFields ) );
                        emailMessage.Send();
                    }
                }
            }
        }
    }
}