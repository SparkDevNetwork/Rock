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
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Sends an event registration confirmation
    /// </summary>
    public sealed class ProcessSendPaymentReceiptEmails : BusStartedTask<ProcessSendPaymentReceiptEmails.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new FinancialTransactionService( rockContext );
                var transaction = service.Get( message.TransactionId );
                if ( transaction != null &&
                    transaction.AuthorizedPersonAlias != null &&
                    transaction.AuthorizedPersonAlias.Person != null )
                {
                    var person = transaction.AuthorizedPersonAlias.Person;

                    // setup merge fields
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Person", person );

                    decimal totalAmount = 0;
                    List<Dictionary<string, object>> accountAmounts = new List<Dictionary<string, object>>();
                    foreach ( var detail in transaction.TransactionDetails )
                    {
                        if ( detail.Account != null && detail.Amount > 0 )
                        {
                            var accountAmount = new Dictionary<string, object>();
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
                    mergeFields.Add( "ForeignKey", transaction.ForeignKey );
                    mergeFields.Add( "Transaction", transaction );
                    mergeFields.Add( "Amounts", accountAmounts );

                    var transactionDetailEntityList = transaction.TransactionDetails.Where( a => a.EntityTypeId.HasValue && a.EntityId.HasValue ).ToList();
                    var transactionEntityList = new List<IEntity>();
                    foreach ( var transactionDetailEntity in transactionDetailEntityList )
                    {
                        var transactionEntityType = EntityTypeCache.Get( transactionDetailEntity.EntityTypeId.Value );
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

                    if ( transactionEntityList.Any() )
                    {
                        mergeFields.Add( "TransactionEntityList", transactionEntityList );
                        mergeFields.Add( "TransactionEntity", transactionEntityList.First() );
                    }

                    var emailMessage = new RockEmailMessage( message.SystemEmailGuid );
                    emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
                    var errors = new List<string>();

                    // errors will be logged by send
                    emailMessage.Send( out errors );
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
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
            public int TransactionId { get; set; }
        }
    }
}