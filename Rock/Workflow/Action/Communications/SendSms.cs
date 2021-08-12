﻿// <copyright>
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

    [DefinedValueField(
        "From",
        Description = "The number to originate message from (configured under Admin Tools > Communications > SMS Phone Numbers).",
        Key = AttributeKey.FromFromDropDown,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
        IsRequired = false,
        AllowMultiple = false,
        DisplayDescription = true,
        Order = 0 )]
    [WorkflowAttribute(
        "From (From Attribute)",
        Description = "The number to originate message from (configured under Admin Tools > Communications > SMS Phone Numbers). This will be used if a value is not entered for the From value above.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.FromFromAttribute,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.DefinedValueFieldType" } )]

    [WorkflowTextOrAttribute(
        "Recipient",
        "Attribute Value",
        Description = "The phone number or an attribute that contains the person or phone number that message should be sent to. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.To,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]
    [WorkflowTextOrAttribute(
        "Message",
        "Attribute Value",
        Description = "The message or an attribute that contains the message that should be sent. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.Message,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]
    [WorkflowAttribute(
        "Attachment",
        Description = "Workflow attribute that contains the attachment to be added. Note that when sending attachments with MMS; jpg, gif, and png images are supported for all carriers. Support for other file types is dependent upon each carrier and device. So make sure to test sending this to different carriers and phone types to see if it will work as expected.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.Attachment,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.FileFieldType", "Rock.Field.Types.ImageFieldType" } )]
    [BooleanField(
        "Save Communication History",
        Description = "Should a record of this communication be saved. If a person is provided then it will save to the recipient's profile. If a phone number is provided then the communication record is saved but a communication recipient is not.",
        DefaultBooleanValue = false,
        Order = 5,
        Key = AttributeKey.SaveCommunicationHistory )]
    public class SendSms : ActionComponent
    {
        #region Workflow Attributes

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string FromFromDropDown = "From";
            public const string FromFromAttribute = "FromFromAttribute";
            public const string To = "To";
            public const string Message = "Message";
            public const string Attachment = "Attachment";
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
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var mergeFields = GetMergeFields( action );

            // Get the From value
            int? fromId = null;
            Guid? fromGuid = GetAttributeValue( action, AttributeKey.FromFromDropDown ).AsGuidOrNull();
            if ( fromGuid.HasValue )
            {
                var fromValue = DefinedValueCache.Get( fromGuid.Value, rockContext );
                if ( fromValue != null )
                {
                    fromId = fromValue.Id;
                }
            }

            if ( !fromId.HasValue )
            {
                Guid fromGuidFromAttribute = GetAttributeValue( action, AttributeKey.FromFromAttribute ).AsGuid();

                fromGuid = action.GetWorkflowAttributeValue( fromGuidFromAttribute ).AsGuidOrNull();

                var fromValue = DefinedValueCache.Get( fromGuid.Value, rockContext );
                if ( fromValue != null )
                {
                    fromId = fromValue.Id;
                }
            }

            var smsFromDefinedValues = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() ).DefinedValues;

            // Now can we do our final check to ensure the guid is a valid SMS From Defined Value
            if ( !fromId.HasValue || !smsFromDefinedValues.Any( a => a.Id == fromId ) )
            {
                var msg = string.Format( $"'From' could not be found for selected value ('{fromGuid}')" );
                errorMessages.Add( msg );
                action.AddLogEntry( msg, true );
                return false;
            }

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
                                    else if ( groupGuid.HasValue )
                                    {
                                        // Handle situations where the attribute value stored is the Guid
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
                smsMessage.FromNumber = DefinedValueCache.Get( fromId.Value );
                smsMessage.Message = message;
                smsMessage.CreateCommunicationRecord = GetAttributeValue( action, "SaveCommunicationHistory" ).AsBoolean();
                smsMessage.CommunicationName = action.ActionTypeCache.Name;

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