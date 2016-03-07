<%@ WebHandler Language="C#" Class="Mailgun" %>
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
using System.Data.Entity;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Newtonsoft.Json;

using Rock;
using Rock.Model;
using Rock.Workflow.Action;

public class Mailgun : IHttpHandler
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
            
            if ( !string.IsNullOrWhiteSpace( request.Form["communication_recipient_guid"] ) )
            {
                Guid? communicationRecipientGuid = request.Form["communication_recipient_guid"].AsGuidOrNull();
                if ( communicationRecipientGuid.HasValue )
                {
                    var communicationRecipientService = new CommunicationRecipientService( rockContext );
                    var communicationRecipient = communicationRecipientService.Get( communicationRecipientGuid.Value );
                    if ( communicationRecipient != null )
                    {

                        int secs = request.Form["timestamp"].AsInteger();
                        DateTime ts = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( secs ).ToLocalTime();

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
                                CommunicationRecipientActivity openActivity = new CommunicationRecipientActivity();
                                openActivity.ActivityType = "Opened";
                                openActivity.ActivityDateTime = ts;
                                openActivity.ActivityDetail = string.Format( "Opened from {0} using {1} {2} {3} {4}",
                                    request.Form["ip"] ?? "",
                                    request.Form["client-os"] ?? "",
                                    request.Form["device-type"] ?? "",
                                    request.Form["client-name"] ?? "",
                                    request.Form["client-type"] ?? "" );
                                communicationRecipient.Activities.Add( openActivity );
                                break;
                                
                            case "clicked":
                                CommunicationRecipientActivity clickActivity = new CommunicationRecipientActivity();
                                clickActivity.ActivityType = "Click";
                                clickActivity.ActivityDateTime = ts;
                                clickActivity.ActivityDetail = string.Format( "Clicked the address {0} from {1} using {2} {3} {4} {5}",
                                    request.Form["url"] ?? "",
                                    request.Form["ip"] ?? "",
                                    request.Form["client-os"] ?? "",
                                    request.Form["device-type"] ?? "",
                                    request.Form["client-name"] ?? "",
                                    request.Form["client-type"] ?? "" );
                                communicationRecipient.Activities.Add( clickActivity );
                                break;
                                
                            case "complained": break;
                            case "unsubscribed": break;
                                
                            case "dropped":
                                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                communicationRecipient.StatusNote = request.Form["description"];
                                break;
                                
                            case "bounced":
                                communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                                communicationRecipient.StatusNote = request.Form["notification"];

                                Rock.Communication.Email.ProcessBounce(
                                    request.Form["recipient"],
                                    Rock.Communication.BounceType.HardBounce,
                                    request.Form["notification"],
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
}