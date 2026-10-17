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
    /// GroupMemberWorkflowTriggers REST API
    /// </summary>
    public partial class GroupMemberWorkflowTriggersController
    {
        /// <inheritdoc/>
        protected override void CheckCanEdit( GroupMemberWorkflowTrigger entity )
        {
            base.CheckCanEdit( entity );

            /*
                10/16/2026 - MSE

                Group member workflow triggers launch any workflow type on group member changes
                for any group or group type. They are administrator settings with no record
                security, and staff hold EDIT on every REST controller through the seeded entity
                type rule, so require ADMINISTRATE on this REST controller for every write.
                Group detail pages save these on the server, not through this API.

                Reason: Staff must not create or change group member workflow triggers.
            */
            CheckCanAdministrateController();
        }
    }
}
