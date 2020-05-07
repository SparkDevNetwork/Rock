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
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;

using Rock;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// Utility class used by the TextToWorkflow webhook
    /// </summary>
    public static class TextToWorkflow
    {
        /// <summary>
        /// Handles a received message and sets the response using the Workflow's "SMSResponse" attribute value (if any).
        /// If a keyword match cannot be found, it returns an 'unrecognized keyword' response message.
        /// </summary>
        /// <param name="toPhone">To phone.</param>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="message">The message.</param>
        /// <param name="response">The response from the Workflow's "SMSResponse" attribute value.</param>
        public static void MessageRecieved( string toPhone, string fromPhone, string message, out string response )
        {
            response = "The keyword you provided was not recognized as a valid keyword.";

            // get TextToWorkflow defined types for this number
            var definedType = DefinedTypeCache.Get( SystemGuid.DefinedType.TEXT_TO_WORKFLOW.AsGuid() );

            // If there are not any defined values, then return with the invalid keyword response
            if ( definedType == null || definedType.DefinedValues == null || !definedType.DefinedValues.Any() ) return;

            // get the TextToWorkflow values for the selected "To" number
            var smsWorkflows = definedType.DefinedValues.Where( v => v.Value.AsNumeric() == toPhone.AsNumeric() )
                .OrderBy( v => v.Order ).ToList();

            // Iterate through workflows looking for a keyword match (Only the first match will be processed)
            foreach ( DefinedValueCache dvWorkflow in smsWorkflows )
            {
                // Get the Keyword expression attribute and see if it matches the message that was received
                string keywordExpression = dvWorkflow.GetAttributeValue( "KeywordExpression" );
                if ( string.IsNullOrWhiteSpace( keywordExpression ) )
                {
                    // if there was no keyword expression add wild card expression
                    keywordExpression = ".*";
                }

                if ( keywordExpression == "*" )
                {
                    // if the keyword is just a * then replace it
                    keywordExpression = ".*";
                }

                if ( !keywordExpression.StartsWith( "^" ) )
                {
                    // Prefix keyword with start-of-string assertion (input needs to start with selected expression)
                    keywordExpression = $"^{keywordExpression}";
                }

                // If the keyword expression does not match the message that was received ignore this TextToWorkflow value and continue to the next
                var match = Regex.Match( message, keywordExpression, RegexOptions.IgnoreCase );
                if ( !match.Success ) continue;

                // Get the workflow type, If there is not a valid workflow type defined, return with invalid keyword response
                var workflowTypeGuid = dvWorkflow.GetAttributeValue( "WorkflowType" ).AsGuidOrNull();
                if ( !workflowTypeGuid.HasValue ) return;

                // Get the configured workflow type, if it is not valid return with invalid keyword response
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                if ( workflowType == null ) return;

                // Get the list of workflow attributes to set
                var workflowAttributesSettings = new List<KeyValuePair<string, object>>();
                var workflowAttributes = dvWorkflow.Attributes["WorkflowAttributes"];
                if ( workflowAttributes != null )
                {
                    var keyValueField = workflowAttributes.FieldType.Field as KeyValueListFieldType;
                    if ( keyValueField != null )
                    {
                        workflowAttributesSettings = keyValueField.GetValuesFromString( null,
                            dvWorkflow.GetAttributeValue( "WorkflowAttributes" ), workflowAttributes.QualifierValues,
                            false );
                    }
                }

                // Get the template for the workflow name
                string nameTemplate = dvWorkflow.GetAttributeValue( "WorkflowNameTemplate" );

                // Create list of the keyword expressions that matched the received message
                var matchGroups = new List<string>();
                foreach ( var matchItem in match.Groups )
                {
                    matchGroups.Add( matchItem.ToString() );
                }

                // Try to find a person associated with phone number received
                Person fromPerson;
                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    fromPerson = personService.GetPersonFromMobilePhoneNumber( fromPhone, false );

                    LaunchWorkflow( workflowType, nameTemplate, fromPerson, fromPhone, toPhone, message, matchGroups, null, workflowAttributesSettings, out response );
                }
                // once we find one match stop processing
                break;
            }
        }

        /// <summary>
        /// Launches the workflow in response to an incoming SMS message.
        /// </summary>
        /// <param name="workflowType">Type of the workflow to launch.</param>
        /// <param name="nameTemplate">The name template to use to resolve into the workflow name.</param>
        /// <param name="fromPerson">The person who sent the message.</param>
        /// <param name="fromPhone">The phone number the message came from.</param>
        /// <param name="toPhone">The phone number the message was sent to.</param>
        /// <param name="message">The message text.</param>
        /// <param name="attachments">The files that were attached to the message.</param>
        /// <param name="workflowAttributesSettings">The workflow attributes to set on the workflow.</param>
        /// <param name="response">The response to be sent back to the user.</param>
        public static void LaunchWorkflow( WorkflowTypeCache workflowType, string nameTemplate, Person fromPerson, string fromPhone, string toPhone, string message, List<BinaryFile> attachments, List<KeyValuePair<string, object>> workflowAttributesSettings, out string response )
        {
            LaunchWorkflow( workflowType, nameTemplate, fromPerson, fromPhone, toPhone, message, null, attachments, workflowAttributesSettings, out response );
        }

        /// <summary>
        /// Launches the workflow in response to an incoming SMS message.
        /// </summary>
        /// <param name="workflowType">Type of the workflow to launch.</param>
        /// <param name="nameTemplate">The name template to use to resolve into the workflow name.</param>
        /// <param name="fromPerson">The person who sent the message.</param>
        /// <param name="fromPhone">The phone number the message came from.</param>
        /// <param name="toPhone">The phone number the message was sent to.</param>
        /// <param name="message">The message text.</param>
        /// <param name="matchGroups">The match groups found in the deprecated defined value.</param>
        /// <param name="attachments">The files that were attached to the message.</param>
        /// <param name="workflowAttributesSettings">The workflow attributes to set on the workflow.</param>
        /// <param name="response">The response to be sent back to the user.</param>
        private static void LaunchWorkflow( WorkflowTypeCache workflowType, string nameTemplate, Person fromPerson, string fromPhone, string toPhone, string message, List<string> matchGroups, List<BinaryFile> attachments, List<KeyValuePair<string, object>> workflowAttributesSettings, out string response )
        {
            using ( var rockContext = new RockContext() )
            {
                // Activate a new workflow
                var workflow = Model.Workflow.Activate( workflowType, "Request from " + ( fromPhone ?? "??" ), rockContext );

                // Set the workflow initiator
                if ( fromPerson != null )
                {
                    workflow.InitiatorPersonAliasId = fromPerson.PrimaryAliasId;
                }

                attachments = attachments ?? new List<BinaryFile>();

                // Format the phone number that was received
                var formattedPhoneNumber = PhoneNumber.CleanNumber( PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), fromPhone ) );

                // Create a list of mergefields used to update workflow attribute values and the workflow name
                var mergeValues = new Dictionary<string, object>
                {
                    { "FromPhone", formattedPhoneNumber },
                    { "ToPhone", toPhone },
                    { "MessageBody", message },
                    { "MatchedGroups", matchGroups },
                    { "ReceivedTime", RockDateTime.Now.ToString("HH:mm:ss") },
                    { "ReceivedDate", RockDateTime.Now.ToShortDateString() },
                    { "ReceivedDateTime", RockDateTime.Now.ToString("o") },
                    { "Attachments", attachments },
                    { "FromPerson", fromPerson }
                };

                // Set the workflow's FromPhone attribute 
                workflow.SetAttributeValue( "FromPhone", fromPhone );

                // Set any other workflow attributes that could have lava
                foreach ( var attribute in workflowAttributesSettings )
                {
                    workflow.SetAttributeValue( attribute.Key,
                        attribute.Value.ToString().ResolveMergeFields( mergeValues ) );
                }

                //
                // Set the attachments as workflow attributes as well.
                //
                for ( int i = 0; i < attachments.Count; i++ )
                {
                    workflow.SetAttributeValue( $"Attachment{ i + 1 }", attachments[i].Guid.ToString() );
                }

                // Set the workflow name
                var name = nameTemplate.ResolveMergeFields( mergeValues );
                if ( name.IsNotNullOrWhiteSpace() )
                {
                    workflow.Name = name;
                }

                // Process the workflow
                List<string> workflowErrors;
                new WorkflowService( rockContext ).Process( workflow, out workflowErrors );

                // Check to see if there is a response to return
                var responseAttribute = workflow.GetAttributeValue( "SMSResponse" );
                response = !string.IsNullOrWhiteSpace( responseAttribute ) ? responseAttribute : string.Empty;
            }
        }
    }
}
