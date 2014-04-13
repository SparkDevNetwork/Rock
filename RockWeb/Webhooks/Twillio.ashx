<%@ WebHandler Language="C#" Class="Twillio" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Rock.Model;

public class Twillio : IHttpHandler
{
    private HttpRequest request;
    private HttpResponse response;
    private int transactionCount = 0;

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

        switch ( request.Form["SmsStatus"] )
        {
            case "received":
                MessageRecieved();
                break;
        }
        
        
        response.Write( String.Format( "Success: Processed {0} transactions.", transactionCount.ToString() ) );

        // must do this or Mandrill will not accept your webhook!
        response.StatusCode = 200;

    }

    private void MessageRecieved()
    {
        var rockContext = new Rock.Data.RockContext();
        CommunicationRecipientService communicationRecipientService = new CommunicationRecipientService( rockContext );

        string messageId = request.Form["MessageSid"];
        
        //var communicationRecipient = communicationRecipientService.Get( communicationRecipientGuid );
         
    }
    
    
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}