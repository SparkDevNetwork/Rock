<%@ WebHandler Language="C#" Class="Mailgun" %>
// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Data.Entity;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Newtonsoft.Json;

using Rock;
using Rock.Model;
using Rock.Workflow.Action;

public class Mailgun : IHttpHandler
{
    private HttpRequest request;
    private HttpResponse response;

    public void ProcessRequest( HttpContext context )
    {
        request = context.Request;
        response = context.Response;

        response.ContentType = "text/plain";

        if ( request.HttpMethod != "POST" )
        {
            response.Write( "Invalid request type." );
            response.StatusCode = 406;
            return;
        }

        using ( var rockContext = new Rock.Data.RockContext() )
        {

            int timestamp = request.Form["timestamp"].AsInteger();
            string token = request.Form["token"];
            string signature = request.Form["signature"];
            string apiKey = string.Empty;

            var mailgunEntity = Rock.Web.Cache.EntityTypeCache.Read( "Rock.Communication.Transport.MailgunSmtp", false );
            if ( mailgunEntity != null )
            {
                apiKey = new AttributeValueService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( v =>
                        v.Attribute.EntityTypeId == mailgunEntity.Id &&
                        v.Attribute.Key == "APIKey" )
                    .Select( v => v.Value )
                    .FirstOrDefault();
            }

            if ( !ValidSignature( apiKey, timestamp, token, signature ) )
            {
                response.Write( "Invalid request signature." );
                response.StatusCode = 406;
                return;
            }

            string eventType = request.Form["event"];

            if ( !string.IsNullOrWhiteSpace( request.Form["workflow_action_guid"] ) )
            {
                Guid? actionGuid = request.Form["workflow_action_guid"].Split( ',' )[0].AsGuidOrNull();
                string status = string.Empty;
                switch ( eventType )
                {
                    case "complained":
                    case "unsubscribed":
                    case "delivered": status = SendEmailWithEvents.SENT_STATUS; break;
                    case "clicked": status = SendEmailWithEvents.CLICKED_STATUS; break;
                    case "opened": status = SendEmailWithEvents.OPENED_STATUS; break;
                    case "dropped":
                    case "suppress-bounce":
                    case "bounced":
                        status = SendEmailWithEvents.FAILED_STATUS;
                        int secs = request.Form["timestamp"].AsInteger();
                        DateTime ts = RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( secs ).ToLocalTime() );
                        Rock.Communication.Email.ProcessBounce(
                               request.Form["recipient"],
                               Rock.Communication.BounceType.HardBounce,
                               request.Form["notification"],
                               ts );
                        break;
                }
                if ( actionGuid != null && !string.IsNullOrWhiteSpace( status ) )
                {
                    SendEmailWithEvents.UpdateEmailStatus( actionGuid.Value, status, eventType, rockContext, true );
                }
            }

            if ( !string.IsNullOrWhiteSpace( request.Form["communication_recipient_guid"] ) )
            {
                // Split on comma if it exists to deal with MailGun issue 269764 (issue #1478)
                Guid? communicationRecipientGuid = request.Form["communication_recipient_guid"].Split( ',' )[0].AsGuidOrNull();
                if ( communicationRecipientGuid.HasValue )
                {
                    var communicationRecipientService = new CommunicationRecipientService( rockContext );
                    var interactionComponentService = new InteractionComponentService( rockContext );
                    var interactionService = new InteractionService( rockContext );

                    var communicationRecipient = communicationRecipientService.Get( communicationRecipientGuid.Value );
                    if ( communicationRecipient != null && communicationRecipient.Communication != null )
                    {

                        int secs = request.Form["timestamp"].AsInteger();
                        DateTime ts = RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( secs ).ToLocalTime() );

                        InteractionComponent interactionComponent = null;
                        var interactionChannel = new InteractionChannelService( rockContext ).Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );
                        if ( interactionChannel != null )
                        {
                            interactionComponent = interactionComponentService.Queryable()
                                    .Where( c =>
                                        c.ChannelId == interactionChannel.Id &&
                                        c.EntityId == communicationRecipient.CommunicationId )
                                    .FirstOrDefault();
                            if ( interactionComponent == null )
                            {

                                interactionComponent = new InteractionComponent();
                                interactionComponent.Name = communicationRecipient.Communication.Subject;
                                interactionComponent.EntityId = communicationRecipient.Communication.Id;
                                interactionComponent.ChannelId = interactionChannel.Id;
                                interactionComponentService.Add( interactionComponent );
                                rockContext.SaveChanges();
                            }
                        }


                        switch ( eventType )
                        {
                            case "delivered":
                                communicationRecipient.Status = CommunicationRecipientStatus.Delivered;
                                communicationRecipient.StatusNote = String.Format( "Confirmed delivered by Mailgun at {0}", ts.ToString() );
                                break;

                            case "opened":
                                communicationRecipient.Status = CommunicationRecipientStatus.Opened;
                                communicationRecipient.OpenedDateTime = ts;
                                communicationRecipient.OpenedClient = String.Format( "{0} {1} ({2})",
                                    request.Form["client-os"] ?? "unknown",
                                    request.Form["client-name"] ?? "unknown",
                                    request.Form["device-type"] ?? "unknown" );

                                if ( interactionComponent != null )
                                {
                                    Interaction openActivity = new Interaction();
                                    openActivity.InteractionComponentId = interactionComponent.Id;
                                    openActivity.EntityId = communicationRecipient.Id;
                                    openActivity.Operation = "Opened";
                                    openActivity.InteractionDateTime = ts;
                                    openActivity.PersonAliasId = communicationRecipient.PersonAliasId;

                                    var openInteractionDeviceType = interactionService.GetInteractionDeviceType( request.Form["client-name"], request.Form["client-os"], request.Form["client-type"], request.Form["device-type"] );
                                    var openInteractionSession = interactionService.GetInteractionSession( null, request.Form["ip"], openInteractionDeviceType.Id );

                                    openActivity.InteractionSessionId = openInteractionSession.Id;
                                    interactionService.Add( openActivity );
                                }
                                break;

                            case "clicked":
                                if ( interactionComponent != null )
                                {
                                    Interaction clickActivity = new Interaction();
                                    clickActivity.InteractionComponentId = interactionComponent.Id;
                                    clickActivity.EntityId = communicationRecipient.Id;
                                    clickActivity.Operation = "Click";
                                    clickActivity.InteractionData = request.Form["url"];
                                    clickActivity.InteractionDateTime = ts;
                                    clickActivity.PersonAliasId = communicationRecipient.PersonAliasId;

                                    var clickInteractionDeviceType = interactionService.GetInteractionDeviceType( request.Form["client-name"], request.Form["client-os"], request.Form["client-type"], request.Form["device-type"] );
                                    var clickInteractionSession = interactionService.GetInteractionSession( null, request.Form["ip"], clickInteractionDeviceType.Id );

                                    clickActivity.InteractionSessionId = clickInteractionSession.Id;
                                    interactionService.Add( clickActivity );
                                }
                                break;

                            case "complained": break;
                            case "unsubscribed": break;

                            case "dropped":
                                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                communicationRecipient.StatusNote = request.Form["description"];
                                break;

                            case "bounced":
                                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                communicationRecipient.StatusNote = request.Form["notification"];

                                Rock.Communication.Email.ProcessBounce(
                                    request.Form["recipient"],
                                    Rock.Communication.BounceType.HardBounce,
                                    request.Form["notification"],
                                    ts );

                                break;
                        }

                        rockContext.SaveChanges();
                    }
                }
            }

            response.Write( String.Format( "Successfully processed '{0}' message", eventType ) );
            response.StatusCode = 200;
        }
    }

    private static bool ValidSignature( string key, int timestamp, string token, string signature )
    {
        var encoding = System.Text.Encoding.ASCII;
        var hmacSha256 = new System.Security.Cryptography.HMACSHA256( encoding.GetBytes( key ) );
        var cleartext = encoding.GetBytes( timestamp + token );
        var hash = hmacSha256.ComputeHash( cleartext );
        var computedSignature = BitConverter.ToString( hash ).Replace( "-", "" ).ToLower();

        return computedSignature == signature;
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}
