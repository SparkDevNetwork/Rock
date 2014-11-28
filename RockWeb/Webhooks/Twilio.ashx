﻿<%@ WebHandler Language="C#" Class="Twilio" %>
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

            new Rock.Communication.Medium.Sms().ProcessResponse( toPhone, fromPhone, body, out errorMessage );

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