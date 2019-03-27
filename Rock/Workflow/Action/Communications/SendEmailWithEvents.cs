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
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sends email
    /// </summary>
    [ActionCategory( "Communications" )]
    [Description( "Sends an email and performs actions when the email is opened, clicked, or not opened within a certain time period. The recipient can either be a group, person or email address determined by the 'To Attribute' value, or an email address entered in the 'To' field." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Email Send with Events" )]

    [WorkflowTextOrAttribute( "From Email Address", "Attribute Value", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", false, "", "", 0, "From",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowTextOrAttribute( "Send To Email Addresses", "Attribute Value", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", true, "", "", 1, "To",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType" } )]
    [TextField( "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", false, "", "", 2 )]
    [CodeEditorField( "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", Web.UI.Controls.CodeEditorMode.Html, Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 3 )]
    [BooleanField( "Save Communication History", "Should a record of this communication be saved to the recipient's profile", false, "", 4 )]

    [WorkflowActivityType("On Open Activity", "The activity to launch when the email is opened.", false, "", "", 5)]
    [WorkflowActivityType( "On Failed Activity", "The activity to launch when the email fails aka bounces.", false, "", "", 6 )]
    [WorkflowActivityType( "On Clicked Activity", "The activity to launch when a link in the email is clicked.", false, "", "", 7 )]
    [WorkflowActivityType( "Unopened Timeout Activity", "The activity to launch when the email is not opened in a specified amount of time.", false, "", "", 8 )]
    [IntegerField( "Unopened Timeout Length", "The amount of time in hours for the timeout.", false, order: 9 )]
    [WorkflowActivityType( "No Action Timeout Activity", "The activity to launch when no action is taken (e.g. no links are clicked)", false, "", "", 10 )]
    [IntegerField( "No Action Timeout Length", "The amount of time in hours for the timeout", false, order: 11 )]
    public class SendEmailWithEvents : ActionComponent
    {
        /// <summary>
        /// The Sent Status
        /// </summary>
        public const string SENT_STATUS = "Sent";

        /// <summary>
        /// The Opened Status
        /// </summary>
        public const string OPENED_STATUS = "Opened";

        /// <summary>
        /// The Clicked Status
        /// </summary>
        public const string CLICKED_STATUS = "Clicked";

        /// <summary>
        /// The Failed Status
        /// </summary>
        public const string FAILED_STATUS = "Failed";

        /// <summary>
        /// The Timeout Status
        /// </summary>
        public const string TIMEOUT_STATUS = "Timeout";

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

            double hoursElapsed = HoursElapsed( action );
            string emailStatus = EmailStatus( action );

            if ( hoursElapsed <= 0 )
            {
                SendEmail( rockContext, action );
            }
            else
            {
                var timedOut = false;

                WorkflowActivityTypeCache unopenedActivityType = null;
                int? unopenedTimeout = null;
                Guid? guid = GetAttributeValue( action, "UnopenedTimeoutActivity" ).AsGuidOrNull();
                if ( guid.HasValue )
                {
                    unopenedActivityType = WorkflowActivityTypeCache.Get( guid.Value );
                    unopenedTimeout = GetAttributeValue( action, "UnopenedTimeoutLength" ).AsIntegerOrNull();

                    if ( emailStatus != OPENED_STATUS &&
                        emailStatus != CLICKED_STATUS &&
                        unopenedActivityType != null &&
                        unopenedTimeout.HasValue &&
                        unopenedTimeout.Value < hoursElapsed )
                    {
                        action.AddLogEntry( "Unopened Timeout Occurred", true );
                        WorkflowActivity.Activate( unopenedActivityType, action.Activity.Workflow, rockContext );
                        timedOut = true;
                    }
                }

                WorkflowActivityTypeCache noActionActivityType = null;
                int? noActionTimeout = null;
                guid = GetAttributeValue( action, "NoActionTimeoutActivity" ).AsGuidOrNull();
                if ( guid.HasValue )
                {
                    noActionActivityType = WorkflowActivityTypeCache.Get( guid.Value );
                    noActionTimeout = GetAttributeValue( action, "NoActionTimeoutLength" ).AsIntegerOrNull();

                    if ( emailStatus != CLICKED_STATUS &&
                        noActionActivityType != null && 
                        noActionTimeout.HasValue && 
                        noActionTimeout.Value < hoursElapsed )
                    {
                        action.AddLogEntry( "No Action Timeout Occurred", true );
                        WorkflowActivity.Activate( noActionActivityType, action.Activity.Workflow, rockContext );
                        timedOut = true;
                    }
                }

                if ( timedOut )
                {
                    UpdateEmailStatus( action.Guid, TIMEOUT_STATUS, string.Empty, rockContext, false );
                    return true;
                }
            }

            return false;
        }

        private double HoursElapsed( WorkflowAction action )
        {
            // Use the current action type' guid as the key for a 'DateTime Sent' attribute 
            string AttrKey = action.ActionTypeCache.Guid.ToString() + "_DateTimeSent";

            // Check to see if the action's activity does not yet have the 'DateTime Sent' attribute.
            // The first time this action runs on any workflow instance using this action instance, the 
            // attribute will not exist and need to be created
            if ( !action.Activity.Attributes.ContainsKey( AttrKey ) )
            {
                var attribute = new Rock.Model.Attribute();
                attribute.EntityTypeId = action.Activity.TypeId;
                attribute.EntityTypeQualifierColumn = "ActivityTypeId";
                attribute.EntityTypeQualifierValue = action.Activity.ActivityTypeId.ToString();
                attribute.Name = "DateTime Sent";
                attribute.Key = AttrKey;
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ).Id;

                // Need to save the attribute now (using different context) so that an attribute id is returned.
                using ( var newRockContext = new RockContext() )
                {
                    new AttributeService( newRockContext ).Add( attribute );
                    newRockContext.SaveChanges();
                }

                action.Activity.Attributes.Add( AttrKey, AttributeCache.Get( attribute ) );
                var attributeValue = new AttributeValueCache();
                attributeValue.AttributeId = attribute.Id;
                attributeValue.Value = RockDateTime.Now.ToString( "o" );
                action.Activity.AttributeValues.Add( AttrKey, attributeValue );
            }
            else
            {
                // Check to see if this action instance has a value for the 'Delay Activated' attribute
                DateTime? dateSent = action.Activity.GetAttributeValue( AttrKey ).AsDateTime();
                if ( dateSent.HasValue )
                {
                    // If a value does exist, check to see if the number of minutes to delay has passed
                    // since the value was saved
                    return RockDateTime.Now.Subtract( dateSent.Value ).TotalHours;
                }
                else
                {
                    // If no value exists, set the value to the current time
                    action.Activity.SetAttributeValue( AttrKey, RockDateTime.Now.ToString( "o" ) );
                }
            }

            return 0.0D;
        }

        private string EmailStatus( WorkflowAction action )
        {
            // Use the current action type' guid as the key for a 'Email Status' attribute 
            string AttrKey = action.ActionTypeCache.Guid.ToString() + "_EmailStatus";

            // Check to see if the action's activity does not yet have the 'Email Status' attribute.
            // The first time this action runs on any workflow instance using this action instance, the 
            // attribute will not exist and need to be created
            if ( !action.Activity.Attributes.ContainsKey( AttrKey ) )
            {
                var attribute = new Rock.Model.Attribute();
                attribute.EntityTypeId = action.Activity.TypeId;
                attribute.EntityTypeQualifierColumn = "ActivityTypeId";
                attribute.EntityTypeQualifierValue = action.Activity.ActivityTypeId.ToString();
                attribute.Name = "Email Status";
                attribute.Key = AttrKey;
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ).Id;

                // Need to save the attribute now (using different context) so that an attribute id is returned.
                using ( var newRockContext = new RockContext() )
                {
                    new AttributeService( newRockContext ).Add( attribute );
                    newRockContext.SaveChanges();
                }

                action.Activity.Attributes.Add( AttrKey, AttributeCache.Get( attribute ) );
                var attributeValue = new AttributeValueCache();
                attributeValue.AttributeId = attribute.Id;
                attributeValue.Value = string.Empty;
                action.Activity.AttributeValues.Add( AttrKey, attributeValue );
            }
            else
            {
                return action.Activity.GetAttributeValue( AttrKey );
            }

            return string.Empty;
        }

        private void SendEmail( RockContext rockContext, WorkflowAction action )
        {
            var mergeFields = GetMergeFields( action );

            string to = GetAttributeValue( action, "To" );
            string fromValue = GetAttributeValue( action, "From" );
            string subject = GetAttributeValue( action, "Subject" );
            string body = GetAttributeValue( action, "Body" );
            bool createCommunicationRecord = GetAttributeValue( action, "SaveCommunicationHistory" ).AsBoolean();

            string fromEmail = string.Empty;
            string fromName = string.Empty;
            Guid? fromGuid = fromValue.AsGuidOrNull();
            if ( fromGuid.HasValue )
            {
                var attribute = AttributeCache.Get( fromGuid.Value, rockContext );
                if ( attribute != null )
                {
                    string fromAttributeValue = action.GetWorklowAttributeValue( fromGuid.Value );
                    if ( !string.IsNullOrWhiteSpace( fromAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.PersonFieldType" )
                        {
                            Guid personAliasGuid = fromAttributeValue.AsGuid();
                            if ( !personAliasGuid.IsEmpty() )
                            {
                                var person = new PersonAliasService( rockContext ).Queryable()
                                    .Where( a => a.Guid.Equals( personAliasGuid ) )
                                    .Select( a => a.Person )
                                    .FirstOrDefault();
                                if ( person != null && !string.IsNullOrWhiteSpace( person.Email ) )
                                {
                                    fromEmail = person.Email;
                                    fromName = person.FullName;
                                }
                            }
                        }
                        else
                        {
                            fromEmail = fromAttributeValue;
                        }
                    }
                }
            }
            else
            {
                fromEmail = fromValue;
            }

            var metaData = new Dictionary<string, string>();
            metaData.Add( "workflow_action_guid", action.Guid.ToString() );

            Guid? guid = to.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var attribute = AttributeCache.Get( guid.Value, rockContext );
                if ( attribute != null )
                {
                    string toValue = action.GetWorklowAttributeValue( guid.Value );
                    if ( !string.IsNullOrWhiteSpace( toValue ) )
                    {
                        switch ( attribute.FieldType.Class )
                        {
                            case "Rock.Field.Types.TextFieldType":
                                {
                                    Send( toValue, fromEmail, fromName, subject, body, mergeFields, rockContext, createCommunicationRecord, metaData );
                                    break;
                                }
                            case "Rock.Field.Types.PersonFieldType":
                                {
                                    Guid personAliasGuid = toValue.AsGuid();
                                    if ( !personAliasGuid.IsEmpty() )
                                    {
                                        var person = new PersonAliasService( rockContext ).Queryable()
                                            .Where( a => a.Guid.Equals( personAliasGuid ) )
                                            .Select( a => a.Person )
                                            .FirstOrDefault();
                                        if ( person == null )
                                        {
                                            action.AddLogEntry( "Invalid Recipient: Person not found", true );
                                        }
                                        else if ( string.IsNullOrWhiteSpace( person.Email ) )
                                        {
                                            action.AddLogEntry( "Email was not sent: Recipient does not have an email address", true );
                                        }
                                        else if ( !person.IsEmailActive )
                                        {
                                            action.AddLogEntry( "Email was not sent: Recipient email is not active", true );
                                        }
                                        else if ( person.EmailPreference == EmailPreference.DoNotEmail )
                                        {
                                            action.AddLogEntry( "Email was not sent: Recipient has requested 'Do Not Email'", true );
                                        }
                                        else
                                        {
                                            var personDict = new Dictionary<string, object>( mergeFields );
                                            personDict.Add( "Person", person );
                                            Send( person.Email, fromEmail, fromName, subject, body, personDict, rockContext, createCommunicationRecord, metaData );
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
            else
            {
                Send( to.ResolveMergeFields( mergeFields ), fromEmail, fromName, subject, body, mergeFields, rockContext, createCommunicationRecord, metaData );
            }
        }

        private void Send( string recipients, string fromEmail, string fromName, string subject, string body, Dictionary<string, object> mergeFields, RockContext rockContext, bool createCommunicationRecord,
            Dictionary<string, string> metaData )
        {
            var emailMessage = new RockEmailMessage();
            foreach ( string recipient in recipients.SplitDelimitedValues().ToList() )
            {
                emailMessage.AddRecipient( new RecipientData( recipient, mergeFields ) );
            }
            emailMessage.FromEmail = fromEmail;
            emailMessage.FromName = fromName;
            emailMessage.Subject = subject;
            emailMessage.Message = body;
            emailMessage.CreateCommunicationRecord = createCommunicationRecord;
            emailMessage.MessageMetaData = metaData;
            emailMessage.Send();
        }

        /// <summary>
        /// Updates the email status.
        /// </summary>
        /// <param name="actionGuid">The action unique identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="emailEventType">Type of the email event.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="ProcessWorkflow">if set to <c>true</c> [process workflow].</param>
        public static void UpdateEmailStatus( Guid actionGuid, string status, string emailEventType, RockContext rockContext, bool ProcessWorkflow )
        {
            var action = new WorkflowActionService( rockContext ).Get( actionGuid );
            if ( action != null && action.Activity != null )
            {

                string attrKey = action.ActionTypeCache.Guid.ToString() + "_EmailStatus";

                action.Activity.LoadAttributes( rockContext );
                string currentStatus = action.Activity.GetAttributeValue( attrKey );

                // Sometimes Clicked events are reported before opens. If this is the case, do not update the status from clicked to opened.
                bool updateStatus = true;
                if ( status == OPENED_STATUS && currentStatus == CLICKED_STATUS )
                {
                    updateStatus = false;
                }

                if ( !string.IsNullOrWhiteSpace( emailEventType ) && ( emailEventType != status || !updateStatus ) )
                {
                    action.AddLogEntry( string.Format( "Email Event Type: {0}", emailEventType ), true );
                }

                if ( updateStatus )
                {
                    action.Activity.SetAttributeValue( attrKey, status );
                    action.Activity.SaveAttributeValues( rockContext );
                    action.AddLogEntry( string.Format( "Email Status Updated to '{0}'", status ), true );
                }

                Guid? activityGuid = null;
                switch( status )
                {
                    case OPENED_STATUS:
                        {
                            activityGuid = GetActionAttributeValue( action, "OnOpenActivity" ).AsGuid();
                            break;
                        }
                    case CLICKED_STATUS:
                        {
                            activityGuid = GetActionAttributeValue( action, "OnClickedActivity" ).AsGuid();
                            break;
                        }
                    case FAILED_STATUS:
                        {
                            activityGuid = GetActionAttributeValue( action, "OnFailedActivity" ).AsGuid();
                            break;
                        }
                }

                if ( activityGuid.HasValue )
                {
                    var workflow = action.Activity.Workflow;
                    var activityType = WorkflowActivityTypeCache.Get( activityGuid.Value );
                    if ( workflow != null && activityType != null )
                    {
                        WorkflowActivity.Activate( activityType, workflow );
                        action.AddLogEntry( string.Format( "Activated new '{0}' activity", activityType.ToString() ) );

                        if ( ProcessWorkflow )
                        {
                            List<string> workflowErrors;
                            new WorkflowService( rockContext ).Process( workflow, out workflowErrors );
                        }
                    }
                }

            }
        }
    }
}
