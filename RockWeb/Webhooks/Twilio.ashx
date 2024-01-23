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
using RockWeb;
using Rock.Communication;

/// <summary>
/// This the Twilio Webwook that updates the communication recipient record to indicate the message status, and runs any Workflow configured with the SMS Phone Number that the message was from.
/// </summary>
public class TwilioAsync : IHttpAsyncHandler
{
    public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, Object extraData )
    {
        TwilioResponseAsync twilioAsync = new TwilioResponseAsync( cb, context, extraData );
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

class TwilioResponseAsync : TwilioDefaultResponseAsync
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioResponseAsync"/> class.
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <param name="context">The context.</param>
    /// <param name="state">The state.</param>
    public TwilioResponseAsync( AsyncCallback callback, HttpContext context, Object state ) : base( callback, context, state ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:TwilioDefaultResponseAsync" /> class.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="toPhone"></param>
    /// <param name="fromPhone"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public override Twilio.TwiML.Message ProcessMessage( HttpRequest request, string toPhone, string fromPhone, string body )
    {
        string errorMessage;

        var medium = CommunicationServicesHost.GetCommunicationMediumSms();
        if ( medium != null )
        {
            medium.ProcessResponse( toPhone, fromPhone, body, out errorMessage );
        }
        else
        {
            errorMessage = "SMS Medium not available.";
        }

        if ( errorMessage.IsNullOrWhiteSpace() )
        {
            return null;
        }

        var twilioMessage = new Twilio.TwiML.Message();
        twilioMessage.Body( errorMessage );
        return twilioMessage;
    }
}