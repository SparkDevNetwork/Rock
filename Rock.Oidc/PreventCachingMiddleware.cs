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

using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Owin;

using Rock.Oidc.Configuration;

namespace Rock.Oidc
{
    /// <summary>
    /// Marks responses from the OIDC endpoints as non-cacheable so that
    /// browsers, proxies and CDNs never store or replay them.
    /// </summary>
    internal class PreventCachingMiddleware : OwinMiddleware
    {
        #region Fields

        /// <summary>
        /// The exact paths of the OIDC endpoints whose responses must never
        /// be cached. These are matched exactly (not by prefix) so that
        /// admin-created pages that happen to live under /Auth are not affected.
        /// </summary>
        private static readonly PathString[] _nonCacheablePaths = new[]
        {
            new PathString( Paths.AuthorizePath ),
            new PathString( Paths.TokenPath ),
            new PathString( Paths.LogoutPath ),
            new PathString( Paths.UserInfo ),
            new PathString( Paths.RegisterPath )
        };

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="PreventCachingMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline.</param>
        public PreventCachingMiddleware( OwinMiddleware next )
            : base( next )
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Task Invoke( IOwinContext context )
        {
            /*
                10/21/26 - CLAUDE

                The OIDC server library (ASOS) intentionally sends no cache headers
                for userinfo responses, and requests that complete inside OWIN never
                get ASP.NET's default "Cache-Control: private" because that is only
                added when a managed handler runs. With no cache headers at all, a
                CDN cached /Auth/UserInfo and served one person's identity to others.

                The headers are set through HttpCachePolicy rather than raw response
                headers because System.Web overwrites raw cache headers whenever the
                policy has been modified (for example by a Rock page's cache settings
                on the Authorize and Logout page routes), and the policy can only be
                made more restrictive once set.

                Reason: Prevent CDNs from caching OIDC responses.
            */
            var requestPath = context.Request.Path;
            var isNonCacheablePath = _nonCacheablePaths.Any( path => path.Equals( requestPath ) );

            if ( isNonCacheablePath )
            {
                var httpContext = context.Get<HttpContextBase>( typeof( HttpContextBase ).FullName );

                if ( httpContext != null )
                {
                    httpContext.Response.Cache.SetCacheability( HttpCacheability.NoCache );
                    httpContext.Response.Cache.SetNoStore();
                }
            }

            return Next.Invoke( context );
        }

        #endregion
    }
}
