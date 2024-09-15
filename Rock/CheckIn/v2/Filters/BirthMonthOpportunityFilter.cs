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

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on the person's
    /// birth month.
    /// </summary>
    internal class BirthMonthOpportunityFilter : OpportunityFilter
    {
        #region Methods

        /// <inheritdoc/>
        public override bool IsGroupValid( GroupOpportunity group )
        {
            // If there is no filter, then it matches.
            if ( !group.CheckInData.MinimumBirthMonth.HasValue && !group.CheckInData.MaximumBirthMonth.HasValue )
            {
                return true;
            }

            // Must have a birth month to match filter.
            if ( !Person.Person.BirthMonth.HasValue )
            {
                return false;
            }

            return CheckBirthMonthMatches( Person.Person.BirthMonth.Value,
                group.CheckInData.MinimumBirthMonth,
                group.CheckInData.MaximumBirthMonth );
        }


        /// <summary>
        /// Checks if the birth monthmatches the specified month range.
        /// </summary>
        /// <param name="birthMonth">The known birth month of the person.</param>
        /// <param name="minimumMonth">The minimum birth month.</param>
        /// <param name="maximumMonth">The maximum birth month.</param>
        /// <returns><c>true</c> if <paramref name="birthMonth"/> matches <paramref name="minimumMonth"/> and <paramref name="maximumMonth"/>, <c>false</c> otherwise.</returns>
        protected static bool CheckBirthMonthMatches( int birthMonth, int? minimumMonth, int? maximumMonth )
        {
            if ( minimumMonth.HasValue && maximumMonth.HasValue && maximumMonth.Value < minimumMonth.Value )
            {
                // If both are provided, then allow for a range like Nov - Feb
                // which would include Nov, Dec, Jan and Feb. This means we
                // basically need to invert the logic.

                if ( birthMonth >= minimumMonth.Value )
                {
                    return true;
                }

                if ( birthMonth <= maximumMonth.Value )
                {
                    return true;
                }

                return false;
            }

            if ( minimumMonth.HasValue && birthMonth < minimumMonth.Value )
            {
                return false;
            }

            if ( maximumMonth.HasValue && birthMonth > maximumMonth.Value )
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
