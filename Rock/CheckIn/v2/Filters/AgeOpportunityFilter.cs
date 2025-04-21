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

using Rock.Enums.CheckIn;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on the person's age.
    /// </summary>
    internal class AgeOpportunityFilter : OpportunityFilter
    {
        #region Properties

        /// <summary>
        /// Gets the person age as an exact decimal value.
        /// </summary>
        /// <value>The person age.</value>
        private Lazy<decimal?> PersonAge { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AgeOpportunityFilter"/> class.
        /// </summary>
        public AgeOpportunityFilter()
        {
            // Converting from double to decimal is a shockingly expensive operation
            // when you are doing it hundreds of times. The actual microsecond amount
            // is small, but it was adding up to a few percentage points of the total
            // run time of this filter so we only do the conversion once now.
            PersonAge = new Lazy<decimal?>( () =>
            {
                return Person.Person.AgePrecise.HasValue
                    ? Convert.ToDecimal( Person.Person.AgePrecise.Value )
                    : ( decimal? ) null;
            } );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsGroupValid( GroupOpportunity group )
        {
            // We only handle GradeAndAgeMustMatch. Other behaviors are handled
            // by GradeAndAgeOpportunityFilter.
            if ( TemplateConfiguration.GradeAndAgeMatchingBehavior != GradeAndAgeMatchingMode.GradeAndAgeMustMatch )
            {
                return true;
            }

            var ageOrBirthdateRangeMatch = CheckAgeOrBirthdateMatches( PersonAge.Value,
                Person.Person.BirthDate?.DateTime,
                group.CheckInData,
                TemplateConfiguration.IsAgeRequired );

            if ( ageOrBirthdateRangeMatch == false )
            {
                return false;
            }

            // Either had a definitive yes, or a "we don't know" which also
            // means yes.
            return true;
        }

        /// <summary>
        /// Determines if either the age or birthdate restrictions on the group
        /// match the person.
        /// </summary>
        /// <param name="age">The age of the person or <c>null</c> if not known.</param>
        /// <param name="birthdate">The known birthdate of the person.</param>
        /// <param name="checkInData">The configuration for the group.</param>
        /// <param name="isAgeRequired">if set to <c>true</c> and no age is provided then <c>false</c> will be returned.</param>
        /// <returns><c>true</c> if either age or birthdate definitely match; <c>false</c> if either age or birthdate definitively don't match; <c>null</c> if a definitive answer could not be determined.</returns>
        internal static bool? CheckAgeOrBirthdateMatches( decimal? age, DateTime? birthdate, GroupConfigurationData checkInData, bool isAgeRequired )
        {
            var ageRangeMatch = CheckAgeMatches( age,
                checkInData.MinimumAge,
                checkInData.MaximumAge,
                isAgeRequired );

            if ( ageRangeMatch == true )
            {
                return true;
            }

            var birthdateRangeMatch = CheckBirthdateMatches( birthdate,
                checkInData.MinimumBirthdate,
                checkInData.MaximumBirthdate,
                isAgeRequired );

            if ( birthdateRangeMatch == true )
            {
                return true;
            }

            if ( ageRangeMatch == false || birthdateRangeMatch == false )
            {
                return false;
            }

            // All checks are indeterminate.
            return null;
        }

        /// <summary>
        /// Checks if the age matches the specified age range.
        /// </summary>
        /// <param name="age">The known age of the person..</param>
        /// <param name="minimumAge">The minimum age.</param>
        /// <param name="maximumAge">The maximum age.</param>
        /// <returns><c>true</c> if <paramref name="age"/> matches <paramref name="minimumAge"/> and <paramref name="maximumAge"/>, <c>false</c> otherwise.</returns>
        internal static bool CheckAgeMatches( decimal age, decimal? minimumAge, decimal? maximumAge )
        {
            if ( minimumAge.HasValue )
            {
                var minimumAgePrecision = minimumAge.Value.GetDecimalPrecision();
                var agePrecise = ( ( decimal? ) age ).Floor( minimumAgePrecision );

                if ( agePrecise < minimumAge.Value )
                {
                    return false;
                }
            }

            if ( maximumAge.HasValue )
            {
                var maximumAgePrecision = maximumAge.Value.GetDecimalPrecision();
                var agePrecise = ( ( decimal? ) age ).Floor( maximumAgePrecision );

                if ( agePrecise > maximumAge.Value )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the age matches the specified age range.
        /// </summary>
        /// <param name="age">The age of the person or <c>null</c> if not known.</param>
        /// <param name="minimumAge">The minimum age.</param>
        /// <param name="maximumAge">The maximum age.</param>
        /// <param name="isAgeRequired">if set to <c>true</c> and no age is provided then <c>false</c> will be returned.</param>
        /// <returns><c>true</c> if <paramref name="age"/> matches <paramref name="minimumAge"/> and <paramref name="maximumAge"/>, <c>false</c> if it doesn't or <c>null</c> it could not be determined.</returns>
        internal static bool? CheckAgeMatches( decimal? age, decimal? minimumAge, decimal? maximumAge, bool isAgeRequired )
        {
            if ( !minimumAge.HasValue && !maximumAge.HasValue )
            {
                return null;
            }

            if ( age.HasValue )
            {
                return CheckAgeMatches( age.Value, minimumAge, maximumAge );
            }
            else
            {
                // If age is required but we don't have one, then no match.
                return isAgeRequired ? false : ( bool? ) null;
            }
        }

        /// <summary>
        /// Checks if the birthdate matches the specified date range.
        /// </summary>
        /// <param name="birthdate">The known birthdate of the person.</param>
        /// <param name="minimumBirthdate">The minimum age.</param>
        /// <param name="maximumBirthdate">The maximum age.</param>
        /// <returns><c>true</c> if <paramref name="birthdate"/> matches <paramref name="minimumBirthdate"/> and <paramref name="maximumBirthdate"/>, <c>false</c> otherwise.</returns>
        internal static bool CheckBirthdateMatches( DateTime birthdate, DateTime? minimumBirthdate, DateTime? maximumBirthdate )
        {
            if ( minimumBirthdate.HasValue && birthdate < minimumBirthdate.Value )
            {
                return false;
            }

            if ( maximumBirthdate.HasValue && birthdate > maximumBirthdate.Value )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the birthdate matches the specified date range.
        /// </summary>
        /// <param name="birthdate">The known birthdate of the person.</param>
        /// <param name="minimumBirthdate">The minimum age.</param>
        /// <param name="maximumBirthdate">The maximum age.</param>
        /// <returns><c>true</c> if <paramref name="birthdate"/> matches <paramref name="minimumBirthdate"/> and <paramref name="maximumBirthdate"/>, <c>false</c> otherwise.</returns>
        /// <param name="isBirthdateRequired">if set to <c>true</c> and no age is provided then <c>false</c> will be returned.</param>
        internal static bool? CheckBirthdateMatches( DateTime? birthdate, DateTime? minimumBirthdate, DateTime? maximumBirthdate, bool isBirthdateRequired )
        {
            if ( !minimumBirthdate.HasValue && !maximumBirthdate.HasValue )
            {
                return null;
            }

            if ( birthdate.HasValue )
            {
                return CheckBirthdateMatches( birthdate.Value, minimumBirthdate, maximumBirthdate );
            }
            else
            {
                // If birthdate is required but we don't have one, then no match.
                return isBirthdateRequired ? false : ( bool? ) null;
            }
        }

        #endregion
    }
}
