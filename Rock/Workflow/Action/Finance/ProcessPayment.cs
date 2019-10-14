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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Processes a payment for a person with a saved account
    /// </summary>
    [ActionCategory( "Finance" )]
    [Description( "Processes a payment from a saved account." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Process Payment" )]

    [FinancialGatewayField( "Financial Gateway", "Workflow attribute that indicates the automated financial gateway to use.", true, "", "", 0, null )]
    [WorkflowAttribute( "Person", "Workflow attribute that contains the person making the payment.", true, "", "", 1, null, new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Amount", "Workflow attribute that contains the amount to charge.", true, "", "", 2, null, new string[] { "Rock.Field.Types.CurrencyFieldType" } )]
    [WorkflowAttribute( "Account", "Workflow attribute that contains the target account.", true, "", "", 3, null, new string[] { "Rock.Field.Types.AccountFieldType" } )]
    [BooleanField( "Enable Duplicate Checking", "Should the processor try to prevent repeat charges?", true, "", 4 )]
    [BooleanField( "Continue On Error", "Should processing continue even if processing errors occur?", false, "", 5 )]
    [WorkflowAttribute( "Result Attribute", "An optional attribute to set to the result transaction ID.", false, "", "", 6, null, new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    public class ProcessPayment : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var continueOnError = GetAttributeValue( action, "ContinueOnError" ).AsBoolean();

            // Get the gateway
            var gateway = GetEntityFromAttributeValue<FinancialGateway>( action, "FinancialGateway", true, rockContext );

            if ( gateway == null )
            {
                errorMessages.Add( "The gateway is not valid" );
            }

            // Get the person
            var person = GetPersonFromAttributeValue( action, "Person", true, rockContext );

            if ( person == null || !person.PrimaryAliasId.HasValue )
            {
                errorMessages.Add( "A valid person was not provided." );
            }

            // Get the amount
            var amount = GetAttributeValue( action, "Amount", true ).AsDecimalOrNull();

            if ( !amount.HasValue || amount.Value < 1m )
            {
                errorMessages.Add( "A valid amount was not provided." );
            }

            // Get the account
            var account = GetEntityFromAttributeValue<FinancialAccount>( action, "Account", true, rockContext );

            if ( account == null )
            {
                errorMessages.Add( "The account is not valid" );
            }

            if ( errorMessages.Any() )
            {
                return HandleExit( action, errorMessages, continueOnError );
            }

            var detailArgs = new AutomatedPaymentArgs.AutomatedPaymentDetailArgs
            {
                AccountId = account.Id,
                Amount = amount.Value
            };

            var automatedPaymentArgs = new AutomatedPaymentArgs
            {
                AuthorizedPersonAliasId = person.PrimaryAliasId.Value,
                AutomatedGatewayId = gateway.Id,
                AutomatedPaymentDetails = new List<AutomatedPaymentArgs.AutomatedPaymentDetailArgs> { detailArgs }
            };

            var enableDuplicateChecking = GetAttributeValue( action, "EnableDuplicateChecking" ).AsBooleanOrNull() ?? true;
            var automatedPaymentProcessor = new AutomatedPaymentProcessor( null, automatedPaymentArgs, rockContext, enableDuplicateChecking, true );
            var transaction = automatedPaymentProcessor.ProcessCharge( out var errorMessage );

            if ( !string.IsNullOrEmpty( errorMessage ) )
            {
                errorMessages.Add( errorMessage );
                return HandleExit( action, errorMessages, continueOnError );
            }

            if ( transaction == null )
            {
                errorMessages.Add( "No transaction was produced" );
                return HandleExit( action, errorMessages, continueOnError );
            }

            action.AddLogEntry( string.Format(
                "{0} made a payment of {1} to {2} resulting in transaction ID {3}",
                person.FullName,
                amount.FormatAsCurrency(),
                account.PublicName,
                transaction.Id ) );

            var attribute = SetWorkflowAttributeValue( action, "ResultAttribute", transaction.Id );

            if ( attribute != null )
            {
                action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, transaction.Id ) );
            }

            return HandleExit( action, errorMessages, continueOnError );
        }

        private bool HandleExit( WorkflowAction action, List<string> errorMessages, bool continueOnError )
        {
            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
            var hasError = errorMessages.Any();
            return continueOnError || !hasError;
        }
    }
}