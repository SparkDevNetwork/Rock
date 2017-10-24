<%@ WebHandler Language="C#" Class="Mandrill" %>
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
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Newtonsoft.Json;

using Rock;
using Rock.Model;
using Rock.Workflow.Action;

public class Mandrill : IHttpHandler
{
    private HttpRequest request;
    private HttpResponse response;
    private int transactionCount = 0;

    public void ProcessRequest( HttpContext context )
    {
        request = context.Request;
        response = context.Response;

        response.ContentType = "text/plain";

        if ( request.HttpMethod != "POST" )
        {
            response.Write( "Invalid request type." );
            return;
        }

        if ( request.Form["mandrill_events"] != null )
        {
            string postedData = request.Form["mandrill_events"];

            var rockContext = new Rock.Data.RockContext();

            var payload = JsonConvert.DeserializeObject<IEnumerable<MailEvent>>( postedData );
            int unsavedCommunicationCount = 0;

            foreach ( var item in payload )
            {
                transactionCount++;
                unsavedCommunicationCount++;

                if ( item.Msg != null && item.Msg.Metadata != null )
                {
                    // Process a SendEmailWithEvents workflow action 
                    if ( item.Msg.Metadata.ContainsKey( "workflow_action_guid" ) )
                    {
                        Guid? actionGuid = item.Msg.Metadata["workflow_action_guid"].AsGuidOrNull();
                        string status = string.Empty;
                        switch ( item.EventType )
                        {
                            case MandrillEventType.Send: status = SendEmailWithEvents.SENT_STATUS; break;
                            case MandrillEventType.Opened: status = SendEmailWithEvents.OPENED_STATUS; break;
                            case MandrillEventType.Clicked: status = SendEmailWithEvents.CLICKED_STATUS; break;
                            case MandrillEventType.HardBounced:
                            case MandrillEventType.Rejected:
                            case MandrillEventType.SoftBounced:
                            case MandrillEventType.Spam:
                            case MandrillEventType.Unsubscribe:
                                status = SendEmailWithEvents.FAILED_STATUS; break;
                        }

                        if ( actionGuid != null && !string.IsNullOrWhiteSpace( status ) )
                        {
                            SendEmailWithEvents.UpdateEmailStatus( actionGuid.Value, status, item.EventType.ConvertToString().SplitCase(), rockContext, true );
                        }
                    }

                    // process a communication recipient
                    if ( item.Msg.Metadata.ContainsKey( "communication_recipient_guid" ) )
                    {
                        Guid communicationRecipientGuid;
                        if ( Guid.TryParse( item.Msg.Metadata["communication_recipient_guid"], out communicationRecipientGuid ) )
                        {
                            var communicationRecipient = new CommunicationRecipientService( rockContext ).Get( communicationRecipientGuid );

                            if ( communicationRecipient != null && communicationRecipient.Communication != null )
                            {
                                if ( item.UserAgent == null )
                                {
                                    item.UserAgent = new UserAgent();
                                }

                                InteractionComponent interactionComponent = new InteractionComponentService( rockContext )
                                    .GetComponentByEntityId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid(),
                                        communicationRecipient.CommunicationId, communicationRecipient.Communication.Subject );
                                rockContext.SaveChanges();

                                var interactionService = new InteractionService( rockContext );

                                switch ( item.EventType )
                                {
                                    case MandrillEventType.Send:
                                        communicationRecipient.Status = CommunicationRecipientStatus.Delivered;
                                        communicationRecipient.StatusNote = String.Format( "Confirmed delivered by Mandrill at {0}", item.EventDateTime.ToString() );
                                        break;

                                    case MandrillEventType.Opened:
                                        communicationRecipient.Status = CommunicationRecipientStatus.Opened;
                                        communicationRecipient.OpenedDateTime = item.EventDateTime;
                                        communicationRecipient.OpenedClient = String.Format( "{0} {1} ({2})",
                                                                                item.UserAgent.OperatingSystemName ?? "unknown",
                                                                                item.UserAgent.UserAgentName ?? "unknown",
                                                                                item.UserAgent.Type ?? "unknown" );

                                        if ( interactionComponent != null )
                                        {
                                            interactionService.AddInteraction( interactionComponent.Id, communicationRecipient.Id, "Opened", "", communicationRecipient.PersonAliasId, item.EventDateTime,
                                                item.UserAgent.UserAgentName, item.UserAgent.OperatingSystemName, item.UserAgent.Type, null, item.IpAddress, null );
                                        }
                                        break;

                                    case MandrillEventType.Clicked:
                                        if ( interactionComponent != null )
                                        {
                                            interactionService.AddInteraction( interactionComponent.Id, communicationRecipient.Id, "Click", "", communicationRecipient.PersonAliasId, item.EventDateTime,
                                                item.UserAgent.UserAgentName, item.UserAgent.OperatingSystemName, item.UserAgent.Type, null, item.IpAddress, null );
                                        }
                                        break;
                                }
                            }
                        }
                    }

                    // save every 100 changes
                    if ( unsavedCommunicationCount >= 100 )
                    {
                        rockContext.SaveChanges();
                        unsavedCommunicationCount = 0;
                    }
                }

                // final save
                rockContext.SaveChanges();

                // if bounced process the bounced message
                if ( item.EventType == MandrillEventType.HardBounced )
                {
                    string bounceDescription = string.Empty;
                    if ( !string.IsNullOrEmpty( item.Msg.BounceDescription ) )
                    {
                        if ( item.Msg.BounceDescription == "bad_mailbox" )
                        {
                            bounceDescription = "Mailbox does not exist";
                        }
                        else if ( item.Msg.BounceDescription == "invalid_domain" )
                        {
                            bounceDescription = "Domain is invalid";
                        }
                    }


                    if ( !string.IsNullOrEmpty( item.Msg.Email ) )
                    {
                        Rock.Communication.Email.ProcessBounce( item.Msg.Email, Rock.Communication.BounceType.HardBounce, bounceDescription, item.EventDateTime );
                    }
                }
            }
        }

        response.Write( String.Format( "Success: Processed {0} transactions.", transactionCount.ToString() ) );

        // must do this or Mandrill will not accept your webhook!
        response.StatusCode = 200;

    }

    // see mandrill webhook format definitions at: http://help.mandrill.com/entries/24466132-Webhook-Format

    public enum MandrillEventType { Send, HardBounced, Opened, Spam, Rejected, Delayed, Clicked, SoftBounced, Unsubscribe, Unknown };

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

    public class MailEvent
    {
        [JsonProperty( PropertyName = "ts" )]
        public string TimeStamp { get; set; }

        public DateTime EventDateTime
        {
            get
            {
                // Unix timestamp is seconds past epoch
                double timeStampSeconds = Double.TryParse( TimeStamp, out timeStampSeconds ) ? timeStampSeconds : 0;
                System.DateTime dtDateTime = new DateTime( 1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc );
                return Rock.RockDateTime.ConvertLocalDateTimeToRockDateTime( dtDateTime.AddSeconds( timeStampSeconds ).ToLocalTime() );
            }

        }

        [JsonProperty( PropertyName = "ip" )]
        public string IpAddress { get; set; }

        [JsonProperty( PropertyName = "url" )]
        public string UrlAddress { get; set; }

        [JsonProperty( PropertyName = "user_agent_parsed" )]
        public UserAgent UserAgent { get; set; }

        [JsonProperty( PropertyName = "location" )]
        public EventLocation EventLocation { get; set; }

        [JsonProperty( PropertyName = "event" )]
        public string Event { get; set; }

        public MandrillEventType EventType
        {
            get
            {
                if ( Event == "open" )
                {
                    return MandrillEventType.Opened;
                }
                else if ( Event == "click" )
                {
                    return MandrillEventType.Clicked;
                }
                else if ( Event == "send" )
                {
                    return MandrillEventType.Send;
                }
                else if ( Event == "deferral" )
                {
                    return MandrillEventType.Delayed;
                }
                else if ( Event == "hard_bounce" )
                {
                    return MandrillEventType.HardBounced;
                }
                else if ( Event == "spam" )
                {
                    return MandrillEventType.Spam;
                }
                else if ( Event == "reject" )
                {
                    return MandrillEventType.Rejected;
                }
                else if ( Event == "soft_bounce" )
                {
                    return MandrillEventType.SoftBounced;
                }

                return MandrillEventType.Unknown;
            }
        }

        [JsonProperty( PropertyName = "msg" )]
        public Message Msg { get; set; }
    }

    public class Message
    {
        [JsonProperty( PropertyName = "raw_msg" )]
        public string RawMessage { get; set; }

        [JsonProperty( PropertyName = "bounce_description" )]
        public string BounceDescription { get; set; }

        [JsonProperty( PropertyName = "headers" )]
        public Header Header { get; set; }

        [JsonProperty( PropertyName = "metadata" )]
        public Metadata Metadata { get; set; }

        [JsonProperty( PropertyName = "text" )]
        public string Text { get; set; }

        [JsonProperty( PropertyName = "html" )]
        public string Html { get; set; }

        [JsonProperty( PropertyName = "from_email" )]
        public string FromEmail { get; set; }

        [JsonProperty( PropertyName = "from_name" )]
        public string FromName { get; set; }

        [JsonProperty( PropertyName = "to" )]
        public string[][] To { get; set; }

        [JsonProperty( PropertyName = "email" )]
        public string Email { get; set; }

        [JsonProperty( PropertyName = "subject" )]
        public string Subject { get; set; }

        [JsonProperty( PropertyName = "tags" )]
        public string[] Tags { get; set; }

        [JsonProperty( PropertyName = "sender" )]
        public string Sender { get; set; }

        [JsonProperty( PropertyName = "dkim" )]
        public DKIM DKIM { get; set; }

        [JsonProperty( PropertyName = "spf" )]
        public SPF SPF { get; set; }

        [JsonProperty( PropertyName = "spam_report" )]
        public SpamReport SpamReport { get; set; }
    }

    [JsonDictionary()]
    public class Header : Dictionary<string, object>
    { }

    [JsonDictionary()]
    public class Metadata : Dictionary<string, string>
    { }

    public class EventLocation
    {
        [JsonProperty( PropertyName = "country_short" )]
        public string CountryShort { get; set; }

        [JsonProperty( PropertyName = "country_long" )]
        public string CountryLong { get; set; }

        [JsonProperty( PropertyName = "region" )]
        public string Region { get; set; }

        [JsonProperty( PropertyName = "city" )]
        public string City { get; set; }

        [JsonProperty( PropertyName = "postal_code" )]
        public string PostalCode { get; set; }

        [JsonProperty( PropertyName = "timezone" )]
        public string TimeZone { get; set; }

        [JsonProperty( PropertyName = "latitude" )]
        public string Latitude { get; set; }

        [JsonProperty( PropertyName = "longitude" )]
        public string Longitude { get; set; }
    }

    public class UserAgent
    {
        [JsonProperty( PropertyName = "mobile" )]
        public bool IsMobile { get; set; }

        [JsonProperty( PropertyName = "os_company" )]
        public string OperatingSystemVendor { get; set; }

        [JsonProperty( PropertyName = "os_company_url" )]
        public string OperatingSystemVendorUrl { get; set; }

        [JsonProperty( PropertyName = "os_family" )]
        public string OperatingSystemFamily { get; set; }

        [JsonProperty( PropertyName = "os_icon" )]
        public string OperatingSystemIcon { get; set; }

        [JsonProperty( PropertyName = "os_name" )]
        public string OperatingSystemName { get; set; }

        [JsonProperty( PropertyName = "os_url" )]
        public string OperatingSystemUrl { get; set; }

        [JsonProperty( PropertyName = "type" )]
        public string Type { get; set; }

        [JsonProperty( PropertyName = "ua_company" )]
        public string UserAgentCompany { get; set; }

        [JsonProperty( PropertyName = "ua_company_url" )]
        public string UserAgentCompanyUrl { get; set; }

        [JsonProperty( PropertyName = "ua_family" )]
        public string UserAgentFamily { get; set; }

        [JsonProperty( PropertyName = "ua_icon" )]
        public string UserAgentIcon { get; set; }

        [JsonProperty( PropertyName = "ua_name" )]
        public string UserAgentName { get; set; }

        [JsonProperty( PropertyName = "ua_url" )]
        public string UserAgentUrl { get; set; }

        [JsonProperty( PropertyName = "ua_version" )]
        public string UserAgentVersion { get; set; }
    }

    public class SpamReport
    {
        [JsonProperty( PropertyName = "score" )]
        public decimal Score { get; set; }

        [JsonProperty( PropertyName = "matched_rules" )]
        public SpamRule[] MatchedRules { get; set; }
    }

    public class SpamRule
    {
        [JsonProperty( PropertyName = "name" )]
        public string Name { get; set; }

        [JsonProperty( PropertyName = "score" )]
        public decimal Score { get; set; }

        [JsonProperty( PropertyName = "description" )]
        public string Description { get; set; }
    }

    public class DKIM
    {
        [JsonProperty( PropertyName = "signed" )]
        public bool Signed { get; set; }

        [JsonProperty( PropertyName = "valid" )]
        public bool Valid { get; set; }
    }

    public class SPF
    {
        [JsonProperty( PropertyName = "result" )]
        public string Result { get; set; }

        [JsonProperty( PropertyName = "detail" )]
        public string Detail { get; set; }
    }
}