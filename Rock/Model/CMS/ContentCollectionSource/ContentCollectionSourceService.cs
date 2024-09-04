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
using System.Linq;

using Rock.Cms.ContentCollection;

namespace Rock.Model
{
    public partial class ContentCollectionSourceService
    {
        #region Methods

        /// <summary>
        /// Gets the trending ranks lookup table for the entities.
        /// </summary>
        /// <param name="viewCounts">The entities that need to be ranked along with their view counts.</param>
        /// <param name="cutOffDate">The cutoff date to use when calculating the trending value, this would be something like 60-days in the past.</param>
        /// <param name="gravity">The gravity to apply to the calculation with 1.0 being the default. Larger numbers make new content more trendy.</param>
        /// <param name="maxItems">The maximum number of items that can be considered as trending.</param>
        /// <returns>A dictionary whose key represents the entity identifier and the corresponding value is the rank.</returns>
        internal static Dictionary<int, int> CalculateTrendingRanksLookup( IEnumerable<EntityViewCount> viewCounts, DateTime cutOffDate, decimal gravity, int maxItems )
        {
            // Apply an algorithm to weight the results.
            var trendingScores = viewCounts
                .Select( a => new
                {
                    a.Id,
                    Score = CalculateTrendingScore( a, cutOffDate, gravity )
                } )
                .OrderByDescending( t => t.Score )
                .Take( maxItems )
                .ToList();

            var lookup = new Dictionary<int, int>();

            for ( int rank = 0; rank < trendingScores.Count; rank++ )
            {
                lookup.TryAdd( trendingScores[rank].Id, rank + 1 );
            }

            return lookup;
        }

        /// <summary>
        /// Calculates the trending score of a single entity.
        /// </summary>
        /// <param name="item">The entity view count information that describes how many views in a recent period of time.</param>
        /// <param name="cutOffDate">The cut off date that describes the period of time.</param>
        /// <param name="gravity">The gravity to apply to the ranking with 1.0 being the default. Larger numbers make new content more trendy.</param>
        /// <returns>The trending rank. The value really has no meaning, but larger values mean the item is more trendy.</returns>
        private static double CalculateTrendingScore( EntityViewCount item, DateTime cutOffDate, decimal gravity )
        {
            var dateTime = item.DateTime > cutOffDate ? item.DateTime : cutOffDate;
            var daysOld = RockDateTime.Now.Subtract( dateTime ).Days;

            var calculatedGravity = Math.Pow( daysOld + 2, ( double ) ( gravity - 1 ) );

            return item.Views / calculatedGravity;
        }

        #endregion
    }
}
