 using System.Collections.Generic;
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Communication;
using Rock.Data;
using Rock.Web.UI;

namespace Rock.Util.WorkflowAction
{
    /// <summary>
    /// Sets a workflow status
    /// </summary>
    [Description( "Set the workflow status" )]
    [Export(typeof(WorkflowActionComponent))]
    [ExportMetadata("ComponentName", "Set Status")]
    [BlockProperty( 0, "EmailTemplate", "The email template to send", true )]
    [BlockProperty( 1, "Recipient", "The email address to send to", true )]
    public class SendEmail : WorkflowActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="dto">The dto.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( Action action, IDto dto, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var recipientEmail = GetAttributeValue( action, "Recipient" );
            var recipients = new Dictionary<string, Dictionary<string, object>>();
            var mergeObjects = new Dictionary<string, object>();

            if ( dto != null )
            {
                mergeObjects.Add( "Entity", dto.ToDictionary() );
            }

            recipients.Add( recipientEmail, mergeObjects );

            Email email = new Email( new System.Guid( GetAttributeValue( action, "EmailTemplate" ) ) );
            email.Send( recipients );

            action.AddLogEntry( string.Format( "Email sent to '{0}'", recipientEmail ) );
            
            return true;
        }
    }
}