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
    /// Sends push notification
    /// </summary>
    [ActionCategory( "Communications" )]
    [Description( "Sends a push notification. The recipient can either be a person or group attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Push Notification Send" )]

    [WorkflowTextOrAttribute( "Recipient", "Attribute Value", "An attribute that contains the person should be sent to. <span class='tip tip-lava'></span>", true, "", "", 1, "To",
        new string[] { "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.SecurityRoleFieldType" } )]
    [WorkflowTextOrAttribute( "Title", "Attribute Value", "The title or an attribute that contains the title that should be sent.", false, "", "", 2, "Title", new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowAttribute( "Sound", "The choice of sound or an attribute that contains the choice of sound that should be sent.", false, "True", "", 2, "Sound", new string[] { "Rock.Field.Types.BooleanFieldType" } )]
    [WorkflowTextOrAttribute( "Message", "Attribute Value", "The message or an attribute that contains the message that should be sent. <span class='tip tip-lava'></span>", true, "", "", 3, "Message",
        new string[] { "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Url", "Attribute Value", "The URL or an attribute that contains the URL that the notification should link to.", false, "", "", 4, "Url", new string[] { "Rock.Field.Types.TextFieldType" } )]
    public class SendPushNotification : ActionComponent
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
            var recipients = new List<RockPushMessageRecipient>();

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
                            case "Rock.Field.Types.PersonFieldType":
                                {
                                    Guid personAliasGuid = toAttributeValue.AsGuid();
                                    if ( !personAliasGuid.IsEmpty() )
                                    {
                                        var personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid );
                                        List<string> devices = new PersonalDeviceService( rockContext ).Queryable()
                                            .Where( a => a.PersonAliasId.HasValue && a.PersonAliasId == personAlias.Id && a.IsActive && a.NotificationsEnabled )
                                            .Select( a => a.DeviceRegistrationId )
                                            .ToList();

                                        string deviceIds = String.Join( ",", devices );

                                        if ( devices.Count == 0 )
                                        {
                                            action.AddLogEntry( "Invalid Recipient: Person does not have devices that support notifications", true );
                                        }
                                        else
                                        {

                                            var person = new PersonAliasService( rockContext ).GetPerson( personAliasGuid );
                                            var recipient = new RockPushMessageRecipient( person, deviceIds, mergeFields );
                                            recipients.Add( recipient );
                                            if ( person != null )
                                            {
                                                recipient.MergeFields.Add( recipient.PersonMergeFieldKey, person );
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
                                            List<string> devices = new PersonalDeviceService( rockContext ).Queryable()
                                                .Where( p => p.PersonAliasId.HasValue && p.PersonAliasId == person.PrimaryAliasId && p.IsActive && p.NotificationsEnabled && !string.IsNullOrEmpty( p.DeviceRegistrationId ) )
                                                .Select( p => p.DeviceRegistrationId )
                                                .ToList();

                                            string deviceIds = String.Join( ",", devices );

                                            if ( deviceIds.IsNotNullOrWhiteSpace() )
                                            {
                                                var recipient = new RockPushMessageRecipient( person, deviceIds, mergeFields );
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
                    recipients.Add( RockPushMessageRecipient.CreateAnonymous( toValue.ResolveMergeFields( mergeFields ), mergeFields ) );
                }
            }

            string message = GetAttributeValue( action, "Message" );
            Guid messageGuid = message.AsGuid();
            if ( !messageGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( messageGuid, rockContext );
                if ( attribute != null )
                {
                    string messageAttributeValue = action.GetWorkflowAttributeValue( messageGuid );
                    if ( !string.IsNullOrWhiteSpace( messageAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" )
                        {
                            message = messageAttributeValue;
                        }
                    }
                }
            }

            string title = GetAttributeValue( action, "Title" );
            Guid titleGuid = title.AsGuid();
            if ( !titleGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( titleGuid, rockContext );
                if ( attribute != null )
                {
                    string titleAttributeValue = action.GetWorkflowAttributeValue( titleGuid );
                    if ( !string.IsNullOrWhiteSpace( titleAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" )
                        {
                            title = titleAttributeValue;
                        }
                    }
                }
            }

            string sound = GetAttributeValue( action, "Sound" );
            Guid soundGuid = sound.AsGuid();
            if ( !soundGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( soundGuid, rockContext );
                if ( attribute != null )
                {
                    string soundAttributeValue = action.GetWorkflowAttributeValue( soundGuid );
                    if ( !string.IsNullOrWhiteSpace( soundAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.BooleanFieldType" )
                        {
                            sound = soundAttributeValue;
                        }
                    }
                }
            }
            sound = sound.AsBoolean() ? "default" : "";

            string url = GetAttributeValue( action, "Url" );
            Guid urlGuid = url.AsGuid();
            if ( !urlGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( urlGuid, rockContext );
                if ( attribute != null )
                {
                    string urlAttributeValue = action.GetWorkflowAttributeValue( urlGuid );
                    if ( !string.IsNullOrWhiteSpace( urlAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" )
                        {
                            url = urlAttributeValue;
                        }
                    }
                }
            }

            if ( recipients.Any() && !string.IsNullOrWhiteSpace( message ) )
            {
                var pushMessage = new RockPushMessage();
                pushMessage.SetRecipients( recipients );
                pushMessage.Title = title;
                pushMessage.Message = message;
                pushMessage.Sound = sound;
                pushMessage.OpenAction = url.IsNotNullOrWhiteSpace() ? Utility.PushOpenAction.LinkToUrl : Utility.PushOpenAction.NoAction;
                pushMessage.Data = new PushData
                {
                    Url = url
                };

                // Check if the URL is a mobile app style URL, which is "<guid>[?key=value]".
                if ( url.Length >= 36 && Guid.TryParse( url.Substring( 0, 36 ), out var pageGuid ) )
                {
                    var pageId = PageCache.Get( pageGuid )?.Id;

                    if ( pageId.HasValue )
                    {
                        pushMessage.Data.MobilePageId = pageId.Value;

                        // Check if there are any query string values.
                        if ( url.Length >= 38 && url[36] == '?' )
                        {
                            var queryString = url.Substring( 37 ).ParseQueryString();

                            pushMessage.Data.MobilePageQueryString = new Dictionary<string, string>();

                            foreach ( string key in queryString.Keys )
                            {
                                pushMessage.Data.MobilePageQueryString.AddOrReplace( key, queryString[key].ToString() );
                            }
                        }

                        pushMessage.OpenAction = Utility.PushOpenAction.LinkToMobilePage;
                    }
                }

                pushMessage.Send( out errorMessages );
            }

            return true;
        }
    }
}
