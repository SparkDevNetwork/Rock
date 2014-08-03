<%@ WebHandler Language="C#" Class="Twilio" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Rock;
using Rock.Model;
using Rock.Web.Cache;

public class Twilio : IHttpHandler
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
            return;
        }


        if ( request.Form["SmsStatus"] != null )
        {
            switch ( request.Form["SmsStatus"] )
            {
                case "received":
                    MessageRecieved();
                    break;
            }

            response.StatusCode = 200;
        }
        else
        {
            response.StatusCode = 500;
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

            new Rock.Communication.Channel.Sms().ProcessResponse( toPhone, fromPhone, body, out errorMessage );

            if ( errorMessage != string.Empty )
            {
                response.Write( errorMessage );
            }
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