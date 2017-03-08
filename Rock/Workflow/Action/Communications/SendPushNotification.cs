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
    [Description( "Sends a push notification. The recipient can either be a person or group attribute or a phone number entered in the 'To' field." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Push Notification Send" )]

    [WorkflowTextOrAttribute( "Recipient", "Attribute Value", "The phone number or an attribute that contains the person or phone number that notification should be sent to. <span class='tip tip-lava'></span>", true, "", "", 1, "To",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]
    [TextField( "Subject", "The subject that should be used when sending a notification. This isn't required. <span class='tip tip-lava'></span>", false, "", "", 2 )]
    [WorkflowTextOrAttribute( "Message", "Attribute Value", "The message or an attribute that contains the message that should be sent. <span class='tip tip-lava'></span>", true, "", "", 3, "Message",
        new string[] { "Rock.Field.Types.TextFieldType" } )]

    class SendPushNotification : ActionComponent
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

            //int? fromId = null;
            //Guid? fromGuid = GetAttributeValue( action, "From" ).AsGuidOrNull();
            //if ( fromGuid.HasValue )
            //{
            //    var fromValue = DefinedValueCache.Read( fromGuid.Value, rockContext );
            //    if ( fromValue != null )
            //    {
            //        fromId = fromValue.Id;
            //    }
            //}

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
                                        var personAliasId = new PersonAliasService( rockContext ).Queryable()
                                            .Where( pa => pa.Guid.Equals( personAliasGuid ) )
                                            .Select( pa => pa.AliasPersonId )
                                            .FirstOrDefault();

                                        var personalDevice = new PersonalDeviceService( rockContext ).Queryable()
                                            .Where( pd => pd.PersonAliasId == personAliasId && pd.NotificationsEnabled )
                                            .FirstOrDefault();

                                        if ( personalDevice == null )
                                        {
                                            action.AddLogEntry( "Invalid Recipient: Valid personal device not found", true );
                                        }
                                        else
                                        {
                                            string deviceRegistrationId = personalDevice.DeviceRegistrationId;

                                            var recipient = new RecipientData( deviceRegistrationId );
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
                                            var personalDevice = new PersonalDeviceService( rockContext ).Queryable()
                                                .Where( pd => pd.PersonAliasId == person.PrimaryAliasId && pd.NotificationsEnabled )
                                                .FirstOrDefault();
                                            if ( personalDevice != null )
                                            {
                                                string deviceRegistrationId = personalDevice.DeviceRegistrationId;

                                                var recipient = new RecipientData( deviceRegistrationId );
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

            string message = GetAttributeValue( action, "Message" );
            Guid messageGuid = message.AsGuid();
            if ( !messageGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Read( messageGuid, rockContext );
                if ( attribute != null )
                {
                    string messageAttributeValue = action.GetWorklowAttributeValue( messageGuid );
                    if ( !string.IsNullOrWhiteSpace( messageAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" )
                        {
                            message = messageAttributeValue;
                        }
                    }
                }
            }

            if ( recipients.Any() && !string.IsNullOrWhiteSpace( message ) )
            {
                var mediumEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid(), rockContext );
                if ( mediumEntity != null )
                {
                    var medium = MediumContainer.GetComponent( mediumEntity.Name );
                    if ( medium != null && medium.IsActive )
                    {
                        var transport = medium.Transport;
                        if ( transport != null && transport.IsActive )
                        {
                            var appRoot = GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" );

                            foreach ( var recipient in recipients )
                            {
                                var recipientMergeFields = new Dictionary<string, object>( mergeFields );
                                foreach ( var mergeField in recipient.MergeFields )
                                {
                                    recipientMergeFields.Add( mergeField.Key, mergeField.Value );
                                }
                                var mediumData = new Dictionary<string, string>();
                                mediumData.Add( "Message", message.ResolveMergeFields( recipientMergeFields ) );

                                var device = new List<string> { recipient.To };

                                transport.Send( mediumData, device, appRoot, string.Empty );
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
