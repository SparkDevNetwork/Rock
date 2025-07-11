﻿// <copyright>
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Utility.CaptchaApi;
using Rock.ViewModels.Blocks;
using Rock.Web.Cache;
using Rock.Security;


#if WEBFORMS
using BadRequestErrorMessageResult = System.Web.Http.Results.BadRequestErrorMessageResult;
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using NakedBodyAttribute = System.Web.Http.NakedBodyAttribute;
using NotFoundResult = System.Web.Http.Results.NotFoundResult;
using OkResult = System.Web.Http.Results.OkResult;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using StatusCodeResult = System.Web.Http.Results.StatusCodeResult;
#endif

namespace Rock.Rest.v2
{
    /// <summary>
    /// API controller for the /api/v2/BlockActions endpoints.
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    [RoutePrefix( "api/v2/blockactions" )]
    [Rock.SystemGuid.RestControllerGuid( "31D6B6FC-7740-483A-81D2-D62283F67C0A")]
    public class BlockActionsController : ApiControllerBase, BlockActionsController.IBlockActionsController
    {
        internal interface IBlockActionsController
        {

        }
        #region Fields

        /// <summary>
        /// The service provider for this instance.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockActionsController"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for this instance.</param>
        public BlockActionsController( IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region API Methods

        /// <summary>
        /// Executes an action handler on a specific block.
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <returns></returns>
        [HttpGet]
        [Route( "{pageGuid:guid}/{blockGuid:guid}/{actionName}" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [Rock.SystemGuid.RestActionGuid( "CC3DE0C2-8703-4925-A16C-F47A31FE9C69" )]
        public async Task<IActionResult> BlockAction( Guid pageGuid, Guid blockGuid, string actionName )
        {
            return await ProcessAction( this, pageGuid, blockGuid, actionName, null, _serviceProvider );
        }

        /// <summary>
        /// Executes an action handler on a specific block.
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [HttpPost]
        [Route( "{pageGuid:guid}/{blockGuid:guid}/{actionName}" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [Rock.SystemGuid.RestActionGuid( "05EAF919-0D36-496E-8924-88DC50A9CD8E" )]
        public async Task<IActionResult> BlockActionAsPost( Guid pageGuid, Guid blockGuid, string actionName, [NakedBody] string parameters )
        {
            if ( parameters == string.Empty )
            {
                return await ProcessAction( this, pageGuid, blockGuid, actionName, null, _serviceProvider );
            }

            //
            // We have to manually parse the JSON data, otherwise any strings
            // that look like dates get converted to Date objects. This causes
            // problems because then when we later stuff that Date object into
            // an actual string, the format has been changed. This happens, for
            // example, with Attribute Values.
            //
            using ( var stringReader = new StringReader( parameters ) )
            {
                using ( var jsonReader = new JsonTextReader( stringReader ) { DateParseHandling = DateParseHandling.None } )
                {
                    var parameterToken = JToken.ReadFrom( jsonReader );

                    return await ProcessAction( this, pageGuid, blockGuid, actionName, parameterToken, _serviceProvider );
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the action.
        /// </summary>
        /// <param name="controller">The API controller that initiated this action.</param>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="serviceProvider">The provider of the services that will be used to process the action.</param>
        /// <returns>The result of the block action method.</returns>
        internal static async Task<IActionResult> ProcessAction( ApiControllerBase controller, Guid? pageGuid, Guid? blockGuid, string actionName, JToken parameters, IServiceProvider serviceProvider )
        {
            try
            {
                BlockCache blockCache = null;
                PageCache pageCache = null;

                //
                // Find the block.
                //
                if ( blockGuid.HasValue )
                {
                    blockCache = BlockCache.Get( blockGuid.Value );
                }

                if ( blockCache == null )
                {
                    return new NotFoundResult( controller );
                }

                // Find the page.
                if ( pageGuid.HasValue )
                {
                    pageCache = PageCache.Get( pageGuid.Value );
                }
                else
                {
                    // This can be removed once the obsolete API endpoints
                    // that allowed for sending an action to a block identifier
                    // without a page identifier are removed.
                    pageCache = blockCache.Page;
                }

                if ( blockCache == null || pageCache == null )
                {
                    return new NotFoundResult( controller );
                }

                if ( controller.RockRequestContext?.IsClientForbidden( pageCache ) == true )
                {
                    return new StatusCodeResult( HttpStatusCode.Forbidden, controller );
                }

                //
                // Get the authenticated person and make sure it's cached.
                //
                var person = GetPerson( controller, null );
#if WEBFORMS
                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", person );
#else
#error Not implemented yet.
#endif

                // Ensure the user has access to both the page and block. For
                // block permissions, we accept VIEW, EDIT, or ADMINISTRATE.
                // This is done on purpose so that we match the behavior of the
                // page rendering, which does the same.
                var canViewBlock = blockCache.IsAuthorized( Security.Authorization.VIEW, person )
                    || blockCache.IsAuthorized( Security.Authorization.EDIT, person )
                    || blockCache.IsAuthorized( Security.Authorization.ADMINISTRATE, person );

                if ( !pageCache.IsAuthorized( Security.Authorization.VIEW, person ) || !canViewBlock )
                {
                    return new StatusCodeResult( HttpStatusCode.Unauthorized, controller );
                }

                // Check if we need to apply rate limiting to this request.
                if ( pageCache.IsRateLimited )
                {
                    var canProcess = RateLimiterCache.CanProcessPage( pageCache.Id,
                        controller.RockRequestContext.ClientInformation.IpAddress,
                        TimeSpan.FromSeconds( pageCache.RateLimitPeriodDurationSeconds.Value ),
                        pageCache.RateLimitRequestPerPeriod.Value );

                    if ( !canProcess )
                    {
                        return new StatusCodeResult( ( HttpStatusCode ) 429, controller );
                    }
                }

                //
                // Get the class that handles the logic for the block.
                //
                var blockCompiledType = blockCache.BlockType.GetCompiledType();
                var block = ActivatorUtilities.CreateInstance( serviceProvider, blockCompiledType );

                if ( !( block is Blocks.IRockBlockType rockBlock ) )
                {
                    return new NotFoundResult( controller );
                }

                var requestContext = controller.RockRequestContext;

                //
                // Set the basic block parameters.
                //
                rockBlock.BlockCache = blockCache;
                rockBlock.PageCache = pageCache;
                rockBlock.RequestContext = requestContext;

                if ( rockBlock is RockBlockType rockBlockType )
                {
                    rockBlockType.RockContext = serviceProvider.GetRequiredService<RockContext>();
                }

                var actionParameters = new Dictionary<string, JToken>( StringComparer.InvariantCultureIgnoreCase );

                //
                // Parse any posted parameter data.
                //
                if ( parameters != null )
                {
                    try
                    {
                        foreach ( var kvp in parameters.ToObject<Dictionary<string, JToken>>() )
                        {
                            if ( kvp.Key == "__context" )
                            {
                                var actionContext = kvp.Value.ToObject<BlockActionContextBag>();

                                // If we are given any page parameters then
                                // override the query string parameters. This
                                // is what allows mobile and obsidian blocks to
                                // pass in the original page parameters.
                                if ( actionContext?.PageParameters != null )
                                {
                                    rockBlock.RequestContext.SetPageParameters( actionContext.PageParameters );
                                }

                                // Restore the interaction session if we have one.
                                if ( actionContext?.SessionGuid != null )
                                {
                                    requestContext.SessionGuid = actionContext.SessionGuid.Value;
                                }

                                if ( ( actionContext?.InteractionGuid ).HasValue )
                                {
                                    requestContext.RelatedInteractionGuid = actionContext.InteractionGuid.Value;
                                }

                                /*
                                    02/22/2024 - JSC

                                    It's important that we perform the captcha
                                    validation even when the actionContext.Captcha
                                    is whitespace. Null indicates the captcha is
                                    not in use by the block, but an empty string
                                    indicates that the global configuration is missing.
                                    In the latter case we still need to return true
                                    so captcha doesn't fail when it's not configured.
                                */
                                if ( actionContext?.Captcha != null )
                                {
                                    var api = new CloudflareApi();
                                    var ipAddress = rockBlock.RequestContext.ClientInformation.IpAddress;

                                    rockBlock.RequestContext.IsCaptchaValid = await api.IsTurnstileTokenValidAsync( actionContext.Captcha, ipAddress );
                                }
                            }
                            else
                            {
                                actionParameters.AddOrReplace( kvp.Key, kvp.Value );
                            }
                        }
                    }
                    catch
                    {
                        return new BadRequestErrorMessageResult( "Invalid parameter data.", controller );
                    }
                }

                //
                // Parse any query string parameter data.
                //
                foreach ( var q in controller.Request.GetQueryNameValuePairs() )
                {
                    actionParameters.AddOrReplace( q.Key, JToken.FromObject( q.Value.ToString() ) );
                }

                requestContext.PrepareRequestForPage( pageCache );

                return await InvokeAction( controller, rockBlock, actionName, actionParameters, parameters );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogApiException( ex, controller.Request, GetPerson( controller, null )?.PrimaryAlias );

#if WEBFORMS
                return new System.Web.Http.Results.NegotiatedContentResult<System.Web.Http.HttpError>( HttpStatusCode.InternalServerError, new System.Web.Http.HttpError( ex.Message ), controller );
#else
#error Not implemented yet.
#endif
            }
        }

        /// <summary>
        /// Processes the specified block action.
        /// </summary>
        /// <param name="controller">The API controller that initiated this action.</param>
        /// <param name="block">The block.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="actionParameters">The action parameters.</param>
        /// <param name="bodyParameters">The posted body parameters.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">actionName
        /// or
        /// actionData</exception>
        internal static async Task<IActionResult> InvokeAction( ApiControllerBase controller, Blocks.IRockBlockType block, string actionName, Dictionary<string, JToken> actionParameters, JToken bodyParameters )
        {
            // Parse the body content into our normal parameters.
            if ( bodyParameters != null )
            {
                try
                {
                    // Parse any posted parameter data, existing query string
                    // parameters take precedence.
                    foreach ( var kvp in bodyParameters.ToObject<Dictionary<string, JToken>>() )
                    {
                        actionParameters.TryAdd( kvp.Key, kvp.Value );
                    }
                }
                catch
                {
                    return new BadRequestErrorMessageResult( "Invalid parameter data.", controller );
                }
            }

            //
            // Find the action they requested. First search by name
            // and then further filter by any method constraint attributes.
            //
            var actions = block.GetType().GetMethods( BindingFlags.Instance | BindingFlags.Public )
                .Where( m => m.GetCustomAttribute<Blocks.BlockActionAttribute>()?.ActionName == actionName )
                .ToList();

            if ( actions.Count == 0 )
            {
                return new NotFoundResult( controller );
            }

            var action = FindBestActionForParameters( actions, actionParameters );

            if ( action == null )
            {
                // This is an actual configuration error, so throw an error.
                throw new AmbiguousMatchException( "The request matched multiple actions." );
            }

            var methodParameters = action.GetParameters();
            var parameters = new List<object>();

            //
            // Go through each parameter and convert it to the proper type.
            //
            for ( int i = 0; i < methodParameters.Length; i++ )
            {
                // Check if this parameter is requesting it's content from the body.
                if ( methodParameters[i].GetCustomAttribute<FromBodyAttribute>() != null )
                {
                    if ( bodyParameters != null )
                    {
                        parameters.Add( bodyParameters.ToObject( methodParameters[i].ParameterType ) );
                    }
                    else if ( methodParameters[i].IsOptional )
                    {
                        parameters.Add( Type.Missing );
                    }
                    else
                    {
                        return new BadRequestErrorMessageResult( $"Parameter '{methodParameters[i].Name}' is required.", controller );
                    }

                    continue;
                }

                var key = actionParameters.Keys.SingleOrDefault( k => k.ToLowerInvariant() == methodParameters[i].Name.ToLower() );

                if ( key != null )
                {
                    try
                    {
                        //
                        // If the target type is nullable and the action parameter is an empty
                        // string then consider it null. A GET query cannot have null values.
                        //
                        if ( Nullable.GetUnderlyingType( methodParameters[i].ParameterType ) != null )
                        {
                            if ( actionParameters[key].Type == JTokenType.String && actionParameters[key].ToString() == string.Empty )
                            {
                                parameters.Add( null );

                                continue;
                            }
                        }

                        parameters.Add( actionParameters[key].ToObject( methodParameters[i].ParameterType ) );
                    }
                    catch ( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine( ex.Message );

                        return new BadRequestErrorMessageResult( $"Parameter type mismatch for '{methodParameters[i].Name}'.", controller );
                    }
                }
                else if ( methodParameters[i].IsOptional )
                {
                    parameters.Add( Type.Missing );
                }
                else
                {
                    return new BadRequestErrorMessageResult( $"Parameter '{methodParameters[i].Name}' is required.", controller );
                }
            }

            object result;
            try
            {
                result = action.Invoke( block, parameters.ToArray() );

                // Check if the result type is a Task.
                if ( result is Task resultTask )
                {
                    await resultTask;

                    // Task<T> is not covariant, so we can't just cast to Task<object>.
                    if ( resultTask.GetType().GetProperty( "Result" ) != null )
                    {
                        result = ( ( dynamic ) resultTask ).Result;
                    }
                    else
                    {
                        result = null;
                    }
                }
            }
            catch ( TargetInvocationException ex )
            {
                ExceptionLogService.LogApiException( ex.InnerException, controller.Request, GetPerson( controller, null )?.PrimaryAlias );
                result = new BlockActionResult( HttpStatusCode.InternalServerError, GetMessageForClient( ex ) );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogApiException( ex, controller.Request, GetPerson( controller, null )?.PrimaryAlias );
                result = new BlockActionResult( HttpStatusCode.InternalServerError, GetMessageForClient( ex ) );
            }

            var defaultContentNegotiator = new System.Net.Http.Formatting.DefaultContentNegotiator();
            var validFormatters = new List<System.Net.Http.Formatting.MediaTypeFormatter>()
            {
                new Rock.Rest.Utility.ApiPickerJsonMediaTypeFormatter(),
                new System.Net.Http.Formatting.JsonMediaTypeFormatter(),
                new System.Net.Http.Formatting.FormUrlEncodedMediaTypeFormatter(),
                new System.Web.Http.ModelBinding.JQueryMvcFormUrlEncodedFormatter()
            };

            // Handle the result type.
            if ( result is IActionResult httpActionResult )
            {
                return httpActionResult;
            }
            else if ( result is BlockActionResult actionResult )
            {
                return await actionResult.ExecuteAsync( controller, defaultContentNegotiator, validFormatters, System.Threading.CancellationToken.None );
            }
            else if ( action.ReturnType == typeof( void ) )
            {
                return new OkResult( controller );
            }
            else
            {
#if WEBFORMS
                return new System.Web.Http.Results.OkNegotiatedContentResult<object>( result, defaultContentNegotiator, controller.Request, validFormatters );
#else
#error Not implemented yet.
#endif
            }
        }

        /// <summary>
        /// Gets the message for client.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private static string GetMessageForClient( Exception exception )
        {
            if ( exception is null )
            {
                return "An unknown error occurred";
            }

            if ( exception.InnerException != null )
            {
                return GetMessageForClient( exception.InnerException );
            }

            if ( exception.Message.IsNullOrWhiteSpace() )
            {
                return "An unknown error occurred";
            }

            return exception.Message;
        }

        /// <summary>
        /// Finds the best action that matches the parameters we have.
        /// </summary>
        /// <param name="actions">The actions to be checked.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The single best match or <c>null</c> if it could not be determined.</returns>
        private static MethodInfo FindBestActionForParameters( IList<MethodInfo> actions, Dictionary<string, JToken> parameters )
        {
            if ( actions.Count == 0 )
            {
                return null;
            }

            // If we have just one action then return it as further error
            // checking will be performed to make sure we are not missing
            // any method parameters later.
            if ( actions.Count == 1 )
            {
                return actions[0];
            }

            // We have multiple actions that pass the initial screening.
            // Determine the best match based on parameters we are given
            // by the request.
            var methodActions = actions
                .Select( a => new
                {
                    Method = a,
                    Parameters = a.GetParameters()
                } )
                .ToList();

            var matchedActions = new List<MethodInfo>();
            var parameterNames = parameters.Keys.Select( k => k.ToLowerInvariant() ).ToList();

            // Look for all methods that we have enough parameters to
            // properly call.
            foreach ( var action in methodActions )
            {
                var matchedParameterCount = action.Parameters
                    .Where( p => parameterNames.Contains( p.Name.ToLowerInvariant() ) || p.IsOptional )
                    .Count();

                if ( matchedParameterCount == action.Parameters.Length )
                {
                    matchedActions.Add( action.Method );
                }
            }

            // If we are left with exactly one method that matches then return
            // it to the caller. Otherwise return null to indicate we could
            // not determine a good match.
            if ( matchedActions.Count == 1 )
            {
                return matchedActions[0];
            }

            return null;
        }

        #endregion
    }
}