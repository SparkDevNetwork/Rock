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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;

using Rock.Data;

namespace Rock.Rest.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidateAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting( HttpActionContext actionContext )
        {
            ModelStateDictionary modelState = actionContext.ModelState;

            IEntity valueArg = null;
            if ( actionContext.ActionArguments.ContainsKey( "value" ) )
            {
                valueArg = actionContext.ActionArguments["value"] as IEntity;
            }

            if ( valueArg != null )
            {
                Type entityType = actionContext.ActionArguments["value"].GetType();
                IgnoreModelErrorsAttribute ignoreModelErrorsAttribute = entityType.GetCustomAttributes( typeof( IgnoreModelErrorsAttribute ), true ).FirstOrDefault() as IgnoreModelErrorsAttribute;

                if ( ignoreModelErrorsAttribute != null )
                {
                    foreach ( string key in ignoreModelErrorsAttribute.Keys )
                    {
                        IEnumerable<string> matchingKeys = modelState.Keys.Where( x => Regex.IsMatch( x, key ) );
                        foreach ( string matchingKey in matchingKeys )
                        {
                            modelState[matchingKey].Errors.Clear();
                        }
                    }
                }
            }

            if ( !actionContext.ModelState.IsValid )
            {
                HttpError httpError = new HttpError();

                foreach ( var item in actionContext.ModelState )
                {
                    var msg = new System.Text.StringBuilder();

                    foreach ( ModelError error in item.Value.Errors )
                    {
                        if ( !string.IsNullOrWhiteSpace( error.ErrorMessage ) )
                        {
                            msg.Append( msg.Length > 0 ? "; " : "" );
                            msg.Append( error.ErrorMessage );
                        }

                        if ( error.Exception != null && !string.IsNullOrWhiteSpace( error.Exception.Message ) )
                        {
                            msg.Append( msg.Length > 0 ? "; " : "" );
                            msg.Append( error.Exception.Message );
                        }
                    }

                    httpError.Add( item.Key, msg.ToString() );
                }

                actionContext.Response = actionContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, httpError );
            }
        }
    }
}