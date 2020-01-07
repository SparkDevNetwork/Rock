using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    public partial class RegistrationTemplatePlacementService
    {
        /// <summary>
        /// Gets the placement groups.
        /// </summary>
        /// <param name="placement">The placement.</param>
        /// <returns></returns>
        public IQueryable<Group> GetPlacementGroups( RegistrationTemplatePlacement placement )
        {
            return this.GetRelatedToSourceEntity<Group>( placement.Id, RelatedEntityPurposeKey.GroupPlacement );
        }

        public void SetPlacementGroups( RegistrationTemplatePlacement placement, List<Group> groups )
        {
            this.SetRelatedToSourceEntity( placement.Id, groups, RelatedEntityPurposeKey.GroupPlacement );
        }

        public IQueryable<Group> GetPlacementGroupTemplates( RegistrationTemplatePlacement placement )
        {
            return this.GetRelatedToSourceEntity<Group>( placement.Id, RelatedEntityPurposeKey.GroupPlacementTemplate );
        }

        public void SetPlacementGroupTemplates( RegistrationTemplatePlacement placement, List<Group> groups )
        {
            this.SetRelatedToSourceEntity( placement.Id, groups, RelatedEntityPurposeKey.GroupPlacementTemplate );
        }
    }
}
