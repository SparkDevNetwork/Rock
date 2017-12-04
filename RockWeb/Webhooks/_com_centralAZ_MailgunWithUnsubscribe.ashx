<%@ WebHandler Language="C#" Class="_com_centralAZ_MailgunWithUnsubscribe" %>
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
using System.IO;

using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Newtonsoft.Json;
using Humanizer;
using Rock;
using Rock.Model;
using Rock.Workflow.Action;

/// <summary>
/// A Mailgun handler class based on the core Mailgun.ashx handler which
/// has the following changes:
///    * processes unsubscribes and complaints by setting all people with that 
///      email address to DoNotEmail.
///    * processes failed and dropped events as if they were bounced events
///      marking their email address inactive and setting the reason note with
///      the reason for the drop/fail.
///    * Writes a demographic history change for each email that is changed.
/// </summary>
public class _com_centralAZ_MailgunWithUnsubscribe : IHttpHandler
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
                    LogMessage( "MAILGUN_unsubs_and_comps", string.Format( "{0} event for communication_recipient_guid {1}", eventType, request.Form["communication_recipient_guid"] ) );
                    var communicationRecipient = new CommunicationRecipientService( rockContext ).Get( communicationRecipientGuid.Value );
                    if ( communicationRecipient != null && communicationRecipient.Communication != null )
                    {
                        int secs = request.Form["timestamp"].AsInteger();
                        DateTime ts = RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( secs ).ToLocalTime() );

                        InteractionComponent interactionComponent = new InteractionComponentService( rockContext )
                            .GetComponentByEntityId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid(),
                                communicationRecipient.CommunicationId, communicationRecipient.Communication.Subject );
                        rockContext.SaveChanges();

                        var interactionService = new InteractionService( rockContext );

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
                                    interactionService.AddInteraction( interactionComponent.Id, communicationRecipient.Id, "Opened", "", communicationRecipient.PersonAliasId, ts,
                                        request.Form["client-name"], request.Form["client-os"], request.Form["client-type"], request.Form["device-type"], request.Form["ip"] );
                                }
                                break;

                            case "clicked":
                                if ( interactionComponent != null )
                                {
                                    interactionService.AddInteraction( interactionComponent.Id, communicationRecipient.Id, "Click", request.Form["url"], communicationRecipient.PersonAliasId, ts,
                                        request.Form["client-name"], request.Form["client-os"], request.Form["client-type"], request.Form["device-type"], request.Form["ip"] );
                                }
                                break;

                            case "complained":
                            case "unsubscribed":
                                ProcessComplaintUnsubscribe(
                                    rockContext,
                                    request.Form["recipient"],
                                    eventType,
                                    communicationRecipient.Communication.Subject,
                                    ts );
                                break;

                            case "failed":
                            case "dropped":
                                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                var reason = string.IsNullOrEmpty( request.Form["description"] ) ? request.Form["reason"] : request.Form["description"];
                                if ( reason == "hardfail" )
                                {
                                    reason = "Not delivering to an address that previously bounced, unsubscribed, or complained.";
                                }
                                communicationRecipient.StatusNote = reason;

                                ProcessBounce(
                                    rockContext,
                                    request.Form["recipient"],
                                    Rock.Communication.BounceType.HardBounce,
                                    reason,
                                    ts );
                                break;

                            case "bounced":
                                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                var note = string.IsNullOrEmpty( request.Form["notification"] ) ? request.Form["error"] : request.Form["notification"];
                                communicationRecipient.StatusNote = note;

                                ProcessBounce(
                                    rockContext,
                                    request.Form["recipient"],
                                    Rock.Communication.BounceType.HardBounce,
                                    note,
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

    /// <summary>
    /// Processes the complaint or unsubscribe.
    /// </summary>
    /// <param name="rockContext">The rock context.</param>
    /// <param name="email">The email.</param>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="bouncedDateTime">The bounced date time.</param>
    private void ProcessComplaintUnsubscribe( Rock.Data.RockContext rockContext, string email, string eventType, string subject, DateTime bouncedDateTime )
    {
        // get people who have those emails
        PersonService personService = new PersonService( rockContext );

        var peopleWithEmail = personService.GetByEmail( email );

        rockContext.WrapTransaction( () =>
        {
            foreach ( var person in peopleWithEmail )
            {
                person.EmailPreference = EmailPreference.DoNotEmail;
                person.EmailNote = String.Format( "Mailgun received a {0} on {1} (Subject: {2}).", eventType, bouncedDateTime.ToShortDateString(), subject );
                RecordEmailChangeToHistory( rockContext, person.Id, person.EmailPreference.ConvertToString(), eventType );
            }

            rockContext.SaveChanges();
        } );
    }

    /// <summary>
    /// Processes the bounce.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="bounceType">Type of the bounce.</param>
    /// <param name="message">The message.</param>
    /// <param name="bouncedDateTime">The bounced date time.</param>
    private static void ProcessBounce( Rock.Data.RockContext rockContext, string email, Rock.Communication.BounceType bounceType, string message, DateTime bouncedDateTime )
    {
        // currently only processing hard bounces
        if ( bounceType == Rock.Communication.BounceType.HardBounce )
        {
            // get people who have those emails
            PersonService personService = new PersonService( rockContext );

            var peopleWithEmail = personService.GetByEmail( email );

            rockContext.WrapTransaction( () =>
            {
                foreach ( var person in peopleWithEmail )
                {
                    person.IsEmailActive = false;
                    person.EmailNote = String.Format( "Email experienced a {0} on {1} ({2}).", bounceType.Humanize(), bouncedDateTime.ToShortDateString(), message );
                    RecordEmailChangeToHistory( rockContext, person.Id, "inactive", message );
                }

                rockContext.SaveChanges();
            } );
        }
    }

    /// <summary>
    /// Records the email change to the person's history.
    /// </summary>
    /// <param name="rockContext">The rock context.</param>
    /// <param name="personId">The person identifier.</param>
    /// <param name="changedToValue">The changed to value.</param>
    /// <param name="reason">The reason.</param>
    private static void RecordEmailChangeToHistory( Rock.Data.RockContext rockContext, int personId, string changedToValue, string reason )
    {
        var changes = new List<string>();
        changes.Add( string.Format( "Email address set to <span class='field-value'>{0}</span> due to <span class='field-value'>{1}</span>.", changedToValue, reason ) );

        HistoryService.SaveChanges(
            rockContext,
            typeof( Person ),
            Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
            personId,
            changes );
    }

    /// <summary>
    /// Logs the message to the App_Data\[fileName].log.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="message">The message.</param>
    private static void LogMessage( string fileName, string message )
    {
        try
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            directory = Path.Combine( directory, "App_Data", "Logs" );

            if ( !Directory.Exists( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            string filePath = Path.Combine( directory, fileName + ".log" );
            string when = RockDateTime.Now.ToString();

            File.AppendAllText( filePath, string.Format( "{0} {1}\r\n", when, message ) );
        }
        catch { }
    }
}