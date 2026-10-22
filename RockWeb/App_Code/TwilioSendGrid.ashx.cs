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
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Rock;
using Rock.Logging;
using Rock.Model;
using Rock.SendGrid.Webhook;
using Rock.Workflow.Action;

public class TwilioSendGrid : IHttpAsyncHandler
{
    public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, Object extraData )
    {
        SendGridResponseAsync sendgridAsync = new SendGridResponseAsync( cb, context, extraData );
        sendgridAsync.StartAsyncWork();
        return sendgridAsync;
    }

    public void EndProcessRequest( IAsyncResult result ) { }

    public void ProcessRequest( HttpContext context )
    {
        throw new InvalidOperationException();
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}

internal class SendGridResponseAsync : IAsyncResult
{
    private bool _completed;
    private readonly Object _state;
    private readonly AsyncCallback _callback;
    private readonly HttpContext _context;
    private readonly ILogger _logger;

    /// <summary>
    /// <c>true</c> when this request's SendGrid signature was verified.
    /// </summary>
    private bool _isSignatureVerified;

    bool IAsyncResult.IsCompleted { get { return _completed; } }
    WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
    Object IAsyncResult.AsyncState { get { return _state; } }
    bool IAsyncResult.CompletedSynchronously { get { return false; } }

    public SendGridResponseAsync( AsyncCallback callback, HttpContext context, Object state )
    {
        _callback = callback;
        _context = context;
        _state = state;
        _completed = false;
        _logger = RockLogger.LoggerFactory.CreateLogger<TwilioSendGrid>();
    }

    public void StartAsyncWork()
    {
        ThreadPool.QueueUserWorkItem( ( workItemState ) =>
        {
            try
            {
                StartAsyncTask( workItemState );
            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( ex );
                _context.Response.StatusCode = 500;
                _completed = true;
                _callback( this );
            }
        }, null );
    }

    private void StartAsyncTask( Object workItemState )
    {
        var request = _context.Request;
        var response = _context.Response;

        response.ContentType = "text/plain";

        if ( request.HttpMethod != "POST" )
        {
            response.Write( "Invalid request type." );
            response.StatusCode = ( int ) System.Net.HttpStatusCode.MethodNotAllowed;
            _completed = true;
            _callback( this );
            return;
        }

        if ( request.ContentType.Contains( "application/json" ) )
        {
            ProcessJsonContent( request, response );
        }
        else
        {
            response.Write( "Invalid content type." );
            response.StatusCode = ( int ) System.Net.HttpStatusCode.NotAcceptable;
        }

        _completed = true;
        _callback( this );
    }

    private void ProcessJsonContent( HttpRequest request, HttpResponse response )
    {
        /*
            10/21/2026 - MSE

            Read the raw body bytes first so the SendGrid signature can be checked
            against exactly what SendGrid signed, and reject the request before any
            event is processed. The bytes are then decoded with the same StreamReader
            (UTF-8 with byte order mark detection) as before, so the JSON that is
            deserialized is unchanged.

            Reason: The SendGrid event webhook accepted unsigned events.
        */
        byte[] body;

        using ( Stream s = request.InputStream )
        {
            using ( var memoryStream = new MemoryStream() )
            {
                s.CopyTo( memoryStream );
                body = memoryStream.ToArray();
            }
        }

        string errorMessage;
        if ( !Rock.Communication.Transport.SendGridHttp.IsEventWebhookRequestAllowed(
            body,
            request.Headers["X-Twilio-Email-Event-Webhook-Signature"],
            request.Headers["X-Twilio-Email-Event-Webhook-Timestamp"],
            out _isSignatureVerified,
            out errorMessage ) )
        {
            response.Write( errorMessage );
            response.StatusCode = ( int ) System.Net.HttpStatusCode.Forbidden;
            return;
        }

        string payload = string.Empty;

        using ( StreamReader readStream = new StreamReader( new MemoryStream( body ), Encoding.UTF8 ) )
        {
            payload = readStream.ReadToEnd();
        }

        var eventList = JsonConvert.DeserializeObject<List<SendGridEvent>>( payload );

        if ( eventList == null )
        {
            response.Write( "Invalid content type." );
            response.StatusCode = ( int ) System.Net.HttpStatusCode.NotAcceptable;
            return;
        }

        ProcessSendGridEventListAsync( eventList );

        response.StatusCode = ( int ) System.Net.HttpStatusCode.OK;
    }

    private void ProcessSendGridEventListAsync( List<SendGridEvent> sendGridEvents )
    {
        var rockContext = new Rock.Data.RockContext();

        foreach ( var sendGridEvent in sendGridEvents )
        {
            ProcessSendGridEvent( sendGridEvent, rockContext );
        }

    }

    private void ProcessSendGridEvent( SendGridEvent sendGridEvent, Rock.Data.RockContext rockContext )
    {

        Guid? actionGuid = null;
        Guid? communicationRecipientGuid = null;

        if ( !string.IsNullOrWhiteSpace( sendGridEvent.WorkflowActionGuid ) )
        {
            actionGuid = sendGridEvent.WorkflowActionGuid.AsGuidOrNull();
        }

        if ( !string.IsNullOrWhiteSpace( sendGridEvent.CommunicationRecipientGuid ) )
        {
            communicationRecipientGuid = sendGridEvent.CommunicationRecipientGuid.AsGuidOrNull();
        }

        if ( actionGuid != null )
        {
            ProcessForWorkflow( actionGuid, rockContext, sendGridEvent );
        }

        if ( communicationRecipientGuid != null )
        {
            ProcessForRecipient( communicationRecipientGuid, rockContext, sendGridEvent );
        }
    }

    /// <summary>
    /// Processes for recipient.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="communicationRecipientGuid">The communication recipient unique identifier.</param>
    /// <param name="rockContext">The rock context.</param>
    private void ProcessForRecipient( Guid? communicationRecipientGuid, Rock.Data.RockContext rockContext, SendGridEvent payload )
    {
        _logger.LogDebug( "ProcessForRecipient {@payload}", payload );

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
            DateTime timeStamp = RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( payload.Timestamp ).ToLocalTime() );

            switch ( payload.EventType )
            {
                case "processed":
                    // Do nothing.
                    break;
                case "dropped":
                    communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                    communicationRecipient.StatusNote = payload.EventTypeReason;

                    if ( payload.EventTypeReason == "Bounced Address" )
                    {
                        Rock.Communication.Email.ProcessBounce(
                            payload.Email,
                            Rock.Communication.BounceType.HardBounce,
                            payload.EventTypeReason,
                            timeStamp );
                    }

                    break;
                case "delivered":
                    communicationRecipient.Status = CommunicationRecipientStatus.Delivered;
                    communicationRecipient.StatusNote = string.Format( "Confirmed delivered by SendGrid at {0}", timeStamp.ToString() );
                    communicationRecipient.DeliveredDateTime = timeStamp;
                    break;
                case "deferred":
                    // TODO: handle deferred.
                    break;
                case "bounce":
                    communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                    communicationRecipient.StatusNote = payload.EventTypeReason + payload.ServerResponse;

                    Rock.Communication.Email.ProcessBounce(
                        payload.Email,
                        Rock.Communication.BounceType.HardBounce,
                        payload.EventTypeReason,
                        timeStamp );
                    break;
                case "blocked":
                    // TODO: handle blocked.
                    break;
                case "open":
                    communicationRecipient.Status = CommunicationRecipientStatus.Opened;
                    communicationRecipient.OpenedDateTime = timeStamp;
                    communicationRecipient.OpenedClient = string.Format(
                        "{0} {1} ({2})",
                        payload.ClientOs ?? "unknown",
                        payload.ClientBrowser ?? "unknown",
                        payload.ClientDeviceType ?? "unknown" );

                    if ( interactionComponent != null )
                    {
                        interactionService.AddInteraction(
                            interactionComponent.Id,
                            communicationRecipient.Id,
                            "Opened",
                            payload.SendGridEventId,
                            communicationRecipient.PersonAliasId,
                            timeStamp,
                            payload.ClientBrowser,
                            payload.ClientOs,
                            payload.ClientDeviceType,
                            payload.ClientDeviceBrand,
                            payload.IpAddress,
                            null );
                    }

                    break;

                case "click":
                    if ( interactionComponent != null )
                    {
                        interactionService.AddInteraction(
                            interactionComponent.Id,
                            communicationRecipient.Id,
                            "Click",
                            payload.Url,
                            communicationRecipient.PersonAliasId,
                            timeStamp,
                            payload.ClientBrowser,
                            payload.ClientOs,
                            payload.ClientDeviceType,
                            payload.ClientDeviceBrand,
                            payload.IpAddress,
                            null );
                    }

                    break;

                case "spamreport":
                    Rock.Communication.Email.ProcessSpamComplaint( payload.Email, timeStamp, true );
                    break;

                case "unsubscribe":
                case "group_unsubscribe":
                case "group_resubscribe":
                    // Do nothing.
                    break;
            }

            rockContext.SaveChanges();
        }
    }

    private void ProcessForWorkflow( Guid? actionGuid, Rock.Data.RockContext rockContext, SendGridEvent payload )
    {
        _logger.LogDebug( "ProcessForWorkflow {@payload}", payload );

        /*
            10/21/2026 - MSE

            Bounce processing below changes every person with the event's email address.
            When the request was not signature verified (a site with no Event Webhook
            Verification Key), only act on workflow events for a workflow action that
            exists, so a made-up workflow_action_guid cannot deactivate any address. On
            those sites a real bounce for a workflow action that no longer exists (deleted
            or purged workflow) or never existed in the database (a workflow type that is
            not persisted using Send Email With Events) is then ignored. Signed requests
            come from SendGrid, so they skip this check and such bounces still deactivate
            the address as before. The workflow does not need to be active.

            Reason: Unsigned events could deactivate any email address.
        */
        if ( !_isSignatureVerified && new WorkflowActionService( rockContext ).GetId( actionGuid.Value ) == null )
        {
            return;
        }

        string status = string.Empty;
        switch ( payload.EventType )
        {
            case "unsubscribe":
            case "delivered":
                status = SendEmailWithEvents.SENT_STATUS;
                break;

            case "click":
                status = SendEmailWithEvents.CLICKED_STATUS;
                break;

            case "open":
                status = SendEmailWithEvents.OPENED_STATUS;
                break;

            case "blocked":
                /* 2023-02-28 ETD
                 * Don't process as bounced, this is not permanent like bounce.
                 * Receiving server could not or would not accept the message temporarily. If a recipient has previously unsubscribed from your emails, the message is dropped.
                 * Blocks happen when your message was rejected for a reason related to the message, not the recipient address.
                 * This can happen when your mail server IP address has been added to a deny list, blocked by an ISP, or if the message content is flagged by a filter on the receiving server.
                */

                status = SendEmailWithEvents.FAILED_STATUS;
                break;

            case "failed":
            case "dropped":
            case "bounce":
            case "bounced":
                status = SendEmailWithEvents.FAILED_STATUS;
                string message = payload.ServerResponse.IsNotNullOrWhiteSpace() ? payload.ServerResponse : payload.EventTypeReason;

                Rock.Communication.Email.ProcessBounce(
                        payload.Email,
                        Rock.Communication.BounceType.HardBounce,
                        message,
                        RockDateTime.ConvertLocalDateTimeToRockDateTime( new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( payload.Timestamp ).ToLocalTime() ) );
                break;
        }

        if ( actionGuid != null && !string.IsNullOrWhiteSpace( status ) )
        {
            SendEmailWithEvents.UpdateEmailStatus( actionGuid.Value, status, payload.EventType, rockContext, true );
        }
    }
}