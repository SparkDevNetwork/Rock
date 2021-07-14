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
    [Description( "Sends a SMS message. The recipient can either be a person or group attribute or a phone number entered in the 'To' field." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "SMS Send" )]

    [WorkflowTextOrAttribute(
        textLabel: "From",
        attributeLabel: "Attribute Value",
        description: "The number to originate message from (configured under Admin Tools > Communications > SMS Phone Numbers).",
        required: true,
        order: 0,
        key: AttributeKey.From,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.DefinedValueFieldType" } )]
    [WorkflowTextOrAttribute(
        textLabel: "Recipient",
        attributeLabel: "Attribute Value",
        description: "The phone number or an attribute that contains the person or phone number that message should be sent to. <span class='tip tip-lava'></span>",
        required: true,
        order: 1,
        key: AttributeKey.To,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]
    [WorkflowTextOrAttribute(
        textLabel: "Message",
        attributeLabel: "Attribute Value",
        description: "The message or an attribute that contains the message that should be sent. <span class='tip tip-lava'></span>",
        required: false,
        order: 2,
        key: AttributeKey.Message,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]
    [WorkflowAttribute(
        "Attachment",
        Description = "Workflow attribute that contains the attachment to be added. Note that when sending attachments with MMS; jpg, gif, and png images are supported for all carriers. Support for other file types is dependent upon each carrier and device. So make sure to test sending this to different carriers and phone types to see if it will work as expected.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.Attachment,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.FileFieldType", "Rock.Field.Types.ImageFieldType" } )]
    [BooleanField(
        name: "Save Communication History",
        description: "Should a record of this communication be saved. If a person is provided then it will save to the recipient's profile. If a phone number is provided then the communication record is saved but a communication recipient is not.",
        defaultValue: false,
        category: "",
        order: 4,
        key: AttributeKey.SaveCommunicationHistory )]
    public class SendSms : ActionComponent
    {
        #region Workflow Attributes

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// From
            /// </summary>
            public const string From = "From";

            /// <summary>
            /// To
            /// </summary>
            public const string To = "To";

            /// <summary>
            /// Message
            /// </summary>
            public const string Message = "Message";

            /// <summary>
            /// Attachment
            /// </summary>
            public const string Attachment = "Attachment";

            /// <summary>
            /// SaveCommunicationHistory
            /// </summary>
            public const string SaveCommunicationHistory = "SaveCommunicationHistory";
        }

        #endregion Workflow Attributes

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

            // If the configured fromGuid is not in the SMS FromDefinedValues, then
            // that guid is probably from an attribute and therefore we need to get the fromGuid
            // from the workflow's attribute value instead.
            Guid fromGuid = GetAttributeValue( action, AttributeKey.From ).AsGuid();
            var smsFromDefinedValues = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() ).DefinedValues;
            if ( !smsFromDefinedValues.Any( a => a.Guid == fromGuid ) )
            {
                var workflowAttributeValue = action.GetWorkflowAttributeValue( fromGuid );

                if ( workflowAttributeValue.IsNotNullOrWhiteSpace() )
                {
                    fromGuid = workflowAttributeValue.AsGuid();
                }
            }

            // Now can we do our final check to ensure the guid is a valid inactive reason.
            if ( !smsFromDefinedValues.Any( a => a.Guid == fromGuid ) )
            {
                var msg = string.Format( "Inactive Reason could not be found for selected value ('{0}')!", fromGuid.ToString() );
                errorMessages.Add( msg );
                action.AddLogEntry( msg, true );
                return false;
            }

            var fromId = smsFromDefinedValues.First( a => a.Guid == fromGuid ).Id;

            // Get the recipients
            var recipients = new List<RockSMSMessageRecipient>();
            string toValue = GetAttributeValue( action, "To" );
            Guid guid = toValue.AsGuid();
            if ( !guid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( guid, rockContext );
                if ( attribute != null )
                {
                    string toAttributeValue = action.GetWorkflowAttributeValue( guid );
                    if ( !string.IsNullOrWhiteSpace( toAttributeValue ) )
                    {
                        switch ( attribute.FieldType.Class )
                        {
                            case "Rock.Field.Types.TextFieldType":
                                {
                                    var smsNumber = toAttributeValue;
                                    recipients.Add( RockSMSMessageRecipient.CreateAnonymous( smsNumber, mergeFields ) );
                                    break;
                                }
                            case "Rock.Field.Types.PersonFieldType":
                                {
                                    Guid personAliasGuid = toAttributeValue.AsGuid();
                                    if ( !personAliasGuid.IsEmpty() )
                                    {
                                        var phoneNumber = new PersonAliasService( rockContext ).Queryable()
                                            .Where( a => a.Guid.Equals( personAliasGuid ) )
                                            .SelectMany( a => a.Person.PhoneNumbers )
                                            .Where( p => p.IsMessagingEnabled )
                                            .FirstOrDefault();

                                        if ( phoneNumber == null )
                                        {
                                            action.AddLogEntry( "Invalid Recipient: Person or valid SMS phone number not found", true );
                                        }
                                        else
                                        {
                                            var person = new PersonAliasService( rockContext ).GetPerson( personAliasGuid );

                                            var recipient = new RockSMSMessageRecipient( person, phoneNumber.ToSmsNumber(), mergeFields );
                                            recipients.Add( recipient );
                                            recipient.MergeFields.Add( recipient.PersonMergeFieldKey, person );
                                        }
                                    }
                                    break;
                                }

                            case "Rock.Field.Types.GroupFieldType":
                            case "Rock.Field.Types.SecurityRoleFieldType":
                                {
                                    int? groupId = toAttributeValue.AsIntegerOrNull();
                                    Guid? groupGuid = toAttributeValue.AsGuidOrNull();
                                    IQueryable<GroupMember> qry = null;

                                    // Handle situations where the attribute value is the ID
                                    if ( groupId.HasValue )
                                    {
                                        qry = new GroupMemberService( rockContext ).GetByGroupId( groupId.Value );
                                    }

                                    // Handle situations where the attribute value stored is the Guid
                                    else if ( groupGuid.HasValue )
                                    {
                                        qry = new GroupMemberService( rockContext ).GetByGroupGuid( groupGuid.Value );
                                    }
                                    else
                                    {
                                        action.AddLogEntry( "Invalid Recipient: No valid group id or Guid", true );
                                    }

                                    if ( qry != null )
                                    {
                                        foreach ( var person in qry
                                            .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                                            .Select( m => m.Person ) )
                                        {
                                            var phoneNumber = person.PhoneNumbers
                                                .Where( p => p.IsMessagingEnabled )
                                                .FirstOrDefault();
                                            if ( phoneNumber != null )
                                            {
                                                var recipientMergeFields = new Dictionary<string, object>( mergeFields );
                                                var recipient = new RockSMSMessageRecipient( person, phoneNumber.ToSmsNumber(), recipientMergeFields );
                                                recipients.Add( recipient );
                                                recipient.MergeFields.Add( recipient.PersonMergeFieldKey, person );
                                            }
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
                if ( !string.IsNullOrWhiteSpace( toValue ) )
                {
                    recipients.Add( RockSMSMessageRecipient.CreateAnonymous( toValue.ResolveMergeFields( mergeFields ), mergeFields ) );
                }
            }

            // Get the message from the Message attribute.
            // NOTE: Passing 'true' as the checkWorkflowAttributeValue will also check the workflow AttributeValue
            // which allows us to remove the unneeded code.
            string message = GetAttributeValue( action, "Message", checkWorkflowAttributeValue: true );

            // Add the attachment (if one was specified)
            var attachmentBinaryFileGuid = GetAttributeValue( action, "Attachment", true ).AsGuidOrNull();
            BinaryFile binaryFile = null;

            if ( attachmentBinaryFileGuid.HasValue && attachmentBinaryFileGuid != Guid.Empty )
            {
                binaryFile = new BinaryFileService( rockContext ).Get( attachmentBinaryFileGuid.Value );
            }

            // Send the message
            if ( recipients.Any() && ( !string.IsNullOrWhiteSpace( message ) || binaryFile != null ) )
            {
                var smsMessage = new RockSMSMessage();
                smsMessage.SetRecipients( recipients );
                smsMessage.FromNumber = DefinedValueCache.Get( fromId );
                smsMessage.Message = message;
                smsMessage.CreateCommunicationRecord = GetAttributeValue( action, "SaveCommunicationHistory" ).AsBoolean();
                smsMessage.communicationName = action.ActionTypeCache.Name;

                if ( binaryFile != null )
                {
                    smsMessage.Attachments.Add( binaryFile );
                }

                smsMessage.Send();
            }
            else
            {
                action.AddLogEntry( "Warning: No text or attachment was supplied so nothing was sent.", true );
            }

            return true;
        }
    }
}