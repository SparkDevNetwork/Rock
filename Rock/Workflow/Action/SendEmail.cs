//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sends email
    /// </summary>
    [Description( "Email the configured recipient the name of the thing being operated against." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Send Email")]
    [EmailTemplateField( "EmailTemplate", "The email template to send" )]
    [TextField( "Recipient", "The email address to send to" )]
    public class SendEmail : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( WorkflowAction action, Object entity, out List<string> errorMessages )
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

            Email email = new Email( new System.Guid( GetAttributeValue( action, "EmailTemplate" ) ) );
            email.Send( recipients );

            action.AddLogEntry( string.Format( "Email sent to '{0}'", recipientEmail ) );
            
            return true;
        }
    }
}