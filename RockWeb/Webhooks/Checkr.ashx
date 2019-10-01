<%@ WebHandler Language="C#" Class="RockWeb.Webhooks.Checkr" %>
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
using System.IO;
using System.Net;
using Rock.Model;

namespace RockWeb.Webhooks
{
    /// <summary>
    /// Handles the background check results sent from Checkr
    /// </summary>
    public class Checkr : IHttpHandler
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
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
            }

            try
            {
                var rockContext = new Rock.Data.RockContext();

                if ( !request.UserAgent.StartsWith( "Checkr-Webhook/" ) )
                {
                    response.Write( "Invalid User-Agent." );
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }

                string postedData = string.Empty;
                using ( var reader = new StreamReader( request.InputStream ) )
                {
                    postedData = reader.ReadToEnd();
                }

                Rock.Checkr.Checkr.SaveWebhookResults( postedData );

                // Per Gerhard this has to be set to 200 regardless of the result or the page will not function.
                response.StatusCode = (int)HttpStatusCode.OK;
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