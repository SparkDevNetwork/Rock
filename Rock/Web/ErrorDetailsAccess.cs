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

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web
{
    /// <summary>
    /// Decides who may see exception details (messages and stack traces)
    /// on Rock's error pages and error layouts.
    /// </summary>
    [RockInternal( "17.14", keepInternalForever: true )]
    public static class ErrorDetailsAccess
    {
        /// <summary>
        /// Determines whether the person currently signed in is an active
        /// member of the Administrators security role. Returns <c>false</c>
        /// when nobody is signed in or when the check itself fails (for
        /// example, when the database is not available).
        /// </summary>
        /// <returns><c>true</c> if the current person may see exception details; otherwise <c>false</c>.</returns>
        public static bool IsActiveAdministrator()
        {
            try
            {
                var userLogin = UserLoginService.GetCurrentUser();

                return IsActiveAdministrator( userLogin?.Person?.Guid );
            }
            catch
            {
                // An error page must never fail because of this check, and
                // it must not show details when we cannot tell who is asking.
                return false;
            }
        }

        /// <summary>
        /// Determines whether the person is an active member of the
        /// Administrators security role.
        /// </summary>
        /// <param name="personGuid">The unique identifier of the person to check.</param>
        /// <returns><c>true</c> if the person may see exception details; otherwise <c>false</c>.</returns>
        public static bool IsActiveAdministrator( Guid? personGuid )
        {
            if ( !personGuid.HasValue )
            {
                return false;
            }

            /*
                11/25/2026 - MSE

                Use the cached role instead of querying the group members directly.
                The role only lists active members of an active role, so inactive
                and pending administrators do not see exception details.

                Reason: Exception details are for active administrators only.
            */
            var adminRole = RoleCache.Get( SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );

            return adminRole != null && adminRole.IsPersonInRole( personGuid );
        }
    }
}
