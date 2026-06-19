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
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Rock.Rest.Filters
{
    /// <summary>
    /// <para>
    /// Checks to see if the logged-in person has authorization to access the
    /// API endpoint. If no security action is specified, then it will be
    /// determined by the HTTP verb (GET = VIEW, all others = EDIT).
    /// </para>
    /// <para>
    /// If multipile security actions are specified, then the person only needs
    /// to have authorization for one of the actions to access the endpoint.
    /// </para>
    /// </summary>
    [Obsolete( "Use Rock.Rest.SecuredAttribute from Rock.Rest.Abstractions assembly instead." )]
    [RockObsolete( "20.0" )]
    public class SecuredAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// The security actions that will be checked when authorizing the request.
        /// If this is empty then it will be automatically determined by the HTTP verb.
        /// </summary>
        public IReadOnlyCollection<string> SecurityActions { get; }

        /// <summary>
        /// The security action that will be checked when authorizing the request.
        /// If this is null or an empty string then it will be automatically
        /// determined by the HTTP verb.
        /// </summary>
        [Obsolete( "Use SecurityActions instead." )]
        [RockObsolete( "19.0" )]
        public string SecurityAction => SecurityActions.FirstOrDefault();

        /// <summary>
        /// Creates a new instance of <see cref="SecuredAttribute"/> that
        /// automatically detects the security action based on the HTTP verb
        /// of the request.
        /// </summary>
        public SecuredAttribute()
        {
            SecurityActions = Array.Empty<string>();
        }

        /// <summary>
        /// Creates a new instance of <see cref="SecuredAttribute"/> that
        /// uses the specified security action when authorizing the request.
        /// </summary>
        /// <param name="securityAction">The security action such as VIEW or EDIT.</param>
        public SecuredAttribute( string securityAction )
        {
            SecurityActions = new[] { securityAction };
        }

        /// <summary>
        /// Creates a new instance of <see cref="SecuredAttribute"/> that
        /// uses the specified security actions when authorizing the request.
        /// If any one of the actions is authorized then the request will proceed.
        /// </summary>
        /// <param name="securityActions">The security actions such as VIEW or EDIT.</param>
        public SecuredAttribute( params string[] securityActions )
        {
            SecurityActions = securityActions ?? Array.Empty<string>();
        }

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting( HttpActionContext actionContext )
        {
            SecuredFilter.AuthorizeRequest( actionContext, SecurityActions );
        }
    }
}
