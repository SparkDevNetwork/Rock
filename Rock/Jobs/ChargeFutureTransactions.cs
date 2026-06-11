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
using System.Threading.Tasks;
using System.Web;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Tasks;
using Rock.Web.Cache;

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
        Order = 0 )]

    [LavaField( "Text to Give Failure Message",
        Key = AttributeKey.TextToGiveFailureMessage,
        Description = "The response message to send to text givers when a transaction fails.  This message will be sent to the giver via SMS (if possible).",
        DefaultValue = "{{Person.FirstName}}, your gift for {{Transaction.TotalAmount}} could not be processed.  Please try your gift again or contact us for assistance.",
        Order = 1 )]

    [SystemPhoneNumberField( "Send SMS Response From",
        Key = AttributeKey.SendSMSResponseFrom,
        Description = "The System Phone Number to use for sending SMS response messages (must be SMS-enabled).  If not specified, the job will not send text responses.",
        Order = 2 )]

    #endregion Job Attributes

    public class ChargeFutureTransactions : RockJob
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ReceiptEmail = "ReceiptEmail";
            public const string TextToGiveFailureMessage = "TextToGiveFailureMessage";
            public const string SendSMSResponseFrom = "SendSMSResponseFrom";
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

                    // If this is a text to give transction, attempt to send the giver a notification that their transaction failed.
                    Task.Run( () => SendTextToGiveFailureNotification( futureTransaction.Id, errorMessage ) );

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

        /// <summary>
        /// Sends a notification to the giver that a text to give transaction failed and then
        /// deletes the failed transaction.  If any part of this process fails the transaction
        /// will not be deleted.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="errorMessage">The error message from the processing gateway.</param>
        private void SendTextToGiveFailureNotification( int transactionId, string errorMessage )
        {
            var exceptionMessage = $"{this.ServiceJobName} job was unable to notify the giver that their text-to-give failed (due to '{errorMessage}') and therefore the transaction with Id {transactionId} was not deleted.";

            var smsBody = GetAttributeValue( AttributeKey.TextToGiveFailureMessage );
            if ( string.IsNullOrWhiteSpace( smsBody ) )
            {
                ExceptionLogService.LogException( exceptionMessage + " Reason: The job is missing the required 'Text to Give Failure Message'." );
                return; // No response message.
            }

            var smsFrom = GetSMSFromNumber();
            var isSmsEnabled = ( smsFrom != null ) && MediumContainer.HasActiveSmsTransport();
            if ( !isSmsEnabled )
            {
                ExceptionLogService.LogException( exceptionMessage + " Reason: Either the SMS Transport is not active or the job is missing the 'Send SMS Response From'." );
                return; // Not configured to send SMS.
            }

            using ( var rockContext = new RockContext() )
            {
                var transactionService = new FinancialTransactionService( rockContext );
                var transaction = transactionService.Get( transactionId );
                var person = transaction?.AuthorizedPersonAlias?.Person;

                if ( person == null )
                {
                    ExceptionLogService.LogException( exceptionMessage + " Reason: The person was missing from the transaction." );
                    return; // Broken transaction (shouldn't happen).
                }

                var smsNumbers = person.PhoneNumbers.Where( n => n.IsMessagingEnabled );
                var smsNumber = smsNumbers.FirstOrDefault();
                if ( smsNumber == null )
                {
                    ExceptionLogService.LogException( exceptionMessage + " Reason: The person does not have an SMS enabled phone number." );
                    return; // No SMS enabled numbers.
                }

                // Prefer mobile number.
                int mobileNumberValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
                if ( smsNumbers.Any( n => n.NumberTypeValueId == mobileNumberValueId ) )
                {
                    smsNumber = smsNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobileNumberValueId );
                }

                var mergeFields = LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "Person", person );
                mergeFields.Add( "Transaction", transaction );

                var smsMessage = new RockSMSMessage
                {
                    FromSystemPhoneNumber = smsFrom,
                    Message = smsBody,
                    CreateCommunicationRecord = false,
                    CommunicationName = "Text to Give Failure Notification"
                };

                var smsRecipient = new RockSMSMessageRecipient( person, smsNumber.Number, mergeFields );
                smsMessage.AddRecipient( smsRecipient );
                if ( smsMessage.Send() )
                {
                    transactionService.Delete( transaction );
                    rockContext.SaveChanges();
                }
                else
                {
                    ExceptionLogService.LogException( exceptionMessage + " Reason: The SMS message failed to send." );
                }
            }
        }

        /// <summary>
        /// Find an appropriate SystemPhoneNumber to send an SMS notification.
        /// </summary>
        /// <returns>A <see cref="SystemPhoneNumberCache"/>.</returns>
        private SystemPhoneNumberCache GetSMSFromNumber()
        {
            var smsFromGuid = GetAttributeValue( AttributeKey.SendSMSResponseFrom ).AsGuidOrNull();
            if ( !smsFromGuid.HasValue )
            {
                return null; // Job attribute not set.
            }

            var smsPhoneNumber = SystemPhoneNumberCache.All( false )
                .Where( spn => spn.IsSmsEnabled && spn.Guid == smsFromGuid.Value ).FirstOrDefault();

            if ( smsPhoneNumber == null )
            {
                return null; // Invalid phone number selected.
            }

            return smsPhoneNumber;
        }
    }
}
