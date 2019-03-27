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
        description: "The number of minutes to delay processing the gifts. Delaying allows for simple refunds within the window because payments have not been sent to the processor.",
        required: true,
        defaultValue: 30,
        order: 3,
        category: "Gift",
        key: AttributeKeys.ProcessingDelayMinutes )]

    [MemoField(
        name: "Help Response",
        description: "The response that will be sent if the sender's message doesn't make sense or is missing information. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "To give, simply text ‘{{ keyword }} 100’ or ‘{{ keyword }} $123.45’.",
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
        /// <returns>
        ///   <c>true</c> if the message should be processed.
        /// </returns>
        public override bool ShouldProcessMessage( SmsActionCache action, SmsMessage message )
        {
            if ( action == null || message == null || message.Message.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // Give the base class a chance to check it's own settings to see if we
            // should process this message.
            if ( !base.ShouldProcessMessage( action, message ) )
            {
                return false;
            }

            var keyword = GetKeyword( action );
            var messageText = message.Message.Trim();

            return DoesKeywordMatch( keyword, messageText );
        }

        /// <summary>
        /// Processes the message that was received from the remote user.
        /// </summary>
        /// <param name="action">The action that contains the configuration for this component.</param>
        /// <param name="message">The message that was received by Rock.</param>
        /// <returns>An SmsMessage that will be sent as the response or null if no response should be sent.</returns>
        public override SmsMessage ProcessMessage( SmsActionCache action, SmsMessage message )
        {
            var rockContext = new RockContext();

            var person = message.FromPerson;
            var keyword = GetKeyword( action );
            var messageText = message.Message.Trim();
            var giftAmountNullable = GetGiftAmount( keyword, messageText );
            var maxAmount = GetMaxAmount( action );
            var defaultSavedAccount = GetDefaultSavedAccount( rockContext, person );

            if ( defaultSavedAccount == null || person == null || !person.ContributionFinancialAccountId.HasValue )
            {
                var lavaTemplate = GetSetupResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, keyword, message );
            }

            if ( !IsAmountValid( giftAmountNullable ) )
            {
                var lavaTemplate = GetHelpResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, keyword, message );
            }

            var giftAmount = giftAmountNullable.Value;
            var exceedsMax = DoesAmountExceedMax( giftAmount, maxAmount );

            if ( exceedsMax )
            {
                var lavaTemplate = GetMaxAmountResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, keyword, message );
            }

            // TODO make the transaction with the processing delay

            return null;
        }

        #endregion

        #region Model Helpers

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

        private static bool DoesKeywordMatch( string keyword, string messageText )
        {
            return messageText.Trim().StartsWith( keyword, StringComparison.CurrentCultureIgnoreCase );
        }

        private static bool IsAmountValid( decimal? amount )
        {
            return amount.HasValue && amount.Value >= 1m;
        }

        private static bool DoesAmountExceedMax( decimal amount, decimal? maxAmount )
        {
            return maxAmount.HasValue && amount > maxAmount.Value;
        }

        private static SmsMessage GetResolvedSmsResponse( string lavaTemplate, string keyword, SmsMessage message )
        {
            var mergeObjects = new Dictionary<string, object>
            {
                { "Message", message },
                { "Keyword", keyword }
            };

            var resolvedMessage = lavaTemplate.ResolveMergeFields( mergeObjects, message.FromPerson );

            return new SmsMessage
            {
                ToNumber = message.FromNumber,
                FromNumber = message.ToNumber,
                Message = resolvedMessage
            };
        }

        #endregion

        #region Attribute Getters

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

        private static decimal? GetMaxAmount( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var maxAmountString = action.GetAttributeValue( AttributeKeys.MaxAmount );
            return maxAmountString.AsDecimalOrNull();
        }

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
