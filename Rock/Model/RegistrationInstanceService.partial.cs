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
using System.Collections.Generic;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RegistrationInstanceService
    {
        /// <summary>
        /// Gets the registration instance placement groups.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <returns></returns>
        public IQueryable<Group> GetRegistrationInstancePlacementGroups( RegistrationInstance registrationInstance )
        {
            return this.RelatedEntities.GetRelatedToSourceEntity<Group>( registrationInstance.Id, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement );
        }

        /// <summary>
        /// Sets the registration instance placement groups.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="groups">The groups.</param>
        public void SetRegistrationInstancePlacementGroups( RegistrationInstance registrationInstance, List<Group> groups )
        {
            this.RelatedEntities.SetRelatedToSourceEntity( registrationInstance.Id, groups, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement );
        }

        /// <summary>
        /// Adds the registration instance placement group. Returns false if the group is already a placement group for this instance.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public bool AddRegistrationInstancePlacementGroup( RegistrationInstance registrationInstance, Group group )
        {
            if ( !this.RelatedEntities.RelatedToSourceEntityAlreadyExists( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement ) )
            {
                this.RelatedEntities.AddRelatedToSourceEntity( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes (detaches) the registration instance placement group.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="group">The group.</param>
        public void DeleteRegistrationInstancePlacementGroup( RegistrationInstance registrationInstance, Group group )
        {
            if ( this.RelatedEntities.RelatedToSourceEntityAlreadyExists( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement ) )
            {
                this.RelatedEntities.DeleteRelatedToSourceEntity( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement );
            }
        }
    }
}
