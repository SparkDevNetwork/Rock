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

using Rock.Enums.CheckIn;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on the person's grade.
    /// </summary>
    internal class GradeOpportunityFilter : OpportunityFilter
    {
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

            var gradeRangeMatch = CheckGradeMatches( Person.Person.GradeOffset,
                group.CheckInData.MinimumGradeOffset,
                group.CheckInData.MaximumGradeOffset,
                TemplateConfiguration.IsGradeRequired );

            return !gradeRangeMatch.HasValue || gradeRangeMatch == true;
        }

        /// <summary>
        /// Checks if the grade matches the specified grade range.
        /// </summary>
        /// <param name="gradeOffset">The known grade offset of the person..</param>
        /// <param name="minimumGradeOffset">The minimum grade offset.</param>
        /// <param name="maximumGradeOffset">The maximum grade offset.</param>
        /// <returns><c>true</c> if <paramref name="gradeOffset"/> matches <paramref name="minimumGradeOffset"/> and <paramref name="maximumGradeOffset"/>, <c>false</c> otherwise.</returns>
        internal static bool CheckGradeMatches( int gradeOffset, int? minimumGradeOffset, int? maximumGradeOffset )
        {
            if ( minimumGradeOffset.HasValue && gradeOffset < minimumGradeOffset.Value )
            {
                return false;
            }

            if ( maximumGradeOffset.HasValue && gradeOffset > maximumGradeOffset.Value )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the grade matches the specified grade range.
        /// </summary>
        /// <param name="gradeOffset">The known grade offset of the person..</param>
        /// <param name="minimumGradeOffset">The minimum grade offset.</param>
        /// <param name="maximumGradeOffset">The maximum grade offset.</param>
        /// <param name="isGradeRequired">if set to <c>true</c> and no grade is provided then <c>false</c> will be returned.</param>
        /// <returns><c>true</c> if <paramref name="gradeOffset"/> matches <paramref name="minimumGradeOffset"/> and <paramref name="maximumGradeOffset"/>, <c>false</c> if it doesn't or <c>null</c> it could not be determined.</returns>
        internal static bool? CheckGradeMatches( int? gradeOffset, int? minimumGradeOffset, int? maximumGradeOffset, bool isGradeRequired )
        {
            if ( !minimumGradeOffset.HasValue && !maximumGradeOffset.HasValue )
            {
                return null;
            }

            if ( gradeOffset.HasValue )
            {
                return CheckGradeMatches( gradeOffset.Value, minimumGradeOffset, maximumGradeOffset );
            }
            else
            {
                // If grade is required but we don't have one, then no match.
                return isGradeRequired ? false : ( bool? ) null;
            }
        }

        #endregion
    }
}
