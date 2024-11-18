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
using System.Collections.Generic;

using Rock.Enums.CheckIn;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on the person's grade
    /// and age as a combined set.
    /// </summary>
    internal class GradeAndAgeOpportunityFilter : OpportunityFilter
    {
        #region Fields

        /// <summary>
        /// The identifiers of the groups that were matched by grade. This will
        /// be <c>null</c> if no matched groups by grade were found. This value
        /// will only be set if <see cref="TemplateConfigurationData.GradeAndAgeMatchingBehavior"/>
        /// is set to <see cref="GradeAndAgeMatchingMode.PrioritizeGradeOverAge"/>.
        /// </summary>
        private List<string> _matchedByGradeGroupIds;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the person age as an exact decimal value.
        /// </summary>
        /// <value>The person age.</value>
        private Lazy<decimal?> PersonAge { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GradeAndAgeOpportunityFilter"/> class.
        /// </summary>
        public GradeAndAgeOpportunityFilter()
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
        public override void FilterGroups( OpportunityCollection opportunities )
        {
            base.FilterGroups( opportunities );

            // If one or more groups were matched by grade then remove all
            // groups that were not matched by grade.
            if ( TemplateConfiguration.GradeAndAgeMatchingBehavior == GradeAndAgeMatchingMode.PrioritizeGradeOverAge )
            {
                if ( _matchedByGradeGroupIds != null )
                {
                    opportunities.Groups.RemoveAll( g => !_matchedByGradeGroupIds.Contains( g.Id ) );
                }
            }
        }

        /// <inheritdoc/>
        public override bool IsGroupValid( GroupOpportunity group )
        {
            // We don't handle GradeAndAgeMustMatch. This behavior is handled
            // by GradeOpportunityFilter and AgeOpportunityFilter.
            if ( TemplateConfiguration.GradeAndAgeMatchingBehavior == GradeAndAgeMatchingMode.GradeAndAgeMustMatch )
            {
                return true;
            }

            var gradeRangeMatch = GradeOpportunityFilter.CheckGradeMatches( Person.Person.GradeOffset,
                group.CheckInData.MinimumGradeOffset,
                group.CheckInData.MaximumGradeOffset,
                TemplateConfiguration.IsGradeRequired );

            if ( gradeRangeMatch.HasValue )
            {
                // If we are configured such that grade matches take priority
                // over other matches then we need to track groups that were
                // matched by grade.
                if ( TemplateConfiguration.GradeAndAgeMatchingBehavior == GradeAndAgeMatchingMode.PrioritizeGradeOverAge )
                {
                    if ( _matchedByGradeGroupIds == null )
                    {
                        // Allocate with a size of 4 to minimize unwanted resizes.
                        _matchedByGradeGroupIds = new List<string>( 4 );
                    }

                    _matchedByGradeGroupIds.Add( group.Id );
                }

                return gradeRangeMatch.Value;
            }

            // Grade match was indeterminate. Check age now.

            var ageOrBirthdateRangeMatch = AgeOpportunityFilter.CheckAgeOrBirthdateMatches( PersonAge.Value,
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

        #endregion
    }
}
