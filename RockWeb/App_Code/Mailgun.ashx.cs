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
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

public class Mailgun : IHttpHandler
{
    private HttpRequest request;
    private HttpResponse response;
    private MailgunRequestPayload mailgunRequestPayload;

    public void ProcessRequest( HttpContext context )
    {
        request = context.Request;
        response = context.Response;
        response.ContentType = "text/plain";

        if ( request.HttpMethod != "POST" )
        {
            response.Write( "Invalid request type." );
            response.StatusCode = ( int ) System.Net.HttpStatusCode.NotAcceptable;
            return;
        }

        switch ( context.Request.ContentType )
        {
            case "application/json":
                ProcessJsonContent();
                break;
            case "application/x-www-form-urlencoded":
                ProcessFormContent();
                break;
            default:
                response.Write( "Invalid content type." );
                response.StatusCode = ( int ) System.Net.HttpStatusCode.NotAcceptable;
                return;
        }
    }

    private void ProcessJsonContent()
    {
        string payload = string.Empty;

        using ( Stream s = request.InputStream )
        {
            using ( StreamReader readStream = new StreamReader( s, Encoding.UTF8 ) )
            {
                payload = readStream.ReadToEnd();
            }
        }

        var jObject = JsonConvert.DeserializeObject<JObject>( payload );
        
        if (jObject == null )
        {
            response.Write( "Invalid content type." );
            response.StatusCode = ( int ) System.Net.HttpStatusCode.NotAcceptable;
            return;
        }

        mailgunRequestPayload = new MailgunRequestPayload();

        if ( jObject["signature"] != null )
        {
            mailgunRequestPayload.TimeStamp = jObject["signature"]["timestamp"] != null ? ( int ) jObject["signature"]["timestamp"] : 0;
            mailgunRequestPayload.Token = jObject["signature"]["token"] != null ? ( string ) jObject["signature"]["token"] : string.Empty;
            mailgunRequestPayload.Signature = jObject["signature"]["signature"] != null ? ( string ) jObject["signature"]["signature"] : string.Empty;
    }

        if ( jObject["event-data"] !=null )
        {
            mailgunRequestPayload.EventType = jObject["event-data"]["event"] != null ? ( string ) jObject["event-data"]["event"] : string.Empty;
            mailgunRequestPayload.EventTypeReason = jObject["event-data"]["reason"] != null ? ( string ) jObject["event-data"]["reason"] : string.Empty;
            mailgunRequestPayload.XMailgunVariables = jObject["event-data"]["user-variables"] != null ? jObject["event-data"]["user-variables"].ToString( Formatting.None ) : string.Empty;
            mailgunRequestPayload.Ip = jObject["event-data"]["ip"] != null ? ( string ) jObject["event-data"]["ip"] : string.Empty;
            mailgunRequestPayload.Url = jObject["event-data"]["url"] != null ? ( string ) jObject["event-data"]["url"] : string.Empty;
            mailgunRequestPayload.Recipient = jObject["event-data"]["recipient"] != null ? ( string ) jObject["event-data"]["recipient"] : string.Empty;

            if ( jObject["event-data"]["client-info"] != null )
            {
                mailgunRequestPayload.ClientName = jObject["event-data"]["client-info"]["client-name"] != null ? ( string ) jObject["event-data"]["client-info"]["client-name"] : string.Empty;
                mailgunRequestPayload.ClientOs = jObject["event-data"]["client-info"]["client-os"] != null ? ( string ) jObject["event-data"]["client-info"]["client-os"] : string.Empty;
                mailgunRequestPayload.ClientType = jObject["event-data"]["client-info"]["client-type"] != null ? ( string ) jObject["event-data"]["client-info"]["client-type"] : string.Empty;
                mailgunRequestPayload.DeviceType = jObject["event-data"]["client-info"]["device-type"] != null ? ( string ) jObject["event-data"]["client-info"]["device-type"] : string.Empty;
            }

            if ( jObject["event-data"]["user-variables"] != null && jObject["event-data"]["user-variables"]["X-Mailgun-Variables"] != null )
            {
                string mailgunVariables = HttpContext.Current.Server.UrlDecode( mailgunRequestPayload.XMailgunVariables );

                if ( mailgunVariables.IsNotNullOrWhiteSpace() )
                {
                    var mailgunLegacyVarDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>( mailgunVariables );
                    var mailgunVarDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>( mailgunLegacyVarDictionary["X-Mailgun-Variables"] );

                    if ( mailgunVarDictionary.ContainsKey( "workflow_action_guid" ) )
                    {
                        mailgunRequestPayload.WorkflowActionGuid = mailgunVarDictionary["workflow_action_guid"];
                    }

                    if ( mailgunVarDictionary.ContainsKey( "communication_recipient_guid" ) )
                    {
                        mailgunRequestPayload.CommunicationRecipientGuid = mailgunVarDictionary["communication_recipient_guid"];
                    }
                }
            }

            if ( jObject["event-data"]["delivery-status"] != null )
            {
                string description = jObject["event-data"]["delivery-status"]["description"] != null ? ( string ) jObject["event-data"]["delivery-status"]["description"] : string.Empty;
                string message = jObject["event-data"]["delivery-status"]["message"] != null ? ( string ) jObject["event-data"]["delivery-status"]["message"] : string.Empty;
                mailgunRequestPayload.Description = description + message;
            }
        }

        ProcessRequest();
    }

    private void ProcessFormContent()
    {
        mailgunRequestPayload = new MailgunRequestPayload
        {
            TimeStamp = request.Form["timestamp"].AsInteger(),
            Token = request.Form["token"] ?? string.Empty,
            Signature = request.Form["signature"] ?? string.Empty,
            EventType = request.Form["event"] ?? string.Empty,
            WorkflowActionGuid = request.Form["workflow_action_guid"] ?? string.Empty,
            CommunicationRecipientGuid = request.Form["communication_recipient_guid"] ?? string.Empty,
            XMailgunVariables = request.Form["X-Mailgun-Variables"] ?? string.Empty,
            ClientName = request.Form["client-name"] ?? string.Empty,
            ClientOs = request.Form["client-os"] ?? string.Empty,
            ClientType = request.Form["client-type"] ?? string.Empty,
            DeviceType = request.Form["device-type"] ?? string.Empty,
            Ip = request.Form["ip"] ?? string.Empty,
            Url = request.Form["url"] ?? string.Empty,
            Description = request.Form["description"] ?? string.Empty,
            Notification = request.Form["notification"] ?? string.Empty,
            Recipient = request.Form["recipient"] ?? string.Empty
        };

        ProcessRequest();
    }

    private void ProcessRequest()
    {
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

            string webhookSigningKey = string.Empty;

            var mailgunEntity = EntityTypeCache.Get( emailMediumAttributeValue.Value.AsGuid(), rockContext );
            if ( mailgunEntity != null )
            {
                /*
                    12/13/2023 - JPH

                    New Mailgun accounts used to be created with a matching key value for "Private API key"and "HTTP
                    webhook signing key". We have historically used this single, matching key value for both sending
                    emails AND validating webhook requests. Mailgun has since changed their account-generation
                    approach as follows:

                        1. A "Private API key" is no longer generated for a newly-created Mailgun account; instead,
                            Mailgun account administrators are responsible for manually generating any desired number
                            of account-wide "Mailgun API keys" and/or domain-specific "Sending API Keys," to be used
                            when sending emails.
                        2. A separate, account-wide "HTTP webhook signing key" is auto-generated at account creation
                           (and may also be recycled at will), to be used for validating webhook requests.

                    This AttributeValue lookup logic has been changed to get the value for a newly-added
                    "HTTPWebhookSigningKey" attribute instead of the "APIKey" attribute. Note that we have also added
                    a data migration as part of this change to copy a Rock instance's "APIKey" attribute value - if
                    present - to the new, "HTTPWebhookSigningKey" attribute value, to ensure Mailgun continues to work
                    for preexisting accounts, while also adding support for newly-created accounts.

                    Reason: Compatibility with preexisting AND newly-created Mailgun accounts.
                */
                webhookSigningKey = new AttributeValueService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( v => v.Attribute.EntityTypeId == mailgunEntity.Id && v.Attribute.Key == "HTTPWebhookSigningKey" )
                    .Select( v => v.Value )
                    .FirstOrDefault();
            }

            if ( webhookSigningKey.IsNullOrWhiteSpace() )
            {
                response.Write( "HTTP webhook signing key not defined." );
                response.StatusCode = 500;
                return;
            }

            if ( !Rock.Mailgun.MailgunUtilities.AuthenticateMailgunRequest( mailgunRequestPayload.TimeStamp, mailgunRequestPayload.Token, mailgunRequestPayload.Signature, webhookSigningKey ) )
            {
                response.Write( "Invalid request signature." );
                response.StatusCode = 406;
                return;
            }

            Guid? actionGuid = null;
            Guid? communicationRecipientGuid = null;

            if ( !string.IsNullOrWhiteSpace( mailgunRequestPayload.WorkflowActionGuid ) )
            {
                actionGuid = mailgunRequestPayload.WorkflowActionGuid.Split( ',' )[0].AsGuidOrNull();
            }

            if ( !string.IsNullOrWhiteSpace( mailgunRequestPayload.CommunicationRecipientGuid ) )
            {
                communicationRecipientGuid = mailgunRequestPayload.CommunicationRecipientGuid.Split( ',' )[0].AsGuidOrNull();
            }

            if ( !string.IsNullOrWhiteSpace( mailgunRequestPayload.XMailgunVariables ) )
            {
                // for http sent email, mailgun puts the info as URL encoded JSON into the form key "X-Mailgun-Variables"
                string mailgunVariables = HttpContext.Current.Server.UrlDecode( mailgunRequestPayload.XMailgunVariables );
                if ( mailgunVariables.IsNotNullOrWhiteSpace() )
                {
                    var mailgunVarDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>( mailgunVariables );

                    if ( actionGuid == null && mailgunVarDictionary.ContainsKey( "workflow_action_guid" ) )
                    {
                        actionGuid = mailgunVarDictionary["workflow_action_guid"].AsGuidOrNull();
                    }

                    if ( communicationRecipientGuid == null && mailgunVarDictionary.ContainsKey( "communication_recipient_guid" ) )
                    {
                        communicationRecipientGuid = mailgunVarDictionary["communication_recipient_guid"].AsGuidOrNull();
                    }
                }
            }

            if ( actionGuid != null )
            {
                ProcessForWorkflow( actionGuid, rockContext );
            }

            if ( communicationRecipientGuid != null )
            {
                ProcessForRecipient( communicationRecipientGuid, rockContext );
            }

            response.Write( string.Format( "Successfully processed '{0}' message", mailgunRequestPayload.EventType ) );
            response.StatusCode = ( int ) System.Net.HttpStatusCode.OK;
        }
    }

    /// <summary>
    /// Processes for recipient.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="communicationRecipientGuid">The communication recipient unique identifier.</param>
    /// <param name="rockContext">The rock context.</param>
    private void ProcessForRecipient( Guid? communicationRecipientGuid, Rock.Data.RockContext rockContext )
    {
        if ( !communicationRecipientGuid.HasValue )
        {
            return;
        }

        var communicationRecipient = new CommunicationRecipientService( rockContext ).Get( communicationRecipientGuid.Value );
        if ( communicationRecipient != null && communicationRecipient.Communication != null )
        {
            
            var communicationGuid = Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid();
            var interactionComponent = new InteractionComponentService( rockContext )
                .GetComponentByEntityId( communicationGuid, communicationRecipient.CommunicationId, communicationRecipient.Communication.Subject );

            rockContext.SaveChanges();

            var interactionService = new InteractionService( rockContext );
            DateTime timeStamp = RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( mailgunRequestPayload.TimeStamp ).ToLocalTime() );

            switch ( mailgunRequestPayload.EventType )
            {
                case "delivered":
                    communicationRecipient.Status = CommunicationRecipientStatus.Delivered;
                    communicationRecipient.StatusNote = string.Format( "Confirmed delivered by Mailgun at {0}", timeStamp.ToString() );
                    break;

                case "opened":
                    communicationRecipient.Status = CommunicationRecipientStatus.Opened;
                    communicationRecipient.OpenedDateTime = timeStamp;
                    communicationRecipient.OpenedClient = string.Format(
                        "{0} {1} ({2})",
                        mailgunRequestPayload.ClientOs ?? "unknown",
                        mailgunRequestPayload.ClientName ?? "unknown",
                        mailgunRequestPayload.DeviceType ?? "unknown" );

                    if ( interactionComponent != null )
                    {
                        interactionService.AddInteraction(
                            interactionComponent.Id,
                            communicationRecipient.Id,
                            "Opened",
                            string.Empty,
                            communicationRecipient.PersonAliasId,
                            timeStamp,
                            mailgunRequestPayload.ClientName,
                            mailgunRequestPayload.ClientOs,
                            mailgunRequestPayload.ClientType,
                            mailgunRequestPayload.DeviceType,
                            mailgunRequestPayload.Ip,
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
                            mailgunRequestPayload.Url,
                            communicationRecipient.PersonAliasId,
                            timeStamp,
                            mailgunRequestPayload.ClientName,
                            mailgunRequestPayload.ClientOs,
                            mailgunRequestPayload.ClientType,
                            mailgunRequestPayload.DeviceType,
                            mailgunRequestPayload.Ip,
                            null );
                    }

                    break;

                case "complained":
                    Rock.Communication.Email.ProcessSpamComplaint( mailgunRequestPayload.Recipient, timeStamp, true );
                    break;

                case "unsubscribed":
                    break;

                case "dropped":
                    communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                    communicationRecipient.StatusNote = mailgunRequestPayload.Description;
                    break;

                case "bounced":
                    communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                    communicationRecipient.StatusNote = mailgunRequestPayload.Notification;

                    Rock.Communication.Email.ProcessBounce(
                        mailgunRequestPayload.Recipient,
                        Rock.Communication.BounceType.HardBounce,
                        mailgunRequestPayload.Notification,
                        timeStamp );
                    break;

                case "failed":
                    // The new mailgun API bundles undeliverable mail into a failed event. The reason (e.g. bounced) is in a seperate property called reason.
                    if( mailgunRequestPayload.EventTypeReason.IsNotNullOrWhiteSpace())
                    {
                        switch ( mailgunRequestPayload.EventTypeReason )
                        {
                            case "bounce":
                            case "suppress-bounce":
                                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                communicationRecipient.StatusNote = mailgunRequestPayload.Description;

                                Rock.Communication.Email.ProcessBounce(
                                    mailgunRequestPayload.Recipient,
                                    Rock.Communication.BounceType.HardBounce,
                                    mailgunRequestPayload.Description,
                                    timeStamp );
                                break;

                            case "suppress-complaint":
                                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                communicationRecipient.StatusNote = mailgunRequestPayload.Description;
                                Rock.Communication.Email.ProcessSpamComplaint( mailgunRequestPayload.Recipient, timeStamp, false );
                                break;

                            default:
                                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                communicationRecipient.StatusNote = mailgunRequestPayload.Description;
                                break;
                        }
                    }
                    
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
    private void ProcessForWorkflow( Guid? actionGuid, Rock.Data.RockContext rockContext )
    {
        string status = string.Empty;
        switch ( mailgunRequestPayload.EventType )
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
                status = SendEmailWithEvents.FAILED_STATUS;
                break;

            case "bounced":
                status = SendEmailWithEvents.FAILED_STATUS;
                string message = mailgunRequestPayload.Notification.IsNotNullOrWhiteSpace() ? mailgunRequestPayload.Notification : mailgunRequestPayload.Description;

                Rock.Communication.Email.ProcessBounce(
                        mailgunRequestPayload.Recipient,
                        Rock.Communication.BounceType.HardBounce,
                        message,
                        RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( mailgunRequestPayload.TimeStamp ).ToLocalTime() ) );
                break;

            case "failed":
                // The new mailgun API bundles undeliverable mail into a failed event. The reason (e.g. bounced) is in a seperate property called reason.
                if( mailgunRequestPayload.EventTypeReason.IsNotNullOrWhiteSpace())
                {
                    switch ( mailgunRequestPayload.EventTypeReason )
                    {
                        case "bounce":
                        case "suppress-bounce":
                            status = SendEmailWithEvents.FAILED_STATUS;

                            Rock.Communication.Email.ProcessBounce(
                                mailgunRequestPayload.Recipient,
                                Rock.Communication.BounceType.HardBounce,
                                mailgunRequestPayload.Description,
                                RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( mailgunRequestPayload.TimeStamp ).ToLocalTime() ) );
                            break;

                        default:
                            status = SendEmailWithEvents.FAILED_STATUS;
                            break;
                    }
                }
                break;

        }

        if ( actionGuid != null && !string.IsNullOrWhiteSpace( status ) )
        {
            SendEmailWithEvents.UpdateEmailStatus( actionGuid.Value, status, mailgunRequestPayload.EventType, rockContext, true );
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

    [Serializable]
    private class MailgunRequestPayload
    {
        public MailgunRequestPayload()
        {
            TimeStamp = 0;
            Token = string.Empty;
            Signature = string.Empty;
            EventType = string.Empty;
            EventTypeReason = string.Empty;
            WorkflowActionGuid = string.Empty;
            CommunicationRecipientGuid = string.Empty;
            XMailgunVariables = string.Empty;
            ClientName = string.Empty;
            ClientOs = string.Empty;
            ClientType = string.Empty;
            DeviceType = string.Empty;
            Ip = string.Empty;
            Url = string.Empty;
            Description = string.Empty;
            Notification = string.Empty;
            Recipient = string.Empty;
        }

        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        /// <value>
        /// The time stamp.
        /// </value>
        public int TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        /// <value>
        /// The signature.
        /// </value>
        public string Signature { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the event type reason.
        /// For a "failed" event type the reason is given.
        /// </summary>
        /// <value>
        /// The event reason.
        /// </value>
        public string EventTypeReason { get; set; }

        /// <summary>
        /// Gets or sets the workflow action unique identifier.
        /// </summary>
        /// <value>
        /// The workflow action unique identifier.
        /// </value>
        public string WorkflowActionGuid { get; set; }

        /// <summary>
        /// Gets or sets the communication recipient unique identifier.
        /// </summary>
        /// <value>
        /// The communication recipient unique identifier.
        /// </value>
        public string CommunicationRecipientGuid { get; set; }

        /// <summary>
        /// Gets or sets the x mailgun variables.
        /// </summary>
        /// <value>
        /// The x mailgun variables.
        /// </value>
        public string XMailgunVariables { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the client os.
        /// </summary>
        /// <value>
        /// The client os.
        /// </value>
        public string ClientOs { get; set; }

        /// <summary>
        /// Gets or sets the type of the client.
        /// </summary>
        /// <value>
        /// The type of the client.
        /// </value>
        public string ClientType { get; set; }

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        /// <value>
        /// The type of the device.
        /// </value>
        public string DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the ip.
        /// </summary>
        /// <value>
        /// The ip.
        /// </value>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// For the new Mailgun API this is an concatenation of Description and message.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the notification.
        /// </summary>
        /// <value>
        /// The notification.
        /// </value>
        public string Notification { get; set; }

        /// <summary>
        /// Gets or sets the recipient.
        /// </summary>
        /// <value>
        /// The recipient.
        /// </value>
        public string Recipient { get; set; }
    }
}
