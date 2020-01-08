using System.Collections.Generic;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RegistrationTemplateService
    {
        /// <summary>
        /// Gets the registration template placement groups.
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <returns></returns>
        public IQueryable<Group> GetRegistrationTemplatePlacementGroups( RegistrationTemplate registrationTemplate )
        {
            return this.GetRelatedToSourceEntity<Group>( registrationTemplate.Id, RelatedEntityPurposeKey.GroupPlacementTemplate );
        }

        /// <summary>
        /// Sets the registration template placement groups, and unlinks empty deleted shared groups from this template's registration instances
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="groups">The groups.</param>
        public void SetRegistrationTemplatePlacementGroups( RegistrationTemplate registrationTemplate, List<Group> groups )
        {
            var currentRegistrationTemplatePlacementGroups = GetRegistrationTemplatePlacementGroups( registrationTemplate ).ToList();
            var deletedRegistrationTemplatePlacementGroups = currentRegistrationTemplatePlacementGroups.Where( a => !groups.Any( g => g.Id == a.Id ) ).ToList();

            var registrationInstanceService = new RegistrationInstanceService( this.Context as Rock.Data.RockContext );
            var registrationInstancesForTemplate = registrationInstanceService.Queryable().Where( a => a.RegistrationTemplateId == registrationTemplate.Id ).ToList();

            foreach ( var registrationInstance in registrationInstancesForTemplate )
            {
                var registrationInstancePlacementGroups = registrationInstanceService.GetRegistrationInstancePlacementGroups( registrationInstance ).ToList();
                var nonEmptyPlacementGroupIds = registrationInstanceService.GetRegistrationInstancePlacementGroups( registrationInstance ).Where( a => a.Members.Any() ).Select( a => a.Id ).ToList();
                foreach ( var deletedRegistrationTemplatePlacementGroup in deletedRegistrationTemplatePlacementGroups )
                {
                    if ( !nonEmptyPlacementGroupIds.Contains( deletedRegistrationTemplatePlacementGroup.Id ) )
                    {
                        registrationInstancePlacementGroups.Remove( deletedRegistrationTemplatePlacementGroup );
                    }
                }

                foreach ( var templateGroup in groups )
                {
                    if ( !registrationInstancePlacementGroups.Contains( templateGroup ) )
                    {
                        registrationInstancePlacementGroups.Add( templateGroup );
                    }
                }

                // set the instance registration groups to what they were minus any empty groups that are part of the shared groups in the Registration Template
                registrationInstanceService.SetRegistrationInstancePlacementGroups( registrationInstance, registrationInstancePlacementGroups );
            }

            this.SetRelatedToSourceEntity( registrationTemplate.Id, groups, RelatedEntityPurposeKey.GroupPlacementTemplate );
        }
    }
}
