<%@ WebHandler Language="C#" Class="Twilio" %>
// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

public class Twilio : IHttpHandler
{    
    private HttpRequest request;
    private HttpResponse response;

    private const bool ENABLE_LOGGING = true;
    
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

        // determine if we should log
        if ( ( !string.IsNullOrEmpty( request.QueryString["Log"] ) && request.QueryString["Log"] == "true" ) || ENABLE_LOGGING )
        {
            WriteToLog();
        }

        if ( request.Form["SmsStatus"] != null )
        {
            switch ( request.Form["SmsStatus"] )
            {
                case "received":
                    MessageRecieved();
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

    private void MessageUndelivered()
    {
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
    
    private void MessageRecieved()
    {
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
        var formValues = new List<string>();
        foreach ( string name in request.Form.AllKeys )
        {
            formValues.Add( string.Format( "{0}: '{1}'", name, request.Form[name] ) );
        }

        WriteToLog( formValues.AsDelimited( ", " ) );
    }
    
    private void WriteToLog( string message )
    {
        string logFile = HttpContext.Current.Server.MapPath( "~/App_Data/Logs/TwilioLog.txt" );
        using ( System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ) )
        using ( System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ) )
        {
            sw.WriteLine( string.Format( "{0} - {1}", RockDateTime.Now.ToString(), message ) );
        }
    }
    
    
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}