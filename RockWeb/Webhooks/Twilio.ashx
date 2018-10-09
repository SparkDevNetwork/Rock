<%@ WebHandler Language="C#" Class="TwilioAsync" %>
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
using System.Threading;
using Newtonsoft.Json;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

public class TwilioAsync : IHttpAsyncHandler
{
    public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)
    {
        TwilioResponseAsync twilioAsync = new TwilioResponseAsync(cb, context, extraData);
        twilioAsync.StartAsyncWork();
        return twilioAsync;
    }

    public void EndProcessRequest(IAsyncResult result)
    {
    }

    public void ProcessRequest(HttpContext context)
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

class TwilioResponseAsync : IAsyncResult
{
    private bool _completed;
    private Object _state;
    private AsyncCallback _callback;
    private HttpContext _context;

    bool IAsyncResult.IsCompleted { get { return _completed; } }
    WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
    Object IAsyncResult.AsyncState { get { return _state; } }
    bool IAsyncResult.CompletedSynchronously { get { return false; } }

    private const bool ENABLE_LOGGING = false;

    public TwilioResponseAsync(AsyncCallback callback, HttpContext context, Object state)
    {
        _callback = callback;
        _context = context;
        _state = state;
        _completed = false;
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

    private void StartAsyncTask(Object workItemState)
    {
        var request = _context.Request;
        var response = _context.Response;

        response.ContentType = "text/plain";

        if (request.HttpMethod != "POST")
        {
            response.Write("Invalid request type.");
        }
        else
        {

            // determine if we should log
            if ((!string.IsNullOrEmpty(request.QueryString["Log"]) && request.QueryString["Log"] == "true") || ENABLE_LOGGING)
            {
                WriteToLog();
            }

            if (request.Form["SmsStatus"] != null)
            {
                switch (request.Form["SmsStatus"])
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
        _callback(this);
    }

    private void MessageUndelivered()
    {

        var request = _context.Request;
        string messageSid = string.Empty;

        if ( !string.IsNullOrEmpty( request.Form["MessageSid"] ) )
        {
            messageSid = request.Form["MessageSid"];

            // get communication from the message side
            RockContext rockContext = new RockContext();
            CommunicationRecipientService recipientService = new CommunicationRecipientService(rockContext);

            var communicationRecipient = recipientService.Queryable().Where( r => r.UniqueMessageId == messageSid ).FirstOrDefault();
            if ( communicationRecipient != null )
            {
                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                communicationRecipient.StatusNote = "Message failure notified from Twilio on " + RockDateTime.Now.ToString();
                rockContext.SaveChanges();
            }
            else
            {
                WriteToLog( "No recipient was found with the specified MessageSid value!" );
            }
        }
    }

    private void MessageReceived()
    {

        var request = _context.Request;
        var response = _context.Response;
        string fromPhone = string.Empty;
        string toPhone = string.Empty;
        string body = string.Empty;

        if ( !string.IsNullOrEmpty( request.Form["To"] ) ) {
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

        if ( !(string.IsNullOrWhiteSpace(toPhone)) && !(string.IsNullOrWhiteSpace(fromPhone)) )
        {
            string errorMessage = string.Empty;

            new Rock.Communication.Medium.Sms().ProcessResponse( toPhone, fromPhone, body, out errorMessage );

            if ( errorMessage != string.Empty )
            {
                response.Write( errorMessage );
            }
        }
    }

    private void WriteToLog ()
    {
        var request = _context.Request;
        var formValues = new List<string>();
        foreach ( string name in request.Form.AllKeys )
        {
            formValues.Add( string.Format( "{0}: '{1}'", name, request.Form[name] ) );
        }

        WriteToLog( formValues.AsDelimited( ", " ) );
    }

    private void WriteToLog( string message )
    {
        string logFile = _context.Server.MapPath( "~/App_Data/Logs/TwilioLog.txt" );

        // Write to the log, but if an ioexception occurs wait a couple seconds and then try again (up to 3 times).
        var maxRetry = 3;
        for ( int retry = 0; retry < maxRetry; retry++ )
        {
            try
            {
                using ( System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ) )
                {
                    using ( System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ) )
                    {
                        sw.WriteLine( string.Format( "{0} - {1}", RockDateTime.Now.ToString(), message ) );
                    }
                }
            }
            catch ( System.IO.IOException )
            {
                if ( retry < maxRetry - 1 )
                {
                    System.Threading.Tasks.Task.Delay( 2000 ).Wait();
                }
            }
        }

    }

}
