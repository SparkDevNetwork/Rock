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

namespace Rock.Model
{
    /// <summary>
    /// Evaluates registrant eligibility based on configured settings and optional Data View filtering.
    /// </summary>
    public sealed class RegistrantEligibilityEvaluator
    {
        private readonly RegistrationTemplate _registrationTemplate;
        private readonly RegistrationTemplate.RegistrantEligibilitySettings _registrantEligibilitySettings;
        private readonly IQueryable<Person> _eligibleDataViewPersonQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantEligibilityEvaluator"/> class.
        /// </summary>
        /// <param name="registrantEligibilitySettings">
        /// The eligibility settings defined on the <see cref="RegistrationTemplate"/>.
        /// If <c>null</c>, all registrants will be considered eligible.
        /// </param>
        /// <param name="eligibleDataViewPersonQuery">
        /// An <see cref="IQueryable{Person}"/> representing the people that match the configured
        /// eligibility Data View. If <c>null</c>, no Data View filtering will be applied.
        /// </param>
        /// <remarks>
        /// This constructor is intended to be called by <see cref="RegistrationTemplateService.GetRegistrantEligibility"/>
        /// after resolving any configured eligibility settings and associated Data View query.
        /// DO NOT CALL IT DIRECTLY.
        /// </remarks>
        internal RegistrantEligibilityEvaluator( RegistrationTemplate registrationTemplate, RegistrationTemplate.RegistrantEligibilitySettings registrantEligibilitySettings, IQueryable<Person> eligibleDataViewPersonQuery )
        {
            _registrationTemplate = registrationTemplate;
            _registrantEligibilitySettings = registrantEligibilitySettings;
            _eligibleDataViewPersonQuery = eligibleDataViewPersonQuery;
        }

        /// <summary>
        /// Determines whether the specified registrant meets the eligibility criteria defined by the current settings.
        /// </summary>
        /// <param name="registrantPerson">The person to evaluate for eligibility. Returns <see langword="false"/> if <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the registrant meets all eligibility requirements; otherwise, <see langword="false"/>.</returns>
        public bool Evaluate( Person registrantPerson )
        {
            return Evaluate( registrantPerson, out var _ );
        }

        /// <summary>
        /// Determines whether the specified registrant meets the eligibility criteria defined by the current settings.
        /// </summary>
        /// <param name="registrantPerson">The person to evaluate for eligibility. Returns <see langword="false"/> if <see langword="null"/>.</param>
        /// <param name="error">The first friendly error explaining why the person is not eligible.</param>
        /// <returns><see langword="true"/> if the registrant meets all eligibility requirements; otherwise, <see langword="false"/>.</returns>
        public bool Evaluate( Person registrantPerson, out string error )
        {
            error = null;

            if ( registrantPerson == null )
            {
                // No registrant provided, so not eligible.
                error = $"{registrantPerson.FullName} does not meet eligibility requirements for this {_registrationTemplate.RegistrationTerm}.";
                return false;
            }

            if ( _registrantEligibilitySettings == null )
            {
                // No eligibility settings provided, so everyone is eligible.
                return true;
            }

            if ( _registrantEligibilitySettings.MinimumAge.HasValue || _registrantEligibilitySettings.MaximumAge.HasValue )
            {
                var age = registrantPerson.Age;

                if ( _registrantEligibilitySettings.MinimumAge.HasValue && ( !age.HasValue || age.Value < _registrantEligibilitySettings.MinimumAge.Value ) )
                {
                    error = $"{registrantPerson.FullName} does not meet the minimum age requirement for this {_registrationTemplate.RegistrationTerm} ({_registrantEligibilitySettings.MinimumAge.Value} years old or older).";
                    return false;
                }

                if ( _registrantEligibilitySettings.MaximumAge.HasValue && ( !age.HasValue || age.Value > _registrantEligibilitySettings.MaximumAge.Value ) )
                {
                    error = $"{registrantPerson.FullName} does not meet the maximum age requirement for this {_registrationTemplate.RegistrationTerm} ({_registrantEligibilitySettings.MaximumAge.Value} years old or younger).";
                    return false;
                }
            }

            if ( _registrantEligibilitySettings.MinimumGradeOffset.HasValue || _registrantEligibilitySettings.MaximumGradeOffset.HasValue )
            {
                var yearsUntilGraduation = registrantPerson.GradeOffset;
                var maxYearsUntilGraduation = _registrantEligibilitySettings.MaximumGradeOffset;
                var minYearsUntilGraduation = _registrantEligibilitySettings.MinimumGradeOffset;

                if ( maxYearsUntilGraduation.HasValue && ( !yearsUntilGraduation.HasValue || yearsUntilGraduation.Value > maxYearsUntilGraduation.Value ) )
                {
                    error = $"{registrantPerson.FullName} does not meet the minimum grade requirement for this {_registrationTemplate.RegistrationTerm}.";
                    return false;
                }
                if ( minYearsUntilGraduation.HasValue && ( !yearsUntilGraduation.HasValue || yearsUntilGraduation.Value < minYearsUntilGraduation.Value ) )
                {
                    error = $"{registrantPerson.FullName} does not meet the maximum grade requirement for this {_registrationTemplate.RegistrationTerm}.";
                    return false;
                }
            }

            if ( _registrantEligibilitySettings.AgeClassification.HasValue && registrantPerson.AgeClassification != _registrantEligibilitySettings.AgeClassification.Value )
            {
                error = $"{registrantPerson.FullName} does not meet the age requirement for this {_registrationTemplate.RegistrationTerm} ({_registrantEligibilitySettings.AgeClassification.GetDisplayName()}).";
                return false;
            }

            if ( _registrantEligibilitySettings.Gender.HasValue && registrantPerson.Gender != _registrantEligibilitySettings.Gender.Value )
            {
                error = $"{registrantPerson.FullName} does not meet the gender requirement for this {_registrationTemplate.RegistrationTerm} ({_registrantEligibilitySettings.Gender.Value.GetDisplayName()}).";
                return false;
            }

            // Null means no DataView filtering is configured.
            if ( _eligibleDataViewPersonQuery != null && !_eligibleDataViewPersonQuery.Any( p => p.Id == registrantPerson.Id ) )
            {
                // Return a generic message if the person doesn't meet the Data View criteria since we don't want to expose any details about the Data View filtering that is being applied.
                error = $"{registrantPerson.FullName} does not meet eligibility requirements for this {_registrationTemplate.RegistrationTerm}.";
                return false;
            }

            // All checks passed, so the registrant is eligible.
            return true;
        }
    }
}
