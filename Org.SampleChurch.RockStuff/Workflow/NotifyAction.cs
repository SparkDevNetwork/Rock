//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;
using Rock.Model;
using Rock.Communication;

namespace Org.SampleChurch.RockStuff.Workflow
{
    /// <summary>
    /// Adds the locations for each members group types
    /// </summary>
    [Description( "Send's and email about checkin" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Checkin Notification" )]
    [EmailTemplateField( "EmailTemplate", "The email template to send" )]
    [TextField( "Recipient", "The email address to send to" )]
    public class NotifyAction : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Rock.Model.WorkflowAction action, Rock.Data.IEntity entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( action, out errorMessages );
            if ( checkInState != null )
            {

                errorMessages = new List<string>();

                var recipientEmail = GetAttributeValue( action, "Recipient" );
                var recipients = new Dictionary<string, Dictionary<string, object>>();
                var mergeObjects = new Dictionary<string, object>();

                if ( entity != null )
                {
                    mergeObjects.Add( "Entity", entity );
                }
                mergeObjects.Add( "checkInState", checkInState );

                recipients.Add( recipientEmail, mergeObjects );

                Email email = new Email( new System.Guid( GetAttributeValue( action, "EmailTemplate" ) ) );
                email.Send( recipients );

                action.AddLogEntry( string.Format( "Email sent to '{0}'", recipientEmail ) );

                return true;
            }

            return false;
        }
    }
}