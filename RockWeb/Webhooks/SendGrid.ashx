<%@ WebHandler Language="C#" Class="SendGrid" %>
using System;
using System.Web;
using System.IO;
using System.Linq;
using System.Text;
using Rock;
using Rock.Model;
using Rock.Workflow.Action;
using Sendgrid.Webhooks.Events;

public class SendGrid : IHttpHandler
{
    private HttpRequest _request;
    private HttpResponse _response;
    private int _transactionCount;

    public void ProcessRequest( HttpContext context )
    {
        _request = context.Request;
        _response = context.Response;

        _response.ContentType = "text/plain";

        if ( !( _request.HttpMethod == "POST" && _request.ContentType.Contains( "application/json" ) ) )
        {
            _response.Write( "Invalid request type." );
            return;
        }

        if ( _request != null )
        {
            string postedData = GetDocumentContents( _request );

            var rockContext = new Rock.Data.RockContext();

            var timeZoneInfo = RockDateTime.OrgTimeZoneInfo;
            var parser = new Sendgrid.Webhooks.Service.WebhookParser();
            var events = parser.ParseEvents( postedData );

            if ( events != null )
            {
                int unsavedCommunicationCount = 0;
                foreach ( var item in events )
                {
                    _transactionCount++;
                    unsavedCommunicationCount++;
                    // Process a SendEmailWithEvents workflow action 
                    if ( item.UniqueParameters.ContainsKey( "workflow_action_guid" ) )
                    {
                        var actionGuid = item.UniqueParameters["workflow_action_guid"].AsGuidOrNull();
                        string status = string.Empty;
                        switch ( item.EventType )
                        {
                            case WebhookEventType.Delivered:
                                status = SendEmailWithEvents.SENT_STATUS;
                                break;
                            case WebhookEventType.Open:
                                status = SendEmailWithEvents.OPENED_STATUS;
                                break;
                            case WebhookEventType.Click:
                                status = SendEmailWithEvents.CLICKED_STATUS;
                                break;
                            case WebhookEventType.Dropped:
                            case WebhookEventType.SpamReport:
                            case WebhookEventType.Unsubscribe:
                            case WebhookEventType.Group_Unsubscribe:
                            case WebhookEventType.Bounce:
                                status = SendEmailWithEvents.FAILED_STATUS;
                                break;

                        }

                        if ( actionGuid != null && !string.IsNullOrWhiteSpace( status ) )
                        {
                            SendEmailWithEvents.UpdateEmailStatus( actionGuid.Value, status, item.EventType.ConvertToString().SplitCase(), rockContext, true );
                        }
                    }

                    // Process a regular communication recipient  
                    if ( item.UniqueParameters.ContainsKey( "communication_recipient_guid" ) )
                    {
                        var communicationRecipientService = new CommunicationRecipientService( rockContext );

                        Guid communicationRecipientGuid;
                        if ( Guid.TryParse( item.UniqueParameters["communication_recipient_guid"], out communicationRecipientGuid ) )
                        {
                            var communicationRecipient = communicationRecipientService.Get( communicationRecipientGuid );
                            if ( communicationRecipient != null && communicationRecipient.Communication != null )
                            {
                                var interactionComponentService = new InteractionComponentService( rockContext );
                                var interactionService = new InteractionService( rockContext );

                                InteractionComponent interactionComponent = null;
                                var interactionChannel = new InteractionChannelService( rockContext ).Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );
                                if ( interactionChannel != null )
                                {
                                    interactionComponent = interactionComponentService
                                        .Queryable()
                                        .FirstOrDefault( c => c.InteractionChannelId == interactionChannel.Id &&
                                                              c.EntityId == communicationRecipient.Communication.Id );
                                    if ( interactionComponent == null )
                                    {
                                        interactionComponent = new InteractionComponent();
                                        interactionComponent.Name = communicationRecipient.Communication.Subject;
                                        interactionComponent.EntityId = communicationRecipient.CommunicationId;
                                        interactionComponent.InteractionChannelId = interactionChannel.Id;
                                        interactionComponentService.Add( interactionComponent );
                                        rockContext.SaveChanges();
                                    }
                                }

                                item.Timestamp = TimeZoneInfo.ConvertTime( item.Timestamp, timeZoneInfo );
                                switch ( item.EventType )
                                {
                                    case WebhookEventType.Delivered:
                                        communicationRecipient.Status = CommunicationRecipientStatus.Delivered;
                                        communicationRecipient.StatusNote =
                                            string.Format( "Confirmed delivered by SendGrid at {0}",
                                                item.Timestamp.ToString( "o" ) );
                                        break;
                                    case WebhookEventType.Open:
                                        communicationRecipient.Status = CommunicationRecipientStatus.Opened;
                                        var openEvent = item as OpenEvent;
                                        if ( openEvent != null )
                                        {
                                            communicationRecipient.OpenedDateTime = openEvent.Timestamp;
                                            communicationRecipient.OpenedClient = openEvent.UserAgent.Truncate( 200 ) ??
                                                                                  "Unknown";
                                            if ( interactionComponent != null )
                                            {

                                                CreateInteraction( communicationRecipient, "Opened", item, interactionService, interactionComponent );
                                            }
                                        }
                                        break;
                                    case WebhookEventType.Click:

                                        var clickEvent = item as ClickEvent;
                                        if ( clickEvent != null && interactionComponent != null )
                                        {
                                            CreateInteraction( communicationRecipient, "Click", item, interactionService, interactionComponent );
                                        }
                                        break;
                                    case WebhookEventType.Dropped:
                                        var dropEvent = item as DroppedEvent;
                                        communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                        if ( dropEvent != null )
                                        {
                                            communicationRecipient.StatusNote = string.Format( "{0} by SendGrid at {1}", dropEvent.Reason, dropEvent.Timestamp.ToString( "o" ) );
                                        }
                                        break;
                                    case WebhookEventType.Bounce:
                                        var bounceEvent = item as BounceEvent;
                                        communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                        if ( bounceEvent != null )
                                        {
                                            communicationRecipient.StatusNote = String.Format( "{0} by SendGrid at {1} - {2}", bounceEvent.BounceType, bounceEvent.Timestamp.ToString( "o" ), bounceEvent.Reason );
                                        }
                                        break;
                                    case WebhookEventType.Unsubscribe:
                                    case WebhookEventType.SpamReport:
                                    case WebhookEventType.Group_Unsubscribe:
                                        communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                        communicationRecipient.StatusNote = String.Format( "Unsubscribed or reported as spam at {0}",
                                            item.Timestamp.ToString( "o" ) );
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

                    // if bounced process the bounced message
                    if ( item.EventType == WebhookEventType.Bounce || item.EventType == WebhookEventType.Dropped ||
                            item.EventType == WebhookEventType.SpamReport || item.EventType == WebhookEventType.Unsubscribe ||
                            item.EventType == WebhookEventType.Group_Unsubscribe )
                    {
                        string failDescription = String.Empty;
                        switch ( item.EventType )
                        {
                            case WebhookEventType.Bounce:
                                var bounceEvent = item as BounceEvent;
                                failDescription = bounceEvent.Reason ?? String.Empty;
                                break;
                            case WebhookEventType.Dropped:
                                var dropEvent = item as BounceEvent;
                                if ( dropEvent != null )
                                {
                                    failDescription = dropEvent.Reason ?? String.Empty;

                                }
                                break;
                        }
                        if ( !string.IsNullOrEmpty( item.Email ) )
                        {
                            Rock.Communication.Email.ProcessBounce( item.Email, Rock.Communication.BounceType.HardBounce, failDescription.Truncate( 200 ), item.Timestamp );
                        }
                    }
                }
                // final save
                rockContext.SaveChanges();
            }
        }

        _response.Write( string.Format( "Success: Processed {0} transactions.", _transactionCount ) );

        _response.StatusCode = 200;
    }

    private static Interaction CreateInteraction( CommunicationRecipient communicationRecipient, string operation, WebhookEventBase item, InteractionService interactionService, InteractionComponent interactionComponent )
    {
        var interaction = new Interaction()
        {
            EntityId = communicationRecipient.Id,
            Operation = operation,
            InteractionDateTime = item.Timestamp,
            PersonAliasId = communicationRecipient.PersonAliasId,
            InteractionComponentId = interactionComponent.Id
        };

        if ( item.EventType == WebhookEventType.Click )
        {
            var clickItem = item as ClickEvent;
            interaction.InteractionData = clickItem.Url.IsNotNullOrWhiteSpace() ? PersonToken.ObfuscateRockMagicToken( clickItem.Url ) : string.Empty;
        }

        var itemEvent = item as EngagementEventBase;
        if ( itemEvent == null )
        {
            throw new Exception( "Webhook event is not an engagement event." );
        }
        var interactionDeviceType = interactionService.GetInteractionDeviceType( itemEvent.UserAgent.Truncate(100), null, null, null );
        var interactionSession = interactionService.GetInteractionSession( null, itemEvent.Ip, interactionDeviceType.Id );

        interaction.InteractionSessionId = interactionSession.Id;
        interactionService.Add( interaction );
        return interaction;
    }


    public bool IsReusable
    {
        get { return false; }
    }

    private static string GetDocumentContents( HttpRequest request )
    {
        string documentContents;
        using ( var receiveStream = request.InputStream )
        {
            using ( var readStream = new StreamReader( receiveStream, Encoding.UTF8 ) )
            {
                documentContents = readStream.ReadToEnd();
            }
        }
        return documentContents;
    }
}
