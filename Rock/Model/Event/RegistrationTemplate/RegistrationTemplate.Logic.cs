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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RegistrationTemplate
    {
        /// <summary>
        /// Gets or sets the collection of payment plan frequency value IDs from which a registrant can select.
        /// <para>
        /// This is a convenient property for working with the IDs as a collection instead of the <see cref="PaymentPlanFrequencyValueIds"/> property directly.
        /// Updates made to <see cref="PaymentPlanFrequencyValueIds"/> will require getting this property again.
        /// </para>
        /// </summary>
        /// <value>
        /// The collection of payment plan frequency value IDs.
        /// </value>
        [NotMapped]
        public ICollection<int> PaymentPlanFrequencyValueIdsCollection
        {
            get
            {
                var ids = new ObservableCollection<int>( this.PaymentPlanFrequencyValueIds?
                    .Split( ',' )
                    .Select( id => id.Trim().AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select(id => id.Value) ?? Enumerable.Empty<int>() );

                // Update the underlying string property whenever an ID is added to or removed from the collection.
                ids.CollectionChanged += ( sender, e ) =>
                {
                    this.PaymentPlanFrequencyValueIdsCollection = ids;
                };

                return ids;
            }
            set
            {
                this.PaymentPlanFrequencyValueIds = value == null ? null : string.Join( ",", value );
            }
        }

        /// <summary>
        /// Retrieves the registrant eligibility settings for the current instance or <c>null</c> if not configured.
        /// </summary>
        /// <returns>A <see cref="RegistrantEligibilitySettings"/> object containing the registrant eligibility settings.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public RegistrantEligibilitySettings GetRegistrantEligibilitySettingsOrNull()
        {
            return this.GetAdditionalSettingsOrNull<RegistrantEligibilitySettings>();
        }

        /// <summary>
        /// Sets the Registrant Eligibility Settings for this Registration Template.
        /// </summary>
        /// <param name="registrantEligibilitySettings"></param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public void SetRegistrantEligibilitySettings( RegistrantEligibilitySettings registrantEligibilitySettings )
        {
            var hasEligibilitySettings =
                registrantEligibilitySettings != null
                && (
                    registrantEligibilitySettings.MaximumAge.HasValue
                    || registrantEligibilitySettings.MinimumAge.HasValue
                    || registrantEligibilitySettings.AgeClassification.HasValue
                    || registrantEligibilitySettings.MaximumGradeOffset.HasValue
                    || registrantEligibilitySettings.MinimumGradeOffset.HasValue
                    || registrantEligibilitySettings.Gender.HasValue
                    || registrantEligibilitySettings.EligibilityDataViewGuid.HasValue
                );

            if ( !hasEligibilitySettings )
            {
                // Remove the settings if there are no eligibility settings defined. 
                this.RemoveAdditionalSettings<RegistrantEligibilitySettings>();
            }
            else
            {
                // Normalize the ranges so minimums are less than maximums.
                if ( registrantEligibilitySettings.MinimumAge > registrantEligibilitySettings.MaximumAge )
                {
                    (registrantEligibilitySettings.MaximumAge, registrantEligibilitySettings.MinimumAge)
                        = (registrantEligibilitySettings.MinimumAge, registrantEligibilitySettings.MaximumAge);
                }

                if ( registrantEligibilitySettings.MinimumGradeOffset > registrantEligibilitySettings.MaximumGradeOffset )
                {
                    ( registrantEligibilitySettings.MinimumGradeOffset, registrantEligibilitySettings.MaximumGradeOffset )
                        = ( registrantEligibilitySettings.MaximumGradeOffset, registrantEligibilitySettings.MinimumGradeOffset );
                }

                this.SetAdditionalSettings( registrantEligibilitySettings );
            }
        }

        public partial class RegistrantEligibilitySettings
        {
            /// <summary>
            /// Gets the effective minimum age based on the current age classification and minimum age settings.
            /// </summary>
            public decimal? GetEffectiveMinimumAge()
            {
                if ( AgeClassification == Model.AgeClassification.Adult && ( !MinimumAge.HasValue || MinimumAge < 18m ) )
                {
                    return 18m;
                }

                return MinimumAge;
            }
             
            /// <summary>
            /// Gets the effective maximum age based on the current age classification and maximum age settings.
            /// </summary>
            public decimal? GetEffectiveMaximumAge()
            {
                if ( AgeClassification == Model.AgeClassification.Child && ( !MaximumAge.HasValue || MaximumAge >= 18m ) )
                {
                    return 17m;
                }

                return MaximumAge;
            }
           
            /// <summary>
            /// Gets the earliest effective birth date that satisfies the minimum age or age classification requirements, if specified.
            /// </summary>
            public DateTime? GetEffectiveMinimumAgeBirthDate()
            {
                var effectiveMinimumAge = GetEffectiveMinimumAge();
                if ( !effectiveMinimumAge.HasValue )
                {
                    return null;
                }

                return GetBirthDateFromFractionalAge( effectiveMinimumAge.Value );
            }

            /// <summary>
            /// Gets the latest effective birth date that satisfies the maximum age or age classification requirements, if specified.
            /// </summary>
            public DateTime? GetEffectiveMaximumAgeBirthDate()
            {
                var effectiveMaximumAge = GetEffectiveMaximumAge();
                if ( !effectiveMaximumAge.HasValue )
                {
                    return null;
                }

                if ( effectiveMaximumAge.Value.IsInteger() )
                {
                    return GetBirthDateFromFractionalAge( effectiveMaximumAge.Value + 1 ).AddDays( 1 );
                }

                return GetBirthDateFromFractionalAge( effectiveMaximumAge.Value );
            }

            /// <summary>
            /// Gets the latest birth date that satisfies the maximum age requirement, if specified.
            /// </summary>
            public DateTime? GetMaximumAgeBirthDate()
            {
                if ( !MaximumAge.HasValue )
                {
                    return null;
                }

                if ( MaximumAge.Value.IsInteger() )
                {
                    return GetBirthDateFromFractionalAge( MaximumAge.Value + 1 ).AddDays( 1 );
                }

                return GetBirthDateFromFractionalAge( MaximumAge.Value );
            }

            /// <summary>
            /// Gets the earliest birth date that satisfies the minimum age requirement, if specified.
            /// </summary>
            public DateTime? GetMinimumAgeBirthDate()
            {
                if ( !MinimumAge.HasValue )
                {
                    return null;
                }

                return GetBirthDateFromFractionalAge( MinimumAge.Value );
            }

            /// <summary>
            /// Retrieves the list of eligible school grades based on the configured minimum and maximum grade offsets.
            /// </summary>
            /// <remarks>
            /// The returned list includes only active grades within the specified grade offset range.
            /// This method is typically used to determine which grades should be available for selection in registration scenarios.
            /// </remarks>
            /// <returns>
            /// A list of <see cref="DefinedValueCache"/> objects representing the valid grades for new registrants,
            /// or <see langword="null"/> if no grade criteria is configured.
            /// </returns>
            public List<DefinedValueCache> GetGradeDefinedValues()
            {
                // Build the list of eligible grades based on the configured grade classifications.
                // This is necessary because the UI needs to know which grades are valid for new registrants.
                if ( !MinimumGradeOffset.HasValue && !MaximumGradeOffset.HasValue )
                {
                    return null;
                }

                var gradesDefinedType = DefinedTypeCache.Get( SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );

                var grades = new List<DefinedValueCache>();

                foreach ( var grade in gradesDefinedType.DefinedValues.Where( dv => dv.IsActive ).OrderBy( dv => dv.Order ) )
                {
                    var gradeOffset = grade.Value.AsIntegerOrNull();

                    if ( gradeOffset.HasValue )
                    {
                        var isAboveMinimumGrade = !MinimumGradeOffset.HasValue || gradeOffset.Value >= MinimumGradeOffset.Value;
                        var isBelowMaximumGrade = !MaximumGradeOffset.HasValue || gradeOffset.Value <= MaximumGradeOffset.Value;

                        if ( isAboveMinimumGrade && isBelowMaximumGrade )
                        {
                            grades.Add( grade );
                        }
                    }
                }

                return grades;
            }

            private static DateTime GetBirthDateFromFractionalAge( decimal age )
            {
                var today = RockDateTime.Today;

                if ( age <= 0 )
                {
                    return today;
                }

                var years = ( int )Math.Floor( age );
                var fraction = age - years;

                // Subtract whole years
                var baseDate = today.AddYears( -years );

                // Determine actual calendar year span
                var nextYearDate = baseDate.AddYears( 1 );

                var spanDays = ( nextYearDate - baseDate ).Days;

                // Subtract fractional portion
                var fractionalDays = ( int )Math.Floor( spanDays * fraction );

                return baseDate.AddDays( -fractionalDays ).Date;
            }
        }
    }
}
