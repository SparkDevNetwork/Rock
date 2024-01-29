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
        [NotMapped]
        [LavaVisible]
        public virtual bool IsAuthenticated
        {
            get
            {
                System.Web.Security.FormsIdentity identity = HttpContext.Current.User?.Identity as System.Web.Security.FormsIdentity;

                if ( identity == null )
                {
                    return false;
                }

                if ( Rock.Security.Authorization.GetUserData( identity.Ticket )?.IsImpersonated == true )
                {
                    return false;
                }

                return true;
            }
        }
        /// <summary>
        /// Gets a flag indicating if the User is two-factor authenticated.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if the user is two-factor authenticated; otherwise <c>false</c>.
        /// </value>
        [NotMapped]
        public virtual bool IsTwoFactorAuthenticated
        {
            get
            {
                System.Web.Security.FormsIdentity identity = HttpContext.Current.User?.Identity as System.Web.Security.FormsIdentity;

                if ( identity == null )
                {
                    return false;
                }

                return Rock.Security.Authorization.GetUserData( identity.Ticket )?.IsTwoFactorAuthenticated == true;
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

            if ( currentPrincipal.Identity.Name.StartsWith( "rckipid=" ) )
            {
                var personToken = new PersonTokenService( new Rock.Data.RockContext() ).GetByImpersonationToken( currentPrincipal.Identity.Name.Substring( 8 ) );
                if ( personToken?.PersonAlias?.Person != null )
                {
                    return personToken.PersonAlias.Person.GetImpersonatedUser()?.UserName ?? currentPrincipal.Identity.Name;
                }

                return currentPrincipal.Identity.Name;
            }
            else
            {
                return currentPrincipal.Identity.Name;
            }
        }

        #endregion
    }
}
