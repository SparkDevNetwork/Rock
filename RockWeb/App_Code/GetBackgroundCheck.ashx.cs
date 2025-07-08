// <copyright>
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
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using Rock;
using Rock.Web.Cache;
using Rock.Model;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving Background Check report
    /// </summary>
    public class GetBackgroundCheck : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ProcessRequest( HttpContext context )
        {
            try
            {
                var entityTypeGuid = context.Request.QueryString["EntityTypeGuid"].AsGuidOrNull();
                int entityTypeId = context.Request.QueryString["EntityTypeId"].AsInteger();
                string recordKey = context.Request.QueryString["RecordKey"];

                if ( ( entityTypeId == 0 && entityTypeGuid == null ) || recordKey.IsNullOrWhiteSpace() )
                {
                    throw new Exception( "Missing or invalid EntityTypeId/Guid or RecordKey" );
                }

                Type backgroundCheckComponentType;
                if ( entityTypeGuid.HasValue )
                {
                    backgroundCheckComponentType = Type.GetType( EntityTypeCache.Get( entityTypeGuid.Value ).AssemblyName );
                }
                else
                {
                    backgroundCheckComponentType = Type.GetType( EntityTypeCache.Get( entityTypeId ).AssemblyName );
                }

                if ( backgroundCheckComponentType != null )
                {
                    MethodInfo methodInfo = backgroundCheckComponentType.GetMethod( "GetReportUrl" );

                    if ( methodInfo != null )
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        object classInstance = Activator.CreateInstance( backgroundCheckComponentType, null );

                        object[] parametersArray = new object[] { recordKey };

                        // The invoke does NOT work;
                        // it throws "Object does not match target type"             
                        string url = methodInfo.Invoke( classInstance, parametersArray ).ToStringSafe();
                        if ( url.IsNotNullOrWhiteSpace() )
                        {
                            try
                            {
                                if ( url == "Unauthorized" )
                                {
                                    context.Response.StatusCode = ( int ) HttpStatusCode.Unauthorized;
                                    return;
                                }
                                else
                                {
                                    context.Response.Redirect( url, false );
                                    context.ApplicationInstance.CompleteRequest(); // https://blogs.msdn.microsoft.com/tmarq/2009/06/25/correct-use-of-system-web-httpresponse-redirect/
                                    return;
                                }
                            }
                            catch ( ThreadAbortException )
                            {
                                // Can safely ignore this exception
                            }
                        }
                        else
                        {
                            SendError( context, 500, "The underlying component was unable to retrieve the requested item.  Additional details can be found in the exception log." );
                        }
                    }
                }

                try
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return;
                }
                catch { }
            }
            catch ( ThreadAbortException )
            {
                // Can safely ignore this exception
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Sends an error code response and completes the request.
        /// </summary>
        /// <param name="context">THe HttpContext for this request.</param>
        /// <param name="code">The response code to send.</param>
        /// <param name="message">The response message to send.</param>
        private void SendError( HttpContext context, int code, string message )
        {
            context.Response.Clear();
            context.Response.StatusCode = code;
            context.Response.StatusDescription = message;
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Sends a 403 (forbidden)
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendNotAuthorized( HttpContext context )
        {
            try
            {
                context.Response.StatusCode = System.Net.HttpStatusCode.Forbidden.ConvertToInt();
                context.Response.StatusDescription = "Not authorized to view file";
                context.ApplicationInstance.CompleteRequest();
            }
            catch ( ThreadAbortException )
            {
                // Can safely ignore this exception
            }
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}