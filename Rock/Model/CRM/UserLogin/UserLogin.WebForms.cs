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
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;

using Rock.Lava;

namespace Rock.Model
{
    public partial class UserLogin
    {
        /// <summary>
        /// Gets a flag indicating if the User authenticated with their last interaction with Rock (versus using an impersonation link).
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if the user actually authenticated; otherwise <c>false</c>.
        /// </value>
        /// <remarks>
        /// Under the PersonSession model authentication strength lives on the
        /// current <see cref="PersonSession"/>, not on the legacy
        /// <c>FormsAuthenticationTicket.UserData</c>. This property is
        /// preserved as a Pattern A bridge so existing readers still compile
        /// during the dual-reader window: it returns <c>true</c> only when the
        /// current request's <see cref="PersonSession"/> is active AND is not
        /// an impersonated session. New callers should read
        /// <c>RockRequestContext.PersonSession</c> (or call
        /// <see cref="Rock.Net.RockRequestContext.MeetsRequirement"/>) directly;
        /// this property will be obsoleted in a follow-up phase.
        /// </remarks>
        [NotMapped]
        [LavaVisible]
        public virtual bool IsAuthenticated
        {
            get
            {
                var personSession = Rock.Net.RockRequestContextAccessor.Current?.PersonSession;
                if ( personSession == null || !personSession.IsActive )
                {
                    return false;
                }

                return !personSession.IsImpersonated();
            }
        }

        /// <summary>
        /// Gets a flag indicating if the User is two-factor authenticated.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if the user is two-factor authenticated; otherwise <c>false</c>.
        /// </value>
        /// <remarks>
        /// Under the PersonSession model two-factor recency lives on the
        /// current <see cref="PersonSession"/>. This Pattern A bridge returns
        /// <c>true</c> only when the current request's session meets the
        /// <see cref="Rock.Enums.Security.AuthenticationRequirement.MultiFactor"/>
        /// requirement. New callers should call
        /// <c>RockRequestContext.MeetsRequirement(MultiFactor)</c> directly;
        /// this property will be obsoleted in a follow-up phase.
        /// </remarks>
        [NotMapped]
        public virtual bool IsTwoFactorAuthenticated
        {
            get
            {
                var requestContext = Rock.Net.RockRequestContextAccessor.Current;
                if ( requestContext == null )
                {
                    return false;
                }

                return requestContext.MeetsRequirement( Rock.Enums.Security.AuthenticationRequirement.MultiFactor );
            }
        }

        #region Static Methods

        /// <summary>
        /// Returns the UserName of the user that is currently logged in.
        /// </summary>
        /// <returns>A <see cref="System.String"/> representing the UserName of the user that is currently logged in.</returns>
        public static string GetCurrentUserName()
        {
            if ( HostingEnvironment.IsHosted )
            {
                HttpContext current = HttpContext.Current;
                if ( current != null && current.User != null )
                {
                    return current.User.Identity.Name;
                }
            }

            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            if ( currentPrincipal?.Identity == null )
            {
                return string.Empty;
            }

            /*
                The legacy `rckipid=` identity-name parsing branch is gone:
                under the PersonSession model the `.ROCK` cookie no longer
                carries `rckipid=` in the identity name. User-token email
                flows are now established by
                `PersonSessionService.ProcessImpersonationToken`, which
                creates a UserToken `PersonSession` and emits a standard
                new-format cookie; by the time this method runs, the
                identity name is already the impersonated user's UserName
                directly.
            */
            return currentPrincipal.Identity.Name;
        }

        #endregion
    }
}
