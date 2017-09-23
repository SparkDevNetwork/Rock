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

    [DefinedValueField( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM, "From", "The phone number to send message from", true, false, "", "", 0 )]
    [WorkflowTextOrAttribute( "Recipient", "Attribute Value", "The phone number or an attribute that contains the person or phone number that message should be sent to. <span class='tip tip-lava'></span>", true, "", "", 1, "To",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]
    [WorkflowTextOrAttribute( "Message", "Attribute Value", "The message or an attribute that contains the message that should be sent. <span class='tip tip-lava'></span>", true, "", "", 2, "Message",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]
    public class SendSms : ActionComponent
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

            // Get the From value
            int? fromId = null;
            Guid? fromGuid = GetAttributeValue( action, "From" ).AsGuidOrNull();
            if ( fromGuid.HasValue )
            {
                var fromValue = DefinedValueCache.Read( fromGuid.Value, rockContext );
                if ( fromValue != null )
                {
                    fromId = fromValue.Id;
                }
            }

            // Get the recipients
            var recipients = new List<RecipientData>();
            string toValue = GetAttributeValue( action, "To" );
            Guid guid = toValue.AsGuid();
            if ( !guid.IsEmpty() )
            {
                var attribute = AttributeCache.Read( guid, rockContext );
                if ( attribute != null )
                {
                    string toAttributeValue = action.GetWorklowAttributeValue( guid );
                    if ( !string.IsNullOrWhiteSpace( toAttributeValue ) )
                    {
                        switch ( attribute.FieldType.Class )
                        {
                            case "Rock.Field.Types.TextFieldType":
                                {
                                    recipients.Add( new RecipientData( toAttributeValue ) );
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
                                            string smsNumber = phoneNumber.Number;
                                            if ( !string.IsNullOrWhiteSpace( phoneNumber.CountryCode ) )
                                            {
                                                smsNumber = "+" + phoneNumber.CountryCode + phoneNumber.Number;
                                            }

                                            var recipient = new RecipientData( smsNumber );
                                            recipients.Add( recipient );

                                            var person = new PersonAliasService( rockContext ).GetPerson( personAliasGuid );
                                            if ( person != null )
                                            {
                                                recipient.MergeFields.Add( "Person", person );
                                            }
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
                                                string smsNumber = phoneNumber.Number;
                                                if ( !string.IsNullOrWhiteSpace( phoneNumber.CountryCode ) )
                                                {
                                                    smsNumber = "+" + phoneNumber.CountryCode + phoneNumber.Number;
                                                }

                                                var recipient = new RecipientData( smsNumber );
                                                recipients.Add( recipient );
                                                recipient.MergeFields.Add( "Person", person );
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
                    recipients.Add( new RecipientData( toValue.ResolveMergeFields( mergeFields ) ) );
                }
            }

            // Get the message
            string message = GetAttributeValue( action, "Message" );
            Guid? messageGuid = message.AsGuidOrNull();
            if ( messageGuid.HasValue )
            {
                var attribute = AttributeCache.Read( messageGuid.Value, rockContext );
                if ( attribute != null )
                {
                    string messageAttributeValue = action.GetWorklowAttributeValue( messageGuid.Value );
                    if ( !string.IsNullOrWhiteSpace( messageAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" ||
                            attribute.FieldType.Class == "Rock.Field.Types.MemoFieldType" )
                        {
                            message = messageAttributeValue;
                        }
                    }
                }
            }

            // Send the message
            if ( recipients.Any() && !string.IsNullOrWhiteSpace( message ) )
            {
                var smsMessage = new RockSMSMessage();
                smsMessage.SetRecipients( recipients );
                smsMessage.FromNumber = DefinedValueCache.Read( fromId.Value );
                smsMessage.Message = message;
                smsMessage.Send();
            }

            return true;
        }
    }
}