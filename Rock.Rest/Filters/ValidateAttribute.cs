//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
                    foreach ( ModelError error in item.Value.Errors )
                    {
                        if ( !httpError.ContainsKey( item.Key ) )
                        {
                            httpError.Add( item.Key, string.Empty );
                            httpError[item.Key] += error.ErrorMessage;
                        }
                        else
                        {
                            httpError[item.Key] += Environment.NewLine + error.ErrorMessage;
                        }
                    }
                }

                actionContext.Response = actionContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, httpError );
            }
        }
    }
}