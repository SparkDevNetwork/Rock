// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Communication;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sends email
    /// </summary>
    [Description( "Email the configured recipient the name of the thing being operated against." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Send Email Template")]

    [EmailTemplateField( "EmailTemplate", "The email template to send. The email templates must be assigned to the 'Workflow' category in order to be displayed on the list." )]
    [TextField( "Recipient", "The email address to send to" )]
    public class SendEmailTemplate : ActionComponent
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

            var recipientEmail = GetAttributeValue( action, "Recipient" );
            var recipients = new Dictionary<string, Dictionary<string, object>>();
            var mergeObjects = new Dictionary<string, object>();

            if ( entity != null )
            {
                mergeObjects.Add( entity.GetType().Name, entity );
            }

            recipients.Add( recipientEmail, mergeObjects );

            Email.Send( GetAttributeValue( action, "EmailTemplate" ).AsGuid(), recipients );

            action.AddLogEntry( string.Format( "Email sent to '{0}'", recipientEmail ) );
            
            return true;
        }
    }
}