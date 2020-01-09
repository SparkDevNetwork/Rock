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
            return this.GetRelatedToSourceEntity<Group>( registrationTemplatePlacement.Id, RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate );
        }

        /// <summary>
        /// Sets the RegistrationTemplatePlacement placement groups
        /// </summary>
        /// <param name="registrationTemplatePlacement">The registration template placement.</param>
        /// <param name="groups">The groups.</param>
        public void SetRegistrationTemplatePlacementPlacementGroups( RegistrationTemplatePlacement registrationTemplatePlacement, List<Group> groups )
        {
            this.SetRelatedToSourceEntity( registrationTemplatePlacement.Id, groups, RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate );
        }
    }
}
