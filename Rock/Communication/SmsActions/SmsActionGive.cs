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
        name: "Keyword",
        description: "The case-insensitive keyword that will be expected at the beginning of the message.",
        required: true,
        defaultValue: "Give",
        order: 1,
        category: "Gift",
        key: AttributeKeys.Keyword )]

    [CurrencyField(
        name: "Max Amount",
        description: "The maximum gift amount. Leave blank to allow gifts of any size.",
        required: false,
        order: 2,
        category: "Gift",
        key: AttributeKeys.MaxAmount )]

    [IntegerField(
        name: "Processing Delay Minutes",
        description: "The number of minutes to delay processing the gifts. Delaying allows for simple refunds within the window because payments have not been sent to the processor. Set to zero or leave blank to process gifts immediately.",
        required: false,
        defaultValue: 30,
        order: 3,
        category: "Gift",
        key: AttributeKeys.ProcessingDelayMinutes )]

    [MemoField(
        name: "Help Response",
        description: "The response that will be sent if the sender's message doesn't make sense, there is missing information, or an error occurs. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "Something went wrong. To give, simply text ‘{{ Keyword }} 100’ or ‘{{ Keyword }} $123.45’. Please contact us if you need help.",
        order: 4,
        category: "Response",
        key: AttributeKeys.HelpResponse )]

    [MemoField(
        name: "Max Amount Response",
        description: "The response that will be sent if the sender is trying to give more than the max amount (if configured). <span class='tip tip-lava'></span>",
        required: false,
        defaultValue: "Thank you for your generosity but our mobile giving solution cannot process a gift this large. Please give using our website.",
        order: 5,
        category: "Response",
        key: AttributeKeys.MaxAmountResponse )]

    [MemoField(
        name: "Setup Response",
        description: "The response that will be sent if the sender is unknown, does not have a saved account, or requests to edit their giving profile. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "Hi there! Please use our website to setup your giving profile before using this mobile giving solution.",
        order: 6,
        category: "Response",
        key: AttributeKeys.SetupResponse )]

    [MemoField(
        name: "Success Response",
        description: "The response that will be sent if the payment is successful. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "Thank you! We received your gift of {{ GiftAmount }} to the {{ AccountName }}.",
        order: 7,
        category: "Response",
        key: AttributeKeys.SuccessResponse )]

    public class SmsActionGive : SmsActionComponent
    {
        private static class AttributeKeys
        {
            public const string Keyword = "Keyword";
            public const string MaxAmount = "MaxAmount";
            public const string MaxAmountResponse = "MaxAmountResponse";
            public const string ProcessingDelayMinutes = "ProcessingDelayMinutes";
            public const string HelpResponse = "HelpResponse";
            public const string SetupResponse = "SetupResponse";
            public const string SuccessResponse = "SuccessResponse";
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
        public override string Description => "Allows an SMS sender to make a payment.";

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

            var keyword = GetKeyword( action );
            var messageText = message.Message.Trim();

            return messageText.StartsWith( keyword, StringComparison.CurrentCultureIgnoreCase );
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
            var rockContext = new RockContext();

            var person = message.FromPerson;
            var keyword = GetKeyword( action );
            var messageText = message.Message.Trim();
            var giftAmountNullable = GetGiftAmount( keyword, messageText );
            var maxAmount = GetMaxAmount( action );
            var defaultSavedAccount = GetDefaultSavedAccount( rockContext, person );

            // If the number is not recognized, the person doesn't have a configured account designation, or the person
            // doesn't have a default saved payment method, send the "setup" response
            if ( defaultSavedAccount == null || person == null || !person.ContributionFinancialAccountId.HasValue || !person.PrimaryAliasId.HasValue )
            {
                var lavaTemplate = GetSetupResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, keyword, message );
            }

            // If the amount is not valid (missing, not valid decimal, or not valid currency format), send back the
            // "help" response
            if ( !giftAmountNullable.HasValue || giftAmountNullable.Value < 1m )
            {
                var lavaTemplate = GetHelpResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, keyword, message );
            }

            // If the gift amount exceeds the max amount, send the "max amount" response
            var giftAmount = giftAmountNullable.Value;
            var exceedsMax = maxAmount.HasValue && giftAmount > maxAmount.Value;

            if ( exceedsMax )
            {
                var lavaTemplate = GetMaxAmountResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, keyword, message, giftAmount.FormatAsCurrency() );
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
                return GetResolvedSmsResponse( lavaTemplate, keyword, message, giftAmount.FormatAsCurrency() );
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
                return GetResolvedSmsResponse( lavaTemplate, keyword, message, giftAmount.FormatAsCurrency() );
            }
            else
            {
                var lavaTemplate = GetSuccessResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, keyword, message, giftAmount.FormatAsCurrency() );
            }            
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
        /// Take the lava template, resolve it with useful text-to-give fields, and generate an SMS object to respond with
        /// </summary>
        /// <param name="lavaTemplate">The response lava template</param>
        /// <param name="keyword">The keyword attribute</param>
        /// <param name="message">The original request message</param>
        /// <param name="formattedGiftAmount">The formatted gift amount, if parsed</param>
        /// <returns></returns>
        private static SmsMessage GetResolvedSmsResponse( string lavaTemplate, string keyword, SmsMessage message, string formattedGiftAmount = null )
        {
            // Add some useful lava fields for the rock admin to make meaningful SMS responses
            var accountName = message.FromPerson?.ContributionFinancialAccount?.PublicName;

            var mergeObjects = new Dictionary<string, object>
            {
                { "Message", message },
                { "Keyword", keyword },
                { "GiftAmount", formattedGiftAmount },
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

        #region Attribute Getters

        /// <summary>
        /// Get and validate the keyword attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetKeyword( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var keyword = action.GetAttributeValue( AttributeKeys.Keyword );

            if ( string.IsNullOrWhiteSpace( keyword ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.Keyword );
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

        #endregion
    }
}
