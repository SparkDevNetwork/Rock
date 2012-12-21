//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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
    /// Sets a workflow status
    /// </summary>
    [Description( "Set the workflow status" )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Set Status")]
    [TextField( 0, "EmailTemplate", "The email template to send", true )]
    [TextField( 1, "Recipient", "The email address to send to", true )]
    public class SendEmail : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( WorkflowAction action, IEntity entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var recipientEmail = GetAttributeValue( action, "Recipient" );
            var recipients = new Dictionary<string, Dictionary<string, object>>();
            var mergeObjects = new Dictionary<string, object>();

            if ( entity != null )
            {
                mergeObjects.Add( "Entity", entity );
            }

            recipients.Add( recipientEmail, mergeObjects );

            Email email = new Email( new System.Guid( GetAttributeValue( action, "EmailTemplate" ) ) );
            email.Send( recipients );

            action.AddLogEntry( string.Format( "Email sent to '{0}'", recipientEmail ) );
            
            return true;
        }
    }
}