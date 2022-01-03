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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Blocks;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.v2
{
    /// <summary>
    /// API controller for the /api/v2/BlockActions endpoints.
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    public class BlockActionsController : ApiControllerBase
    {
        #region API Methods

        /// <summary>
        /// Executes an action handler on a specific block.
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpGet]
        [Route( "api/v2/BlockActions/{pageGuid:guid}/{blockGuid:guid}/{actionName}" )]
        public IHttpActionResult BlockAction( Guid pageGuid, Guid blockGuid, string actionName )
        {
            return ProcessAction( this, pageGuid, blockGuid, actionName, null );
        }

        /// <summary>
        /// Executes an action handler on a specific block.
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpPost]
        [Route( "api/v2/BlockActions/{pageGuid:guid}/{blockGuid:guid}/{actionName}" )]
        public IHttpActionResult BlockActionAsPost( Guid pageGuid, Guid blockGuid, string actionName, [NakedBody] string parameters )
        {
            if ( parameters == string.Empty )
            {
                return ProcessAction( this, pageGuid, blockGuid, actionName, null );
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

                    return ProcessAction( this, pageGuid, blockGuid, actionName, parameterToken );
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
        /// <returns></returns>
        internal static IHttpActionResult ProcessAction( ApiControllerBase controller, Guid? pageGuid, Guid? blockGuid, string actionName, JToken parameters )
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

                //
                // Get the authenticated person and make sure it's cached.
                //
                var person = GetPerson( controller, null );
                HttpContext.Current.AddOrReplaceItem( "CurrentPerson", person );

                //
                // Ensure the user has access to both the page and block.
                //
                if ( !pageCache.IsAuthorized( Security.Authorization.VIEW, person ) || !blockCache.IsAuthorized( Security.Authorization.VIEW, person ) )
                {
                    return new StatusCodeResult( HttpStatusCode.Unauthorized, controller );
                }

                //
                // Get the class that handles the logic for the block.
                //
                var blockCompiledType = blockCache.BlockType.GetCompiledType();
                var block = Activator.CreateInstance( blockCompiledType );

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
                                // If we are given any page parameters then
                                // override the query string parameters. This
                                // is what allows mobile and obsidian blocks to
                                // pass in the original page parameters.
                                if ( kvp.Value["pageParameters"] != null )
                                {
                                    var pageParameters = kvp.Value["pageParameters"].ToObject<Dictionary<string, string>>();

                                    rockBlock.RequestContext.SetPageParameters( pageParameters );
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

                requestContext.AddContextEntitiesForPage( pageCache );

                return InvokeAction( controller, rockBlock, actionName, actionParameters, parameters );
            }
            catch ( Exception ex )
            {
                return new BadRequestErrorMessageResult( ex.Message, controller );
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
        internal static IHttpActionResult InvokeAction( ApiControllerBase controller, Blocks.IRockBlockType block, string actionName, Dictionary<string, JToken> actionParameters, JToken bodyParameters )
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
                        actionParameters.AddOrIgnore( kvp.Key, kvp.Value );
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

            //
            // Handle the result type.
            //
            if ( result is IHttpActionResult httpActionResult )
            {
                return httpActionResult;
            }
            else if ( result is BlockActionResult actionResult )
            {
                var isErrorStatusCode = ( int ) actionResult.StatusCode >= 400;

                if ( isErrorStatusCode && actionResult.Content is string )
                {
                    return new NegotiatedContentResult<HttpError>( actionResult.StatusCode, new HttpError( actionResult.Content.ToString() ), controller );
                }
                else if ( actionResult.Error != null )
                {
                    return new NegotiatedContentResult<HttpError>( actionResult.StatusCode, new HttpError( actionResult.Error ), controller );
                }
                else if ( actionResult.Content is HttpContent httpContent )
                {
                    var response = controller.Request.CreateResponse( actionResult.StatusCode );
                    response.Content = httpContent;
                    return new ResponseMessageResult( response );
                }
                else if ( actionResult.ContentClrType != null )
                {
                    var genericType = typeof( System.Web.Http.Results.NegotiatedContentResult<> ).MakeGenericType( actionResult.ContentClrType );
                    return ( IHttpActionResult ) Activator.CreateInstance( genericType, actionResult.StatusCode, actionResult.Content, controller );
                }
                else
                {
                    return new StatusCodeResult( actionResult.StatusCode, controller );
                }
            }
            else if ( action.ReturnType == typeof( void ) )
            {
                return new OkResult( controller );
            }
            else
            {
                return new OkNegotiatedContentResult<object>( result, controller );
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
