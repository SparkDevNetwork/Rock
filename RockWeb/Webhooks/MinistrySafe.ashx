<%@ WebHandler Language="C#" Class="com.bemaservices.Webhooks.MinistrySafe" %>

using System;
using System.Web;
using System.IO;
using System.Net;
using Rock.Model;

namespace com.bemaservices.Webhooks
{
    /// <summary>
    /// Handles the background check results sent from Checkr
    /// </summary>
    public class MinistrySafe : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ProcessRequest( HttpContext context )
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.ContentType = "text/plain";

            if ( request.HttpMethod != "POST" )
            {
                response.Write( "Invalid request type." );
                response.StatusCode = ( int ) HttpStatusCode.NotImplemented;
                return;
            }

            try
            {
                var rockContext = new Rock.Data.RockContext();

                string postedData = string.Empty;
                using ( var reader = new StreamReader( request.InputStream ) )
                {
                    postedData = reader.ReadToEnd();
                }

                if ( !com.bemaservices.MinistrySafe.MinistrySafe.SaveWebhookResults( postedData ) )
                {
                    response.StatusCode = ( int ) HttpStatusCode.OK; //If it is not ok, the website will re-direct to the error screen
                    return;
                }

                try
                {
                    response.StatusCode = ( int ) HttpStatusCode.OK;
                }
                catch { }
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex, context );
            }
        }

        /// <summary>
        /// These webhooks are not reusable and must only be used once.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}