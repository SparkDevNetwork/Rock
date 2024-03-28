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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Tasks;

namespace Rock.Jobs
{
    /// <summary>
    /// Charges financial transactions that have a FutureProcessingDateTime
    /// </summary>
    [DisplayName( "Charge Future Transactions" )]
    [Description( "Charge future transactions where the FutureProcessingDateTime is now or has passed." )]

    #region Job Attributes
    [SystemCommunicationField( "Receipt Email",
        Key = AttributeKey.ReceiptEmail,
        Description = "The system email to use to send the receipt.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = "7DBF229E-7DEE-A684-4929-6C37312A0039",
        Order = 2 )]

    #endregion Job Attributes

    public class ChargeFutureTransactions : RockJob
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ReceiptEmail = "ReceiptEmail";
        }

        #endregion Attribute Keys

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

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            Guid? receiptEmail = GetAttributeValue( AttributeKey.ReceiptEmail ).AsGuidOrNull();

            var rockContext = new RockContext();
            var transactionService = new FinancialTransactionService( rockContext );
            var futureTransactions = transactionService.GetFutureTransactions()
                .Where( ft => ft.FutureProcessingDateTime <= RockDateTime.Now
                            && ft.Status != "ChargeFailed" ) // ignore transactions that failed when sent to the gateway.
                .ToList();

            var errors = new List<string>();
            var successCount = 0;

            foreach ( var futureTransaction in futureTransactions )
            {
                var automatedPaymentProcessor = new AutomatedPaymentProcessor( futureTransaction, rockContext );
                var transaction = automatedPaymentProcessor.ProcessCharge( out var errorMessage );

                if ( !string.IsNullOrEmpty( errorMessage ) )
                {
                    // If the charge attempt fails, flag the transaction so we don't attempt to send it back to the gateway next time the job runs.
                    futureTransaction.Status = "ChargeFailed";
                    futureTransaction.StatusMessage = errorMessage;
                    rockContext.SaveChanges();

                    errors.Add( errorMessage );
                }
                else
                {
                    successCount++;
                    SendReceipt( receiptEmail, transaction.Id );
                }
            }

            UpdateLastStatusMessage( string.Format( "{0} future transactions charged", successCount ) );

            if ( errors.Any() )
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine( string.Format( "{0} Errors: ", errors.Count() ) );
                errors.ForEach( e => sb.AppendLine( e ) );

                var errorMessage = sb.ToString();

                this.Result += errorMessage;

                var exception = new Exception( errorMessage );
                var context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );

                throw exception;
            }
        }

        /// <summary>
        /// Sends the receipt.
        /// </summary>
        /// <param name="receiptEmail">The <see cref="Guid"/> of the receipt email.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        private void SendReceipt( Guid? receiptEmail, int transactionId )
        {
            if ( !receiptEmail.HasValue )
            {
                return;
            }

            // Queue a bus message to send receipts
            var sendPaymentReceiptsTask = new ProcessSendPaymentReceiptEmails.Message
            {
                SystemEmailGuid = receiptEmail.Value,
                TransactionId = transactionId
            };

            sendPaymentReceiptsTask.Send();
        }
    }
}
