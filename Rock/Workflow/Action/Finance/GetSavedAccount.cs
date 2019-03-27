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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute with the ID of a financial person saved account ID with the person's saved
    /// account for the given gateway that is the default or, if no default, the first.
    /// </summary>
    [ActionCategory( "Finance" )]
    [Description( "Sets an attribute with the default or, if no default, the first saved account for the given person and given gateway." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Get Saved Account" )]

    [FinancialGatewayField( "Financial Gateway", "Workflow attribute that indicates the financial gateway associated with the saved account.", true, "", "", 0, null )]
    [WorkflowAttribute( "Person", "Workflow attribute that contains the person who should be the owner of the saved account.", true, "", "", 1, null, new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [BooleanField( "Continue On Error", "Should processing continue even if processing errors occur?", false, "", 2 )]
    [WorkflowAttribute( "Result Attribute", "An attribute to set to calculated saved account ID.", false, "", "", 3, null, new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    public class GetDefaultSavedAccount : ActionComponent
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

            // If there are any errors accumulated, exit
            if ( errorMessages.Any() )
            {
                return HandleExit( action, errorMessages, continueOnError );
            }

            // Find the saved account
            var savedAccountQry = new FinancialPersonSavedAccountService( rockContext )
                .GetByPersonId( person.Id )
                .AsNoTracking()
                .Where( sa => sa.FinancialGatewayId == gateway.Id );
            var savedAccount = savedAccountQry.FirstOrDefault( sa => sa.IsDefault ) ?? savedAccountQry.FirstOrDefault();

            // Log the result and set the result attribute
            if ( savedAccount == null )
            {
                action.AddLogEntry( string.Format(
                    "{0} does not have a saved account for {1}",
                    person.FullName,
                    gateway.Name ) );

                var attribute = SetWorkflowAttributeValue( action, "ResultAttribute", ( int? ) null );
                if ( attribute != null )
                {
                    action.AddLogEntry( string.Format( "Set '{0}' attribute to null.", attribute.Name ) );
                }
            }
            else
            {
                action.AddLogEntry( string.Format(
                    "{0} has a saved account with ID {1} for {2}",
                    person.FullName,
                    savedAccount.Id,
                    gateway.Name ) );

                var attribute = SetWorkflowAttributeValue( action, "ResultAttribute", savedAccount.Id );
                if ( attribute != null )
                {
                    action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, savedAccount.Id ) );
                }
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