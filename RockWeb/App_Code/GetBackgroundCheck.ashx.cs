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
        /// Hard-coded copy of the legacy Protect My Ministry (v1) EntityType Guid. The
        /// PMM v1 component was removed in Rock v20 (see the SunsetProtectMyMinistry
        /// plugin migration), so we cannot reach the Rock.SystemGuid.EntityType
        /// constant of the same name (it is <c>internal</c> to the Rock assembly).
        /// Historical background-check <c>[AttributeValue]</c> rows stored just a
        /// BinaryFile Guid under this provider, so when we see this EntityTypeGuid on
        /// the URL we stream the BinaryFile by Guid instead of trying to instantiate
        /// the (now-removed) component class.
        /// </summary>
        private static readonly Guid ProtectMyMinistryLegacyProviderGuid =
            new Guid( "C16856F4-3C6B-4AFB-A0B8-88A303508206" );

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

                // Legacy Protect My Ministry (v1) documents were stored with just a BinaryFile Guid
                // and the PMM component was removed in Rock v20, so it is no longer possible to
                // reflect over the (now-missing) type. Redirect straight to GetFile.ashx to stream
                // the BinaryFile by Guid.
                if ( entityTypeGuid.HasValue && entityTypeGuid.Value == ProtectMyMinistryLegacyProviderGuid )
                {
                    if ( !Guid.TryParse( recordKey, out Guid binaryFileGuid ) )
                    {
                        throw new Exception( "Missing or invalid BinaryFile Guid for a legacy Protect My Ministry background check document." );
                    }

                    /*
                        7/13/26 - NA

                        PMM v1's own GetReportUrl gated on this.IsAuthorized( VIEW,
                        currentPerson ) before returning the URL. That check is gone
                        with the component. The closest surviving equivalent is to
                        gate on the CURRENTLY-active background check component's VIEW
                        auth: if a Rock admin has configured Checkr (or another
                        provider) with per-person VIEW permissions, we honor those for
                        legacy PMM documents too. If no provider is currently active,
                        fall through to GetFile.ashx which still enforces the
                        BinaryFile / BinaryFileType-level auth as a safety net.

                        Reason: Legacy background check reports security checks should not be
                                weakened relative to what PMM originally required.
                    */
                    var activeComponent = Rock.Security.BackgroundCheckContainer.GetActiveComponent();
                    if ( activeComponent != null )
                    {
                        using ( var rockContext = new Rock.Data.RockContext() )
                        {
                            var currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
                            var currentPerson = currentUser?.Person;

                            if ( !activeComponent.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                            {
                                context.Response.StatusCode = ( int ) HttpStatusCode.Unauthorized;
                                context.ApplicationInstance.CompleteRequest();
                                return;
                            }
                        }
                    }

                    var legacyFileUrl = System.Web.VirtualPathUtility.ToAbsolute( "~/GetFile.ashx" )
                        + "?guid=" + binaryFileGuid.ToString();
                    context.Response.Redirect( legacyFileUrl, false );
                    context.ApplicationInstance.CompleteRequest();
                    return;
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