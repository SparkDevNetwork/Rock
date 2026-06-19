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

namespace Rock.Rest;

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
[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
public class SecuredAttribute : System.Attribute
{
    /// <summary>
    /// The security actions that will be checked when authorizing the request.
    /// If this is empty then it will be automatically determined by the HTTP verb.
    /// </summary>
    public IReadOnlyCollection<string> SecurityActions { get; }

    /// <summary>
    /// Creates a new instance of <see cref="SecuredAttribute"/> that
    /// automatically detects the security action based on the HTTP verb
    /// of the request.
    /// </summary>
    public SecuredAttribute()
    {
        SecurityActions = [];
    }

    /// <summary>
    /// Creates a new instance of <see cref="SecuredAttribute"/> that
    /// uses the specified security action when authorizing the request.
    /// </summary>
    /// <param name="securityAction">The security action such as VIEW or EDIT.</param>
    public SecuredAttribute( string securityAction )
    {
        SecurityActions = [securityAction];
    }

    /// <summary>
    /// Creates a new instance of <see cref="SecuredAttribute"/> that
    /// uses the specified security actions when authorizing the request.
    /// If any one of the actions is authorized then the request will proceed.
    /// </summary>
    /// <param name="securityAction">The security action such as VIEW or EDIT.</param>
    /// <param name="additionalSecurityActions">Additional security actions that will be checked when authorizing the request.</param>
    public SecuredAttribute( string securityAction, params string[] additionalSecurityActions )
    {
        SecurityActions = new[] { securityAction }.Concat( additionalSecurityActions ?? [] ).ToArray();
    }
}
