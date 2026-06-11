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

using System.Collections.Generic;
using System.Linq;

using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for Rock.Model.RegistrationTemplate
    /// </summary>
    public partial class RegistrationTemplateService
    {
        /// <summary>
        /// Determines whether the Registration Template has any Registration Template Placements.
        /// </summary>
        /// <param name="registrationTemplateId">The registration template identifier.</param>
        /// <returns>
        ///   <c>true</c> if [has registration template placements] [the specified registration template identifier]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasRegistrationTemplatePlacements( int registrationTemplateId )
        {
            return Queryable().Any( r => r.Id == registrationTemplateId && r.Placements.Any() );
        }

        /// <summary>
        /// Creates a registrant eligibility evaluator based on the eligibility settings of the specified registration template.
        /// </summary>
        /// <remarks>
        /// If the registration template specifies an eligibility data view, the evaluator will
        /// use it to efficiently determine eligible registrants. Otherwise, all registrants are considered eligible.
        /// This method does not modify the registration template or its settings.
        /// </remarks>
        /// <param name="registrationTemplateId">The identifier of the registration template whose eligibility settings are used to determine registrant eligibility. Cannot be null.</param>
        /// <returns>
        /// A RegistrantEligibilityEvaluator instance configured according to the eligibility settings of the registration template.
        /// If no eligibility settings are defined, the evaluator will treat all registrants as eligible.
        /// </returns>
        public RegistrantEligibilityEvaluator GetRegistrantEligibility( int registrationTemplateId )
        {
            return GetRegistrantEligibility( Get( registrationTemplateId ) );
        }

        /// <summary>
        /// Creates a registrant eligibility evaluator based on the eligibility settings of the specified registration template.
        /// </summary>
        /// <remarks>
        /// If the registration template specifies an eligibility data view, the evaluator will
        /// use it to efficiently determine eligible registrants. Otherwise, all registrants are considered eligible.
        /// This method does not modify the registration template or its settings.
        /// </remarks>
        /// <param name="registrationTemplate">The registration template whose eligibility settings are used to determine registrant eligibility. Cannot be null.</param>
        /// <returns>
        /// A RegistrantEligibilityEvaluator instance configured according to the eligibility settings of the registration template.
        /// If no eligibility settings are defined, the evaluator will treat all registrants as eligible.
        /// </returns>
        public RegistrantEligibilityEvaluator GetRegistrantEligibility( RegistrationTemplate registrationTemplate )
        {
            var registrantEligibilitySettings = registrationTemplate.GetRegistrantEligibilitySettingsOrNull();
            if ( registrantEligibilitySettings == null )
            {
                // Return an evaluator that will treat all registrants as eligible
                // if there are no eligibility settings configured for the registration template.
                return new RegistrantEligibilityEvaluator( registrationTemplate, null, null );
            }

            // Prepare the query of eligible people from the data view if an eligibility data view is configured for the registration template.
            // This will allow the RegistrantEligibilityEvaluator to efficiently determine if a registrant is eligible
            // by checking if their PersonId is in the set of eligible PersonIds from the data view.
            IQueryable<Person> eligibleDataViewPersonQuery = null;
            if ( registrantEligibilitySettings.EligibilityDataViewGuid.HasValue )
            {
                var rockContext = ( RockContext ) Context;
                var dataView = new DataViewService( rockContext ).Get( registrantEligibilitySettings.EligibilityDataViewGuid.Value );

                if ( dataView != null )
                {
                    var personService = new PersonService( rockContext );
                    eligibleDataViewPersonQuery = personService.GetQueryUsingDataView( dataView );
                }
            }

            return new RegistrantEligibilityEvaluator( registrationTemplate, registrantEligibilitySettings, eligibleDataViewPersonQuery );
        }
    }
}
