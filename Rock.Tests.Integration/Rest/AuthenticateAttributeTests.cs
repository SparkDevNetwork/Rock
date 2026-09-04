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
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Net;
using Rock.Rest.Filters;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Rest
{
    /// <summary>
    /// Tests for the REST PIN-authentication guard.
    /// </summary>
    /// <remarks>
    /*
        9/4/26 - CLAUDE

        This is the new home of the regression coverage for issue #4977
        ("Tree Viewers breaking when PIN authentication is disabled"). The guard
        that rejects PIN logins from the REST API used to live in SecuredAttribute
        and was exercised by SecuredAttributeTests. The PersonSession refactor
        moved that guard to AuthenticateAttribute (it now reads the resolved
        UserLogin from the RockRequestContext rather than re-resolving the person
        from the request principal), so the coverage moved with it.

        Unlike the original test, these do NOT toggle the PIN component's "Active"
        attribute. The new guard (AuthenticateAttribute.IsPinAuthentication)
        decides purely on the login's EntityType and never consults the Active
        state, so a non-PIN login is allowed through whether PIN authentication is
        enabled or disabled. Mutating the shared cached component - as the old test
        did, without cleanup - would only add a global side effect that no longer
        changes the outcome.

        Reason: #4977 regression coverage follows the PIN guard to its new home.
    */
    /// </remarks>
    [TestClass]
    public class AuthenticateAttributeTests : DatabaseTestsBase
    {
        /// <summary>
        /// A normal (non-PIN) login must be allowed through the REST API. This is
        /// the direct regression guard for #4977: disabling or bypassing PIN
        /// authentication must never cause the guard to reject ordinary users.
        /// </summary>
        [TestMethod]
        public void NonPinAuthenticationIsAllowedThroughRestApi()
        {
            var databaseEntityTypeId = EntityTypeCache.Get( typeof( Rock.Security.Authentication.Database ) ).Id;
            var databaseUser = new UserLogin { UserName = "tdecker", EntityTypeId = databaseEntityTypeId };

            var actionContext = AuthorizeWithCurrentUser( databaseUser );

            // OnAuthorization only assigns a Response when it rejects the request,
            // so a null Response means the guard let the request continue.
            Assert.IsNull( actionContext.Response );
        }

        /// <summary>
        /// A PIN login must be rejected with 401 Unauthorized. PIN logins identify
        /// a person for low-trust scenarios (e.g. check-in) and are intentionally
        /// not permitted to access the REST API.
        /// </summary>
        [TestMethod]
        public void PinAuthenticationIsRejectedFromRestApi()
        {
            var pinEntityTypeId = EntityTypeCache.Get( typeof( Rock.Security.Authentication.PINAuthentication ) ).Id;
            var pinUser = new UserLogin { UserName = "7777", EntityTypeId = pinEntityTypeId };

            var actionContext = AuthorizeWithCurrentUser( pinUser );

            Assert.IsNotNull( actionContext.Response );
            Assert.AreEqual( HttpStatusCode.Unauthorized, actionContext.Response.StatusCode );
        }

        /// <summary>
        /// Runs <see cref="AuthenticateAttribute.OnAuthorization"/> for a request
        /// whose already-resolved current user is <paramref name="currentUser"/>,
        /// mirroring the state the PersonSession pipeline establishes before the
        /// filter runs. The resolved user is exposed through the same
        /// IServiceProvider -&gt; IRockRequestContextAccessor lookup the filter uses
        /// in production, and a matching principal is placed on the current thread
        /// (which is where <c>TryAuthenticateFromCurrentPrincipal</c> reads it).
        /// </summary>
        /// <param name="currentUser">The user login resolved for the request.</param>
        /// <returns>The action context after authorization, for inspecting its Response.</returns>
        private static HttpActionContext AuthorizeWithCurrentUser( UserLogin currentUser )
        {
            // The person is irrelevant to the PIN guard, which reads only the
            // current user's EntityType; pass null to keep the setup focused.
            var requestContext = new RockRequestContext();
            requestContext.SetCurrentIdentity( null, currentUser );

            var accessor = new TestRockRequestContextAccessor { RockRequestContext = requestContext };
            var serviceProvider = new TestServiceProvider( accessor );

            var request = new HttpRequestMessage();
            request.Properties["RockServiceProvider"] = serviceProvider;

            var principal = new GenericPrincipal( new GenericIdentity( currentUser.UserName ), null );
            var httpRequestContext = new HttpRequestContext { Principal = principal };

            var actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext
                {
                    Request = request,
                    RequestContext = httpRequestContext
                }
            };

            var originalPrincipal = Thread.CurrentPrincipal;
            try
            {
                // TryAuthenticateFromCurrentPrincipal reads Thread.CurrentPrincipal,
                // not the request-context principal, so set it here.
                Thread.CurrentPrincipal = principal;
                new AuthenticateAttribute().OnAuthorization( actionContext );
            }
            finally
            {
                Thread.CurrentPrincipal = originalPrincipal;
            }

            return actionContext;
        }

        /// <summary>
        /// Minimal <see cref="IRockRequestContextAccessor"/> that returns a
        /// preset context, standing in for the DI-provided accessor.
        /// </summary>
        private sealed class TestRockRequestContextAccessor : IRockRequestContextAccessor
        {
            public RockRequestContext RockRequestContext { get; set; }
        }

        /// <summary>
        /// Minimal <see cref="IServiceProvider"/> that resolves only the
        /// <see cref="IRockRequestContextAccessor"/> the filter asks for.
        /// </summary>
        private sealed class TestServiceProvider : IServiceProvider
        {
            private readonly IRockRequestContextAccessor _accessor;

            public TestServiceProvider( IRockRequestContextAccessor accessor )
            {
                _accessor = accessor;
            }

            public object GetService( Type serviceType )
            {
                if ( serviceType == typeof( IRockRequestContextAccessor ) )
                {
                    return _accessor;
                }

                return null;
            }
        }
    }
}
