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
            return this.GetRelatedToSourceEntity<Group>( registrationInstance.Id, RelatedEntityPurposeKey.GroupPlacement );
        }

        /// <summary>
        /// Sets the registration instance placement groups.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="groups">The groups.</param>
        public void SetRegistrationInstancePlacementGroups( RegistrationInstance registrationInstance, List<Group> groups )
        {
            this.SetRelatedToSourceEntity( registrationInstance.Id, groups, RelatedEntityPurposeKey.GroupPlacement );
        }
    }
}
