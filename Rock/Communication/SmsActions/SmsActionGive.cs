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
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication.SmsActions
{
    [Description( "Allows an SMS sender to make a payment." )]
    [Export( typeof( SmsActionComponent ) )]
    [ExportMetadata( "ComponentName", "Give" )]

    [TextField(
        name: "Give Keyword",
        description: "The case-insensitive keyword that will be expected at the beginning of the message.",
        required: true,
        defaultValue: "Give",
        order: 1,
        category: "Giving",
        key: AttributeKeys.GivingKeyword )]

    [CurrencyField(
        name: "Max Amount",
        description: "The maximum gift amount. Leave blank to allow gifts of any size.",
        required: false,
        order: 2,
        category: "Giving",
        key: AttributeKeys.MaxAmount )]

    [IntegerField(
        name: "Processing Delay Minutes",
        description: "The number of minutes to delay processing the gifts. Delaying allows for refunds through the refund action within the window because payments have not been sent to the processor. Set to zero or leave blank to process gifts immediately.",
        required: false,
        defaultValue: 30,
        order: 3,
        category: "Giving",
        key: AttributeKeys.ProcessingDelayMinutes )]

    [TextField(
        name: "Refund Keyword",
        description: "The case-insensitive keyword that is expected to trigger the refund. Leave blank to disable SMS refunds.",
        required: false,
        defaultValue: "Refund",
        order: 4,
        category: "Refund",
        key: AttributeKeys.RefundKeyword )]

    [IntegerField(
        name: "Maximum Refund Timeframe",
        description: "The number of minutes since a gift was made that a refund through this SMS action is allowed. Refunds are always allowed inside of the Processing Delay of the Give action no matter what this is set to. To only allow refunds inside the Processing Delay, leave this blank or set to zero.",
        required: false,
        defaultValue: 0,
        order: 5,
        category: "Refund",
        key: AttributeKeys.MaxRefundMinutes )]

    [MemoField(
        name: "Help Response",
        description: "The response that will be sent if the sender's message doesn't make sense, there is missing information, or an error occurs. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "Something went wrong. To give, simply text ‘{{ Keyword }} 100’ or ‘{{ Keyword }} $123.45’. Please contact us if you need help.",
        order: 6,
        category: "Response",
        key: AttributeKeys.HelpResponse )]

    [MemoField(
        name: "Max Amount Response",
        description: "The response that will be sent if the sender is trying to give more than the max amount (if configured). <span class='tip tip-lava'></span>",
        required: false,
        defaultValue: "Thank you for your generosity but our mobile giving solution cannot process a gift this large. Please give using our website.",
        order: 7,
        category: "Response",
        key: AttributeKeys.MaxAmountResponse )]

    [MemoField(
        name: "Setup Response",
        description: "The response that will be sent if the sender is unknown, does not have a saved account, or requests to edit their giving profile. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "Hi there! Please use our website to setup your giving profile before using this mobile giving solution.",
        order: 8,
        category: "Response",
        key: AttributeKeys.SetupResponse )]

    [MemoField(
        name: "Success Response",
        description: "The response that will be sent if the payment is successful. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "Thank you! We received your gift of {{ GiftAmount }} to the {{ AccountName }}.",
        order: 9,
        category: "Response",
        key: AttributeKeys.SuccessResponse )]

    [MemoField(
        name: "Refund Failure Response",
        description: "The response that will be sent if the sender's gift cannot be refunded. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "We are unable to process a refund for your last gift. Please contact us for assistance.",
        order: 10,
        category: "Response",
        key: AttributeKeys.RefundFailureResponse )]

    [MemoField(
        name: "Refund Success Response",
        description: "The response that will be sent if the refund is successful. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "Your gift for {{ GiftAmount }} to the {{ AccountName }} has been refunded.",
        order: 11,
        category: "Response",
        key: AttributeKeys.RefundSuccessResponse )]

    public class SmsActionGive : SmsActionComponent
    {
        private static class AttributeKeys
        {
            public const string GivingKeyword = "Keyword";
            public const string MaxAmount = "MaxAmount";
            public const string MaxAmountResponse = "MaxAmountResponse";
            public const string ProcessingDelayMinutes = "ProcessingDelayMinutes";

            public const string RefundKeyword = "RefundKeyword";
            public const string MaxRefundMinutes = "MaxRefundMinutes";

            public const string HelpResponse = "HelpResponse";
            public const string SetupResponse = "SetupResponse";
            public const string SuccessResponse = "SuccessResponse";

            public const string RefundSuccessResponse = "RefundSuccessResponse";
            public const string RefundFailureResponse = "RefundFailureResponse";
        }

        #region Properties

        /// <summary>
        /// Gets the component title to be displayed to the user.
        /// </summary>
        /// <value>
        /// The component title to be displayed to the user.
        /// </value>
        public override string Title => "Give";

        /// <summary>
        /// Gets the icon CSS class used to identify this component type.
        /// </summary>
        /// <value>
        /// The icon CSS class used to identify this component type.
        /// </value>
        public override string IconCssClass => "fa fa-dollar";

        /// <summary>
        /// Gets the description of this SMS Action.
        /// </summary>
        /// <value>
        /// The description of this SMS Action.
        /// </value>
        public override string Description => "Allows an SMS sender to make a payment and get a refund.";

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Checks the attributes for this component and determines if the message
        /// should be processed.
        /// </summary>
        /// <param name="action">The action that contains the configuration for this component.</param>
        /// <param name="message">The message that is to be checked.</param>
        /// <param name="errorMessage">If there is a problem processing, this should be set</param>
        /// <returns>
        ///   <c>true</c> if the message should be processed.
        /// </returns>
        public override bool ShouldProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( action == null || message == null || message.Message.IsNullOrWhiteSpace() )
            {
                errorMessage = "Cannot handle null action, null message, or empty message text";
                return false;
            }

            // Give the base class a chance to check it's own settings to see if we
            // should process this message.
            if ( !base.ShouldProcessMessage( action, message, out errorMessage ) )
            {
                return false;
            }

            var messageText = message.Message.Trim();
            return IsGivingMessage( action, messageText ) || IsRefundMessage( action, messageText );
        }

        /// <summary>
        /// Processes the message that was received from the remote user.
        /// </summary>
        /// <param name="action">The action that contains the configuration for this component.</param>
        /// <param name="message">The message that was received by Rock.</param>
        /// <param name="errorMessage">If there is a problem processing, this should be set</param>
        /// <returns>An SmsMessage that will be sent as the response or null if no response should be sent.</returns>
        public override SmsMessage ProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;      
            var messageText = message.Message.Trim();

            var isGiving = IsGivingMessage( action, messageText );
            var isRefund = IsRefundMessage( action, messageText );

            if ( isGiving )
            {
                return DoGift( action, message, out errorMessage );
            }
            else if ( isRefund )
            {
                return DoRefund( action, message, out errorMessage );
            }

            errorMessage = "The message was not a giving or refund request.";
            return null;
        }

        #endregion

        #region Giving

        /// <summary>
        /// Determine if the message has the giving keyword
        /// </summary>
        /// <param name="action"></param>
        /// <param name="messageText"></param>
        /// <returns></returns>
        private bool IsGivingMessage( SmsActionCache action, string messageText )
        {
            var givingKeyword = GetGivingKeyword( action );
            return messageText.StartsWith( givingKeyword, StringComparison.CurrentCultureIgnoreCase );
        }

        /// <summary>
        /// Process a gift if the sender requests it
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private SmsMessage DoGift( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();

            var messageText = message.Message.Trim();
            var givingKeyword = GetGivingKeyword( action );
            var giftAmountNullable = GetGiftAmount( givingKeyword, messageText );
            var maxAmount = GetMaxAmount( action );

            var person = message.FromPerson;
            var defaultSavedAccount = GetDefaultSavedAccount( rockContext, person );

            // If the number is not recognized, the person doesn't have a configured account designation, or the person
            // doesn't have a default saved payment method, send the "setup" response
            if ( defaultSavedAccount == null || person == null || !person.ContributionFinancialAccountId.HasValue || !person.PrimaryAliasId.HasValue )
            {
                var lavaTemplate = GetSetupResponse( action );
                return GetResolvedGiftSmsResponse( lavaTemplate, givingKeyword, message, null );
            }

            // If the amount is not valid (missing, not valid decimal, or not valid currency format), send back the
            // "help" response
            if ( !giftAmountNullable.HasValue || giftAmountNullable.Value < 1m )
            {
                var lavaTemplate = GetHelpResponse( action );
                return GetResolvedGiftSmsResponse( lavaTemplate, givingKeyword, message, null );
            }

            // If the gift amount exceeds the max amount, send the "max amount" response
            var giftAmount = giftAmountNullable.Value;
            var exceedsMax = maxAmount.HasValue && giftAmount > maxAmount.Value;

            if ( exceedsMax )
            {
                var lavaTemplate = GetMaxAmountResponse( action );
                return GetResolvedGiftSmsResponse( lavaTemplate, givingKeyword, message, giftAmount.FormatAsCurrency() );
            }

            // Validation has passed so prepare the automated payment processor args to charge the payment
            var automatedPaymentArgs = new AutomatedPaymentArgs
            {
                AuthorizedPersonAliasId = person.PrimaryAliasId.Value,
                AutomatedGatewayId = defaultSavedAccount.Id,
                FinancialPersonSavedAccountId = defaultSavedAccount.Id,
                AutomatedPaymentDetails = new List<AutomatedPaymentArgs.AutomatedPaymentDetailArgs>
                {
                    new AutomatedPaymentArgs.AutomatedPaymentDetailArgs {
                        AccountId = person.ContributionFinancialAccountId.Value,
                        Amount = giftAmount
                    }
                }
            };

            // Determine if this is a future transaction and when it should be processed
            var minutesDelay = GetDelayMinutes( action );

            if ( minutesDelay.HasValue && minutesDelay > 0 )
            {
                var delayTimeSpan = TimeSpan.FromMinutes( minutesDelay.Value );
                automatedPaymentArgs.FutureProcessingDateTime = RockDateTime.Now.Add( delayTimeSpan );
            }

            // Create the processor
            var automatedPaymentProcessor = new AutomatedPaymentProcessor( null, automatedPaymentArgs, rockContext );

            // If the args are not valid send the setup response
            if ( !automatedPaymentProcessor.AreArgsValid( out errorMessage ) )
            {
                var lavaTemplate = GetHelpResponse( action );
                return GetResolvedGiftSmsResponse( lavaTemplate, givingKeyword, message, giftAmount.FormatAsCurrency() );
            }

            // If charge seems like a duplicate or repeat, tell the sender
            if ( automatedPaymentProcessor.IsRepeatCharge( out errorMessage ) )
            {
                return new SmsMessage
                {
                    ToNumber = message.FromNumber,
                    FromNumber = message.ToNumber,
                    Message = "It looks like you've given very recently. In order for us to avoid accidental charges, please wait several minutes before giving again. Thank you!"
                };
            }

            // Charge the payment and send an appropriate response
            var transaction = automatedPaymentProcessor.ProcessCharge( out errorMessage );

            if ( transaction == null || !string.IsNullOrEmpty( errorMessage ) )
            {
                var lavaTemplate = GetHelpResponse( action );
                return GetResolvedGiftSmsResponse( lavaTemplate, givingKeyword, message, giftAmount.FormatAsCurrency() );
            }

            // Tag the transaction's summary with info from this action
            transaction.Summary = string.Format( "{0}{1}Text To Give from {2} with the message `{3}`",
                transaction.Summary,
                transaction.Summary.IsNullOrWhiteSpace() ? string.Empty : ". ",
                message.FromNumber,
                messageText );
            rockContext.SaveChanges();

            // Let the sender know that the gift was a success
            var successTemplate = GetSuccessResponse( action );
            return GetResolvedGiftSmsResponse( successTemplate, givingKeyword, message, giftAmount.FormatAsCurrency() );
        }

        #endregion

        #region Refund

        /// <summary>
        /// Return true if the message text is requesting a refund
        /// </summary>
        /// <param name="action"></param>
        /// <param name="messageText"></param>
        /// <returns></returns>
        public bool IsRefundMessage( SmsActionCache action, string messageText )
        {
            var refundKeyword = GetRefundKeyword( action );
            return messageText.Equals( refundKeyword, StringComparison.CurrentCultureIgnoreCase );
        }

        /// <summary>
        /// Handles the action of giving a refund if the message requests it
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private SmsMessage DoRefund( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();

            var person = message.FromPerson;
            var keyword = GetRefundKeyword( action );

            if ( person == null || !person.PrimaryAliasId.HasValue )
            {
                var template = GetRefundFailureResponse( action );
                return GetResolvedRefundSmsResponse( template, keyword, message, null );
            }

            var service = new FinancialTransactionService( rockContext );
            var futureTransactions = service.GetFutureTransactionsThatNeedToBeCharged();
            var transactionToRefund = futureTransactions.FirstOrDefault( ft => ft.AuthorizedPersonAliasId == person.PrimaryAliasId );

            // If there is a future transaction, it can simply be deleted since it has not been processed yet
            if ( transactionToRefund != null )
            {
                var canDelete = service.CanDelete( transactionToRefund, out errorMessage );
                var success = canDelete && service.Delete( transactionToRefund );

                if ( success )
                {
                    var template = GetRefundSuccessResponse( action );
                    return GetResolvedRefundSmsResponse( template, keyword, message, transactionToRefund );
                }
                else
                {
                    var template = GetRefundFailureResponse( action );
                    return GetResolvedRefundSmsResponse( template, keyword, message, transactionToRefund );
                }
            }

            // Check for non future transactions if the attribute setting allows it
            var maxRefundMinutes = GetMaxRefundMinutes( action );

            if ( maxRefundMinutes.HasValue && maxRefundMinutes.Value > 0 )
            {
                var minDateTime = RockDateTime.Now.Subtract( TimeSpan.FromMinutes( maxRefundMinutes.Value ) );

                // Query for transactions that are within the refund threshold
                transactionToRefund = service.Queryable()
                    .Where( ft =>
                        ft.TransactionDateTime.HasValue
                        && ft.TransactionDateTime.Value >= minDateTime
                        && ft.AuthorizedPersonAliasId == person.PrimaryAliasId )
                    .OrderByDescending( ft => ft.TransactionDateTime )
                    .FirstOrDefault();

                if ( transactionToRefund != null )
                {
                    var refundTransaction = transactionToRefund.ProcessRefund( out errorMessage );

                    if ( refundTransaction != null )
                    {
                        var template = GetRefundSuccessResponse( action );
                        return GetResolvedRefundSmsResponse( template, keyword, message, transactionToRefund );
                    }
                    else
                    {
                        var template = GetRefundFailureResponse( action );
                        return GetResolvedRefundSmsResponse( template, keyword, message, transactionToRefund );
                    }
                }
            }

            var lavaTemplate = GetRefundFailureResponse( action );
            return GetResolvedRefundSmsResponse( lavaTemplate, keyword, message, transactionToRefund );
        }

        #endregion

        #region Model Helpers

        /// <summary>
        /// Get the person's default saved account if they have one.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        private static FinancialPersonSavedAccount GetDefaultSavedAccount( RockContext rockContext, Person person )
        {
            if (person == null)
            {
                return null;
            }

            return new FinancialPersonSavedAccountService( rockContext )
                .GetByPersonId( person.Id )
                .AsNoTracking()
                .FirstOrDefault( sa => sa.IsDefault );
        }

        #endregion

        #region Parsing Helpers

        private static decimal? GetGiftAmount( string keyword, string messageText )
        {
            messageText = messageText.Trim();
            keyword = keyword.Trim();
            var textWithoutKeyword = Regex.Replace( messageText, keyword, string.Empty, RegexOptions.IgnoreCase ).Trim();

            // First try to parse a decimal like "1123.56"
            var successfulParse = decimal.TryParse( textWithoutKeyword, out var parsedValue );

            // Second try to parse currency like "$1,123.56"
            if ( !successfulParse )
            {
                successfulParse = decimal.TryParse( textWithoutKeyword, NumberStyles.Currency, CultureInfo.CurrentCulture, out parsedValue );
            }

            if ( successfulParse )
            {
                return decimal.Round( parsedValue, 2 );
            }

            return null;
        }

        #endregion

        #region Attribute Helpers

        /// <summary>
        /// Use this method with the refund action to get a formatted response
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <param name="keyword"></param>
        /// <param name="message"></param>
        /// <param name="transactionToRefund"></param>
        /// <returns></returns>
        private static SmsMessage GetResolvedRefundSmsResponse( string lavaTemplate, string keyword, SmsMessage message, FinancialTransaction transactionToRefund )
        {
            if ( transactionToRefund != null && transactionToRefund.TransactionDetails != null && transactionToRefund.TransactionDetails.Any() )
            {
                var formattedGiftAmount = transactionToRefund.TotalAmount.FormatAsCurrency();
                var firstDetail = transactionToRefund.TransactionDetails.FirstOrDefault();
                var accountName = firstDetail?.Account?.PublicName;
                return GetResolvedSmsResponse( lavaTemplate, keyword, message, formattedGiftAmount, accountName );
            }

            return GetResolvedSmsResponse( lavaTemplate, keyword, message, null, null );
        }

        /// <summary>
        /// Use this method with the give action to get a formatted response
        /// </summary>
        /// <param name="lavaTemplate">The response lava template</param>
        /// <param name="keyword">The keyword attribute</param>
        /// <param name="message">The original request message</param>
        /// <param name="formattedAmount">The formatted gift amount, if parsed</param>
        /// <returns></returns>
        private static SmsMessage GetResolvedGiftSmsResponse( string lavaTemplate, string keyword, SmsMessage message, string formattedAmount )
        {
            // Add some useful lava fields for the rock admin to make meaningful SMS responses
            var accountName = message.FromPerson?.ContributionFinancialAccount?.PublicName;
            return GetResolvedSmsResponse(lavaTemplate, keyword, message, formattedAmount, accountName);
        }

        /// <summary>
        /// Take the lava template, resolve it with useful text-to-give fields, and generate an SMS object to respond with
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <param name="keyword"></param>
        /// <param name="message"></param>
        /// <param name="formattedAmount"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        private static SmsMessage GetResolvedSmsResponse( string lavaTemplate, string keyword, SmsMessage message, string formattedAmount, string accountName )
        {
            var mergeObjects = new Dictionary<string, object>
            {
                { "Message", message },
                { "Keyword", keyword },
                { "Amount", formattedAmount },
                { "AccountName", accountName }
            };

            // Resolve the lava template with the lava fields
            var resolvedMessage = lavaTemplate.ResolveMergeFields( mergeObjects, message.FromPerson );

            // Generate a reply SMS object
            return new SmsMessage
            {
                ToNumber = message.FromNumber,
                FromNumber = message.ToNumber,
                Message = resolvedMessage
            };
        }

        #endregion

        #region Giving Attribute Getters

        /// <summary>
        /// Get and validate the keyword attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetGivingKeyword( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var keyword = action.GetAttributeValue( AttributeKeys.GivingKeyword );

            if ( string.IsNullOrWhiteSpace( keyword ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.GivingKeyword );
            }

            return keyword.Trim();
        }

        /// <summary>
        /// Get and validate the delay minutes attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static int? GetDelayMinutes( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var minutes = action.GetAttributeValue( AttributeKeys.ProcessingDelayMinutes ).AsIntegerOrNull();
            return minutes;
        }

        /// <summary>
        /// Get and validate the max amount attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static decimal? GetMaxAmount( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var maxAmountString = action.GetAttributeValue( AttributeKeys.MaxAmount );
            return maxAmountString.AsDecimalOrNull();
        }

        #endregion

        #region Refund Attribute Getters

        /// <summary>
        /// Get and validate the keyword attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetRefundKeyword( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var keyword = action.GetAttributeValue( AttributeKeys.RefundKeyword );

            if ( keyword.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return keyword.Trim();
        }

        /// <summary>
        /// Get and validate the max refund minutes attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static int? GetMaxRefundMinutes( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var minutes = action.GetAttributeValue( AttributeKeys.MaxRefundMinutes ).AsIntegerOrNull();
            return minutes;
        }

        #endregion

        #region Response Attribute Getters

        /// <summary>
        /// Get and validate the success response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetSuccessResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.SuccessResponse );

            if ( string.IsNullOrWhiteSpace( response ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.SuccessResponse );
            }

            return response.Trim();
        }

        /// <summary>
        /// Get and validate the help response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetHelpResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var helpResponse = action.GetAttributeValue( AttributeKeys.HelpResponse );

            if ( string.IsNullOrWhiteSpace( helpResponse ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.HelpResponse );
            }

            return helpResponse.Trim();
        }

        /// <summary>
        /// Get and validate the setup response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetSetupResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.SetupResponse );

            if ( string.IsNullOrWhiteSpace( response ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.SetupResponse );
            }

            return response.Trim();
        }

        /// <summary>
        /// Get and validate the max amount exceeded response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetMaxAmountResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.MaxAmountResponse );
            return response?.Trim();
        }

        /// <summary>
        /// Get and validate the success response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetRefundSuccessResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.RefundSuccessResponse );

            if ( string.IsNullOrWhiteSpace( response ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.RefundSuccessResponse );
            }

            return response.Trim();
        }

        /// <summary>
        /// Get and validate the failure response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetRefundFailureResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.RefundFailureResponse );

            if ( string.IsNullOrWhiteSpace( response ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.RefundFailureResponse );
            }

            return response.Trim();
        }

        #endregion
    }
}
