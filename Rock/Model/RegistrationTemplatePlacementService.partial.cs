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
using System.Collections.Generic;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RegistrationTemplatePlacementService
    {
        /// <summary>
        /// Gets the replacement groups for a specific RegistrationTemplatePlacement
        /// </summary>
        /// <param name="registrationTemplatePlacement">The registration template placement.</param>
        /// <returns></returns>
        public IQueryable<Group> GetRegistrationTemplatePlacementPlacementGroups( RegistrationTemplatePlacement registrationTemplatePlacement )
        {
            return this.RelatedEntities.GetRelatedToSourceEntity<Group>( registrationTemplatePlacement.Id, RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate );
        }

        /// <summary>
        /// Sets the RegistrationTemplatePlacement placement groups
        /// </summary>
        /// <param name="registrationTemplatePlacement">The registration template placement.</param>
        /// <param name="groups">The groups.</param>
        public void SetRegistrationTemplatePlacementPlacementGroups( RegistrationTemplatePlacement registrationTemplatePlacement, List<Group> groups )
        {
            this.RelatedEntities.SetRelatedToSourceEntity( registrationTemplatePlacement.Id, groups, RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate );
        }

        /// <summary>
        /// Adds the registrationTemplatePlacement group. Returns false if the group is already a placement group for this registrationTemplatePlacement
        /// </summary>
        /// <param name="registrationTemplatePlacement">The registration template placement.</param>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public bool AddRegistrationTemplatePlacementPlacementGroup( RegistrationTemplatePlacement registrationTemplatePlacement, Group group )
        {
            if ( !this.RelatedEntities.RelatedToSourceEntityAlreadyExists( registrationTemplatePlacement.Id, group, RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate ) )
            {
                this.RelatedEntities.AddRelatedToSourceEntity( registrationTemplatePlacement.Id, group, RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes the registration template placement placement group.
        /// </summary>
        /// <param name="registrationTemplatePlacement">The registration template placement.</param>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public bool DeleteRegistrationTemplatePlacementPlacementGroup( RegistrationTemplatePlacement registrationTemplatePlacement, Group group )
        {
            if ( this.RelatedEntities.RelatedToSourceEntityAlreadyExists( registrationTemplatePlacement.Id, group, RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate ) )
            {
                this.RelatedEntities.DeleteRelatedToSourceEntity( registrationTemplatePlacement.Id, group, RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate );
                return true;
            }

            return false;
        }
    }
}
