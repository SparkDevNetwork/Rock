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

        public void AddRegistrationInstancePlacementGroups( RegistrationInstance registrationInstance, Group group )
        {
            this.RelatedEntities.AddRelatedToSourceEntity( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement );
        }
    }
}
