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
    /// PersonAlias REST API
    /// </summary>
    public partial class PersonAliasController
    {
        /// <inheritdoc/>
        protected override void CheckCanEdit( PersonAlias entity )
        {
            base.CheckCanEdit( entity );

            /*
                10/16/2026 - MSE

                An alias decides which person receives the security grants and the records made
                by that alias, so moving an alias to another person hands that person the
                grants. Aliases have no record security, and staff hold EDIT on every REST
                controller through the seeded entity type rule, so require ADMINISTRATE on this
                REST controller for every write.

                Reason: Staff must not move or change person aliases through v1 REST.
            */
            CheckCanAdministrateController();
        }
    }
}
