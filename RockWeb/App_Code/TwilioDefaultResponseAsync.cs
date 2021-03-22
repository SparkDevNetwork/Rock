﻿// <copyright>
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
using System.Linq;
using System.Threading;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using RockWeb;
using Rock.SystemKey;

public abstract class TwilioDefaultResponseAsync : IAsyncResult
{
    private bool _completed;
    private readonly Object _state;
    private readonly AsyncCallback _callback;
    private readonly HttpContext _context;

    bool IAsyncResult.IsCompleted { get { return _completed; } }
    WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
    Object IAsyncResult.AsyncState { get { return _state; } }
    bool IAsyncResult.CompletedSynchronously { get { return false; } }

    /// <summary>
    /// Gets a <see cref="T:System.Threading.WaitHandle" /> that is used to wait for an asynchronous operation to complete.
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public WaitHandle AsyncWaitHandle
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// The enable logging
    /// </summary>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioDefaultResponseAsync"/> class.
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <param name="context">The context.</param>
    /// <param name="state">The state.</param>
    public TwilioDefaultResponseAsync( AsyncCallback callback, HttpContext context, Object state )
    {
        _callback = callback;
        _context = context;
        _state = state;
        _completed = false;
    }

    /// <summary>
    /// Starts the asynchronous work.
    /// </summary>
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

    /// <summary>
    /// Starts the asynchronous task.
    /// </summary>
    /// <param name="workItemState">State of the work item.</param>
    private void StartAsyncTask( Object workItemState )
    {
        var request = _context.Request;
        var response = _context.Response;

        response.ContentType = "text/plain";

        if ( !IsValidRequest( request ) )
        {
            response.Write( "Invalid request type." );
        }
        else
        {
            TwilioLogging.HandleRequestLogging( _context, EnableLogging );

            if ( request.Form["SmsStatus"] != null )
            {
                switch ( request.Form["SmsStatus"] )
                {
                    case "received":
                        MessageReceived();
                        break;
                    case "undelivered":
                        MessageUndelivered();
                        break;
                }

                response.StatusCode = 200;
            }
            else
            {
                response.StatusCode = 500;
            }
        }

        _completed = true;
        _callback( this );
    }

    /// <summary>
    /// Messages the undelivered.
    /// </summary>
    private void MessageUndelivered()
    {
        var request = _context.Request;

        if ( request.Form["MessageSid"].IsNotNullOrWhiteSpace() )
        {
            var messageSid = request.Form["MessageSid"];

            // get communication from the message side
            using ( RockContext rockContext = new RockContext() )
            {
                CommunicationRecipientService recipientService = new CommunicationRecipientService( rockContext );

                var communicationRecipient = recipientService
                    .Queryable()
                    .Where( r => r.UniqueMessageId == messageSid )
                    .FirstOrDefault();

                if ( communicationRecipient != null )
                {
                    communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                    communicationRecipient.StatusNote = "Message failure notified from Twilio on " + RockDateTime.Now.ToString();
                    rockContext.SaveChanges();
                }
                else
                {
                    Rock.Model.ExceptionLogService.LogException( "No recipient was found with the specified MessageSid value!" );
                }
            }
        }
    }

    /// <summary>
    /// Messages the received.
    /// </summary>
    private void MessageReceived()
    {
        var request = _context.Request;

        string fromPhone = string.Empty;
        string toPhone = string.Empty;
        string body = string.Empty;

        if ( !string.IsNullOrEmpty( request.Form["To"] ) )
        {
            toPhone = request.Form["To"];
        }

        if ( !string.IsNullOrEmpty( request.Form["From"] ) )
        {
            fromPhone = request.Form["From"];
        }

        if ( !string.IsNullOrEmpty( request.Form["Body"] ) )
        {
            body = request.Form["Body"];
        }

        Twilio.TwiML.Message twilioMessage = null;
        if ( toPhone.IsNotNullOrWhiteSpace() && fromPhone.IsNotNullOrWhiteSpace() )
        {
            twilioMessage = ProcessMessage( request, toPhone, fromPhone, body );
        }

        if ( twilioMessage != null )
        {
            var messagingResponse = new Twilio.TwiML.MessagingResponse();
            messagingResponse.Message( twilioMessage );

            var response = _context.Response;
            response.ContentType = "application/xml";

            response.Write( messagingResponse.ToString() );
        }
    }

    private bool IsValidRequest( HttpRequest request )
    {
        if ( request.HttpMethod != "POST" )
        {
            if ( EnableLogging )
            {
                TwilioLogging.WriteRequestToTwilioLog( _context, "Request was not a post." );
            }
            return false;
        }

        // TODO Only check if enabled in TwilioComponent config
        // otherwise return true;
        var twilioComponent = new Rock.Communication.Transport.Twilio();
        twilioComponent.LoadAttributes();

        if ( twilioComponent.AttributeValues.ContainsKey( TwilioAttributeKey.EnableValidation ) )
        {
            var continueValidation = twilioComponent.AttributeValues[TwilioAttributeKey.EnableValidation].Value.AsBoolean();
            if ( !continueValidation )
            {
                return true;
            }
        }

        var signature = request.Headers["X-Twilio-Signature"];
        if ( signature.IsNullOrWhiteSpace() )
        {
            if ( EnableLogging )
            {
                TwilioLogging.WriteRequestToTwilioLog( _context, "X-Twilio-Signature not found." );
            }

            return false;
        }

        var authToken = string.Empty;
        if ( twilioComponent.AttributeValues.ContainsKey( TwilioAttributeKey.AuthToken ) )
        {
            authToken = twilioComponent.AttributeValues[TwilioAttributeKey.AuthToken].Value;
        }

        if ( authToken.IsNullOrWhiteSpace() )
        {
            if ( EnableLogging )
            {
                TwilioLogging.WriteRequestToTwilioLog( _context, "Not auth token found." );
            }
            return false;
        }

        var requestValidator = new Twilio.Security.RequestValidator( authToken );
        var isValid = requestValidator.Validate( request.Url.AbsoluteUri, request.Form, signature );
        if ( isValid )
        {
            return isValid;
        }

        // Build public URL and append the given request's
        // path and query (e.g., /Webhooks/TwilioSms.ashx?SmsPipelineId=2)
        var requestUrl = GlobalAttributesCache
            .Get()
            .GetValue( "PublicApplicationRoot" ) + request.Url.PathAndQuery.RemoveLeadingForwardslash();

        isValid = requestValidator.Validate( requestUrl, request.Form, signature );

        if ( !isValid && EnableLogging )
        {
            TwilioLogging.WriteRequestToTwilioLog( _context, "Authentication Failed: request.Url.AbsoluteUri: " + request.Url.AbsoluteUri + " requestUrl: " + requestUrl + " authToken: " + authToken );
        }

        return isValid;
    }

    /// <summary>
    /// Processes the message.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="toPhone">To phone.</param>
    /// <param name="fromPhone">From phone.</param>
    /// <param name="body">The body.</param>
    /// <returns></returns>
    public abstract Twilio.TwiML.Message ProcessMessage( HttpRequest request, string toPhone, string fromPhone, string body );
}
