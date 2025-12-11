<%@ WebHandler Language="C#" Class="TwilioSmsAsync" %>
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

using Rock;
using Rock.Communication;
using Rock.Communication.Medium;
using Rock.Communication.SmsActions;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;

/// <summary>
/// This the Twilio Webwook that processes incoming SMS messages thru the SMS Pipeline. See https://community.rockrms.com/Rock/BookContent/8#smstwilio
/// </summary>
public class TwilioSmsAsync : IHttpAsyncHandler
{
    public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, Object extraData )
    {
        TwilioSmsResponseAsync twilioAsync = new TwilioSmsResponseAsync( cb, context, extraData );
        twilioAsync.StartAsyncWork();
        return twilioAsync;
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

class TwilioSmsResponseAsync : TwilioDefaultResponseAsync
{
    private TransportComponent _component;

    /// <summary>
    /// Gets the Twilio transport component from the container.
    /// </summary>
    private TransportComponent TwilioTransportComponent
    {
        get
        {
            if ( _component == null )
            {
                var entityTypeGuid = new Guid( "CF9FD146-8623-4D9A-98E6-4BD710F071A4" );
                foreach ( var serviceEntry in TransportContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    var entityType = Rock.Web.Cache.EntityTypeCache.Get( component.GetType() );
                    if ( entityType != null && entityType.Guid.Equals( entityTypeGuid ) )
                    {
                        _component = component;
                        break;
                    }
                }
            }

            return _component;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioSmsResponseAsync"/> class.
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <param name="context">The context.</param>
    /// <param name="state">The state.</param>
    public TwilioSmsResponseAsync( AsyncCallback callback, HttpContext context, Object state ) : base( callback, context, state ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:TwilioDefaultResponseAsync" /> class.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="toPhone"></param>
    /// <param name="fromPhone"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public override Twilio.TwiML.Messaging.Message ProcessMessage( HttpRequest request, string toPhone, string fromPhone, string body )
    {
        var message = new SmsMessage
        {
            ToNumber = toPhone,
            FromNumber = fromPhone,
            Message = body
        };

        if ( message.ToNumber.IsNotNullOrWhiteSpace() && message.FromNumber.IsNotNullOrWhiteSpace() )
        {
            // Opt-in/opt-out tracking should be processed before anything else, to ensure we respect the sender's
            // preferences and also to ensure we remain compliant with messaging regulations.
            SmsActionService.TryUpdateOptInOutTrackingForSender( message );

            using ( var rockContext = new RockContext() )
            {
                message.FromPerson = new PersonService( rockContext ).GetPersonFromMobilePhoneNumber( message.FromNumber, true );

                var smsPipelineId = request.QueryString["smsPipelineId"].AsIntegerOrNull();

                int? numberOfAttachments = request.Params["NumMedia"].IsNotNullOrWhiteSpace() ? request.Params["NumMedia"].AsIntegerOrNull() : null;

                if ( numberOfAttachments != null && this.TwilioTransportComponent != null )
                {
                    var accountSid = this.TwilioTransportComponent.GetAttributeValue( TwilioAttributeKey.Sid );
                    var authToken = this.TwilioTransportComponent.GetAttributeValue( TwilioAttributeKey.AuthToken );

                    if ( accountSid.IsNotNullOrWhiteSpace() && authToken.IsNotNullOrWhiteSpace() )
                    {
                        Guid imageGuid;
                        for ( int i = 0; i < numberOfAttachments.Value; i++ )
                        {
                            string imageUrl = request.Params[string.Format( "MediaUrl{0}", i )];
                            string mimeType = request.Params[string.Format( "MediaContentType{0}", i )];
                            imageGuid = Guid.NewGuid();

                            var httpWebRequest = ( HttpWebRequest ) HttpWebRequest.Create( imageUrl );
                            httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String( Encoding.Default.GetBytes( accountSid + ":" + authToken ) );

                            var httpWebResponse = ( HttpWebResponse ) httpWebRequest.GetResponse();

                            if ( httpWebResponse.ContentLength == 0 )
                            {
                                continue;
                            }

                            string fileExtension = Rock.Utility.FileUtilities.GetFileExtensionFromContentType( mimeType );
                            string fileName = string.Format( "SMS-Attachment-{0}-{1}.{2}", imageGuid, i, fileExtension );
                            var stream = httpWebResponse.GetResponseStream();
                            var binaryFile = new BinaryFileService( rockContext ).AddFileFromStream( stream, mimeType, httpWebResponse.ContentLength, fileName, Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT, imageGuid );
                            message.Attachments.Add( binaryFile );
                        }
                    }
                }

                var outcomes = SmsActionService.ProcessIncomingMessage( message, smsPipelineId );
                var smsResponse = SmsActionService.GetResponseFromOutcomes( outcomes );

                if ( smsResponse == null )
                {
                    return null;
                }

                var fromPersonAliasId = message.FromPerson?.PrimaryAliasId;
                if ( fromPersonAliasId.HasValue )
                {
                    var responseCommunicationId = SmsActionService.CreateAndEnqueueResponseCommunication(
                        smsResponse,
                        fromPersonAliasId.Value,
                        message.ToNumber,
                        rockContext
                    );

                    if ( responseCommunicationId.HasValue )
                    {
                        // There's no need to send a message object back to the caller of this method since we've
                        // already queued a response to be sent.
                        return null;
                    }
                }

                var twilioMessage = new Twilio.TwiML.Messaging.Message();

                if ( smsResponse.Message.IsNotNullOrWhiteSpace() )
                {
                    twilioMessage.Body( smsResponse.Message );
                }

                if ( smsResponse.Attachments != null && smsResponse.Attachments.Any() )
                {
                    foreach ( var binaryFile in smsResponse.Attachments )
                    {
                        if ( Uri.TryCreate( binaryFile.Url, UriKind.Absolute, out var uri ) )
                        {
                            twilioMessage.Media( uri );
                        }
                    }
                }

                return twilioMessage;
            }
        }

        return null;
    }
}
