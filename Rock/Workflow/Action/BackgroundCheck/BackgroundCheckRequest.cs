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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sends a Background Check Request.
    /// </summary>
    [ActionCategory( "Background Check" )]
    [Description( "Sends a Background Check Request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Background Check Send Request" )]

    [ComponentField( "Rock.Security.BackgroundCheckContainer, Rock", "Background Check Provider", "The Background Check provider to use", false, "", "", 0, "Provider" )]
    [WorkflowAttribute("Person Attribute", "The Person attribute that contains the person who the background check should be submitted for.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "SSN Attribute", "The attribute that contains the Social Security Number of the person who the background check should be submitted for ( Must be an 'Encrypted Text' attribute )", false, "", "", 2, null,
        new string[] { "Rock.Field.Types.SSNFieldType" } )]
    [WorkflowAttribute( "Request Type Attribute", "The attribute that contains the type of background check to submit (Specific to provider).", false, "", "", 3, null)]
    [WorkflowAttribute( "Billing Code Attribute", "The attribute that contains the billing code to use when submitting background check.", false, "", "", 4 )]
    public class BackgroundCheckRequest : ActionComponent
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

            string providerGuid = GetAttributeValue( action, "Provider" );
            if ( !string.IsNullOrWhiteSpace( providerGuid ) )
            {
                var provider = BackgroundCheckContainer.GetComponent( providerGuid );
                if ( provider != null )
                {
                    var personAttribute = AttributeCache.Get( GetAttributeValue( action, "PersonAttribute" ).AsGuid() );
                    var ssnAttribute = AttributeCache.Get( GetAttributeValue( action, "SSNAttribute" ).AsGuid() );
                    var requestTypeAttribute = AttributeCache.Get( GetAttributeValue( action, "RequestTypeAttribute" ).AsGuid() );
                    var billingCodeAttribute = AttributeCache.Get( GetAttributeValue( action, "BillingCodeAttribute" ).AsGuid() );

                    return provider.SendRequest( rockContext, action.Activity.Workflow, personAttribute,
                        ssnAttribute, requestTypeAttribute, billingCodeAttribute, out errorMessages );
                }
                else
                {
                    errorMessages.Add( "Invalid Background Check Provider!" );
                }
            }
            else
            {
                errorMessages.Add( "Invalid Background Check Provider Guid!" );
            }

            return false;
        }
    }
}