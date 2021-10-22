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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
#if !NET5_0_OR_GREATER
using System.Web.Http.ModelBinding;
#endif

#if NET5_0_OR_GREATER
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
#endif

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Rest.Filters
{
    /// <summary>
    /// 
    /// </summary>
#if NET5_0_OR_GREATER
    public class RockCacheabilityAttribute : System.Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync( ActionExecutingContext actionContext, ActionExecutionDelegate next )
        {
            await next();

            var reflectedHttpActionDescriptor = ( ControllerActionDescriptor ) actionContext.ActionDescriptor;
            var actionMethod = actionContext.HttpContext.Request.Method;
            var controller = reflectedHttpActionDescriptor;

            var apiId = RestControllerService.GetApiId( reflectedHttpActionDescriptor.MethodInfo, actionMethod, controller.ControllerName );
            var restActionCache = RestActionCache.Get( apiId );
            if ( restActionCache != null && restActionCache.CacheControlHeader.IsNotNullOrWhiteSpace() )
            {
                actionContext.HttpContext.Response.Headers.Add( "Cache-Control", restActionCache.CacheControlHeader );
            }
        }
    }
#else
    public class RockCacheabilityAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Occurs after the action method is invoked.
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context.</param>
        public override void OnActionExecuted( HttpActionExecutedContext actionExecutedContext )
        {
            base.OnActionExecuted( actionExecutedContext );

            var reflectedHttpActionDescriptor = ( ReflectedHttpActionDescriptor ) actionExecutedContext.ActionContext.ActionDescriptor;
            var actionMethod = actionExecutedContext.Request.Method.Method;
            var controller = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor;

            var apiId = RestControllerService.GetApiId( reflectedHttpActionDescriptor.MethodInfo, actionMethod, controller.ControllerName );
            var restActionCache = RestActionCache.Get( apiId );
            var cacheControl = restActionCache.CacheControlHeader.IsNotNullOrWhiteSpace() ? restActionCache.CacheControlHeader : "no-store";

            if ( restActionCache != null && restActionCache.CacheControlHeader.IsNotNullOrWhiteSpace() )
            {
                actionExecutedContext.Response.Headers.Add( "Cache-Control", cacheControl );
            }
        }
    }
#endif
    }
