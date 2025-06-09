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
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

using Microsoft.AspNetCore.Mvc;

using Rock.Cms;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

#if WEBFORMS
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
#endif

namespace Rock.Rest.v2
{
    /// <summary>
    /// Provides the API interfaces for executing Lava Applications.
    /// </summary>
    [RoutePrefix( "api/v2/lava-app" )]
    [Rock.SystemGuid.RestControllerGuid( "8af769e9-972c-4f40-8344-89ff4b07fcbd" )]
    public class LavaAppController : ApiControllerBase
    {
        #region Keys

        /// <summary>
        /// Custom header keys used by Lava Applications.
        /// </summary>
        private static class HeaderKeys
        {
            public const string CrossSiteForgeryFlag = "X-Helix-CSRF-Protection";
        }

        #endregion

        #region REST Methods

        /// <summary>
        /// Executes a Lava Application Endpoint and returns the results.
        /// </summary>
        /// <param name="applicationSlug">The slug that identifies the Lava Application.</param>
        /// <param name="endpointSlug">The slug that identifies the Endpoint in the Lava Application.</param>
        /// <returns>The response from executing the Lava defined on the endpoint.</returns>
        [HttpGet]
        [Route( "1/{applicationSlug}/{*endpointSlug?}" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [Rock.SystemGuid.RestActionGuid( "0d1184d4-20e4-4e23-a5eb-47cab3f59063" )]
        public IActionResult GetEndpoint( string applicationSlug, string endpointSlug )
        {
            return ProcessEndpoint( applicationSlug, endpointSlug );
        }

        /// <summary>
        /// Executes a Lava Application Endpoint and returns the results.
        /// </summary>
        /// <param name="applicationSlug">The slug that identifies the Lava Application.</param>
        /// <param name="endpointSlug">The slug that identifies the Endpoint in the Lava Application.</param>
        /// <returns>The response from executing the Lava defined on the endpoint.</returns>
        [HttpHead]
        [Route( "1/{applicationSlug}/{*endpointSlug?}" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [Rock.SystemGuid.RestActionGuid( "ec8b1359-82f6-48a8-bca2-2efdcfa7934d" )]
        public IActionResult HeadEndpoint( string applicationSlug, string endpointSlug )
        {
            return ProcessEndpoint( applicationSlug, endpointSlug );
        }

        /// <summary>
        /// Executes a Lava Application Endpoint and returns the results.
        /// </summary>
        /// <param name="applicationSlug">The slug that identifies the Lava Application.</param>
        /// <param name="endpointSlug">The slug that identifies the Endpoint in the Lava Application.</param>
        /// <returns>The response from executing the Lava defined on the endpoint.</returns>
        [HttpPost]
        [Route( "1/{applicationSlug}/{*endpointSlug?}" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [Rock.SystemGuid.RestActionGuid( "f3462fca-151f-4c1e-b4d3-eeb9e4e3f1d4" )]
        public IActionResult PostEndpoint( string applicationSlug, string endpointSlug )
        {
            return ProcessEndpoint( applicationSlug, endpointSlug );
        }

        /// <summary>
        /// Executes a Lava Application Endpoint and returns the results.
        /// </summary>
        /// <param name="applicationSlug">The slug that identifies the Lava Application.</param>
        /// <param name="endpointSlug">The slug that identifies the Endpoint in the Lava Application.</param>
        /// <returns>The response from executing the Lava defined on the endpoint.</returns>
        [HttpPut]
        [Route( "1/{applicationSlug}/{*endpointSlug?}" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [Rock.SystemGuid.RestActionGuid( "10ef48cd-9a38-426d-a9f3-7af442e8b713" )]
        public IActionResult PutEndpoint( string applicationSlug, string endpointSlug )
        {
            return ProcessEndpoint( applicationSlug, endpointSlug );
        }

        #endregion

        #region Processing Logic

        /// <summary>
        /// Processes any type of request to a Lava endpoint.
        /// </summary>
        /// <param name="applicationSlug"></param>
        /// <param name="endpointSlug"></param>
        /// <returns></returns>
        private IActionResult ProcessEndpoint( string applicationSlug, string endpointSlug )
        {
            var context = ProcessEndpointRequest( applicationSlug, endpointSlug );

            // Create response
            HttpResponseMessage responseMessage = new HttpResponseMessage( context.EndpointResponse.ResponseStatus );
            if ( context.EndpointResponse.Content != null )
            {
                responseMessage.Content = new StringContent( context.EndpointResponse.Content, Encoding.UTF8, "text/html" );
            }

            // Add caching headers
            var cacheHeader = context.LavaEndpoint?.CacheControlHeader.ToStringSafe();

            if ( cacheHeader.IsNotNullOrWhiteSpace() )
            {
                responseMessage.Headers.Add( "Cache-Control", cacheHeader );
            }

            // Return results
            return ResponseMessage( responseMessage );
        }

        /// <summary>
        /// Method that processed the endpoint logic.
        /// </summary>
        /// <param name="applicationSlug"></param>
        /// <param name="endpointSlug"></param>
        /// <returns></returns>
        private EndpointExecutionContext ProcessEndpointRequest( string applicationSlug, string endpointSlug )
        {
            var context = new EndpointExecutionContext( applicationSlug, endpointSlug, RockRequestContext.CurrentPerson );

            // Get Application and Endpoints
            if ( !GetApplicationEndpoint( context ) )
            {
                return context;
            }

            // Check Cross-site Forgery Protection
            if ( !CheckCrossSiteForgery( context ) )
            {
                return context;
            }

            // Check Authorization
            if ( !IsAuthorized( context ) )
            {
                return context;
            }

            // Setup observability
            SetupObservability( context );

            // Merge Request
            MergeRequest( context );

            // Done!
            return context;
        }

        /// <summary>
        /// Determines if the request passes the cross-site forgery protection check.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool CheckCrossSiteForgery( EndpointExecutionContext context )
        {
            if ( context.LavaEndpoint.EnableCrossSiteForgeryProtection == false )
            {
                return true;
            }

            if ( HttpContext.Current.Request.Headers[HeaderKeys.CrossSiteForgeryFlag] == null
                    || HttpContext.Current.Request.Headers[HeaderKeys.CrossSiteForgeryFlag].AsBoolean() != true )
            {
                context.EndpointResponse.ResponseStatus = HttpStatusCode.Unauthorized;
                context.EndpointResponse.Message = "CSRF token is missing. Please ensure your request includes the appropriate security headers.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the current person is allowed to use this endpoint.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool IsAuthorized( EndpointExecutionContext context )
        {
            var isAuthorized = context.LavaEndpoint.IsAuthorized( "Execute", context.CurrentPerson );

            if ( !isAuthorized )
            {
                context.EndpointResponse.ResponseStatus = HttpStatusCode.Unauthorized;
                context.EndpointResponse.Message = "Current user is not authorized to use this endpoint.";
                return false;
            }

            return isAuthorized;
        }

        /// <summary>
        /// Get's the Lava application and endpoint based on the provided slugs.
        /// </summary>
        /// <param name="context"></param>
        private bool GetApplicationEndpoint( EndpointExecutionContext context )
        {
            context.LavaApplication = LavaApplicationCache.GetBySlug( context.ApplicationSlug );

            // Get Lava Application
            if ( context.LavaApplication == null || !context.LavaApplication.IsActive )
            {
                context.EndpointResponse.ResponseStatus = HttpStatusCode.NotFound;
                context.EndpointResponse.Message = $"Could not find Lava Application with the slug {context.ApplicationSlug}";
                return false;
            }

            // Get Lava Endpoint
            context.LavaEndpoint = context.LavaApplication.GetEndpoint( context.EndpointSlug, LavaEndpoint.GetHttpMethodFromRequestMethod( Request.Method ) );

            if ( context.LavaEndpoint == null || !context.LavaEndpoint.IsActive )
            {
                context.EndpointResponse.ResponseStatus = HttpStatusCode.NotFound;
                context.EndpointResponse.Message = $"Could not find Lava Endpoint with the slug {context.EndpointSlug}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Merges the lava template of the endpoint
        /// </summary>
        /// <param name="context"></param>
        private void MergeRequest( EndpointExecutionContext context )
        {
            var mergeFields = LavaApplicationRequestHelpers.RequestToDictionary( HttpContext.Current.Request, context.CurrentPerson );
            mergeFields.Add( "ConfigurationRigging", context.LavaApplication.ConfigurationRigging );

            var content = string.Empty;

            try
            {
                content = context.LavaEndpoint.CodeTemplate.ResolveMergeFields(
                        mergeFields,
                        context.CurrentPerson,
                        context.LavaEndpoint.EnabledLavaCommands ).Trim();
            }
            catch ( Exception ) { }

            // Check for a HTTP status code in the Lava
            if ( HttpContext.Current?.Response.StatusCode != 200 )
            {
                content = $"Endpoint returned status of {HttpContext.Current?.Response.StatusCode}.";
                context.EndpointResponse.ResponseStatus = ( HttpStatusCode ) HttpContext.Current?.Response.StatusCode;
            }

            context.EndpointResponse.Content = content;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Adds information to the observability activity.
        /// </summary>
        /// <param name="context"></param>
        private void SetupObservability( EndpointExecutionContext context )
        {
            var rootActivity = GetRootActivity( Activity.Current );

            if ( rootActivity is null )
            {
                return;
            }

            rootActivity.DisplayName = $"LavaEndpoint: {context.LavaEndpoint.Name} | {context.LavaEndpoint.LavaApplication.Name}";
            rootActivity.AddTag( "rock.lava_endpoint", context.LavaEndpoint.Name );
            rootActivity.AddTag( "rock.lava_application", context.LavaApplication.Name );
        }

        /// <summary>
        /// This is a temporary solution until ObservabilityHelper.GetRootActivity can be marked as public.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private Activity GetRootActivity( Activity activity )
        {
            if ( activity == null )
            {
                return null;
            }

            while ( activity.Parent != null )
            {
                activity = activity.Parent;
            }

            return activity;
        }

        #endregion

        #region Support Classes

        private class EndpointExecutionContext
        {
            #region Constructors

            internal EndpointExecutionContext( string applicationSlug, string endpointSlug, Person currentPerson )
            {
                ApplicationSlug = applicationSlug;
                EndpointSlug = endpointSlug;
                CurrentPerson = currentPerson;
            }

            #endregion

            #region Properties

            public string ApplicationSlug { get; set; }

            public string EndpointSlug { get; set; }

            public LavaApplicationCache LavaApplication { get; set; }

            public LavaEndpointCache LavaEndpoint { get; set; }

            public Person CurrentPerson { get; set; }

            public EndpointResponse EndpointResponse { get; set; } = new EndpointResponse();

            #endregion
        }

        private class EndpointResponse
        {
            #region Properties

            public string Message { get; set; }

            public HttpStatusCode ResponseStatus { get; set; } = HttpStatusCode.OK;

            public string Content { get; set; }

            #endregion
        }

        #endregion
    }
}
