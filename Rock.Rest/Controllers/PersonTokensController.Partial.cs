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
using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// PersonTokens REST API
    /// </summary>
    public partial class PersonTokensController
    {
        /// <inheritdoc/>
        protected override void CheckCanEdit( PersonToken entity )
        {
            base.CheckCanEdit( entity );

            /*
                11/16/2026 - MSE

                A person token is a sign-in credential, and its PersonAliasId decides which
                person the token signs in as, so re-pointing a token to another person lets the
                caller sign in as that person. Person tokens have no record security, and staff
                hold EDIT on every REST controller through the seeded entity type rule, so
                require ADMINISTRATE on this REST controller for every write. Rock creates and
                expires tokens through PersonTokenService, never over REST, and reads are not
                changed.

                Reason: Staff must not re-point, reset or delete sign-in tokens through v1 REST.
            */
            CheckCanAdministrateController();
        }
    }
}
