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
    /// EntityTypes REST API
    /// </summary>
    public partial class EntityTypesController
    {
        /// <inheritdoc/>
        protected override void CheckCanEdit( EntityType entity )
        {
            base.CheckCanEdit( entity );

            /*
                10/16/2026 - MSE

                Entity type settings control caching, search result templates and security flags
                for every record of that type, and a renamed entity type breaks lookups by name.
                Entity types have no record security, and staff hold EDIT on every REST
                controller through the seeded entity type rule, so require ADMINISTRATE on this
                REST controller for every write.

                Reason: Staff must not create or change entity types.
            */
            CheckCanAdministrateController();
        }
    }
}
