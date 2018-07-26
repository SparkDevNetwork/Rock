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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web;
using System.Linq;

using Newtonsoft.Json;

using Rock;
using Rock.Model;
using Rock.Workflow.Action;
using Rock.Web.Cache;

public class Mailgun : IHttpHandler
{
    private HttpRequest request;
    private HttpResponse response;

    /// <summary>
    /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
    /// </summary>
    /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
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

        int timestamp = request.Form["timestamp"].AsInteger();
        string token = request.Form["token"];
        string signature = request.Form["signature"];
        string apiKey = string.Empty;

        using ( var rockContext = new Rock.Data.RockContext() )
        {
            // We need to get the transport type now that there are two
            var emailMediumEntity = EntityTypeCache.Get( "Rock.Communication.Medium.Email" );

            var emailMediumAttribute = new AttributeService( rockContext )
                    .Queryable()
                    .Where( a => a.EntityTypeId == emailMediumEntity.Id && a.Key == "TransportContainer" )
                    .FirstOrDefault();

            var emailMediumAttributeValue = new AttributeValueService( rockContext )
                    .Queryable()
                    .Where( v => v.AttributeId == emailMediumAttribute.Id )
                    .FirstOrDefault();

            var mailgunEntity = EntityTypeCache.Get( emailMediumAttributeValue.Value.AsGuid(), rockContext );

            if ( mailgunEntity != null )
            {
                apiKey = new AttributeValueService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( v => v.Attribute.EntityTypeId == mailgunEntity.Id && v.Attribute.Key == "APIKey" )
                    .Select( v => v.Value )
                    .FirstOrDefault();
            }

            if ( !Rock.Mailgun.MailgunUtilities.AuthenticateMailgunRequest( timestamp, token, signature, apiKey ) )
            {
                response.Write( "Invalid request signature." );
                response.StatusCode = 406;
                return;
            }

            string eventType = request.Form["event"];
            Guid? actionGuid = null;
            Guid? communicationRecipientGuid = null;

            if ( !string.IsNullOrWhiteSpace( request.Form["workflow_action_guid"] ) )
            {
                actionGuid = request.Form["workflow_action_guid"].Split( ',' )[0].AsGuidOrNull();
            }

            if ( !string.IsNullOrWhiteSpace( request.Form["communication_recipient_guid"] ) )
            {
                communicationRecipientGuid = request.Form["communication_recipient_guid"].Split( ',' )[0].AsGuidOrNull();
            }

            if ( !string.IsNullOrWhiteSpace( request.Form["X-Mailgun-Variables"] ) )
            {
                // for http sent email, mailgun puts the info as URL encoded JSON into the form key "X-Mailgun-Variables"
                string mailgunVariables = HttpContext.Current.Server.UrlDecode( request.Form["X-Mailgun-Variables"] );
                if ( mailgunVariables.IsNotNullOrWhiteSpace() )
                {
                    var mailgunVarDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>( mailgunVariables );
                    actionGuid = mailgunVarDictionary.ContainsKey( "workflow_action_guid" ) ? mailgunVarDictionary["workflow_action_guid"].AsGuidOrNull() : null;
                    communicationRecipientGuid = mailgunVarDictionary.ContainsKey( "communication_recipient_guid" ) ? mailgunVarDictionary["communication_recipient_guid"].AsGuidOrNull() : null;
                }
            }

            if ( actionGuid != null )
            {
                ProcessForWorkflow( eventType, actionGuid, rockContext );
            }

            if ( communicationRecipientGuid != null )
            {
                ProcessForReceipent( eventType, communicationRecipientGuid, rockContext );
            }

            response.Write( string.Format( "Successfully processed '{0}' message", eventType ) );
            response.StatusCode = 200;
        }
    }

    /// <summary>
    /// Processes for recipient.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="communicationRecipientGuid">The communication recipient unique identifier.</param>
    /// <param name="rockContext">The rock context.</param>
    private void ProcessForReceipent( string eventType, Guid? communicationRecipientGuid, Rock.Data.RockContext rockContext )
    {
        if ( !communicationRecipientGuid.HasValue )
        {
            return;
        }

        var communicationRecipient = new CommunicationRecipientService( rockContext ).Get( communicationRecipientGuid.Value );
        if ( communicationRecipient != null && communicationRecipient.Communication != null )
        {
            int secs = request.Form["timestamp"].AsInteger();
            DateTime ts = RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( secs ).ToLocalTime() );

            var communicationGuid = Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid();

            InteractionComponent interactionComponent = new InteractionComponentService( rockContext )
                .GetComponentByEntityId( communicationGuid, communicationRecipient.CommunicationId, communicationRecipient.Communication.Subject );

            rockContext.SaveChanges();

            var interactionService = new InteractionService( rockContext );

            switch ( eventType )
            {
                case "delivered":
                    communicationRecipient.Status = CommunicationRecipientStatus.Delivered;
                    communicationRecipient.StatusNote = string.Format( "Confirmed delivered by Mailgun at {0}", ts.ToString() );
                    break;

                case "opened":
                    communicationRecipient.Status = CommunicationRecipientStatus.Opened;
                    communicationRecipient.OpenedDateTime = ts;
                    communicationRecipient.OpenedClient = string.Format(
                        "{0} {1} ({2})",
                        request.Form["client-os"] ?? "unknown",
                        request.Form["client-name"] ?? "unknown",
                        request.Form["device-type"] ?? "unknown" );

                    if ( interactionComponent != null )
                    {
                        interactionService.AddInteraction(
                            interactionComponent.Id,
                            communicationRecipient.Id,
                            "Opened",
                            string.Empty,
                            communicationRecipient.PersonAliasId,
                            ts,
                            request.Form["client-name"],
                            request.Form["client-os"],
                            request.Form["client-type"],
                            request.Form["device-type"],
                            request.Form["ip"],
                            null );
                    }

                    break;

                case "clicked":
                    if ( interactionComponent != null )
                    {
                        interactionService.AddInteraction( 
                            interactionComponent.Id,
                            communicationRecipient.Id,
                            "Click",
                            request.Form["url"],
                            communicationRecipient.PersonAliasId,
                            ts,
                            request.Form["client-name"],
                            request.Form["client-os"],
                            request.Form["client-type"],
                            request.Form["device-type"],
                            request.Form["ip"],
                            null );
                    }

                    break;

                case "complained":
                    break;
                case "unsubscribed":
                    break;

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

    /// <summary>
    /// Processes for workflow.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="actionGuid">The action unique identifier.</param>
    /// <param name="rockContext">The rock context.</param>
    private void ProcessForWorkflow( string eventType, Guid? actionGuid, Rock.Data.RockContext rockContext )
    {
        string status = string.Empty;
        switch ( eventType )
        {
            case "complained":
            case "unsubscribed":
            case "delivered":
                status = SendEmailWithEvents.SENT_STATUS;
                break;

            case "clicked":
                status = SendEmailWithEvents.CLICKED_STATUS;
                break;

            case "opened":
                status = SendEmailWithEvents.OPENED_STATUS;
                break;

            case "dropped":
            case "suppress-bounce":
            case "bounced":
                status = SendEmailWithEvents.FAILED_STATUS;
                int secs = request.Form["timestamp"].AsInteger();
                Rock.Communication.Email.ProcessBounce(
                        request.Form["recipient"],
                        Rock.Communication.BounceType.HardBounce,
                        request.Form["notification"],
                        RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( secs ).ToLocalTime() ) );
                break;
        }

        if ( actionGuid != null && !string.IsNullOrWhiteSpace( status ) )
        {
            SendEmailWithEvents.UpdateEmailStatus( actionGuid.Value, status, eventType, rockContext, true );
        }
    }

    /// <summary>
    /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
    /// </summary>
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}
