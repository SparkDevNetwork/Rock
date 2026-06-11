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

using System;
using System.Linq;

using Rock.Model.Event.RegistrationTemplate.Options;

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
        /// <param name="registrationTemplate">The source registration template to use for evaluation.</param>
        /// <param name="registrantEligibilitySettings">
        /// The eligibility settings defined on the <see cref="RegistrationTemplate"/>.
        /// If <c>null</c>, all registrants will be considered eligible.
        /// </param>
        /// <param name="eligibleDataViewPersonQuery">
        /// An <see cref="IQueryable{Person}"/> representing the people that match the configured
        /// eligibility Data View. If <c>null</c>, no Data View filtering will be applied.
        /// </param>
        /// <remarks>
        /// This constructor is intended to be called by RegistrationTemplateService.GetRegistrantEligibility
        /// after resolving any configured eligibility settings and associated Data View query.
        /// DO NOT CALL IT DIRECTLY.
        /// </remarks>
        internal RegistrantEligibilityEvaluator( RegistrationTemplate registrationTemplate, RegistrationTemplate.RegistrantEligibilitySettings registrantEligibilitySettings, IQueryable<Person> eligibleDataViewPersonQuery)
        {
            _registrationTemplate = registrationTemplate;
            _registrantEligibilitySettings = registrantEligibilitySettings;
            _eligibleDataViewPersonQuery = eligibleDataViewPersonQuery;
        }

        /// <summary>
        /// Determines whether the specified registrant meets the eligibility criteria defined by
        /// the current settings using <see cref="RegistrantEligibilityEvaluationMode.Strict"/> mode.
        /// </summary>
        /// <param name="registrantPerson">The person to evaluate for eligibility. Returns <see langword="false"/> if <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the registrant meets all eligibility requirements; otherwise, <see langword="false"/>.</returns>
        public bool Evaluate( Person registrantPerson )
        {
            return Evaluate( registrantPerson, options: null, out var _ );
        }

        /// <summary>
        /// Determines whether the specified registrant meets the eligibility criteria defined by
        /// the current settings using <see cref="RegistrantEligibilityEvaluationMode.Strict"/> mode.
        /// </summary>
        /// <param name="registrantPerson">The person to evaluate for eligibility. Returns <see langword="false"/> if <see langword="null"/>.</param>
        /// <param name="error">The first friendly error explaining why the person is not eligible.</param>
        /// <returns><see langword="true"/> if the registrant meets all eligibility requirements; otherwise, <see langword="false"/>.</returns>
        public bool Evaluate( Person registrantPerson, out string error )
        {
            return Evaluate( registrantPerson, options: null, out error );
        }

        /// <summary>
        /// Determines whether the specified registrant meets the eligibility criteria defined by
        /// the current settings, using the supplied <paramref name="options"/> to control how
        /// missing-data scenarios are handled.
        /// </summary>
        /// <param name="registrantPerson">The person to evaluate for eligibility. Returns <see langword="false"/> if <see langword="null"/>.</param>
        /// <param name="options">
        /// Options that control how the evaluator treats missing data on the person. If
        /// <see langword="null"/>, defaults to <see cref="RegistrantEligibilityEvaluationMode.Strict"/>.
        /// </param>
        /// <returns><see langword="true"/> if the registrant meets all eligibility requirements; otherwise, <see langword="false"/>.</returns>
        public bool Evaluate( Person registrantPerson, RegistrantEligibilityEvaluationOptions options )
        {
            return Evaluate( registrantPerson, options, out var _ );
        }

        /// <summary>
        /// Determines whether the specified registrant meets the eligibility criteria defined by
        /// the current settings, using the supplied <paramref name="options"/> to control how
        /// missing-data scenarios are handled.
        /// </summary>
        /// <param name="registrantPerson">The person to evaluate for eligibility. Returns <see langword="false"/> if <see langword="null"/>.</param>
        /// <param name="options">
        /// Options that control how the evaluator treats missing data on the person. If
        /// <see langword="null"/>, defaults to <see cref="RegistrantEligibilityEvaluationMode.Strict"/>.
        /// </param>
        /// <param name="error">The first friendly error explaining why the person is not eligible.</param>
        /// <returns><see langword="true"/> if the registrant meets all eligibility requirements; otherwise, <see langword="false"/>.</returns>
        public bool Evaluate( Person registrantPerson, RegistrantEligibilityEvaluationOptions options, out string error )
        {
            error = null;

            if ( registrantPerson == null )
            {
                // No registrant provided, so not eligible.
                error = $"{registrantPerson.FullName} does not meet eligibility requirements for this {_registrationTemplate.RegistrationTerm}.";
                return false;
            }

            // Any eligibility settings?
            if ( _registrantEligibilitySettings == null )
            {
                // No eligibility settings provided, so everyone is eligible.
                return true;
            }

            // Resolve the evaluation mode. A null options instance falls back to the default
            // (Strict), preserving the behavior of the parameterless Evaluate overloads.
            var evaluationMode = ( options ?? new RegistrantEligibilityEvaluationOptions() ).Mode;

            // Minimum Age
            if ( _registrantEligibilitySettings.MinimumAge.HasValue )
            {
                var minAgeError = $"{registrantPerson.FullName} does not meet the minimum age requirement for this {_registrationTemplate.RegistrationTerm} ({_registrantEligibilitySettings.MinimumAge.Value} years old or older).";

                if ( !registrantPerson.Age.HasValue )
                {
                    // Age is required to evaluate this rule. Strict mode rejects, Lax mode skips.
                    if ( evaluationMode == RegistrantEligibilityEvaluationMode.Strict )
                    {
                        error = minAgeError;
                        return false;
                    }
                }
                else if ( registrantPerson.Age.Value < _registrantEligibilitySettings.MinimumAge.Value )
                {
                    error = minAgeError;
                    return false;
                }
            }

            // Maximum Age
            if ( _registrantEligibilitySettings.MaximumAge.HasValue )
            {
                var isMaxAgeAnInteger = _registrantEligibilitySettings.MaximumAge.Value.IsInteger();
                var maxAgeError = $"{registrantPerson.FullName} does not meet the maximum age requirement for this {_registrationTemplate.RegistrationTerm} ({_registrantEligibilitySettings.MaximumAge.Value} years old or younger).";

                if ( !registrantPerson.Age.HasValue )
                {
                    // Age is required to evaluate this rule. Strict mode rejects, Lax mode skips.
                    if ( evaluationMode == RegistrantEligibilityEvaluationMode.Strict )
                    {
                        error = maxAgeError;
                        return false;
                    }
                }
                else if ( isMaxAgeAnInteger )
                {
                    // If max age is an integer number of years old, then treat it as "up to and including that age".
                    // So if max age is 18, then a registrant who is 18 years and 11 months old would still be eligible,
                    // but a registrant who is 19 years old would not be eligible.
                    if ( registrantPerson.Age.Value >= _registrantEligibilitySettings.MaximumAge.Value + 1 )
                    {
                        error = maxAgeError;
                        return false;
                    }
                }
                else
                {
                    // Otherwise, if max age is not an integer, then treat it as "up to that specific age".
                    // So if max age is 18.5, then a registrant who is 18 years and 6 months old would still be eligible,
                    // but a registrant who is 18 years and 7 months old would not be eligible.
                    if ( registrantPerson.Age.Value > _registrantEligibilitySettings.MaximumAge.Value )
                    {
                        error = maxAgeError;
                        return false;
                    }
                }
            }

            // Age Classification
            if ( _registrantEligibilitySettings.AgeClassification.HasValue )
            {
                // Typically age classification is set in the person save hook,
                // but that won't run until a running transaction is committed.
                // Resolve it here using the same logic so we can determine registrant eligibility when a new person is created for a registration.
                var resolvedAgeClassification = registrantPerson.AgeClassification;
                if ( resolvedAgeClassification == AgeClassification.Unknown && registrantPerson.Age.HasValue )
                {
                    if ( registrantPerson.Age < 18 )
                    {
                        resolvedAgeClassification = AgeClassification.Child;
                    }
                    else
                    {
                        resolvedAgeClassification = AgeClassification.Adult;
                    }
                }

                var ageClassificationError = $"{registrantPerson.FullName} does not meet the age requirement for this {_registrationTemplate.RegistrationTerm} ({_registrantEligibilitySettings.AgeClassification.GetDisplayName()}).";

                if ( resolvedAgeClassification == AgeClassification.Unknown )
                {
                    // Age classification could not be determined (no explicit value and no age to derive it).
                    // Strict mode rejects, Lax mode skips this rule.
                    if ( evaluationMode == RegistrantEligibilityEvaluationMode.Strict )
                    {
                        error = ageClassificationError;
                        return false;
                    }
                }
                else if ( resolvedAgeClassification != _registrantEligibilitySettings.AgeClassification.Value )
                {
                    error = ageClassificationError;
                    return false;
                }
            }

            // Minimum Grade (i.e. maximum years until graduation a.k.a. maximum grade offset)
            if ( _registrantEligibilitySettings.MaximumGradeOffset.HasValue )
            {
                var minGradeError = $"{registrantPerson.FullName} does not meet the minimum grade requirement for this {_registrationTemplate.RegistrationTerm}.";

                if ( !registrantPerson.GradeOffset.HasValue )
                {
                    // Grade is required to evaluate this rule. Strict mode rejects, Lax mode skips.
                    if ( evaluationMode == RegistrantEligibilityEvaluationMode.Strict )
                    {
                        error = minGradeError;
                        return false;
                    }
                }
                else if ( registrantPerson.GradeOffset.Value > _registrantEligibilitySettings.MaximumGradeOffset.Value )
                {
                    error = minGradeError;
                    return false;
                }
            }

            // Maximum Grade (i.e. minimum years until graduation a.k.a. minimum grade offset)
            if ( _registrantEligibilitySettings.MinimumGradeOffset.HasValue )
            {
                var maxGradeError = $"{registrantPerson.FullName} does not meet the maximum grade requirement for this {_registrationTemplate.RegistrationTerm}.";

                if ( !registrantPerson.GradeOffset.HasValue )
                {
                    // Grade is required to evaluate this rule. Strict mode rejects, Lax mode skips.
                    if ( evaluationMode == RegistrantEligibilityEvaluationMode.Strict )
                    {
                        error = maxGradeError;
                        return false;
                    }
                }
                else if ( registrantPerson.GradeOffset.Value < _registrantEligibilitySettings.MinimumGradeOffset.Value )
                {
                    error = maxGradeError;
                    return false;
                }
            }

            // Gender
            if ( _registrantEligibilitySettings.Gender.HasValue )
            {
                var genderError = $"{registrantPerson.FullName} does not meet the gender requirement for this {_registrationTemplate.RegistrationTerm} ({_registrantEligibilitySettings.Gender.Value.GetDisplayName()}).";

                if ( registrantPerson.Gender == Gender.Unknown )
                {
                    // Gender is required to evaluate this rule. Strict mode rejects, Lax mode skips.
                    if ( evaluationMode == RegistrantEligibilityEvaluationMode.Strict )
                    {
                        error = genderError;
                        return false;
                    }
                }
                else if ( registrantPerson.Gender != _registrantEligibilitySettings.Gender.Value )
                {
                    error = genderError;
                    return false;
                }
            }

            // Data View
            // The Data View check is based on concrete membership in a configured query rather than
            // missing-data semantics, so it is enforced regardless of the evaluation mode.
            if ( _eligibleDataViewPersonQuery != null )
            {
                if ( !_eligibleDataViewPersonQuery.Any( p => p.Id == registrantPerson.Id ) )
                {
                    // Return a generic message if the person doesn't meet the Data View criteria since we don't want to expose any details about the Data View filtering that is being applied.
                    error = $"{registrantPerson.FullName} does not meet eligibility requirements for this {_registrationTemplate.RegistrationTerm}.";
                    return false;
                }
            }

            // All checks passed, so the registrant is eligible.
            return true;
        }
    }
}
