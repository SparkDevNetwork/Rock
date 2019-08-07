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
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Submits a message into the two-way SMS system as if it was just received from the SMS provider.
    /// </summary>
    [ActionCategory( "Communications" )]
    [Description( "Submits a message into the two-way SMS system as if it was just received from the SMS provider." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "SMS Communication Process Response" )]

    [WorkflowTextOrAttribute( "From Number", "Or Attribute", "The number to report that the message was received from. <span class='tip tip-lava'></span>", true, order: 0, key: "FromNumber" )]
    [WorkflowTextOrAttribute( "To Number", "Or Attribute", "The number to report that the message was sent to. <span class='tip tip-lava'></span>", true, order: 1, key: "ToNumber" )]
    [TextField( "Message", "The message content to process. <span class='tip tip-lava'></span>", true, order: 2 )]
    [WorkflowAttribute( "Error Attribute", "Filled in by the SMS system if an error occurred processing the message. This error should generally be sent back to the original sender. Empty string is set if no error occurred.", false, order: 3 )]
    public class SmsCommunicationProcessResponse : ActionComponent
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

            var mergeFields = GetMergeFields( action );

            var fromNumber = GetAttributeValue( action, "FromNumber", true ).ResolveMergeFields( mergeFields ).Trim();
            var toNumber = GetAttributeValue( action, "ToNumber", true ).ResolveMergeFields( mergeFields ).Trim();
            var message = GetAttributeValue( action, "Message" ).ResolveMergeFields( mergeFields ).Trim();
            var attribute = AttributeCache.Get( GetAttributeValue( action, "ErrorAttribute" ).AsGuid(), rockContext );

            string errorMessage;
            new Rock.Communication.Medium.Sms().ProcessResponse( toNumber, fromNumber, message, out errorMessage );

            action.AddLogEntry( string.Format( "Processed SMS '{2}' from '{0}' to '{1}'", fromNumber, toNumber, message ) );

            if ( !string.IsNullOrWhiteSpace( errorMessage ) )
            {
                action.AddLogEntry( errorMessage );
            }

            if ( attribute != null )
            {
                SetWorkflowAttributeValue( action, attribute.Guid, errorMessage );
            }

            return true;
        }
    }
}